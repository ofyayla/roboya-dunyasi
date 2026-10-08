using System;
using System.Collections.Generic;

namespace Roboya.CodingEngine.Progress
{
    /// <summary>A robot part from content/rewards/robot-parts.json.</summary>
    public sealed class RobotPart
    {
        public RobotPart(string id, string slot, int order)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Slot = slot ?? throw new ArgumentNullException(nameof(slot));
            Order = order;
        }

        public string Id { get; }

        /// <summary>antenna, color, wings or hat.</summary>
        public string Slot { get; }

        /// <summary>1-based position in the fixed earning order.</summary>
        public int Order { get; }
    }

    /// <summary>
    /// PRD rewards: a robot part every <see cref="ProgressRules.LevelsPerPart"/> completed levels and one per finished
    /// region. The order is fixed and known in advance: no surprise boxes or chance (CLAUDE.md §13). Earned parts
    /// are derived from progress, never bought.
    /// </summary>
    public static class RewardRules
    {
        public static int EarnedCount(int completedLevels, int completedRegions, ProgressRules rules)
        {
            int perPart = Math.Max(1, rules.LevelsPerPart);
            return (Math.Max(0, completedLevels) / perPart) + Math.Max(0, completedRegions);
        }

        public static List<RobotPart> Earned(IEnumerable<RobotPart> catalog, int earnedCount)
        {
            var result = new List<RobotPart>();
            foreach (var part in catalog)
            {
                if (part.Order <= earnedCount)
                {
                    result.Add(part);
                }
            }

            result.Sort((a, b) => a.Order.CompareTo(b.Order));
            return result;
        }

        /// <summary>Parts earned by moving from <paramref name="before"/> to <paramref name="after"/> (the reveal moment).</summary>
        public static List<RobotPart> NewlyEarned(IEnumerable<RobotPart> catalog, int before, int after)
        {
            var result = new List<RobotPart>();
            foreach (var part in catalog)
            {
                if (part.Order > before && part.Order <= after)
                {
                    result.Add(part);
                }
            }

            result.Sort((a, b) => a.Order.CompareTo(b.Order));
            return result;
        }
    }
}
