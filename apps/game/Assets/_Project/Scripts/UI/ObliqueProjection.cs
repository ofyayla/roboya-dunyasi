using UnityEngine;

namespace Roboya.UI
{
    /// <summary>
    /// Mild one-point perspective for grid boards seen from the front and above. Grid coordinates are continuous
    /// (cell centre = x + 0.5, y + 0.5; y grows towards the viewer). Screen x and y are both linear in the depth
    /// scale, so grid lines stay straight and "up on screen" still means north, which young children rely on.
    /// </summary>
    public readonly struct ObliqueProjection
    {
        private readonly float _depthStep;
        private readonly float _centerX;
        private readonly float _top;

        /// <param name="area">Space available for the board, in panel units.</param>
        /// <param name="topScale">Size of the back row relative to the front row (1 = no perspective).</param>
        /// <param name="rowRatio">Average on-screen row height relative to the cell width (tilt).</param>
        /// <param name="headroom">Space above the back row for standing sprites, in front-cell units.</param>
        /// <param name="slab">Thickness of the board's front face, in front-cell units.</param>
        /// <param name="sideMargin">Extra width reserved for edge decoration, in front-cell units.</param>
        public ObliqueProjection(
            int columns,
            int rows,
            Vector2 area,
            float topScale = 0.8f,
            float rowRatio = 0.7f,
            float headroom = 0.75f,
            float slab = 0.3f,
            float sideMargin = 1.2f)
        {
            Columns = columns;
            Rows = rows;
            TopScale = Mathf.Clamp(topScale, 0.3f, 1f);
            _depthStep = (1f / TopScale - 1f) / Mathf.Max(1, rows);

            float groundUnits = rowRatio * rows * (1f + TopScale) * 0.5f;
            float heightUnits = (headroom * TopScale) + groundUnits + slab;
            float widthUnits = columns + sideMargin;
            Cell = Mathf.Max(0f, Mathf.Floor(Mathf.Min(area.x / widthUnits, area.y / heightUnits)));
            GroundHeight = Cell * groundUnits;
            SlabHeight = Cell * slab;
            _centerX = area.x * 0.5f;
            _top = ((area.y - (Cell * heightUnits)) * 0.5f) + (Cell * headroom * TopScale);
        }

        public int Columns { get; }

        public int Rows { get; }

        /// <summary>Width of a front-row cell in panel units.</summary>
        public float Cell { get; }

        public float TopScale { get; }

        public float GroundHeight { get; }

        public float SlabHeight { get; }

        public bool IsValid => Cell > 0f;

        /// <summary>Size factor at depth <paramref name="gy"/>: TopScale at the back edge, 1 at the front edge.</summary>
        public float ScaleAt(float gy) => 1f / (1f + ((Rows - gy) * _depthStep));

        /// <summary>Screen position of a point on the board surface.</summary>
        public Vector2 Project(float gx, float gy)
        {
            float s = ScaleAt(gy);
            float depth = TopScale >= 1f ? gy / Mathf.Max(1, Rows) : (s - TopScale) / (1f - TopScale);
            return new Vector2(_centerX + ((gx - (Columns * 0.5f)) * Cell * s), _top + (depth * GroundHeight));
        }

        /// <summary>Screen position of the centre of cell (x, y).</summary>
        public Vector2 CellCenter(int x, int y) => Project(x + 0.5f, y + 0.5f);
    }
}
