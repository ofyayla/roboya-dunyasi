using NUnit.Framework;
using Roboya.CodingEngine.World;

namespace Roboya.Tests.CodingEngine
{
    public class DirectionTests
    {
        [TestCase(Direction.North, Direction.West)]
        [TestCase(Direction.West, Direction.South)]
        [TestCase(Direction.South, Direction.East)]
        [TestCase(Direction.East, Direction.North)]
        public void TurnLeft_AnyDirection_RotatesCounterClockwise(Direction from, Direction expected)
        {
            Assert.AreEqual(expected, from.TurnLeft());
        }

        [TestCase(Direction.North, Direction.East)]
        [TestCase(Direction.West, Direction.North)]
        public void TurnRight_AnyDirection_RotatesClockwise(Direction from, Direction expected)
        {
            Assert.AreEqual(expected, from.TurnRight());
        }

        [TestCase(Direction.North, Direction.South)]
        [TestCase(Direction.East, Direction.West)]
        public void Opposite_AnyDirection_ReturnsReverse(Direction from, Direction expected)
        {
            Assert.AreEqual(expected, from.Opposite());
        }

        [TestCase(Direction.North, 2, 1)]
        [TestCase(Direction.East, 3, 2)]
        [TestCase(Direction.South, 2, 3)]
        [TestCase(Direction.West, 1, 2)]
        public void Step_FromCenter_MovesOneCell(Direction d, int x, int y)
        {
            Assert.AreEqual(new GridPosition(x, y), new GridPosition(2, 2).Step(d));
        }

        [Test]
        public void GridPosition_Equality_UsesCoordinates()
        {
            var a = new GridPosition(1, 2);
            object boxed = new GridPosition(1, 2);

            Assert.IsTrue(a == new GridPosition(1, 2));
            Assert.IsTrue(a != new GridPosition(2, 1));
            Assert.IsTrue(a.Equals(boxed));
            Assert.IsFalse(a.Equals("(1,2)"));
            Assert.AreEqual(new GridPosition(1, 2).GetHashCode(), a.GetHashCode());
            Assert.AreEqual("(1,2)", a.ToString());
        }
    }
}
