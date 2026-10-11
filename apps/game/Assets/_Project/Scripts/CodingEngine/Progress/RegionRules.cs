using System.Collections.Generic;

namespace Roboya.CodingEngine.Progress
{
    /// <summary>
    /// Which island regions can be entered (PRD "İlerleme yapısı": regions open in order). Product decision
    /// 2026-10-11 (A2, A3): a region opens once the region before it is finished, and "finished" means its last
    /// level (the region finale) has a star. Only the finale counts because an older child starts mid-path
    /// (ADR 0018) and never has to replay the younger levels to move on. Regions without levels never open and
    /// never block the ones after them. Premium is not decided here: inside a region the path's own rules apply.
    /// </summary>
    public static class RegionRules
    {
        /// <param name="pathsInOrder">Each region's ordered level ids, in island order; empty when it has no content yet.</param>
        /// <param name="unlockAll">Development builds only (ADR 0009): every region with content opens.</param>
        public static bool[] OpenRegions(IReadOnlyList<IReadOnlyList<string>> pathsInOrder, ProgressBook book, bool unlockAll = false)
        {
            var open = new bool[pathsInOrder.Count];
            bool previousFinished = true;
            for (int i = 0; i < pathsInOrder.Count; i++)
            {
                var path = pathsInOrder[i];
                if (path == null || path.Count == 0)
                {
                    continue;
                }

                open[i] = unlockAll || previousFinished;
                previousFinished = previousFinished && IsFinished(path, book);
            }

            return open;
        }

        /// <summary>
        /// The free tier belongs to the first region of the island only (product decision 2026-10-11, plan C1): the other regions
        /// are premium content, so they have no free levels. The island order is content (content/map/island.json).
        /// </summary>
        public static int FreeLevelCountFor(int islandIndex, int freeLevelCount) => islandIndex == 0 ? freeLevelCount : 0;

        /// <summary>A region is finished when its finale, the last level on its path, has at least one star.</summary>
        public static bool IsFinished(IReadOnlyList<string> path, ProgressBook book) =>
            path != null && path.Count > 0 && book.IsCompleted(path[path.Count - 1]);
    }
}
