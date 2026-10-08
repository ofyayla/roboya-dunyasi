using Roboya.CodingEngine.Levels.Generated;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Games.YonAvcisi
{
    /// <summary>Thin scene entry: loads levels and hands the UI root to the controller.</summary>
    public sealed class YonAvcisiScreen : MonoBehaviour, ISceneEntry
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private int startLevelIndex;
        [SerializeField] private RegionArt art;

        private YonAvcisiController _controller;

        public async void Enter(GameServices services)
        {
            try
            {
                var catalog = LevelCatalog.Parse(await services.Levels.LoadAllAsync());
                var levels = catalog.ForGame(GameId.YonAvcisi);
                if (levels.Count == 0)
                {
                    Debug.LogError("No Yön Avcısı levels found.");
                    return;
                }

                _controller = new YonAvcisiController(document.rootVisualElement, services, levels, art);
                _controller.Start(startLevelIndex);
            }
            catch (System.Exception e)
            {
                // async void: log instead of losing the exception.
                Debug.LogException(e);
            }
        }

        private void OnDestroy()
        {
            _controller?.Dispose();
        }
    }
}
