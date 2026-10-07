using System;

namespace Roboya.CodingEngine.World
{
    /// <summary>Immutable rectangular grid of cells.</summary>
    public sealed class Grid
    {
        public const int MinSize = 2;
        public const int MaxSize = 12;

        private readonly CellType[] _cells;

        public Grid(int width, int height, CellType[] cells)
        {
            if (width < MinSize || width > MaxSize || height < MinSize || height > MaxSize)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Grid size must be between " + MinSize + " and " + MaxSize + ".");
            }

            if (cells == null || cells.Length != width * height)
            {
                throw new ArgumentException("Cell count must equal width * height.", nameof(cells));
            }

            Width = width;
            Height = height;
            _cells = (CellType[])cells.Clone();
        }

        public int Width { get; }

        public int Height { get; }

        public int CellCount => _cells.Length;

        /// <summary>Parses rows such as "..#." where '.' is floor and '#' is blocked. Row 0 is north.</summary>
        public static Grid FromRows(string[] rows)
        {
            if (rows == null || rows.Length == 0)
            {
                throw new ArgumentException("At least one row is required.", nameof(rows));
            }

            int width = rows[0].Length;
            var cells = new CellType[width * rows.Length];
            for (int y = 0; y < rows.Length; y++)
            {
                if (rows[y].Length != width)
                {
                    throw new ArgumentException("Row " + y + " has a different length.", nameof(rows));
                }

                for (int x = 0; x < width; x++)
                {
                    char c = rows[y][x];
                    switch (c)
                    {
                        case '.': cells[y * width + x] = CellType.Floor; break;
                        case '#': cells[y * width + x] = CellType.Blocked; break;
                        default: throw new ArgumentException("Unknown cell '" + c + "' at " + new GridPosition(x, y) + ".", nameof(rows));
                    }
                }
            }

            return new Grid(width, rows.Length, cells);
        }

        public bool Contains(GridPosition p) => p.X >= 0 && p.Y >= 0 && p.X < Width && p.Y < Height;

        public CellType this[GridPosition p] => _cells[Index(p)];

        public bool IsWalkable(GridPosition p) => Contains(p) && _cells[Index(p)] == CellType.Floor;

        public int Index(GridPosition p) => p.Y * Width + p.X;
    }
}
