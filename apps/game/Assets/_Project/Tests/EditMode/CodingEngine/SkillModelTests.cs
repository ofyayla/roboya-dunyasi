using System.Collections.Generic;
using NUnit.Framework;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Progress;
using Roboya.CodingEngine.Skills;

namespace Roboya.Tests.CodingEngine
{
    public class SkillModelTests
    {
        private static LevelFacts L(string id, Concept concept = Concept.Sequencing, bool introduces = false) =>
            new LevelFacts(id, new[] { concept }, 1, introduces, null);

        private static ProgressBook Book(params (string, int)[] results)
        {
            var book = new ProgressBook();
            foreach (var (id, stars) in results)
            {
                book.Record(id, stars);
            }

            return book;
        }

        [Test]
        public void Mastery_NothingFinished_IsPrior()
        {
            var model = SkillModel.Build(new[] { L("a") }, new ProgressBook());

            Assert.AreEqual(SkillModel.Prior, model.Mastery(Concept.Sequencing), 1e-9);
            Assert.AreEqual(0, model.FinishedLevels(Concept.Sequencing));
            Assert.AreEqual(0, model.ThreeStarStreak(Concept.Sequencing));
        }

        [Test]
        public void Mastery_ThreeStarResults_RisesTowardsOne()
        {
            var path = new[] { L("a"), L("b"), L("c"), L("d") };
            var model = SkillModel.Build(path, Book(("a", 3), ("b", 3), ("c", 3), ("d", 3)));

            Assert.Greater(model.Mastery(Concept.Sequencing), 0.8);
            Assert.AreEqual(4, model.ThreeStarStreak(Concept.Sequencing));
        }

        [Test]
        public void Mastery_OneStarResults_FallsBelowPrior()
        {
            var model = SkillModel.Build(new[] { L("a"), L("b") }, Book(("a", 1), ("b", 1)));

            Assert.Less(model.Mastery(Concept.Sequencing), SkillModel.Prior);
            Assert.AreEqual(0, model.ThreeStarStreak(Concept.Sequencing));
        }

        [Test]
        public void Streak_BrokenByLowerStars_RestartsCounting()
        {
            var path = new[] { L("a"), L("b"), L("c"), L("d") };
            var model = SkillModel.Build(path, Book(("a", 3), ("b", 2), ("c", 3), ("d", 3)));

            Assert.AreEqual(2, model.ThreeStarStreak(Concept.Sequencing));
        }

        [Test]
        public void Build_UnfinishedLevelsAndOtherConcepts_AreSkipped()
        {
            var path = new[] { L("a"), L("b", Concept.Direction), L("c") };
            var model = SkillModel.Build(path, Book(("a", 3), ("c", 3)));

            Assert.AreEqual(2, model.ThreeStarStreak(Concept.Sequencing));
            Assert.AreEqual(0, model.FinishedLevels(Concept.Direction));
        }

        [Test]
        public void LevelFacts_From_ReadsMetaCardsAndAlternative()
        {
            var dto = new LevelDto
            {
                Id = "x",
                Meta = new LevelMeta { Concepts = new List<Concept> { Concept.Loops }, Difficulty = 4 },
                Cards = new CardsDto { Introduces = CardId.Repeat },
                AlternativeLevelId = "easier",
            };

            var facts = LevelFacts.From(dto);

            Assert.AreEqual(4, facts.Difficulty);
            Assert.IsTrue(facts.IntroducesCard);
            Assert.AreEqual("easier", facts.AlternativeLevelId);
            Assert.Throws<System.ArgumentNullException>(() => LevelFacts.From(null));
        }
    }

    public class SupportAdjusterTests
    {
        private static LevelFacts Level(bool introduces = false) =>
            new LevelFacts("next", new[] { Concept.Sequencing }, 1, introduces, null);

        private static SkillModel ModelWithStreak(int threeStars)
        {
            var path = new List<LevelFacts>();
            var book = new ProgressBook();
            for (int i = 0; i < threeStars; i++)
            {
                path.Add(new LevelFacts("p" + i, new[] { Concept.Sequencing }, 1, false, null));
                book.Record("p" + i, 3);
            }

            return SkillModel.Build(path, book);
        }

        [Test]
        public void For_TwoThreeStarLevels_KeepsAuthoredSupport()
        {
            var support = SupportAdjuster.For(Level(), true, true, ModelWithStreak(2));

            Assert.IsTrue(support.GhostPath);
            Assert.IsTrue(support.Guided);
            Assert.IsFalse(support.SuggestHarder);
        }

        [Test]
        public void For_ThreeThreeStarLevels_RemovesSupport()
        {
            var support = SupportAdjuster.For(Level(), true, true, ModelWithStreak(3));

            Assert.IsFalse(support.GhostPath);
            Assert.IsFalse(support.Guided);
        }

        [Test]
        public void For_LongStreak_AlsoSuggestsHarder()
        {
            Assert.IsTrue(SupportAdjuster.For(Level(), false, false, ModelWithStreak(6)).SuggestHarder);
        }

        [Test]
        public void For_LevelIntroducingACard_KeepsAllSupportEvenWithAStreak()
        {
            var support = SupportAdjuster.For(Level(introduces: true), true, true, ModelWithStreak(6));

            Assert.IsTrue(support.GhostPath);
            Assert.IsTrue(support.Guided);
            Assert.IsFalse(support.SuggestHarder);
        }

        [Test]
        public void For_NeverAddsSupportThatWasNotAuthored()
        {
            var support = SupportAdjuster.For(Level(), false, false, ModelWithStreak(0));

            Assert.IsFalse(support.GhostPath);
            Assert.IsFalse(support.Guided);
            Assert.IsFalse(SupportAdjuster.For(Level(), true, true, null).SuggestHarder);
        }
    }
}
