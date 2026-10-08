using System;
using System.Collections.Generic;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Progress;

namespace Roboya.CodingEngine.Skills
{
    /// <summary>What the skill model needs to know about a level (from its meta block).</summary>
    public sealed class LevelFacts
    {
        public LevelFacts(string id, IReadOnlyList<Concept> concepts, int difficulty, bool introducesCard, string alternativeLevelId)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Concepts = concepts ?? Array.Empty<Concept>();
            Difficulty = difficulty;
            IntroducesCard = introducesCard;
            AlternativeLevelId = alternativeLevelId;
        }

        public string Id { get; }

        public IReadOnlyList<Concept> Concepts { get; }

        public int Difficulty { get; }

        /// <summary>The level teaches a new card; that moment always keeps full support.</summary>
        public bool IntroducesCard { get; }

        /// <summary>An easier level offered after repeated failures (YZ-03), or null.</summary>
        public string AlternativeLevelId { get; }

        public static LevelFacts From(LevelDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            return new LevelFacts(
                dto.Id,
                dto.Meta?.Concepts ?? new List<Concept>(),
                dto.Meta != null ? (int)dto.Meta.Difficulty : 0,
                dto.Cards?.Introduces != null,
                dto.AlternativeLevelId);
        }
    }

    /// <summary>
    /// Skill model v1 (PRD "Beceri modeli", P0 simple): an Elo-like mastery per concept plus the current run of
    /// three-star levels. It is derived from the stars already in <see cref="ProgressBook"/>, so it needs no extra
    /// storage and no personal data, and it never goes down because a child replayed a level (stars only rise).
    /// </summary>
    public sealed class SkillModel
    {
        /// <summary>Starting estimate before any level of a concept has been finished.</summary>
        public const double Prior = 0.5;

        private readonly Dictionary<Concept, double> _mastery = new Dictionary<Concept, double>();
        private readonly Dictionary<Concept, int> _streak = new Dictionary<Concept, int>();
        private readonly Dictionary<Concept, int> _finished = new Dictionary<Concept, int>();

        private SkillModel()
        {
        }

        /// <param name="path">Levels in play order.</param>
        /// <param name="learningRate">How strongly the latest level moves the estimate (0..1).</param>
        public static SkillModel Build(IEnumerable<LevelFacts> path, ProgressBook book, double learningRate = 0.35)
        {
            var model = new SkillModel();
            foreach (var level in path)
            {
                int stars = book.Stars(level.Id);
                if (stars <= 0)
                {
                    continue;
                }

                foreach (var concept in level.Concepts)
                {
                    double current = model._mastery.TryGetValue(concept, out var m) ? m : Prior;
                    model._mastery[concept] = current + (learningRate * ((stars / 3.0) - current));
                    model._streak[concept] = stars >= 3 ? (model._streak.TryGetValue(concept, out var s) ? s : 0) + 1 : 0;
                    model._finished[concept] = (model._finished.TryGetValue(concept, out var f) ? f : 0) + 1;
                }
            }

            return model;
        }

        /// <summary>Mastery estimate in 0..1; <see cref="Prior"/> when nothing was finished.</summary>
        public double Mastery(Concept concept) => _mastery.TryGetValue(concept, out var m) ? m : Prior;

        /// <summary>Finished levels in a row with three stars, counted from the latest backwards (YZ-02).</summary>
        public int ThreeStarStreak(Concept concept) => _streak.TryGetValue(concept, out var s) ? s : 0;

        public int FinishedLevels(Concept concept) => _finished.TryGetValue(concept, out var f) ? f : 0;
    }
}
