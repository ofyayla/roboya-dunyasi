using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.UI
{
    /// <summary>
    /// Vector icon drawn with Painter2D: no sprite assets, crisp at any size, and every meaning is carried by
    /// shape, not colour alone (CLAUDE.md §7 accessibility). Placeholder art until F0-17 delivers illustrations.
    /// </summary>
    public sealed class Icon : VisualElement
    {
        private IconKind _kind;
        private Color _color = Color.white;
        private Color _accent = new Color(1f, 1f, 1f, 0.6f);

        public Icon(IconKind kind = IconKind.None)
        {
            _kind = kind;
            pickingMode = PickingMode.Ignore;
            AddToClassList("icon");
            generateVisualContent += Draw;
        }

        public IconKind Kind
        {
            get => _kind;
            set
            {
                _kind = value;
                MarkDirtyRepaint();
            }
        }

        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                MarkDirtyRepaint();
            }
        }

        public Color Accent
        {
            get => _accent;
            set
            {
                _accent = value;
                MarkDirtyRepaint();
            }
        }

        private void Draw(MeshGenerationContext ctx)
        {
            var r = contentRect;
            float s = Mathf.Min(r.width, r.height);
            if (s <= 0f || _kind == IconKind.None)
            {
                return;
            }

            var p = ctx.painter2D;
            var o = new Vector2(r.x + (r.width - s) * 0.5f, r.y + (r.height - s) * 0.5f);
            Vector2 P(float x, float y) => o + new Vector2(x * s, y * s);

            p.fillColor = _color;
            p.strokeColor = _color;
            p.lineWidth = s * 0.11f;
            p.lineCap = LineCap.Round;
            p.lineJoin = LineJoin.Round;

            switch (_kind)
            {
                case IconKind.Forward:
                    Arrow(p, P(0.5f, 0.85f), P(0.5f, 0.2f), s);
                    break;
                case IconKind.Backward:
                    Arrow(p, P(0.5f, 0.15f), P(0.5f, 0.8f), s);
                    break;
                case IconKind.TurnLeft:
                    CurvedArrow(p, o, s, left: true);
                    break;
                case IconKind.TurnRight:
                    CurvedArrow(p, o, s, left: false);
                    break;
                case IconKind.Play:
                    p.BeginPath();
                    p.MoveTo(P(0.3f, 0.18f));
                    p.LineTo(P(0.82f, 0.5f));
                    p.LineTo(P(0.3f, 0.82f));
                    p.ClosePath();
                    p.Fill();
                    break;
                case IconKind.Listen:
                    p.BeginPath();
                    p.MoveTo(P(0.12f, 0.38f));
                    p.LineTo(P(0.3f, 0.38f));
                    p.LineTo(P(0.5f, 0.18f));
                    p.LineTo(P(0.5f, 0.82f));
                    p.LineTo(P(0.3f, 0.62f));
                    p.LineTo(P(0.12f, 0.62f));
                    p.ClosePath();
                    p.Fill();
                    p.lineWidth = s * 0.07f;
                    ArcStroke(p, P(0.5f, 0.5f), s * 0.18f, -45f, 45f);
                    ArcStroke(p, P(0.5f, 0.5f), s * 0.32f, -50f, 50f);
                    break;
                case IconKind.Hint:
                    p.BeginPath();
                    p.Arc(P(0.5f, 0.4f), s * 0.25f, Angle.Degrees(0f), Angle.Degrees(360f));
                    p.Fill();
                    p.BeginPath();
                    p.MoveTo(P(0.38f, 0.6f));
                    p.LineTo(P(0.62f, 0.6f));
                    p.LineTo(P(0.6f, 0.78f));
                    p.LineTo(P(0.4f, 0.78f));
                    p.ClosePath();
                    p.Fill();
                    p.lineWidth = s * 0.06f;
                    Line(p, P(0.42f, 0.88f), P(0.58f, 0.88f));
                    break;
                case IconKind.Clear:
                    Line(p, P(0.25f, 0.25f), P(0.75f, 0.75f));
                    Line(p, P(0.75f, 0.25f), P(0.25f, 0.75f));
                    break;
                case IconKind.Star:
                case IconKind.StarEmpty:
                    StarPath(p, P(0.5f, 0.52f), s * 0.46f, s * 0.2f);
                    if (_kind == IconKind.Star)
                    {
                        p.Fill();
                    }
                    else
                    {
                        p.lineWidth = s * 0.06f;
                        p.Stroke();
                    }

                    break;
                case IconKind.Next:
                    Arrow(p, P(0.15f, 0.5f), P(0.82f, 0.5f), s);
                    break;
                case IconKind.Retry:
                    p.lineWidth = s * 0.1f;
                    ArcStroke(p, P(0.5f, 0.5f), s * 0.3f, -60f, 220f);
                    p.BeginPath();
                    var tip = P(0.5f, 0.5f) + Polar(-60f, s * 0.3f);
                    p.MoveTo(tip + new Vector2(-s * 0.16f, -s * 0.02f));
                    p.LineTo(tip + new Vector2(s * 0.06f, -s * 0.14f));
                    p.LineTo(tip + new Vector2(s * 0.08f, s * 0.12f));
                    p.ClosePath();
                    p.Fill();
                    break;
                case IconKind.Robot:
                    DrawRobot(p, P, s);
                    break;
                case IconKind.Turtle:
                    DrawTurtle(p, P, s);
                    break;
                case IconKind.Fruit:
                    p.BeginPath();
                    p.Arc(P(0.5f, 0.56f), s * 0.32f, Angle.Degrees(0f), Angle.Degrees(360f));
                    p.Fill();
                    p.fillColor = _accent;
                    p.BeginPath();
                    p.MoveTo(P(0.5f, 0.26f));
                    p.QuadraticCurveTo(P(0.72f, 0.06f), P(0.8f, 0.22f));
                    p.QuadraticCurveTo(P(0.66f, 0.32f), P(0.5f, 0.26f));
                    p.Fill();
                    break;
                case IconKind.Gear:
                    DrawGear(p, P(0.5f, 0.5f), s);
                    break;
                case IconKind.Home:
                    // House: roof triangle over a body with a door gap.
                    p.BeginPath();
                    p.MoveTo(P(0.12f, 0.5f));
                    p.LineTo(P(0.5f, 0.14f));
                    p.LineTo(P(0.88f, 0.5f));
                    p.ClosePath();
                    p.Fill();
                    RoundedRect(p, P(0.24f, 0.44f), P(0.76f, 0.86f), s * 0.05f);
                    p.Fill();
                    p.fillColor = _accent;
                    RoundedRect(p, P(0.42f, 0.6f), P(0.58f, 0.86f), s * 0.04f);
                    p.Fill();
                    break;
                case IconKind.Lock:
                    // Padlock: shackle arc above a rounded body with a keyhole (GLR-01 grown-up gate).
                    p.lineWidth = s * 0.1f;
                    ArcStroke(p, P(0.5f, 0.42f), s * 0.18f, 180f, 360f);
                    Line(p, P(0.32f, 0.42f), P(0.32f, 0.5f));
                    Line(p, P(0.68f, 0.42f), P(0.68f, 0.5f));
                    RoundedRect(p, P(0.2f, 0.48f), P(0.8f, 0.88f), s * 0.08f);
                    p.Fill();
                    p.fillColor = _accent;
                    p.BeginPath();
                    p.Arc(P(0.5f, 0.63f), s * 0.06f, Angle.Degrees(0f), Angle.Degrees(360f));
                    p.Fill();
                    RoundedRect(p, P(0.47f, 0.64f), P(0.53f, 0.78f), s * 0.02f);
                    p.Fill();
                    break;
                case IconKind.Ship:
                    // Round ship with a porthole, two fins and an antenna: the ship workshop (ILR-03).
                    p.BeginPath();
                    p.MoveTo(P(0.22f, 0.62f));
                    p.LineTo(P(0.08f, 0.82f));
                    p.LineTo(P(0.3f, 0.78f));
                    p.ClosePath();
                    p.Fill();
                    p.BeginPath();
                    p.MoveTo(P(0.78f, 0.62f));
                    p.LineTo(P(0.92f, 0.82f));
                    p.LineTo(P(0.7f, 0.78f));
                    p.ClosePath();
                    p.Fill();
                    p.BeginPath();
                    p.Arc(P(0.5f, 0.56f), s * 0.3f, Angle.Degrees(0f), Angle.Degrees(360f));
                    p.Fill();
                    p.lineWidth = s * 0.06f;
                    Line(p, P(0.56f, 0.27f), P(0.62f, 0.12f));
                    p.fillColor = _accent;
                    p.BeginPath();
                    p.Arc(P(0.5f, 0.54f), s * 0.13f, Angle.Degrees(0f), Angle.Degrees(360f));
                    p.Fill();
                    break;
                case IconKind.Island:
                    // Island: a mound with a palm-like tree over a wave line.
                    p.BeginPath();
                    p.Arc(P(0.5f, 0.78f), s * 0.34f, Angle.Degrees(180f), Angle.Degrees(360f));
                    p.ClosePath();
                    p.Fill();
                    p.lineWidth = s * 0.07f;
                    Line(p, P(0.5f, 0.48f), P(0.54f, 0.2f));
                    p.BeginPath();
                    p.Arc(P(0.54f, 0.22f), s * 0.14f, Angle.Degrees(180f), Angle.Degrees(360f));
                    p.ClosePath();
                    p.Fill();
                    p.strokeColor = _accent;
                    p.lineWidth = s * 0.06f;
                    Line(p, P(0.1f, 0.88f), P(0.9f, 0.88f));
                    break;
            }
        }

        private void DrawRobot(Painter2D p, System.Func<float, float, Vector2> P, float s)
        {
            // Body faces "up" (north); the view rotates the element for the robot's facing.
            RoundedRect(p, P(0.18f, 0.22f), P(0.82f, 0.86f), s * 0.14f);
            p.Fill();
            p.fillColor = _accent;
            p.BeginPath();
            p.Arc(P(0.37f, 0.46f), s * 0.08f, Angle.Degrees(0f), Angle.Degrees(360f));
            p.Fill();
            p.BeginPath();
            p.Arc(P(0.63f, 0.46f), s * 0.08f, Angle.Degrees(0f), Angle.Degrees(360f));
            p.Fill();
            // Antenna + direction nose so facing is readable without colour.
            p.fillColor = _color;
            p.BeginPath();
            p.MoveTo(P(0.4f, 0.22f));
            p.LineTo(P(0.5f, 0.04f));
            p.LineTo(P(0.6f, 0.22f));
            p.ClosePath();
            p.Fill();
            p.strokeColor = _accent;
            p.lineWidth = s * 0.05f;
            Line(p, P(0.38f, 0.68f), P(0.62f, 0.68f));
        }

        private void DrawTurtle(Painter2D p, System.Func<float, float, Vector2> P, float s)
        {
            p.fillColor = _accent;
            foreach (var leg in new[] { P(0.25f, 0.35f), P(0.75f, 0.35f), P(0.25f, 0.78f), P(0.75f, 0.78f) })
            {
                p.BeginPath();
                p.Arc(leg, s * 0.09f, Angle.Degrees(0f), Angle.Degrees(360f));
                p.Fill();
            }

            p.BeginPath();
            p.Arc(P(0.5f, 0.18f), s * 0.11f, Angle.Degrees(0f), Angle.Degrees(360f));
            p.Fill();
            p.fillColor = _color;
            p.BeginPath();
            p.Arc(P(0.5f, 0.57f), s * 0.3f, Angle.Degrees(0f), Angle.Degrees(360f));
            p.Fill();
            p.strokeColor = _accent;
            p.lineWidth = s * 0.04f;
            p.BeginPath();
            p.MoveTo(P(0.38f, 0.45f));
            p.LineTo(P(0.62f, 0.45f));
            p.LineTo(P(0.68f, 0.62f));
            p.LineTo(P(0.5f, 0.74f));
            p.LineTo(P(0.32f, 0.62f));
            p.ClosePath();
            p.Stroke();
        }

        private void DrawGear(Painter2D p, Vector2 c, float s)
        {
            const int teeth = 8;
            float outer = s * 0.44f;
            float inner = s * 0.33f;
            p.BeginPath();
            for (int i = 0; i < teeth * 2; i++)
            {
                float a0 = i * Mathf.PI / teeth;
                float a1 = (i + 1) * Mathf.PI / teeth;
                float rad = i % 2 == 0 ? outer : inner;
                var v0 = c + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * rad;
                var v1 = c + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * rad;
                if (i == 0)
                {
                    p.MoveTo(v0);
                }
                else
                {
                    p.LineTo(v0);
                }

                p.LineTo(v1);
            }

            p.ClosePath();
            p.Fill();
            p.fillColor = _accent;
            p.BeginPath();
            p.Arc(c, s * 0.13f, Angle.Degrees(0f), Angle.Degrees(360f));
            p.Fill();
        }

        private static void Arrow(Painter2D p, Vector2 from, Vector2 to, float s)
        {
            var dir = (to - from).normalized;
            var normal = new Vector2(-dir.y, dir.x);
            float head = s * 0.26f;
            Line(p, from, to - dir * head * 0.6f);
            p.BeginPath();
            p.MoveTo(to);
            p.LineTo(to - dir * head + normal * head * 0.75f);
            p.LineTo(to - dir * head - normal * head * 0.75f);
            p.ClosePath();
            p.Fill();
        }

        private static void CurvedArrow(Painter2D p, Vector2 o, float s, bool left)
        {
            // Up from the bottom, then bend 90° toward the turn side; the arrowhead points sideways.
            float side = left ? -1f : 1f;
            var start = o + new Vector2(s * (0.5f - side * 0.12f), s * 0.88f);
            var corner = o + new Vector2(s * (0.5f - side * 0.12f), s * 0.32f);
            var end = o + new Vector2(s * (0.5f + side * 0.22f), s * 0.32f);
            p.BeginPath();
            p.MoveTo(start);
            p.LineTo(corner + new Vector2(0f, s * 0.12f));
            p.QuadraticCurveTo(corner, corner + new Vector2(side * s * 0.12f, 0f));
            p.LineTo(end);
            p.Stroke();
            float head = s * 0.24f;
            var tip = end + new Vector2(side * head * 0.75f, 0f);
            p.BeginPath();
            p.MoveTo(tip);
            p.LineTo(end + new Vector2(-side * head * 0.1f, -head * 0.75f));
            p.LineTo(end + new Vector2(-side * head * 0.1f, head * 0.75f));
            p.ClosePath();
            p.Fill();
        }

        private static void StarPath(Painter2D p, Vector2 c, float outer, float inner)
        {
            p.BeginPath();
            for (int i = 0; i < 10; i++)
            {
                float a = -Mathf.PI / 2f + i * Mathf.PI / 5f;
                float rad = i % 2 == 0 ? outer : inner;
                var v = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * rad;
                if (i == 0)
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

        private static void RoundedRect(Painter2D p, Vector2 min, Vector2 max, float radius)
        {
            p.BeginPath();
            p.MoveTo(new Vector2(min.x + radius, min.y));
            p.ArcTo(new Vector2(max.x, min.y), new Vector2(max.x, max.y), radius);
            p.ArcTo(new Vector2(max.x, max.y), new Vector2(min.x, max.y), radius);
            p.ArcTo(new Vector2(min.x, max.y), new Vector2(min.x, min.y), radius);
            p.ArcTo(new Vector2(min.x, min.y), new Vector2(max.x, min.y), radius);
            p.ClosePath();
        }

        private static void Line(Painter2D p, Vector2 a, Vector2 b)
        {
            p.BeginPath();
            p.MoveTo(a);
            p.LineTo(b);
            p.Stroke();
        }

        private static void ArcStroke(Painter2D p, Vector2 c, float radius, float fromDeg, float toDeg)
        {
            p.BeginPath();
            p.Arc(c, radius, Angle.Degrees(fromDeg), Angle.Degrees(toDeg));
            p.Stroke();
        }

        private static Vector2 Polar(float deg, float radius) =>
            new Vector2(Mathf.Cos(deg * Mathf.Deg2Rad), Mathf.Sin(deg * Mathf.Deg2Rad)) * radius;
    }
}
