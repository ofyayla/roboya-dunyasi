using System;
using System.Collections.Generic;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>
    /// The ship workshop (ILR-03): Roboya beside its ship, with every repair part in the fixed earning order.
    /// Earned parts are fitted on the ship and filled in the row below; the rest show as faint shapes with a
    /// padlock, so the child sees how far the repair has come. Nothing is chosen, equipped or bought here.
    /// </summary>
    public sealed class WorkshopView : VisualElement, IRefreshable
    {
        private readonly GameServices _services;
        private readonly ShipView _ship;
        private readonly List<(VisualElement Tile, int Order)> _tiles = new List<(VisualElement, int)>();

        public WorkshopView(GameServices services, RegionArt art, PartArt partArt, Action onBack)
        {
            _services = services;
            name = "workshop";
            AddToClassList("map-view");
            AddToClassList("workshop");
            if (art != null && art.Background != null)
            {
                style.backgroundImage = new StyleBackground(art.Background);
            }

            var stage = new VisualElement();
            stage.AddToClassList("workshop__stage");
            _ship = new ShipView(partArt, services.Parts) { name = "workshop-ship" };
            _ship.AddToClassList("workshop__ship");
            stage.Add(_ship);
            var robot = new VisualElement { name = "workshop-robot", pickingMode = PickingMode.Ignore };
            robot.AddToClassList("workshop__robot");
            if (art != null && art.RobotFront != null)
            {
                robot.style.backgroundImage = new StyleBackground(art.RobotFront);
            }

            stage.Add(robot);
            Add(stage);

            var row = new VisualElement { name = "workshop-row" };
            row.AddToClassList("workshop__row");
            foreach (var part in services.Parts.Parts)
            {
                var tile = new VisualElement { name = "part-tile-" + part.Id };
                tile.AddToClassList("workshop__tile");
                var layers = services.Parts.LayersOf(part.Id);
                var sprite = layers.Count > 0 && partArt != null ? partArt.Find(layers[0].Sprite) : null;
                var image = new VisualElement();
                image.AddToClassList("workshop__part");
                if (sprite != null)
                {
                    image.style.backgroundImage = new StyleBackground(sprite);
                }

                tile.Add(image);
                var padlock = new Icon(IconKind.Lock) { name = "tile-lock", Color = new Color(0.45f, 0.36f, 0.3f), Accent = Color.white };
                padlock.AddToClassList("workshop__lock");
                tile.Add(padlock);
                tile.RegisterCallback<ClickEvent>(_ =>
                {
                    if (!tile.ClassListContains("workshop__tile--earned"))
                    {
                        _services.Voice.Play(MapController.LockedWaitVoice);
                    }
                });
                row.Add(tile);
                _tiles.Add((tile, part.Order));
            }

            Add(row);

            var bar = new VisualElement();
            bar.AddToClassList("map-bar");
            bar.Add(new IconButton(IconKind.Home, onBack) { name = "workshop-back" });
            Add(bar);
        }

        public void Refresh()
        {
            var book = _services.Progress.Book;
            int earned = ProgressQueries.EarnedParts(_services);
            _ship.Show(earned, book.ShipPartsSeen, showComing: true, () => _services.Sfx.Play(SfxKind.Place));
            foreach (var (tile, order) in _tiles)
            {
                bool owned = order <= earned;
                tile.EnableInClassList("workshop__tile--earned", owned);
                tile.Q("tile-lock").style.display = owned ? DisplayStyle.None : DisplayStyle.Flex;
            }

            if (book.MarkShipPartsSeen(earned))
            {
                try
                {
                    _services.Progress.Save();
                }
                catch (System.IO.IOException e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}
