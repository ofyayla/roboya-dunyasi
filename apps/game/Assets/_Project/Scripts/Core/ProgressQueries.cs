using System.Collections.Generic;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Profiles;
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
            PathRules.StatesOf(path, s.Progress.Book, s.ProgressRules.FreeLevelCount, s.Entitlements.HasPremium, StartIndex(s, path), s.ProgressRules.UnlockAll, IntroLevels(s.Catalog, path));

        /// <summary>A game's first level and every level that introduces a card: the tutorials a late starter must not skip.</summary>
        public static List<bool> IntroLevels(LevelCatalog catalog, IReadOnlyList<string> path)
        {
            var seen = new HashSet<GameId>();
            var flags = new List<bool>(path.Count);
            foreach (var id in path)
            {
                var dto = catalog.Find(id)?.Dto;
                bool first = dto != null && seen.Add(dto.Game);
                flags.Add(first || (dto?.Cards?.Introduces != null));
            }

            return flags;
        }

        /// <summary>Where the active child's age band begins on a path (F1-08); the first level when no profile is active.</summary>
        public static int StartIndex(GameServices s, IReadOnlyList<string> path)
        {
            if (!s.Profiles.HasActive)
            {
                return 0;
            }

            var bands = new List<IReadOnlyCollection<AgeBand>>();
            foreach (var id in path)
            {
                var set = new List<AgeBand>();
                var levels = s.Catalog.Find(id)?.Dto.Meta.AgeLevels;
                if (levels != null)
                {
                    foreach (var l in levels)
                    {
                        set.Add(l == AgeLevel.Minik ? AgeBand.Minik : l == AgeLevel.Kasif ? AgeBand.Kasif : AgeBand.Mucit);
                    }
                }

                bands.Add(set);
            }

            return PathRules.StartIndex(bands, s.Profiles.Active.AgeBand);
        }
    }
}
