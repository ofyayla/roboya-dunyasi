using System.Collections.Generic;
using Roboya.CodingEngine.World;

namespace Roboya.CodingEngine.Play
{
    public enum HiveState
    {
        /// <summary>Nothing has been given yet.</summary>
        Empty,

        /// <summary>Some honey, less than the hive asks for.</summary>
        Partial,

        /// <summary>Exactly what the hive asks for (a table with honey on it counts as full).</summary>
        Full,

        /// <summary>More than the hive asks for: the honey spills over.</summary>
        Overflow,
    }

    /// <summary>How one drop point stands: its kind, what it asks for, what it holds.</summary>
    public readonly struct DropStatus
    {
        public DropStatus(int index, DropPointKind kind, int need, int count, HiveState state)
        {
            Index = index;
            Kind = kind;
            Need = need;
            Count = count;
            State = state;
        }

        public int Index { get; }

        public DropPointKind Kind { get; }

        /// <summary>Honey a hive asks for; 0 for the table.</summary>
        public int Need { get; }

        /// <summary>Honey on it now.</summary>
        public int Count { get; }

        public HiveState State { get; }
    }

    /// <summary>
    /// The board's honey in one place (ADR 0030): the basket and how each hive and the table stand in a robot state. The view draws
    /// from it and the end-of-run feedback reads it, so the rule for "empty, partial, full, spilling" lives here once.
    /// </summary>
    public sealed class HiveReport
    {
        private HiveReport(int basket, IReadOnlyList<DropStatus> drops)
        {
            Basket = basket;
            Drops = drops;
        }

        public int Basket { get; }

        public IReadOnlyList<DropStatus> Drops { get; }

        public static HiveReport Of(Level level, RobotState state)
        {
            var drops = new List<DropStatus>(level.Drops.Count);
            for (int i = 0; i < level.Drops.Count; i++)
            {
                var drop = level.Drops[i];
                int count = state.DeliveredAt(i);
                drops.Add(new DropStatus(i, drop.Kind, drop.Need, count, StateOf(drop, count)));
            }

            return new HiveReport(state.Basket, drops);
        }

        private static HiveState StateOf(DropPoint drop, int count)
        {
            if (count == 0)
            {
                return HiveState.Empty;
            }

            if (drop.Kind == DropPointKind.Table)
            {
                return HiveState.Full;
            }

            if (count > drop.Need)
            {
                return HiveState.Overflow;
            }

            return count == drop.Need ? HiveState.Full : HiveState.Partial;
        }
    }
}
