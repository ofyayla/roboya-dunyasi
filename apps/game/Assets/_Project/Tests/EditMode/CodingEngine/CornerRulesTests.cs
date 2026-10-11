using NUnit.Framework;
using Roboya.CodingEngine.Progress;

namespace Roboya.Tests.CodingEngine
{
    public class CornerRulesTests
    {
        [TestCase(36, 9)]
        [TestCase(24, 6)]
        [TestCase(26, 6)]
        [TestCase(4, 1)]
        [TestCase(3, 0)]
        [TestCase(0, 0)]
        public void CornerLength_PathCount_SplitsIntoFour(int count, int expected)
        {
            Assert.AreEqual(expected, CornerRules.CornerLength(count));
        }

        [Test]
        public void CornerCount_ShortPath_NoCorners()
        {
            Assert.AreEqual(0, CornerRules.CornerCount(3));
            Assert.AreEqual(4, CornerRules.CornerCount(24));
        }

        [Test]
        public void IsCornerEnd_Forest_EveryNinthLevel()
        {
            var ends = new System.Collections.Generic.List<int>();
            for (int i = 0; i < 36; i++)
            {
                if (CornerRules.IsCornerEnd(i, 36))
                {
                    ends.Add(i);
                }
            }

            CollectionAssert.AreEqual(new[] { 8, 17, 26, 35 }, ends);
        }

        [Test]
        public void IsCornerEnd_UnevenPath_LastCornerEndsOnFinale()
        {
            // 26 levels: corners of 6 end at 6, 12 and 18; the fourth takes the spare levels and ends on the finale.
            Assert.IsTrue(CornerRules.IsCornerEnd(17, 26));
            Assert.IsFalse(CornerRules.IsCornerEnd(23, 26));
            Assert.IsTrue(CornerRules.IsCornerEnd(25, 26));
        }

        [TestCase(1, 36, 8)]
        [TestCase(3, 36, 26)]
        [TestCase(4, 36, 35)]
        [TestCase(4, 26, 25)]
        [TestCase(2, 24, 11)]
        public void CornerEnd_Corner_LastLevelOfThatStretch(int corner, int count, int expected)
        {
            Assert.AreEqual(expected, CornerRules.CornerEnd(corner, count));
        }

        [Test]
        public void IsCornerEnd_ShortPathOrNegativeIndex_False()
        {
            Assert.IsFalse(CornerRules.IsCornerEnd(0, 3));
            Assert.IsFalse(CornerRules.IsCornerEnd(-1, 36));
        }
    }
}
