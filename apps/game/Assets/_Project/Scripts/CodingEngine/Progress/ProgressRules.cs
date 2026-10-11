namespace Roboya.CodingEngine.Progress
{
    /// <summary>
    /// Tunable progression values. Defaults follow the PRD and product decisions; production values come from
    /// server configuration (CLAUDE.md §6, §12: the free level count is not hard-coded in game logic).
    /// </summary>
    public sealed class ProgressRules
    {
        public static readonly ProgressRules Default = new ProgressRules(3, 1, 4);

        public ProgressRules(int freeLevelCount, int freeProfiles = 1, int premiumProfiles = 4, bool unlockAll = false, bool askRemaining = false)
        {
            AskRemaining = askRemaining;
            UnlockAll = unlockAll;
            FreeProfiles = freeProfiles;
            PremiumProfiles = premiumProfiles;
            FreeLevelCount = freeLevelCount;
        }

        /// <summary>
        /// Testing only: every level can be opened, whatever the order and the free tier. Set solely by development player
        /// builds (never in release builds or the editor), so it can never grant access to a real user.
        /// </summary>
        public bool UnlockAll { get; }

        /// <summary>
        /// Paylaşım Köyü "Kaç kaldı?" step (PK-0002): off until the child test says it helps; the level option alone never turns it on.
        /// Production values come from server configuration like the others.
        /// </summary>
        public bool AskRemaining { get; }

        /// <summary>Product decision: the first 3 levels are free.</summary>
        public int FreeLevelCount { get; }

        /// <summary>PRD: 1 child profile on the free tier, up to 4 with Family Premium (the server enforces the same).</summary>
        public int FreeProfiles { get; }

        public int PremiumProfiles { get; }

        public int ProfileLimit(bool premium) => premium ? PremiumProfiles : FreeProfiles;
    }
}
