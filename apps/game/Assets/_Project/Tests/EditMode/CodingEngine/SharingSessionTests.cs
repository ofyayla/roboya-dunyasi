using System;
using NUnit.Framework;
using Roboya.CodingEngine.Play;
using Roboya.CodingEngine.Sharing;
using static Roboya.Tests.CodingEngine.SharingLevels;

namespace Roboya.Tests.CodingEngine
{
    public class SharingPlanTests
    {
        [Test]
        public void Add_WithoutABox_GoesToTheTailUntilFull()
        {
            var plan = new SharingPlan(Make(new[] { 1, 1 }, basket: 2, maxCards: 2));

            Assert.IsTrue(plan.Add(G0));
            Assert.IsTrue(plan.Add(G1));
            Assert.IsTrue(plan.IsFull);
            Assert.IsFalse(plan.Add(G0));
            Assert.AreEqual(2, plan.Count);
            Assert.AreEqual(0, plan.Box.Count);
        }

        [Test]
        public void Add_Null_IsRejected() => Assert.IsFalse(new SharingPlan(OneEach()).Add(null));

        [Test]
        public void Add_WithABox_HiveCardsGoInsideAndTheTableAfter()
        {
            var plan = new SharingPlan(ShareThreeWithTable());

            Assert.IsTrue(plan.Add(G0));
            Assert.IsTrue(plan.Add(GT));
            Assert.AreEqual(1, plan.Box.Count);
            Assert.AreEqual(1, plan.Tail.Count);
        }

        [Test]
        public void Add_FullBox_RejectsAnotherHiveCard()
        {
            var plan = new SharingPlan(Make(new[] { 0, 0 }, basket: 4, goal: SharingGoalKind.Share, maxCards: 4, box: new RepeatBoxSpec(2, 3)));
            plan.Add(G0);
            plan.Add(G1);

            Assert.IsTrue(plan.BoxIsFull);
            Assert.IsFalse(plan.Add(G0));
            Assert.AreEqual(2, plan.Count);
        }

        [Test]
        public void Remove_ByIndex_AndOutOfRange()
        {
            var plan = new SharingPlan(OneEach());
            plan.Add(G0);
            plan.Add(G1);

            Assert.IsFalse(plan.RemoveTailAt(5));
            Assert.IsFalse(plan.RemoveBoxAt(0));
            Assert.IsTrue(plan.RemoveTailAt(0));
            Assert.AreEqual(G1, plan.Tail[0]);
        }

        [Test]
        public void RemoveBoxAt_RemovesFromTheBox()
        {
            var plan = new SharingPlan(ShareThreeWithBox());
            plan.Add(G0);

            Assert.IsTrue(plan.RemoveBoxAt(0));
            Assert.IsTrue(plan.IsEmpty);
        }

        [Test]
        public void SetCount_OnlyWhereCardsCarryACount()
        {
            var counted = new SharingPlan(Make(new[] { 3 }, basket: 3, amounts: true));
            counted.Add(SharingCard.Give(0));
            var plain = new SharingPlan(Make(new[] { 3 }, basket: 3));
            plain.Add(SharingCard.Give(0));

            Assert.IsTrue(counted.SetCount(false, 0, 3));
            Assert.AreEqual(SharingCard.Give(0, 3), counted.Tail[0]);
            Assert.IsFalse(counted.SetCount(false, 0, 4), "above the most one card hands over");
            Assert.IsFalse(counted.SetCount(false, 0, 0));
            Assert.IsFalse(counted.SetCount(false, 5, 2));
            Assert.IsFalse(plain.SetCount(false, 0, 2));
        }

        [Test]
        public void SetCount_ATakeCard_IsRejected()
        {
            var plan = new SharingPlan(BasketOfTwo());
            plan.Add(T0);

            Assert.IsFalse(plan.SetCount(false, 0, 2));
        }

        [Test]
        public void SetCount_InsideTheBox()
        {
            var plan = new SharingPlan(Make(new[] { 0, 0 }, basket: 6, goal: SharingGoalKind.Share, amounts: true, maxCards: 3, box: new RepeatBoxSpec(2, 3)));
            plan.Add(G0);

            Assert.IsTrue(plan.SetCount(true, 0, 2));
            Assert.AreEqual(2, plan.Box[0].Count);
        }

        [Test]
        public void SetTurns_OnlyWithABoxAndWithinItsLimit()
        {
            var boxed = new SharingPlan(ShareThreeWithBox());
            var plain = new SharingPlan(OneEach());

            Assert.IsTrue(boxed.SetTurns(2));
            Assert.AreEqual(2, boxed.Turns);
            Assert.IsFalse(boxed.SetTurns(4));
            Assert.IsFalse(boxed.SetTurns(0));
            Assert.IsFalse(plain.SetTurns(2));
        }

        [Test]
        public void Clear_EmptiesTheStripAndResetsTheTurns()
        {
            var plan = new SharingPlan(ShareThreeWithTable());
            plan.Add(G0);
            plan.Add(GT);
            plan.SetTurns(3);

            plan.Clear();

            Assert.IsTrue(plan.IsEmpty);
            Assert.AreEqual(1, plan.Turns);
        }

        [Test]
        public void ToProgram_CopiesThePlan()
        {
            var plan = new SharingPlan(ShareThreeWithTable());
            plan.Add(G0);
            plan.Add(GT);
            plan.SetTurns(2);

            var program = plan.ToProgram();
            plan.Clear();

            Assert.AreEqual(2, program.Turns);
            Assert.AreEqual(1, program.Box.Count);
            Assert.AreEqual(1, program.Tail.Count);
        }

        [Test]
        public void BoxIsFull_WithoutABox_IsFalse() => Assert.IsFalse(new SharingPlan(OneEach()).BoxIsFull);
    }

    public class SharingSessionTests
    {
        private static SharingSession Session(SharingLevel level, int shortest = 2) => new SharingSession(level, shortest);

        [Test]
        public void CanPlay_NeedsAPlan()
        {
            var session = Session(OneEach());

            Assert.IsFalse(session.CanPlay);
            session.Plan.Add(G0);
            Assert.IsTrue(session.CanPlay);
        }

        [Test]
        public void Play_Empty_Throws() => Assert.Throws<InvalidOperationException>(() => Session(OneEach()).Play());

        [Test]
        public void Play_WinningPlan_CompletesWithAllThreeStars()
        {
            var session = Session(OneEach());
            session.Plan.Add(G0);
            session.Plan.Add(G1);

            var result = session.Play();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(SessionState.Completed, session.State);
            Assert.AreEqual(1, session.Attempts);
            Assert.AreEqual(3, session.Stars);
            Assert.AreSame(result, session.LastResult);
            Assert.IsFalse(session.CanPlay);
            Assert.Throws<InvalidOperationException>(() => session.Play());
        }

        [Test]
        public void Play_FailedRun_KeepsThePlanAndCountsTheFailure()
        {
            var session = Session(OneEach());
            session.Plan.Add(G0);

            session.Play();

            Assert.AreEqual(SessionState.Planning, session.State);
            Assert.AreEqual(1, session.FailuresInARow);
            Assert.AreEqual(1, session.Plan.Count);
            Assert.AreEqual(0, session.Stars);
            Assert.IsTrue(session.CanPlay);
        }

        [Test]
        public void ShouldOfferHint_AfterThreeFailures_And_Alternative_AfterFive()
        {
            var session = Session(OneEach());
            session.Plan.Add(G0);

            for (int i = 0; i < 2; i++)
            {
                session.Play();
            }

            Assert.IsFalse(session.ShouldOfferHint);
            session.Play();
            Assert.IsTrue(session.ShouldOfferHint);
            Assert.IsFalse(session.ShouldOfferAlternative);
            session.Play();
            session.Play();
            Assert.IsTrue(session.ShouldOfferAlternative);
        }

        [Test]
        public void Play_WinAfterFailures_ResetsTheStreakAndGivesFewerStars()
        {
            var session = Session(OneEach());
            session.Plan.Add(G0);
            session.Play();
            session.Play();
            session.Play();
            session.Plan.Add(G1);

            session.Play();

            Assert.AreEqual(0, session.FailuresInARow);
            Assert.AreEqual(2, session.Stars, "one star for finishing and one for the card count, none for the number of runs");
        }

        [Test]
        public void RequestHint_ReturnsTheNextStepAndCountsIt()
        {
            var session = Session(OneEach());

            var hint = session.RequestHint();

            Assert.AreEqual(SharingHintKind.Add, hint.Kind);
            Assert.AreEqual(1, session.HintsUsed);
        }

        [Test]
        public void New_NullLevel_Throws() => Assert.Throws<ArgumentNullException>(() => new SharingSession(null, 2));

        [Test]
        public void Play_RepeatPlan_RunsTheBox()
        {
            var session = Session(ShareThreeWithBox(), 3);
            session.Plan.Add(G0);
            session.Plan.Add(G1);
            session.Plan.Add(G2);
            session.Plan.SetTurns(2);

            Assert.IsTrue(session.Play().IsSuccess);
            Assert.AreEqual(3, session.Stars);
        }
    }
}
