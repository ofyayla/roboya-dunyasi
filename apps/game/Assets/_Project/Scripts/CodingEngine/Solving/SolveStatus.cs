namespace Roboya.CodingEngine.Solving
{
    public enum SolveStatus
    {
        Solved,

        /// <summary>No sequence of available move cards reaches the goal within the plan length.</summary>
        Unsolvable,

        /// <summary>The search hit its state budget; treat the level as invalid until simplified.</summary>
        SearchLimitReached,
    }
}
