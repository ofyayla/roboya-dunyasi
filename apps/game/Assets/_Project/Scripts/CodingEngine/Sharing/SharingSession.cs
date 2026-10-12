using System;
using Roboya.CodingEngine.Play;
using Roboya.CodingEngine.Scoring;

namespace Roboya.CodingEngine.Sharing
{
    /// <summary>
    /// One child playing one village level: the plan, runs, attempt counting, hint offer and stars. Like <see cref="LevelSession"/>, mistakes
    /// are never punished: failed runs only unlock help (PRD "Hata cezalandırılmaz"). The view animates the events a run returns.
    /// </summary>
    public sealed class SharingSession
    {
        private readonly SessionRules _rules;

        public SharingSession(SharingLevel level, int shortestLength, SessionRules rules = null)
        {
            Level = level ?? throw new ArgumentNullException(nameof(level));
            ShortestLength = shortestLength;
            _rules = rules ?? SessionRules.Default;
            Plan = new SharingPlan(level);
        }

        public SharingLevel Level { get; }

        public SharingPlan Plan { get; }

        /// <summary>From the validator; -1 if unknown (the third star then cannot be earned).</summary>
        public int ShortestLength { get; }

        public SessionState State { get; private set; } = SessionState.Planning;

        public int Attempts { get; private set; }

        public int FailuresInARow { get; private set; }

        public int HintsUsed { get; private set; }

        public int Stars { get; private set; }

        public SharingResult LastResult { get; private set; }

        /// <summary>The whole plan runs at once, so there must be something to run.</summary>
        public bool CanPlay => State == SessionState.Planning && !Plan.IsEmpty;

        /// <summary>OYN-05 / YZ-01: after this many failed runs in a row the hand points at the next step on its own.</summary>
        public bool ShouldOfferHint => State == SessionState.Planning && FailuresInARow >= _rules.HintAfterFailures;

        /// <summary>YZ-03: an easier level is offered after this many failed runs.</summary>
        public bool ShouldOfferAlternative => State == SessionState.Planning && FailuresInARow >= _rules.AlternativeAfterFailures;

        /// <summary>Runs the plan and records the attempt. A won level completes; a lost run goes back to planning with the plan kept.</summary>
        public SharingResult Play()
        {
            if (!CanPlay)
            {
                throw new InvalidOperationException("Nothing to play or already finished.");
            }

            var result = SharingInterpreter.Run(Level, Plan.ToProgram());
            Attempts++;
            LastResult = result;
            if (result.IsSuccess)
            {
                State = SessionState.Completed;
                FailuresInARow = 0;
                Stars = StarRating.Best(Stars, StarRating.Compute(true, Attempts, Plan.Count, ShortestLength, _rules.FewAttemptsForStar));
            }
            else
            {
                FailuresInARow++;
            }

            return result;
        }

        /// <summary>The next step for the guiding hand. Asking counts as one hint used (analytics only; it never costs anything).</summary>
        public SharingHintResult RequestHint()
        {
            HintsUsed++;
            return SharingHint.Next(Level, Plan.ToProgram());
        }
    }
}
