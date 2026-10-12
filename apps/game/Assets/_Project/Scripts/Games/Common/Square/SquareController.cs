using System;
using System.Collections.Generic;
using System.Threading;
using Roboya.CodingEngine.Levels;
using Roboya.CodingEngine.Play;
using Roboya.CodingEngine.Progress;
using Roboya.CodingEngine.Sharing;
using Roboya.Core;
using Roboya.Games.Common;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;
using FriendPose = Roboya.CodingEngine.Levels.Generated.FriendPose;
using RegionId = Roboya.CodingEngine.Levels.Generated.RegionId;

namespace Roboya.Games.Square
{
    /// <summary>
    /// Flow of one village level (ADR 0031): story intro → plan (tap hives, flowers, the table) → Ari runs the plan → story outro and result.
    /// Owns no game rules: those are <see cref="SharingSession"/> and the engine (pure C#, tested in CI). The hand and the speech guide the
    /// child, because no text is shown and no adult should have to explain the screen.
    /// </summary>
    public sealed class SquareController : IDisposable
    {
        private const string BasketFullVoice = "koy_meydani.basket_full";
        private const string FlowerEmptyVoice = "koy_meydani.flower_empty";
        private const string BasketEmptyVoice = "koy_meydani.basket_empty";
        private const string WaitingVoice = "koy_meydani.waiting";
        private const string NotEqualVoice = "koy_meydani.not_equal";
        private const string LeftoverVoice = "koy_meydani.leftover";
        private const string PlayPromptVoice = "roboya.play_prompt";
        private const string AlternativeVoice = "roboya.alternative_offer";
        private const string PartUnlockedVoice = "reward.part_unlocked";
        private const int CountVoices = 10;
        private const long IdleHintMs = 8000;
        private const long AfterRunHintMs = 1800;

        private readonly GameServices _services;
        private readonly IReadOnlyList<LevelEntry> _levels;
        private readonly RegionArt _art;
        private readonly PartArt _partArt;
        private readonly List<string> _path;
        private readonly VisualElement _screen;
        private readonly SquareView _square;
        private readonly SquarePlanBar _bar;
        private readonly GuideHand _hand = new GuideHand();
        private readonly IconButton _play;
        private readonly IconButton _hint;
        private readonly IconButton _easier;
        private readonly IconButton _clear;
        private readonly VisualElement _result;
        private readonly VisualElement _stars;
        private readonly StoryStage _story;
        private readonly ValueCard _valueCard;
        private readonly IVisualElementScheduledItem _idle;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        private int _index;
        private LevelEntry _entry;
        private SharingLevel _level;
        private SharingSession _session;
        private LevelEntry _alternative;
        private bool _alternativeOffered;
        private bool _guided;
        private bool _running;
        private bool _open;
        private bool _introActive;
        private float _startedAt;

        public SquareController(VisualElement root, GameServices services, IReadOnlyList<LevelEntry> levels, RegionArt art = null, PartArt partArt = null)
        {
            _services = services;
            _levels = levels;
            _art = art;
            _partArt = partArt;
            _path = levels.Count > 0 ? ProgressQueries.PathOf(services.Catalog, levels[0].Dto.Region) : new List<string>();
            _screen = root.Q("screen");

            if (art != null && art.Background != null)
            {
                _screen.style.backgroundImage = new StyleBackground(art.Background);
                _screen.AddToClassList("screen--art");
            }

            // The village has no card palette and no dragging: the board's own things are tapped.
            root.Q("palette").style.display = DisplayStyle.None;
            root.Q("drag-layer").style.display = DisplayStyle.None;

            _square = new SquareView(art);
            root.Q("board-host").Add(_square);
            // The thing that was tapped answers at once (a small pop), then its card lands on the strip.
            _square.HiveTapped += hive =>
            {
                Answer(_square.Hives[hive]);
                AddCard(SharingCard.Give(hive));
            };
            _square.FlowerTapped += flower =>
            {
                Answer(_square.Flowers[flower]);
                AddCard(SharingCard.Take(flower));
            };
            _square.TableTapped += () =>
            {
                Answer(_square.Table);
                AddCard(SharingCard.Give(SharingLevel.Table));
            };

            _bar = new SquarePlanBar(root.Q("plan"), art);
            _bar.CardTapped += OnCardTapped;

            _play = new IconButton(IconKind.Play, () => _ = PlayAsync()) { name = "play" };
            _play.AddToClassList("icon-button--play");
            root.Q("play-host").Add(_play);

            var home = new IconButton(IconKind.Home, () => _services.Navigator.GoToMap()) { name = "home" };
            home.AddToClassList("icon-button--small");
            root.Q("top-left").Add(home);
            _easier = new IconButton(IconKind.Easier, GoToAlternative) { name = "easier" };
            _easier.AddToClassList("icon-button--hint");
            _easier.AddToClassList("hidden");
            // Help sits on the right, apart from Home: the button a child presses most is not next to the one that leaves.
            root.Q("top-right").Add(_easier);
            var listen = new IconButton(IconKind.Listen, Listen) { name = "listen" };
            root.Q("top-right").Add(listen);
            _hint = new IconButton(IconKind.Hint, HintClicked) { name = "hint" };
            _hint.AddToClassList("icon-button--hint");
            root.Q("top-right").Add(_hint);
            _clear = new IconButton(IconKind.Clear, ClearPlan) { name = "clear" };
            _clear.AddToClassList("icon-button--small");
            root.Q("top-right").Add(_clear);

            _screen.Add(_hand);

            _result = root.Q("result");
            _result.pickingMode = PickingMode.Ignore;
            foreach (var container in _result.Query<VisualElement>().ToList())
            {
                container.pickingMode = PickingMode.Ignore;
            }

            _story = new StoryStage(art, services.Voice);
            _result.parent.Insert(_result.parent.IndexOf(_result), _story);
            _valueCard = new ValueCard(services.Voice);
            _result.parent.Insert(_result.parent.IndexOf(_result), _valueCard);
            _stars = root.Q("stars");
            var retry = new IconButton(IconKind.Retry, Retry) { name = "retry" };
            var next = new IconButton(IconKind.Next, Next) { name = "next" };
            next.AddToClassList("icon-button--play");
            root.Q("result-buttons").Add(retry);
            root.Q("result-buttons").Add(next);

            _idle = _screen.schedule.Execute(ShowHintHand);
            _idle.Pause();
        }

        /// <summary>Id of the level on the screen; lets tests tell which level is loaded.</summary>
        public string CurrentLevelId => _entry?.Id;

        /// <param name="withStory">False on retry: the child has just heard the story, go straight to the board.</param>
        public void Start(int index, bool withStory = true)
        {
            ReportAbandon();
            _index = Mathf.Clamp(index, 0, _levels.Count - 1);
            _entry = _levels[_index];
            if (!_entry.IsSharing)
            {
                Debug.LogError("Village square cannot play level " + _entry.Id + ": it has no sharing data.");
                return;
            }

            _level = _entry.Sharing;
            _session = new SharingSession(_level, _entry.ShortestLength, _services.Rules);
            _running = false;
            _alternativeOffered = false;
            _alternative = null;
            _easier.AddToClassList("hidden");

            // YZ-02: after three three-star levels in a concept the next level starts with less help.
            _guided = SupportPlanner.For(_services.Catalog, _entry, _services.Progress.Book).Guided;

            _square.Build(_level);
            _square.SetInputEnabled(true);
            _bar.Bind(_level);
            _hand.Hide();
            _result.AddToClassList("hidden");
            _story.Hide();
            _introActive = true;
            RefreshPlan();
            _ = IntroAsync(_entry, withStory);
            _startedAt = Time.realtimeSinceStartup;
            _open = true;
            _services.Analytics.Track("level_start", _entry.Id, LevelFlow.Props(_services));
        }

        public void Dispose()
        {
            ReportAbandon();
            _lifetime.Cancel();
            _lifetime.Dispose();
            _services.Voice.Stop();
        }

        private int Seconds => Mathf.Clamp(Mathf.RoundToInt(Time.realtimeSinceStartup - _startedAt), 0, 86_400);

        private void ReportAbandon()
        {
            if (!_open)
            {
                return;
            }

            _open = false;
            LevelFlow.ReportAbandon(_services, _entry, _session.Attempts, Seconds, _session.HintsUsed);
        }

        /// <summary>Loads the level's lines first so the story and the feedback play without a gap, then asks for what is wanted without words.</summary>
        private async Awaitable IntroAsync(LevelEntry entry, bool withStory)
        {
            var token = _lifetime.Token;
            try
            {
                await _services.Voice.PreloadAsync(new[] { entry.Dto.Voice.Intro });
                if (_entry == entry && withStory)
                {
                    await _story.PlayIntroAsync(LevelStory.Intro(entry.Dto), token);
                }

                var rest = new List<string> { BasketFullVoice, FlowerEmptyVoice, BasketEmptyVoice, WaitingVoice, NotEqualVoice, LeftoverVoice, PlayPromptVoice };
                if (entry.Dto.Voice.Success != null)
                {
                    rest.Add(entry.Dto.Voice.Success);
                }

                if (entry.Dto.Voice.Hints != null)
                {
                    rest.AddRange(entry.Dto.Voice.Hints);
                }

                for (int n = 1; n <= Mathf.Min(entry.Sharing.TotalHoney, CountVoices); n++)
                {
                    rest.Add("count." + n.ToString("D2"));
                }

                await _services.Voice.PreloadAsync(rest);
                if (_entry == entry)
                {
                    await _square.ShowGoal(token);
                }
            }
            catch (OperationCanceledException)
            {
                // Screen closed during the story.
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                // The hand waits for the story to end: it would otherwise point over the story scene.
                if (_entry == entry)
                {
                    _introActive = false;
                    RestartGuidance();
                }
            }
        }

        // ---- The plan ----

        private bool Interactive => _session != null && _session.State == SessionState.Planning && !_running;

        private void Answer(VisualElement tapped)
        {
            if (Interactive)
            {
                _ = _square.Pop(tapped, _lifetime.Token);
            }
        }

        private void AddCard(SharingCard card)
        {
            if (!Interactive)
            {
                return;
            }

            ResetScene();
            if (!_session.Plan.Add(card))
            {
                _ = _square.Shake(_bar.Host, _lifetime.Token);
                return;
            }

            _services.Sfx.Play(SfxKind.Place);
            RefreshPlan();
            if (_session.Plan.IsFull)
            {
                _services.Voice.Play(PlayPromptVoice);
            }
        }

        private void OnCardTapped(bool inBox, int index)
        {
            if (!Interactive)
            {
                return;
            }

            ResetScene();
            if (inBox ? _session.Plan.RemoveBoxAt(index) : _session.Plan.RemoveTailAt(index))
            {
                RefreshPlan();
            }
        }

        private void ClearPlan()
        {
            if (!Interactive)
            {
                return;
            }

            ResetScene();
            _session.Plan.Clear();
            RefreshPlan();
        }

        /// <summary>Back to how the level starts: the honey where it was, Ari at home, nobody waiting. The child edits the plan from a clean scene.</summary>
        private void ResetScene()
        {
            _square.ShowState(_level.Start());
            _square.ResetPoses();
            _square.SnapAriHome();
            _bar.ClearMarks();
        }

        private void RefreshPlan()
        {
            _bar.Render(_session.Plan);
            UpdateButtons();
            RestartGuidance();
        }

        /// <summary>Hides the hand and decides when it comes back: at once on a corner's first level, after a quiet moment elsewhere.</summary>
        private void RestartGuidance()
        {
            _hand.Hide();
            _idle.Pause();
            if (!Interactive || _introActive)
            {
                return;
            }

            if (_guided)
            {
                // The first level of a corner: the hand shows every step.
                ShowHintHand();
            }
            else
            {
                _idle.ExecuteLater(IdleHintMs);
            }
        }

        private void UpdateButtons()
        {
            _play.SetEnabled(_session.CanPlay && !_running);
            bool ready = Interactive && _session.CanPlay && SharingHint.Next(_level, _session.Plan.ToProgram()).Kind == SharingHintKind.PressPlay;
            _play.EnableInClassList("icon-button--pulse", ready);
            _hint.EnableInClassList("hidden", !_session.ShouldOfferHint && _session.HintsUsed == 0);
            _hint.EnableInClassList("icon-button--pulse", _session.ShouldOfferHint);
            // Nothing to clear on an empty strip: the button is not shown, so a child is not drawn to it.
            _clear.EnableInClassList("hidden", _session.Plan.IsEmpty || _running);
        }

        // ---- Guidance ----

        private void Listen()
        {
            _services.Voice.Play(_entry.Dto.Voice.Intro);
            if (Interactive)
            {
                _ = _square.ShowGoal(_lifetime.Token);
            }
        }

        private void HintClicked()
        {
            if (!Interactive)
            {
                return;
            }

            var hint = _session.RequestHint();
            _services.Analytics.Track("hint_used", _entry.Id, LevelFlow.Props(_services, ("hint_tier", Mathf.Clamp(_session.HintsUsed, 1, 3))));
            var keys = _entry.Dto.Voice.Hints;
            if (keys != null && keys.Count > 0)
            {
                _services.Voice.Play(keys[Mathf.Min(_session.HintsUsed - 1, keys.Count - 1)]);
            }

            PointAtHint(hint);
            UpdateButtons();
        }

        /// <summary>The hand shows the next step by itself: always on a corner's first level, after a quiet moment elsewhere, and after a run that did not work.</summary>
        private void ShowHintHand()
        {
            if (!Interactive)
            {
                return;
            }

            PointAtHint(SharingHint.Next(_level, _session.Plan.ToProgram()));
        }

        private void PointAtHint(SharingHintResult hint)
        {
            var target = TargetOf(hint);
            if (target == null)
            {
                _hand.Hide();
                return;
            }

            _hand.PointAt(_screen.WorldToLocal(target.worldBound.center));
        }

        private VisualElement TargetOf(SharingHintResult hint)
        {
            switch (hint.Kind)
            {
                case SharingHintKind.PressPlay:
                    return _play;
                case SharingHintKind.Add:
                    if (hint.Card.IsTable)
                    {
                        return _square.Table;
                    }

                    return hint.Card.Kind == SharingCardKind.Take ? (VisualElement)_square.Flowers[hint.Card.Flower] : _square.Hives[hint.Card.Target];
                case SharingHintKind.Remove:
                case SharingHintKind.SetCount:
                    return _bar.ElementOf(hint.Card);
                default:
                    return null;
            }
        }

        // ---- Running ----

        private async Awaitable PlayAsync()
        {
            if (!_session.CanPlay || _running)
            {
                return;
            }

            _running = true;
            _idle.Pause();
            _hand.Hide();
            _square.SetInputEnabled(false);
            _bar.SetLocked(true);
            _bar.ClearMarks();
            var token = _lifetime.Token;
            _square.ShowState(_level.Start());
            _square.ResetPoses();
            _square.SnapAriHome();
            var result = _session.Play();
            UpdateButtons();

            try
            {
                await AnimateAsync(result, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                // Fire-and-forget from a button; never lose the error or leave the strip locked.
                Debug.LogException(ex);
                Unlock();
                return;
            }

            if (result.IsSuccess)
            {
                await ShowSuccess(token);
                return;
            }

            await ExplainAsync(result, token);
        }

        private void Unlock()
        {
            _running = false;
            _square.SetInputEnabled(true);
            _bar.SetLocked(false);
            UpdateButtons();
        }

        /// <summary>Plays the run event by event: Ari flies, honey travels, the cells fill and the numbers are said.</summary>
        private async Awaitable AnimateAsync(SharingResult result, CancellationToken token)
        {
            var shown = _level.Start();
            int at = int.MinValue;
            foreach (var e in result.Events)
            {
                _bar.Glow(e.Card);
                switch (e.Kind)
                {
                    case SharingEventKind.Took:
                    {
                        int spot = -2 - e.Flower;
                        if (at != spot)
                        {
                            await _square.MoveAri(_square.SpotBesideFlower(e.Flower), token);
                            at = spot;
                        }

                        for (int k = 1; k <= e.Amount; k++)
                        {
                            await _square.FlyDrop(_square.FlowerPoint(e.Flower), _square.BasketPoint(), token);
                            _square.Flowers[e.Flower].Set(shown.FlowerAt(e.Flower) - k);
                            _square.Ari.Basket.Set(shown.Basket + k);
                            _services.Sfx.Play(SfxKind.Collect);
                            _ = _square.Pop(_square.Ari.Basket, token);
                        }

                        shown = shown.Took(e.Flower, e.Amount);
                        break;
                    }

                    case SharingEventKind.Gave:
                    {
                        if (at != e.Target)
                        {
                            await _square.MoveAri(e.Target == SharingLevel.Table ? _square.SpotBelowTable() : _square.SpotBelowHive(e.Target), token);
                            at = e.Target;
                        }

                        Vector2 landing = e.Target == SharingLevel.Table
                            ? _square.TablePoint()
                            : _square.Hives[e.Target].LandingPoint(Mathf.Clamp(e.TargetCountAfter - 1, 0, Mathf.Max(0, _square.Hives[e.Target].Comb.Total - 1)));
                        await _square.FlyDrop(_square.BasketPoint(), landing, token);
                        shown = shown.Gave(e.Target, 1);
                        _square.Ari.Basket.Set(shown.Basket);
                        _services.Sfx.Play(SfxKind.Place);
                        if (e.Target == SharingLevel.Table)
                        {
                            _square.Table.Set(shown.Table);
                        }
                        else
                        {
                            _square.Hives[e.Target].Show(shown.HiveAt(e.Target));
                            _ = _square.Pop(_square.Hives[e.Target].Comb, token);
                        }

                        // Counting aloud: how much that hive or the table holds now ("bir, iki, üç").
                        if (e.TargetCountAfter >= 1 && e.TargetCountAfter <= CountVoices)
                        {
                            _services.Voice.Play("count." + e.TargetCountAfter.ToString("D2"));
                        }

                        await Tween.Delay(0.35f, token);
                        break;
                    }

                    case SharingEventKind.Stopped:
                        _bar.MarkStopped(e.Card);
                        await StopFeedbackAsync(e, token);
                        break;
                }
            }

            _bar.Glow(null);
            await _square.GoHome(token);
        }

        private async Awaitable StopFeedbackAsync(SharingEvent e, CancellationToken token)
        {
            switch (e.Reason)
            {
                case SharingStopReason.FlowerEmpty:
                    _services.Voice.Play(FlowerEmptyVoice);
                    await _square.Shake(_square.Flowers[e.Flower].Image, token);
                    break;
                case SharingStopReason.BasketFull:
                    _services.Voice.Play(BasketFullVoice);
                    await _square.ShakeBasket(token);
                    break;
                default:
                    _services.Voice.Play(BasketEmptyVoice);
                    await _square.ShakeBasket(token);
                    break;
            }
        }

        /// <summary>A run that did not win: nobody is told off. Whoever still waits for honey waits, Roboya says what is missing, the hand shows the way.</summary>
        private async Awaitable ExplainAsync(SharingResult result, CancellationToken token)
        {
            try
            {
                var waiting = SharingRules.WaitingHives(_level, result.Final);
                for (int i = 0; i < waiting.Length; i++)
                {
                    _square.Hives[i].SetWaiting(waiting[i]);
                }

                if (!result.Stopped)
                {
                    _services.Voice.Play(
                        SharingRules.IsEqualButLeftOver(_level, result.Final) ? LeftoverVoice : _level.Goal == SharingGoalKind.Share ? NotEqualVoice : WaitingVoice);
                }

                Unlock();
                await OfferAlternativeIfNeeded(token);
                _idle.ExecuteLater(AfterRunHintMs);
            }
            catch (OperationCanceledException)
            {
                // Screen closed.
            }
        }

        private async Awaitable ShowSuccess(CancellationToken token)
        {
            // ILR-01/02: keep the best result on the device; parts are derived from it (ILR-03).
            var newParts = LevelFlow.RecordWin(_services, _entry, _session.Stars, _session.Attempts, Seconds, _session.HintsUsed, _session.Plan.Count);
            _open = false;
            Sprite reward = null;
            if (newParts.Count > 0 && _partArt != null)
            {
                // ILR-03: the earned ship part is shown in the outro; it is fitted to the ship on the map.
                var layers = _services.Parts.LayersOf(newParts[0].Id);
                reward = layers.Count > 0 ? _partArt.Find(layers[0].Sprite) : null;
            }

            _services.Sfx.Play(SfxKind.Success);
            await _square.Celebrate(token);
            await _story.PlayOutroAsync(
                LevelStory.Outro(_entry.Dto),
                token,
                reward,
                PartUnlockedVoice,
                reward != null ? (Action)(() => _services.Navigator.GoToWorkshop()) : null);
            if (CornerRules.IsCornerEnd(_index, _levels.Count))
            {
                // The sharing value moment closes each corner; Roboya finishes his own success line first.
                await WaitForQuietVoiceAsync(token);
                await _valueCard.ShowAsync(ValueMoments.Of(_entry.Dto.Meta.Value, _services.Voice.Has), FriendCardSprite(), token);
            }

            _stars.Clear();
            for (int i = 0; i < 3; i++)
            {
                var star = new Icon(i < _session.Stars ? IconKind.Star : IconKind.StarEmpty) { Color = new Color(1f, 0.78f, 0.15f) };
                star.AddToClassList("result__star");
                _stars.Add(star);
            }

            _result.RemoveFromClassList("hidden");
        }

        /// <summary>The region's friend for the calm value card: the old village bee is a picture, not code.</summary>
        private Sprite FriendCardSprite() =>
            _art != null && _entry.Dto.Region != RegionId.SabirOrmani ? _art.FriendPoseSprite(FriendPose.Thanks) : null;

        /// <summary>Waits until no narration has played for a short moment, at most 25 s.</summary>
        private async Awaitable WaitForQuietVoiceAsync(CancellationToken token)
        {
            float quiet = 0f;
            float waited = 0f;
            while (quiet < 0.8f && waited < 25f)
            {
                await Awaitable.NextFrameAsync(token);
                float dt = Time.unscaledDeltaTime;
                waited += dt;
                quiet = _services.Voice.IsPlaying ? 0f : quiet + dt;
            }
        }

        // ---- After the result ----

        private void Retry()
        {
            if (_services.ScreenTime.IsExhausted)
            {
                _services.Navigator.GoToMap();
                return;
            }

            Start(_index, withStory: false);
        }

        private void Next() => LevelFlow.GoNext(_services, _path, _levels, _index);

        /// <summary>YZ-03: after five failed runs Roboya offers an easier level; the child can always say no.</summary>
        private async Awaitable OfferAlternativeIfNeeded(CancellationToken token)
        {
            if (_alternativeOffered || !_session.ShouldOfferAlternative)
            {
                return;
            }

            var states = ProgressQueries.PathStates(_services, _path);
            _alternative = SupportPlanner.AlternativeFor(_services.Catalog, _entry, id =>
            {
                int i = _path.IndexOf(id);
                return i >= 0 && PathRules.CanPlay(states[i]);
            });
            if (_alternative == null)
            {
                return;
            }

            _alternativeOffered = true;
            _easier.RemoveFromClassList("hidden");
            _easier.AddToClassList("icon-button--pulse");
            // Let the "not there yet" line finish first; a new line would cut it off.
            float waited = 0f;
            while (_services.Voice.IsPlaying && waited < 6f)
            {
                await Awaitable.NextFrameAsync(token);
                waited += Time.deltaTime;
            }

            _services.Voice.Play(AlternativeVoice);
        }

        private void GoToAlternative()
        {
            if (_alternative != null)
            {
                _services.Navigator.PlayLevel(_alternative.Id);
            }
        }
    }
}
