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
            Roboya.Services.AccountService account,
            SyncService sync,
            AnalyticsService analytics,
            PrivacyService privacy,
            SubscriptionService subscription)
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
            Sync = sync;
            Analytics = analytics;
            Privacy = privacy;
            Subscription = subscription;
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

        /// <summary>Profile and progress sync with the parent account (F1-15); does nothing offline or signed out.</summary>
        public SyncService Sync { get; }

        /// <summary>Anonymous first-party usage events (F1-18); records nothing without consent or a server.</summary>
        public AnalyticsService Analytics { get; }

        /// <summary>Privacy centre actions: see and save data, withdraw consent, wipe the device, request deletion (F1-13).</summary>
        public PrivacyService Privacy { get; }

        /// <summary>Buying Aile Premium through the store bridge; the server decides what is granted (F1-16).</summary>
        public SubscriptionService Subscription { get; }

        public ISceneNavigator Navigator { get; }

        /// <summary>Text for adult screens only.</summary>
        public LocalizedStrings Strings { get; }

        /// <summary>The privacy notice the parent consents to before the first profile.</summary>
        public LocalNotice Notice { get; }

        /// <summary>Today's play time and the daily limit (VEL-02).</summary>
        public ScreenTimeService ScreenTime { get; }
    }
}
