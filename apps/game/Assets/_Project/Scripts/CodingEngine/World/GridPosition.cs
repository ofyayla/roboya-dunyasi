using System;

namespace Roboya.CodingEngine.World
{
    public readonly struct GridPosition : IEquatable<GridPosition>
    {
        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }

        public int Y { get; }

        public GridPosition Step(Direction direction)
        {
            var o = direction.Offset();
            return new GridPosition(X + o.X, Y + o.Y);
        }

        public bool Equals(GridPosition other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);

        public override int GetHashCode() => (X * 397) ^ Y;

        public static bool operator ==(GridPosition a, GridPosition b) => a.Equals(b);

        public static bool operator !=(GridPosition a, GridPosition b) => !a.Equals(b);

        public override string ToString() => "(" + X + "," + Y + ")";
    }
}
