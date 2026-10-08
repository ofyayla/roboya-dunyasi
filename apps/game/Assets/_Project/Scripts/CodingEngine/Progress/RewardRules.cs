using System;
using System.Collections.Generic;

namespace Roboya.CodingEngine.Progress
{
    /// <summary>A repair part for Roboya's ship, from content/rewards/ship-parts.json.</summary>
    public sealed class ShipPart
    {
        public ShipPart(string id, int order)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Order = order;
        }

        public string Id { get; }

        /// <summary>1-based position in the fixed earning order.</summary>
        public int Order { get; }
    }

    /// <summary>
    /// PRD rewards: a ship repair part every <see cref="ProgressRules.LevelsPerPart"/> completed levels and one per
    /// finished region; Roboya's crashed ship is mended step by step. The order is fixed and known in advance: no
    /// surprise boxes or chance (CLAUDE.md §13). Earned parts are derived from progress, never bought.
    /// </summary>
    public static class RewardRules
    {
        public static int EarnedCount(int completedLevels, int completedRegions, ProgressRules rules)
        {
            int perPart = Math.Max(1, rules.LevelsPerPart);
            return (Math.Max(0, completedLevels) / perPart) + Math.Max(0, completedRegions);
        }

        public static List<ShipPart> Earned(IEnumerable<ShipPart> catalog, int earnedCount)
        {
            var result = new List<ShipPart>();
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
        public static List<ShipPart> NewlyEarned(IEnumerable<ShipPart> catalog, int before, int after)
        {
            var result = new List<ShipPart>();
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
