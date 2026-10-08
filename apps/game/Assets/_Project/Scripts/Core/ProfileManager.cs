using System;
using System.Collections.Generic;
using System.IO;
using Roboya.CodingEngine.Profiles;
using Roboya.CodingEngine.Progress;
using UnityEngine;

namespace Roboya.Core
{
    /// <summary>
    /// The child profiles on this device (F1-10) and the progress book of the active one. A profile has its own
    /// progress file; the registry (profiles.json) holds the allowed fields only and the parent's consent. Writes are
    /// atomic. Installs from before profiles keep their progress as a first profile.
    /// </summary>
    public sealed class ProfileManager
    {
        public const string File = "profiles.json";

        private readonly string _folder;
        private readonly Dictionary<string, FileProgressStore> _stores = new Dictionary<string, FileProgressStore>();
        private readonly IProgressStore _none = new MemoryProgressStore();

        private ProfileManager(string folder, ProfileRegistry registry)
        {
            _folder = folder;
            Registry = registry;
        }

        /// <summary>Raised after the active profile changed, was added, edited or removed.</summary>
        public event Action Changed;

        public ProfileRegistry Registry { get; }

        public ChildProfileData Active => Registry.Active;

        public bool HasActive => Registry.Active != null;

        /// <summary>The active profile's progress; an empty throw-away book when there is no profile yet (onboarding).</summary>
        public IProgressStore Progress => HasActive ? StoreFor(Registry.Active.Id) : _none;

        public static ProfileManager Load(string folder, string defaultNickname = "Mucit", string defaultAvatar = "robot-turuncu")
        {
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, File);
            if (System.IO.File.Exists(path))
            {
                try
                {
                    return new ProfileManager(folder, ProfileRegistry.FromJson(System.IO.File.ReadAllText(path)));
                }
                catch (FormatException e)
                {
                    // Keep the bad file for support and start the onboarding again; never crash on it.
                    string aside = path + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                    System.IO.File.Move(path, aside);
                    Debug.LogWarning("Profile file unreadable, starting fresh: " + e.Message);
                }
            }

            var registry = new ProfileRegistry();
            string legacy = FileProgressStore.LegacyProfileId(folder);
            if (legacy != null)
            {
                // An install from before profiles: keep its progress as the first profile. Consent is still asked.
                registry.Add(defaultNickname, defaultAvatar, AgeBand.Minik, legacy);
            }

            var manager = new ProfileManager(folder, registry);
            if (legacy != null)
            {
                manager.Save();
            }

            return manager;
        }

        public ChildProfileData Add(string nickname, string avatarId, AgeBand band)
        {
            var profile = Registry.Add(nickname, avatarId, band);
            SaveAndNotify();
            return profile;
        }

        public void Update(string id, string nickname, string avatarId, AgeBand band)
        {
            Registry.Update(id, nickname, avatarId, band);
            SaveAndNotify();
        }

        public void SetActive(string id)
        {
            Registry.SetActive(id);
            SaveAndNotify();
        }

        /// <summary>Removes the profile and its progress file for good.</summary>
        public bool Remove(string id)
        {
            if (!Registry.Remove(id))
            {
                return false;
            }

            _stores.Remove(id);
            FileProgressStore.Delete(_folder, id);
            SaveAndNotify();
            return true;
        }

        public void RecordConsent(string noticeVersion)
        {
            Registry.RecordConsent(noticeVersion, DateTime.UtcNow);
            SaveAndNotify();
        }

        public void WithdrawConsent()
        {
            Registry.WithdrawConsent();
            SaveAndNotify();
        }

        public void Save()
        {
            string path = Path.Combine(_folder, File);
            string temp = path + ".tmp";
            System.IO.File.WriteAllText(temp, Registry.ToJson());
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Replace(temp, path, null);
            }
            else
            {
                System.IO.File.Move(temp, path);
            }
        }

        private void SaveAndNotify()
        {
            Save();
            Changed?.Invoke();
        }

        private IProgressStore StoreFor(string id)
        {
            if (!_stores.TryGetValue(id, out var store))
            {
                store = new FileProgressStore(_folder, id);
                _stores[id] = store;
            }

            return store;
        }

        /// <summary>Used before the first profile exists so screens never see a null book.</summary>
        private sealed class MemoryProgressStore : IProgressStore
        {
            public ProgressBook Book { get; } = new ProgressBook();

            public void Save()
            {
            }
        }
    }
}
