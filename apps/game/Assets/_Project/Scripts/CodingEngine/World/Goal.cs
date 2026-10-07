using System;
using System.Collections.Generic;

namespace Roboya.CodingEngine.World
{
    /// <summary>
    /// Success condition, evaluated when the program ends: the robot stands on <see cref="Reach"/> (if set)
    /// and every item index in <see cref="MustCollect"/> has been collected.
    /// </summary>
    public sealed class Goal
    {
        public Goal(GridPosition? reach, IReadOnlyList<int> mustCollect)
        {
            Reach = reach;
            MustCollect = mustCollect ?? Array.Empty<int>();
            ulong mask = 0;
            foreach (int index in MustCollect)
            {
                mask |= 1UL << index;
            }

            MustCollectMask = mask;
        }

        public GridPosition? Reach { get; }

        public IReadOnlyList<int> MustCollect { get; }

        public ulong MustCollectMask { get; }

        public bool IsSatisfiedBy(RobotState state)
        {
            bool reached = !Reach.HasValue || state.Position == Reach.Value;
            return reached && (state.CollectedMask & MustCollectMask) == MustCollectMask;
        }
    }
}
