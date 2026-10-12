using System;
using System.Collections.Generic;

namespace Roboya.CodingEngine.Sharing
{
    public enum SharingHintKind
    {
        /// <summary>Nothing to point at (the level cannot be won from here).</summary>
        None,

        /// <summary>The plan is right: press play.</summary>
        PressPlay,

        /// <summary>Add this card (to the box when <see cref="SharingHintResult.InBox"/>).</summary>
        Add,

        /// <summary>This card is not on a way to the goal: take it out.</summary>
        Remove,

        /// <summary>Right card, wrong count: change it to <see cref="SharingHintResult.Count"/>.</summary>
        SetCount,

        /// <summary>The box should repeat <see cref="SharingHintResult.Count"/> times.</summary>
        SetTurns,
    }

    /// <summary>What the guiding hand should point at (ADR 0031): one step, never the whole answer.</summary>
    public sealed class SharingHintResult
    {
        public static readonly SharingHintResult NoHint = new SharingHintResult(SharingHintKind.None, null, false, -1, 0);
        public static readonly SharingHintResult Play = new SharingHintResult(SharingHintKind.PressPlay, null, false, -1, 0);

        public SharingHintResult(SharingHintKind kind, SharingCard card, bool inBox, int index, int count)
        {
            Kind = kind;
            Card = card;
            InBox = inBox;
            Index = index;
            Count = count;
        }

        public SharingHintKind Kind { get; }

        /// <summary>The card to add, or the card in the plan the hint is about.</summary>
        public SharingCard Card { get; }

        public bool InBox { get; }

        /// <summary>Where in the box or tail the card stands (Remove, SetCount); -1 otherwise.</summary>
        public int Index { get; }

        /// <summary>The count a card or the box should have (SetCount, SetTurns).</summary>
        public int Count { get; }
    }

    /// <summary>
    /// The next step towards the goal from the plan the child has now. Any shortest way counts, so a child who gives to the second hive first
    /// is not told to start over. Pure logic; the view only shows the hand.
    /// </summary>
    public static class SharingHint
    {
        /// <summary>The search depth for "how far from the goal": more than any level needs.</summary>
        private const int Horizon = 14;

        public static SharingHintResult Next(SharingLevel level, SharingProgram program)
        {
            if (SharingInterpreter.Run(level, program).IsSuccess)
            {
                return SharingHintResult.Play;
            }

            return level.RepeatBox != null ? ForRepeat(level, program) : ForLinear(level, program);
        }

        private static SharingHintResult ForLinear(SharingLevel level, SharingProgram program)
        {
            var alphabet = SharingSolver.Alphabet(level);
            var state = level.Start();
            int distance = SharingSolver.ShortestFrom(level, state, Horizon, alphabet, null);
            if (distance < 0)
            {
                return SharingHintResult.NoHint;
            }

            var cards = program.Tail;
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                if (SharingInterpreter.TryDo(level, state, card, out var next) && SharingSolver.ShortestFrom(level, next, Horizon, alphabet, null) == distance - 1)
                {
                    state = next;
                    distance--;
                    continue;
                }

                // A card that is off the shortest way may only have the wrong count.
                if (card.Kind == SharingCardKind.Give && level.AllowsAmounts)
                {
                    for (int n = 1; n <= SharingLevel.MaxGive; n++)
                    {
                        var other = card.WithCount(n);
                        if (n != card.Count && SharingInterpreter.TryDo(level, state, other, out var via) && SharingSolver.ShortestFrom(level, via, Horizon, alphabet, null) == distance - 1)
                        {
                            return new SharingHintResult(SharingHintKind.SetCount, card, false, i, n);
                        }
                    }
                }

                return new SharingHintResult(SharingHintKind.Remove, card, false, i, 0);
            }

            foreach (var card in alphabet)
            {
                if (SharingInterpreter.TryDo(level, state, card, out var next) && SharingSolver.ShortestFrom(level, next, Horizon, alphabet, null) == distance - 1)
                {
                    return new SharingHintResult(SharingHintKind.Add, card, false, -1, 0);
                }
            }

            return SharingHintResult.NoHint;
        }

        private static SharingHintResult ForRepeat(SharingLevel level, SharingProgram program)
        {
            var solution = SharingSolver.Solve(level);
            if (!solution.IsSolvable)
            {
                return SharingHintResult.NoHint;
            }

            // The shortest plan that shares the most with what the child has built, so the hint follows the child's own way.
            SharingProgram best = null;
            int bestScore = -1;
            foreach (var candidate in solution.Programs)
            {
                int score = (Common(program.Box, candidate.Box) * 100) + (Common(program.Tail, candidate.Tail) * 10) + (program.Turns == candidate.Turns ? 1 : 0);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            int inBox = Common(program.Box, best.Box);
            if (inBox < program.Box.Count)
            {
                return new SharingHintResult(SharingHintKind.Remove, program.Box[inBox], true, inBox, 0);
            }

            if (inBox < best.Box.Count)
            {
                return new SharingHintResult(SharingHintKind.Add, best.Box[inBox], true, -1, 0);
            }

            if (program.Turns != best.Turns)
            {
                return new SharingHintResult(SharingHintKind.SetTurns, null, true, -1, best.Turns);
            }

            int inTail = Common(program.Tail, best.Tail);
            if (inTail < program.Tail.Count)
            {
                return new SharingHintResult(SharingHintKind.Remove, program.Tail[inTail], false, inTail, 0);
            }

            if (inTail < best.Tail.Count)
            {
                return new SharingHintResult(SharingHintKind.Add, best.Tail[inTail], false, -1, 0);
            }

            return SharingHintResult.Play;
        }

        private static int Common(IReadOnlyList<SharingCard> a, IReadOnlyList<SharingCard> b)
        {
            int n = Math.Min(a.Count, b.Count);
            int i = 0;
            while (i < n && a[i].Equals(b[i]))
            {
                i++;
            }

            return i;
        }
    }

    /// <summary>"How much honey will be left?" (corner 3): three dot cards, the right one among them, placed by a fixed order of the level id.</summary>
    public static class SharingPrediction
    {
        private static readonly int[][] Orders =
        {
            new[] { 0, 1, 2 }, new[] { 0, 2, 1 }, new[] { 1, 0, 2 }, new[] { 1, 2, 0 }, new[] { 2, 0, 1 }, new[] { 2, 1, 0 },
        };

        /// <summary>The honey in the basket when the plan has run: the right answer.</summary>
        public static int LeftAfter(SharingLevel level, SharingProgram program) => SharingInterpreter.Run(level, program).Final.Basket;

        /// <summary>
        /// Three different counts: what is left (the answer), what was given and what there was. The order is a permutation of the level id, so the
        /// right card is not always in the same place and a test can still predict it.
        /// </summary>
        public static int[] Choices(string levelId, int before, int left)
        {
            var set = new List<int> { left };
            AddDistinct(set, Math.Max(0, before - left));
            AddDistinct(set, before);
            for (int d = 1; set.Count < 3; d++)
            {
                AddDistinct(set, left + d);
            }

            int sum = 0;
            foreach (char c in levelId ?? string.Empty)
            {
                sum += c;
            }

            var order = Orders[sum % Orders.Length];
            return new[] { set[order[0]], set[order[1]], set[order[2]] };
        }

        private static void AddDistinct(List<int> set, int value)
        {
            if (!set.Contains(value))
            {
                set.Add(value);
            }
        }
    }
}
