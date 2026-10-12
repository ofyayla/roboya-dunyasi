using System.Collections.Generic;
using Roboya.CodingEngine.Progress;
using UnityEngine;

namespace Roboya.Core
{
    /// <summary>
    /// What every level screen does around a play, kept in one place so the grid games and the village square agree: the analytics props,
    /// keeping the best result and the earned ship parts after a win, saving, and where "next" leads. Views stay in the controllers.
    /// </summary>
    public static class LevelFlow
    {
        private const string MoreAheadVoice = "map.welcome";
        private const string AskGrownUpVoice = "roboya.ask_grownup";

        /// <summary>The analytics props of a level event: the child's age band (when known) and anything extra.</summary>
        public static IReadOnlyDictionary<string, object> Props(GameServices services, params (string key, object value)[] extra)
        {
            var map = new Dictionary<string, object>();
            string band = services.Analytics.ActiveBand;
            if (band != null)
            {
                map["level_band"] = band;
            }

            foreach (var (key, value) in extra)
            {
                map[key] = value;
            }

            return map;
        }

        /// <summary>Leaving a level that was not finished (home button, another level) counts as abandoned.</summary>
        public static void ReportAbandon(GameServices services, LevelEntry entry, int attempts, int seconds, int hints)
        {
            services.Analytics.Track("level_abandon", entry.Id, Props(services, ("attempts", attempts), ("duration_s", seconds), ("hints", hints)));
        }

        /// <summary>
        /// ILR-01/02: keeps the best result on the device and reports the win. Ship parts are derived from the book (ILR-03), so the parts a win
        /// earns are what the count gained.
        /// </summary>
        public static List<ShipPart> RecordWin(GameServices services, LevelEntry entry, int stars, int attempts, int seconds, int hints, int codeLength)
        {
            int partsBefore = ProgressQueries.EarnedParts(services);
            services.Progress.Book.Record(entry.Id, stars);
            SaveProgress(services);
            services.Analytics.Track(
                "level_complete",
                entry.Id,
                Props(services, ("attempts", attempts), ("duration_s", seconds), ("stars", stars), ("hints", hints), ("code_length", Mathf.Clamp(codeLength, 0, 64))));
            return RewardRules.NewlyEarned(services.Parts.Parts, partsBefore, ProgressQueries.EarnedParts(services));
        }

        private static int IndexOf(IReadOnlyList<string> path, string id)
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (path[i] == id)
                {
                    return i;
                }
            }

            return -1;
        }

        public static void SaveProgress(GameServices services)
        {
            try
            {
                services.Progress.Save();
            }
            catch (System.IO.IOException e)
            {
                // A full disk must not interrupt the child's success moment; the book stays in memory.
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// After the result the child goes back to the map, where the trail shows the next stone; levels are not chained so the child sees
        /// there is more ahead. Past the free tier Roboya asks a grown-up instead (GLR-01), and when today's play time is used up she rests (VEL-02).
        /// </summary>
        public static void GoNext(GameServices services, IReadOnlyList<string> path, IReadOnlyList<LevelEntry> levels, int index)
        {
            if (services.ScreenTime.IsExhausted)
            {
                services.Navigator.GoToMap();
                return;
            }

            var states = ProgressQueries.PathStates(services, path);
            int start = ProgressQueries.StartIndex(services, path);
            for (int next = index + 1; next < levels.Count; next++)
            {
                int pathIndex = IndexOf(path, levels[next].Id);
                var state = pathIndex >= 0 ? states[pathIndex] : NodeState.Locked;
                if (PathRules.CanPlay(state))
                {
                    services.Navigator.GoToMap(MoreAheadVoice);
                    return;
                }

                // Levels before the child's age start are skipped quietly (they are paid and not theirs);
                // anything from the start on that is paid asks a grown-up (GLR-01).
                if (state == NodeState.NeedsGrownUp && pathIndex >= start)
                {
                    services.Navigator.GoToMap(AskGrownUpVoice);
                    return;
                }

                if (state != NodeState.NeedsGrownUp)
                {
                    break;
                }
            }

            services.Navigator.GoToMap();
        }
    }
}
