using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Roboya.CodingEngine.Profiles;
using Roboya.Services;

namespace Roboya.Core
{
    public enum SyncResult
    {
        /// <summary>No server configured, or the server cannot be reached: the game simply keeps working offline.</summary>
        Offline,
        NotSignedIn,

        /// <summary>The parent has not accepted the current notice on this device, so no child data is sent (UYM-01).</summary>
        ConsentNeeded,

        /// <summary>The server's notice is a different version than this app's.</summary>
        NoticeOutdated,

        /// <summary>Done, but at least one profile could not be stored (the account's profile limit).</summary>
        PartlyDone,
        Done,
        Failed,
    }

    /// <summary>
    /// Keeps child profiles and stars in step with the parent's account (F1-15 client, ADR 0020). The server keeps the
    /// highest stars per level, so syncing is repeatable and safe with several devices; local data is never lowered.
    /// A profile removed here is removed on the server on the next sync.
    /// </summary>
    public sealed class SyncService
    {
        public const string File = "sync.json";

        private readonly ApiClient _api;
        private readonly AccountService _account;
        private readonly ProfileManager _profiles;
        private readonly LocalNotice _notice;
        private readonly EntitlementService _entitlements;
        private readonly string _path;
        private readonly HashSet<string> _onServer;
        private bool _running;

        public SyncService(ApiClient api, AccountService account, ProfileManager profiles, LocalNotice notice, string folder, EntitlementService entitlements = null)
        {
            _entitlements = entitlements;
            _api = api;
            _account = account;
            _profiles = profiles;
            _notice = notice;
            _path = Path.Combine(folder, File);
            _onServer = LoadIds(_path);
        }

        public SyncResult LastResult { get; private set; } = SyncResult.Offline;

        public event Action Finished;

        public async Task<SyncResult> SyncAsync()
        {
            if (_running)
            {
                return LastResult;
            }

            _running = true;
            try
            {
                LastResult = await RunAsync();
            }
            finally
            {
                _running = false;
            }

            Finished?.Invoke();
            return LastResult;
        }

        private async Task<SyncResult> RunAsync()
        {
            if (_api == null)
            {
                return SyncResult.Offline;
            }

            if (!_account.IsSignedIn)
            {
                return SyncResult.NotSignedIn;
            }

            if (!_profiles.Registry.HasConsentFor(_notice.Version))
            {
                return SyncResult.ConsentNeeded;
            }

            try
            {
                string token = await _account.GetAccessTokenAsync();
                if (token == null)
                {
                    return _account.IsSignedIn ? SyncResult.Offline : SyncResult.NotSignedIn;
                }

                await _api.SendAsync("POST", "/v1/me/consents", new { notice_version = _notice.Version }, token);
                // First, so the profile limit the server applies below matches what this device shows.
                if (_entitlements != null)
                {
                    await _entitlements.RefreshAsync();
                }

                bool partly = false;
                foreach (var profile in new List<ChildProfileData>(_profiles.Registry.Profiles))
                {
                    partly |= !await SyncProfileAsync(profile, token);
                }

                await RemoveDeletedAsync(token);
                return partly ? SyncResult.PartlyDone : SyncResult.Done;
            }
            catch (ApiException e) when (e.IsNetwork)
            {
                return SyncResult.Offline;
            }
            catch (ApiException e) when (e.Code == "notice_outdated")
            {
                return SyncResult.NoticeOutdated;
            }
            catch (ApiException)
            {
                return SyncResult.Failed;
            }
        }

        /// <summary>False when the server refused this profile because of the account's profile limit.</summary>
        private async Task<bool> SyncProfileAsync(ChildProfileData profile, string token)
        {
            string path = "/v1/me/profiles/" + profile.Id;
            try
            {
                await _api.SendAsync("PUT", path, new
                {
                    nickname = profile.Nickname,
                    avatar_id = profile.AvatarId,
                    age_band = profile.AgeBand.ToString().ToLowerInvariant(),
                }, token);
            }
            catch (ApiException e) when (e.Code == "profile_limit" || e.Code == "not_found")
            {
                return false;
            }

            var store = _profiles.ProgressOf(profile.Id);
            var merged = await _api.SendAsync<StarsReply>(
                "POST", path + "/progress/sync", new { stars = new Dictionary<string, int>(store.Book.AllStars) }, token);
            bool changed = false;
            foreach (var pair in merged.Stars)
            {
                changed |= store.Book.Record(pair.Key, pair.Value);
            }

            if (changed)
            {
                store.Save();
            }

            if (_onServer.Add(profile.Id))
            {
                SaveIds();
            }

            return true;
        }

        private async Task RemoveDeletedAsync(string token)
        {
            foreach (var id in new List<string>(_onServer))
            {
                if (_profiles.Registry.Find(id) != null)
                {
                    continue;
                }

                try
                {
                    await _api.SendAsync("DELETE", "/v1/me/profiles/" + id, null, token);
                }
                catch (ApiException e) when (e.Code != "not_found" && !e.IsNetwork)
                {
                    continue;
                }

                _onServer.Remove(id);
                SaveIds();
            }
        }

        private void SaveIds()
        {
            string temp = _path + ".tmp";
            System.IO.File.WriteAllText(temp, JsonConvert.SerializeObject(new State { OnServer = new List<string>(_onServer) }));
            if (System.IO.File.Exists(_path))
            {
                System.IO.File.Replace(temp, _path, null);
            }
            else
            {
                System.IO.File.Move(temp, _path);
            }
        }

        private static HashSet<string> LoadIds(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                return new HashSet<string>();
            }

            try
            {
                var state = JsonConvert.DeserializeObject<State>(System.IO.File.ReadAllText(path));
                return new HashSet<string>(state?.OnServer ?? new List<string>());
            }
            catch (JsonException)
            {
                // Only a to-do list for the server; losing it is harmless.
                return new HashSet<string>();
            }
        }

        private sealed class State
        {
            [JsonProperty("onServer")]
            public List<string> OnServer { get; set; }
        }

        private sealed class StarsReply
        {
            [JsonProperty("stars")]
            public Dictionary<string, int> Stars { get; set; } = new Dictionary<string, int>();
        }
    }
}
