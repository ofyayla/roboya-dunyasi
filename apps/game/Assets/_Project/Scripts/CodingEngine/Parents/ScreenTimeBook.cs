using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Roboya.CodingEngine.Profiles;

namespace Roboya.CodingEngine.Parents
{
    /// <summary>Daily play-time choices and the recommended default per age band (PRD "Günlük süre sınırı").</summary>
    public static class ScreenTimeRules
    {
        /// <summary>0 means unlimited.</summary>
        public static readonly IReadOnlyList<int> Options = new[] { 10, 15, 20, 30, 0 };

        public static int RecommendedMinutes(AgeBand band)
        {
            switch (band)
            {
                case AgeBand.Minik: return 10;
                case AgeBand.Kasif: return 15;
                default: return 20;
            }
        }
    }

    /// <summary>
    /// Play time per profile and day, and the parent's daily limit (F1-11, VEL-02). A parent setting kept apart from the
    /// child profile (which holds only a nickname, avatar and age band). Pure logic; the clock is passed in. When the
    /// limit is used up a level in progress may be finished but a new one cannot start (the caller enforces that).
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ScreenTimeBook
    {
        public const int CurrentVersion = 1;
        public const int KeepDays = 30;

        [JsonProperty("version")]
        private int _version = CurrentVersion;

        [JsonProperty("limits")]
        private Dictionary<string, int> _limits = new Dictionary<string, int>(StringComparer.Ordinal);

        [JsonProperty("usage")]
        private Dictionary<string, Dictionary<string, double>> _usage =
            new Dictionary<string, Dictionary<string, double>>(StringComparer.Ordinal);

        public static string DayKey(DateTime localDate) => localDate.ToString("yyyy-MM-dd");

        /// <summary>The limit in minutes (0 = unlimited): the parent's choice, or the recommendation for the age band.</summary>
        public int LimitMinutes(string profileId, AgeBand band) =>
            profileId != null && _limits.TryGetValue(profileId, out var minutes) ? minutes : ScreenTimeRules.RecommendedMinutes(band);

        public bool HasChosenLimit(string profileId) => profileId != null && _limits.ContainsKey(profileId);

        public void SetLimit(string profileId, int minutes)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                throw new ArgumentException("A profile id is required.", nameof(profileId));
            }

            if (!((IList<int>)ScreenTimeRules.Options).Contains(minutes))
            {
                throw new ArgumentOutOfRangeException(nameof(minutes), "Not one of the offered limits.");
            }

            _limits[profileId] = minutes;
        }

        public double UsedSeconds(string profileId, DateTime localDate) =>
            profileId != null && _usage.TryGetValue(profileId, out var days) && days.TryGetValue(DayKey(localDate), out var s) ? s : 0.0;

        public void AddUsage(string profileId, DateTime localDate, double seconds)
        {
            if (string.IsNullOrEmpty(profileId) || seconds <= 0.0 || double.IsNaN(seconds) || double.IsInfinity(seconds))
            {
                return;
            }

            if (!_usage.TryGetValue(profileId, out var days))
            {
                days = new Dictionary<string, double>(StringComparer.Ordinal);
                _usage[profileId] = days;
            }

            string key = DayKey(localDate);
            days[key] = (days.TryGetValue(key, out var used) ? used : 0.0) + seconds;
            Prune(days, localDate);
        }

        /// <summary>Seconds left today; <see cref="double.PositiveInfinity"/> when unlimited.</summary>
        public double RemainingSeconds(string profileId, AgeBand band, DateTime localDate)
        {
            int limit = LimitMinutes(profileId, band);
            return limit <= 0 ? double.PositiveInfinity : Math.Max(0.0, (limit * 60.0) - UsedSeconds(profileId, localDate));
        }

        public bool IsExhausted(string profileId, AgeBand band, DateTime localDate) =>
            RemainingSeconds(profileId, band, localDate) <= 0.0;

        /// <summary>Keeps only the profiles that still exist (a removed child's settings and usage go too).</summary>
        public void Retain(IEnumerable<string> profileIds)
        {
            var keep = new HashSet<string>(profileIds, StringComparer.Ordinal);
            foreach (var id in new List<string>(_limits.Keys))
            {
                if (!keep.Contains(id))
                {
                    _limits.Remove(id);
                }
            }

            foreach (var id in new List<string>(_usage.Keys))
            {
                if (!keep.Contains(id))
                {
                    _usage.Remove(id);
                }
            }
        }

        /// <summary>Forgets a profile that was removed.</summary>
        public void Forget(string profileId)
        {
            if (profileId == null)
            {
                return;
            }

            _limits.Remove(profileId);
            _usage.Remove(profileId);
        }

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static ScreenTimeBook FromJson(string json)
        {
            ScreenTimeBook book;
            try
            {
                book = JsonConvert.DeserializeObject<ScreenTimeBook>(json);
            }
            catch (JsonException e)
            {
                throw new FormatException("Screen time file is malformed: " + e.Message, e);
            }

            if (book == null)
            {
                throw new FormatException("Screen time file is empty.");
            }

            if (book._version > CurrentVersion)
            {
                throw new FormatException("Screen time version " + book._version + " is newer than this app.");
            }

            book._limits = book._limits ?? new Dictionary<string, int>(StringComparer.Ordinal);
            book._usage = book._usage ?? new Dictionary<string, Dictionary<string, double>>(StringComparer.Ordinal);
            return book;
        }

        /// <summary>Drops days older than <see cref="KeepDays"/> before the newest day recorded.</summary>
        private static void Prune(Dictionary<string, double> days, DateTime _)
        {
            if (days.Count <= KeepDays)
            {
                return;
            }

            string newest = null;
            foreach (var key in days.Keys)
            {
                if (newest == null || string.CompareOrdinal(key, newest) > 0)
                {
                    newest = key;
                }
            }

            string oldestKept = DayKey(DateTime.ParseExact(newest, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).AddDays(-KeepDays));
            foreach (var key in new List<string>(days.Keys))
            {
                if (string.CompareOrdinal(key, oldestKept) < 0)
                {
                    days.Remove(key);
                }
            }
        }
    }
}
