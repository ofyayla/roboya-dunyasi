using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.Execution;
using Roboya.CodingEngine.World;
using static Roboya.Tests.CodingEngine.TestLevels;

namespace Roboya.Tests.CodingEngine
{
    public class InterpreterTests
    {
        // R at bottom-left facing north, G at top-right.
        private static readonly string[] Corner = { "..G", "...", "R.." };

        [Test]
        public void Execute_CorrectPlan_Succeeds()
        {
            var result = Interpreter.Execute(Build(Corner), Prog(F, F, Rt, F, F));

            Assert.AreEqual(ExecutionOutcome.Success, result.Outcome);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(new GridPosition(2, 0), result.FinalState.Position);
            Assert.AreEqual(5, result.StepsExecuted);
            Assert.IsNull(result.ErrorCommandPath);
        }

        [Test]
        public void Execute_Wall_StopsAtBumpAndReportsCard()
        {
            var result = Interpreter.Execute(Build(Corner), Prog(F, F, F, Rt, F));

            Assert.AreEqual(ExecutionOutcome.Bumped, result.Outcome);
            CollectionAssert.AreEqual(new[] { 2 }, result.ErrorCommandPath);
            Assert.AreEqual(new GridPosition(0, 0), result.FinalState.Position);
            Assert.AreEqual(3, result.StepsExecuted);
        }

        [Test]
        public void Execute_BlockedCell_Bumps()
        {
            var level = Build(new[] { "G#", "R." }, Direction.East);

            var result = Interpreter.Execute(level, Prog(F, L, F));

            Assert.AreEqual(ExecutionOutcome.Bumped, result.Outcome);
            CollectionAssert.AreEqual(new[] { 2 }, result.ErrorCommandPath);
        }

        [Test]
        public void Execute_PlanTooShort_EndsAwayFromGoal()
        {
            var result = Interpreter.Execute(Build(Corner), Prog(F));

            Assert.AreEqual(ExecutionOutcome.EndedAwayFromGoal, result.Outcome);
        }

        [Test]
        public void Execute_PassesGoalAndContinues_Fails()
        {
            var level = Build(new[] { "...", ".G.", ".R." });

            Assert.AreEqual(ExecutionOutcome.EndedAwayFromGoal, Interpreter.Execute(level, Prog(F, F)).Outcome);
        }

        [Test]
        public void Execute_Backward_MovesOpposite()
        {
            var level = Build(new[] { "R.", "G." });

            var result = Interpreter.Execute(level, Prog(B));

            Assert.AreEqual(ExecutionOutcome.Success, result.Outcome);
        }

        [Test]
        public void Execute_ItemsMissing_ReportsMissingItems()
        {
            // The flower sits off the route, so reaching G alone is not enough.
            var level = Build(new[] { "..G", "...", "R.a" });

            var result = Interpreter.Execute(level, Prog(F, F, Rt, F, F));

            Assert.AreEqual(ExecutionOutcome.MissingItems, result.Outcome);
        }

        [Test]
        public void Execute_CollectsItems_Succeeds()
        {
            var result = Interpreter.Execute(Build(new[] { "aG", "R." }), Prog(F, Rt, F));

            Assert.AreEqual(ExecutionOutcome.Success, result.Outcome);
            Assert.IsTrue(result.FinalState.HasCollected(0));
        }

        [Test]
        public void Execute_ItemsOnlyGoal_SucceedsWithoutGoalCell()
        {
            var level = Build(new[] { "a.", "R." }, requireGoalCell: false);

            Assert.AreEqual(ExecutionOutcome.Success, Interpreter.Execute(level, Prog(F)).Outcome);
        }

        [Test]
        public void Run_Collect_EmitsCollectedOnceForRevisit()
        {
            var level = Build(new[] { ".a", "RG" }, Direction.East, collectAll: true);
            var program = Prog(L, F, Rt, F, Rt, F, Rt, F, Rt, F);

            var events = new Interpreter(level, program).Run().ToList();

            Assert.AreEqual(1, events.Count(e => e.Kind == ExecutionEventKind.Collected));
            var collected = events.First(e => e.Kind == ExecutionEventKind.Collected);
            Assert.AreEqual(0, collected.ItemIndex);
            Assert.IsTrue(collected.After.HasCollected(0));
        }

        [Test]
        public void Run_Events_HighlightEachCardBeforeEffect()
        {
            var events = new Interpreter(Build(Corner), Prog(F, Rt)).Run().ToList();
            var kinds = events.Select(e => e.Kind).ToArray();

            CollectionAssert.AreEqual(
                new[]
                {
                    ExecutionEventKind.CommandStarted, ExecutionEventKind.Moved,
                    ExecutionEventKind.CommandStarted, ExecutionEventKind.Turned,
                    ExecutionEventKind.Finished,
                },
                kinds);
            CollectionAssert.AreEqual(new[] { 1 }, events[2].CommandPath);
            Assert.AreEqual(Direction.East, events[3].After.Facing);
            Assert.IsNotNull(events.Last().Result);
        }

        [Test]
        public void Run_Trail_RecordsVisitedCells()
        {
            var result = Interpreter.Execute(Build(Corner), Prog(F, Rt, F));

            CollectionAssert.AreEqual(
                new[] { new GridPosition(0, 2), new GridPosition(0, 1), new GridPosition(1, 1) },
                result.Trail);
        }

        [Test]
        public void Run_Repeat_RunsBodyAndReportsNestedPaths()
        {
            var program = new Program(new Command[]
            {
                new RepeatCommand(2, new Command[] { MoveCommand.Forward }),
                MoveCommand.TurnRight,
                new RepeatCommand(2, new Command[] { MoveCommand.Forward }),
            });

            var events = new Interpreter(Build(Corner), program).Run().ToList();
            var result = events.Last().Result;

            Assert.AreEqual(ExecutionOutcome.Success, result.Outcome);
            Assert.AreEqual(4, events.Count(e => e.Kind == ExecutionEventKind.LoopIteration));
            Assert.AreEqual(2, events.First(e => e.Kind == ExecutionEventKind.LoopIteration && e.Iteration == 2).Iteration);
            CollectionAssert.AreEqual(new[] { 2, 0 }, events.Where(e => e.Kind == ExecutionEventKind.Moved).Last().CommandPath);
        }

        [Test]
        public void Run_RepeatBump_StopsRemainingIterations()
        {
            var program = new Program(new Command[] { new RepeatCommand(5, new Command[] { MoveCommand.Forward }) });

            var result = Interpreter.Execute(Build(Corner), program);

            Assert.AreEqual(ExecutionOutcome.Bumped, result.Outcome);
            CollectionAssert.AreEqual(new[] { 0, 0 }, result.ErrorCommandPath);
            Assert.AreEqual(3, result.StepsExecuted);
        }

        [Test]
        public void Run_If_TakesBranchBySensor()
        {
            // Facing a wall at the top edge: blocked -> turn right; then forward twice.
            var level = Build(new[] { "R.G", "..." });
            var program = new Program(new Command[]
            {
                new IfCommand(PathBlockedAhead.Instance, new Command[] { MoveCommand.TurnRight }, new Command[] { MoveCommand.TurnLeft }),
                MoveCommand.Forward,
                MoveCommand.Forward,
            });

            var events = new Interpreter(level, program).Run().ToList();

            Assert.AreEqual(ExecutionOutcome.Success, events.Last().Result.Outcome);
            CollectionAssert.AreEqual(new[] { 0, 0, 0 }, events.First(e => e.Kind == ExecutionEventKind.Turned).CommandPath);
        }

        [Test]
        public void Run_IfElse_TakesElseBranch()
        {
            var level = Build(new[] { "...", "R.G" }, Direction.East);
            var program = new Program(new Command[]
            {
                new IfCommand(new Not(new OnItemColor("red")), new Command[] { MoveCommand.Forward }, new Command[] { MoveCommand.TurnLeft }),
                new IfCommand(PathBlockedAhead.Instance, new Command[] { MoveCommand.TurnLeft }, new Command[] { MoveCommand.Forward }),
            });

            var events = new Interpreter(level, program).Run().ToList();

            Assert.AreEqual(ExecutionOutcome.Success, events.Last().Result.Outcome);
            CollectionAssert.AreEqual(new[] { 1, 1, 0 }, events.Where(e => e.Kind == ExecutionEventKind.Moved).Last().CommandPath);
        }

        [Test]
        public void OnItemColor_MatchingItem_IsTrue()
        {
            var level = Build(new[] { "y.", "Ra" }, requireGoalCell: false);
            var onYellow = new RobotState(new GridPosition(0, 0), Direction.North, 0);
            var onRed = new RobotState(new GridPosition(1, 1), Direction.North, 0);
            var onNothing = new RobotState(new GridPosition(1, 0), Direction.North, 0);

            Assert.IsTrue(new OnItemColor("yellow").Evaluate(level, onYellow));
            Assert.IsFalse(new OnItemColor("yellow").Evaluate(level, onRed));
            Assert.IsFalse(new OnItemColor("yellow").Evaluate(level, onNothing));
            Assert.AreEqual("yellow", new OnItemColor("yellow").Color);
        }

        [Test]
        public void Run_CallProcedure_RunsBody()
        {
            var procedures = new Dictionary<string, IReadOnlyList<Command>>
            {
                ["iki-ileri"] = new Command[] { MoveCommand.Forward, MoveCommand.Forward },
            };
            var program = new Program(
                new Command[] { new CallCommand("iki-ileri"), MoveCommand.TurnRight, new CallCommand("iki-ileri") },
                procedures);

            Assert.AreEqual(ExecutionOutcome.Success, Interpreter.Execute(Build(Corner), program).Outcome);
        }

        [Test]
        public void Run_UnknownProcedure_FailsAtCallCard()
        {
            var program = new Program(new Command[] { MoveCommand.Forward, new CallCommand("yok") });

            var result = Interpreter.Execute(Build(Corner), program);

            Assert.AreEqual(ExecutionOutcome.UnknownProcedure, result.Outcome);
            CollectionAssert.AreEqual(new[] { 1 }, result.ErrorCommandPath);
        }

        [Test]
        public void Run_RecursiveProcedure_StopsAtDepthLimit()
        {
            var procedures = new Dictionary<string, IReadOnlyList<Command>>
            {
                ["sonsuz"] = new Command[] { MoveCommand.TurnLeft, new CallCommand("sonsuz") },
            };

            var result = Interpreter.Execute(Build(Corner), new Program(new Command[] { new CallCommand("sonsuz") }, procedures));

            Assert.AreEqual(ExecutionOutcome.StepLimitExceeded, result.Outcome);
        }

        [Test]
        public void Run_StepLimit_StopsLongPrograms()
        {
            var program = new Program(new Command[]
            {
                new RepeatCommand(9, new Command[] { new RepeatCommand(9, new Command[] { MoveCommand.TurnLeft }) }),
            });

            var result = Interpreter.Execute(Build(Corner), program, stepLimit: 50);

            Assert.AreEqual(ExecutionOutcome.StepLimitExceeded, result.Outcome);
            Assert.AreEqual(50, result.StepsExecuted);
        }

        [Test]
        public void Run_StepLimitOnAction_Stops()
        {
            var program = new Program(new Command[] { new ActionCommand("alkis"), new ActionCommand("alkis") });

            Assert.AreEqual(ExecutionOutcome.StepLimitExceeded, Interpreter.Execute(Build(Corner), program, stepLimit: 1).Outcome);
        }

        [Test]
        public void Run_Action_EmitsActionWithoutMoving()
        {
            var events = new Interpreter(Build(Corner), new Program(new Command[] { new ActionCommand("zipla") })).Run().ToList();
            var action = events.Single(e => e.Kind == ExecutionEventKind.ActionPerformed);

            Assert.AreEqual("zipla", action.ActionId);
            Assert.AreEqual(action.Before, action.After);
        }

        [Test]
        public void Run_FromState_ContinuesFromGivenPosition()
        {
            var level = Build(Corner);
            var from = new RobotState(new GridPosition(2, 2), Direction.North, 0);

            var events = new Interpreter(level, Prog(F, F)).Run(from).ToList();

            Assert.AreEqual(ExecutionOutcome.Success, events.Last().Result.Outcome);
        }

        [Test]
        public void Run_EnumeratedTwice_IsIndependent()
        {
            var interpreter = new Interpreter(Build(Corner), Prog(F, F, Rt, F, F));

            var first = interpreter.Run().Last().Result;
            var second = interpreter.Run().Last().Result;

            Assert.AreEqual(first.Outcome, second.Outcome);
            Assert.AreEqual(first.Trail.Count, second.Trail.Count);
        }

        [Test]
        public void Run_UnknownCommandType_Throws()
        {
            var program = new Program(new Command[] { new FakeCommand() });

            Assert.Throws<NotSupportedException>(() => Interpreter.Execute(Build(Corner), program));
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new Interpreter(null, Prog(F)));
            Assert.Throws<ArgumentNullException>(() => new Interpreter(Build(Corner), null));
        }

        private sealed class FakeCommand : Command
        {
            public FakeCommand()
                : base(CardType.Action)
            {
            }

            public override int CardCount => 1;
        }
    }
}
