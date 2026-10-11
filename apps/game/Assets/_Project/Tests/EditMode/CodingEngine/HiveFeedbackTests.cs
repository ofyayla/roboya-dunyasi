using NUnit.Framework;
using Roboya.CodingEngine.Play;
using Roboya.CodingEngine.World;
using static Roboya.Tests.CodingEngine.HoneyLevels;

namespace Roboya.Tests.CodingEngine
{
    public class HiveFeedbackTests
    {
        // A and B each need 2, the table is T; 5 honey to share.
        private static Level Share() => Build(new[] { "R5ABT" }, needs: new[] { 2, 2 }, shareAmong: "AB", leftover: 'T');

        private static RobotState State(Level level, int a = 0, int b = 0, int t = 0, int basket = 0) =>
            level.Start.WithBasket(basket).WithDelivered(0, a).WithDelivered(1, b).WithDelivered(2, t);

        [Test]
        public void Of_WonWithHoneyOnTheTable_AllHappyAndSharedWithTheTable()
        {
            var level = Share();

            var f = HiveFeedback.Of(level, State(level, 2, 2, 1), success: true);

            Assert.AreEqual(HiveVerdict.SharedWithTable, f.Verdict);
            CollectionAssert.AreEqual(new[] { HiveMood.Happy, HiveMood.Happy }, f.Moods);
            CollectionAssert.AreEqual(new[] { 0, 1 }, f.Hives);
        }

        [Test]
        public void Of_WonWithoutAnythingOnTheTable_HasNothingToSay()
        {
            var level = Build(new[] { "R4AB" }, needs: new[] { 2, 2 }, shareAmong: "AB");

            Assert.AreEqual(HiveVerdict.None, HiveFeedback.Of(level, State(level, 2, 2), success: true).Verdict);
        }

        [Test]
        public void Of_OneHiveGotNothing_ItIsTheForgottenOne()
        {
            var level = Share();

            var f = HiveFeedback.Of(level, State(level, 2, 0, basket: 3), success: false);

            Assert.AreEqual(HiveVerdict.Forgotten, f.Verdict);
            CollectionAssert.AreEqual(new[] { HiveMood.Happy, HiveMood.Sad }, f.Moods);
        }

        [Test]
        public void Of_HivesHoldDifferentAmounts_NotEqualAndTheSmallerIsSad()
        {
            var level = Share();

            var f = HiveFeedback.Of(level, State(level, 2, 1, basket: 2), success: false);

            Assert.AreEqual(HiveVerdict.NotEqual, f.Verdict);
            CollectionAssert.AreEqual(new[] { HiveMood.Happy, HiveMood.Sad }, f.Moods);
        }

        [Test]
        public void Of_ShareEqualButHoneyStillInTheBasket_IsNotYetAndEveryoneIsHappy()
        {
            var level = Share();

            var f = HiveFeedback.Of(level, State(level, 1, 1, basket: 3), success: false);

            Assert.AreEqual(HiveVerdict.NotYet, f.Verdict);
            CollectionAssert.AreEqual(new[] { HiveMood.Happy, HiveMood.Happy }, f.Moods);
        }

        [Test]
        public void Of_NothingDeliveredYet_IsNotYetNotForgotten()
        {
            var level = Share();

            Assert.AreEqual(HiveVerdict.NotYet, HiveFeedback.Of(level, State(level, basket: 5), success: false).Verdict);
        }

        [Test]
        public void Of_DeliverGoalWithTooMuchOnAHive_ItSpills()
        {
            var level = Build(new[] { "R5AB" }, needs: new[] { 3, 3 }, deliver: new[] { ('A', 2), ('B', 2) });

            var f = HiveFeedback.Of(level, State(level, 3, 1, basket: 1), success: false);

            Assert.AreEqual(HiveVerdict.TooMuch, f.Verdict);
            CollectionAssert.AreEqual(new[] { HiveMood.Spilling, HiveMood.Sad }, f.Moods);
        }

        [Test]
        public void Of_DeliverGoalHiveWithLess_IsSad()
        {
            var level = Build(new[] { "R5AB" }, needs: new[] { 2, 2 }, deliver: new[] { ('A', 2), ('B', 2) });

            var f = HiveFeedback.Of(level, State(level, 2, 1), success: false);

            Assert.AreEqual(HiveVerdict.NotEqual, f.Verdict);
            Assert.AreEqual(HiveMood.Sad, f.Moods[1]);
        }

        [Test]
        public void Of_LevelWithoutHivesOrHoneyGoal_HasNothingToSay()
        {
            var plain = Build(new[] { "RfG" }, collectAll: false, reachG: true);

            Assert.AreEqual(HiveVerdict.None, HiveFeedback.Of(plain, plain.Start, success: false).Verdict);
            Assert.AreEqual(0, HiveFeedback.Of(plain, plain.Start, success: false).Hives.Count);
        }
    }
}
