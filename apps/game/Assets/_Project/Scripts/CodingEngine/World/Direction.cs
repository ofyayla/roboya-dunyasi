namespace Roboya.CodingEngine.World
{
    /// <summary>Facing on the grid. Y grows downward (row 0 is the top / north edge).</summary>
    public enum Direction
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3,
    }

    public static class DirectionExtensions
    {
        public static Direction TurnLeft(this Direction d) => (Direction)(((int)d + 3) % 4);

        public static Direction TurnRight(this Direction d) => (Direction)(((int)d + 1) % 4);

        public static Direction Opposite(this Direction d) => (Direction)(((int)d + 2) % 4);

        public static GridPosition Offset(this Direction d)
        {
            switch (d)
            {
                case Direction.North: return new GridPosition(0, -1);
                case Direction.East: return new GridPosition(1, 0);
                case Direction.South: return new GridPosition(0, 1);
                default: return new GridPosition(-1, 0);
            }
        }
    }
}
