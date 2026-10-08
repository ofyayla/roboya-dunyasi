using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Roboya.Services
{
    /// <summary>What the device keeps about the parent's account: tokens, the e-mail for display and a random device id (never an ad id).</summary>
    public sealed class AccountSession
    {
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("accessExpiresUtc")]
        public DateTime AccessExpiresUtc { get; set; }

        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// The parent's account on this device (F1-14 client, ADR 0019): e-mail code sign-in, silent token refresh,
    /// sign-out. Signed out is the normal state: the game never needs an account except to buy (GLR-03).
    /// </summary>
    public sealed class AccountService
    {
        public const string File = "account.json";
        private static readonly TimeSpan RefreshMargin = TimeSpan.FromSeconds(60);

        private readonly ApiClient _api;
        private readonly string _path;
        private readonly string _platform;
        private readonly Func<DateTime> _utcNow;
        private AccountSession _session;
        private Task<string> _refreshing;

        public AccountService(ApiClient api, string folder, string platform, Func<DateTime> utcNow = null)
        {
            _api = api;
            _path = Path.Combine(folder, File);
            _platform = platform;
            _utcNow = utcNow ?? (() => DateTime.UtcNow);
            _session = Load(_path);
            if (_session.DeviceId == null)
            {
                _session.DeviceId = Guid.NewGuid().ToString();
                Save();
            }
        }

        /// <summary>False when no server address is configured: the game then runs fully offline.</summary>
        public bool IsConfigured => _api != null;

        public bool IsSignedIn => _session.RefreshToken != null;

        public string Email => _session.Email;

        public string DeviceId => _session.DeviceId;

        public event Action Changed;

        public Task RequestCodeAsync(string email) =>
            _api.SendAsync("POST", "/v1/auth/code", new { email = email.Trim() });

        public async Task SignInAsync(string email, string code)
        {
            var tokens = await _api.SendAsync<Tokens>("POST", "/v1/auth/verify", new
            {
                email = email.Trim(),
                code = code.Trim(),
                device = new { device_id = _session.DeviceId, platform = _platform },
            });
            Adopt(tokens);
            _session.Email = email.Trim();
            Save();
            Changed?.Invoke();
        }

        /// <summary>A valid access token, refreshed when it is about to expire; null when signed out or the session ended.</summary>
        public Task<string> GetAccessTokenAsync()
        {
            if (!IsSignedIn)
            {
                return Task.FromResult<string>(null);
            }

            if (_session.AccessToken != null && _utcNow() + RefreshMargin < _session.AccessExpiresUtc)
            {
                return Task.FromResult(_session.AccessToken);
            }

            // One refresh at a time: refresh tokens rotate, so a second parallel call would look like token theft.
            return _refreshing ?? (_refreshing = RefreshAsync());
        }

        public async Task SignOutAsync()
        {
            string refresh = _session.RefreshToken;
            ClearTokens();
            Save();
            Changed?.Invoke();
            if (refresh != null)
            {
                try
                {
                    await _api.SendAsync("POST", "/v1/auth/logout", new { refresh_token = refresh });
                }
                catch (ApiException)
                {
                    // Already signed out here; the server drops the token when it expires.
                }
            }
        }

        private async Task<string> RefreshAsync()
        {
            try
            {
                var tokens = await _api.SendAsync<Tokens>("POST", "/v1/auth/refresh", new { refresh_token = _session.RefreshToken });
                Adopt(tokens);
                Save();
                return _session.AccessToken;
            }
            catch (ApiException e) when (!e.IsNetwork)
            {
                // The server refused the token: the session is over; the parent signs in again.
                ClearTokens();
                Save();
                Changed?.Invoke();
                return null;
            }
            catch (ApiException)
            {
                // Offline: keep the session, try again later.
                return null;
            }
            finally
            {
                _refreshing = null;
            }
        }

        private void Adopt(Tokens t)
        {
            _session.AccountId = t.AccountId;
            _session.AccessToken = t.AccessToken;
            _session.RefreshToken = t.RefreshToken;
            _session.AccessExpiresUtc = _utcNow().AddSeconds(t.ExpiresIn);
        }

        private void ClearTokens()
        {
            _session.AccountId = null;
            _session.Email = null;
            _session.AccessToken = null;
            _session.RefreshToken = null;
        }

        private void Save()
        {
            string temp = _path + ".tmp";
            System.IO.File.WriteAllText(temp, JsonConvert.SerializeObject(_session, Formatting.Indented));
            if (System.IO.File.Exists(_path))
            {
                System.IO.File.Replace(temp, _path, null);
            }
            else
            {
                System.IO.File.Move(temp, _path);
            }
        }

        private static AccountSession Load(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                return new AccountSession();
            }

            try
            {
                return JsonConvert.DeserializeObject<AccountSession>(System.IO.File.ReadAllText(path)) ?? new AccountSession();
            }
            catch (JsonException)
            {
                System.IO.File.Move(path, path + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
                return new AccountSession();
            }
        }

        private sealed class Tokens
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("account_id")]
            public string AccountId { get; set; }
        }
    }
}
