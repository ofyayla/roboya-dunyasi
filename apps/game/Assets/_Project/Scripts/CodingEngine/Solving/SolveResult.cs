using System.Collections.Generic;
using Roboya.CodingEngine.Commands;

namespace Roboya.CodingEngine.Solving
{
    public sealed class SolveResult
    {
        internal SolveResult(SolveStatus status, IReadOnlyList<CardType> solution, int statesExplored)
        {
            Status = status;
            Solution = solution;
            StatesExplored = statesExplored;
        }

        public SolveStatus Status { get; }

        public bool IsSolved => Status == SolveStatus.Solved;

        /// <summary>One shortest sequence of primitive move cards; empty unless solved.</summary>
        public IReadOnlyList<CardType> Solution { get; }

        /// <summary>Length of the shortest solution in primitive cards, or -1.</summary>
        public int ShortestLength => IsSolved ? Solution.Count : -1;

        public int StatesExplored { get; }
    }
}
