using System.Collections.Generic;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Progress;

namespace Roboya.CodingEngine.Skills
{
    /// <summary>How far a child has come with one concept; deliberately gentle wording, never a grade (YZ-04).</summary>
    public enum SkillLevel
    {
        NotStarted,
        Exploring,
        Growing,
        Confident,
    }

    public sealed class ConceptProgress
    {
        public ConceptProgress(Concept concept, SkillLevel level, int finishedLevels, int totalLevels)
        {
            Concept = concept;
            Level = level;
            FinishedLevels = finishedLevels;
            TotalLevels = totalLevels;
        }

        public Concept Concept { get; }

        public SkillLevel Level { get; }

        public int FinishedLevels { get; }

        public int TotalLevels { get; }
    }

    /// <summary>
    /// The parent's progress summary and skill report (F1-12, PRD "Beceri raporu"). Everything is derived from the
    /// stars already stored on the device, so it needs no new data. It describes play, it never diagnoses (YZ-04).
    /// </summary>
    public sealed class ProgressReport
    {
        /// <summary>Mastery needed for "Confident"; with at least <see cref="ConfidentLevels"/> finished levels.</summary>
        public const double ConfidentMastery = 0.8;

        public const int ConfidentLevels = 3;

        public const double GrowingMastery = 0.6;

        private ProgressReport(int completed, int total, int stars, List<ConceptProgress> concepts)
        {
            CompletedLevels = completed;
            TotalLevels = total;
            Stars = stars;
            Concepts = concepts;
        }

        public int CompletedLevels { get; }

        public int TotalLevels { get; }

        public int Stars { get; }

        /// <summary>Only concepts that appear in the available levels, in enum order.</summary>
        public IReadOnlyList<ConceptProgress> Concepts { get; }

        public static ProgressReport Build(IReadOnlyList<LevelFacts> levels, ProgressBook book)
        {
            var model = SkillModel.Build(levels, book);
            int completed = 0;
            int stars = 0;
            var totals = new Dictionary<Concept, int>();
            foreach (var level in levels)
            {
                int s = book.Stars(level.Id);
                stars += s;
                if (s > 0)
                {
                    completed++;
                }

                foreach (var concept in level.Concepts)
                {
                    totals[concept] = (totals.TryGetValue(concept, out var t) ? t : 0) + 1;
                }
            }

            var concepts = new List<ConceptProgress>();
            foreach (Concept concept in System.Enum.GetValues(typeof(Concept)))
            {
                if (!totals.TryGetValue(concept, out var total))
                {
                    continue;
                }

                int finished = model.FinishedLevels(concept);
                concepts.Add(new ConceptProgress(concept, LevelOf(model, concept, finished), finished, total));
            }

            return new ProgressReport(completed, levels.Count, stars, concepts);
        }

        private static SkillLevel LevelOf(SkillModel model, Concept concept, int finished)
        {
            if (finished == 0)
            {
                return SkillLevel.NotStarted;
            }

            double mastery = model.Mastery(concept);
            if (finished >= ConfidentLevels && mastery >= ConfidentMastery)
            {
                return SkillLevel.Confident;
            }

            return mastery >= GrowingMastery ? SkillLevel.Growing : SkillLevel.Exploring;
        }
    }
}
