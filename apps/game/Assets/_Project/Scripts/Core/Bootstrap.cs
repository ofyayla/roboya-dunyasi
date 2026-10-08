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

            IEntitlementSource entitlements = new FreeTierEntitlements();
#if UNITY_EDITOR
            if (DevEntitlements.Requested)
            {
                Debug.Log("[dev] " + DevEntitlements.Variable + "=1: all levels unlocked in the editor.");
                entitlements = new DevEntitlements();
            }
#endif

            // Rules will come from server configuration once the API exists (CLAUDE.md §6).
            return new GameServices(
                catalog,
                await ComposeVoiceAsync(),
                SessionRules.Default,
                ProgressRules.Default,
                new FileProgressStore(ProgressFolder),
                entitlements,
                parts,
                island,
                this,
                strings);
        }

        /// <summary>Tests point this at a temporary folder so each run starts with fresh progress.</summary>
        public static string ProgressFolderOverride { get; set; }

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
