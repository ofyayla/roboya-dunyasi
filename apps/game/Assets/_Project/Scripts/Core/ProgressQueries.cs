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
        /// <summary>Levels per "forest corner": the path map marks the end of each stretch and the game closes it with a value moment.</summary>
        public const int CornerLength = 9;

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

        /// <summary>Wire id of a region, as used in content files and content/map/island.json.</summary>
        public static string WireId(RegionId region)
        {
            switch (region)
            {
                case RegionId.SabirOrmani: return "sabir-ormani";
                case RegionId.PaylasimKoyu: return "paylasim-koyu";
                case RegionId.YardimlasmaLimani: return "yardimlasma-limani";
                case RegionId.NezaketBahcesi: return "nezaket-bahcesi";
                case RegionId.SorumlulukKulesi: return "sorumluluk-kulesi";
                default: return "mucit-atolyesi";
            }
        }

        /// <summary>
        /// Wire ids of the regions the child can enter. The unlock order is the order of content/map/island.json,
        /// so the order lives in content, not code; the rule itself is <see cref="RegionRules"/>.
        /// </summary>
        public static HashSet<string> OpenRegions(GameServices s)
        {
            var regions = s.Island.Regions;
            var paths = new List<IReadOnlyList<string>>(regions.Count);
            foreach (var region in regions)
            {
                var path = new List<string>();
                foreach (var e in s.Catalog.All)
                {
                    if (WireId(e.Dto.Region) == region.Id)
                    {
                        path.Add(e.Id);
                    }
                }

                paths.Add(path);
            }

            var open = RegionRules.OpenRegions(paths, s.Progress.Book, s.ProgressRules.UnlockAll);
            var ids = new HashSet<string>();
            for (int i = 0; i < regions.Count; i++)
            {
                if (open[i])
                {
                    ids.Add(regions[i].Id);
                }
            }

            return ids;
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
