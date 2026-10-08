using System.Collections.Generic;
using Roboya.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.UI
{
    /// <summary>
    /// Roboya's front view wearing the parts equipped in the garage (ILR-03): a colour variant replaces the body,
    /// other parts are layered at the catalog placements (fractions of the front sprite). Used on the island,
    /// the region path and in the garage; story scenes and the board keep the plain sprites for now.
    /// </summary>
    public sealed class RobotAvatar : VisualElement
    {
        private readonly PartArt _partArt;
        private readonly Sprite _front;
        private readonly VisualElement _back = new VisualElement();
        private readonly VisualElement _body = new VisualElement();
        private readonly VisualElement _over = new VisualElement();
        private readonly List<(VisualElement View, PartPlacement Place, float Aspect)> _layers = new List<(VisualElement, PartPlacement, float)>();
        private float _bodyAspect = 1f;

        public RobotAvatar(PartArt partArt, Sprite front)
        {
            _partArt = partArt;
            _front = front;
            pickingMode = PickingMode.Ignore;
            AddToClassList("robot-avatar");
            foreach (var layer in new[] { _back, _body, _over })
            {
                layer.pickingMode = PickingMode.Ignore;
                layer.style.position = Position.Absolute;
                layer.style.left = 0;
                layer.style.top = 0;
                layer.style.right = 0;
                layer.style.bottom = 0;
                Add(layer);
            }

            _body.style.backgroundImage = new StyleBackground(front);
            _body.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            _bodyAspect = Aspect(front);
            RegisterCallback<GeometryChangedEvent>(_ => Layout());
        }

        /// <summary>Redraws Roboya wearing <paramref name="equipped"/> (slot → part id).</summary>
        public void Wear(IReadOnlyDictionary<string, string> equipped, RobotPartCatalog catalog)
        {
            _back.Clear();
            _over.Clear();
            _layers.Clear();
            var body = _front;
            if (equipped != null && catalog != null)
            {
                foreach (var pair in equipped)
                {
                    var place = catalog.PlacementOf(pair.Value);
                    var sprite = place != null && _partArt != null ? _partArt.Find(place.Sprite) : null;
                    if (sprite == null)
                    {
                        continue;
                    }

                    if (pair.Key == RobotPartCatalog.ColorSlot)
                    {
                        body = sprite;
                        continue;
                    }

                    var view = new VisualElement { pickingMode = PickingMode.Ignore, name = "part-" + pair.Value };
                    view.style.position = Position.Absolute;
                    view.style.backgroundImage = new StyleBackground(sprite);
                    view.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                    (place.Behind ? _back : _over).Add(view);
                    _layers.Add((view, place, Aspect(sprite)));
                }
            }

            _body.style.backgroundImage = new StyleBackground(body);
            _bodyAspect = Aspect(body);
            Layout();
        }

        private void Layout()
        {
            var r = contentRect;
            if (r.width <= 0f || r.height <= 0f)
            {
                return;
            }

            // The body is fitted into the element; parts are placed relative to the fitted body rect.
            float bw = Mathf.Min(r.width, r.height * _bodyAspect);
            float bh = bw / _bodyAspect;
            float bx = (r.width - bw) * 0.5f;
            float by = r.height - bh;
            foreach (var (view, place, aspect) in _layers)
            {
                float w = place.Width * bw;
                float h = w / aspect;
                view.style.width = w;
                view.style.height = h;
                view.style.left = bx + (place.X * bw) - (w * 0.5f);
                view.style.top = by + (place.Y * bh) - (h * 0.5f);
            }

            _body.style.top = by;
            _body.style.height = bh;
            _body.style.bottom = StyleKeyword.Auto;
        }

        private static float Aspect(Sprite s) => s != null && s.rect.height > 0f ? s.rect.width / s.rect.height : 1f;
    }
}
