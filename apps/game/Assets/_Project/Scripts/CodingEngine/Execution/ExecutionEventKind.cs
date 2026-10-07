namespace Roboya.CodingEngine.Execution
{
    public enum ExecutionEventKind
    {
        /// <summary>A command became active (OYN-03 highlight). Emitted before its effect.</summary>
        CommandStarted,
        Moved,
        Turned,

        /// <summary>The robot tried to enter a blocked or outside cell; it stays in place and execution stops.</summary>
        Bumped,
        Collected,
        ActionPerformed,

        /// <summary>A repeat body starts iteration <see cref="ExecutionEvent.Iteration"/> (1-based).</summary>
        LoopIteration,

        /// <summary>Always the last event; carries <see cref="ExecutionEvent.Result"/>.</summary>
        Finished,
    }
}
