using System;

namespace Roboya.CodingEngine.World
{
    public enum DropPointKind
    {
        /// <summary>A bee family's honeycomb: it asks for <see cref="DropPoint.Need"/> honey.</summary>
        Hive,

        /// <summary>The village's shared table: takes whatever is left over.</summary>
        Table,
    }

    /// <summary>A cell where the bee can leave honey (ADR 0030). The cell is walkable.</summary>
    public sealed class DropPoint
    {
        public DropPoint(string id, DropPointKind kind, GridPosition position, int need = 0)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Kind = kind;
            Position = position;
            Need = need;
        }

        public string Id { get; }

        public DropPointKind Kind { get; }

        public GridPosition Position { get; }

        /// <summary>Honey a hive asks for; 0 for the table.</summary>
        public int Need { get; }
    }
}
