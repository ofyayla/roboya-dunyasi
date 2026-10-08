using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Roboya.CodingEngine.Progress
{
    /// <summary>
    /// One child profile's progress (ILR-01): best stars per level and how many ship parts the child has already
    /// watched being fitted. Earned parts are not stored; <see cref="RewardRules"/> derives them from completed
    /// levels. Holds no personal data. Files written by older builds may carry an "equipped" map; it is ignored.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ProgressBook
    {
        public const int CurrentVersion = 1;

        [JsonProperty("version")]
        private int _version = CurrentVersion;

        [JsonProperty("stars")]
        private Dictionary<string, int> _stars = new Dictionary<string, int>(StringComparer.Ordinal);

        [JsonProperty("shipPartsSeen")]
        private int _shipPartsSeen;

        public int Version => _version;

        /// <summary>Levels with at least one star, i.e. finished.</summary>
        public int CompletedCount
        {
            get
            {
                int n = 0;
                foreach (var s in _stars.Values)
                {
                    if (s > 0)
                    {
                        n++;
                    }
                }

                return n;
            }
        }

        /// <summary>Every recorded level with its best stars (read-only view; used to sync with the server).</summary>
        public IReadOnlyDictionary<string, int> AllStars => _stars;

        public int Stars(string levelId) => levelId != null && _stars.TryGetValue(levelId, out var s) ? s : 0;

        public bool IsCompleted(string levelId) => Stars(levelId) > 0;

        /// <summary>ILR-02: replays never lower the best result. Returns true when the stored value improved.</summary>
        public bool Record(string levelId, int stars)
        {
            if (string.IsNullOrEmpty(levelId))
            {
                throw new ArgumentException("Level id is required.", nameof(levelId));
            }

            stars = Math.Max(0, Math.Min(3, stars));
            if (stars <= Stars(levelId))
            {
                return false;
            }

            _stars[levelId] = stars;
            return true;
        }

        /// <summary>Ship parts already shown being fitted; newer ones animate in once on the map (ILR-03).</summary>
        public int ShipPartsSeen => _shipPartsSeen;

        /// <summary>Only moves forward, so a part is never celebrated twice.</summary>
        public bool MarkShipPartsSeen(int count)
        {
            if (count <= _shipPartsSeen)
            {
                return false;
            }

            _shipPartsSeen = count;
            return true;
        }

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

        /// <summary>Parses a saved book; unknown future versions and malformed input throw <see cref="FormatException"/>.</summary>
        public static ProgressBook FromJson(string json)
        {
            ProgressBook book;
            try
            {
                book = JsonConvert.DeserializeObject<ProgressBook>(json);
            }
            catch (JsonException e)
            {
                throw new FormatException("Progress JSON is malformed: " + e.Message, e);
            }

            if (book == null)
            {
                throw new FormatException("Progress JSON is empty.");
            }

            if (book._version > CurrentVersion)
            {
                throw new FormatException("Progress version " + book._version + " is newer than this app.");
            }

            book._stars = new Dictionary<string, int>(book._stars ?? new Dictionary<string, int>(), StringComparer.Ordinal);
            book._shipPartsSeen = Math.Max(0, book._shipPartsSeen);
            return book;
        }
    }
}
