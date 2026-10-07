using System;
using System.Collections.Generic;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.World;

namespace Roboya.CodingEngine.Solving
{
    /// <summary>
    /// Finds the first card in a flat program that makes the level unsolvable from that point, and the card
    /// that would fit there (YZ-01 hint tiers 2 and 3: highlight the wrong card, then show the right one).
    /// </summary>
    public static class HintAdvisor
    {
        public static Hint Analyze(Level level, IReadOnlyList<CardType> program)
        {
            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            if (program == null)
            {
                throw new ArgumentNullException(nameof(program));
            }

            var state = level.Start;
            var fromStart = Solver.Solve(level, state, level.MaxProgramLength);
            if (!fromStart.IsSolved)
            {
                return Hint.None;
            }

            // Walk the prefix while it can still be completed within the remaining slots.
            for (int i = 0; i < program.Count; i++)
            {
                var card = program[i];
                var continuation = Solver.Solve(level, state, level.MaxProgramLength - i);
                var suggested = continuation.Solution.Count > 0 ? continuation.Solution[0] : (CardType?)null;

                if (!card.IsPrimitiveMove() || !Solver.TryApply(level, state, card, out var next))
                {
                    return new Hint(i, suggested);
                }

                var after = Solver.Solve(level, next, level.MaxProgramLength - i - 1);
                if (!after.IsSolved)
                {
                    return new Hint(i, suggested);
                }

                state = next;
            }

            if (level.Goal.IsSatisfiedBy(state))
            {
                return Hint.None;
            }

            // Every card was fine but the plan is too short: point at the next empty slot.
            var rest = Solver.Solve(level, state, level.MaxProgramLength - program.Count);
            return new Hint(program.Count, rest.Solution.Count > 0 ? rest.Solution[0] : (CardType?)null);
        }
    }

    public readonly struct Hint
    {
        public static readonly Hint None = new Hint(-1, null);

        public Hint(int wrongIndex, CardType? suggestedCard)
        {
            WrongIndex = wrongIndex;
            SuggestedCard = suggestedCard;
        }

        /// <summary>Index of the first wrong card, the program length if a card is missing, or -1 if correct.</summary>
        public int WrongIndex { get; }

        public CardType? SuggestedCard { get; }

        public bool HasHint => WrongIndex >= 0;
    }
}
