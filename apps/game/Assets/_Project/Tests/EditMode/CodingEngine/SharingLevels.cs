using System.Collections.Generic;
using Roboya.CodingEngine.Sharing;

namespace Roboya.Tests.CodingEngine
{
    /// <summary>Village levels for tests: the same shapes the first corners of the game use (ADR 0031).</summary>
    internal static class SharingLevels
    {
        public static readonly SharingCard G0 = SharingCard.Give(0);
        public static readonly SharingCard G1 = SharingCard.Give(1);
        public static readonly SharingCard G2 = SharingCard.Give(2);
        public static readonly SharingCard GT = SharingCard.Give(SharingLevel.Table);
        public static readonly SharingCard T0 = SharingCard.Take(0);
        public static readonly SharingCard T1 = SharingCard.Take(1);

        /// <param name="needs">One entry per hive; 0 in a sharing level.</param>
        public static SharingLevel Make(
            int[] needs,
            int basket = 0,
            int[] flowers = null,
            int cap = 0,
            bool table = false,
            SharingGoalKind goal = SharingGoalKind.Deliver,
            bool amounts = false,
            int maxCards = 9,
            RepeatBoxSpec box = null,
            bool predict = false,
            int[] has = null,
            string id = "test.level")
        {
            var hives = new List<HiveSpec>();
            for (int i = 0; i < needs.Length; i++)
            {
                hives.Add(new HiveSpec(needs[i], has != null ? has[i] : 0));
            }

            return new SharingLevel(id, hives, flowers ?? new int[0], basket, cap, table, goal, amounts, maxCards, box, predict);
        }

        /// <summary>Corner 1: two hives that ask for one honey each, two honey in the basket.</summary>
        public static SharingLevel OneEach() => Make(new[] { 1, 1 }, basket: 2);

        /// <summary>Corner 2: take from two flowers, the basket holds two, one hive asks for three.</summary>
        public static SharingLevel BasketOfTwo() => Make(new[] { 3 }, flowers: new[] { 2, 1 }, cap: 2, amounts: true);

        /// <summary>Corner 3: five in the basket, the hive asks for two, three are left.</summary>
        public static SharingLevel Subtraction() => Make(new[] { 2 }, basket: 5, amounts: true, predict: true);

        /// <summary>Corner 4: six honey, three hives, the repeat box holds three cards (so the box is needed).</summary>
        public static SharingLevel ShareThreeWithBox() => Make(new[] { 0, 0, 0 }, basket: 6, goal: SharingGoalKind.Share, maxCards: 3, box: new RepeatBoxSpec(3, 3));

        /// <summary>Corner 4 finale: seven honey, three hives, a table for the left-over.</summary>
        public static SharingLevel ShareThreeWithTable() => Make(new[] { 0, 0, 0 }, basket: 7, table: true, goal: SharingGoalKind.Share, maxCards: 4, box: new RepeatBoxSpec(3, 3));

        public static SharingProgram Plan(params SharingCard[] cards) => SharingProgram.Linear(cards);

        public static SharingProgram Repeat(int turns, SharingCard[] box, params SharingCard[] tail) => new SharingProgram(turns, box, tail);
    }
}
