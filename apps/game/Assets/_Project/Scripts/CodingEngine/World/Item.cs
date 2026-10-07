namespace Roboya.CodingEngine.World
{
    /// <summary>A collectible object (flower, honey, gear...). Collected automatically when the robot enters its cell.</summary>
    public sealed class Item
    {
        public Item(string id, string kind, string color, GridPosition position)
        {
            Id = id;
            Kind = kind;
            Color = color;
            Position = position;
        }

        public string Id { get; }

        public string Kind { get; }

        /// <summary>Optional colour tag (BAL-01). Never the only signal in the UI; the view adds a shape.</summary>
        public string Color { get; }

        public GridPosition Position { get; }
    }
}
