using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Roboya.CodingEngine.Progress
{
    /// <summary>
    /// One child profile's progress (ILR-01): best stars per level and the robot parts Roboya wears. Earned parts
    /// are not stored; <see cref="RewardRules"/> derives them from completed levels. Holds no personal data.
    /// </summary>
    public sealed class ProgressBook
    {
        public const int CurrentVersion = 1;

        [JsonProperty("version")]
        private int _version = CurrentVersion;

        [JsonProperty("stars")]
        private Dictionary<string, int> _stars = new Dictionary<string, int>(StringComparer.Ordinal);

        [JsonProperty("equipped")]
        private Dictionary<string, string> _equipped = new Dictionary<string, string>(StringComparer.Ordinal);

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

        /// <summary>The part worn in a slot (antenna, color, wings, hat), or null.</summary>
        public string Equipped(string slot) => slot != null && _equipped.TryGetValue(slot, out var id) ? id : null;

        public void Equip(string slot, string partId)
        {
            if (string.IsNullOrEmpty(slot))
            {
                throw new ArgumentException("Slot is required.", nameof(slot));
            }

            if (string.IsNullOrEmpty(partId))
            {
                _equipped.Remove(slot);
            }
            else
            {
                _equipped[slot] = partId;
            }
        }

        public IReadOnlyDictionary<string, string> AllEquipped => _equipped;

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
            book._equipped = new Dictionary<string, string>(book._equipped ?? new Dictionary<string, string>(), StringComparer.Ordinal);
            return book;
        }
    }
}
