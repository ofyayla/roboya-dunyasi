using System;
using System.Collections.Generic;
using Roboya.CodingEngine.Execution;
using Roboya.CodingEngine.Scoring;
using Roboya.CodingEngine.Solving;
using Roboya.CodingEngine.World;

namespace Roboya.CodingEngine.Play
{
    /// <summary>
    /// One child playing one level: the plan strip, runs, attempt counting, hint escalation, alternative
    /// level offer and stars. Pure logic; the game view drives it and animates the events it yields.
    /// Mistakes are never punished: failures only unlock help (PRD "Hata cezalandırılmaz").
    /// </summary>
    public sealed class LevelSession
    {
        private readonly SessionRules _rules;

        /// <param name="keepsState">
        /// Bal Peşinde (BAL-02): the bee stays where it stopped and its memory (the plan) is kept after a run, so a program
        /// started without clearing runs the old commands again. Collected items stay collected.
        /// </param>
        public LevelSession(Level level, int shortestLength, SessionRules rules = null, bool keepsState = false)
        {
            KeepsState = keepsState;
            Level = level ?? throw new ArgumentNullException(nameof(level));
            Robot = level.Start;
            ShortestLength = shortestLength;
            _rules = rules ?? SessionRules.Default;
            Plan = new PlanStrip(level.MaxProgramLength, level.AvailableCards);
        }

        public Level Level { get; }

        public bool KeepsState { get; }

        /// <summary>Where the robot is now: the level start, or in <see cref="KeepsState"/> mode where the last run ended.</summary>
        public RobotState Robot { get; private set; }

        public int ShortestLength { get; }

        public PlanStrip Plan { get; }

        public SessionState State { get; private set; } = SessionState.Planning;

        public int Attempts { get; private set; }

        public int FailuresInARow { get; private set; }

        public HintTier HintTier { get; private set; }

        public int Stars { get; private set; }

        public ExecutionResult LastResult { get; private set; }

        /// <summary>YON-02: the whole plan runs at once, so there must be something to run.</summary>
        public bool CanPlay => State == SessionState.Planning && !Plan.IsEmpty;

        public bool ShouldOfferHint => State == SessionState.Planning && FailuresInARow >= _rules.HintAfterFailures && HintTier < (KeepsState ? HintTier.HighlightWrongCard : HintTier.ShowCorrectCard);

        public bool ShouldOfferAlternative => State == SessionState.Planning && FailuresInARow >= _rules.AlternativeAfterFailures;

        /// <summary>Runs the current plan. The result is recorded when the caller consumes the Finished event.</summary>
        public IEnumerable<ExecutionEvent> Play()
        {
            if (!CanPlay)
            {
                throw new InvalidOperationException("Nothing to play or already running.");
            }

            State = SessionState.Running;
            return Track(new Interpreter(Level, Plan.ToProgram()).Run(Robot));
        }

        /// <summary>Escalates one hint tier and returns what to show (YZ-01).</summary>
        public HintResult RequestHint()
        {
            if (State != SessionState.Planning)
            {
                return new HintResult(HintTier.None, -1, null);
            }

            if (KeepsState)
            {
                // The memory is partly old by design, so there is no "wrong card" to point at: Roboya only talks.
                HintTier = HintTier.Voice;
                return new HintResult(HintTier.Voice, -1, null);
            }

            if (HintTier < HintTier.ShowCorrectCard)
            {
                HintTier++;
            }

            if (HintTier == HintTier.Voice)
            {
                return new HintResult(HintTier.Voice, -1, null);
            }

            var hint = HintAdvisor.Analyze(Level, Plan.Cards);
            if (!hint.HasHint)
            {
                return new HintResult(HintTier.Voice, -1, null);
            }

            return HintTier == HintTier.HighlightWrongCard
                ? new HintResult(HintTier.HighlightWrongCard, hint.WrongIndex, null)
                : new HintResult(HintTier.ShowCorrectCard, hint.WrongIndex, hint.SuggestedCard);
        }

        /// <summary>Back to planning after a run (the plan stays so the child can fix it).</summary>
        public void ResetRun()
        {
            if (State == SessionState.Running)
            {
                State = SessionState.Planning;
            }
        }

        private IEnumerable<ExecutionEvent> Track(IEnumerable<ExecutionEvent> events)
        {
            foreach (var e in events)
            {
                if (e.Kind == ExecutionEventKind.Finished)
                {
                    Record(e.Result);
                }

                yield return e;
            }
        }

        private void Record(ExecutionResult result)
        {
            Attempts++;
            LastResult = result;
            if (KeepsState)
            {
                Robot = result.FinalState;
            }

            if (result.IsSuccess)
            {
                State = SessionState.Completed;
                FailuresInARow = 0;
                Stars = StarRating.Best(Stars, StarRating.Compute(true, Attempts, Plan.Count, ShortestLength, _rules.FewAttemptsForStar));
            }
            else
            {
                State = SessionState.Planning;
                FailuresInARow++;
            }
        }
    }
}
