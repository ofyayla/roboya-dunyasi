using NUnit.Framework;
using Roboya.CodingEngine.World;
using static Roboya.Tests.CodingEngine.HoneyLevels;

namespace Roboya.Tests.CodingEngine
{
    public class HoneyRulesTests
    {
        private static RobotState At(Level level, int x, int y) => level.Start.With(new GridPosition(x, y));

        [Test]
        public void Collect_HoneyItem_AddsItsAmountToTheBasket()
        {
            var level = Build(new[] { "R3A" });

            var after = HoneyRules.Collect(level, level.Start.WithCollected(0), 0);

            Assert.AreEqual(3, after.Basket);
        }

        [Test]
        public void Collect_Flower_LeavesTheBasketAlone()
        {
            var level = Build(new[] { "RfA" });

            var after = HoneyRules.Collect(level, level.Start.WithCollected(0), 0);

            Assert.AreEqual(0, after.Basket);
        }

        [Test]
        public void AutoDropAmount_AutoDropOff_IsZero()
        {
            var level = Build(new[] { "R.A" }, carrying: 2);

            Assert.AreEqual(0, HoneyRules.AutoDropAmount(level, At(level, 2, 0), 0));
        }

        [Test]
        public void AutoDropAmount_BasketBiggerThanNeed_TakesOnlyTheNeed()
        {
            var level = Build(new[] { "R.A" }, needs: new[] { 2 }, carrying: 5, autoDrop: true);

            Assert.AreEqual(2, HoneyRules.AutoDropAmount(level, At(level, 2, 0), 0));
        }

        [Test]
        public void AutoDropAmount_BasketSmallerThanNeed_TakesWhatItHas()
        {
            var level = Build(new[] { "R.A" }, needs: new[] { 3 }, carrying: 1, autoDrop: true);

            Assert.AreEqual(1, HoneyRules.AutoDropAmount(level, At(level, 2, 0), 0));
        }

        [Test]
        public void AutoDropAmount_HiveAlreadyFull_IsZero()
        {
            var level = Build(new[] { "R.A" }, needs: new[] { 2 }, carrying: 2, autoDrop: true);
            var full = At(level, 2, 0).WithDelivered(0, 2);

            Assert.AreEqual(0, HoneyRules.AutoDropAmount(level, full, 0));
        }

        [Test]
        public void AutoDropAmount_Table_IsZero()
        {
            var level = Build(new[] { "R.T" }, carrying: 2, autoDrop: true);

            Assert.AreEqual(0, HoneyRules.AutoDropAmount(level, At(level, 2, 0), 0));
        }

        [Test]
        public void AutoDrop_Hive_MovesHoneyFromBasketToHive()
        {
            var level = Build(new[] { "R.A" }, needs: new[] { 2 }, carrying: 3, autoDrop: true);

            var after = HoneyRules.AutoDrop(level, At(level, 2, 0), 0);

            Assert.AreEqual(1, after.Basket);
            Assert.AreEqual(2, after.DeliveredAt(0));
        }

        [Test]
        public void TryDrop_HiveWithRoom_Drops()
        {
            var level = Build(new[] { "R.A" }, needs: new[] { 2 }, carrying: 1);

            var outcome = HoneyRules.TryDrop(level, At(level, 2, 0), out var next);

            Assert.AreEqual(DropOutcome.Dropped, outcome);
            Assert.AreEqual(0, next.Basket);
            Assert.AreEqual(1, next.DeliveredAt(0));
        }

        [Test]
        public void TryDrop_EmptyBasket_NothingToDrop()
        {
            var level = Build(new[] { "R.A" });
            var state = At(level, 2, 0);

            var outcome = HoneyRules.TryDrop(level, state, out var next);

            Assert.AreEqual(DropOutcome.NothingToDrop, outcome);
            Assert.AreEqual(state, next);
        }

        [Test]
        public void TryDrop_OffADropPoint_NothingToDrop()
        {
            var level = Build(new[] { "R.A" }, carrying: 1);

            Assert.AreEqual(DropOutcome.NothingToDrop, HoneyRules.TryDrop(level, level.Start, out _));
        }

        [Test]
        public void TryDrop_PastTheNeed_OverflowsAndStillCountsTheSpill()
        {
            var level = Build(new[] { "R.A" }, needs: new[] { 1 }, carrying: 2);
            var full = At(level, 2, 0).WithDelivered(0, 1);

            var outcome = HoneyRules.TryDrop(level, full, out var next);

            Assert.AreEqual(DropOutcome.Overflow, outcome);
            Assert.AreEqual(2, next.DeliveredAt(0));
            Assert.AreEqual(1, next.Basket);
        }

        [Test]
        public void TryDrop_OnTheTable_NeverOverflows()
        {
            var level = Build(new[] { "R.T" }, carrying: 4);
            var state = At(level, 2, 0).WithDelivered(0, 3);

            Assert.AreEqual(DropOutcome.Dropped, HoneyRules.TryDrop(level, state, out var next));
            Assert.AreEqual(4, next.DeliveredAt(0));
        }
    }
}
