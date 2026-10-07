using System;
using NUnit.Framework;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.Execution;
using Roboya.CodingEngine.Solving;
using Roboya.CodingEngine.World;
using static Roboya.Tests.CodingEngine.TestLevels;

namespace Roboya.Tests.CodingEngine
{
    public class SolverTests
    {
        [Test]
        public void Solve_StraightLine_ReturnsForwardsOnly()
        {
            var result = Solver.Solve(Build(new[] { "G", ".", "." , "R" }.Select2()));

            Assert.AreEqual(SolveStatus.Solved, result.Status);
            CollectionAssert.AreEqual(new[] { F, F, F }, result.Solution);
            Assert.AreEqual(3, result.ShortestLength);
            Assert.Greater(result.StatesExplored, 0);
        }

        [Test]
        public void Solve_Corner_ShortestIsFive()
        {
            var level = Build(new[] { "..G", "...", "R.." });

            var result = Solver.Solve(level);

            Assert.AreEqual(5, result.ShortestLength);
            Assert.IsTrue(Interpreter.Execute(level, Prog(Solution(result))).IsSuccess);
        }

        [Test]
        public void Solve_OnlyRightTurns_FindsLongerPath()
        {
            // Goal is to the left; with only right turns the robot needs three right turns.
            var level = Build(new[] { "G.R" }.Pad(), Direction.North, new[] { F, Rt });

            var result = Solver.Solve(level);

            Assert.AreEqual(5, result.ShortestLength);
        }

        [Test]
        public void Solve_WithBackward_UsesIt()
        {
            var level = Build(new[] { "R", "G" }.Select2(), Direction.North, new[] { F, B, L, Rt });

            CollectionAssert.AreEqual(new[] { B }, Solver.Solve(level).Solution);
        }

        [Test]
        public void Solve_Obstacle_GoesAround()
        {
            var level = Build(new[] { "...", ".#.", "R#G" });

            var result = Solver.Solve(level);

            Assert.AreEqual(8, result.ShortestLength);
            Assert.IsTrue(Interpreter.Execute(level, Prog(Solution(result))).IsSuccess);
        }

        [Test]
        public void Solve_CollectItems_VisitsAll()
        {
            var level = Build(new[] { "a.b", "...", "R.G" });

            var result = Solver.Solve(level);

            Assert.IsTrue(result.IsSolved);
            Assert.IsTrue(Interpreter.Execute(level, Prog(Solution(result))).IsSuccess);
        }

        [Test]
        public void Solve_WalledOffGoal_IsUnsolvable()
        {
            var result = Solver.Solve(Build(new[] { "R#G", ".#." }));

            Assert.AreEqual(SolveStatus.Unsolvable, result.Status);
            Assert.AreEqual(-1, result.ShortestLength);
            Assert.IsEmpty(result.Solution);
        }

        [Test]
        public void Solve_PlanTooShort_IsUnsolvable()
        {
            var result = Solver.Solve(Build(new[] { "..G", "...", "R.." }, maxLength: 4));

            Assert.AreEqual(SolveStatus.Unsolvable, result.Status);
        }

        [Test]
        public void Solve_AlreadyAtGoal_ReturnsEmptySolution()
        {
            var level = Build(new[] { "G.", "R." });

            var result = Solver.Solve(level, level.Goal.Reach.Value.ToState(), 5);

            Assert.IsTrue(result.IsSolved);
            Assert.AreEqual(0, result.ShortestLength);
        }

        [Test]
        public void Solve_TinyBudget_ReportsLimit()
        {
            var level = Build(new[] { "a.......b", ".........", "........G", "R........" });

            var result = Solver.Solve(level, stateBudget: 10);

            Assert.AreEqual(SolveStatus.SearchLimitReached, result.Status);
        }

        [Test]
        public void Solve_NullLevel_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Solver.Solve(null, default(RobotState), 3));
        }

        [Test]
        public void TryApply_NonMoveCard_Throws()
        {
            var level = Build(new[] { "G.", "R." });

            Assert.Throws<ArgumentException>(() => Solver.TryApply(level, level.Start, CardType.Repeat, out _));
        }

        private static CardType[] Solution(SolveResult r)
        {
            var cards = new CardType[r.Solution.Count];
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i] = r.Solution[i];
            }

            return cards;
        }
    }

    internal static class SolverTestExtensions
    {
        /// <summary>Widens one-column maps to the minimum grid width by adding a blocked column.</summary>
        public static string[] Select2(this string[] rows)
        {
            var result = new string[rows.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                result[i] = rows[i] + "#";
            }

            return result;
        }

        /// <summary>Adds a blocked row so one-row maps reach the minimum grid height.</summary>
        public static string[] Pad(this string[] rows)
        {
            var result = new string[rows.Length + 1];
            Array.Copy(rows, result, rows.Length);
            result[rows.Length] = new string('#', rows[0].Length);
            return result;
        }

        public static RobotState ToState(this GridPosition p) => new RobotState(p, Direction.North, 0);
    }
}
