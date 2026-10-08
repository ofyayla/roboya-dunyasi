using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>Thin scene entry for the Map scene: island, region path and garage (F1-05, ILR-03).</summary>
    public sealed class MapScreen : MonoBehaviour, ISceneEntry
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private RegionArt art;
        [SerializeField] private PartArt partArt;

        private MapController _controller;

        public void Enter(GameServices services)
        {
            _controller = new MapController(document.rootVisualElement, services, art, partArt);
            _controller.Open();
        }

        private void OnDestroy()
        {
            _controller?.Dispose();
        }
    }
}
