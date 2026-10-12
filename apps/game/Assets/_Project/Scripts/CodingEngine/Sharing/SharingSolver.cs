using System;
using System.Collections.Generic;

namespace Roboya.CodingEngine.Sharing
{
    /// <summary>The shortest plans of a level, for the validator, the star rating and the hints.</summary>
    public sealed class SharingSolution
    {
        public SharingSolution(int length, IReadOnlyList<SharingProgram> programs)
        {
            Length = length;
            Programs = programs;
        }

        /// <summary>The cards of the shortest plan, -1 when the level cannot be won within its card limit.</summary>
        public int Length { get; }

        public bool IsSolvable => Length >= 0;

        /// <summary>Every shortest plan, at most <see cref="SharingSolver.MaxPrograms"/>.</summary>
        public IReadOnlyList<SharingProgram> Programs { get; }
    }

    /// <summary>
    /// Finds the shortest plans. A level without a repeat box is searched breadth first over honey states (every plan of the same state is
    /// the same, so the search stays tiny). A level with a repeat box tries every box body and turn count and finishes with the table cards.
    /// </summary>
    public static class SharingSolver
    {
        public const int MaxPrograms = 256;

        /// <param name="allowRepeat">False searches as if the level had no repeat box: used to check that the box is really needed.</param>
        public static SharingSolution Solve(SharingLevel level, bool allowRepeat = true)
        {
            return level.RepeatBox != null && allowRepeat ? SolveRepeat(level) : SolveLinear(level);
        }

        /// <summary>Every card a plan may use on this level, in a fixed order: takes, then gives to each hive, then the table.</summary>
        public static List<SharingCard> Alphabet(SharingLevel level)
        {
            var cards = new List<SharingCard>();
            for (int f = 0; f < level.Flowers.Count; f++)
            {
                cards.Add(SharingCard.Take(f));
            }

            int most = level.AllowsAmounts ? SharingLevel.MaxGive : 1;
            for (int h = 0; h < level.Hives.Count; h++)
            {
                for (int n = 1; n <= most; n++)
                {
                    cards.Add(SharingCard.Give(h, n));
                }
            }

            if (level.HasTable)
            {
                for (int n = 1; n <= most; n++)
                {
                    cards.Add(SharingCard.Give(SharingLevel.Table, n));
                }
            }

            return cards;
        }

        /// <summary>The fewest cards that take the honey from <paramref name="start"/> to the goal and the plans that do it; depth -1 if none within <paramref name="maxDepth"/>.</summary>
        public static int ShortestFrom(SharingLevel level, SharingState start, int maxDepth, IReadOnlyList<SharingCard> cards, List<List<SharingCard>> plans)
        {
            plans?.Clear();
            if (SharingRules.IsGoalMet(level, start))
            {
                plans?.Add(new List<SharingCard>());
                return 0;
            }

            var depthOf = new Dictionary<long, int> { { start.Key, 0 } };
            var layers = new List<List<SharingState>> { new List<SharingState> { start } };
            int goalDepth = -1;
            for (int depth = 1; depth <= maxDepth && goalDepth < 0; depth++)
            {
                var next = new List<SharingState>();
                foreach (var state in layers[depth - 1])
                {
                    for (int c = 0; c < cards.Count; c++)
                    {
                        if (!SharingInterpreter.TryDo(level, state, cards[c], out var after) || depthOf.ContainsKey(after.Key))
                        {
                            continue;
                        }

                        depthOf[after.Key] = depth;
                        next.Add(after);
                        if (SharingRules.IsGoalMet(level, after))
                        {
                            goalDepth = depth;
                        }
                    }
                }

                layers.Add(next);
                if (next.Count == 0)
                {
                    break;
                }
            }

            if (goalDepth < 0)
            {
                return -1;
            }

            if (plans != null)
            {
                // Walk back from the goal states: a state is on a shortest plan if one card leads from it to such a state one layer deeper.
                var good = new HashSet<long>[goalDepth + 1];
                good[goalDepth] = new HashSet<long>();
                foreach (var state in layers[goalDepth])
                {
                    if (SharingRules.IsGoalMet(level, state))
                    {
                        good[goalDepth].Add(state.Key);
                    }
                }

                for (int d = goalDepth - 1; d >= 0; d--)
                {
                    good[d] = new HashSet<long>();
                    foreach (var state in layers[d])
                    {
                        for (int c = 0; c < cards.Count; c++)
                        {
                            if (SharingInterpreter.TryDo(level, state, cards[c], out var after) && depthOf.TryGetValue(after.Key, out int dd) && dd == d + 1 && good[d + 1].Contains(after.Key))
                            {
                                good[d].Add(state.Key);
                                break;
                            }
                        }
                    }
                }

                Collect(level, start, 0, goalDepth, cards, depthOf, good, new List<SharingCard>(), plans);
            }

            return goalDepth;
        }

        private static void Collect(
            SharingLevel level,
            SharingState state,
            int depth,
            int goalDepth,
            IReadOnlyList<SharingCard> cards,
            Dictionary<long, int> depthOf,
            HashSet<long>[] good,
            List<SharingCard> path,
            List<List<SharingCard>> plans)
        {
            if (plans.Count >= MaxPrograms)
            {
                return;
            }

            if (depth == goalDepth)
            {
                plans.Add(new List<SharingCard>(path));
                return;
            }

            for (int c = 0; c < cards.Count; c++)
            {
                if (SharingInterpreter.TryDo(level, state, cards[c], out var after) && depthOf.TryGetValue(after.Key, out int dd) && dd == depth + 1 && good[depth + 1].Contains(after.Key))
                {
                    path.Add(cards[c]);
                    Collect(level, after, depth + 1, goalDepth, cards, depthOf, good, path, plans);
                    path.RemoveAt(path.Count - 1);
                }
            }
        }

        private static SharingSolution SolveLinear(SharingLevel level)
        {
            var plans = new List<List<SharingCard>>();
            int length = ShortestFrom(level, level.Start(), level.MaxCards, Alphabet(level), plans);
            var programs = new List<SharingProgram>(plans.Count);
            foreach (var plan in plans)
            {
                programs.Add(SharingProgram.Linear(plan));
            }

            return new SharingSolution(length, programs);
        }

        private static SharingSolution SolveRepeat(SharingLevel level)
        {
            var box = level.RepeatBox;
            var alphabet = new List<SharingCard>();
            foreach (var card in Alphabet(level))
            {
                if (!card.IsTable)
                {
                    alphabet.Add(card);
                }
            }

            var tailCards = new List<SharingCard>();
            if (level.HasTable)
            {
                tailCards.Add(SharingCard.Give(SharingLevel.Table, 1));
            }

            int best = level.MaxCards + 1;
            var found = new List<SharingProgram>();
            var tails = new List<List<SharingCard>>();
            for (int size = 1; size <= box.Slots && size <= level.MaxCards; size++)
            {
                foreach (var body in Sequences(alphabet, size))
                {
                    for (int turns = 1; turns <= box.MaxTurns; turns++)
                    {
                        if (!TryRepeat(level, body, turns, out var after))
                        {
                            continue;
                        }

                        int limit = Math.Min(level.MaxCards, best) - size;
                        if (limit < 0)
                        {
                            continue;
                        }

                        int tail = ShortestFrom(level, after, limit, tailCards, tails);
                        if (tail < 0)
                        {
                            continue;
                        }

                        int total = size + tail;
                        if (total < best)
                        {
                            best = total;
                            found.Clear();
                        }

                        foreach (var t in tails)
                        {
                            if (found.Count < MaxPrograms)
                            {
                                found.Add(new SharingProgram(turns, body, t));
                            }
                        }
                    }
                }
            }

            return found.Count == 0 ? new SharingSolution(-1, found) : new SharingSolution(best, found);
        }

        private static bool TryRepeat(SharingLevel level, IReadOnlyList<SharingCard> body, int turns, out SharingState after)
        {
            after = level.Start();
            for (int t = 0; t < turns; t++)
            {
                for (int i = 0; i < body.Count; i++)
                {
                    if (!SharingInterpreter.TryDo(level, after, body[i], out after))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>Every sequence of <paramref name="size"/> cards over the alphabet, in a fixed order.</summary>
        private static IEnumerable<SharingCard[]> Sequences(IReadOnlyList<SharingCard> alphabet, int size)
        {
            if (alphabet.Count == 0)
            {
                yield break;
            }

            var index = new int[size];
            while (true)
            {
                var sequence = new SharingCard[size];
                for (int i = 0; i < size; i++)
                {
                    sequence[i] = alphabet[index[i]];
                }

                yield return sequence;
                int k = size - 1;
                while (k >= 0 && ++index[k] == alphabet.Count)
                {
                    index[k] = 0;
                    k--;
                }

                if (k < 0)
                {
                    yield break;
                }
            }
        }
    }
}
