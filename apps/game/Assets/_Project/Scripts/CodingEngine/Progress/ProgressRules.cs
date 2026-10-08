namespace Roboya.CodingEngine.Progress
{
    /// <summary>
    /// Tunable progression values. Defaults follow the PRD and product decisions; production values come from
    /// server configuration (CLAUDE.md §6, §12: the free level count is not hard-coded in game logic).
    /// </summary>
    public sealed class ProgressRules
    {
        public static readonly ProgressRules Default = new ProgressRules(3, 5);

        public ProgressRules(int freeLevelCount, int levelsPerPart)
        {
            FreeLevelCount = freeLevelCount;
            LevelsPerPart = levelsPerPart;
        }

        /// <summary>Product decision: the first 3 levels are free.</summary>
        public int FreeLevelCount { get; }

        /// <summary>PRD rewards: a ship repair part every 5 completed levels.</summary>
        public int LevelsPerPart { get; }
    }
}
