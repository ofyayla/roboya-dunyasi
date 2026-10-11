using System.Linq;
using NUnit.Framework;
using Roboya.CodingEngine.Execution;
using Roboya.CodingEngine.Play;
using static Roboya.Tests.CodingEngine.HoneyLevels;

namespace Roboya.Tests.CodingEngine
{
    public class RemainingChoicesTests
    {
        private static RemainingQuestion Question(int carrying = 5, int give = 2)
        {
            var level = Build(new[] { "R.A" }, needs: new[] { give }, carrying: carrying, deliver: new[] { ('A', give) });
            var cards = new[] { F, F }.Concat(Enumerable.Repeat(D, give)).ToArray();
            return RemainingQuestion.From(level, Interpreter.Execute(level, Prog(cards)).FinalState);
        }

        [Test]
        public void For_AnyLevel_HasEachKindOnceWithTheQuestionsNumbers()
        {
            var choices = RemainingChoices.For("paylasim-koyu.bal-pesinde.19", Question());

            CollectionAssert.AreEquivalent(new[] { RemainingKind.Before, RemainingKind.Given, RemainingKind.Left }, choices.Select(c => c.Kind));
            Assert.AreEqual(5, choices.Single(c => c.Kind == RemainingKind.Before).Count);
            Assert.AreEqual(2, choices.Single(c => c.Kind == RemainingKind.Given).Count);
            Assert.AreEqual(3, choices.Single(c => c.Kind == RemainingKind.Left).Count);
            Assert.AreEqual(1, choices.Count(c => c.IsCorrect));
            Assert.IsTrue(choices.Single(c => c.IsCorrect).Kind == RemainingKind.Left);
        }

        [Test]
        public void For_SameLevel_IsAlwaysInTheSameOrder()
        {
            var a = RemainingChoices.For("paylasim-koyu.bal-pesinde.20", Question()).Select(c => c.Kind);
            var b = RemainingChoices.For("paylasim-koyu.bal-pesinde.20", Question()).Select(c => c.Kind);

            CollectionAssert.AreEqual(a, b);
        }

        [Test]
        public void For_DifferentLevels_DoNotAllPutTheRightCardInTheSamePlace()
        {
            var places = Enumerable.Range(19, 6)
                .Select(n => RemainingChoices.For("paylasim-koyu.bal-pesinde." + n, Question()).ToList().FindIndex(c => c.IsCorrect))
                .Distinct()
                .Count();

            Assert.Greater(places, 1);
        }

        [Test]
        public void ShouldAsk_NeedsTheOptionTheSwitchAKasifChildAndDifferentAnswers()
        {
            var q = Question();

            Assert.IsTrue(RemainingChoices.ShouldAsk(true, true, false, q));
            Assert.IsFalse(RemainingChoices.ShouldAsk(false, true, false, q), "the level does not ask");
            Assert.IsFalse(RemainingChoices.ShouldAsk(true, false, false, q), "the step is switched off until the child test");
            Assert.IsFalse(RemainingChoices.ShouldAsk(true, true, true, q), "never for Minik");
            Assert.IsFalse(RemainingChoices.ShouldAsk(true, true, false, Question(carrying: 4, give: 2)), "two cards would be the same");
        }
    }
}
