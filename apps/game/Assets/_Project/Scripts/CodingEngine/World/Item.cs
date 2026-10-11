namespace Roboya.CodingEngine.World
{
    /// <summary>A collectible object (flower, honey, gear...). Collected automatically when the robot enters its cell.</summary>
    public sealed class Item
    {
        public Item(string id, string kind, string color, GridPosition position, int amount = 1)
        {
            Id = id;
            Kind = kind;
            Color = color;
            Position = position;
            Amount = amount;
        }

        public string Id { get; }

        public string Kind { get; }

        /// <summary>Optional colour tag (BAL-01). Never the only signal in the UI; the view adds a shape.</summary>
        public string Color { get; }

        public GridPosition Position { get; }

        /// <summary>Honey this item puts in the basket when collected (honey items only; 1 unless the level says more).</summary>
        public int Amount { get; }
    }
}
