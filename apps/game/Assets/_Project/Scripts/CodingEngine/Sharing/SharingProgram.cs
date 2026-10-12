using System;
using System.Collections.Generic;

namespace Roboya.CodingEngine.Sharing
{
    public enum SharingCardKind
    {
        /// <summary>Ari takes the honey of a flower into the basket (as much as fits).</summary>
        Take,

        /// <summary>Ari gives honey from the basket to a hive or the table.</summary>
        Give,
    }

    /// <summary>One card of the plan: take from a flower, or give an amount to a hive or the table. Immutable.</summary>
    public sealed class SharingCard : IEquatable<SharingCard>
    {
        private SharingCard(SharingCardKind kind, int flower, int target, int count)
        {
            Kind = kind;
            Flower = flower;
            Target = target;
            Count = count;
        }

        public SharingCardKind Kind { get; }

        /// <summary>The flower of a take card, -1 for a give card.</summary>
        public int Flower { get; }

        /// <summary>The hive index or <see cref="SharingLevel.Table"/> of a give card, 0 for a take card.</summary>
        public int Target { get; }

        /// <summary>The honey a give card hands over, 0 for a take card.</summary>
        public int Count { get; }

        public bool IsTable => Kind == SharingCardKind.Give && Target == SharingLevel.Table;

        public static SharingCard Take(int flower) => new SharingCard(SharingCardKind.Take, flower, 0, 0);

        public static SharingCard Give(int target, int count = 1) => new SharingCard(SharingCardKind.Give, -1, target, count);

        public SharingCard WithCount(int count) => Give(Target, count);

        /// <summary>Same card, ignoring the count: the "where" of a give card.</summary>
        public bool SameTarget(SharingCard other) => other != null && other.Kind == Kind && other.Flower == Flower && other.Target == Target;

        public bool Equals(SharingCard other) => SameTarget(other) && other.Count == Count;

        public override bool Equals(object obj) => Equals(obj as SharingCard);

        public override int GetHashCode() => ((int)Kind * 397) ^ (Flower * 31) ^ (Target * 7) ^ Count;

        public override string ToString() => Kind == SharingCardKind.Take ? "take(" + Flower + ")" : "give(" + (IsTable ? "table" : Target.ToString()) + "," + Count + ")";
    }

    /// <summary>
    /// What a run does: the cards of the repeat box <see cref="Turns"/> times, then the tail. A level without a repeat box has an empty
    /// box and the whole plan in the tail.
    /// </summary>
    public sealed class SharingProgram
    {
        private static readonly SharingCard[] None = new SharingCard[0];

        public SharingProgram(int turns, IReadOnlyList<SharingCard> box, IReadOnlyList<SharingCard> tail)
        {
            Turns = turns < 1 ? 1 : turns;
            Box = box ?? None;
            Tail = tail ?? None;
        }

        public int Turns { get; }

        public IReadOnlyList<SharingCard> Box { get; }

        public IReadOnlyList<SharingCard> Tail { get; }

        /// <summary>The cards the child placed; a repeated card counts once.</summary>
        public int CardCount => Box.Count + Tail.Count;

        public static SharingProgram Linear(IReadOnlyList<SharingCard> cards) => new SharingProgram(1, None, cards);

        /// <summary>The cards in the order they run: the box once per turn, then the tail. Cards keep their identity across turns.</summary>
        public List<SharingCard> Expand()
        {
            var all = new List<SharingCard>(Box.Count * Turns + Tail.Count);
            if (Box.Count > 0)
            {
                for (int t = 0; t < Turns; t++)
                {
                    for (int i = 0; i < Box.Count; i++)
                    {
                        all.Add(Box[i]);
                    }
                }
            }

            for (int i = 0; i < Tail.Count; i++)
            {
                all.Add(Tail[i]);
            }

            return all;
        }
    }
}
