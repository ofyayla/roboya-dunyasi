using System.Collections.Generic;
using Roboya.CodingEngine.World;

namespace Roboya.CodingEngine.Execution
{
    public sealed class ExecutionResult
    {
        internal ExecutionResult(
            ExecutionOutcome outcome,
            RobotState finalState,
            IReadOnlyList<int> errorCommandPath,
            int stepsExecuted,
            IReadOnlyList<GridPosition> trail)
        {
            Outcome = outcome;
            FinalState = finalState;
            ErrorCommandPath = errorCommandPath;
            StepsExecuted = stepsExecuted;
            Trail = trail;
        }

        public ExecutionOutcome Outcome { get; }

        public bool IsSuccess => Outcome == ExecutionOutcome.Success;

        public RobotState FinalState { get; }

        /// <summary>The card where execution went wrong (bump, unknown procedure); null otherwise.</summary>
        public IReadOnlyList<int> ErrorCommandPath { get; }

        /// <summary>Executed effectful commands (moves, turns, actions).</summary>
        public int StepsExecuted { get; }

        /// <summary>Cells the robot occupied, starting with the start cell (KUT-02 trail).</summary>
        public IReadOnlyList<GridPosition> Trail { get; }
    }
}
