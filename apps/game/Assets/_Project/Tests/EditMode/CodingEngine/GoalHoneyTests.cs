using NUnit.Framework;
using Roboya.CodingEngine.World;
using static Roboya.Tests.CodingEngine.HoneyLevels;

namespace Roboya.Tests.CodingEngine
{
    public class GoalHoneyTests
    {
        // A, B, C hives and a table T: indexes 0, 1, 2, 3.
        private static readonly string[] Village = { "RABCT" };

        private static RobotState State(Level level, int a = 0, int b = 0, int c = 0, int t = 0, int basket = 0) =>
            level.Start.WithBasket(basket).WithDelivered(0, a).WithDelivered(1, b).WithDelivered(2, c).WithDelivered(3, t);

        [Test]
        public void IsSatisfiedBy_DeliverExactCount_IsTrue()
        {
            var level = Build(Village, deliver: new[] { ('A', 2) });

            Assert.IsTrue(level.Goal.IsSatisfiedBy(State(level, a: 2)));
        }

        [Test]
        public void IsSatisfiedBy_DeliverTooFewOrTooMany_IsFalse()
        {
            var level = Build(Village, deliver: new[] { ('A', 2) });

            Assert.IsFalse(level.Goal.IsSatisfiedBy(State(level, a: 1)));
            Assert.IsFalse(level.Goal.IsSatisfiedBy(State(level, a: 3)));
        }

        [Test]
        public void IsSatisfiedBy_TwoDeliveries_NeedBoth()
        {
            var level = Build(Village, deliver: new[] { ('A', 1), ('B', 2) });

            Assert.IsTrue(level.Goal.IsSatisfiedBy(State(level, a: 1, b: 2)));
            Assert.IsFalse(level.Goal.IsSatisfiedBy(State(level, a: 1, b: 1)));
        }

        [Test]
        public void IsSatisfiedBy_ShareEqualAndBasketEmpty_IsTrue()
        {
            var level = Build(Village, shareAmong: "AB", leftover: 'T');

            Assert.IsTrue(level.Goal.IsSatisfiedBy(State(level, a: 2, b: 2)));
        }

        [Test]
        public void IsSatisfiedBy_ShareWithFewerLeftoverThanHives_IsTrue()
        {
            var level = Build(Village, shareAmong: "AB", leftover: 'T');

            Assert.IsTrue(level.Goal.IsSatisfiedBy(State(level, a: 2, b: 2, t: 1)));
        }

        [Test]
        public void IsSatisfiedBy_ShareLeavingAsManyAsHivesOnTheTable_IsFalse()
        {
            var level = Build(Village, shareAmong: "AB", leftover: 'T');

            Assert.IsFalse(level.Goal.IsSatisfiedBy(State(level, a: 1, b: 1, t: 2)));
        }

        [Test]
        public void IsSatisfiedBy_ShareUnequal_IsFalse()
        {
            var level = Build(Village, shareAmong: "AB", leftover: 'T');

            Assert.IsFalse(level.Goal.IsSatisfiedBy(State(level, a: 3, b: 1)));
        }

        [Test]
        public void IsSatisfiedBy_ShareWithHoneyStillInTheBasket_IsFalse()
        {
            var level = Build(Village, shareAmong: "AB", leftover: 'T');

            Assert.IsFalse(level.Goal.IsSatisfiedBy(State(level, a: 2, b: 2, basket: 1)));
        }

        [Test]
        public void IsSatisfiedBy_ShareWithHoneyOnAnUnlistedHive_IsFalse()
        {
            var level = Build(Village, shareAmong: "AB", leftover: 'T');

            Assert.IsFalse(level.Goal.IsSatisfiedBy(State(level, a: 1, b: 1, c: 1)));
        }

        [Test]
        public void IsSatisfiedBy_ShareWithoutATable_NeedsNothingLeftOver()
        {
            var level = Build(new[] { "RAB" }, shareAmong: "AB");

            Assert.IsTrue(level.Goal.IsSatisfiedBy(level.Start.WithDelivered(0, 2).WithDelivered(1, 2)));
        }

        [Test]
        public void IsSatisfiedBy_NothingDeliveredAndEmptyBasket_CountsAsEqualButOtherPartsStillApply()
        {
            // The collect part keeps "share nothing" from passing in real levels: see LevelHoneyTests.
            var level = Build(Village, shareAmong: "AB", collectAll: false);

            Assert.IsTrue(level.Goal.IsSatisfiedBy(State(level)));
        }

        [Test]
        public void IsSatisfiedBy_CarryAtEnd_NeedsExactlyThatManyInTheBasket()
        {
            var level = Build(Village, deliver: new[] { ('A', 1) }, carryAtEnd: 2);

            Assert.IsTrue(level.Goal.IsSatisfiedBy(State(level, a: 1, basket: 2)));
            Assert.IsFalse(level.Goal.IsSatisfiedBy(State(level, a: 1, basket: 1)));
        }

        [Test]
        public void IsSatisfiedBy_HoneyGoalPlusReach_NeedsBoth()
        {
            var level = Build(new[] { "RAG" }, deliver: new[] { ('A', 1) }, reachG: true);
            var delivered = level.Start.WithDelivered(0, 1);

            Assert.IsFalse(level.Goal.IsSatisfiedBy(delivered));
            Assert.IsTrue(level.Goal.IsSatisfiedBy(delivered.With(new GridPosition(2, 0))));
        }

        [Test]
        public void HasHoneyGoal_PlainGoal_IsFalse()
        {
            var level = Build(new[] { "RAG" }, reachG: true);

            Assert.IsFalse(level.Goal.HasHoneyGoal);
            Assert.IsTrue(level.Goal.IsHoneySatisfiedBy(level.Start));
        }
    }
}
