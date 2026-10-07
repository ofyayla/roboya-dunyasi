using System;
using NUnit.Framework;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.World;

namespace Roboya.Tests.CodingEngine
{
    public class LevelTests
    {
        private static readonly Grid Open = Grid.FromRows(new[] { "...", "...", ".#." });
        private static readonly RobotState Start = new RobotState(new GridPosition(0, 2), Direction.North, 0);
        private static readonly Goal ReachTopRight = new Goal(new GridPosition(2, 0), null);

        private static Level Make(
            Grid grid = null,
            RobotState? start = null,
            Item[] items = null,
            Goal goal = null,
            CardType[] cards = null,
            int max = 5) =>
            new Level("l", grid ?? Open, start ?? Start, items, goal ?? ReachTopRight, cards ?? TestLevels.Moves, max);

        [Test]
        public void Constructor_ValidLevel_IndexesItems()
        {
            var level = Make(items: new[] { new Item("a", "flower", "red", new GridPosition(1, 1)) });

            Assert.AreEqual(0, level.ItemIndexAt(new GridPosition(1, 1)));
            Assert.AreEqual(-1, level.ItemIndexAt(new GridPosition(0, 0)));
            Assert.AreEqual(-1, level.ItemIndexAt(new GridPosition(9, 9)));
            Assert.AreEqual("l", level.Id);
            Assert.AreEqual(5, level.MaxProgramLength);
        }

        [Test]
        public void Constructor_StartOnBlockedCell_Throws()
        {
            Assert.Throws<LevelValidationException>(() => Make(start: new RobotState(new GridPosition(1, 2), Direction.North, 0)));
        }

        [Test]
        public void Constructor_StartWithCollectedItems_Throws()
        {
            Assert.Throws<LevelValidationException>(() => Make(start: new RobotState(new GridPosition(0, 2), Direction.North, 1)));
        }

        [Test]
        public void Constructor_GoalOnBlockedCell_Throws()
        {
            Assert.Throws<LevelValidationException>(() => Make(goal: new Goal(new GridPosition(1, 2), null)));
        }

        [Test]
        public void Constructor_EmptyGoal_Throws()
        {
            Assert.Throws<LevelValidationException>(() => Make(goal: new Goal(null, null)));
        }

        [Test]
        public void Constructor_GoalReferencesMissingItem_Throws()
        {
            Assert.Throws<LevelValidationException>(() => Make(goal: new Goal(null, new[] { 3 })));
            Assert.Throws<LevelValidationException>(() => Make(goal: new Goal(null, new[] { -1 })));
        }

        [Test]
        public void Constructor_DuplicateItemId_Throws()
        {
            var items = new[]
            {
                new Item("a", "flower", "red", new GridPosition(0, 0)),
                new Item("a", "flower", "red", new GridPosition(1, 0)),
            };

            Assert.Throws<LevelValidationException>(() => Make(items: items));
        }

        [Test]
        public void Constructor_TwoItemsSameCell_Throws()
        {
            var items = new[]
            {
                new Item("a", "flower", "red", new GridPosition(0, 0)),
                new Item("b", "flower", "red", new GridPosition(0, 0)),
            };

            Assert.Throws<LevelValidationException>(() => Make(items: items));
        }

        [Test]
        public void Constructor_ItemOnBlockedOrStartCell_Throws()
        {
            Assert.Throws<LevelValidationException>(() => Make(items: new[] { new Item("a", "f", null, new GridPosition(1, 2)) }));
            Assert.Throws<LevelValidationException>(() => Make(items: new[] { new Item("a", "f", null, new GridPosition(0, 2)) }));
        }

        [Test]
        public void Constructor_TooManyItems_Throws()
        {
            var grid = new Grid(12, 12, new CellType[144]);
            var items = new Item[65];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new Item("i" + i, "f", null, new GridPosition(i % 12, 1 + i / 12));
            }

            Assert.Throws<LevelValidationException>(() =>
                new Level("l", grid, new RobotState(new GridPosition(0, 0), Direction.North, 0), items, new Goal(new GridPosition(1, 0), null), TestLevels.Moves, 5));
        }

        [Test]
        public void Constructor_InvalidPlanOrCards_Throws()
        {
            Assert.Throws<LevelValidationException>(() => Make(max: 0));
            Assert.Throws<LevelValidationException>(() => Make(cards: new CardType[0]));
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new Level(null, Open, Start, null, ReachTopRight, TestLevels.Moves, 5));
            Assert.Throws<ArgumentNullException>(() => new Level("l", null, Start, null, ReachTopRight, TestLevels.Moves, 5));
            Assert.Throws<ArgumentNullException>(() => new Level("l", Open, Start, null, null, TestLevels.Moves, 5));
            Assert.Throws<ArgumentNullException>(() => new Level("l", Open, Start, null, ReachTopRight, null, 5));
        }

        [Test]
        public void IsSatisfiedBy_ReachAndCollect_RequiresBoth()
        {
            var goal = new Goal(new GridPosition(1, 1), new[] { 0, 2 });
            var at = new RobotState(new GridPosition(1, 1), Direction.North, 0b101);

            Assert.IsTrue(goal.IsSatisfiedBy(at));
            Assert.IsFalse(goal.IsSatisfiedBy(new RobotState(new GridPosition(1, 1), Direction.North, 0b001)));
            Assert.IsFalse(goal.IsSatisfiedBy(new RobotState(new GridPosition(0, 1), Direction.North, 0b101)));
            Assert.AreEqual(0b101UL, goal.MustCollectMask);
        }
    }
}
