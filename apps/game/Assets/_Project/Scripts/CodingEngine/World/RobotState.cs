using System;

namespace Roboya.CodingEngine.World
{
    /// <summary>Immutable robot state: position, facing and collected items (bit per item index).</summary>
    public readonly struct RobotState : IEquatable<RobotState>
    {
        public RobotState(GridPosition position, Direction facing, ulong collectedMask)
        {
            Position = position;
            Facing = facing;
            CollectedMask = collectedMask;
        }

        public GridPosition Position { get; }

        public Direction Facing { get; }

        public ulong CollectedMask { get; }

        public RobotState With(GridPosition position) => new RobotState(position, Facing, CollectedMask);

        public RobotState With(Direction facing) => new RobotState(Position, facing, CollectedMask);

        public RobotState WithCollected(int itemIndex) => new RobotState(Position, Facing, CollectedMask | (1UL << itemIndex));

        public bool HasCollected(int itemIndex) => (CollectedMask & (1UL << itemIndex)) != 0;

        public bool Equals(RobotState other) =>
            Position == other.Position && Facing == other.Facing && CollectedMask == other.CollectedMask;

        public override bool Equals(object obj) => obj is RobotState other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Position.GetHashCode() * 31 + (int)Facing) * 31 + CollectedMask.GetHashCode();
            }
        }
    }
}
