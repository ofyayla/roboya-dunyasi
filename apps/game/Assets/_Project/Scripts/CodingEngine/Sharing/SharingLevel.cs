using System;
using System.Collections.Generic;
using Roboya.CodingEngine.World;

namespace Roboya.CodingEngine.Sharing
{
    /// <summary>How a village level is won (ADR 0031).</summary>
    public enum SharingGoalKind
    {
        /// <summary>Every hive holds exactly the honey it asks for. Honey may stay in the basket (subtraction).</summary>
        Deliver,

        /// <summary>The hives hold the same honey, the basket is empty and what lies on the table is fewer than the hives.</summary>
        Share,
    }

    /// <summary>A hive of the village: the honey it asks for (<see cref="Need"/>, 0 in sharing levels) and what it already holds.</summary>
    public readonly struct HiveSpec
    {
        public HiveSpec(int need, int has = 0)
        {
            Need = need;
            Has = has;
        }

        public int Need { get; }

        public int Has { get; }
    }

    /// <summary>The fixed repeat box of the plan strip (corner 4): how many cards fit inside and how many turns it may run.</summary>
    public sealed class RepeatBoxSpec
    {
        public RepeatBoxSpec(int slots, int maxTurns)
        {
            if (slots < 1 || slots > SharingLevel.MaxHives)
            {
                throw new LevelValidationException("The repeat box holds 1 to " + SharingLevel.MaxHives + " cards.");
            }

            if (maxTurns < 1 || maxTurns > SharingLevel.MaxTurns)
            {
                throw new LevelValidationException("The repeat box may run 1 to " + SharingLevel.MaxTurns + " turns.");
            }

            Slots = slots;
            MaxTurns = maxTurns;
        }

        public int Slots { get; }

        public int MaxTurns { get; }
    }

    /// <summary>
    /// A validated village level: hives, flowers, Ari's basket and what the plan may use. No grid, no direction (ADR 0031).
    /// Targets of a give card are hive indexes, or <see cref="Table"/> for the village table.
    /// </summary>
    public sealed class SharingLevel
    {
        public const int MaxHives = 4;
        public const int MaxFlowers = 3;

        /// <summary>All honey of a level at most: the start basket plus every flower.</summary>
        public const int MaxHoney = 7;

        /// <summary>The most honey one give card hands over.</summary>
        public const int MaxGive = 3;

        public const int MaxTurns = 3;

        /// <summary>The target number of the village table.</summary>
        public const int Table = -1;

        private readonly HiveSpec[] _hives;
        private readonly int[] _flowers;

        /// <param name="basketCap">The most honey the basket holds; 0 for no limit.</param>
        /// <param name="amounts">Give cards carry a count (1 to <see cref="MaxGive"/>); otherwise they hand over one honey.</param>
        public SharingLevel(
            string id,
            IReadOnlyList<HiveSpec> hives,
            IReadOnlyList<int> flowers,
            int basketStart,
            int basketCap,
            bool hasTable,
            SharingGoalKind goal,
            bool amounts,
            int maxCards,
            RepeatBoxSpec repeatBox = null,
            bool asksPrediction = false)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            if (hives == null || hives.Count < 1 || hives.Count > MaxHives)
            {
                throw new LevelValidationException("A level has 1 to " + MaxHives + " hives.");
            }

            _hives = new HiveSpec[hives.Count];
            for (int i = 0; i < _hives.Length; i++)
            {
                _hives[i] = hives[i];
                if (goal == SharingGoalKind.Deliver && hives[i].Need < 1)
                {
                    throw new LevelValidationException("Every hive of a delivery level asks for at least one honey.");
                }

                if (hives[i].Has < 0 || hives[i].Need < 0)
                {
                    throw new LevelValidationException("Hive honey cannot be negative.");
                }
            }

            if (goal == SharingGoalKind.Share && _hives.Length < 2)
            {
                throw new LevelValidationException("Sharing needs at least two hives.");
            }

            int flowerCount = flowers?.Count ?? 0;
            if (flowerCount > MaxFlowers)
            {
                throw new LevelValidationException("A level has at most " + MaxFlowers + " flowers.");
            }

            _flowers = new int[flowerCount];
            int honey = basketStart;
            for (int i = 0; i < flowerCount; i++)
            {
                if (flowers[i] < 1)
                {
                    throw new LevelValidationException("A flower holds at least one honey.");
                }

                _flowers[i] = flowers[i];
                honey += flowers[i];
            }

            if (basketStart < 0 || basketCap < 0 || (basketCap > 0 && basketStart > basketCap))
            {
                throw new LevelValidationException("The start basket does not fit the basket.");
            }

            if (honey > MaxHoney)
            {
                throw new LevelValidationException("A level has at most " + MaxHoney + " honey in all.");
            }

            if (maxCards < 1)
            {
                throw new LevelValidationException("maxCards must be at least 1.");
            }

            BasketStart = basketStart;
            BasketCap = basketCap;
            HasTable = hasTable;
            Goal = goal;
            AllowsAmounts = amounts;
            MaxCards = maxCards;
            RepeatBox = repeatBox;
            AsksPrediction = asksPrediction;
            TotalHoney = honey;
        }

        public string Id { get; }

        public IReadOnlyList<HiveSpec> Hives => _hives;

        /// <summary>The honey each flower holds.</summary>
        public IReadOnlyList<int> Flowers => _flowers;

        public int BasketStart { get; }

        /// <summary>The most honey the basket holds, 0 for no limit.</summary>
        public int BasketCap { get; }

        public bool HasTable { get; }

        public SharingGoalKind Goal { get; }

        public bool AllowsAmounts { get; }

        /// <summary>The plan strip holds this many cards in all (those inside the repeat box count once).</summary>
        public int MaxCards { get; }

        public RepeatBoxSpec RepeatBox { get; }

        /// <summary>Before the run the child guesses how much honey is left in the basket (corner 3).</summary>
        public bool AsksPrediction { get; }

        /// <summary>The honey of the level: the start basket plus every flower.</summary>
        public int TotalHoney { get; }

        /// <summary>The state a run starts from.</summary>
        public SharingState Start()
        {
            var hives = new int[_hives.Length];
            for (int i = 0; i < hives.Length; i++)
            {
                hives[i] = _hives[i].Has;
            }

            return new SharingState(BasketStart, _flowers, hives, 0);
        }

        /// <summary>The honey the basket still has room for in <paramref name="state"/>.</summary>
        public int RoomIn(SharingState state) => BasketCap > 0 ? BasketCap - state.Basket : int.MaxValue;
    }
}
