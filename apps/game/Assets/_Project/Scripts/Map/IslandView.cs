using System;
using System.Collections.Generic;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>
    /// The island overview (PRD: "Ada haritası"). Regions open in order; an open region pulses, closed ones sit
    /// under soft clouds with a padlock. Hotspot positions come from content/map/island.json as fractions of the
    /// illustration, which is drawn "cover" so it fills the screen at any aspect.
    /// </summary>
    public sealed class IslandView : VisualElement, IRefreshable
    {
        private readonly GameServices _services;
        private readonly PartArt _partArt;
        private readonly VisualElement _image = new VisualElement { name = "island-image" };
        private readonly List<(VisualElement View, IslandLayout.Spot Spot, float Size)> _spots = new List<(VisualElement, IslandLayout.Spot, float)>();
        private readonly RobotAvatar _robot;
        private readonly HashSet<string> _open = new HashSet<string>();
        private float _aspect = 16f / 9f;
        private float _time;

        public IslandView(GameServices services, RegionArt art, PartArt partArt, Action<string> onRegion, Action onLocked)
        {
            _services = services;
            _partArt = partArt;
            name = "island";
            AddToClassList("map-view");
            _image.AddToClassList("island__image");
            Add(_image);

            var bg = partArt != null ? partArt.Find(services.Island.Image) : null;
            if (bg != null)
            {
                _image.style.backgroundImage = new StyleBackground(bg);
                _aspect = bg.rect.width / bg.rect.height;
            }

            foreach (var e in services.Catalog.All)
            {
                _open.Add(MapController.WireId(e.Dto.Region));
            }

            int cloud = 0;
            foreach (var region in services.Island.Regions)
            {
                bool open = _open.Contains(region.Id);
                var spot = new VisualElement { name = "region-" + region.Id };
                spot.AddToClassList("island__region");
                spot.EnableInClassList("island__region--open", open);
                if (!open)
                {
                    var sprite = partArt != null ? partArt.Find(cloud++ % 2 == 0 ? "map_cloud_wide" : "map_cloud_round") : null;
                    if (sprite != null)
                    {
                        spot.style.backgroundImage = new StyleBackground(sprite);
                    }

                    var padlock = new Icon(IconKind.Lock) { Color = new Color(0.45f, 0.36f, 0.3f), Accent = Color.white };
                    padlock.AddToClassList("island__lock");
                    spot.Add(padlock);
                }

                string id = region.Id;
                spot.RegisterCallback<ClickEvent>(_ =>
                {
                    if (_open.Contains(id))
                    {
                        onRegion(id);
                    }
                    else
                    {
                        onLocked();
                    }
                });
                _image.Add(spot);
                _spots.Add((spot, region, region.Radius * 2f));
            }

            _robot = new RobotAvatar(partArt, art != null ? art.RobotFront : null) { name = "island-robot" };
            _robot.AddToClassList("island__robot");
            _image.Add(_robot);

            RegisterCallback<GeometryChangedEvent>(_ => Layout());
            schedule.Execute(Pulse).Every(33);
        }

        public void Refresh()
        {
            _robot.Wear(_services.Progress.Book.AllEquipped, _services.Parts);
            Layout();
        }

        private void Layout()
        {
            var r = contentRect;
            if (r.width <= 0f || r.height <= 0f)
            {
                return;
            }

            // "Cover": scale the illustration to fill, centred, cropping the overflow.
            float w = Mathf.Max(r.width, r.height * _aspect);
            float h = w / _aspect;
            _image.style.width = w;
            _image.style.height = h;
            _image.style.left = (r.width - w) * 0.5f;
            _image.style.top = (r.height - h) * 0.5f;

            foreach (var (view, spot, size) in _spots)
            {
                float d = size * w * (view.ClassListContains("island__region--open") ? 0.9f : 1f);
                view.style.width = d;
                view.style.height = d * 0.7f;
                view.style.left = (spot.X * w) - (d * 0.5f);
                view.style.top = (spot.Y * h) - (d * 0.35f);
            }

            // Roboya waits at the forest edge, next to its crashed ship.
            var ship = _services.Island.Ship;
            float rh = h * 0.2f;
            _robot.style.width = rh * 0.95f;
            _robot.style.height = rh;
            _robot.style.left = ((ship != null ? ship.X : 0.15f) * w) + (h * 0.06f);
            _robot.style.top = ((ship != null ? ship.Y : 0.78f) * h) - rh;
        }

        private void Pulse()
        {
            _time += 0.033f;
            float s = 1f + (Mathf.Max(0f, Mathf.Sin(_time * 3f)) * 0.06f);
            foreach (var (view, _, _) in _spots)
            {
                if (view.ClassListContains("island__region--open"))
                {
                    view.style.scale = new Scale(new Vector3(s, s, 1f));
                }
            }
        }
    }
}
