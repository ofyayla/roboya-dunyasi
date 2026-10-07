using System;
using System.Linq;
using NUnit.Framework;
using Roboya.CodingEngine.Execution;
using Roboya.CodingEngine.Play;
using Roboya.CodingEngine.Scoring;
using static Roboya.Tests.CodingEngine.TestLevels;

namespace Roboya.Tests.CodingEngine
{
    public class LevelSessionTests
    {
        private static readonly string[] Corner = { "..G", "...", "R.." };

        private static LevelSession Session(int max = 7) => new LevelSession(Build(Corner, maxLength: max), 5);

        private static ExecutionResult PlayAll(LevelSession s) => s.Play().Last().Result;

        [Test]
        public void CanPlay_EmptyPlan_IsFalse()
        {
            var s = Session();

            Assert.IsFalse(s.CanPlay);
            Assert.Throws<InvalidOperationException>(() => s.Play());
        }

        [Test]
        public void Play_CorrectShortestPlanFirstTry_ThreeStars()
        {
            var s = Session();
            s.Plan.Load(new[] { F, F, Rt, F, F });

            var result = PlayAll(s);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(SessionState.Completed, s.State);
            Assert.AreEqual(3, s.Stars);
            Assert.AreEqual(1, s.Attempts);
            Assert.AreSame(result, s.LastResult);
            Assert.IsFalse(s.CanPlay);
        }

        [Test]
        public void Play_Failure_ReturnsToPlanningAndCounts()
        {
            var s = Session();
            s.Plan.Load(new[] { F, F, F });

            Assert.IsFalse(PlayAll(s).IsSuccess);

            Assert.AreEqual(SessionState.Planning, s.State);
            Assert.AreEqual(1, s.FailuresInARow);
            Assert.AreEqual(0, s.Stars);
            Assert.IsTrue(s.CanPlay);
        }

        [Test]
        public void Play_WhileRunning_StateIsRunningUntilFinished()
        {
            var s = Session();
            s.Plan.Load(new[] { F });

            using (var e = s.Play().GetEnumerator())
            {
                e.MoveNext();
                Assert.AreEqual(SessionState.Running, s.State);
                Assert.IsFalse(s.CanPlay);
                while (e.MoveNext())
                {
                }
            }

            Assert.AreEqual(SessionState.Planning, s.State);
        }

        [Test]
        public void ResetRun_AbandonedRun_ReturnsToPlanning()
        {
            var s = Session();
            s.Plan.Load(new[] { F });
            using (var e = s.Play().GetEnumerator())
            {
                e.MoveNext();
            }

            s.ResetRun();

            Assert.AreEqual(SessionState.Planning, s.State);
            Assert.AreEqual(0, s.Attempts);
        }

        [Test]
        public void Stars_LongerPlanAfterManyTries_OneStar()
        {
            var s = Session();
            s.Plan.Load(new[] { F });
            PlayAll(s);
            PlayAll(s);
            s.Plan.Load(new[] { Rt, F, F, L, F, F });

            PlayAll(s);

            Assert.AreEqual(1, s.Stars);
            Assert.AreEqual(0, s.FailuresInARow);
        }

        [Test]
        public void ShouldOfferHint_AfterThreeFailures_True()
        {
            var s = Session();
            s.Plan.Load(new[] { F });

            for (int i = 0; i < 2; i++)
            {
                PlayAll(s);
            }

            Assert.IsFalse(s.ShouldOfferHint);
            PlayAll(s);
            Assert.IsTrue(s.ShouldOfferHint);
            Assert.IsFalse(s.ShouldOfferAlternative);
            PlayAll(s);
            PlayAll(s);
            Assert.IsTrue(s.ShouldOfferAlternative);
        }

        [Test]
        public void RequestHint_EscalatesThroughTiers()
        {
            var s = Session();
            s.Plan.Load(new[] { F, F, F });

            var first = s.RequestHint();
            var second = s.RequestHint();
            var third = s.RequestHint();
            var fourth = s.RequestHint();

            Assert.AreEqual(HintTier.Voice, first.Tier);
            Assert.AreEqual(-1, first.SlotIndex);
            Assert.AreEqual(HintTier.HighlightWrongCard, second.Tier);
            Assert.AreEqual(2, second.SlotIndex);
            Assert.IsNull(second.Card);
            Assert.AreEqual(HintTier.ShowCorrectCard, third.Tier);
            Assert.AreEqual(Rt, third.Card);
            Assert.AreEqual(HintTier.ShowCorrectCard, fourth.Tier);
            Assert.AreEqual(HintTier.ShowCorrectCard, s.HintTier);
        }

        [Test]
        public void RequestHint_CorrectPlan_FallsBackToVoice()
        {
            var s = Session();
            s.Plan.Load(new[] { F, F, Rt, F, F });
            s.RequestHint();

            Assert.AreEqual(HintTier.Voice, s.RequestHint().Tier);
        }

        [Test]
        public void RequestHint_AfterCompletion_ReturnsNone()
        {
            var s = Session();
            s.Plan.Load(new[] { F, F, Rt, F, F });
            PlayAll(s);

            Assert.AreEqual(HintTier.None, s.RequestHint().Tier);
            Assert.IsFalse(s.ShouldOfferHint);
        }

        [Test]
        public void Constructor_NullLevel_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new LevelSession(null, 3));
        }

        [TestCase(false, 1, 3, 3, 0)]
        [TestCase(true, 1, 3, 3, 3)]
        [TestCase(true, 5, 3, 3, 2)]
        [TestCase(true, 1, 4, 3, 2)]
        [TestCase(true, 5, 4, 3, 1)]
        [TestCase(true, 1, 4, -1, 2)]
        public void StarRating_Compute_FollowsRules(bool solved, int attempts, int cards, int shortest, int expected)
        {
            Assert.AreEqual(expected, StarRating.Compute(solved, attempts, cards, shortest, 2));
        }

        [Test]
        public void StarRating_Best_NeverLowers()
        {
            Assert.AreEqual(3, StarRating.Best(3, 1));
            Assert.AreEqual(2, StarRating.Best(1, 2));
        }
    }
}
