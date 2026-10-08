using NUnit.Framework;
using Roboya.CodingEngine.Progress;

namespace Roboya.Tests.CodingEngine
{
    public class RewardRulesTests
    {
        private static readonly RobotPart[] Catalog =
        {
            new RobotPart("wings-leaf", "wings", 2),
            new RobotPart("antenna-star", "antenna", 1),
            new RobotPart("hat-acorn", "hat", 3),
        };

        [TestCase(0, 0, 0)]
        [TestCase(4, 0, 0)]
        [TestCase(5, 0, 1)]
        [TestCase(10, 0, 2)]
        [TestCase(10, 1, 3)]
        public void EarnedCount_EveryFiveLevelsAndRegionEnds(int levels, int regions, int expected)
        {
            Assert.AreEqual(expected, RewardRules.EarnedCount(levels, regions, ProgressRules.Default));
        }

        [Test]
        public void EarnedCount_NegativeOrZeroRule_IsSafe()
        {
            Assert.AreEqual(3, RewardRules.EarnedCount(3, -1, new ProgressRules(3, 0)));
        }

        [Test]
        public void Earned_ReturnsFixedOrder()
        {
            var earned = RewardRules.Earned(Catalog, 2);

            Assert.AreEqual(2, earned.Count);
            Assert.AreEqual("antenna-star", earned[0].Id);
            Assert.AreEqual("wings-leaf", earned[1].Id);
        }

        [Test]
        public void NewlyEarned_ReplayWithoutNewCompletion_GivesNothing()
        {
            Assert.AreEqual(0, RewardRules.NewlyEarned(Catalog, 1, 1).Count);
            var fresh = RewardRules.NewlyEarned(Catalog, 1, 3);
            Assert.AreEqual(2, fresh.Count);
            Assert.AreEqual("wings-leaf", fresh[0].Id);
        }

        [Test]
        public void RobotPart_NullFields_Throw()
        {
            Assert.Throws<System.ArgumentNullException>(() => new RobotPart(null, "hat", 1));
            Assert.Throws<System.ArgumentNullException>(() => new RobotPart("x", null, 1));
        }
    }
}
