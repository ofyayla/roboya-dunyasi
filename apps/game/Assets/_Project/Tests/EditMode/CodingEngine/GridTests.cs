using System;
using NUnit.Framework;
using Roboya.CodingEngine.World;

namespace Roboya.Tests.CodingEngine
{
    public class GridTests
    {
        [Test]
        public void FromRows_ValidMap_ParsesCells()
        {
            var grid = Grid.FromRows(new[] { "..#", "#.." });

            Assert.AreEqual(3, grid.Width);
            Assert.AreEqual(2, grid.Height);
            Assert.AreEqual(CellType.Blocked, grid[new GridPosition(2, 0)]);
            Assert.AreEqual(CellType.Floor, grid[new GridPosition(1, 1)]);
        }

        [Test]
        public void IsWalkable_BlockedOrOutside_ReturnsFalse()
        {
            var grid = Grid.FromRows(new[] { ".#", ".." });

            Assert.IsTrue(grid.IsWalkable(new GridPosition(0, 0)));
            Assert.IsFalse(grid.IsWalkable(new GridPosition(1, 0)));
            Assert.IsFalse(grid.IsWalkable(new GridPosition(-1, 0)));
            Assert.IsFalse(grid.IsWalkable(new GridPosition(0, 2)));
        }

        [Test]
        public void FromRows_RaggedRows_Throws()
        {
            Assert.Throws<ArgumentException>(() => Grid.FromRows(new[] { "...", ".." }));
        }

        [Test]
        public void FromRows_UnknownCharacter_Throws()
        {
            Assert.Throws<ArgumentException>(() => Grid.FromRows(new[] { "..", ".x" }));
        }

        [Test]
        public void FromRows_Empty_Throws()
        {
            Assert.Throws<ArgumentException>(() => Grid.FromRows(new string[0]));
            Assert.Throws<ArgumentException>(() => Grid.FromRows(null));
        }

        [Test]
        public void Constructor_SizeOutOfRange_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Grid(1, 4, new CellType[4]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Grid(13, 2, new CellType[26]));
        }

        [Test]
        public void Constructor_WrongCellCount_Throws()
        {
            Assert.Throws<ArgumentException>(() => new Grid(2, 2, new CellType[3]));
            Assert.Throws<ArgumentException>(() => new Grid(2, 2, null));
        }
    }
}
