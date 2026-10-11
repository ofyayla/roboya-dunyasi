using System.Collections.Generic;
using Roboya.CodingEngine.World;

namespace Roboya.CodingEngine.Play
{
    public enum HiveMood
    {
        /// <summary>The hive has what it should (or the level is won).</summary>
        Happy,

        /// <summary>The hive has less than it should; with nothing at all it is the "forgotten" one.</summary>
        Sad,

        /// <summary>The hive got more than it should and spills.</summary>
        Spilling,
    }

    /// <summary>What the village should say when a run ends (plan D3). Never a penalty: it only names what to look at.</summary>
    public enum HiveVerdict
    {
        /// <summary>Nothing to say about the hives (no hives, no honey goal, or all as they should be).</summary>
        None,

        /// <summary>The level is won with honey on the table: the left-over was shared with everyone.</summary>
        SharedWithTable,

        /// <summary>The hives hold the same, but something else is still missing (honey in the basket, not all picked up).</summary>
        NotYet,

        /// <summary>A hive got more than it asks for.</summary>
        TooMuch,

        /// <summary>A hive got nothing while another got honey: the forgotten one.</summary>
        Forgotten,

        /// <summary>Hives hold different amounts where they should be equal (or one holds less than it asks).</summary>
        NotEqual,
    }

    /// <summary>
    /// How the hives stand at the end of a run, in the children's terms: who is happy, who is sad because they got less, who spills
    /// over (PK-0001, K5: no numbers, only "the same"). Built from the same <see cref="RobotState"/> counters as the board, so the
    /// feedback and the goal check cannot disagree about who got what.
    /// </summary>
    public sealed class HiveFeedback
    {
        private HiveFeedback(HiveVerdict verdict, IReadOnlyList<int> hives, IReadOnlyList<HiveMood> moods)
        {
            Verdict = verdict;
            Hives = hives;
            Moods = moods;
        }

        public HiveVerdict Verdict { get; }

        /// <summary>Indexes (in <see cref="Level.Drops"/>) of the hives, in board order.</summary>
        public IReadOnlyList<int> Hives { get; }

        /// <summary>One mood per entry of <see cref="Hives"/>.</summary>
        public IReadOnlyList<HiveMood> Moods { get; }

        public static HiveFeedback Of(Level level, RobotState final, bool success)
        {
            var hives = new List<int>();
            for (int i = 0; i < level.Drops.Count; i++)
            {
                if (level.Drops[i].Kind == DropPointKind.Hive)
                {
                    hives.Add(i);
                }
            }

            var moods = new List<HiveMood>(hives.Count);
            if (hives.Count == 0 || !level.Goal.HasHoneyGoal)
            {
                foreach (var _ in hives)
                {
                    moods.Add(HiveMood.Happy);
                }

                return new HiveFeedback(HiveVerdict.None, hives, moods);
            }

            if (success)
            {
                foreach (var _ in hives)
                {
                    moods.Add(HiveMood.Happy);
                }

                bool table = false;
                for (int i = 0; i < level.Drops.Count; i++)
                {
                    table |= level.Drops[i].Kind == DropPointKind.Table && final.DeliveredAt(i) > 0;
                }

                return new HiveFeedback(table ? HiveVerdict.SharedWithTable : HiveVerdict.None, hives, moods);
            }

            var expected = Expected(level, final, hives);
            bool spilling = false;
            bool forgotten = false;
            bool less = false;
            bool anyHoney = false;
            for (int k = 0; k < hives.Count; k++)
            {
                int count = final.DeliveredAt(hives[k]);
                anyHoney |= count > 0;
                if (count > expected[k])
                {
                    moods.Add(HiveMood.Spilling);
                    spilling = true;
                }
                else if (count < expected[k])
                {
                    moods.Add(HiveMood.Sad);
                    less = true;
                    forgotten |= count == 0;
                }
                else
                {
                    moods.Add(HiveMood.Happy);
                }
            }

            // "Forgotten" only makes sense next to a hive that did get honey; with nothing delivered yet there is no one to compare with.
            var verdict = spilling ? HiveVerdict.TooMuch
                : forgotten && anyHoney ? HiveVerdict.Forgotten
                : less && anyHoney ? HiveVerdict.NotEqual
                : less ? HiveVerdict.NotYet
                : HiveVerdict.NotYet;
            return new HiveFeedback(verdict, hives, moods);
        }

        // What each hive should hold: its delivery, or for an equal share the largest amount any of the sharing hives holds.
        private static int[] Expected(Level level, RobotState final, List<int> hives)
        {
            var expected = new int[hives.Count];
            var share = level.Goal.Share;
            int each = 0;
            if (share != null)
            {
                foreach (int hive in share.Among)
                {
                    each = System.Math.Max(each, final.DeliveredAt(hive));
                }
            }

            for (int k = 0; k < hives.Count; k++)
            {
                int hive = hives[k];
                int want = final.DeliveredAt(hive); // a hive nobody asked anything of is happy as it is
                foreach (var d in level.Goal.Deliver)
                {
                    if (d.DropIndex == hive)
                    {
                        want = d.Count;
                    }
                }

                if (share != null && System.Array.IndexOf(ToArray(share.Among), hive) >= 0)
                {
                    want = each;
                }

                expected[k] = want;
            }

            return expected;
        }

        private static int[] ToArray(IReadOnlyList<int> list)
        {
            var array = new int[list.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = list[i];
            }

            return array;
        }
    }
}
