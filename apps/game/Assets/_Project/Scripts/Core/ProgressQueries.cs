using System.Collections.Generic;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Progress;

namespace Roboya.Core
{
    /// <summary>Progress questions that need the level catalog (regions, region paths).</summary>
    public static class ProgressQueries
    {
        /// <summary>The ordered level ids on a region's path (all games of the region, by order).</summary>
        public static List<string> PathOf(LevelCatalog catalog, RegionId region)
        {
            var ids = new List<string>();
            foreach (var e in catalog.All)
            {
                if (e.Dto.Region == region)
                {
                    ids.Add(e.Id);
                }
            }

            return ids;
        }

        /// <summary>Regions whose every level has at least one star.</summary>
        public static int CompletedRegions(LevelCatalog catalog, ProgressBook book)
        {
            var seen = new Dictionary<RegionId, bool>();
            foreach (var e in catalog.All)
            {
                bool done = book.IsCompleted(e.Id);
                seen[e.Dto.Region] = seen.TryGetValue(e.Dto.Region, out var all) ? all && done : done;
            }

            int count = 0;
            foreach (var done in seen.Values)
            {
                if (done)
                {
                    count++;
                }
            }

            return count;
        }

        public static int EarnedParts(GameServices s) =>
            RewardRules.EarnedCount(s.Progress.Book.CompletedCount, CompletedRegions(s.Catalog, s.Progress.Book), s.ProgressRules);

        public static NodeState[] PathStates(GameServices s, IReadOnlyList<string> path) =>
            PathRules.StatesOf(path, s.Progress.Book, s.ProgressRules.FreeLevelCount, s.Entitlements.HasPremium);
    }
}
