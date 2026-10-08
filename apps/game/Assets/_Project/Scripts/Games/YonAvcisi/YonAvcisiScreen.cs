using Roboya.CodingEngine.Levels.Generated;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Games.YonAvcisi
{
    /// <summary>Thin scene entry: opens the level chosen on the map and hands the UI root to the controller.</summary>
    public sealed class YonAvcisiScreen : MonoBehaviour, ISceneEntry
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private RegionArt art;
        [SerializeField] private PartArt partArt;

        private YonAvcisiController _controller;
        private GameServices _services;
        private bool _limitReported;

        public void Enter(GameServices services)
        {
            _services = services;
            var levels = services.Catalog.ForGame(GameId.YonAvcisi);
            if (levels.Count == 0)
            {
                Debug.LogError("No Yön Avcısı levels found.");
                return;
            }

            int start = levels.FindIndex(l => l.Id == services.Navigator.SelectedLevelId);
            _controller = new YonAvcisiController(document.rootVisualElement, services, levels, art, partArt);
            _controller.Start(start < 0 ? 0 : start);
        }

        private void Update()
        {
            // Only time spent inside a level counts; the map and the parent area do not.
            _services?.ScreenTime.Tick(Time.unscaledDeltaTime);
            if (_services != null && !_limitReported && _services.ScreenTime.IsExhausted)
            {
                _limitReported = true;
                _services.Analytics.Track(
                    "session_limit_reached", null, AnalyticsService.Props("limit_minutes", Mathf.Clamp(_services.ScreenTime.LimitMinutes, 0, 720)));
            }
        }

        private void OnDestroy()
        {
            _services?.ScreenTime.Flush();
            _controller?.Dispose();
        }
    }
}
