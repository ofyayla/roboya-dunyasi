using System.Collections.Generic;

namespace Roboya.CodingEngine.Progress
{
    public enum NodeState
    {
        /// <summary>Finished at least once; can be replayed (ILR-02).</summary>
        Completed,

        /// <summary>The next level to play.</summary>
        Current,

        /// <summary>An earlier level is not finished yet.</summary>
        Locked,

        /// <summary>Beyond the free tier without premium: Roboya asks the child to ask a grown-up (GLR-01).</summary>
        NeedsGrownUp,
    }

    /// <summary>
    /// Which levels on a region path can be opened. The client never grants premium itself (CLAUDE.md golden
    /// rule 3): <c>hasPremium</c> comes from the server-backed entitlement source.
    /// </summary>
    public static class PathRules
    {
        public static NodeState[] StatesOf(IReadOnlyList<string> path, ProgressBook book, int freeLevelCount, bool hasPremium)
        {
            var states = new NodeState[path.Count];
            bool previousDone = true;
            bool currentGiven = false;
            for (int i = 0; i < path.Count; i++)
            {
                bool done = book.IsCompleted(path[i]);
                // Beyond the free tier the grown-up gate applies even to finished levels (e.g. premium lapsed);
                // the stars stay in the book and return with premium.
                bool paid = i >= freeLevelCount && !hasPremium;
                if (paid)
                {
                    states[i] = NodeState.NeedsGrownUp;
                }
                else if (done)
                {
                    states[i] = NodeState.Completed;
                }
                else if (previousDone && !currentGiven)
                {
                    states[i] = NodeState.Current;
                    currentGiven = true;
                }
                else
                {
                    states[i] = NodeState.Locked;
                }

                previousDone = done;
            }

            return states;
        }

        /// <summary>True when the level at <paramref name="index"/> may be opened now.</summary>
        public static bool CanPlay(NodeState state) => state == NodeState.Completed || state == NodeState.Current;
    }
}
