using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>
    /// Code-drawn stand-in for the path map sprites (map_stone_*, docs/art/style-guide.md): a round stepping stone
    /// seen slightly from above, or the wide tree stump of a forest corner. Flat fills, one soft shadow, one small
    /// highlight and a thin warm outline. A view shows the sprite instead once the region art has one.
    /// </summary>
    public sealed class StoneShape : VisualElement
    {
        private static readonly Color Outline = new Color(0.227f, 0.165f, 0.114f, 0.55f);
        private static readonly Color Shadow = new Color(0.2f, 0.32f, 0.12f, 0.28f);
        private static readonly Color Highlight = new Color(1f, 1f, 1f, 0.7f);

        private StoneLook _look;

        public StoneShape(StoneLook look)
        {
            _look = look;
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        public StoneLook Look
        {
            get => _look;
            set
            {
                if (_look != value)
                {
                    _look = value;
                    MarkDirtyRepaint();
                }
            }
        }

        /// <summary>Top face centre as a fraction of the element height; numbers and Roboya stand there.</summary>
        public const float TopCentre = 0.37f;

        private void Draw(MeshGenerationContext ctx)
        {
            var r = contentRect;
            if (!(r.width > 0f) || !(r.height > 0f))
            {
                return;
            }

            var p = ctx.painter2D;
            float w = r.width;
            float h = r.height;
            float rx = w * 0.46f;
            float ry = h * 0.31f;
            float thick = h * 0.2f;
            var top = new Vector2(w * 0.5f, h * TopCentre);
            Colours(_look, out var face, out var side);

            Fill(p, new Vector2(top.x, top.y + thick + (ry * 0.35f)), rx * 1.02f, ry * 0.7f, Shadow);
            // The side band: the top oval pushed down, joined to the top by its widest points.
            p.fillColor = side;
            p.strokeColor = Outline;
            p.lineWidth = 3f;
            p.BeginPath();
            p.MoveTo(new Vector2(top.x - rx, top.y));
            p.LineTo(new Vector2(top.x - rx, top.y + thick));
            HalfEllipse(p, new Vector2(top.x, top.y + thick), rx, ry);
            p.LineTo(new Vector2(top.x + rx, top.y));
            p.ClosePath();
            p.Fill();
            p.Stroke();

            Fill(p, top, rx, ry, face);
            Stroke(p, top, rx, ry, Outline, 3f);
            if (_look == StoneLook.Corner)
            {
                // Growth rings on the stump.
                var ring = new Color(side.r, side.g, side.b, 0.45f);
                Stroke(p, top, rx * 0.66f, ry * 0.62f, ring, 2.5f);
                Stroke(p, top, rx * 0.33f, ry * 0.3f, ring, 2.5f);
            }
            else
            {
                Fill(p, new Vector2(top.x - (rx * 0.5f), top.y - (ry * 0.45f)), rx * 0.16f, ry * 0.16f, Highlight);
            }
        }

        private static void Colours(StoneLook look, out Color face, out Color side)
        {
            switch (look)
            {
                case StoneLook.Current:
                    face = new Color(1f, 0.79f, 0.29f);
                    side = new Color(0.95f, 0.55f, 0.16f);
                    break;
                case StoneLook.Ahead:
                    face = new Color(0.85f, 0.84f, 0.81f);
                    side = new Color(0.71f, 0.70f, 0.66f);
                    break;
                case StoneLook.Corner:
                    face = new Color(0.91f, 0.76f, 0.52f);
                    side = new Color(0.62f, 0.42f, 0.26f);
                    break;
                default:
                    face = new Color(1f, 0.96f, 0.89f);
                    side = new Color(0.91f, 0.79f, 0.56f);
                    break;
            }
        }

        // Four cubic arcs: Painter2D's Arc is circular only.
        private const float K = 0.5523f;

        private static void Ellipse(Painter2D p, Vector2 c, float rx, float ry)
        {
            p.BeginPath();
            p.MoveTo(new Vector2(c.x + rx, c.y));
            p.BezierCurveTo(new Vector2(c.x + rx, c.y + (ry * K)), new Vector2(c.x + (rx * K), c.y + ry), new Vector2(c.x, c.y + ry));
            p.BezierCurveTo(new Vector2(c.x - (rx * K), c.y + ry), new Vector2(c.x - rx, c.y + (ry * K)), new Vector2(c.x - rx, c.y));
            p.BezierCurveTo(new Vector2(c.x - rx, c.y - (ry * K)), new Vector2(c.x - (rx * K), c.y - ry), new Vector2(c.x, c.y - ry));
            p.BezierCurveTo(new Vector2(c.x + (rx * K), c.y - ry), new Vector2(c.x + rx, c.y - (ry * K)), new Vector2(c.x + rx, c.y));
            p.ClosePath();
        }

        /// <summary>Lower half, left to right; the pen must stand on the left end.</summary>
        private static void HalfEllipse(Painter2D p, Vector2 c, float rx, float ry)
        {
            p.BezierCurveTo(new Vector2(c.x - rx, c.y + (ry * K)), new Vector2(c.x - (rx * K), c.y + ry), new Vector2(c.x, c.y + ry));
            p.BezierCurveTo(new Vector2(c.x + (rx * K), c.y + ry), new Vector2(c.x + rx, c.y + (ry * K)), new Vector2(c.x + rx, c.y));
        }

        private static void Fill(Painter2D p, Vector2 c, float rx, float ry, Color color)
        {
            p.fillColor = color;
            Ellipse(p, c, rx, ry);
            p.Fill();
        }

        private static void Stroke(Painter2D p, Vector2 c, float rx, float ry, Color color, float width)
        {
            p.strokeColor = color;
            p.lineWidth = width;
            Ellipse(p, c, rx, ry);
            p.Stroke();
        }
    }

    /// <summary>How a stone on the path looks: walked, the next one, still ahead, or a forest corner stump.</summary>
    public enum StoneLook
    {
        Done,
        Current,
        Ahead,
        Corner,
    }
}
