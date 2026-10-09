using System;
using System.Collections.Generic;
using System.Threading;
using Roboya.CodingEngine.Execution;
using Roboya.CodingEngine.Levels;
using GameId = Roboya.CodingEngine.Levels.Generated.GameId;
using Roboya.CodingEngine.Play;
using Roboya.CodingEngine.Progress;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Games.Common
{
    /// <summary>
    /// Flow of one Yön Avcısı level: story intro → plan → play (animate events) → story outro + result → retry or next.
    /// Owns no game rules; those live in <see cref="LevelSession"/> (pure C#, tested in CI).
    /// </summary>
    public sealed class GameController : IDisposable
    {
        private const string BumpVoice = "roboya.bump.01";
        private const string NotThereVoice = "roboya.not_there.01";
        private const string MissingItemsVoice = "roboya.missing_items.01";
        private const string PlayPromptVoice = "roboya.play_prompt";
        private const string AskGrownUpVoice = "roboya.ask_grownup";
        private const string AlternativeVoice = "roboya.alternative_offer";
        private const string PartUnlockedVoice = "reward.part_unlocked";
        private const string ForgotClearVoice = "bal_pesinde.forgot_clear";
        private const string HuntVoice = "kodlama_kutusu.hunt";
        private const int CountVoices = 10;

        private readonly GameServices _services;
        private readonly IReadOnlyList<LevelEntry> _levels;
        private readonly BoardView _board;
        private readonly CardTray _tray;
        private readonly IconButton _play;
        private readonly IconButton _hint;
        private readonly VisualElement _result;
        private readonly VisualElement _stars;
        private readonly VisualElement _progress;
        private readonly StoryStage _story;
        private readonly PartArt _partArt;
        private readonly List<string> _path;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        private int _index;
        private LevelEntry _entry;
        private LevelSession _session;
        private readonly IconButton _easier;
        private bool _guided;
        private bool _alternativeOffered;
        private LevelEntry _alternative;
        private bool _guideDone;

        public GameController(VisualElement root, GameServices services, IReadOnlyList<LevelEntry> levels, RegionArt art = null, PartArt partArt = null)
        {
            _services = services;
            _levels = levels;
            _partArt = partArt;
            _path = levels.Count > 0 ? ProgressQueries.PathOf(services.Catalog, levels[0].Dto.Region) : new List<string>();

            _board = new BoardView(art);
            root.Q("board-host").Add(_board);
            _badge = new GoalBadge(art);
            root.Q("board-host").Add(_badge);
            if (art != null && art.Background != null)
            {
                var screen = root.Q("screen");
                screen.style.backgroundImage = new StyleBackground(art.Background);
                screen.AddToClassList("screen--art");
            }
            _planElement = root.Q("plan");
            _tray = new CardTray(root.Q("palette"), _planElement, root.Q("drag-layer"));
            _tray.Changed += OnPlanChanged;

            _play = new IconButton(IconKind.Play, () => _ = PlayAsync()) { name = "play" };
            _play.AddToClassList("icon-button--play");
            root.Q("play-host").Add(_play);

            var home = new IconButton(IconKind.Home, () => _services.Navigator.GoToMap()) { name = "home" };
            home.AddToClassList("icon-button--small");
            root.Q("top-left").Add(home);
            var listen = new IconButton(IconKind.Listen, Listen) { name = "listen" };
            root.Q("top-left").Add(listen);
            _easier = new IconButton(IconKind.Easier, GoToAlternative) { name = "easier" };
            _easier.AddToClassList("icon-button--hint");
            _easier.AddToClassList("hidden");
            root.Q("top-right").Add(_easier);
            _hint = new IconButton(IconKind.Hint, Hint) { name = "hint" };
            _hint.AddToClassList("icon-button--hint");
            root.Q("top-right").Add(_hint);
            var clear = new IconButton(IconKind.Clear, ClearPlan) { name = "clear" };
            clear.AddToClassList("icon-button--small");
            root.Q("top-right").Add(clear);

            _progress = root.Q("progress");
            _result = root.Q("result");

            // The story scene covers the whole screen; the result panel stays above it.
            _story = new StoryStage(art, services.Voice);
            _result.parent.Insert(_result.parent.IndexOf(_result), _story);
            _stars = root.Q("stars");
            var retry = new IconButton(IconKind.Retry, Retry) { name = "retry" };
            var next = new IconButton(IconKind.Next, Next) { name = "next" };
            next.AddToClassList("icon-button--play");
            root.Q("result-buttons").Add(retry);
            root.Q("result-buttons").Add(next);
        }

        /// <param name="withStory">False on retry: the child has just heard the story, go straight to the board.</param>
        public void Start(int index, bool withStory = true)
        {
            // Leaving a level that was not finished (home button, next level pick) counts as abandoned.
            ReportAbandon();
            _index = Mathf.Clamp(index, 0, _levels.Count - 1);
            _entry = _levels[_index];
            // Bal Peşinde: the bee keeps its place and its memory between runs (BAL-02).
            _bee = _entry.Dto.Game == GameId.BalPesinde;
            _clearedSinceRun = true;
            _lastPlanCount = 0;
            _session = new LevelSession(_entry.Level, _entry.ShortestLength, _services.Rules, keepsState: _bee);
            // The memory is invisible for the youngest (BAL, Minik): the strip shows blank cards, so only the count is seen.
            _planElement.EnableInClassList("plan--memory", _bee && _entry.Dto.Options?.BeeMemoryVisible != true);
            if (_entry.Dto.StarterProgram != null)
            {
                _session.Plan.Load(CardsOf(_entry));
            }

            // YZ-02: after three three-star levels in a concept the next level starts with less help.
            var support = SupportPlanner.For(_services.Catalog, _entry, _services.Progress.Book);
            _guided = support.Guided;
            bool ghost = support.GhostPath;
            _alternativeOffered = false;
            _alternative = null;
            _easier.AddToClassList("hidden");
            _board.Show(_entry.Level, ghost, _entry.Dto);
            _badge.Show(_entry.Level);
            _tray.BoxCards = _entry.Dto.StarterProgram != null ? new List<Roboya.CodingEngine.Commands.CardType>(CardsOf(_entry)) : null;
            _tray.Bind(_session.Plan, _entry.Level.AvailableCards);
            // Hata avcısı: the child may edit only after the ready-made code has shown what goes wrong (DemoAsync unlocks).
            _tray.SetLocked(withStory && _entry.Dto.StarterProgram != null);
            _result.AddToClassList("hidden");
            _story.Hide();
            RenderProgress();
            UpdateGuide();
            UpdateButtons();
            _ = IntroAsync(_entry, withStory);
            _startedAt = Time.realtimeSinceStartup;
            _hintsUsed = 0;
            _open = true;
            _services.Analytics.Track("level_start", _entry.Id, LevelProps());
        }

        private const int ProgressDots = 9;
        private readonly VisualElement _planElement;
        private readonly GoalBadge _badge;
        private bool _bee;
        private int _lastPlanCount;
        private bool _clearedSinceRun = true;
        private bool _keptOldCommands;
        private float _startedAt;
        private int _hintsUsed;
        private bool _open;

        private IReadOnlyDictionary<string, object> LevelProps(params (string key, object value)[] extra)
        {
            var map = new Dictionary<string, object>();
            string band = _services.Analytics.ActiveBand;
            if (band != null)
            {
                map["level_band"] = band;
            }

            foreach (var (key, value) in extra)
            {
                map[key] = value;
            }

            return map;
        }

        private int Seconds => Mathf.Clamp(Mathf.RoundToInt(Time.realtimeSinceStartup - _startedAt), 0, 86_400);

        private void ReportAbandon()
        {
            if (!_open)
            {
                return;
            }

            _open = false;
            _services.Analytics.Track(
                "level_abandon", _entry.Id, LevelProps(("attempts", _session.Attempts), ("duration_s", Seconds), ("hints", _hintsUsed)));
        }

        /// <summary>Loads this level's lines first so the story scene and feedback play without a gap.</summary>
        private async Awaitable IntroAsync(LevelEntry entry, bool withStory)
        {
            try
            {
                await _services.Voice.PreloadAsync(new[] { entry.Dto.Voice.Intro });
                if (_entry == entry && withStory)
                {
                    await _story.PlayIntroAsync(LevelStory.Intro(entry.Dto), _lifetime.Token);
                }

                var rest = new List<string> { BumpVoice, NotThereVoice, MissingItemsVoice, PlayPromptVoice };
                if (entry.Dto.Voice.Success != null)
                {
                    rest.Add(entry.Dto.Voice.Success);
                }

                if (entry.Dto.Voice.Hints != null)
                {
                    rest.AddRange(entry.Dto.Voice.Hints);
                }

                if (entry.Dto.StarterProgram != null)
                {
                    rest.Add(HuntVoice);
                }

                int wanted = entry.Dto.Goal?.Collect != null ? entry.Dto.Goal.Collect.Count : 0;
                for (int n = 1; wanted >= 2 && n <= Mathf.Min(wanted, CountVoices); n++)
                {
                    rest.Add("count." + n.ToString("D2"));
                }

                await _services.Voice.PreloadAsync(rest);
                if (_entry == entry && withStory && entry.Dto.StarterProgram != null)
                {
                    await DemoAsync(entry);
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
                // Whatever happened to the story or the demo, the child must never be left with a locked strip.
                if (_entry == entry && withStory && entry.Dto.StarterProgram != null)
                {
                    _tray.SetLocked(false);
                }
            }
        }

        public void Dispose()
        {
            ReportAbandon();
            _lifetime.Cancel();
            _lifetime.Dispose();
            _services.Voice.Stop();
        }

        /// <summary>Animates one run, event by event, and returns its result (null when it never finished).</summary>
        private async Awaitable<ExecutionResult> AnimateAsync(IEnumerable<ExecutionEvent> events, CancellationToken token)
        {
            ExecutionResult result = null;
            foreach (var e in events)
            {
                switch (e.Kind)
                {
                    case ExecutionEventKind.CommandStarted:
                        _tray.HighlightSlot(e.CommandPath[0]);
                        break;
                    case ExecutionEventKind.Moved:
                        _services.Sfx.Play(SfxKind.Step);
                        await _board.AnimateMove(e.Before.Position, e.After.Position, token);
                        break;
                    case ExecutionEventKind.Turned:
                        _services.Sfx.Play(SfxKind.Turn);
                        await _board.AnimateTurn(e.Before.Facing, e.After.Facing, token);
                        break;
                    case ExecutionEventKind.Bumped:
                        _services.Voice.Play(BumpVoice);
                        _services.Sfx.Play(SfxKind.Bump);
                        var bump = _board.AnimateBump(e.Before.Facing, token);
                        var shake = _tray.ShakeSlot(e.CommandPath[0], token);
                        await bump;
                        await shake;
                        break;
                    case ExecutionEventKind.Collected:
                        _badge.MarkCollected(e.ItemIndex);
                        // Counting aloud (BAL-01): "bir, iki, üç" when more than one thing is wanted.
                        if (_badge.Count >= 2 && _badge.CollectedCount >= 1 && _badge.CollectedCount <= CountVoices)
                        {
                            _services.Voice.Play("count." + _badge.CollectedCount.ToString("D2"));
                        }

                        _services.Sfx.Play(SfxKind.Collect);
                        await _board.AnimateCollect(e.ItemIndex, token);
                        break;
                    case ExecutionEventKind.Finished:
                        result = e.Result;
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Hata avcısı (KUT-01): the ready-made code runs once on its own, leaves its footprints and goes wrong, then Roboya asks the
        /// child to find the card that is wrong. It does not count as an attempt, so stars are not affected.
        /// </summary>
        private async Awaitable DemoAsync(LevelEntry entry)
        {
            var token = _lifetime.Token;
            try
            {
                _tray.SetLocked(true);
                _tray.ClearMarks();
                _board.ResetRobot(entry.Level.Start);
                _badge.Set(0);
                await Tween.Delay(0.5f, token);
                if (_entry != entry || !_session.CanPlay)
                {
                    return;
                }

                await AnimateAsync(_session.Demo(), token);
                _tray.HighlightSlot(-1);
                await Tween.Delay(1.6f, token);
                _board.ResetRobot(entry.Level.Start);
                _badge.Set(0);
                _services.Voice.Play(HuntVoice);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                if (_entry == entry)
                {
                    _tray.SetLocked(false);
                    UpdateGuide();
                    UpdateButtons();
                }
            }
        }

        private async Awaitable PlayAsync()
        {
            if (!_session.CanPlay)
            {
                return;
            }

            var token = _lifetime.Token;
            _tray.SetLocked(true);
            _tray.ClearMarks();
            _tray.Guide(null);
            _keptOldCommands = _bee && _session.Attempts > 0 && !_clearedSinceRun;
            _clearedSinceRun = false;
            if (!_bee)
            {
                _board.ResetRobot(_entry.Level.Start);
                _badge.Set(0);
            }

            var events = _session.Play();
            UpdateButtons(); // after Play(): the session is now Running, so play is disabled.

            ExecutionResult result = null;
            try
            {
                result = await AnimateAsync(events, token);
            }
            catch (OperationCanceledException)
            {
                _session.ResetRun();
                return;
            }
            catch (Exception ex)
            {
                // Fire-and-forget from a button; never lose the error or leave the tray locked.
                Debug.LogException(ex);
                _session.ResetRun();
                _tray.SetLocked(false);
                UpdateButtons();
                return;
            }

            _tray.HighlightSlot(-1);
            if (result != null && result.IsSuccess)
            {
                await ShowSuccess(token);
                return;
            }

            // A bump already had its own line; otherwise explain gently what is missing.
            if (_keptOldCommands)
            {
                // BAL-02: the bee also ran what it still remembered; a gentle, teaching line rather than a scolding.
                _services.Voice.Play(ForgotClearVoice);
            }
            else if (result != null && result.Outcome != ExecutionOutcome.Bumped)
            {
                _services.Voice.Play(result.Outcome == ExecutionOutcome.MissingItems ? MissingItemsVoice : NotThereVoice);
            }

            // Kodlama Kutusu (KUT-02): the footprints stay a little longer so the child can compare the planned and the real path.
            // What is still missing breathes meanwhile, so the child sees where to go next (no penalty, only direction).
            var pulse = _board.PulseMissing(result != null ? result.FinalState.CollectedMask : 0UL, token);
            await Tween.Delay(_entry.Dto.Game == GameId.KodlamaKutusu ? 2.5f : 1.5f, token);
            await pulse;
            if (!_bee)
            {
                _board.ResetRobot(_entry.Level.Start);
                _badge.Set(0);
            }

            _tray.SetLocked(false);
            UpdateGuide();
            UpdateButtons();
            await OfferAlternativeIfNeeded(token);
        }

        private async Awaitable ShowSuccess(CancellationToken token)
        {
            // ILR-01/02: keep the best result on the device; parts are derived from it (ILR-03).
            int partsBefore = ProgressQueries.EarnedParts(_services);
            _services.Progress.Book.Record(_entry.Id, _session.Stars);
            SaveProgress();
            _open = false;
            _services.Analytics.Track(
                "level_complete",
                _entry.Id,
                LevelProps(
                    ("attempts", _session.Attempts),
                    ("duration_s", Seconds),
                    ("stars", _session.Stars),
                    ("hints", _hintsUsed),
                    ("code_length", Mathf.Clamp(_session.Plan.Cards.Count, 0, 64))));
            var newParts = RewardRules.NewlyEarned(_services.Parts.Parts, partsBefore, ProgressQueries.EarnedParts(_services));
            Sprite reward = null;
            if (newParts.Count > 0 && _partArt != null)
            {
                // ILR-03: the earned ship part is shown in the outro; it is fitted to the ship on the map.
                var layers = _services.Parts.LayersOf(newParts[0].Id);
                reward = layers.Count > 0 ? _partArt.Find(layers[0].Sprite) : null;
            }

            _services.Sfx.Play(SfxKind.Success);
            await _board.Celebrate(token);
            await _story.PlayOutroAsync(LevelStory.Outro(_entry.Dto), token, reward, PartUnlockedVoice);
            _stars.Clear();
            for (int i = 0; i < 3; i++)
            {
                var star = new Icon(i < _session.Stars ? IconKind.Star : IconKind.StarEmpty) { Color = new Color(1f, 0.78f, 0.15f) };
                star.AddToClassList("result__star");
                _stars.Add(star);
            }

            _result.RemoveFromClassList("hidden");
            RenderProgress();
        }

        private void OnPlanChanged()
        {
            if (_session.Plan.Count > _lastPlanCount)
            {
                _services.Sfx.Play(SfxKind.Place);
            }

            _lastPlanCount = _session.Plan.Count;
            _tray.ClearHint();
            UpdateGuide();
            UpdateButtons();
            if (_session.Plan.IsFull)
            {
                _services.Voice.Play(PlayPromptVoice);
            }
        }

        private void Listen() => _services.Voice.Play(_entry.Dto.Voice.Intro);

        private void Hint()
        {
            var hint = _session.RequestHint();
            _hintsUsed++;
            _services.Analytics.Track("hint_used", _entry.Id, LevelProps(("hint_tier", Mathf.Clamp((int)hint.Tier, 0, 3))));
            var keys = _entry.Dto.Voice.Hints;
            int tier = (int)hint.Tier - 1;
            if (_bee && _session.Attempts > 0 && !_clearedSinceRun && !_session.Plan.IsEmpty)
            {
                _services.Voice.Play(ForgotClearVoice);
            }
            else if (keys != null && tier >= 0 && tier < keys.Count)
            {
                _services.Voice.Play(keys[tier]);
            }

            if (hint.Tier >= HintTier.HighlightWrongCard)
            {
                _tray.MarkHint(hint.SlotIndex, hint.Tier == HintTier.ShowCorrectCard ? hint.Card : null);
            }

            UpdateButtons();
        }

        private void ClearPlan()
        {
            if (_session.State != SessionState.Planning)
            {
                return;
            }

            _clearedSinceRun = true;
            _session.Plan.Clear();
            _tray.Refresh();
            UpdateGuide();
            UpdateButtons();
        }

        private void Retry()
        {
            if (_services.ScreenTime.IsExhausted)
            {
                _services.Navigator.GoToMap();
                return;
            }

            Start(_index, withStory: false);
        }

        /// <summary>Next playable level on the path; otherwise back to the map (GLR-01: ask a grown-up past the free tier).</summary>
        private void Next()
        {
            // Today's play time is used up: Roboya rests; the map shows the rest screen (VEL-02).
            if (!_services.ScreenTime.IsExhausted)
            {
                var states = ProgressQueries.PathStates(_services, _path);
                int start = ProgressQueries.StartIndex(_services, _path);
                for (int next = _index + 1; next < _levels.Count; next++)
                {
                    int pathIndex = _path.IndexOf(_levels[next].Id);
                    var state = pathIndex >= 0 ? states[pathIndex] : NodeState.Locked;
                    if (PathRules.CanPlay(state))
                    {
                        Start(next);
                        return;
                    }

                    // Levels before the child's age start are skipped quietly (they are paid and not theirs);
                    // anything from the start on that is paid asks a grown-up (GLR-01).
                    if (state == NodeState.NeedsGrownUp && pathIndex >= start)
                    {
                        _services.Navigator.GoToMap(AskGrownUpVoice);
                        return;
                    }

                    if (state != NodeState.NeedsGrownUp)
                    {
                        break;
                    }
                }
            }

            _services.Navigator.GoToMap();
        }

        /// <summary>Guided levels: Roboya points at the next card, then at the play button; the child places every card.</summary>
        private void UpdateGuide()
        {
            _guideDone = false;
            if (!_guided || _session.State != SessionState.Planning)
            {
                _tray.Guide(null);
                return;
            }

            var step = GuidedPlan.Next(_entry.Level, _session.Plan.Cards);
            _guideDone = step.IsComplete;
            _tray.Guide(step.NextCard);
        }

        /// <summary>YZ-03: after five failed runs Roboya offers an easier level; the child can always say no.</summary>
        private async Awaitable OfferAlternativeIfNeeded(CancellationToken token)
        {
            if (_alternativeOffered || !_session.ShouldOfferAlternative)
            {
                return;
            }

            var path = ProgressQueries.PathOf(_services.Catalog, _entry.Dto.Region);
            var states = ProgressQueries.PathStates(_services, path);
            _alternative = SupportPlanner.AlternativeFor(_services.Catalog, _entry, id =>
            {
                int i = path.IndexOf(id);
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

        private void SaveProgress()
        {
            try
            {
                _services.Progress.Save();
            }
            catch (System.IO.IOException e)
            {
                // A full disk must not interrupt the child's success moment; the book stays in memory.
                Debug.LogException(e);
            }
        }

        private void UpdateButtons()
        {
            _play.SetEnabled(_session.CanPlay);
            _play.EnableInClassList("icon-button--pulse", _session.CanPlay && (_session.Plan.IsFull || _guideDone));
            _hint.EnableInClassList("hidden", !_session.ShouldOfferHint && _session.HintTier == HintTier.None);
            _hint.EnableInClassList("icon-button--pulse", _session.ShouldOfferHint);
        }

        private void RenderProgress()
        {
            _progress.Clear();
            // A region path can hold 36 levels: show a window of dots around the current one.
            int first = Mathf.Clamp(_index - (ProgressDots / 2), 0, Mathf.Max(0, _levels.Count - ProgressDots));
            int last = Mathf.Min(_levels.Count, first + ProgressDots);
            for (int i = first; i < last; i++)
            {
                var dot = new VisualElement();
                dot.AddToClassList("progress__dot");
                dot.EnableInClassList("progress__dot--current", i == _index);
                dot.EnableInClassList("progress__dot--done", i < _index || (i == _index && _session.State == SessionState.Completed));
                _progress.Add(dot);
            }
        }

        private static IEnumerable<Roboya.CodingEngine.Commands.CardType> CardsOf(LevelEntry entry)
        {
            foreach (var c in entry.Dto.StarterProgram)
            {
                yield return Roboya.CodingEngine.Levels.LevelLoader.ToCard(c.Op);
            }
        }
    }
}
