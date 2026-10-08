using System.Collections.Generic;
using Roboya.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.UI
{
    /// <summary>
    /// Roboya's crashed ship with the repair parts earned so far (ILR-03). Parts sit at catalog placements
    /// (fractions of the body sprite); parts not yet watched drop into place once, so each repair is a small
    /// event. In the workshop, parts still to come show as faint shapes so the child sees what is left.
    /// </summary>
    public sealed class ShipView : VisualElement
    {
        // Parts reach about 15% beyond the body on every side; the body is fitted with that margin.
        private const float Margin = 1.32f;
        private const float DropSeconds = 0.55f;

        private readonly PartArt _art;
        private readonly ShipPartCatalog _catalog;
        private readonly VisualElement _back = new VisualElement();
        private readonly VisualElement _body = new VisualElement { name = "ship-body" };
        private readonly VisualElement _front = new VisualElement();
        private readonly List<Layer> _layers = new List<Layer>();
        private float _bodyAspect = 1f;

        public ShipView(PartArt art, ShipPartCatalog catalog)
        {
            _art = art;
            _catalog = catalog;
            pickingMode = PickingMode.Ignore;
            AddToClassList("ship");
            foreach (var layer in new[] { _back, _body, _front })
            {
                layer.pickingMode = PickingMode.Ignore;
                layer.style.position = Position.Absolute;
                Add(layer);
            }

            var body = art != null && catalog != null ? art.Find(catalog.BaseSprite) : null;
            if (body != null)
            {
                _body.style.backgroundImage = new StyleBackground(body);
                _body.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                _bodyAspect = Aspect(body);
            }

            RegisterCallback<GeometryChangedEvent>(_ => Layout());
        }

        /// <summary>
        /// Shows the first <paramref name="earned"/> parts. Parts after <paramref name="seen"/> drop in one by one.
        /// With <paramref name="showComing"/>, parts still to be earned appear as faint shapes.
        /// </summary>
        public void Show(int earned, int seen, bool showComing)
        {
            _back.Clear();
            _front.Clear();
            _layers.Clear();
            if (_catalog == null || _art == null)
            {
                return;
            }

            int stagger = 0;
            foreach (var part in _catalog.Parts)
            {
                bool owned = part.Order <= earned;
                if (!owned && !showComing)
                {
                    continue;
                }

                bool fresh = owned && part.Order > seen;
                foreach (var layer in _catalog.LayersOf(part.Id))
                {
                    var sprite = _art.Find(layer.Sprite);
                    if (sprite == null)
                    {
                        continue;
                    }

                    var view = new VisualElement { pickingMode = PickingMode.Ignore, name = "ship-" + part.Id };
                    view.style.position = Position.Absolute;
                    view.style.backgroundImage = new StyleBackground(sprite);
                    view.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                    view.EnableInClassList("ship__part--coming", !owned);
                    view.EnableInClassList("ship__part--owned", owned);
                    // Faint shapes of parts still to come stay behind the body so they never smudge it.
                    (layer.Behind || !owned ? _back : _front).Add(view);
                    _layers.Add(new Layer { View = view, Place = layer, Aspect = Aspect(sprite), Delay = fresh ? stagger * 0.35f : -1f });
                }

                if (fresh)
                {
                    stagger++;
                }
            }

            Layout();
            if (stagger > 0)
            {
                _ = DropInAsync();
            }
        }

        /// <summary>Fresh parts fall from above and settle with a small bounce.</summary>
        private async Awaitable DropInAsync()
        {
            float t = 0f;
            float end = 0f;
            foreach (var l in _layers)
            {
                end = Mathf.Max(end, l.Delay + DropSeconds);
            }

            while (t <= end + 0.05f && panel != null)
            {
                foreach (var l in _layers)
                {
                    if (l.Delay < 0f)
                    {
                        continue;
                    }

                    float k = Mathf.Clamp01((t - l.Delay) / DropSeconds);
                    float fall = (1f - EaseOutBack(k)) * -contentRect.height * 0.5f;
                    l.View.style.translate = new Translate(0f, fall);
                    l.View.style.opacity = k <= 0f ? 0f : Mathf.Min(1f, k * 3f);
                }

                await Awaitable.NextFrameAsync();
                t += Time.deltaTime;
            }

            foreach (var l in _layers)
            {
                l.View.style.translate = new Translate(0f, 0f);
                l.View.style.opacity = StyleKeyword.Null;
            }
        }

        private void Layout()
        {
            var r = contentRect;
            if (r.width <= 0f || r.height <= 0f)
            {
                return;
            }

            float bw = Mathf.Min(r.width / Margin, (r.height / Margin) * _bodyAspect);
            float bh = bw / _bodyAspect;
            float bx = (r.width - bw) * 0.5f;
            float by = (r.height - bh) * 0.45f;
            foreach (var target in new[] { _back, _front })
            {
                target.style.left = 0;
                target.style.top = 0;
                target.style.width = r.width;
                target.style.height = r.height;
            }

            _body.style.left = bx;
            _body.style.top = by;
            _body.style.width = bw;
            _body.style.height = bh;
            foreach (var l in _layers)
            {
                float w = l.Place.Width * bw;
                float h = w / l.Aspect;
                l.View.style.width = w;
                l.View.style.height = h;
                l.View.style.left = bx + (l.Place.X * bw) - (w * 0.5f);
                l.View.style.top = by + (l.Place.Y * bh) - (h * 0.5f);
            }
        }

        private static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + (c3 * Mathf.Pow(x - 1f, 3f)) + (c1 * Mathf.Pow(x - 1f, 2f));
        }

        private static float Aspect(Sprite s) => s != null && s.rect.height > 0f ? s.rect.width / s.rect.height : 1f;

        private sealed class Layer
        {
            public VisualElement View;
            public ShipLayer Place;
            public float Aspect;

            /// <summary>Seconds before this layer drops in; negative when it is already in place.</summary>
            public float Delay;
        }
    }
}
