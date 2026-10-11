using NUnit.Framework;
using Roboya.CodingEngine.Play;
using Roboya.CodingEngine.World;
using static Roboya.Tests.CodingEngine.HoneyLevels;

namespace Roboya.Tests.CodingEngine
{
    public class HiveReportTests
    {
        // A needs 2, B needs 3, the table is T.
        private static Level Village() => Build(new[] { "RABT" }, needs: new[] { 2, 3 }, carrying: 4);

        [Test]
        public void Of_StartState_EverythingIsEmptyAndTheBasketIsFull()
        {
            var level = Village();

            var report = HiveReport.Of(level, level.Start);

            Assert.AreEqual(4, report.Basket);
            Assert.AreEqual(3, report.Drops.Count);
            foreach (var d in report.Drops)
            {
                Assert.AreEqual(HiveState.Empty, d.State);
                Assert.AreEqual(0, d.Count);
            }
        }

        [Test]
        public void Of_PartialAndFullHives_AreTold()
        {
            var level = Village();
            var state = level.Start.WithBasket(0).WithDelivered(0, 2).WithDelivered(1, 1);

            var report = HiveReport.Of(level, state);

            Assert.AreEqual(HiveState.Full, report.Drops[0].State);
            Assert.AreEqual(HiveState.Partial, report.Drops[1].State);
            Assert.AreEqual(2, report.Drops[0].Need);
            Assert.AreEqual(1, report.Drops[1].Count);
        }

        [Test]
        public void Of_MoreThanAHiveAsksFor_Overflows()
        {
            var level = Village();

            var report = HiveReport.Of(level, level.Start.WithDelivered(0, 3));

            Assert.AreEqual(HiveState.Overflow, report.Drops[0].State);
        }

        [Test]
        public void Of_Table_IsFullAsSoonAsItHoldsHoney_AndNeverOverflows()
        {
            var level = Village();

            var empty = HiveReport.Of(level, level.Start);
            var some = HiveReport.Of(level, level.Start.WithDelivered(2, 5));

            Assert.AreEqual(HiveState.Empty, empty.Drops[2].State);
            Assert.AreEqual(HiveState.Full, some.Drops[2].State);
            Assert.AreEqual(DropPointKind.Table, some.Drops[2].Kind);
            Assert.AreEqual(0, some.Drops[2].Need);
        }

        [Test]
        public void Of_LevelWithoutDropPoints_HasOnlyTheBasket()
        {
            var level = Build(new[] { "R1G" }, reachG: true);

            var report = HiveReport.Of(level, level.Start.WithBasket(2));

            Assert.AreEqual(0, report.Drops.Count);
            Assert.AreEqual(2, report.Basket);
        }
    }
}
