using System;
using NUnit.Framework;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.Solving;
using static Roboya.Tests.CodingEngine.TestLevels;

namespace Roboya.Tests.CodingEngine
{
    public class HintAdvisorTests
    {
        private static readonly string[] Corner = { "..G", "...", "R.." };

        [Test]
        public void Analyze_CorrectProgram_NoHint()
        {
            var hint = HintAdvisor.Analyze(Build(Corner), new[] { F, F, Rt, F, F });

            Assert.IsFalse(hint.HasHint);
        }

        [Test]
        public void Analyze_BumpingCard_PointsAtIt()
        {
            var hint = HintAdvisor.Analyze(Build(Corner), new[] { F, F, F, Rt, F });

            Assert.AreEqual(2, hint.WrongIndex);
            Assert.AreEqual(Rt, hint.SuggestedCard);
        }

        [Test]
        public void Analyze_DeadEndTurnWithTightPlan_PointsAtTurn()
        {
            // Turning left first wastes slots: with 5 slots the goal is no longer reachable.
            var hint = HintAdvisor.Analyze(Build(Corner, maxLength: 5), new[] { L, F, F, Rt, F });

            Assert.AreEqual(0, hint.WrongIndex);
            Assert.IsTrue(hint.SuggestedCard.HasValue);
        }

        [Test]
        public void Analyze_MissingCards_PointsAtNextSlot()
        {
            var hint = HintAdvisor.Analyze(Build(Corner), new[] { F, F, Rt });

            Assert.AreEqual(3, hint.WrongIndex);
            Assert.AreEqual(F, hint.SuggestedCard);
        }

        [Test]
        public void Analyze_NonMoveCard_PointsAtIt()
        {
            var hint = HintAdvisor.Analyze(Build(Corner), new[] { F, CardType.Repeat });

            Assert.AreEqual(1, hint.WrongIndex);
        }

        [Test]
        public void Analyze_UnsolvableLevel_NoHint()
        {
            Assert.IsFalse(HintAdvisor.Analyze(Build(new[] { "R#G", ".#." }), new[] { F }).HasHint);
        }

        [Test]
        public void Analyze_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => HintAdvisor.Analyze(null, new CardType[0]));
            Assert.Throws<ArgumentNullException>(() => HintAdvisor.Analyze(Build(Corner), null));
        }
    }
}
