using System.Collections.Generic;
using Roboya.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.UI
{
    /// <summary>
    /// Roboya in one pose, wearing the parts equipped in the garage (ILR-03). A colour variant replaces the body
    /// sprite; other parts are layered from the pose's anchors (head box, antenna bulb), so one catalog entry fits
    /// every pose. Wings go behind the body. Parts may overflow the element; the body is fitted to it.
    /// </summary>
    public sealed class RobotAvatar : VisualElement
    {
        private readonly RobotWardrobe _wardrobe;
        private readonly VisualElement _back = new VisualElement();
        private readonly VisualElement _body = new VisualElement();
        private readonly VisualElement _over = new VisualElement();
        private readonly List<(VisualElement View, PartPlacement Place, float Aspect)> _layers = new List<(VisualElement, PartPlacement, float)>();
        private string _pose;
        private Sprite _plain;
        private Sprite _shown;
        private float _bodyAspect = 1f;

        public RobotAvatar(RobotWardrobe wardrobe, Sprite sprite, string pose = "front")
        {
            _wardrobe = wardrobe;
            pickingMode = PickingMode.Ignore;
            AddToClassList("robot-avatar");
            foreach (var layer in new[] { _back, _body, _over })
            {
                layer.pickingMode = PickingMode.Ignore;
                layer.style.position = Position.Absolute;
                layer.style.left = 0;
                layer.style.right = 0;
                Add(layer);
            }

            _back.style.top = 0;
            _back.style.bottom = 0;
            _over.style.top = 0;
            _over.style.bottom = 0;
            _body.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            _pose = pose;
            _plain = sprite;
            RegisterCallback<GeometryChangedEvent>(_ => Layout());
            Refresh();
        }

        /// <summary>The sprite currently drawn for the body (colour variant applied).</summary>
        public Sprite BodySprite => _shown;

        /// <summary>Switches pose (and the plain sprite) and redraws the worn parts for it.</summary>
        public void SetPose(string pose, Sprite plain)
        {
            if (pose == _pose && plain == _plain)
            {
                return;
            }

            _pose = pose;
            _plain = plain;
            Refresh();
        }

        /// <summary>Redraws with the parts equipped right now (call after the garage changed them).</summary>
        public void Refresh()
        {
            _back.Clear();
            _over.Clear();
            _layers.Clear();
            var body = _plain;
            var anchors = _wardrobe?.Anchors.For(_pose);
            if (_wardrobe != null)
            {
                foreach (var pair in _wardrobe.Equipped)
                {
                    var place = _wardrobe.Catalog.PlacementOf(pair.Value);
                    var sprite = place != null && _wardrobe.Art != null ? _wardrobe.Art.Find(place.SpriteFor(_pose)) : null;
                    if (sprite == null)
                    {
                        continue;
                    }

                    if (place.IsColourVariant)
                    {
                        body = sprite;
                    }
                    else if (anchors != null && (place.Anchor != PartAnchor.Bulb || anchors.HasBulb))
                    {
                        var view = new VisualElement { pickingMode = PickingMode.Ignore, name = "part-" + pair.Value };
                        view.style.position = Position.Absolute;
                        view.style.backgroundImage = new StyleBackground(sprite);
                        view.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                        (place.Behind ? _back : _over).Add(view);
                        _layers.Add((view, place, Aspect(sprite)));
                    }
                }
            }

            _shown = body;
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

            // The body is fitted into the element and sits on its bottom edge; parts are placed on that rect.
            float bw = Mathf.Min(r.width, r.height * _bodyAspect);
            float bh = bw / _bodyAspect;
            float bx = (r.width - bw) * 0.5f;
            float by = r.height - bh;
            var anchors = _wardrobe?.Anchors.For(_pose);
            foreach (var (view, place, aspect) in _layers)
            {
                if (anchors == null)
                {
                    continue;
                }

                float w;
                float cx;
                float cy;
                switch (place.Anchor)
                {
                    case PartAnchor.Bulb:
                        w = anchors.BulbWidth * bw * place.Scale;
                        cx = anchors.BulbX * bw;
                        cy = anchors.BulbY * bh;
                        break;
                    case PartAnchor.HeadTop:
                        w = anchors.HeadWidth * bw * place.Scale;
                        cx = anchors.HeadCenterX * bw;
                        // The part's bottom edge rests on the head top, sunk by Dy head heights.
                        cy = (anchors.HeadTop * bh) + (place.Dy * anchors.HeadHeight * bh) - (w / aspect * 0.5f);
                        break;
                    default:
                        w = anchors.HeadWidth * bw * place.Scale;
                        cx = anchors.HeadCenterX * bw;
                        cy = (anchors.HeadCenterY * bh) + (place.Dy * anchors.HeadHeight * bh);
                        break;
                }

                float h = w / aspect;
                view.style.width = w;
                view.style.height = h;
                view.style.left = bx + cx - (w * 0.5f);
                view.style.top = by + cy - (h * 0.5f);
            }

            _body.style.top = by;
            _body.style.height = bh;
        }

        private static float Aspect(Sprite s) => s != null && s.rect.height > 0f ? s.rect.width / s.rect.height : 1f;
    }
}
