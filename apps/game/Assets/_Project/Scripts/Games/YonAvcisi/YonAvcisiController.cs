using System;
using System.Collections.Generic;
using System.Threading;
using Roboya.CodingEngine.Execution;
using Roboya.CodingEngine.Play;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Games.YonAvcisi
{
    /// <summary>
    /// Flow of one Yön Avcısı level: plan → play (animate events) → result → retry or next.
    /// Owns no game rules; those live in <see cref="LevelSession"/> (pure C#, tested in CI).
    /// </summary>
    public sealed class YonAvcisiController : IDisposable
    {
        private const string BumpVoice = "roboya.bump.01";
        private const string NotThereVoice = "roboya.not_there.01";
        private const string MissingItemsVoice = "roboya.missing_items.01";
        private const string PlayPromptVoice = "roboya.play_prompt";

        private readonly GameServices _services;
        private readonly IReadOnlyList<LevelEntry> _levels;
        private readonly BoardView _board;
        private readonly CardTray _tray;
        private readonly IconButton _play;
        private readonly IconButton _hint;
        private readonly VisualElement _result;
        private readonly VisualElement _stars;
        private readonly VisualElement _progress;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        private int _index;
        private LevelEntry _entry;
        private LevelSession _session;

        public YonAvcisiController(VisualElement root, GameServices services, IReadOnlyList<LevelEntry> levels)
        {
            _services = services;
            _levels = levels;

            _board = new BoardView();
            root.Q("board-host").Add(_board);
            _tray = new CardTray(root.Q("palette"), root.Q("plan"), root.Q("drag-layer"));
            _tray.Changed += OnPlanChanged;

            _play = new IconButton(IconKind.Play, () => _ = PlayAsync()) { name = "play" };
            _play.AddToClassList("icon-button--play");
            root.Q("play-host").Add(_play);

            var listen = new IconButton(IconKind.Listen, Listen) { name = "listen" };
            root.Q("top-left").Add(listen);
            _hint = new IconButton(IconKind.Hint, Hint) { name = "hint" };
            _hint.AddToClassList("icon-button--hint");
            root.Q("top-right").Add(_hint);
            var clear = new IconButton(IconKind.Clear, ClearPlan) { name = "clear" };
            clear.AddToClassList("icon-button--small");
            root.Q("top-right").Add(clear);

            _progress = root.Q("progress");
            _result = root.Q("result");
            _stars = root.Q("stars");
            var retry = new IconButton(IconKind.Retry, Retry) { name = "retry" };
            var next = new IconButton(IconKind.Next, Next) { name = "next" };
            next.AddToClassList("icon-button--play");
            root.Q("result-buttons").Add(retry);
            root.Q("result-buttons").Add(next);
        }

        public void Start(int index)
        {
            _index = Mathf.Clamp(index, 0, _levels.Count - 1);
            _entry = _levels[_index];
            _session = new LevelSession(_entry.Level, _entry.ShortestLength, _services.Rules);
            if (_entry.Dto.StarterProgram != null)
            {
                _session.Plan.Load(CardsOf(_entry));
            }

            bool ghost = _entry.Dto.Options?.GhostPath ?? false;
            _board.Show(_entry.Level, ghost);
            _tray.Bind(_session.Plan, _entry.Level.AvailableCards);
            _tray.SetLocked(false);
            _result.AddToClassList("hidden");
            RenderProgress();
            UpdateButtons();
            _ = IntroAsync(_entry);
        }

        /// <summary>Loads this level's lines first so the intro and feedback play without a gap.</summary>
        private async Awaitable IntroAsync(LevelEntry entry)
        {
            try
            {
                await _services.Voice.PreloadAsync(new[] { entry.Dto.Voice.Intro });
                if (_entry == entry)
                {
                    _services.Voice.Play(entry.Dto.Voice.Intro);
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

                await _services.Voice.PreloadAsync(rest);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
            _services.Voice.Stop();
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
            _board.ResetRobot(_entry.Level.Start);
            var events = _session.Play();
            UpdateButtons(); // after Play(): the session is now Running, so play is disabled.

            ExecutionResult result = null;
            try
            {
                foreach (var e in events)
                {
                    switch (e.Kind)
                    {
                        case ExecutionEventKind.CommandStarted:
                            _tray.HighlightSlot(e.CommandPath[0]);
                            break;
                        case ExecutionEventKind.Moved:
                            await _board.AnimateMove(e.Before.Position, e.After.Position, token);
                            break;
                        case ExecutionEventKind.Turned:
                            await _board.AnimateTurn(e.Before.Facing, e.After.Facing, token);
                            break;
                        case ExecutionEventKind.Bumped:
                            _services.Voice.Play(BumpVoice);
                            var bump = _board.AnimateBump(e.Before.Facing, token);
                            var shake = _tray.ShakeSlot(e.CommandPath[0], token);
                            await bump;
                            await shake;
                            break;
                        case ExecutionEventKind.Collected:
                            await _board.AnimateCollect(e.ItemIndex, token);
                            break;
                        case ExecutionEventKind.Finished:
                            result = e.Result;
                            break;
                    }
                }
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
            if (result != null && result.Outcome != ExecutionOutcome.Bumped)
            {
                _services.Voice.Play(result.Outcome == ExecutionOutcome.MissingItems ? MissingItemsVoice : NotThereVoice);
            }

            await Tween.Delay(0.8f, token);
            _board.ResetRobot(_entry.Level.Start);
            _tray.SetLocked(false);
            UpdateButtons();
        }

        private async Awaitable ShowSuccess(CancellationToken token)
        {
            _services.Voice.Play(_entry.Dto.Voice.Success);
            await _board.Celebrate(token);
            _stars.Clear();
            for (int i = 0; i < 3; i++)
            {
                var star = new Icon(i < _session.Stars ? IconKind.Star : IconKind.StarEmpty) { Color = new Color(1f, 0.78f, 0.15f) };
                star.AddToClassList("result__star");
                _stars.Add(star);
            }

            _result.Q("next").SetEnabled(_index < _levels.Count - 1);
            _result.RemoveFromClassList("hidden");
            RenderProgress();
        }

        private void OnPlanChanged()
        {
            _tray.ClearHint();
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
            var keys = _entry.Dto.Voice.Hints;
            int tier = (int)hint.Tier - 1;
            if (keys != null && tier >= 0 && tier < keys.Count)
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

            _session.Plan.Clear();
            _tray.Refresh();
            UpdateButtons();
        }

        private void Retry()
        {
            Start(_index);
        }

        private void Next()
        {
            Start(_index + 1);
        }

        private void UpdateButtons()
        {
            _play.SetEnabled(_session.CanPlay);
            _play.EnableInClassList("icon-button--pulse", _session.CanPlay && _session.Plan.IsFull);
            _hint.EnableInClassList("hidden", !_session.ShouldOfferHint && _session.HintTier == HintTier.None);
            _hint.EnableInClassList("icon-button--pulse", _session.ShouldOfferHint);
        }

        private void RenderProgress()
        {
            _progress.Clear();
            for (int i = 0; i < _levels.Count; i++)
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
