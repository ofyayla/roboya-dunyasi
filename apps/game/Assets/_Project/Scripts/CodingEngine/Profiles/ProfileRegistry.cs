using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Roboya.CodingEngine.Profiles
{
    /// <summary>Age band chosen by the parent; it sets where the child starts (F1-08). Wire values are lower case.</summary>
    public enum AgeBand
    {
        Minik,
        Kasif,
        Mucit,
    }

    /// <summary>
    /// A child profile with only the fields the product allows (CLAUDE.md §11): a nickname, an avatar and an age band.
    /// Never a real name, birth date, photo or any device identifier.
    /// </summary>
    public sealed class ChildProfileData
    {
        public const int MaxNicknameLength = 24;
        private static readonly Regex AvatarPattern = new Regex("^[a-z0-9-]{1,32}$", RegexOptions.Compiled);

        [JsonConstructor]
        public ChildProfileData(string id, string nickname, string avatarId, AgeBand ageBand)
        {
            Id = id;
            Nickname = nickname;
            AvatarId = avatarId;
            AgeBand = ageBand;
        }

        [JsonProperty("id")]
        public string Id { get; }

        [JsonProperty("nickname")]
        public string Nickname { get; internal set; }

        [JsonProperty("avatarId")]
        public string AvatarId { get; internal set; }

        [JsonProperty("ageBand")]
        public AgeBand AgeBand { get; internal set; }

        /// <summary>Trims the nickname and checks every field; returns null when valid, otherwise a short code.</summary>
        public static string Validate(ref string nickname, string avatarId)
        {
            nickname = (nickname ?? string.Empty).Trim();
            if (nickname.Length == 0)
            {
                return "nickname_empty";
            }

            if (nickname.Length > MaxNicknameLength)
            {
                return "nickname_too_long";
            }

            return avatarId != null && AvatarPattern.IsMatch(avatarId) ? null : "avatar_invalid";
        }
    }

    /// <summary>The parent's consent to one notice version, kept on the device (UYM-01) until it can be sent with an account.</summary>
    public sealed class ConsentRecord
    {
        [JsonConstructor]
        public ConsentRecord(string noticeVersion, string acceptedAtUtc)
        {
            NoticeVersion = noticeVersion;
            AcceptedAtUtc = acceptedAtUtc;
        }

        [JsonProperty("noticeVersion")]
        public string NoticeVersion { get; }

        [JsonProperty("acceptedAtUtc")]
        public string AcceptedAtUtc { get; }
    }

    /// <summary>
    /// The profiles on this device, which one is active, and the parent's consent. Pure logic; the file store and the
    /// profile limit (1 free, 4 premium, from the server) live outside. Holds no personal data beyond the allowed fields.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ProfileRegistry
    {
        public const int CurrentVersion = 1;

        [JsonProperty("version")]
        private int _version = CurrentVersion;

        [JsonProperty("profiles")]
        private List<ChildProfileData> _profiles = new List<ChildProfileData>();

        [JsonProperty("activeId")]
        private string _activeId;

        [JsonProperty("consent")]
        private ConsentRecord _consent;

        public IReadOnlyList<ChildProfileData> Profiles => _profiles;

        public ChildProfileData Active => Find(_activeId);

        public ConsentRecord Consent => _consent;

        public ChildProfileData Find(string id)
        {
            foreach (var p in _profiles)
            {
                if (p.Id == id)
                {
                    return p;
                }
            }

            return null;
        }

        /// <summary>True when the parent consented to exactly this notice version.</summary>
        public bool HasConsentFor(string noticeVersion) =>
            _consent != null && !string.IsNullOrEmpty(noticeVersion) && _consent.NoticeVersion == noticeVersion;

        public void RecordConsent(string noticeVersion, DateTime whenUtc)
        {
            if (string.IsNullOrEmpty(noticeVersion))
            {
                throw new ArgumentException("A notice version is required.", nameof(noticeVersion));
            }

            _consent = new ConsentRecord(noticeVersion, whenUtc.ToUniversalTime().ToString("o"));
        }

        public void WithdrawConsent() => _consent = null;

        /// <summary>Adds a profile and makes it active when it is the first. Throws <see cref="ArgumentException"/> on bad fields.</summary>
        public ChildProfileData Add(string nickname, string avatarId, AgeBand band, string id = null)
        {
            var problem = ChildProfileData.Validate(ref nickname, avatarId);
            if (problem != null)
            {
                throw new ArgumentException(problem);
            }

            if (id != null && Find(id) != null)
            {
                throw new ArgumentException("duplicate_id");
            }

            var profile = new ChildProfileData(id ?? Guid.NewGuid().ToString("N"), nickname, avatarId, band);
            _profiles.Add(profile);
            if (_activeId == null)
            {
                _activeId = profile.Id;
            }

            return profile;
        }

        public void Update(string id, string nickname, string avatarId, AgeBand band)
        {
            var profile = Find(id) ?? throw new ArgumentException("unknown_profile");
            var problem = ChildProfileData.Validate(ref nickname, avatarId);
            if (problem != null)
            {
                throw new ArgumentException(problem);
            }

            profile.Nickname = nickname;
            profile.AvatarId = avatarId;
            profile.AgeBand = band;
        }

        public void SetActive(string id)
        {
            if (Find(id) == null)
            {
                throw new ArgumentException("unknown_profile");
            }

            _activeId = id;
        }

        /// <summary>Removes a profile; when it was active the first remaining one takes over (or none).</summary>
        public bool Remove(string id)
        {
            var profile = Find(id);
            if (profile == null)
            {
                return false;
            }

            _profiles.Remove(profile);
            if (_activeId == id)
            {
                _activeId = _profiles.Count > 0 ? _profiles[0].Id : null;
            }

            return true;
        }

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter(new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy()));

        public static ProfileRegistry FromJson(string json)
        {
            ProfileRegistry registry;
            try
            {
                registry = JsonConvert.DeserializeObject<ProfileRegistry>(json, new Newtonsoft.Json.Converters.StringEnumConverter(new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy()));
            }
            catch (JsonException e)
            {
                throw new FormatException("Profile file is malformed: " + e.Message, e);
            }

            if (registry == null)
            {
                throw new FormatException("Profile file is empty.");
            }

            if (registry._version > CurrentVersion)
            {
                throw new FormatException("Profile file version " + registry._version + " is newer than this app.");
            }

            registry._profiles = registry._profiles ?? new List<ChildProfileData>();
            if (registry._activeId != null && registry.Find(registry._activeId) == null)
            {
                registry._activeId = registry._profiles.Count > 0 ? registry._profiles[0].Id : null;
            }

            return registry;
        }
    }
}
