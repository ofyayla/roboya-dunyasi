using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.UI
{
    /// <summary>
    /// A fallen log drawn in code with the region palette (docs/art/style-guide.md). The only log art we have is a
    /// grass tile, which looks wrong standing in a wide scene.
    /// </summary>
    public sealed class LogShape : VisualElement
    {
        /// <summary>Width / height of the drawing.</summary>
        public const float Aspect = 2.2f;

        private static readonly Color Outline = new Color(0.227f, 0.165f, 0.114f);
        private static readonly Color Bark = new Color(0.627f, 0.384f, 0.227f);
        private static readonly Color BarkDark = new Color(0.541f, 0.353f, 0.231f);
        private static readonly Color Wood = new Color(0.906f, 0.737f, 0.502f);
        private static readonly Color Ring = new Color(0.78f, 0.58f, 0.36f);

        public LogShape()
        {
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.left = 0;
            style.top = 0;
            style.right = 0;
            style.bottom = 0;
            generateVisualContent += Draw;
        }

        private void Draw(MeshGenerationContext mgc)
        {
            var r = contentRect;
            if (r.width <= 0f || r.height <= 0f)
            {
                return;
            }

            var p = mgc.painter2D;
            float stroke = Mathf.Max(2f, r.height * 0.06f);
            float h = r.height - stroke;
            float top = stroke * 0.5f;
            float endRx = h * 0.32f;
            float left = stroke * 0.5f + endRx;
            float right = r.width - (stroke * 0.5f) - endRx;
            float cy = top + (h * 0.5f);

            // Body: a rounded bar from the hidden left end to the visible cut end on the right.
            p.lineJoin = LineJoin.Round;
            p.fillColor = Bark;
            p.BeginPath();
            p.MoveTo(new Vector2(left, top));
            p.LineTo(new Vector2(right, top));
            p.LineTo(new Vector2(right, top + h));
            p.LineTo(new Vector2(left, top + h));
            p.Arc(new Vector2(left, cy), h * 0.5f, 90f, 270f);
            p.ClosePath();
            p.Fill();

            // Shade on the lower third and bark lines.
            p.fillColor = BarkDark;
            p.BeginPath();
            p.MoveTo(new Vector2(left, top + (h * 0.68f)));
            p.LineTo(new Vector2(right, top + (h * 0.68f)));
            p.LineTo(new Vector2(right, top + h));
            p.LineTo(new Vector2(left, top + h));
            p.ClosePath();
            p.Fill();

            p.strokeColor = Outline;
            p.lineWidth = stroke * 0.6f;
            p.lineCap = LineCap.Round;
            p.BeginPath();
            p.MoveTo(new Vector2(left + (h * 0.2f), top + (h * 0.3f)));
            p.LineTo(new Vector2(left + (h * 0.9f), top + (h * 0.3f)));
            p.MoveTo(new Vector2(left + (h * 1.1f), top + (h * 0.55f)));
            p.LineTo(new Vector2(right - (h * 0.4f), top + (h * 0.55f)));
            p.Stroke();

            // Outline of the body.
            p.lineWidth = stroke;
            p.BeginPath();
            p.MoveTo(new Vector2(right, top));
            p.LineTo(new Vector2(left, top));
            p.Arc(new Vector2(left, cy), h * 0.5f, 270f, 90f, ArcDirection.CounterClockwise);
            p.LineTo(new Vector2(right, top + h));
            p.Stroke();

            // Cut end with growth rings.
            Ellipse(p, new Vector2(right, cy), endRx, h * 0.5f, Wood, true);
            Ellipse(p, new Vector2(right, cy), endRx * 0.62f, h * 0.31f, Ring, false, stroke * 0.5f);
            Ellipse(p, new Vector2(right, cy), endRx * 0.28f, h * 0.14f, Ring, false, stroke * 0.5f);
            Ellipse(p, new Vector2(right, cy), endRx, h * 0.5f, Outline, false, stroke);
        }

        private static void Ellipse(Painter2D p, Vector2 c, float rx, float ry, Color color, bool fill, float width = 0f)
        {
            const int steps = 28;
            p.BeginPath();
            for (int i = 0; i <= steps; i++)
            {
                float a = i * Mathf.PI * 2f / steps;
                var pt = c + new Vector2(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry);
                if (i == 0)
                {
                    p.MoveTo(pt);
                }
                else
                {
                    p.LineTo(pt);
                }
            }

            p.ClosePath();
            if (fill)
            {
                p.fillColor = color;
                p.Fill();
            }
            else
            {
                p.strokeColor = color;
                p.lineWidth = width;
                p.Stroke();
            }
        }
    }
}
