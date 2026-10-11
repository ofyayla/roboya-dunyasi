using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.Solving;
using Roboya.CodingEngine.World;
using static Roboya.Tests.CodingEngine.HoneyLevels;

namespace Roboya.Tests.CodingEngine
{
    public class PerformanceBudgetTests
    {
        // The heaviest shapes of docs/pedagoji/paylasim-koyu-taslagi.md (levels 12, 17, 23, 24, a Minik auto-drop level) kept
        // within PK-0003 (shortest solution <= 10 cards) but on boards at least as roomy as the village boards will be.
        private static Level Corner12() =>
            Build(new[] { "RABC...", ".......", ".......", "......." }, needs: new[] { 1, 2, 3 }, carrying: 6, maxLength: 11, collectAll: false,
                deliver: new[] { ('A', 1), ('B', 2), ('C', 3) });

        private static Level Blocked17() =>
            Build(new[] { "R.A..", ".#.#.", "..B..", ".##.." }, needs: new[] { 2, 2 }, carrying: 4, maxLength: 11, collectAll: false, shareAmong: "AB");

        private static Level Mixed23() =>
            Build(new[] { "R2ABT.", "......", "......", "......" }, needs: new[] { 2, 2 }, carrying: 3, maxLength: 11, shareAmong: "AB", leftover: 'T');

        private static Level Final24() =>
            Build(new[] { "RABCTG", "......", "......", "......" }, needs: new[] { 1, 1, 1 }, carrying: 4, maxLength: 11, collectAll: false, reachG: true,
                shareAmong: "ABC", leftover: 'T');

        private static Level Minik() =>
            Build(new[] { "R2A....", "......." }, needs: new[] { 2 }, autoDrop: true, cards: new[] { CardType.Forward, CardType.TurnRight }, maxLength: 6, deliver: new[] { ('A', 2) });

        private static readonly (string Name, System.Func<Level> Make, int ShortestLength)[] Heavy =
        {
            ("12", Corner12, 9), ("17", Blocked17, 9), ("23", Mixed23, 9), ("24", Final24, 9), ("minik", Minik, 2),
        };

        // Far roomier than any planned board (8 x 8 open, seven honey, three hives and a table, 12 slots).
        private static Level Stress() =>
            Build(new[] { "RABCT...", "........", "........", "........", "........", "........", "........", "........" },
                needs: new[] { 2, 2, 2 }, carrying: 7, maxLength: 12, collectAll: false, shareAmong: "ABC", leftover: 'T');

        // Measured on the desktop when B3 was written (states / full-plan hint): 12: 954 / 6 ms, 17: 211 / 1 ms, 23: 801 / 3 ms,
        // 24: 814 / 3 ms, stress board: 4 473 / 19 ms. The caps below are about 5x the measured counts of the real shapes and the
        // plan's 20 000-state limit for the stress board: crossing one means the search needs the reachable-graph cache (plan B3).
        private const int StateCap = 5000;
        private const int StressStateCap = 20000;

        [Test]
        public void Solve_HeaviestVillageShapes_StayWithinTheStateCap()
        {
            foreach (var (name, make, shortest) in Heavy)
            {
                var result = Solver.Solve(make());

                Assert.AreEqual(SolveStatus.Solved, result.Status, name);
                Assert.AreEqual(shortest, result.ShortestLength, name);
                Assert.LessOrEqual(result.ShortestLength, 10, name + " must stay within the Kaşif card limit (PK-0003)");
                Assert.LessOrEqual(result.StatesExplored, StateCap, name);
            }
        }

        [Test]
        public void Solve_OversizedOpenBoard_StaysWithinTheStressCap()
        {
            var result = Solver.Solve(Stress());

            Assert.AreEqual(SolveStatus.Solved, result.Status);
            Assert.LessOrEqual(result.StatesExplored, StressStateCap);
        }

        [Test]
        public void Analyze_FullCorrectPlans_AreFastAndFindNothingWrong()
        {
            foreach (var (name, make, _) in Heavy.Concat(new (string, System.Func<Level>, int)[] { ("stress", Stress, 0) }))
            {
                var level = make();
                var plan = Solver.Solve(level).Solution;

                var sw = Stopwatch.StartNew();
                var hint = HintAdvisor.Analyze(level, plan);
                sw.Stop();

                TestContext.Progress.WriteLine($"{name}: full-plan hint {sw.ElapsedMilliseconds} ms");
                Assert.IsFalse(hint.HasHint, name);
                Assert.Less(sw.ElapsedMilliseconds, 2000, name + ": gross regression guard, the real cap is the state count");
            }
        }
    }
}
