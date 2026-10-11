namespace Roboya.CodingEngine.Progress
{
    /// <summary>
    /// A region path is split into <see cref="CornersPerRegion"/> equal corners (Sabır Ormanı 36 = 4 × 9, Paylaşım
    /// Köyü 24 = 4 × 6). The path map marks the end of each corner and the game closes it with the region's value
    /// moment (F1-24). The corner length follows the path, so a new region needs no code change.
    /// </summary>
    public static class CornerRules
    {
        public const int CornersPerRegion = 4;

        /// <summary>Levels per corner; 0 when the path is too short to have corners.</summary>
        public static int CornerLength(int pathCount) => pathCount >= CornersPerRegion ? pathCount / CornersPerRegion : 0;

        /// <summary>Corners drawn on a path of <paramref name="pathCount"/> levels.</summary>
        public static int CornerCount(int pathCount) => CornerLength(pathCount) > 0 ? CornersPerRegion : 0;

        /// <summary>
        /// Index (0-based) of the last level of corner <paramref name="corner"/> (1-based). The last corner always ends on
        /// the region finale, so a path that does not split evenly gives its spare levels to the last stretch.
        /// </summary>
        public static int CornerEnd(int corner, int pathCount) =>
            corner >= CornersPerRegion ? pathCount - 1 : (corner * CornerLength(pathCount)) - 1;

        /// <summary>True when the level at <paramref name="index"/> (0-based) is the last one of a corner.</summary>
        public static bool IsCornerEnd(int index, int pathCount)
        {
            for (int c = 1; c <= CornerCount(pathCount); c++)
            {
                if (CornerEnd(c, pathCount) == index)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
