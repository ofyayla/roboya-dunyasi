using System.Linq;
using NUnit.Framework;
using Roboya.CodingEngine.Execution;
using Roboya.CodingEngine.World;
using static Roboya.Tests.CodingEngine.HoneyLevels;

namespace Roboya.Tests.CodingEngine
{
    public class InterpreterHoneyTests
    {
        [Test]
        public void Execute_StepOnHoney_PutsItsAmountInTheBasket()
        {
            var level = Build(new[] { "R3A" });

            var result = Interpreter.Execute(level, Prog(F));

            Assert.AreEqual(3, result.FinalState.Basket);
        }

        [Test]
        public void Execute_DropOnHive_DeliversAndSucceeds()
        {
            var level = Build(new[] { "R1A" }, deliver: new[] { ('A', 1) });

            var result = Interpreter.Execute(level, Prog(F, F, D));

            Assert.AreEqual(ExecutionOutcome.Success, result.Outcome);
            Assert.AreEqual(1, result.FinalState.DeliveredAt(0));
            Assert.AreEqual(0, result.FinalState.Basket);
            Assert.AreEqual(3, result.StepsExecuted);
        }

        [Test]
        public void Execute_DropWithEmptyBasket_StopsOnThatCard()
        {
            var level = Build(new[] { "R.A" }, deliver: new[] { ('A', 1) });

            var result = Interpreter.Execute(level, Prog(F, F, D, F));

            Assert.AreEqual(ExecutionOutcome.NothingToDrop, result.Outcome);
            CollectionAssert.AreEqual(new[] { 2 }, result.ErrorCommandPath);
            Assert.AreEqual(3, result.StepsExecuted);
        }

        [Test]
        public void Execute_DropAwayFromAnyDropPoint_StopsWithNothingToDrop()
        {
            var level = Build(new[] { "R1A" }, deliver: new[] { ('A', 1) });

            var events = new Interpreter(level, Prog(F, D)).Run().ToList();

            var stop = events.Single(e => e.Kind == ExecutionEventKind.NothingToDrop);
            Assert.AreEqual(-1, stop.DropIndex);
            Assert.AreEqual(ExecutionOutcome.NothingToDrop, events.Last().Result.Outcome);
        }

        [Test]
        public void Execute_DropPastTheNeed_OverflowsAndStopsOnThatCard()
        {
            var level = Build(new[] { "R2A" }, needs: new[] { 1 }, deliver: new[] { ('A', 1) });

            var events = new Interpreter(level, Prog(F, F, D, D, D)).Run().ToList();

            var overflow = events.Single(e => e.Kind == ExecutionEventKind.HiveOverflow);
            Assert.AreEqual(0, overflow.DropIndex);
            Assert.AreEqual(2, overflow.After.DeliveredAt(0));
            var result = events.Last().Result;
            Assert.AreEqual(ExecutionOutcome.HiveOverflow, result.Outcome);
            CollectionAssert.AreEqual(new[] { 3 }, result.ErrorCommandPath);
        }

        [Test]
        public void Execute_AutoDrop_HiveTakesWhatItNeedsAndTheBeeKeepsTheRest()
        {
            var level = Build(new[] { "R3A" }, needs: new[] { 2 }, autoDrop: true, deliver: new[] { ('A', 2) }, cards: MovesAndDrop);

            var events = new Interpreter(level, Prog(F, F)).Run().ToList();

            var result = events.Last().Result;
            Assert.AreEqual(1, result.FinalState.Basket);
            Assert.AreEqual(2, result.FinalState.DeliveredAt(0));
            Assert.AreEqual(ExecutionOutcome.Success, result.Outcome);
            Assert.AreEqual(2, result.StepsExecuted, "taking honey is not a card");
        }

        [Test]
        public void Execute_AutoDrop_EmitsOneDroppedEventPerHoney()
        {
            var level = Build(new[] { "R2A" }, needs: new[] { 2 }, autoDrop: true, deliver: new[] { ('A', 2) });

            var kinds = new Interpreter(level, Prog(F, F)).Run().Select(e => e.Kind).ToList();

            CollectionAssert.AreEqual(
                new[]
                {
                    ExecutionEventKind.CommandStarted, ExecutionEventKind.Moved, ExecutionEventKind.Collected,
                    ExecutionEventKind.CommandStarted, ExecutionEventKind.Moved, ExecutionEventKind.Dropped, ExecutionEventKind.Dropped,
                    ExecutionEventKind.Finished,
                },
                kinds);
        }

        [Test]
        public void Execute_AutoDropOff_LeavesTheHoneyInTheBasket()
        {
            var level = Build(new[] { "R2A" }, deliver: new[] { ('A', 2) });

            var result = Interpreter.Execute(level, Prog(F, F));

            Assert.AreEqual(2, result.FinalState.Basket);
            Assert.AreEqual(ExecutionOutcome.HoneyGoalNotMet, result.Outcome);
        }

        [Test]
        public void Execute_ItemsMissing_BeatsHoneyGoalNotMet()
        {
            var level = Build(new[] { "R1A" }, deliver: new[] { ('A', 1) });

            Assert.AreEqual(ExecutionOutcome.MissingItems, Interpreter.Execute(level, Prog(Rt)).Outcome);
        }

        [Test]
        public void Execute_HoneyDoneButAwayFromTheReachCell_IsEndedAwayFromGoal()
        {
            var level = Build(new[] { "R1AG" }, deliver: new[] { ('A', 1) }, reachG: true);

            var result = Interpreter.Execute(level, Prog(F, F, D));

            Assert.AreEqual(ExecutionOutcome.EndedAwayFromGoal, result.Outcome);
        }

        [Test]
        public void Run_FromAKeptState_KeepsTheBasketAndDeliveries()
        {
            var level = Build(new[] { "R2AB" }, needs: new[] { 1, 1 }, deliver: new[] { ('A', 1), ('B', 1) });
            var first = Interpreter.Execute(level, Prog(F, F, D));

            var second = new Interpreter(level, Prog(F, D)).Run(first.FinalState).Last().Result;

            Assert.AreEqual(ExecutionOutcome.Success, second.Outcome);
            Assert.AreEqual(1, second.FinalState.DeliveredAt(0));
            Assert.AreEqual(1, second.FinalState.DeliveredAt(1));
        }

        [Test]
        public void Execute_VisitingCollectedHoneyAgain_DoesNotAddItTwice()
        {
            var level = Build(new[] { "R1A" }, deliver: new[] { ('A', 1) });

            var result = Interpreter.Execute(level, Prog(F, F, CodingBack, F));

            Assert.AreEqual(1, result.FinalState.Basket);
        }

        private const Roboya.CodingEngine.Commands.CardType CodingBack = Roboya.CodingEngine.Commands.CardType.Backward;
    }
}
