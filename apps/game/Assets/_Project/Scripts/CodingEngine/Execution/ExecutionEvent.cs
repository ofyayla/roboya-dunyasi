using System.Collections.Generic;
using Roboya.CodingEngine.World;

namespace Roboya.CodingEngine.Execution
{
    /// <summary>
    /// One observable step of execution, consumed by the view to animate.
    /// <see cref="CommandPath"/> addresses the active card: top-level index, then for a repeat the body index,
    /// for an if the branch (0 = then, 1 = else) followed by the index, and for a call the index in the procedure.
    /// </summary>
    public sealed class ExecutionEvent
    {
        internal ExecutionEvent(
            ExecutionEventKind kind,
            IReadOnlyList<int> commandPath,
            RobotState before,
            RobotState after,
            int itemIndex = -1,
            string actionId = null,
            int iteration = 0,
            ExecutionResult result = null)
        {
            Kind = kind;
            CommandPath = commandPath;
            Before = before;
            After = after;
            ItemIndex = itemIndex;
            ActionId = actionId;
            Iteration = iteration;
            Result = result;
        }

        public ExecutionEventKind Kind { get; }

        public IReadOnlyList<int> CommandPath { get; }

        public RobotState Before { get; }

        public RobotState After { get; }

        public int ItemIndex { get; }

        public string ActionId { get; }

        public int Iteration { get; }

        public ExecutionResult Result { get; }
    }
}
