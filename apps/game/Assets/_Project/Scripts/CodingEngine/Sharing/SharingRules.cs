namespace Roboya.CodingEngine.Sharing
{
    /// <summary>When a village level is won and who is still waiting for honey (ADR 0031).</summary>
    public static class SharingRules
    {
        /// <summary>
        /// Deliver: every hive holds what it asked for. Share: the hives hold the same honey (at least one each), the basket is empty and
        /// the table holds fewer than there are hives (so the biggest equal share is the only right one); without a table nothing is left over.
        /// </summary>
        public static bool IsGoalMet(SharingLevel level, SharingState state)
        {
            if (level.Goal == SharingGoalKind.Deliver)
            {
                for (int i = 0; i < state.HiveCount; i++)
                {
                    if (state.HiveAt(i) != level.Hives[i].Need)
                    {
                        return false;
                    }
                }

                return true;
            }

            int first = state.HiveAt(0);
            if (first < 1 || state.Basket != 0)
            {
                return false;
            }

            for (int i = 1; i < state.HiveCount; i++)
            {
                if (state.HiveAt(i) != first)
                {
                    return false;
                }
            }

            return level.HasTable ? state.Table < state.HiveCount : state.Table == 0;
        }

        /// <summary>
        /// The hives whose villager is still waiting at the end of a run: short of what they asked for, or below the others in a share. The
        /// view shows them in a waiting pose, not a sad one (K12). When the goal is met nobody waits.
        /// </summary>
        public static bool[] WaitingHives(SharingLevel level, SharingState state)
        {
            var waiting = new bool[state.HiveCount];
            if (IsGoalMet(level, state))
            {
                return waiting;
            }

            int most = 0;
            for (int i = 0; i < state.HiveCount; i++)
            {
                if (state.HiveAt(i) > most)
                {
                    most = state.HiveAt(i);
                }
            }

            for (int i = 0; i < waiting.Length; i++)
            {
                waiting[i] = level.Goal == SharingGoalKind.Deliver ? state.HiveAt(i) < level.Hives[i].Need : state.HiveAt(i) < most || state.HiveAt(i) == 0;
            }

            return waiting;
        }

        /// <summary>A share that came out equal but left too much on the table or in the basket: Roboya says so instead of "not equal".</summary>
        public static bool IsEqualButLeftOver(SharingLevel level, SharingState state)
        {
            if (level.Goal != SharingGoalKind.Share || IsGoalMet(level, state) || state.HiveAt(0) < 1)
            {
                return false;
            }

            for (int i = 1; i < state.HiveCount; i++)
            {
                if (state.HiveAt(i) != state.HiveAt(0))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
