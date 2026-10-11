using System.Collections.Generic;
using NUnit.Framework;
using Roboya.CodingEngine.Progress;

namespace Roboya.Tests.CodingEngine
{
    public class RegionRulesTests
    {
        private static readonly string[] Forest = { "f1", "f2", "f3" };
        private static readonly string[] Village = { "v1", "v2" };
        private static readonly string[] Empty = new string[0];

        private static IReadOnlyList<IReadOnlyList<string>> Paths(params string[][] paths) => paths;

        [Test]
        public void OpenRegions_NewProfile_OnlyFirstRegionOpen()
        {
            var open = RegionRules.OpenRegions(Paths(Forest, Village), new ProgressBook());

            CollectionAssert.AreEqual(new[] { true, false }, open);
        }

        [Test]
        public void OpenRegions_FinaleDone_NextRegionOpens()
        {
            var book = new ProgressBook();
            book.Record("f3", 1);

            var open = RegionRules.OpenRegions(Paths(Forest, Village), book);

            CollectionAssert.AreEqual(new[] { true, true }, open);
        }

        [Test]
        public void OpenRegions_AllButFinaleDone_NextRegionStaysClosed()
        {
            var book = new ProgressBook();
            book.Record("f1", 3);
            book.Record("f2", 3);

            var open = RegionRules.OpenRegions(Paths(Forest, Village), book);

            CollectionAssert.AreEqual(new[] { true, false }, open);
        }

        [Test]
        public void OpenRegions_RegionWithoutLevels_NeverOpensAndDoesNotBlock()
        {
            var book = new ProgressBook();
            book.Record("f3", 1);

            var open = RegionRules.OpenRegions(Paths(Forest, Empty, Village), book);

            CollectionAssert.AreEqual(new[] { true, false, true }, open);
        }

        [Test]
        public void OpenRegions_LaterFinaleWithoutEarlier_StaysClosed()
        {
            // A synced or stale book may hold a later finale; order still decides.
            var book = new ProgressBook();
            book.Record("v2", 1);

            var open = RegionRules.OpenRegions(Paths(Forest, Village, new[] { "h1" }), book);

            CollectionAssert.AreEqual(new[] { true, false, false }, open);
        }

        [Test]
        public void OpenRegions_UnlockAll_OpensEveryRegionWithContent()
        {
            var open = RegionRules.OpenRegions(Paths(Forest, Empty, Village), new ProgressBook(), unlockAll: true);

            CollectionAssert.AreEqual(new[] { true, false, true }, open);
        }

        [Test]
        public void OpenRegions_NullPath_TreatedAsNoContent()
        {
            var open = RegionRules.OpenRegions(Paths(Forest, null), new ProgressBook());

            CollectionAssert.AreEqual(new[] { true, false }, open);
        }

        [Test]
        public void IsFinished_EmptyPath_False()
        {
            Assert.IsFalse(RegionRules.IsFinished(Empty, new ProgressBook()));
        }
    }
}
