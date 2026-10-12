using System;

namespace Roboya.CodingEngine.Sharing
{
    /// <summary>
    /// Where the honey is: Ari's basket, each flower, each hive and the table. Immutable; every change returns a new state, so the
    /// solver can keep them in a set by <see cref="Key"/>.
    /// </summary>
    public sealed class SharingState : IEquatable<SharingState>
    {
        private readonly int[] _flowers;
        private readonly int[] _hives;

        public SharingState(int basket, int[] flowers, int[] hives, int table)
        {
            Basket = basket;
            _flowers = (int[])flowers.Clone();
            _hives = (int[])hives.Clone();
            Table = table;
        }

        public int Basket { get; }

        public int Table { get; }

        public int FlowerCount => _flowers.Length;

        public int HiveCount => _hives.Length;

        public int FlowerAt(int index) => _flowers[index];

        public int HiveAt(int index) => _hives[index];

        /// <summary>The honey at a give target: a hive, or the table for <see cref="SharingLevel.Table"/>.</summary>
        public int At(int target) => target == SharingLevel.Table ? Table : _hives[target];

        /// <summary>Ari takes <paramref name="amount"/> honey from a flower into the basket.</summary>
        public SharingState Took(int flower, int amount)
        {
            var flowers = (int[])_flowers.Clone();
            flowers[flower] -= amount;
            return new SharingState(Basket + amount, flowers, _hives, Table);
        }

        /// <summary>Ari gives <paramref name="amount"/> honey from the basket to a hive or the table.</summary>
        public SharingState Gave(int target, int amount)
        {
            if (target == SharingLevel.Table)
            {
                return new SharingState(Basket - amount, _flowers, _hives, Table + amount);
            }

            var hives = (int[])_hives.Clone();
            hives[target] += amount;
            return new SharingState(Basket - amount, _flowers, hives, Table);
        }

        /// <summary>Packs the state into one number: four bits per count, at most 4 hives and 3 flowers.</summary>
        public long Key
        {
            get
            {
                long key = Basket;
                key = (key << 4) | (long)Table;
                for (int i = 0; i < _flowers.Length; i++)
                {
                    key = (key << 4) | (long)_flowers[i];
                }

                for (int i = 0; i < _hives.Length; i++)
                {
                    key = (key << 4) | (long)_hives[i];
                }

                return key;
            }
        }

        public bool Equals(SharingState other) => other != null && other._flowers.Length == _flowers.Length && other._hives.Length == _hives.Length && other.Key == Key;

        public override bool Equals(object obj) => Equals(obj as SharingState);

        public override int GetHashCode() => Key.GetHashCode();
    }
}
