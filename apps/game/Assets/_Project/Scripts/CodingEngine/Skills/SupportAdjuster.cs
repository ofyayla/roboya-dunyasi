namespace Roboya.CodingEngine.Skills
{
    /// <summary>How much help the next level starts with.</summary>
    public readonly struct Support
    {
        public Support(bool ghostPath, bool guided, bool suggestHarder)
        {
            GhostPath = ghostPath;
            Guided = guided;
            SuggestHarder = suggestHarder;
        }

        /// <summary>Show the faint route (YON-03).</summary>
        public bool GhostPath { get; }

        /// <summary>Roboya points at the next card.</summary>
        public bool Guided { get; }

        /// <summary>The child is ready for a harder level (YZ-02); used when level variants exist.</summary>
        public bool SuggestHarder { get; }
    }

    /// <summary>
    /// Difficulty adjuster v1 (PRD "Zorluk ayarlayıcı", rule based on the skill model). YZ-02: once a concept has
    /// three three-star levels in a row, the next level of that concept starts with less support. A level that
    /// introduces a new card keeps all its support: a new card is a new idea. The authored options are the ceiling;
    /// the adjuster only ever removes help, never adds it.
    /// </summary>
    public static class SupportAdjuster
    {
        public const int StreakForLessSupport = 3;

        public const double MasteryForHarder = 0.8;

        public static Support For(LevelFacts level, bool authoredGhostPath, bool authoredGuided, SkillModel model)
        {
            if (level.IntroducesCard || model == null)
            {
                return new Support(authoredGhostPath, authoredGuided, false);
            }

            bool ready = false;
            bool harder = false;
            foreach (var concept in level.Concepts)
            {
                if (model.ThreeStarStreak(concept) >= StreakForLessSupport)
                {
                    ready = true;
                    harder |= model.Mastery(concept) >= MasteryForHarder;
                }
            }

            return ready
                ? new Support(ghostPath: false, guided: false, suggestHarder: harder)
                : new Support(authoredGhostPath, authoredGuided, false);
        }
    }
}
