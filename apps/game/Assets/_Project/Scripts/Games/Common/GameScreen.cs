using System.Linq;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Games.Common
{
    /// <summary>Thin scene entry: opens the level chosen on the map and hands the UI root to the controller.</summary>
    public sealed class GameScreen : MonoBehaviour, ISceneEntry
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private RegionArt art;
        [SerializeField] private PartArt partArt;

        private GameController _controller;
        private GameServices _services;
        private bool _limitReported;

        public void Enter(GameServices services)
        {
            _services = services;
            // One path per region, all games mixed in play order: "next" simply follows it.
            var selected = services.Catalog.Find(services.Navigator.SelectedLevelId);
            var region = selected != null ? selected.Dto.Region : RegionId.SabirOrmani;
            var levels = services.Catalog.All.Where(l => l.Dto.Region == region).ToList();
            if (levels.Count == 0)
            {
                Debug.LogError("No levels found for region " + region);
                return;
            }

            int start = levels.FindIndex(l => l.Id == services.Navigator.SelectedLevelId);
            _controller = new GameController(document.rootVisualElement, services, levels, art, partArt);
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
