using Roboya.CodingEngine.Play;
using Roboya.CodingEngine.Progress;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Roboya.Core
{
    /// <summary>
    /// The single composition root (CLAUDE.md §7). Lives in the Boot scene, builds services and loads the
    /// first scene, then passes services to that scene's <see cref="ISceneEntry"/>. Also the scene navigator.
    /// </summary>
    public sealed class Bootstrap : MonoBehaviour, ISceneNavigator
    {
        public const string MapScene = "Map";
        public const string GameScene = "Game";

        [SerializeField] private string firstScene = MapScene;

        private GameServices _services;

        public string SelectedLevelId { get; private set; }

        public string PendingMapLine { get; private set; }

        /// <summary>True when the map should open on the region path (coming back from a level).</summary>
        public bool ReturningFromLevel { get; private set; }

        public void ConsumeMapLine() => PendingMapLine = null;

        public void PlayLevel(string levelId)
        {
            SelectedLevelId = levelId;
            _ = LoadAsync(GameScene);
        }

        public void GoToMap(string lineOnArrival = null)
        {
            PendingMapLine = lineOnArrival;
            ReturningFromLevel = true;
            _ = LoadAsync(MapScene);
            _ = _services.Sync.SyncAsync();
            _ = _services.Analytics.FlushAsync();
        }

        private async Awaitable Start()
        {
            // Re-entering the Boot scene must not create a second composition root.
            if (FindObjectsByType<Bootstrap>(FindObjectsSortMode.None).Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            _services = await ComposeAsync();
            await LoadAsync(firstScene);
            // A signed-in parent's profiles and stars catch up in the background; offline simply does nothing.
            _ = _services.Sync.SyncAsync();
            _ = _services.Privacy.FlushPendingAsync();
            _ = _services.Subscription.RecoverAsync();
            _services.Analytics.Track("app_open");
            _ = _services.Analytics.FlushAsync();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && _services != null)
            {
                _ = _services.Analytics.FlushAsync();
            }
        }

        public async Awaitable LoadAsync(string sceneName)
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var entry = root.GetComponentInChildren<ISceneEntry>(true);
                if (entry != null)
                {
                    entry.Enter(_services);
                    return;
                }
            }

            Debug.LogError("Scene '" + sceneName + "' has no ISceneEntry component.");
        }

        private async Awaitable<GameServices> ComposeAsync()
        {
            ILevelSource levels;
#if UNITY_EDITOR
            levels = System.IO.Directory.Exists(FileLevelSource.RepositoryLevelsPath)
                ? new FileLevelSource(FileLevelSource.RepositoryLevelsPath)
                : (ILevelSource)new StreamingAssetsLevelSource();
#else
            levels = new StreamingAssetsLevelSource();
#endif
            var catalog = LevelCatalog.Parse(await levels.LoadAllAsync());
            var parts = ShipPartCatalog.Parse(await ContentFiles.ReadAsync(ShipPartCatalog.File));
            var strings = LocalizedStrings.Parse(await ContentFiles.ReadAsync(LocalizedStrings.File));
            var island = IslandLayout.Parse(await ContentFiles.ReadAsync(IslandLayout.File));
            var notice = LocalNotice.Parse(await ContentFiles.ReadAsync(LocalNotice.File));
            var storeProducts = StoreProducts.Parse(await ContentFiles.ReadAsync(StoreProducts.File));

            IEntitlementSource entitlements = null;
#if UNITY_EDITOR
            if (DevEntitlements.Requested)
            {
                Debug.Log("[dev] " + DevEntitlements.Variable + "=1: all levels unlocked in the editor.");
                entitlements = new DevEntitlements();
            }
#endif

            var profiles = ProfileManager.Load(ProgressFolder, strings.Get(StringKeys.ProfileDefaultNickname));
            var api = ComposeApi();
            var account = ComposeAccount(api);
            var serverEntitlements = new EntitlementService(api, account, ProgressFolder);
            entitlements = entitlements ?? serverEntitlements;
            // Rules will come from server configuration once the API exists (CLAUDE.md §6).
            var sync = new SyncService(api, account, profiles, notice, ProgressFolder, serverEntitlements);
            return new GameServices(
                catalog,
                await ComposeVoiceAsync(),
                SessionRules.Default,
                Rules(),
                profiles,
                entitlements,
                parts,
                island,
                this,
                strings,
                notice,
                new ScreenTimeService(ProgressFolder, profiles),
                account,
                sync,
                new AnalyticsService(api, profiles, notice, ProgressFolder, Application.version, PlatformWord),
                new PrivacyService(api, account, profiles, sync, ProgressFolder),
                new SubscriptionService(api, account, StoreBridgeOverride ?? new Roboya.Services.NoStoreBridge(), serverEntitlements, storeProducts));
        }

        /// <summary>Tests point this at a temporary folder so each run starts with fresh progress.</summary>
        public static string ProgressFolderOverride { get; set; }

        /// <summary>Tests replace the network with a fake and give a placeholder server address.</summary>
        public static Roboya.Services.IHttpTransport TransportOverride { get; set; }

        /// <summary>Tests use a fake store; the native bridges (StoreKit 2, Play Billing) replace the default when they exist.</summary>
        public static Roboya.Services.IStoreBridge StoreBridgeOverride { get; set; }

        private static Roboya.Services.ApiClient ComposeApi()
        {
            string url = Roboya.Services.ApiConfig.Resolve(ProgressFolder);
            return url == null
                ? null
                : new Roboya.Services.ApiClient(url, TransportOverride ?? new Roboya.Services.UnityHttpTransport());
        }

        private static ProgressRules Rules()
        {
#if !UNITY_EDITOR
            if (Debug.isDebugBuild)
            {
                // Development player builds open every level for testing on a device; release builds never do (ADR 0009).
                var d = ProgressRules.Default;
                return new ProgressRules(d.FreeLevelCount, d.LevelsPerPart, d.FreeProfiles, d.PremiumProfiles, unlockAll: true);
            }
#endif
            return ProgressRules.Default;
        }

        private static string PlatformWord => Application.platform == RuntimePlatform.IPhonePlayer ? "ios" : "android";

        private static Roboya.Services.AccountService ComposeAccount(Roboya.Services.ApiClient api)
        {
            return new Roboya.Services.AccountService(api, ProgressFolder, PlatformWord);
        }

        private static string ProgressFolder => ProgressFolderOverride ?? FileProgressStore.DefaultFolder;

        private async Awaitable<IVoicePlayer> ComposeVoiceAsync()
        {
            try
            {
                var library = await VoiceLibrary.LoadAsync(VoiceLibrary.DefaultBaseUrl);
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                return new ClipVoicePlayer(source, library);
            }
            catch (System.IO.IOException e)
            {
                // The game stays playable without narration; testers see the keys in the log.
                Debug.LogWarning(e.Message + " — falling back to logging voice player.");
                return new LoggingVoicePlayer();
            }
        }
    }
}
