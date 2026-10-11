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

        /// <summary>The region with this wire id, or null for an unknown id.</summary>
        public static RegionId? RegionOf(string wireId)
        {
            foreach (RegionId region in System.Enum.GetValues(typeof(RegionId)))
            {
                if (WireId(region) == wireId)
                {
                    return region;
                }
            }

            return null;
        }

        /// <summary>
        /// Each region's path in island order (content/map/island.json), empty for a region without levels. The island
        /// order is the order regions open in and the order their ship parts are earned in.
        /// </summary>
        public static List<IReadOnlyList<string>> PathsInIslandOrder(GameServices s)
        {
            var paths = new List<IReadOnlyList<string>>(s.Island.Regions.Count);
            foreach (var region in s.Island.Regions)
            {
                var id = RegionOf(region.Id);
                paths.Add(id.HasValue ? PathOf(s.Catalog, id.Value) : new List<string>());
            }

            return paths;
        }

        /// <summary>
        /// Ship parts that belong to the regions before <paramref name="region"/> in island order: the catalog order of
        /// this region's first part is one more (RewardRules).
        /// </summary>
        public static int PartsBefore(GameServices s, RegionId region)
        {
            string wire = WireId(region);
            var regions = s.Island.Regions;
            var paths = PathsInIslandOrder(s);
            int count = 0;
            for (int i = 0; i < regions.Count && regions[i].Id != wire; i++)
            {
                count += CornerRules.CornerCount(paths[i].Count);
            }

            return count;
        }

        /// <summary>
        /// Wire ids of the regions the child can enter. The unlock order is the order of content/map/island.json,
        /// so the order lives in content, not code; the rule itself is <see cref="RegionRules"/>.
        /// </summary>
        public static HashSet<string> OpenRegions(GameServices s)
        {
            var regions = s.Island.Regions;
            var paths = PathsInIslandOrder(s);
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

        public static int EarnedParts(GameServices s) => RewardRules.EarnedCount(PathsInIslandOrder(s), s.Progress.Book);

        public static NodeState[] PathStates(GameServices s, IReadOnlyList<string> path) =>
            PathRules.StatesOf(path, s.Progress.Book, s.ProgressRules.FreeLevelCount, s.Entitlements.HasPremium, StartIndex(s, path), s.ProgressRules.UnlockAll, IntroLevels(s.Catalog, path));

        /// <summary>A game's first level, every level that introduces a card and every level marked <c>options.intro</c>: the tutorials a late starter must not skip.</summary>
        public static List<bool> IntroLevels(LevelCatalog catalog, IReadOnlyList<string> path)
        {
            var seen = new HashSet<GameId>();
            var flags = new List<bool>(path.Count);
            foreach (var id in path)
            {
                var dto = catalog.Find(id)?.Dto;
                bool first = dto != null && seen.Add(dto.Game);
                flags.Add(first || (dto?.Cards?.Introduces != null) || dto?.Options?.Intro == true);
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
