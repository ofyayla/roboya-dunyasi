using NUnit.Framework;
using Roboya.CodingEngine.World;

namespace Roboya.Tests.CodingEngine
{
    public class RobotStateTests
    {
        [Test]
        public void WithCollected_Item_SetsBit()
        {
            var s = new RobotState(new GridPosition(0, 0), Direction.East, 0).WithCollected(3);

            Assert.IsTrue(s.HasCollected(3));
            Assert.IsFalse(s.HasCollected(2));
        }

        [Test]
        public void Equals_SameFields_AreEqual()
        {
            var a = new RobotState(new GridPosition(1, 1), Direction.South, 4);
            var b = new RobotState(new GridPosition(1, 1), Direction.South, 4);
            object boxed = b;

            Assert.IsTrue(a.Equals(boxed));
            Assert.IsFalse(a.Equals("x"));
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            Assert.AreNotEqual(a, a.With(Direction.North));
            Assert.AreNotEqual(a, a.With(new GridPosition(0, 0)));
        }
    }
}
