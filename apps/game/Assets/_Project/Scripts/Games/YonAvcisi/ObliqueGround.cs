using System.Collections.Generic;
using Roboya.CodingEngine.World;
using Roboya.UI;
using BoardGrid = Roboya.CodingEngine.World.Grid;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Games.YonAvcisi
{
    /// <summary>
    /// The board surface drawn in code with the region palette (docs/art/style-guide.md): a grass slab seen at an
    /// angle, a faint checker so children can count cells, and the YON-03 ghost route as a continuous dirt path.
    /// Drawing in code keeps the perspective exact, which tile sprites cannot do.
    /// </summary>
    public sealed class ObliqueGround : VisualElement
    {
        private static readonly Color Outline = Hex(0x3A2A1D);
        private static readonly Color Grass = Hex(0x8BC26B);
        private static readonly Color GrassLight = Hex(0x96CA77);
        private static readonly Color GrassDark = Hex(0x5E9443);
        private static readonly Color GrassLip = Hex(0x6FA552);
        private static readonly Color Dirt = Hex(0xE7C98F);
        private static readonly Color DirtEdge = Hex(0xD4AE6E);
        private static readonly Color SlabFace = Hex(0xB07D4F);
        private static readonly Color SlabShade = Hex(0x8A5A3B);
        private static readonly Color GridLine = new Color(0.23f, 0.16f, 0.11f, 0.13f);
        private static readonly Color DropShadow = new Color(0.23f, 0.16f, 0.11f, 0.18f);

        private ObliqueProjection _projection;
        private BoardGrid _grid;
        private HashSet<GridPosition> _path = new HashSet<GridPosition>();

        public ObliqueGround()
        {
            AddToClassList("board__ground");
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        public void Set(BoardGrid grid, HashSet<GridPosition> path)
        {
            _grid = grid;
            _path = path ?? new HashSet<GridPosition>();
            MarkDirtyRepaint();
        }

        public void SetProjection(ObliqueProjection projection)
        {
            _projection = projection;
            MarkDirtyRepaint();
        }

        private void Draw(MeshGenerationContext mgc)
        {
            if (_grid == null || !_projection.IsValid)
            {
                return;
            }

            var p = mgc.painter2D;
            var proj = _projection;
            int w = _grid.Width;
            int h = _grid.Height;
            float cell = proj.Cell;
            Vector2 tl = proj.Project(0, 0), tr = proj.Project(w, 0), br = proj.Project(w, h), bl = proj.Project(0, h);
            var down = new Vector2(0f, proj.SlabHeight);

            // Soft contact shadow under the slab.
            var shadowOffset = new Vector2(0f, proj.SlabHeight + (cell * 0.12f));
            Polygon(p, DropShadow, bl + shadowOffset + new Vector2(-cell * 0.15f, 0f), br + shadowOffset + new Vector2(cell * 0.15f, 0f), br + down, bl + down);

            // Front face of the slab: earth with a darker lower band and a grass lip on top.
            Polygon(p, SlabFace, bl, br, br + down, bl + down);
            Polygon(p, SlabShade, bl + (down * 0.62f), br + (down * 0.62f), br + down, bl + down);
            Polygon(p, GrassLip, bl, br, br + (down * 0.3f), bl + (down * 0.3f));

            // Surface and checker.
            Polygon(p, Grass, tl, tr, br, bl);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (((x + y) & 1) == 0)
                    {
                        Quad(p, GrassLight, x, y, 0f);
                    }
                }
            }

            // Ghost route: inset patches joined across shared edges so it reads as one trail.
            foreach (var c in _path)
            {
                Quad(p, DirtEdge, c.X, c.Y, 0.08f, cell * 0.12f);
            }

            foreach (var c in _path)
            {
                var east = new GridPosition(c.X + 1, c.Y);
                var south = new GridPosition(c.X, c.Y + 1);
                if (_path.Contains(east))
                {
                    Patch(p, Dirt, c.X + 0.5f, c.Y + 0.2f, c.X + 1.5f, c.Y + 0.8f);
                }

                if (_path.Contains(south))
                {
                    Patch(p, Dirt, c.X + 0.2f, c.Y + 0.5f, c.X + 0.8f, c.Y + 1.5f);
                }

                Quad(p, Dirt, c.X, c.Y, 0.14f, cell * 0.1f);
            }

            // Grid lines: straight under this projection, faint so they guide without boxing in.
            p.strokeColor = GridLine;
            p.lineWidth = Mathf.Max(1.5f, cell * 0.018f);
            p.lineCap = LineCap.Round;
            p.BeginPath();
            for (int x = 1; x < w; x++)
            {
                p.MoveTo(proj.Project(x, 0));
                p.LineTo(proj.Project(x, h));
            }

            for (int y = 1; y < h; y++)
            {
                p.MoveTo(proj.Project(0, y));
                p.LineTo(proj.Project(w, y));
            }

            p.Stroke();

            // Grass tufts on a stable subset of open cells, so a level always looks the same.
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var c = new GridPosition(x, y);
                    if (_path.Contains(c) || _grid[c] == CellType.Blocked || ((x * 5) + (y * 3)) % 3 != 0)
                    {
                        continue;
                    }

                    float ox = ((x * 37) + (y * 11)) % 2 == 0 ? 0.24f : 0.7f;
                    float oy = ((x * 13) + (y * 7)) % 2 == 0 ? 0.3f : 0.72f;
                    Tuft(p, proj.Project(x + ox, y + oy), cell * proj.ScaleAt(y + oy) * 0.16f);
                }
            }

            // One continuous outline around surface and front face.
            p.strokeColor = Outline;
            p.lineWidth = Mathf.Max(3f, cell * 0.035f);
            p.lineJoin = LineJoin.Round;
            p.BeginPath();
            p.MoveTo(tl);
            p.LineTo(tr);
            p.LineTo(br + down);
            p.LineTo(bl + down);
            p.ClosePath();
            p.Stroke();
            p.lineWidth = Mathf.Max(2f, cell * 0.022f);
            p.BeginPath();
            p.MoveTo(bl);
            p.LineTo(br);
            p.Stroke();
        }

        private void Quad(Painter2D p, Color color, int x, int y, float inset, float round = 0f)
        {
            Patch(p, color, x + inset, y + inset, x + 1 - inset, y + 1 - inset, round);
        }

        /// <summary>Fills a board-space rectangle; a same-colour round-joined stroke softens the corners.</summary>
        private void Patch(Painter2D p, Color color, float x0, float y0, float x1, float y1, float round = 0f)
        {
            var proj = _projection;
            p.fillColor = color;
            p.BeginPath();
            p.MoveTo(proj.Project(x0, y0));
            p.LineTo(proj.Project(x1, y0));
            p.LineTo(proj.Project(x1, y1));
            p.LineTo(proj.Project(x0, y1));
            p.ClosePath();
            p.Fill();
            if (round > 0f)
            {
                p.strokeColor = color;
                p.lineWidth = round;
                p.lineJoin = LineJoin.Round;
                p.Stroke();
            }
        }

        private static void Polygon(Painter2D p, Color color, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            p.fillColor = color;
            p.BeginPath();
            p.MoveTo(a);
            p.LineTo(b);
            p.LineTo(c);
            p.LineTo(d);
            p.ClosePath();
            p.Fill();
        }

        private static void Tuft(Painter2D p, Vector2 at, float size)
        {
            p.fillColor = GrassDark;
            Blade(p, at + new Vector2(-size * 0.45f, 0f), -0.35f, size * 0.75f, size * 0.22f);
            Blade(p, at, 0f, size, size * 0.26f);
            Blade(p, at + new Vector2(size * 0.45f, 0f), 0.35f, size * 0.75f, size * 0.22f);
        }

        // Straight segments only: a leaf outline from line segments avoids curve joins leaking across the board.
        private static void Blade(Painter2D p, Vector2 basePoint, float lean, float height, float halfWidth)
        {
            var tip = basePoint + new Vector2(lean * height, -height);
            var mid = basePoint + new Vector2(lean * height * 0.45f, -height * 0.55f);
            p.BeginPath();
            p.MoveTo(basePoint + new Vector2(-halfWidth, 0f));
            p.LineTo(mid + new Vector2(-halfWidth * 0.75f, 0f));
            p.LineTo(tip);
            p.LineTo(mid + new Vector2(halfWidth * 0.75f, 0f));
            p.LineTo(basePoint + new Vector2(halfWidth, 0f));
            p.ClosePath();
            p.Fill();
        }

        private static Color Hex(int rgb) => new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f);
    }
}
