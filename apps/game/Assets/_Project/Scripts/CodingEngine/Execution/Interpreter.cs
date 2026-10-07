using System;
using System.Collections.Generic;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.World;

namespace Roboya.CodingEngine.Execution
{
    /// <summary>
    /// Executes a program on a level. Execution is lazy: <see cref="Run"/> yields one event at a time so the
    /// view can animate each step. The whole plan runs at once (YON-02 sabır mekaniği); there is no single-step
    /// play from the child's side, but the view paces the events.
    /// </summary>
    public sealed class Interpreter
    {
        /// <summary>Guards against runaway programs (deep recursion through procedures).</summary>
        public const int DefaultStepLimit = 500;

        private const int MaxCallDepth = 16;

        private readonly Level _level;
        private readonly Program _program;
        private readonly int _stepLimit;

        private RobotState _state;
        private List<GridPosition> _trail;
        private int _steps;
        private ExecutionOutcome? _failure;
        private IReadOnlyList<int> _errorPath;
        private int _callDepth;

        public Interpreter(Level level, Program program, int stepLimit = DefaultStepLimit)
        {
            _level = level ?? throw new ArgumentNullException(nameof(level));
            _program = program ?? throw new ArgumentNullException(nameof(program));
            _stepLimit = stepLimit;
        }

        /// <summary>Runs to completion and returns only the result.</summary>
        public static ExecutionResult Execute(Level level, Program program, int stepLimit = DefaultStepLimit)
        {
            ExecutionResult result = null;
            foreach (var e in new Interpreter(level, program, stepLimit).Run())
            {
                result = e.Result ?? result;
            }

            return result;
        }

        /// <summary>Starts a fresh execution from the level start. Each enumeration is independent.</summary>
        public IEnumerable<ExecutionEvent> Run() => Run(_level.Start);

        /// <summary>Starts execution from an arbitrary state (Bal Peşinde keeps the robot where it stopped).</summary>
        public IEnumerable<ExecutionEvent> Run(RobotState from)
        {
            _state = from;
            _trail = new List<GridPosition> { from.Position };
            _steps = 0;
            _failure = null;
            _errorPath = null;
            _callDepth = 0;

            foreach (var e in RunBlock(_program.Main, Array.Empty<int>()))
            {
                yield return e;
            }

            var outcome = _failure ?? EvaluateGoal();
            var result = new ExecutionResult(outcome, _state, _errorPath, _steps, _trail.AsReadOnly());
            yield return new ExecutionEvent(ExecutionEventKind.Finished, _errorPath ?? Array.Empty<int>(), _state, _state, result: result);
        }

        private ExecutionOutcome EvaluateGoal()
        {
            if (_level.Goal.IsSatisfiedBy(_state))
            {
                return ExecutionOutcome.Success;
            }

            var mask = _level.Goal.MustCollectMask;
            return (_state.CollectedMask & mask) != mask ? ExecutionOutcome.MissingItems : ExecutionOutcome.EndedAwayFromGoal;
        }

        private IEnumerable<ExecutionEvent> RunBlock(IReadOnlyList<Command> block, int[] prefix)
        {
            for (int i = 0; i < block.Count && _failure == null; i++)
            {
                var path = Append(prefix, i);
                foreach (var e in RunCommand(block[i], path))
                {
                    yield return e;
                }
            }
        }

        private IEnumerable<ExecutionEvent> RunCommand(Command command, int[] path)
        {
            yield return new ExecutionEvent(ExecutionEventKind.CommandStarted, path, _state, _state);

            switch (command)
            {
                case MoveCommand move:
                    foreach (var e in RunMove(move, path))
                    {
                        yield return e;
                    }

                    break;

                case RepeatCommand repeat:
                    for (int n = 1; n <= repeat.Times && _failure == null; n++)
                    {
                        yield return new ExecutionEvent(ExecutionEventKind.LoopIteration, path, _state, _state, iteration: n);
                        foreach (var e in RunBlock(repeat.Body, path))
                        {
                            yield return e;
                        }
                    }

                    break;

                case IfCommand branch:
                    bool holds = branch.Condition.Evaluate(_level, _state);
                    var body = holds ? branch.Then : branch.Else;
                    foreach (var e in RunBlock(body, Append(path, holds ? 0 : 1)))
                    {
                        yield return e;
                    }

                    break;

                case CallCommand call:
                    if (!_program.Procedures.TryGetValue(call.Procedure, out var procedure) || _callDepth >= MaxCallDepth)
                    {
                        Fail(_callDepth >= MaxCallDepth ? ExecutionOutcome.StepLimitExceeded : ExecutionOutcome.UnknownProcedure, path);
                        break;
                    }

                    _callDepth++;
                    foreach (var e in RunBlock(procedure, path))
                    {
                        yield return e;
                    }

                    _callDepth--;
                    break;

                case ActionCommand action:
                    if (CountStep(path))
                    {
                        yield return new ExecutionEvent(ExecutionEventKind.ActionPerformed, path, _state, _state, actionId: action.ActionId);
                    }

                    break;

                default:
                    throw new NotSupportedException("Unknown command type " + command.GetType().Name);
            }
        }

        private IEnumerable<ExecutionEvent> RunMove(MoveCommand move, int[] path)
        {
            if (!CountStep(path))
            {
                yield break;
            }

            var before = _state;
            switch (move.Card)
            {
                case CardType.TurnLeft:
                    _state = _state.With(_state.Facing.TurnLeft());
                    yield return new ExecutionEvent(ExecutionEventKind.Turned, path, before, _state);
                    yield break;

                case CardType.TurnRight:
                    _state = _state.With(_state.Facing.TurnRight());
                    yield return new ExecutionEvent(ExecutionEventKind.Turned, path, before, _state);
                    yield break;
            }

            var direction = move.Card == CardType.Forward ? _state.Facing : _state.Facing.Opposite();
            var target = _state.Position.Step(direction);
            if (!_level.Grid.IsWalkable(target))
            {
                Fail(ExecutionOutcome.Bumped, path);
                yield return new ExecutionEvent(ExecutionEventKind.Bumped, path, before, _state);
                yield break;
            }

            _state = _state.With(target);
            _trail.Add(target);
            yield return new ExecutionEvent(ExecutionEventKind.Moved, path, before, _state);

            int item = _level.ItemIndexAt(target);
            if (item >= 0 && !_state.HasCollected(item))
            {
                var beforeCollect = _state;
                _state = _state.WithCollected(item);
                yield return new ExecutionEvent(ExecutionEventKind.Collected, path, beforeCollect, _state, itemIndex: item);
            }
        }

        private bool CountStep(int[] path)
        {
            if (_steps >= _stepLimit)
            {
                Fail(ExecutionOutcome.StepLimitExceeded, path);
                return false;
            }

            _steps++;
            return true;
        }

        private void Fail(ExecutionOutcome outcome, int[] path)
        {
            _failure = outcome;
            _errorPath = path;
        }

        private static int[] Append(int[] prefix, int index)
        {
            var path = new int[prefix.Length + 1];
            Array.Copy(prefix, path, prefix.Length);
            path[prefix.Length] = index;
            return path;
        }
    }
}
