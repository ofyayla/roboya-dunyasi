using NUnit.Framework;
using Roboya.CodingEngine.Execution;
using Roboya.CodingEngine.Play;
using static Roboya.Tests.CodingEngine.HoneyLevels;

namespace Roboya.Tests.CodingEngine
{
    public class RemainingQuestionTests
    {
        [Test]
        public void From_FiveHoneyTwoGiven_IsFiveTwoThreeAndDistinct()
        {
            var level = Build(new[] { "R5A" }, needs: new[] { 2 }, deliver: new[] { ('A', 2) });
            var final = Interpreter.Execute(level, Prog(F, F, D, D)).FinalState;

            var q = RemainingQuestion.From(level, final);

            Assert.AreEqual(5, q.Before);
            Assert.AreEqual(2, q.Given);
            Assert.AreEqual(3, q.Left);
            Assert.IsTrue(q.HasDistinctOptions);
        }

        [Test]
        public void From_FourHoneyTwoGiven_TwoCardsWouldBeTheSame()
        {
            var level = Build(new[] { "R4A" }, needs: new[] { 2 }, deliver: new[] { ('A', 2) });
            var final = Interpreter.Execute(level, Prog(F, F, D, D)).FinalState;

            var q = RemainingQuestion.From(level, final);

            Assert.AreEqual((4, 2, 2), (q.Before, q.Given, q.Left));
            Assert.IsFalse(q.HasDistinctOptions);
        }

        [Test]
        public void From_StartingBasketCountsAsHavingHadIt()
        {
            var level = Build(new[] { "R2A" }, needs: new[] { 2 }, carrying: 3, deliver: new[] { ('A', 2) });
            var final = Interpreter.Execute(level, Prog(F, F, D, D)).FinalState;

            var q = RemainingQuestion.From(level, final);

            Assert.AreEqual(5, q.Before);
            Assert.AreEqual(3, q.Left);
        }

        [Test]
        public void From_NothingGiven_BeforeAndLeftAreTheSame()
        {
            var level = Build(new[] { "R3A" }, deliver: new[] { ('A', 1) });
            var final = Interpreter.Execute(level, Prog(F)).FinalState;

            var q = RemainingQuestion.From(level, final);

            Assert.AreEqual((3, 0, 3), (q.Before, q.Given, q.Left));
            Assert.IsFalse(q.HasDistinctOptions);
        }

        [Test]
        public void From_EverythingGiven_BeforeAndGivenAreTheSame()
        {
            var level = Build(new[] { "R2A" }, needs: new[] { 2 }, deliver: new[] { ('A', 2) });
            var final = Interpreter.Execute(level, Prog(F, F, D, D)).FinalState;

            Assert.IsFalse(RemainingQuestion.From(level, final).HasDistinctOptions);
        }

        [Test]
        public void From_HoneyOnTheTableCountsAsGiven()
        {
            var level = Build(new[] { "R5AT" }, needs: new[] { 2 }, deliver: new[] { ('A', 2) });
            var final = Interpreter.Execute(level, Prog(F, F, D, D, F, D)).FinalState;

            var q = RemainingQuestion.From(level, final);

            Assert.AreEqual(3, q.Given);
            Assert.AreEqual(2, q.Left);
        }

        [Test]
        public void From_FlowerIsNotHoney()
        {
            var level = Build(new[] { "RfG" }, collectAll: false, reachG: true);
            var final = Interpreter.Execute(level, Prog(F, F)).FinalState;

            Assert.AreEqual(0, RemainingQuestion.From(level, final).Before);
        }
    }
}
