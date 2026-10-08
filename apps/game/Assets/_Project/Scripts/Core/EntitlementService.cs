using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Roboya.Services;

namespace Roboya.Core
{
    /// <summary>
    /// Premium as the server decided it (golden rule 3, ADR 0013): the client asks <c>GET /v1/me/entitlement</c>, keeps the
    /// answer until the server's <c>cache_until</c> and reads it offline. It never computes or grants premium itself; when the
    /// cache runs out the child is on the free tier again until the next successful answer. Signing out drops the cache.
    /// </summary>
    public sealed class EntitlementService : IEntitlementSource
    {
        public const string File = "entitlement.json";

        private readonly ApiClient _api;
        private readonly AccountService _account;
        private readonly string _path;
        private readonly Func<DateTime> _utcNow;
        private Cached _cached;

        public EntitlementService(ApiClient api, AccountService account, string folder, Func<DateTime> utcNow = null)
        {
            _api = api;
            _account = account;
            _path = Path.Combine(folder, File);
            _utcNow = utcNow ?? (() => DateTime.UtcNow);
            _cached = Load(_path);
            account.Changed += () =>
            {
                if (!account.IsSignedIn)
                {
                    Clear();
                }
            };
        }

        public event Action Changed;

        public bool HasPremium => _cached != null && _cached.Tier == "premium" && _utcNow() < _cached.CacheUntilUtc;

        /// <summary>"free", "trial", "active" or "grace" while the cache is fresh; "free" otherwise.</summary>
        public string Status => HasPremium ? _cached.Status : "free";

        public DateTime? ExpiresUtc => HasPremium ? _cached.ExpiresUtc : null;

        /// <summary>Asks the server; true when a fresh answer was stored. Offline or signed out changes nothing.</summary>
        public async Task<bool> RefreshAsync()
        {
            if (_api == null || !_account.IsSignedIn)
            {
                return false;
            }

            try
            {
                string token = await _account.GetAccessTokenAsync();
                if (token == null)
                {
                    return false;
                }

                var reply = await _api.SendAsync<Reply>("GET", "/v1/me/entitlement", null, token);
                _cached = new Cached
                {
                    Tier = reply.Tier,
                    Status = reply.Status,
                    ExpiresUtc = reply.ExpiresAt?.ToUniversalTime(),
                    CacheUntilUtc = reply.CacheUntil.ToUniversalTime(),
                };
                Save();
                Changed?.Invoke();
                return true;
            }
            catch (ApiException)
            {
                return false;
            }
        }

        public void Clear()
        {
            if (_cached == null)
            {
                return;
            }

            _cached = null;
            if (System.IO.File.Exists(_path))
            {
                System.IO.File.Delete(_path);
            }

            Changed?.Invoke();
        }

        private void Save()
        {
            string temp = _path + ".tmp";
            System.IO.File.WriteAllText(temp, JsonConvert.SerializeObject(_cached));
            if (System.IO.File.Exists(_path))
            {
                System.IO.File.Replace(temp, _path, null);
            }
            else
            {
                System.IO.File.Move(temp, _path);
            }
        }

        private static Cached Load(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<Cached>(System.IO.File.ReadAllText(path));
            }
            catch (JsonException)
            {
                // A damaged cache must never grant premium: start free.
                return null;
            }
        }

        private sealed class Cached
        {
            public string Tier { get; set; }

            public string Status { get; set; }

            public DateTime? ExpiresUtc { get; set; }

            public DateTime CacheUntilUtc { get; set; }
        }

        private sealed class Reply
        {
            [JsonProperty("tier")]
            public string Tier { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("expires_at")]
            public DateTime? ExpiresAt { get; set; }

            [JsonProperty("cache_until")]
            public DateTime CacheUntil { get; set; }
        }
    }
}
