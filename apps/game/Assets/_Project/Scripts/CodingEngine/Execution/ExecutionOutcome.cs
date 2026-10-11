namespace Roboya.CodingEngine.Execution
{
    public enum ExecutionOutcome
    {
        Success,
        Bumped,
        EndedAwayFromGoal,
        MissingItems,
        StepLimitExceeded,
        UnknownProcedure,

        /// <summary>"Bırak" with nothing to drop here (ADR 0030).</summary>
        NothingToDrop,

        /// <summary>A hive was given more honey than it needs.</summary>
        HiveOverflow,

        /// <summary>Items and position are right but the delivery, share or basket count is not.</summary>
        HoneyGoalNotMet,
    }
}
