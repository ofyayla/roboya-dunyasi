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
    }
}
