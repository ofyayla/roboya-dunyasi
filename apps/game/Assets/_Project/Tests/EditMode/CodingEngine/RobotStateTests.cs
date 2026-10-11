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

        [Test]
        public void WithBasket_Count_IsReadBack()
        {
            var s = new RobotState(new GridPosition(0, 0), Direction.East, 0).WithBasket(7);

            Assert.AreEqual(7, s.Basket);
            Assert.AreEqual(0, s.DeliveredAt(0));
        }

        [Test]
        public void WithDelivered_EachDropPoint_KeepsItsOwnCount()
        {
            var s = new RobotState(new GridPosition(0, 0), Direction.East, 0)
                .WithBasket(5).WithDelivered(0, 1).WithDelivered(1, 2).WithDelivered(2, 3).WithDelivered(3, 15);

            Assert.AreEqual(5, s.Basket);
            Assert.AreEqual(1, s.DeliveredAt(0));
            Assert.AreEqual(2, s.DeliveredAt(1));
            Assert.AreEqual(3, s.DeliveredAt(2));
            Assert.AreEqual(15, s.DeliveredAt(3));
        }

        [Test]
        public void WithDelivered_OverwritingACount_LeavesTheOthers()
        {
            var s = new RobotState(new GridPosition(0, 0), Direction.East, 0).WithDelivered(1, 4).WithDelivered(2, 4).WithDelivered(1, 1);

            Assert.AreEqual(1, s.DeliveredAt(1));
            Assert.AreEqual(4, s.DeliveredAt(2));
        }

        [Test]
        public void WithBasket_AboveTheCounterOrBelowZero_Throws()
        {
            var s = new RobotState(new GridPosition(0, 0), Direction.East, 0);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => s.WithBasket(16));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => s.WithBasket(-1));
        }

        [Test]
        public void DeliveredAt_FifthDropPoint_Throws()
        {
            var s = new RobotState(new GridPosition(0, 0), Direction.East, 0);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => s.DeliveredAt(4));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => s.WithDelivered(-1, 1));
        }

        [Test]
        public void Equals_DifferentHoney_AreNotEqual()
        {
            var a = new RobotState(new GridPosition(1, 1), Direction.South, 4);

            Assert.AreNotEqual(a, a.WithBasket(1));
            Assert.AreNotEqual(a.WithDelivered(0, 1), a.WithDelivered(1, 1));
            Assert.AreEqual(a.WithBasket(2).GetHashCode(), a.WithBasket(2).GetHashCode());
        }

        [Test]
        public void With_PositionOrFacing_KeepsTheHoney()
        {
            var a = new RobotState(new GridPosition(1, 1), Direction.South, 4).WithBasket(3).WithDelivered(2, 2);

            Assert.AreEqual(3, a.With(Direction.North).Basket);
            Assert.AreEqual(2, a.With(new GridPosition(0, 0)).DeliveredAt(2));
            Assert.AreEqual(3, a.WithCollected(1).Basket);
        }
    }
}
