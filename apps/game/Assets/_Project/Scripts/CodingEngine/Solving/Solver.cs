using System;
using System.Collections.Generic;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.World;

namespace Roboya.CodingEngine.Solving
{
    /// <summary>
    /// Breadth-first search over (position, facing, collected items) using the level's primitive move cards.
    /// Gives solvability and the shortest solution length used for the 3rd star (OYN-04), validation
    /// and hints. Loop-aware "shortest code" (counting repeat cards) is a later extension (Döngü Dansı).
    /// </summary>
    public static class Solver
    {
        public const int DefaultStateBudget = 500_000;

        private static readonly CardType[] SearchOrder =
        {
            CardType.Forward, CardType.TurnLeft, CardType.TurnRight, CardType.Backward,
        };

        public static SolveResult Solve(Level level, int stateBudget = DefaultStateBudget) =>
            Solve(level, level.Start, level.MaxProgramLength, stateBudget);

        /// <summary>Shortest continuation from <paramref name="from"/> using at most <paramref name="maxLength"/> cards.</summary>
        public static SolveResult Solve(Level level, RobotState from, int maxLength, int stateBudget = DefaultStateBudget)
        {
            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            var cards = new List<CardType>(4);
            foreach (var card in SearchOrder)
            {
                if (level.AvailableCards.Contains(card))
                {
                    cards.Add(card);
                }
            }

            if (level.Goal.IsSatisfiedBy(from))
            {
                return new SolveResult(SolveStatus.Solved, Array.Empty<CardType>(), 1);
            }

            var parent = new Dictionary<RobotState, Link>();
            var depth = new Dictionary<RobotState, int>();
            var queue = new Queue<RobotState>();
            parent[from] = default;
            depth[from] = 0;
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                var state = queue.Dequeue();
                int d = depth[state];
                if (d >= maxLength)
                {
                    continue;
                }

                foreach (var card in cards)
                {
                    if (!TryApply(level, state, card, out var next) || parent.ContainsKey(next))
                    {
                        continue;
                    }

                    parent[next] = new Link(state, card);
                    depth[next] = d + 1;
                    if (level.Goal.IsSatisfiedBy(next))
                    {
                        return new SolveResult(SolveStatus.Solved, Reconstruct(parent, from, next), parent.Count);
                    }

                    if (parent.Count >= stateBudget)
                    {
                        return new SolveResult(SolveStatus.SearchLimitReached, Array.Empty<CardType>(), parent.Count);
                    }

                    queue.Enqueue(next);
                }
            }

            return new SolveResult(SolveStatus.Unsolvable, Array.Empty<CardType>(), parent.Count);
        }

        /// <summary>Applies one primitive card. Returns false when the robot would bump.</summary>
        public static bool TryApply(Level level, RobotState state, CardType card, out RobotState next)
        {
            switch (card)
            {
                case CardType.TurnLeft:
                    next = state.With(state.Facing.TurnLeft());
                    return true;
                case CardType.TurnRight:
                    next = state.With(state.Facing.TurnRight());
                    return true;
                case CardType.Forward:
                case CardType.Backward:
                    var dir = card == CardType.Forward ? state.Facing : state.Facing.Opposite();
                    var target = state.Position.Step(dir);
                    if (!level.Grid.IsWalkable(target))
                    {
                        next = state;
                        return false;
                    }

                    next = state.With(target);
                    int item = level.ItemIndexAt(target);
                    if (item >= 0)
                    {
                        next = next.WithCollected(item);
                    }

                    return true;
                default:
                    throw new ArgumentException(card + " is not a primitive move card.", nameof(card));
            }
        }

        private static IReadOnlyList<CardType> Reconstruct(Dictionary<RobotState, Link> parent, RobotState from, RobotState goal)
        {
            var cards = new List<CardType>();
            var s = goal;
            while (!s.Equals(from))
            {
                var link = parent[s];
                cards.Add(link.Card);
                s = link.Previous;
            }

            cards.Reverse();
            return cards.AsReadOnly();
        }

        private readonly struct Link
        {
            public Link(RobotState previous, CardType card)
            {
                Previous = previous;
                Card = card;
            }

            public RobotState Previous { get; }

            public CardType Card { get; }
        }
    }
}
