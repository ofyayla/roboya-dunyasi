using Roboya.CodingEngine.Play;
using Roboya.CodingEngine.Progress;

namespace Roboya.Core
{
    /// <summary>Everything a scene needs, built once by <see cref="Bootstrap"/> and passed in (no singletons).</summary>
    public sealed class GameServices
    {
        public GameServices(
            LevelCatalog catalog,
            IVoicePlayer voice,
            SessionRules rules,
            ProgressRules progressRules,
            ProfileManager profiles,
            IEntitlementSource entitlements,
            ShipPartCatalog parts,
            IslandLayout island,
            ISceneNavigator navigator,
            LocalizedStrings strings,
            LocalNotice notice,
            ScreenTimeService screenTime,
            Roboya.Services.AccountService account)
        {
            Catalog = catalog;
            Voice = voice;
            Rules = rules;
            ProgressRules = progressRules;
            Profiles = profiles;
            Entitlements = entitlements;
            Parts = parts;
            Island = island;
            Navigator = navigator;
            Strings = strings;
            Notice = notice;
            ScreenTime = screenTime;
            Account = account;
        }

        /// <summary>All levels, parsed once at start-up.</summary>
        public LevelCatalog Catalog { get; }

        public IVoicePlayer Voice { get; }

        public SessionRules Rules { get; }

        public ProgressRules ProgressRules { get; }

        /// <summary>The child profiles on this device and the parent's consent (F1-10).</summary>
        public ProfileManager Profiles { get; }

        /// <summary>The active profile's progress.</summary>
        public IProgressStore Progress => Profiles.Progress;

        /// <summary>Server-backed premium flag; never computed on the client (golden rule 3).</summary>
        public IEntitlementSource Entitlements { get; }

        /// <summary>Ship repair parts earned with progress (ILR-03).</summary>
        public ShipPartCatalog Parts { get; }

        public IslandLayout Island { get; }

        /// <summary>The parent's account (F1-14); signed out by default and absent when no server is configured.</summary>
        public Roboya.Services.AccountService Account { get; }

        public ISceneNavigator Navigator { get; }

        /// <summary>Text for adult screens only.</summary>
        public LocalizedStrings Strings { get; }

        /// <summary>The privacy notice the parent consents to before the first profile.</summary>
        public LocalNotice Notice { get; }

        /// <summary>Today's play time and the daily limit (VEL-02).</summary>
        public ScreenTimeService ScreenTime { get; }
    }
}
