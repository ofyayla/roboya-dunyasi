using Roboya.CodingEngine.Play;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Roboya.Core
{
    /// <summary>
    /// The single composition root (CLAUDE.md §7). Lives in the Boot scene, builds services and loads the
    /// first scene, then passes services to that scene's <see cref="ISceneEntry"/>.
    /// </summary>
    public sealed class Bootstrap : MonoBehaviour
    {
        [SerializeField] private string firstScene = "Game";

        private GameServices _services;

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
            _services = Compose();
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

        private static GameServices Compose()
        {
            ILevelSource levels;
#if UNITY_EDITOR
            levels = System.IO.Directory.Exists(FileLevelSource.RepositoryLevelsPath)
                ? new FileLevelSource(FileLevelSource.RepositoryLevelsPath)
                : (ILevelSource)new StreamingAssetsLevelSource();
#else
            levels = new StreamingAssetsLevelSource();
#endif
            // Rules will come from server configuration once the API exists (CLAUDE.md §6).
            return new GameServices(levels, new LoggingVoicePlayer(), SessionRules.Default);
        }
    }
}
