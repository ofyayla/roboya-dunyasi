using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Games.Common
{
    /// <summary>
    /// A flat arrow lying on the board in front of the robot, so a child can see which way it faces (and so which way
    /// "turn right" goes). The polygon is projected with the board's perspective, so it lies on the ground like the grid.
    /// </summary>
    public sealed class FacingArrow : VisualElement
    {
        public const int PointCount = 7;

        private static readonly Color Fill = new Color(1f, 0.82f, 0.28f, 0.95f);
        private static readonly Color Edge = new Color(0.227f, 0.165f, 0.114f, 1f);

        private readonly Vector2[] _points = new Vector2[PointCount];
        private bool _visible;

        public FacingArrow()
        {
            name = "facing-arrow";
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.left = 0;
            style.top = 0;
            style.right = 0;
            style.bottom = 0;
            generateVisualContent += Draw;
        }

        public bool IsVisible => _visible;

        public Vector2[] Points => _points;

        public void Set(bool visible)
        {
            _visible = visible;
            MarkDirtyRepaint();
        }

        private void Draw(MeshGenerationContext mgc)
        {
            if (!_visible)
            {
                return;
            }

            var p = mgc.painter2D;
            p.lineJoin = LineJoin.Round;
            p.BeginPath();
            p.MoveTo(_points[0]);
            for (int i = 1; i < PointCount; i++)
            {
                p.LineTo(_points[i]);
            }

            p.ClosePath();
            p.fillColor = Fill;
            p.Fill();
            p.strokeColor = Edge;
            p.lineWidth = 3f;
            p.Stroke();
        }

        /// <summary>
        /// Arrow outline in grid cells around <paramref name="centre"/>, pointing along (<paramref name="dx"/>, <paramref name="dy"/>).
        /// </summary>
        public static void Outline(Vector2 centre, float dx, float dy, Vector2[] into)
        {
            float sx = -dy;
            float sy = dx;
            Vector2 At(float along, float side) => new Vector2(centre.x + (dx * along) + (sx * side), centre.y + (dy * along) + (sy * side));
            into[0] = At(0.30f, 0f);
            into[1] = At(-0.04f, 0.24f);
            into[2] = At(-0.04f, 0.09f);
            into[3] = At(-0.28f, 0.09f);
            into[4] = At(-0.28f, -0.09f);
            into[5] = At(-0.04f, -0.09f);
            into[6] = At(-0.04f, -0.24f);
        }
    }
}
