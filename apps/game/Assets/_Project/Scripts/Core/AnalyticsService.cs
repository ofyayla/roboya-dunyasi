using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Roboya.Services;

namespace Roboya.Core
{
    /// <summary>
    /// First-party, anonymous usage events (F1-18 client, ADR 0014, ADR 0022). An event carries a random per-profile
    /// id that is not the profile id, a fixed name and a few small numbers or fixed words, never a nickname, e-mail or free text.
    /// Nothing is recorded without the parent's consent for the current notice, nor when no server is configured.
    /// Events wait in a small file and go out in batches; offline play just fills the queue.
    /// </summary>
    public sealed class AnalyticsService
    {
        public const string File = "analytics.json";
        public const int MaxQueued = 1000;
        public const int BatchSize = 100;
        public const int FlushThreshold = 20;

        /// <summary>The names the server accepts; the purchase facts come from store notifications, not from the app.</summary>
        public static readonly HashSet<string> Names = new HashSet<string>
        {
            "app_open", "level_start", "level_complete", "level_abandon", "hint_used", "session_limit_reached", "paywall_view",
        };

        public static readonly HashSet<string> NumberProps = new HashSet<string>
        {
            "attempts", "duration_s", "stars", "hints", "hint_tier", "code_length", "last_step", "limit_minutes",
        };

        public static readonly Dictionary<string, HashSet<string>> WordProps = new Dictionary<string, HashSet<string>>
        {
            { "profile_kind", new HashSet<string> { "family", "school" } },
            { "device_class", new HashSet<string> { "phone", "tablet" } },
            { "level_band", new HashSet<string> { "minik", "kasif", "mucit" } },
            { "source", new HashSet<string> { "map", "path", "level_end", "parent" } },
        };

        private readonly ApiClient _api;
        private readonly ProfileManager _profiles;
        private readonly LocalNotice _notice;
        private readonly string _path;
        private readonly string _appVersion;
        private readonly string _platform;
        private readonly Func<DateTime> _utcNow;
        private readonly State _state;
        private bool _flushing;

        public AnalyticsService(
            ApiClient api, ProfileManager profiles, LocalNotice notice, string folder, string appVersion, string platform, Func<DateTime> utcNow = null)
        {
            _api = api;
            _profiles = profiles;
            _notice = notice;
            _path = Path.Combine(folder, File);
            _appVersion = appVersion;
            _platform = platform;
            _utcNow = utcNow ?? (() => DateTime.UtcNow);
            _state = Load(_path);
            profiles.Changed += Prune;
        }

        public int QueuedCount => _state.Queue.Count;

        /// <summary>Builds a property set from alternating key and value arguments: <c>Props("stars", 3, "level_band", "minik")</c>.</summary>
        public static IReadOnlyDictionary<string, object> Props(params object[] pairs)
        {
            var map = new Dictionary<string, object>();
            for (int i = 0; i + 1 < pairs.Length; i += 2)
            {
                map[(string)pairs[i]] = pairs[i + 1];
            }

            return map;
        }

        /// <summary>The level band word for the active child, for events that describe a level.</summary>
        public string ActiveBand => _profiles.HasActive ? _profiles.Active.AgeBand.ToString().ToLowerInvariant() : null;

        /// <summary>Records an event for the active child; props are validated against the server's fixed lists.</summary>
        public void Track(string name, string levelId = null, IReadOnlyDictionary<string, object> props = null)
        {
            Validate(name, props);
            if (_api == null || !_profiles.HasActive || !_profiles.Registry.HasConsentFor(_notice.Version))
            {
                return;
            }

            _state.Queue.Add(new Queued
            {
                AnonId = AnonIdOf(_profiles.Active.Id),
                Id = Guid.NewGuid().ToString(),
                Name = name,
                LevelId = levelId,
                OccurredAt = _utcNow(),
                Props = props == null ? new Dictionary<string, object>() : new Dictionary<string, object>(ToDictionary(props)),
            });
            while (_state.Queue.Count > MaxQueued)
            {
                _state.Queue.RemoveAt(0);
            }

            Save();
            if (_state.Queue.Count >= FlushThreshold)
            {
                _ = FlushAsync();
            }
        }

        /// <summary>Sends what is waiting. Network trouble keeps the events; a rejected batch is dropped so it cannot block the rest.</summary>
        public async Task FlushAsync()
        {
            if (_api == null || _flushing || _state.Queue.Count == 0)
            {
                return;
            }

            if (!_profiles.Registry.HasConsentFor(_notice.Version))
            {
                _state.Queue.Clear();
                Save();
                return;
            }

            _flushing = true;
            try
            {
                while (_state.Queue.Count > 0)
                {
                    string anon = _state.Queue[0].AnonId;
                    var batch = new List<Queued>();
                    foreach (var q in _state.Queue)
                    {
                        if (q.AnonId == anon && batch.Count < BatchSize)
                        {
                            batch.Add(q);
                        }
                    }

                    try
                    {
                        await _api.SendAsync("POST", "/v1/events", new
                        {
                            anon_id = anon,
                            app_version = _appVersion,
                            platform = _platform,
                            events = batch.ConvertAll(ToWire),
                        });
                    }
                    catch (ApiException e) when (e.IsNetwork)
                    {
                        return;
                    }
                    catch (ApiException)
                    {
                        // The server will never accept this batch (bad shape, rate limit); drop it rather than retry forever.
                    }

                    _state.Queue.RemoveAll(q => batch.Contains(q));
                    Save();
                }
            }
            finally
            {
                _flushing = false;
            }
        }

        private static void Validate(string name, IReadOnlyDictionary<string, object> props)
        {
            if (!Names.Contains(name))
            {
                throw new ArgumentException("Unknown event name: " + name, nameof(name));
            }

            if (props == null)
            {
                return;
            }

            foreach (var pair in props)
            {
                if (NumberProps.Contains(pair.Key))
                {
                    if (!(pair.Value is int))
                    {
                        throw new ArgumentException("Property must be an int: " + pair.Key, nameof(props));
                    }
                }
                else if (WordProps.TryGetValue(pair.Key, out var allowed))
                {
                    if (!(pair.Value is string word) || !allowed.Contains(word))
                    {
                        throw new ArgumentException("Property value not allowed: " + pair.Key, nameof(props));
                    }
                }
                else
                {
                    throw new ArgumentException("Unknown property: " + pair.Key, nameof(props));
                }
            }
        }

        private static Dictionary<string, object> ToDictionary(IReadOnlyDictionary<string, object> props)
        {
            var copy = new Dictionary<string, object>();
            foreach (var pair in props)
            {
                copy[pair.Key] = pair.Value;
            }

            return copy;
        }

        private static object ToWire(Queued q) => new
        {
            id = q.Id,
            name = q.Name,
            level_id = q.LevelId,
            occurred_at = q.OccurredAt.ToString("o"),
            props = q.Props,
        };

        /// <summary>A random id per child on this device; deliberately not the profile id, so events cannot be tied to the account's profiles.</summary>
        private string AnonIdOf(string profileId)
        {
            if (!_state.Anon.TryGetValue(profileId, out var anon))
            {
                anon = Guid.NewGuid().ToString();
                _state.Anon[profileId] = anon;
            }

            return anon;
        }

        /// <summary>A removed profile takes its anonymous id and waiting events with it.</summary>
        private void Prune()
        {
            var gone = new List<string>();
            foreach (var id in _state.Anon.Keys)
            {
                if (_profiles.Registry.Find(id) == null)
                {
                    gone.Add(id);
                }
            }

            if (gone.Count == 0)
            {
                return;
            }

            foreach (var id in gone)
            {
                string anon = _state.Anon[id];
                _state.Anon.Remove(id);
                _state.Queue.RemoveAll(q => q.AnonId == anon);
            }

            Save();
        }

        private void Save()
        {
            string temp = _path + ".tmp";
            System.IO.File.WriteAllText(temp, JsonConvert.SerializeObject(_state));
            if (System.IO.File.Exists(_path))
            {
                System.IO.File.Replace(temp, _path, null);
            }
            else
            {
                System.IO.File.Move(temp, _path);
            }
        }

        private static State Load(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                return new State();
            }

            try
            {
                return JsonConvert.DeserializeObject<State>(System.IO.File.ReadAllText(path)) ?? new State();
            }
            catch (JsonException)
            {
                // Only anonymous statistics; a damaged file is simply started over.
                return new State();
            }
        }

        private sealed class State
        {
            public Dictionary<string, string> Anon { get; set; } = new Dictionary<string, string>();

            public List<Queued> Queue { get; set; } = new List<Queued>();
        }

        private sealed class Queued
        {
            public string AnonId { get; set; }

            public string Id { get; set; }

            public string Name { get; set; }

            public string LevelId { get; set; }

            public DateTime OccurredAt { get; set; }

            public Dictionary<string, object> Props { get; set; } = new Dictionary<string, object>();
        }
    }
}
