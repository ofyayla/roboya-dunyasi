using System.Collections.Generic;
using NUnit.Framework;
using Roboya.CodingEngine.Progress;

namespace Roboya.Tests.CodingEngine
{
    public class RewardRulesTests
    {
        private static readonly ShipPart[] Catalog =
        {
            new ShipPart("lights", 2),
            new ShipPart("propeller", 1),
            new ShipPart("fins", 3),
        };

        private static string[] PathOf(string prefix, int count)
        {
            var path = new string[count];
            for (int i = 0; i < count; i++)
            {
                path[i] = prefix + (i + 1);
            }

            return path;
        }

        private static ProgressBook Done(string prefix, params int[] levels)
        {
            var book = new ProgressBook();
            foreach (int level in levels)
            {
                book.Record(prefix + level, 1);
            }

            return book;
        }

        [TestCase(new int[0], 0)]
        [TestCase(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 0)]
        [TestCase(new[] { 9 }, 1)]
        [TestCase(new[] { 18 }, 2)]
        [TestCase(new[] { 27, 30 }, 3)]
        [TestCase(new[] { 36 }, 4)]
        public void EarnedOn_Forest_OnePartPerCornerReached(int[] done, int expected)
        {
            Assert.AreEqual(expected, RewardRules.EarnedOn(PathOf("f", 36), Done("f", done)));
        }

        [Test]
        public void EarnedOn_OlderChildStartingMidPath_EarnsCornersWalkedPast()
        {
            // A Kaşif child starts at level 10 (ADR 0018) and never plays level 9, the first corner's end.
            Assert.AreEqual(1, RewardRules.EarnedOn(PathOf("f", 36), Done("f", 10)));
        }

        [Test]
        public void EarnedOn_ShortOrMissingPath_NoParts()
        {
            Assert.AreEqual(0, RewardRules.EarnedOn(PathOf("s", 3), Done("s", 1, 2, 3)));
            Assert.AreEqual(0, RewardRules.EarnedOn(null, new ProgressBook()));
        }

        [Test]
        public void EarnedCount_TwoRegions_AddsUp()
        {
            var book = Done("f", 36);
            book.Record("v12", 2);
            var paths = new IReadOnlyList<string>[] { PathOf("f", 36), PathOf("v", 24), new string[0] };

            // Forest finale: 4 parts; village level 12 is the end of its second corner: 2 more.
            Assert.AreEqual(6, RewardRules.EarnedCount(paths, book));
        }

        [Test]
        public void Earned_ReturnsFixedOrder()
        {
            var earned = RewardRules.Earned(Catalog, 2);

            Assert.AreEqual(2, earned.Count);
            Assert.AreEqual("propeller", earned[0].Id);
            Assert.AreEqual("lights", earned[1].Id);
        }

        [Test]
        public void NewlyEarned_ReplayWithoutNewCompletion_GivesNothing()
        {
            Assert.AreEqual(0, RewardRules.NewlyEarned(Catalog, 1, 1).Count);
            var fresh = RewardRules.NewlyEarned(Catalog, 1, 3);
            Assert.AreEqual(2, fresh.Count);
            Assert.AreEqual("lights", fresh[0].Id);
        }

        [Test]
        public void ShipPart_NullId_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => new ShipPart(null, 1));
        }
    }
}
