using System.Collections.Generic;
using NUnit.Framework;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Progress;
using Roboya.CodingEngine.Skills;

namespace Roboya.Tests.CodingEngine
{
    public class ProgressReportTests
    {
        private static LevelFacts Level(string id, params Concept[] concepts) =>
            new LevelFacts(id, concepts, 1, false, null);

        private static List<LevelFacts> Path() => new List<LevelFacts>
        {
            Level("l1", Concept.Direction),
            Level("l2", Concept.Direction),
            Level("l3", Concept.Direction, Concept.Sequencing),
            Level("l4", Concept.Sequencing),
        };

        [Test]
        public void Build_NothingPlayed_AllConceptsNotStarted()
        {
            var r = ProgressReport.Build(Path(), new ProgressBook());

            Assert.AreEqual(0, r.CompletedLevels);
            Assert.AreEqual(4, r.TotalLevels);
            Assert.AreEqual(0, r.Stars);
            Assert.AreEqual(2, r.Concepts.Count, "only concepts present in the levels");
            Assert.AreEqual(SkillLevel.NotStarted, r.Concepts[0].Level);
        }

        [Test]
        public void Build_SumsStarsAndCompletedLevels()
        {
            var book = new ProgressBook();
            book.Record("l1", 3);
            book.Record("l2", 1);

            var r = ProgressReport.Build(Path(), book);

            Assert.AreEqual(2, r.CompletedLevels);
            Assert.AreEqual(4, r.Stars);
        }

        [Test]
        public void Build_ThreeStarsOnThreeLevels_ConceptIsConfident()
        {
            var book = new ProgressBook();
            book.Record("l1", 3);
            book.Record("l2", 3);
            book.Record("l3", 3);

            var r = ProgressReport.Build(Path(), book);

            Assert.AreEqual(Concept.Direction, r.Concepts[0].Concept);
            Assert.AreEqual(SkillLevel.Confident, r.Concepts[0].Level);
            Assert.AreEqual(3, r.Concepts[0].FinishedLevels);
            Assert.AreEqual(3, r.Concepts[0].TotalLevels);
            Assert.AreNotEqual(SkillLevel.Confident, r.Concepts[1].Level);
        }

        [Test]
        public void Build_OneStarEach_StaysExploring()
        {
            var book = new ProgressBook();
            book.Record("l1", 1);
            book.Record("l2", 1);

            var r = ProgressReport.Build(Path(), book);

            Assert.AreEqual(SkillLevel.Exploring, r.Concepts[0].Level);
        }

        [Test]
        public void Build_MixedStars_IsGrowing()
        {
            var book = new ProgressBook();
            book.Record("l1", 3);
            book.Record("l2", 2);

            var r = ProgressReport.Build(Path(), book);

            Assert.AreEqual(SkillLevel.Growing, r.Concepts[0].Level);
        }

        [Test]
        public void Build_StarsOfLevelsOutsideThePath_AreIgnored()
        {
            var book = new ProgressBook();
            book.Record("other", 3);

            var r = ProgressReport.Build(Path(), book);

            Assert.AreEqual(0, r.Stars);
            Assert.AreEqual(0, r.CompletedLevels);
        }
    }
}
