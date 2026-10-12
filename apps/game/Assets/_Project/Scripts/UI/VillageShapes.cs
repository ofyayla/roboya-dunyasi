using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.UI
{
    /// <summary>
    /// The village's drawings in code (ADR 0031): a basket you can tell is a basket, a straw skep, honeycomb made of cells that share edges, a
    /// honey drop and the little marks that tell hives apart without colour. They are the picture until sprite art replaces them, and the honeycomb
    /// stays in code for good: the number of cells is the number the child counts, so it must be exact.
    /// </summary>
    public static class VillagePalette
    {
        public static readonly Color Outline = new Color32(0x3A, 0x2A, 0x1D, 0xFF);
        public static readonly Color Straw = new Color32(0xE9, 0xBC, 0x5E, 0xFF);
        public static readonly Color StrawDark = new Color32(0xCF, 0x95, 0x3E, 0xFF);
        public static readonly Color Wicker = new Color32(0xC9, 0x8D, 0x52, 0xFF);
        public static readonly Color WickerDark = new Color32(0x8A, 0x5A, 0x2B, 0xFF);
        public static readonly Color Inside = new Color32(0x5A, 0x3A, 0x1E, 0xFF);
        public static readonly Color Honey = new Color32(0xF5, 0xB2, 0x1A, 0xFF);
        public static readonly Color HoneyDark = new Color32(0x8A, 0x5A, 0x12, 0xFF);
        public static readonly Color Cream = new Color32(0xFF, 0xF4, 0xE2, 0xFF);
        public static readonly Color Wax = new Color32(0x9A, 0x66, 0x1C, 0xFF);
        public static readonly Color Cloth = new Color32(0xD9, 0x53, 0x4A, 0xFF);

        /// <summary>The colour of a hive's mark; always paired with its own shape.</summary>
        public static Color MarkColor(int index)
        {
            switch (index)
            {
                case 0: return new Color32(0xD9, 0x53, 0x4A, 0xFF);
                case 1: return new Color32(0x3A, 0x78, 0xC9, 0xFF);
                case 2: return new Color32(0x3F, 0x9A, 0x52, 0xFF);
                default: return new Color32(0x8E, 0x5B, 0xD0, 0xFF);
            }
        }
    }

    /// <summary>Shared Painter2D helpers of the village shapes.</summary>
    public static class ShapePaint
    {
        public static void Ellipse(Painter2D p, Vector2 c, float rx, float ry)
        {
            const float k = 0.5522848f;
            p.BeginPath();
            p.MoveTo(new Vector2(c.x + rx, c.y));
            p.BezierCurveTo(new Vector2(c.x + rx, c.y + (ry * k)), new Vector2(c.x + (rx * k), c.y + ry), new Vector2(c.x, c.y + ry));
            p.BezierCurveTo(new Vector2(c.x - (rx * k), c.y + ry), new Vector2(c.x - rx, c.y + (ry * k)), new Vector2(c.x - rx, c.y));
            p.BezierCurveTo(new Vector2(c.x - rx, c.y - (ry * k)), new Vector2(c.x - (rx * k), c.y - ry), new Vector2(c.x, c.y - ry));
            p.BezierCurveTo(new Vector2(c.x + (rx * k), c.y - ry), new Vector2(c.x + rx, c.y - (ry * k)), new Vector2(c.x + rx, c.y));
            p.ClosePath();
        }

        public static void RoundedRect(Painter2D p, Vector2 min, Vector2 max, float radius)
        {
            radius = Mathf.Min(radius, (max.x - min.x) * 0.5f, (max.y - min.y) * 0.5f);
            p.BeginPath();
            p.MoveTo(new Vector2(min.x + radius, min.y));
            p.LineTo(new Vector2(max.x - radius, min.y));
            p.ArcTo(new Vector2(max.x, min.y), new Vector2(max.x, min.y + radius), radius);
            p.LineTo(new Vector2(max.x, max.y - radius));
            p.ArcTo(new Vector2(max.x, max.y), new Vector2(max.x - radius, max.y), radius);
            p.LineTo(new Vector2(min.x + radius, max.y));
            p.ArcTo(new Vector2(min.x, max.y), new Vector2(min.x, max.y - radius), radius);
            p.LineTo(new Vector2(min.x, min.y + radius));
            p.ArcTo(new Vector2(min.x, min.y), new Vector2(min.x + radius, min.y), radius);
            p.ClosePath();
        }

        /// <summary>A pointy-top hexagon around <paramref name="c"/>.</summary>
        public static void Hexagon(Painter2D p, Vector2 c, float r)
        {
            p.BeginPath();
            for (int k = 0; k < 6; k++)
            {
                float a = Mathf.Deg2Rad * ((60f * k) - 90f);
                var v = new Vector2(c.x + (r * Mathf.Cos(a)), c.y + (r * Mathf.Sin(a)));
                if (k == 0)
                {
                    p.MoveTo(v);
                }
                else
                {
                    p.LineTo(v);
                }
            }

            p.ClosePath();
        }

        /// <summary>The square the shape is drawn in: the content rect's largest centred square.</summary>
        public static bool Frame(VisualElement element, out Vector2 origin, out float size)
        {
            var r = element.contentRect;
            size = Mathf.Min(r.width, r.height);
            origin = new Vector2(r.x + ((r.width - size) * 0.5f), r.y + ((r.height - size) * 0.5f));
            return size > 1f;
        }
    }

    /// <summary>
    /// Ari's basket: a round wicker basket with a tall arched handle, woven bands, a dark open mouth and a checked cloth. The honey sits in the mouth
    /// (<see cref="MouthCenter"/>), drawn by the view, so the basket itself never says a number.
    /// </summary>
    public sealed class BasketShape : VisualElement
    {
        public BasketShape()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        /// <summary>Where the basket's mouth is, as a fraction of the drawn square: honey dots are laid out around it.</summary>
        public static readonly Vector2 MouthCenter = new Vector2(0.5f, 0.46f);

        /// <summary>Half the width of the mouth as a fraction of the drawn square.</summary>
        public const float MouthHalfWidth = 0.3f;

        private void Draw(MeshGenerationContext ctx)
        {
            if (!ShapePaint.Frame(this, out var o, out float s))
            {
                return;
            }

            var p = ctx.painter2D;
            Vector2 P(float x, float y) => o + new Vector2(x * s, y * s);
            p.lineJoin = LineJoin.Round;
            p.lineCap = LineCap.Round;

            // Handle: a tall arch behind the mouth, drawn twice (dark rope, light strand) so it reads as woven cane.
            p.strokeColor = VillagePalette.Outline;
            p.lineWidth = s * 0.125f;
            p.BeginPath();
            p.Arc(P(0.5f, 0.46f), s * 0.34f, Angle.Degrees(180f), Angle.Degrees(360f));
            p.Stroke();
            p.strokeColor = VillagePalette.Wicker;
            p.lineWidth = s * 0.06f;
            p.BeginPath();
            p.Arc(P(0.5f, 0.46f), s * 0.34f, Angle.Degrees(180f), Angle.Degrees(360f));
            p.Stroke();

            // Body: wider at the mouth, narrower at the foot.
            p.BeginPath();
            p.MoveTo(P(0.14f, 0.46f));
            p.LineTo(P(0.86f, 0.46f));
            p.BezierCurveTo(P(0.88f, 0.66f), P(0.8f, 0.86f), P(0.72f, 0.92f));
            p.LineTo(P(0.28f, 0.92f));
            p.BezierCurveTo(P(0.2f, 0.86f), P(0.12f, 0.66f), P(0.14f, 0.46f));
            p.ClosePath();
            p.fillColor = VillagePalette.Wicker;
            p.Fill();
            p.strokeColor = VillagePalette.Outline;
            p.lineWidth = s * 0.045f;
            p.Stroke();

            // Weave: three bands and short ticks between them.
            p.strokeColor = VillagePalette.WickerDark;
            p.lineWidth = s * 0.03f;
            float[] bands = { 0.58f, 0.7f, 0.82f };
            for (int i = 0; i < bands.Length; i++)
            {
                float inset = 0.02f + ((bands[i] - 0.46f) * 0.2f);
                p.BeginPath();
                p.MoveTo(P(0.14f + inset, bands[i]));
                p.QuadraticCurveTo(P(0.5f, bands[i] + 0.035f), P(0.86f - inset, bands[i]));
                p.Stroke();
                for (int k = 0; k < 5; k++)
                {
                    float x = 0.26f + (k * 0.12f) + ((i % 2) * 0.06f);
                    p.BeginPath();
                    p.MoveTo(P(x, bands[i] + 0.01f));
                    p.LineTo(P(x, bands[i] + 0.1f));
                    p.Stroke();
                }
            }

            // Mouth: a dark open ellipse with a rim, so it is plainly something you put honey into.
            ShapePaint.Ellipse(p, P(0.5f, 0.46f), s * 0.38f, s * 0.11f);
            p.fillColor = VillagePalette.Inside;
            p.Fill();
            p.strokeColor = VillagePalette.Outline;
            p.lineWidth = s * 0.05f;
            p.Stroke();
            ShapePaint.Ellipse(p, P(0.5f, 0.45f), s * 0.405f, s * 0.125f);
            p.strokeColor = VillagePalette.WickerDark;
            p.lineWidth = s * 0.03f;
            p.Stroke();

            // Cloth tucked over the right edge: red with a white check.
            p.BeginPath();
            p.MoveTo(P(0.66f, 0.43f));
            p.LineTo(P(0.9f, 0.5f));
            p.LineTo(P(0.8f, 0.7f));
            p.LineTo(P(0.68f, 0.52f));
            p.ClosePath();
            p.fillColor = VillagePalette.Cloth;
            p.Fill();
            p.strokeColor = VillagePalette.Outline;
            p.lineWidth = s * 0.03f;
            p.Stroke();
            p.fillColor = VillagePalette.Cream;
            foreach (var spot in new[] { new Vector2(0.74f, 0.52f), new Vector2(0.82f, 0.56f), new Vector2(0.77f, 0.62f) })
            {
                p.BeginPath();
                p.MoveTo(P(spot.x - 0.025f, spot.y - 0.025f));
                p.LineTo(P(spot.x + 0.025f, spot.y - 0.025f));
                p.LineTo(P(spot.x + 0.025f, spot.y + 0.025f));
                p.LineTo(P(spot.x - 0.025f, spot.y + 0.025f));
                p.ClosePath();
                p.Fill();
            }
        }
    }

    /// <summary>A straw skep: the beehive everyone knows, stacked rings narrowing to a dome, a dark round entrance and a wooden step. No cells in it.</summary>
    public sealed class SkepShape : VisualElement
    {
        public SkepShape()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        private void Draw(MeshGenerationContext ctx)
        {
            if (!ShapePaint.Frame(this, out var o, out float s))
            {
                return;
            }

            var p = ctx.painter2D;
            Vector2 P(float x, float y) => o + new Vector2(x * s, y * s);
            p.lineJoin = LineJoin.Round;
            p.lineWidth = s * 0.04f;
            p.strokeColor = VillagePalette.Outline;

            // Wooden step.
            ShapePaint.RoundedRect(p, P(0.1f, 0.88f), P(0.9f, 0.99f), s * 0.04f);
            p.fillColor = VillagePalette.WickerDark;
            p.Fill();
            p.Stroke();

            // Rings from the foot up, each narrower: together a dome.
            float[] widths = { 0.8f, 0.7f, 0.58f, 0.44f, 0.26f };
            float ringHeight = 0.17f;
            for (int i = 0; i < widths.Length; i++)
            {
                float bottom = 0.9f - (i * ringHeight * 0.92f);
                float top = bottom - ringHeight;
                float half = widths[i] * 0.5f;
                ShapePaint.RoundedRect(p, P(0.5f - half, top), P(0.5f + half, bottom), s * 0.07f);
                p.fillColor = i % 2 == 0 ? VillagePalette.Straw : VillagePalette.StrawDark;
                p.Fill();
                p.Stroke();
            }

            // A little tuft on top, then the entrance: a dark round door with a light rim.
            p.BeginPath();
            p.MoveTo(P(0.5f, 0.13f));
            p.LineTo(P(0.5f, 0.05f));
            p.Stroke();
            ShapePaint.Ellipse(p, P(0.5f, 0.72f), s * 0.12f, s * 0.13f);
            p.fillColor = VillagePalette.Inside;
            p.Fill();
            p.Stroke();
            ShapePaint.Ellipse(p, P(0.5f, 0.74f), s * 0.07f, s * 0.08f);
            p.fillColor = new Color32(0x2A, 0x1A, 0x0E, 0xFF);
            p.Fill();
        }
    }

    /// <summary>
    /// Honeycomb: <see cref="Total"/> hexagonal cells that share their walls, <see cref="Filled"/> of them holding honey. Cells are empty cream or
    /// amber with a shine, so empty and full differ by shape and brightness, never colour alone. A level that does not say how many a hive wants
    /// (sharing) shows only the cells that are full, or one faint cell while it is empty.
    /// </summary>
    public sealed class CombCells : VisualElement
    {
        // Cell positions in cell widths (x) and rows (y), pointy-top hexagons that interlock. Index = number of cells.
        private static readonly Vector2[][] Layouts =
        {
            new Vector2[0],
            new[] { new Vector2(0f, 0f) },
            new[] { new Vector2(-0.5f, 0f), new Vector2(0.5f, 0f) },
            new[] { new Vector2(-0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 1f) },
            new[] { new Vector2(-0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) },
            new[] { new Vector2(-1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(-0.5f, 1f), new Vector2(0.5f, 1f) },
            new[] { new Vector2(-1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(-0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(1.5f, 1f) },
            new[] { new Vector2(-0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-1f, 1f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(-0.5f, 2f), new Vector2(0.5f, 2f) },
        };

        public const int MaxCells = 7;

        private int _total;
        private int _filled;
        private bool _ghost;

        public CombCells()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        public int Total => _total;

        public int Filled => _filled;

        /// <summary>Sets how many cells there are and how many are full; <paramref name="ghost"/> draws a single faint cell when there are none.</summary>
        public void Set(int total, int filled, bool ghost = false)
        {
            total = Mathf.Clamp(total, 0, MaxCells);
            filled = Mathf.Clamp(filled, 0, total);
            if (total == _total && filled == _filled && ghost == _ghost)
            {
                return;
            }

            _total = total;
            _filled = filled;
            _ghost = ghost;
            MarkDirtyRepaint();
        }

        /// <summary>The centre of cell <paramref name="index"/> in the element's own space (for the honey that flies into it); falls back to the middle.</summary>
        public Vector2 CellCenter(int index)
        {
            var cells = Cells(_total > 0 ? _total : (_ghost ? 1 : 0), out float r, out var origin);
            if (cells == null || index < 0 || index >= cells.Length)
            {
                return layout.size * 0.5f;
            }

            return origin + new Vector2(cells[index].x * Mathf.Sqrt(3f) * r, cells[index].y * 1.5f * r);
        }

        private Vector2[] Cells(int count, out float r, out Vector2 origin)
        {
            r = 0f;
            origin = Vector2.zero;
            if (count < 1)
            {
                return null;
            }

            var cells = Layouts[count];
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float maxRow = 0f;
            foreach (var c in cells)
            {
                minX = Mathf.Min(minX, c.x);
                maxX = Mathf.Max(maxX, c.x);
                maxRow = Mathf.Max(maxRow, c.y);
            }

            var rect = contentRect;
            float widthInCells = (maxX - minX) + 1f;
            float heightInR = (maxRow * 1.5f) + 2f;
            r = Mathf.Min(rect.width / (widthInCells * Mathf.Sqrt(3f)), rect.height / heightInR);
            float usedW = widthInCells * Mathf.Sqrt(3f) * r;
            float usedH = heightInR * r;
            origin = new Vector2(
                rect.x + ((rect.width - usedW) * 0.5f) + (((-minX) + 0.5f) * Mathf.Sqrt(3f) * r),
                rect.y + ((rect.height - usedH) * 0.5f) + r);
            return cells;
        }

        private void Draw(MeshGenerationContext ctx)
        {
            int count = _total > 0 ? _total : (_ghost ? 1 : 0);
            var cells = Cells(count, out float r, out var origin);
            if (cells == null || r < 1f)
            {
                return;
            }

            var p = ctx.painter2D;
            p.lineJoin = LineJoin.Round;
            float w = Mathf.Sqrt(3f) * r;
            Vector2 At(int i) => origin + new Vector2(cells[i].x * w, cells[i].y * 1.5f * r);

            // The comb's wax: every cell a little larger underneath, so the walls between cells are one shared wax line.
            if (_total > 0)
            {
                p.fillColor = VillagePalette.Wax;
                p.strokeColor = VillagePalette.Outline;
                p.lineWidth = r * 0.12f;
                for (int i = 0; i < cells.Length; i++)
                {
                    ShapePaint.Hexagon(p, At(i), r * 1.02f);
                    p.Fill();
                }
            }

            for (int i = 0; i < cells.Length; i++)
            {
                bool full = i < _filled;
                ShapePaint.Hexagon(p, At(i), r * 0.86f);
                p.fillColor = _total == 0 ? new Color(VillagePalette.Cream.r, VillagePalette.Cream.g, VillagePalette.Cream.b, 0.35f) : full ? VillagePalette.Honey : VillagePalette.Cream;
                p.Fill();
                p.strokeColor = _total == 0 ? new Color(VillagePalette.Wax.r, VillagePalette.Wax.g, VillagePalette.Wax.b, 0.6f) : VillagePalette.Outline;
                p.lineWidth = r * 0.1f;
                p.Stroke();
                if (full)
                {
                    // A shine in the top-left of a full cell.
                    p.BeginPath();
                    p.Arc(At(i) + new Vector2(-r * 0.3f, -r * 0.32f), r * 0.14f, Angle.Degrees(0f), Angle.Degrees(360f));
                    p.fillColor = new Color(1f, 1f, 1f, 0.75f);
                    p.Fill();
                }
            }
        }
    }

    /// <summary>One drop of honey: a teardrop with a shine. Used where no sprite is set.</summary>
    public sealed class HoneyDropShape : VisualElement
    {
        public HoneyDropShape()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        private void Draw(MeshGenerationContext ctx)
        {
            if (!ShapePaint.Frame(this, out var o, out float s))
            {
                return;
            }

            var p = ctx.painter2D;
            Vector2 P(float x, float y) => o + new Vector2(x * s, y * s);
            p.lineJoin = LineJoin.Round;
            p.BeginPath();
            p.MoveTo(P(0.5f, 0.04f));
            p.BezierCurveTo(P(0.62f, 0.28f), P(0.86f, 0.44f), P(0.86f, 0.66f));
            p.BezierCurveTo(P(0.86f, 0.86f), P(0.68f, 0.97f), P(0.5f, 0.97f));
            p.BezierCurveTo(P(0.32f, 0.97f), P(0.14f, 0.86f), P(0.14f, 0.66f));
            p.BezierCurveTo(P(0.14f, 0.44f), P(0.38f, 0.28f), P(0.5f, 0.04f));
            p.ClosePath();
            p.fillColor = VillagePalette.Honey;
            p.Fill();
            p.strokeColor = VillagePalette.HoneyDark;
            p.lineWidth = s * 0.07f;
            p.Stroke();
            p.BeginPath();
            p.Arc(P(0.36f, 0.62f), s * 0.07f, Angle.Degrees(0f), Angle.Degrees(360f));
            p.fillColor = new Color(1f, 1f, 1f, 0.8f);
            p.Fill();
        }
    }

    /// <summary>A hive's mark: its own shape in its own colour (circle, triangle, square, diamond), so hives are told apart without colour.</summary>
    public sealed class HiveMark : VisualElement
    {
        private readonly int _index;

        public HiveMark(int index)
        {
            _index = index;
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        private void Draw(MeshGenerationContext ctx)
        {
            if (!ShapePaint.Frame(this, out var o, out float s))
            {
                return;
            }

            var p = ctx.painter2D;
            Vector2 P(float x, float y) => o + new Vector2(x * s, y * s);
            p.lineJoin = LineJoin.Round;
            p.fillColor = VillagePalette.MarkColor(_index);
            p.strokeColor = VillagePalette.Cream;
            p.lineWidth = s * 0.1f;
            switch (_index)
            {
                case 0:
                    p.BeginPath();
                    p.Arc(P(0.5f, 0.5f), s * 0.38f, Angle.Degrees(0f), Angle.Degrees(360f));
                    break;
                case 1:
                    p.BeginPath();
                    p.MoveTo(P(0.5f, 0.1f));
                    p.LineTo(P(0.92f, 0.88f));
                    p.LineTo(P(0.08f, 0.88f));
                    p.ClosePath();
                    break;
                case 2:
                    p.BeginPath();
                    p.MoveTo(P(0.14f, 0.14f));
                    p.LineTo(P(0.86f, 0.14f));
                    p.LineTo(P(0.86f, 0.86f));
                    p.LineTo(P(0.14f, 0.86f));
                    p.ClosePath();
                    break;
                default:
                    p.BeginPath();
                    p.MoveTo(P(0.5f, 0.06f));
                    p.LineTo(P(0.94f, 0.5f));
                    p.LineTo(P(0.5f, 0.94f));
                    p.LineTo(P(0.06f, 0.5f));
                    p.ClosePath();
                    break;
            }

            p.Fill();
            p.Stroke();
        }
    }
}
