using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Roboya.Services;

namespace Roboya.Core
{
    /// <summary>What the server holds about the parent account, in short (UYM-03).</summary>
    public sealed class MyData
    {
        public string Email { get; set; }

        public List<string> ProfileNicknames { get; set; } = new List<string>();

        public int Devices { get; set; }

        public int Consents { get; set; }

        public int Subscriptions { get; set; }

        public DateTime? DeletionScheduledUtc { get; set; }
    }

    /// <summary>
    /// The privacy centre's actions (F1-13 client, ADR 0023): see and save the data, withdraw consent, wipe this device,
    /// and ask the server to delete the account. Local actions always work offline; server actions need a signed-in parent.
    /// </summary>
    public sealed class PrivacyService
    {
        public const string ExportFile = "roboya-verilerim.json";
        public const string PendingFile = "privacy.json";

        private readonly ApiClient _api;
        private readonly AccountService _account;
        private readonly ProfileManager _profiles;
        private readonly SyncService _sync;
        private readonly string _folder;

        public PrivacyService(ApiClient api, AccountService account, ProfileManager profiles, SyncService sync, string folder)
        {
            _sync = sync;
            _api = api;
            _account = account;
            _profiles = profiles;
            _folder = folder;
        }

        public bool CanUseServer => _api != null && _account.IsSignedIn;

        /// <summary>Withdraws consent here at once (nothing about a child is sent after this), and on the server when possible.</summary>
        public async Task WithdrawConsentAsync()
        {
            _profiles.WithdrawConsent();
            if (CanUseServer)
            {
                await TryServerWithdrawalAsync();
            }
        }

        /// <summary>Retries a withdrawal that could not reach the server (start-up, next sign-in).</summary>
        public async Task FlushPendingAsync()
        {
            if (CanUseServer && System.IO.File.Exists(Path.Combine(_folder, PendingFile)))
            {
                await TryServerWithdrawalAsync();
            }
        }

        /// <summary>Removes every child profile and its progress from this device and withdraws consent.</summary>
        public async Task WipeDeviceAsync()
        {
            foreach (var profile in new List<Roboya.CodingEngine.Profiles.ChildProfileData>(_profiles.Registry.Profiles))
            {
                _profiles.Remove(profile.Id);
            }

            // Tells the server to drop the profiles that just left this device (sync removes what is gone locally).
            await _sync.SyncAsync();
            await WithdrawConsentAsync();
        }

        public async Task<MyData> GetMyDataAsync()
        {
            string json = await _api.GetTextAsync("/v1/me/data", await TokenAsync());
            var root = JObject.Parse(json);
            var data = new MyData
            {
                Email = (string)(root["account"] as JObject)?["email"],
                Devices = ((JArray)root["devices"])?.Count ?? 0,
                Consents = ((JArray)root["consents"])?.Count ?? 0,
                Subscriptions = ((JArray)root["subscriptions"])?.Count ?? 0,
                DeletionScheduledUtc = (DateTime?)(root["deletion_request"] as JObject)?["scheduled_for"],
            };
            if (root["profiles"] is JArray profiles)
            {
                foreach (var p in profiles)
                {
                    data.ProfileNicknames.Add((string)p["nickname"]);
                }
            }

            return data;
        }

        /// <summary>Saves the full export into the app's folder and returns its path.</summary>
        public async Task<string> ExportAsync()
        {
            string json = await _api.GetTextAsync("/v1/me/data/export", await TokenAsync());
            string path = Path.Combine(_folder, ExportFile);
            System.IO.File.WriteAllText(path, json);
            return path;
        }

        /// <summary>Asks the server to delete the account after the cooling-off days; returns when it will happen.</summary>
        public async Task<DateTime> RequestDeletionAsync()
        {
            var reply = await _api.SendAsync<Deletion>("POST", "/v1/me/deletion-request", null, await TokenAsync());
            return reply.ScheduledFor.ToUniversalTime();
        }

        public async Task CancelDeletionAsync() =>
            await _api.SendAsync("DELETE", "/v1/me/deletion-request", null, await TokenAsync());

        /// <summary>The scheduled deletion date, or null when none is pending.</summary>
        public async Task<DateTime?> DeletionAsync()
        {
            try
            {
                var reply = await _api.SendAsync<Deletion>("GET", "/v1/me/deletion-request", null, await TokenAsync());
                return reply.ScheduledFor.ToUniversalTime();
            }
            catch (ApiException e) when (e.Code == "not_found")
            {
                return null;
            }
        }

        private async Task TryServerWithdrawalAsync()
        {
            try
            {
                await _api.SendAsync("DELETE", "/v1/me/consents", null, await TokenAsync());
                SetPending(false);
            }
            catch (ApiException)
            {
                // Withdrawn here already; the server is told as soon as it can be reached.
                SetPending(true);
            }
        }

        private async Task<string> TokenAsync()
        {
            string token = await _account.GetAccessTokenAsync();
            if (token == null)
            {
                throw new ApiException(401, "invalid_token");
            }

            return token;
        }

        private void SetPending(bool pending)
        {
            string path = Path.Combine(_folder, PendingFile);
            if (pending)
            {
                System.IO.File.WriteAllText(path, "{\"withdrawConsent\":true}");
            }
            else if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

        private sealed class Deletion
        {
            [JsonProperty("scheduled_for")]
            public DateTime ScheduledFor { get; set; }
        }
    }
}
