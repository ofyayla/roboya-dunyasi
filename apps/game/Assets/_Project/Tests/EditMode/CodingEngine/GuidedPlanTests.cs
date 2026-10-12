using NUnit.Framework;
using Roboya.CodingEngine.Play;
using static Roboya.Tests.CodingEngine.TestLevels;

namespace Roboya.Tests.CodingEngine
{
    public class GuidedPlanTests
    {
        private static readonly string[] Corner = { "..G", "...", "R.." };

        [Test]
        public void Next_EmptyPlan_PointsAtFirstCard()
        {
            var step = GuidedPlan.Next(Build(Corner), new Roboya.CodingEngine.Commands.CardType[0]);

            Assert.AreEqual(F, step.NextCard);
            Assert.IsFalse(step.IsComplete);
            Assert.IsFalse(step.NeedsFix);
        }

        [Test]
        public void Next_PartialCorrectPlan_PointsAtNextCard()
        {
            var step = GuidedPlan.Next(Build(Corner), new[] { F, F });

            Assert.AreEqual(Rt, step.NextCard);
            Assert.IsFalse(step.NeedsFix);
        }

        [Test]
        public void Next_CorrectFullPlan_IsComplete()
        {
            var step = GuidedPlan.Next(Build(Corner), new[] { F, F, Rt, F, F });

            Assert.IsTrue(step.IsComplete);
            Assert.IsNull(step.NextCard);
        }

        [Test]
        public void Next_WrongCard_PointsAtTheRightOneAndNeedsFix()
        {
            var step = GuidedPlan.Next(Build(Corner), new[] { F, F, F });

            Assert.IsTrue(step.NeedsFix);
            Assert.AreEqual(Rt, step.NextCard);
        }
    }
}
