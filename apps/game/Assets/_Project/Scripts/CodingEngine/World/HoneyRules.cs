using System;

namespace Roboya.CodingEngine.World
{
    public enum DropOutcome
    {
        Dropped,

        /// <summary>The basket is empty or there is no drop point on this cell.</summary>
        NothingToDrop,

        /// <summary>The honey was left on a hive that already had everything it needs; it overflows.</summary>
        Overflow,
    }

    /// <summary>
    /// How honey moves (ADR 0030, PK-0001). One place for the numbers: the interpreter calls these rules step by step to
    /// animate, the solver calls them to search, so a level can never play one way and solve another.
    /// </summary>
    public static class HoneyRules
    {
        /// <summary>Puts the honey of item <paramref name="itemIndex"/> in the basket; only honey items carry honey.</summary>
        public static RobotState Collect(Level level, RobotState state, int itemIndex)
        {
            var item = level.Items[itemIndex];
            return item.Kind == "honey" ? state.WithBasket(state.Basket + item.Amount) : state;
        }

        /// <summary>How many honey a hive takes when the bee enters its cell in auto-drop mode (Minik).</summary>
        public static int AutoDropAmount(Level level, RobotState state, int dropIndex)
        {
            var drop = level.Drops[dropIndex];
            if (!level.AutoDrop || drop.Kind != DropPointKind.Hive)
            {
                return 0;
            }

            return Math.Max(0, Math.Min(state.Basket, drop.Need - state.DeliveredAt(dropIndex)));
        }

        /// <summary>Moves one honey from the basket to a drop point.</summary>
        public static RobotState GiveOne(RobotState state, int dropIndex) =>
            state.WithBasket(state.Basket - 1).WithDelivered(dropIndex, state.DeliveredAt(dropIndex) + 1);

        /// <summary>Applies the auto-drop on entering the cell of <paramref name="dropIndex"/>; returns the state after.</summary>
        public static RobotState AutoDrop(Level level, RobotState state, int dropIndex)
        {
            int amount = AutoDropAmount(level, state, dropIndex);
            for (int i = 0; i < amount; i++)
            {
                state = GiveOne(state, dropIndex);
            }

            return state;
        }

        /// <summary>The "Bırak" card: leaves one honey where the bee stands.</summary>
        public static DropOutcome TryDrop(Level level, RobotState state, out RobotState next)
        {
            int index = level.DropIndexAt(state.Position);
            if (index < 0 || state.Basket == 0)
            {
                next = state;
                return DropOutcome.NothingToDrop;
            }

            next = GiveOne(state, index);
            var drop = level.Drops[index];
            return drop.Kind == DropPointKind.Hive && next.DeliveredAt(index) > drop.Need ? DropOutcome.Overflow : DropOutcome.Dropped;
        }
    }
}
