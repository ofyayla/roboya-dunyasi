using System.Collections.Generic;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Progress;
using Roboya.CodingEngine.Skills;

namespace Roboya.Core
{
    /// <summary>
    /// Decides how much help a level starts with (YZ-02) from the child's finished levels: the authored help is the
    /// ceiling and the skill model only removes it. Runs on the device; nothing leaves it.
    /// </summary>
    public static class SupportPlanner
    {
        public static Support For(LevelCatalog catalog, LevelEntry entry, ProgressBook book)
        {
            var path = new List<LevelFacts>();
            foreach (var e in catalog.All)
            {
                if (e.Dto.Region == entry.Dto.Region)
                {
                    path.Add(LevelFacts.From(e.Dto));
                }
            }

            var model = SkillModel.Build(path, book);
            return SupportAdjuster.For(
                LevelFacts.From(entry.Dto),
                entry.Dto.Options?.GhostPath ?? false,
                entry.Dto.Options?.Guided ?? false,
                model);
        }

        /// <summary>
        /// The easier level to offer after repeated failures (YZ-03), or null when the level has none or it is not
        /// playable yet. A child is never stuck: failures also unlock hints, and the alternative is one more way out.
        /// </summary>
        public static LevelEntry AlternativeFor(LevelCatalog catalog, LevelEntry entry, System.Func<string, bool> canPlay)
        {
            string id = entry.Dto.AlternativeLevelId;
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            var alt = catalog.Find(id);
            return alt != null && canPlay(alt.Id) ? alt : null;
        }
    }
}
