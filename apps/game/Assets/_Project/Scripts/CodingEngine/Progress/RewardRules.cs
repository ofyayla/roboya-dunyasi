using System;
using System.Collections.Generic;

namespace Roboya.CodingEngine.Progress
{
    /// <summary>A repair part for Roboya's ship, from content/rewards/ship-parts.json.</summary>
    public sealed class ShipPart
    {
        public ShipPart(string id, int order)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Order = order;
        }

        public string Id { get; }

        /// <summary>1-based position in the fixed earning order.</summary>
        public int Order { get; }
    }

    /// <summary>
    /// Ship parts (ILR-03; product decision A4, 2026-10-11): each region gives one part per corner, small ones at the
    /// ends of the first three corners and a big one on the region finale, so the ship is whole after the five value
    /// regions. A corner counts once the child has finished a level at or beyond its end: an older child who starts
    /// mid-path (ADR 0018) earns the corners they walked past. The order is fixed and known in advance: no surprise
    /// boxes or chance (CLAUDE.md §13). Earned parts are derived from progress, never bought or stored.
    /// </summary>
    public static class RewardRules
    {
        /// <summary>Parts earned across regions; <paramref name="pathsInOrder"/> lists each region's levels in island order.</summary>
        public static int EarnedCount(IReadOnlyList<IReadOnlyList<string>> pathsInOrder, ProgressBook book)
        {
            int count = 0;
            foreach (var path in pathsInOrder)
            {
                count += EarnedOn(path, book);
            }

            return count;
        }

        /// <summary>Parts earned on one region's path: the corners whose last level the child has reached.</summary>
        public static int EarnedOn(IReadOnlyList<string> path, ProgressBook book)
        {
            if (path == null)
            {
                return 0;
            }

            int furthest = -1;
            for (int i = 0; i < path.Count; i++)
            {
                if (book.IsCompleted(path[i]))
                {
                    furthest = i;
                }
            }

            int earned = 0;
            for (int c = 1; c <= CornerRules.CornerCount(path.Count); c++)
            {
                if (CornerRules.CornerEnd(c, path.Count) <= furthest)
                {
                    earned++;
                }
            }

            return earned;
        }

        public static List<ShipPart> Earned(IEnumerable<ShipPart> catalog, int earnedCount)
        {
            var result = new List<ShipPart>();
            foreach (var part in catalog)
            {
                if (part.Order <= earnedCount)
                {
                    result.Add(part);
                }
            }

            result.Sort((a, b) => a.Order.CompareTo(b.Order));
            return result;
        }

        /// <summary>Parts earned by moving from <paramref name="before"/> to <paramref name="after"/> (the reveal moment).</summary>
        public static List<ShipPart> NewlyEarned(IEnumerable<ShipPart> catalog, int before, int after)
        {
            var result = new List<ShipPart>();
            foreach (var part in catalog)
            {
                if (part.Order > before && part.Order <= after)
                {
                    result.Add(part);
                }
            }

            result.Sort((a, b) => a.Order.CompareTo(b.Order));
            return result;
        }
    }
}
