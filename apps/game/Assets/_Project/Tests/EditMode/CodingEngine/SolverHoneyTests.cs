using NUnit.Framework;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.Execution;
using Roboya.CodingEngine.Solving;
using Roboya.CodingEngine.World;
using static Roboya.Tests.CodingEngine.HoneyLevels;

namespace Roboya.Tests.CodingEngine
{
    public class SolverHoneyTests
    {
        [Test]
        public void Solve_DeliverOneHoney_NeedsTheDropCard()
        {
            var level = Build(new[] { "R1A" }, deliver: new[] { ('A', 1) });

            var result = Solver.Solve(level);

            Assert.AreEqual(SolveStatus.Solved, result.Status);
            CollectionAssert.AreEqual(new[] { F, F, D }, result.Solution);
        }

        [Test]
        public void Solve_AutoDrop_NeedsNoDropCard()
        {
            var level = Build(new[] { "R1A" }, autoDrop: true, deliver: new[] { ('A', 1) }, cards: new[] { CardType.Forward });

            var result = Solver.Solve(level);

            Assert.AreEqual(2, result.ShortestLength);
        }

        [Test]
        public void Solve_ShareFiveHoneyBetweenTwoHives_LeavesOneOnTheTable()
        {
            var level = Build(new[] { "R5ABT" }, needs: new[] { 3, 3 }, shareAmong: "AB", leftover: 'T', maxLength: 12);

            var result = Solver.Solve(level);

            Assert.AreEqual(SolveStatus.Solved, result.Status);
            Assert.AreEqual(9, result.ShortestLength); // F F, D D, F, D D, F, D
            var run = Interpreter.Execute(level, ToProgram(result));
            Assert.AreEqual(ExecutionOutcome.Success, run.Outcome);
            Assert.AreEqual(2, run.FinalState.DeliveredAt(0));
            Assert.AreEqual(2, run.FinalState.DeliveredAt(1));
            Assert.AreEqual(1, run.FinalState.DeliveredAt(2));
        }

        [Test]
        public void Solve_DeliveryTheHiveCannotTake_IsUnsolvable()
        {
            var level = Build(new[] { "R2A" }, needs: new[] { 1 }, deliver: new[] { ('A', 2) });

            Assert.AreEqual(SolveStatus.Unsolvable, Solver.Solve(level).Status);
        }

        [Test]
        public void Solve_HoneyPassedTwice_IsOnlyCollectedOnce()
        {
            var level = Build(new[] { "R1A" }, needs: new[] { 3 }, deliver: new[] { ('A', 2) }, cards: new[] { CardType.Forward, CardType.Backward, CardType.Drop }, maxLength: 12);

            Assert.AreEqual(SolveStatus.Unsolvable, Solver.Solve(level).Status);
        }

        [Test]
        public void Solve_CarryAtEnd_KeepsTheRestInTheBasket()
        {
            var level = Build(new[] { "R3A" }, needs: new[] { 3 }, deliver: new[] { ('A', 1) }, carryAtEnd: 2);

            var result = Solver.Solve(level);

            CollectionAssert.AreEqual(new[] { F, F, D }, result.Solution);
        }

        [Test]
        public void Solve_StartingBasket_CountsTowardsTheDelivery()
        {
            var level = Build(new[] { "R.A" }, deliver: new[] { ('A', 2) }, carrying: 2, collectAll: false);

            CollectionAssert.AreEqual(new[] { F, F, D, D }, Solver.Solve(level).Solution);
        }

        [Test]
        public void TryApply_DropOnAHiveThatCannotTakeMore_ReturnsFalseAndKeepsTheState()
        {
            var level = Build(new[] { "R.A" }, needs: new[] { 1 }, carrying: 2);
            var full = level.Start.With(new GridPosition(2, 0)).WithDelivered(0, 1);

            Assert.IsFalse(Solver.TryApply(level, full, CardType.Drop, out var next));
            Assert.AreEqual(full, next);
        }

        [Test]
        public void TryApply_MoveOntoAHive_AutoDrops()
        {
            var level = Build(new[] { "R.A" }, needs: new[] { 2 }, carrying: 3, autoDrop: true);
            var before = level.Start.With(new GridPosition(1, 0));

            Assert.IsTrue(Solver.TryApply(level, before, CardType.Forward, out var next));
            Assert.AreEqual(2, next.DeliveredAt(0));
            Assert.AreEqual(1, next.Basket);
        }

        private static Program ToProgram(SolveResult result) => Prog(System.Linq.Enumerable.ToArray(result.Solution));
    }
}
