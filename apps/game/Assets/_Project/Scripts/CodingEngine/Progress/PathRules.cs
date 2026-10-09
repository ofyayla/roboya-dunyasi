using System.Collections.Generic;
using Roboya.CodingEngine.Profiles;

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

        /// <summary>Before the child's age-based start and not finished: open to play, but not "the next one" (premium only).</summary>
        Open,

        /// <summary>Beyond the free tier without premium: Roboya asks the child to ask a grown-up (GLR-01).</summary>
        NeedsGrownUp,
    }

    /// <summary>
    /// Which levels on a region path can be opened. The client never grants premium itself (CLAUDE.md golden
    /// rule 3): <c>hasPremium</c> comes from the server-backed entitlement source.
    /// </summary>
    public static class PathRules
    {
        /// <summary>
        /// The first level of the path meant for <paramref name="band"/> (F1-08, PRD age levels). When no level names the
        /// band (Mucit content arrives later) the nearest younger band is used; otherwise the path begins at its start.
        /// </summary>
        public static int StartIndex(IReadOnlyList<IReadOnlyCollection<AgeBand>> bandsPerLevel, AgeBand band)
        {
            for (int b = (int)band; b >= 0; b--)
            {
                for (int i = 0; i < bandsPerLevel.Count; i++)
                {
                    foreach (var named in bandsPerLevel[i])
                    {
                        if (named == (AgeBand)b)
                        {
                            return i;
                        }
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// <paramref name="startIndex"/> is where the child's age puts them. The free tier is the first
        /// <paramref name="freeLevelCount"/> levels from there, so an older child's free levels are the ones meant for them.
        /// <paramref name="intro"/> marks introduction levels (a game's first level, a level that introduces a card): when the
        /// child starts after them they are still played first, free of charge, so no child meets a mechanic untaught.
        /// </summary>
        public static NodeState[] StatesOf(
            IReadOnlyList<string> path,
            ProgressBook book,
            int freeLevelCount,
            bool hasPremium,
            int startIndex = 0,
            bool unlockAll = false,
            IReadOnlyList<bool> intro = null)
        {
            if (unlockAll)
            {
                return AllOpen(path, book);
            }

            var states = new NodeState[path.Count];
            bool pendingIntro = false;
            for (int i = 0; i < startIndex && i < path.Count; i++)
            {
                pendingIntro |= IsIntro(intro, i) && !book.IsCompleted(path[i]);
            }

            bool previousDone = !pendingIntro;
            bool currentGiven = false;
            for (int i = 0; i < path.Count; i++)
            {
                bool done = book.IsCompleted(path[i]);
                bool earlyIntro = i < startIndex && IsIntro(intro, i);
                // Beyond the free tier the grown-up gate applies even to finished levels (e.g. premium lapsed);
                // the stars stay in the book and return with premium.
                bool paid = ((i < startIndex && !earlyIntro) || i >= startIndex + freeLevelCount) && !hasPremium;
                if (earlyIntro && !done)
                {
                    // Tutorials before the age start come first: the earliest unfinished one is "the next".
                    states[i] = !currentGiven ? NodeState.Current : NodeState.Open;
                    currentGiven = true;
                    continue;
                }

                if (i < startIndex && !paid && !done)
                {
                    states[i] = NodeState.Open;
                    continue;
                }

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

                previousDone = i < startIndex || done;
            }

            return states;
        }

        private static bool IsIntro(IReadOnlyList<bool> intro, int index) =>
            intro != null && index < intro.Count && intro[index];

        private static NodeState[] AllOpen(IReadOnlyList<string> path, ProgressBook book)
        {
            var states = new NodeState[path.Count];
            bool currentGiven = false;
            for (int i = 0; i < path.Count; i++)
            {
                if (book.IsCompleted(path[i]))
                {
                    states[i] = NodeState.Completed;
                }
                else if (!currentGiven)
                {
                    states[i] = NodeState.Current;
                    currentGiven = true;
                }
                else
                {
                    states[i] = NodeState.Open;
                }
            }

            return states;
        }

        /// <summary>True when the level at <paramref name="index"/> may be opened now.</summary>
        public static bool CanPlay(NodeState state) =>
            state == NodeState.Completed || state == NodeState.Current || state == NodeState.Open;
    }
}
