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

        /// <summary>One honey left the basket for a drop point (<see cref="ExecutionEvent.DropIndex"/>), by the card or by a hive taking it.</summary>
        Dropped,

        /// <summary>"Bırak" with an empty basket or off a drop point; execution stops on that card.</summary>
        NothingToDrop,

        /// <summary>A hive got more than it needs; the honey is shown spilling and execution stops on that card.</summary>
        HiveOverflow,

        /// <summary>A repeat body starts iteration <see cref="ExecutionEvent.Iteration"/> (1-based).</summary>
        LoopIteration,

        /// <summary>Always the last event; carries <see cref="ExecutionEvent.Result"/>.</summary>
        Finished,
    }
}
