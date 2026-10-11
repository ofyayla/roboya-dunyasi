using System;
using System.Collections.Generic;

namespace Roboya.CodingEngine.World
{
    /// <summary>Exactly <see cref="Count"/> honey must end up at drop point <see cref="DropIndex"/>.</summary>
    public readonly struct Delivery
    {
        public Delivery(int dropIndex, int count)
        {
            DropIndex = dropIndex;
            Count = count;
        }

        public int DropIndex { get; }

        public int Count { get; }
    }

    /// <summary>
    /// Equal sharing (PK-0001): the hives in <see cref="Among"/> hold the same honey, none is left in the basket, and
    /// what could not be split evenly lies on <see cref="LeftoverTo"/> (a table, or -1 for none) and is fewer than the hives.
    /// </summary>
    public sealed class ShareGoal
    {
        public ShareGoal(IReadOnlyList<int> among, int leftoverTo = -1)
        {
            Among = among ?? throw new ArgumentNullException(nameof(among));
            LeftoverTo = leftoverTo;
        }

        public IReadOnlyList<int> Among { get; }

        public int LeftoverTo { get; }
    }

    /// <summary>
    /// Success condition, evaluated when the program ends: the robot stands on <see cref="Reach"/> (if set),
    /// every item index in <see cref="MustCollect"/> has been collected, and the honey goals hold: the
    /// <see cref="Deliver"/> counts, the <see cref="Share"/> split and the <see cref="CarryAtEnd"/> basket (ADR 0030).
    /// </summary>
    public sealed class Goal
    {
        private static readonly IReadOnlyList<Delivery> NoDeliveries = Array.Empty<Delivery>();

        public Goal(
            GridPosition? reach,
            IReadOnlyList<int> mustCollect,
            IReadOnlyList<Delivery> deliver = null,
            ShareGoal share = null,
            int? carryAtEnd = null)
        {
            Reach = reach;
            MustCollect = mustCollect ?? Array.Empty<int>();
            Deliver = deliver ?? NoDeliveries;
            Share = share;
            CarryAtEnd = carryAtEnd;
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

        public IReadOnlyList<Delivery> Deliver { get; }

        public ShareGoal Share { get; }

        /// <summary>Honey that must still be in the basket at the end (subtraction levels).</summary>
        public int? CarryAtEnd { get; }

        /// <summary>True when the goal has a honey part: a delivery, a share or a basket count.</summary>
        public bool HasHoneyGoal => Deliver.Count > 0 || Share != null || CarryAtEnd.HasValue;

        public bool IsSatisfiedBy(RobotState state)
        {
            bool reached = !Reach.HasValue || state.Position == Reach.Value;
            return reached && (state.CollectedMask & MustCollectMask) == MustCollectMask && IsHoneySatisfiedBy(state);
        }

        /// <summary>The delivery, share and basket conditions alone; true when the goal has none.</summary>
        public bool IsHoneySatisfiedBy(RobotState state)
        {
            foreach (var d in Deliver)
            {
                if (state.DeliveredAt(d.DropIndex) != d.Count)
                {
                    return false;
                }
            }

            if (CarryAtEnd.HasValue && state.Basket != CarryAtEnd.Value)
            {
                return false;
            }

            return Share == null || IsShared(state);
        }

        private bool IsShared(RobotState state)
        {
            if (state.Basket != 0 || Share.Among.Count == 0)
            {
                return false;
            }

            int each = state.DeliveredAt(Share.Among[0]);
            foreach (int hive in Share.Among)
            {
                if (state.DeliveredAt(hive) != each)
                {
                    return false;
                }
            }

            int leftover = Share.LeftoverTo >= 0 ? state.DeliveredAt(Share.LeftoverTo) : 0;
            if (leftover >= Share.Among.Count)
            {
                return false;
            }

            // Honey on any other drop point is honey that was given to the wrong place.
            for (int i = 0; i < RobotState.MaxDropPoints; i++)
            {
                if (i != Share.LeftoverTo && !ContainsIndex(Share.Among, i) && state.DeliveredAt(i) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ContainsIndex(IReadOnlyList<int> list, int value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
