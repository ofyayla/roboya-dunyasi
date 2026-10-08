using NUnit.Framework;
using Roboya.CodingEngine.Progress;

namespace Roboya.Tests.CodingEngine
{
    public class PathRulesTests
    {
        private static readonly string[] Path = { "l1", "l2", "l3", "l4", "l5" };

        [Test]
        public void StatesOf_NewProfile_FirstIsCurrentRestLocked()
        {
            var s = PathRules.StatesOf(Path, new ProgressBook(), 3, false);

            CollectionAssert.AreEqual(
                new[] { NodeState.Current, NodeState.Locked, NodeState.Locked, NodeState.NeedsGrownUp, NodeState.NeedsGrownUp }, s);
        }

        [Test]
        public void StatesOf_FirstTwoDone_ThirdIsCurrent()
        {
            var book = new ProgressBook();
            book.Record("l1", 1);
            book.Record("l2", 3);

            var s = PathRules.StatesOf(Path, book, 3, false);

            Assert.AreEqual(NodeState.Completed, s[0]);
            Assert.AreEqual(NodeState.Completed, s[1]);
            Assert.AreEqual(NodeState.Current, s[2]);
        }

        [Test]
        public void StatesOf_FreeTierFinished_NextNeedsGrownUp()
        {
            var book = new ProgressBook();
            book.Record("l1", 1);
            book.Record("l2", 1);
            book.Record("l3", 1);

            var s = PathRules.StatesOf(Path, book, 3, false);

            Assert.AreEqual(NodeState.NeedsGrownUp, s[3]);
            Assert.IsFalse(PathRules.CanPlay(s[3]));
        }

        [Test]
        public void StatesOf_WithPremium_UnlocksInOrder()
        {
            var book = new ProgressBook();
            book.Record("l1", 1);
            book.Record("l2", 1);
            book.Record("l3", 1);

            var s = PathRules.StatesOf(Path, book, 3, true);

            Assert.AreEqual(NodeState.Current, s[3]);
            Assert.AreEqual(NodeState.Locked, s[4]);
            Assert.IsTrue(PathRules.CanPlay(s[3]));
            Assert.IsFalse(PathRules.CanPlay(s[4]));
        }

        [Test]
        public void StatesOf_PremiumLapsed_FinishedPaidLevelNeedsGrownUp()
        {
            var book = new ProgressBook();
            foreach (var id in Path)
            {
                book.Record(id, 2);
            }

            var s = PathRules.StatesOf(Path, book, 3, false);

            Assert.AreEqual(NodeState.Completed, s[2]);
            Assert.AreEqual(NodeState.NeedsGrownUp, s[4]);
            Assert.AreEqual(2, book.Stars("l5"), "stars stay for when premium returns");
        }

        [Test]
        public void StatesOf_EmptyPath_ReturnsEmpty()
        {
            Assert.AreEqual(0, PathRules.StatesOf(new string[0], new ProgressBook(), 3, false).Length);
        }
    }
}
