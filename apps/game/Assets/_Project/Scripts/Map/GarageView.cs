using System;
using System.Collections.Generic;
using Roboya.CodingEngine.Progress;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>
    /// Roboya's garage (ILR-03): every part in the fixed earning order. Earned parts can be put on and taken off
    /// with a tap; parts not earned yet show as faded silhouettes with a padlock. Nothing here can be bought.
    /// </summary>
    public sealed class GarageView : VisualElement, IRefreshable
    {
        private readonly GameServices _services;
        private readonly PartArt _partArt;
        private readonly RobotAvatar _robot;
        private readonly List<(VisualElement Tile, RobotPart Part)> _tiles = new List<(VisualElement, RobotPart)>();
        private HashSet<string> _earned = new HashSet<string>();

        public GarageView(GameServices services, RegionArt art, PartArt partArt, Action onBack)
        {
            _services = services;
            _partArt = partArt;
            name = "garage";
            AddToClassList("map-view");
            AddToClassList("garage");
            if (art != null && art.Background != null)
            {
                style.backgroundImage = new StyleBackground(art.Background);
            }

            var stage = new VisualElement();
            stage.AddToClassList("garage__stage");
            _robot = new RobotAvatar(partArt, art != null ? art.RobotFront : null) { name = "garage-robot" };
            _robot.AddToClassList("garage__robot");
            stage.Add(_robot);
            Add(stage);

            var shelf = new VisualElement { name = "garage-shelf" };
            shelf.AddToClassList("garage__shelf");
            foreach (var part in services.Parts.Parts)
            {
                var tile = new VisualElement { name = "part-tile-" + part.Id };
                tile.AddToClassList("garage__tile");
                var image = new VisualElement();
                image.AddToClassList("garage__part");
                var place = services.Parts.PlacementOf(part.Id);
                var sprite = place != null && partArt != null ? partArt.Find(place.Sprite) : null;
                if (sprite != null)
                {
                    image.style.backgroundImage = new StyleBackground(sprite);
                }

                tile.Add(image);
                var padlock = new Icon(IconKind.Lock) { name = "tile-lock", Color = new Color(0.45f, 0.36f, 0.3f), Accent = Color.white };
                padlock.AddToClassList("garage__lock");
                tile.Add(padlock);
                var p = part;
                tile.RegisterCallback<ClickEvent>(_ => Toggle(p));
                shelf.Add(tile);
                _tiles.Add((tile, part));
            }

            Add(shelf);

            var bar = new VisualElement();
            bar.AddToClassList("map-bar");
            bar.Add(new IconButton(IconKind.Home, onBack) { name = "garage-back" });
            Add(bar);
        }

        public void Refresh()
        {
            _earned = new HashSet<string>();
            foreach (var part in RewardRules.Earned(_services.Parts.Parts, ProgressQueries.EarnedParts(_services)))
            {
                _earned.Add(part.Id);
            }

            var book = _services.Progress.Book;
            foreach (var (tile, part) in _tiles)
            {
                bool earned = _earned.Contains(part.Id);
                tile.EnableInClassList("garage__tile--earned", earned);
                tile.EnableInClassList("garage__tile--worn", earned && book.Equipped(part.Slot) == part.Id);
                tile.Q("tile-lock").style.display = earned ? DisplayStyle.None : DisplayStyle.Flex;
            }

            _robot.Wear(book.AllEquipped, _services.Parts);
        }

        private void Toggle(RobotPart part)
        {
            if (!_earned.Contains(part.Id))
            {
                _services.Voice.Play(MapController.LockedWaitVoice);
                return;
            }

            var book = _services.Progress.Book;
            book.Equip(part.Slot, book.Equipped(part.Slot) == part.Id ? null : part.Id);
            try
            {
                _services.Progress.Save();
            }
            catch (System.IO.IOException e)
            {
                Debug.LogException(e);
            }

            Refresh();
        }
    }
}
