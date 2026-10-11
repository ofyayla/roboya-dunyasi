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

        [Test]
        public void Next_StandingOnTheHiveWithHoney_PointsAtTheDropCard()
        {
            var level = HoneyLevels.Build(new[] { "R1A" }, deliver: new[] { ('A', 1) });

            var step = GuidedPlan.Next(level, new[] { F, F });

            Assert.AreEqual(Roboya.CodingEngine.Commands.CardType.Drop, step.NextCard);
        }

        [Test]
        public void Next_FollowedCardByCard_BuildsTheShortestVillagePlanAndSolvesIt()
        {
            // Level 23 shape: pick up, give two hives their share, the rest goes to the table.
            var level = HoneyLevels.Build(new[] { "R2ABT." , "......" }, needs: new[] { 2, 2 }, carrying: 3, maxLength: 11, shareAmong: "AB", leftover: 'T');
            var plan = new System.Collections.Generic.List<Roboya.CodingEngine.Commands.CardType>();

            for (int guard = 0; guard < 20; guard++)
            {
                var step = GuidedPlan.Next(level, plan);
                if (step.IsComplete)
                {
                    break;
                }

                Assert.IsTrue(step.NextCard.HasValue);
                plan.Add(step.NextCard.Value);
            }

            Assert.AreEqual(9, plan.Count);
            var result = Roboya.CodingEngine.Execution.Interpreter.Execute(level, HoneyLevels.Prog(plan.ToArray()));
            Assert.AreEqual(Roboya.CodingEngine.Execution.ExecutionOutcome.Success, result.Outcome);
        }
    }
}
