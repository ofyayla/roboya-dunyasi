using System;

namespace Roboya.CodingEngine.World
{
    /// <summary>
    /// Immutable robot state: position, facing, collected items (bit per item index) and, for Paylaşım Köyü (ADR 0030),
    /// the honey basket and how much honey each drop point has received. Basket and deliveries share one packed
    /// <see cref="Honey"/> word (4 bits each) so states stay small keys for the solver's search.
    /// </summary>
    public readonly struct RobotState : IEquatable<RobotState>
    {
        /// <summary>Largest value a 4-bit counter (basket or one drop point) can hold.</summary>
        public const int MaxCount = 15;

        /// <summary>Drop points a level may define; the packed word has room for exactly this many.</summary>
        public const int MaxDropPoints = 4;

        private const int Bits = 4;
        private const ulong Mask = 0xF;

        public RobotState(GridPosition position, Direction facing, ulong collectedMask, ulong honey = 0)
        {
            Position = position;
            Facing = facing;
            CollectedMask = collectedMask;
            Honey = honey;
        }

        public GridPosition Position { get; }

        public Direction Facing { get; }

        public ulong CollectedMask { get; }

        /// <summary>Packed counters: bits 0–3 the basket, then 4 bits per drop point.</summary>
        public ulong Honey { get; }

        /// <summary>Honey the robot is carrying.</summary>
        public int Basket => (int)(Honey & Mask);

        /// <summary>Honey handed to drop point <paramref name="dropIndex"/> so far.</summary>
        public int DeliveredAt(int dropIndex) => (int)((Honey >> ShiftOf(dropIndex)) & Mask);

        public RobotState With(GridPosition position) => new RobotState(position, Facing, CollectedMask, Honey);

        public RobotState With(Direction facing) => new RobotState(Position, facing, CollectedMask, Honey);

        public RobotState WithCollected(int itemIndex) => new RobotState(Position, Facing, CollectedMask | (1UL << itemIndex), Honey);

        public RobotState WithBasket(int count) => new RobotState(Position, Facing, CollectedMask, Set(Honey, 0, count));

        public RobotState WithDelivered(int dropIndex, int count) =>
            new RobotState(Position, Facing, CollectedMask, Set(Honey, ShiftOf(dropIndex), count));

        public bool HasCollected(int itemIndex) => (CollectedMask & (1UL << itemIndex)) != 0;

        public bool Equals(RobotState other) =>
            Position == other.Position && Facing == other.Facing && CollectedMask == other.CollectedMask && Honey == other.Honey;

        public override bool Equals(object obj) => obj is RobotState other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (((Position.GetHashCode() * 31 + (int)Facing) * 31 + CollectedMask.GetHashCode()) * 31) + Honey.GetHashCode();
            }
        }

        private static int ShiftOf(int dropIndex)
        {
            if (dropIndex < 0 || dropIndex >= MaxDropPoints)
            {
                throw new ArgumentOutOfRangeException(nameof(dropIndex), "A level has at most " + MaxDropPoints + " drop points.");
            }

            return Bits * (dropIndex + 1);
        }

        private static ulong Set(ulong word, int shift, int count)
        {
            if (count < 0 || count > MaxCount)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "A honey counter holds 0.." + MaxCount + ".");
            }

            return (word & ~(Mask << shift)) | ((ulong)count << shift);
        }
    }
}
