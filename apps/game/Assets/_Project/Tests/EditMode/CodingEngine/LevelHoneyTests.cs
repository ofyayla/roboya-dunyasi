using System;
using System.Collections.Generic;
using NUnit.Framework;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.World;
using static Roboya.Tests.CodingEngine.HoneyLevels;

namespace Roboya.Tests.CodingEngine
{
    public class LevelHoneyTests
    {
        private static readonly string[] Rows = { "...", "..#" };

        private static Level Make(
            IReadOnlyList<DropPoint> drops,
            IReadOnlyList<Item> items = null,
            Goal goal = null,
            RobotState? start = null)
        {
            return new Level(
                "t",
                Grid.FromRows(Rows),
                start ?? new RobotState(new GridPosition(0, 0), Direction.East, 0),
                items ?? Array.Empty<Item>(),
                goal ?? new Goal(new GridPosition(1, 0), null),
                new[] { CardType.Forward, CardType.Drop },
                8,
                drops);
        }

        private static DropPoint Hive(string id, int x, int y, int need = 2) => new DropPoint(id, DropPointKind.Hive, new GridPosition(x, y), need);

        private static DropPoint Table(string id, int x, int y) => new DropPoint(id, DropPointKind.Table, new GridPosition(x, y));

        [Test]
        public void DropIndexAt_DropPointCell_ReturnsItsIndex()
        {
            var level = Make(new[] { Hive("a", 1, 0), Table("t", 2, 0) });

            Assert.AreEqual(1, level.DropIndexAt(new GridPosition(2, 0)));
            Assert.AreEqual(-1, level.DropIndexAt(new GridPosition(0, 1)));
            Assert.AreEqual(-1, level.DropIndexAt(new GridPosition(9, 9)));
        }

        [Test]
        public void New_NoDrops_HasEmptyDropListAndNoAutoDrop()
        {
            var level = Make(null);

            Assert.AreEqual(0, level.Drops.Count);
            Assert.IsFalse(level.AutoDrop);
        }

        [Test]
        public void New_DropOnABlockedCell_Throws()
        {
            Assert.Throws<LevelValidationException>(() => Make(new[] { Hive("a", 2, 1) }));
        }

        [Test]
        public void New_DropOnTheStartCell_Throws()
        {
            Assert.Throws<LevelValidationException>(() => Make(new[] { Hive("a", 0, 0) }));
        }

        [Test]
        public void New_TwoDropsOnOneCell_Throws()
        {
            Assert.Throws<LevelValidationException>(() => Make(new[] { Hive("a", 1, 0), Hive("b", 1, 0) }));
        }

        [Test]
        public void New_DuplicateDropId_Throws()
        {
            Assert.Throws<LevelValidationException>(() => Make(new[] { Hive("a", 1, 0), Hive("a", 2, 0) }));
        }

        [Test]
        public void New_DropOnAnItem_Throws()
        {
            var item = new Item("h", "honey", null, new GridPosition(1, 0));

            Assert.Throws<LevelValidationException>(() => Make(new[] { Hive("a", 1, 0) }, new[] { item }, new Goal(null, new[] { 0 })));
        }

        [Test]
        public void New_MoreThanFourDrops_Throws()
        {
            var drops = new[] { Hive("a", 1, 0), Hive("b", 2, 0), Hive("c", 0, 1), Hive("d", 1, 1), Table("t", 2, 0) };

            Assert.Throws<LevelValidationException>(() => Make(drops));
        }

        [Test]
        public void New_HiveNeedingNothingOrTooMuch_Throws()
        {
            Assert.Throws<LevelValidationException>(() => Make(new[] { Hive("a", 1, 0, 0) }));
            Assert.Throws<LevelValidationException>(() => Make(new[] { Hive("a", 1, 0, RobotState.MaxCount) }));
        }

        [Test]
        public void New_MoreHoneyThanTheCountersHold_Throws()
        {
            var items = new[] { new Item("h", "honey", null, new GridPosition(1, 0), 9), new Item("g", "honey", null, new GridPosition(2, 0), 8) };

            Assert.Throws<LevelValidationException>(() => Make(null, items, new Goal(null, new[] { 0, 1 })));
        }

        [Test]
        public void New_StartWithDeliveries_Throws()
        {
            var start = new RobotState(new GridPosition(0, 0), Direction.East, 0).WithDelivered(0, 1);

            Assert.Throws<LevelValidationException>(() => Make(new[] { Hive("a", 1, 0) }, start: start));
        }

        [Test]
        public void New_StartWithABasket_Loads()
        {
            var start = new RobotState(new GridPosition(0, 0), Direction.East, 0).WithBasket(3);

            Assert.AreEqual(3, Make(new[] { Hive("a", 1, 0) }, start: start).Start.Basket);
        }

        [Test]
        public void New_GoalDeliveringToAnUnknownDropOrImpossibleCount_Throws()
        {
            var drops = new[] { Hive("a", 1, 0) };

            Assert.Throws<LevelValidationException>(() => Make(drops, goal: new Goal(null, null, new[] { new Delivery(3, 1) })));
            Assert.Throws<LevelValidationException>(() => Make(drops, goal: new Goal(null, null, new[] { new Delivery(0, 0) })));
        }

        [Test]
        public void New_CarryAtEndOutOfRange_Throws()
        {
            var drops = new[] { Hive("a", 1, 0) };

            Assert.Throws<LevelValidationException>(() => Make(drops, goal: new Goal(new GridPosition(1, 0), null, carryAtEnd: 16)));
        }

        [Test]
        public void New_GoalOnlyCarryAtEnd_Throws()
        {
            Assert.Throws<LevelValidationException>(() => Make(new[] { Hive("a", 1, 0) }, goal: new Goal(null, null, carryAtEnd: 1)));
        }

        [Test]
        public void New_ShareAmongOneHive_Throws()
        {
            var drops = new[] { Hive("a", 1, 0), Table("t", 2, 0) };

            Assert.Throws<LevelValidationException>(() => Make(drops, goal: new Goal(null, null, share: new ShareGoal(new[] { 0 }, 1))));
        }

        [Test]
        public void New_ShareAmongATableOrOutsideTheDrops_Throws()
        {
            var drops = new[] { Hive("a", 1, 0), Table("t", 2, 0) };

            Assert.Throws<LevelValidationException>(() => Make(drops, goal: new Goal(null, null, share: new ShareGoal(new[] { 0, 1 }))));
            Assert.Throws<LevelValidationException>(() => Make(drops, goal: new Goal(null, null, share: new ShareGoal(new[] { 0, 7 }))));
        }

        [Test]
        public void New_ShareLeftoverToAHive_Throws()
        {
            var drops = new[] { Hive("a", 1, 0), Hive("b", 2, 0) };

            Assert.Throws<LevelValidationException>(() => Make(drops, goal: new Goal(null, null, share: new ShareGoal(new[] { 0, 1 }, 1))));
            Assert.Throws<LevelValidationException>(() => Make(drops, goal: new Goal(null, null, share: new ShareGoal(new[] { 0, 1 }, 5))));
        }

        [Test]
        public void New_ShareWithoutRequiringAllHoney_Throws()
        {
            var drops = new[] { Hive("a", 1, 0), Hive("b", 2, 0) };
            var items = new[] { new Item("h", "honey", null, new GridPosition(0, 1), 2) };

            var ex = Assert.Throws<LevelValidationException>(() => Make(drops, items, new Goal(null, null, share: new ShareGoal(new[] { 0, 1 }))));
            StringAssert.Contains("all the honey", ex.Message);
        }

        [Test]
        public void New_ShareRequiringAllHoney_Loads()
        {
            var drops = new[] { Hive("a", 1, 0), Hive("b", 2, 0) };
            var items = new[] { new Item("h", "honey", null, new GridPosition(0, 1), 2) };

            Assert.DoesNotThrow(() => Make(drops, items, new Goal(null, new[] { 0 }, share: new ShareGoal(new[] { 0, 1 }))));
        }

        [Test]
        public void Build_FlowerAmountDoesNotCountAsHoney_Loads()
        {
            Assert.DoesNotThrow(() => Build(new[] { "RfA" }));
        }
    }
}
