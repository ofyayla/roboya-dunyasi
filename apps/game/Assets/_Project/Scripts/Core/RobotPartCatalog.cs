using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Roboya.CodingEngine.Progress;

namespace Roboya.Core
{
    /// <summary>Where a part sits on Roboya's front sprite, as fractions of its width and height.</summary>
    public sealed class PartPlacement
    {
        public PartPlacement(string sprite, float x, float y, float width, bool behind)
        {
            Sprite = sprite;
            X = x;
            Y = y;
            Width = width;
            Behind = behind;
        }

        public string Sprite { get; }

        public float X { get; }

        public float Y { get; }

        public float Width { get; }

        /// <summary>Drawn behind the body (wings).</summary>
        public bool Behind { get; }
    }

    /// <summary>content/rewards/robot-parts.json: the fixed earning order (ILR-03) plus how each part is drawn.</summary>
    public sealed class RobotPartCatalog
    {
        public const string File = "rewards/robot-parts.json";
        public const string ColorSlot = "color";

        private static readonly string[] Slots = { "antenna", ColorSlot, "wings", "hat" };

        private readonly Dictionary<string, PartPlacement> _placements;

        private RobotPartCatalog(List<RobotPart> parts, Dictionary<string, PartPlacement> placements)
        {
            Parts = parts;
            _placements = placements;
        }

        public IReadOnlyList<RobotPart> Parts { get; }

        public static IReadOnlyList<string> AllSlots => Slots;

        public PartPlacement PlacementOf(string partId) =>
            partId != null && _placements.TryGetValue(partId, out var p) ? p : null;

        public RobotPart Find(string partId)
        {
            foreach (var part in Parts)
            {
                if (part.Id == partId)
                {
                    return part;
                }
            }

            return null;
        }

        public static RobotPartCatalog Parse(string json)
        {
            var file = JsonConvert.DeserializeObject<CatalogFile>(json) ?? throw new FormatException("Empty robot part catalog.");
            var parts = new List<RobotPart>();
            var placements = new Dictionary<string, PartPlacement>(StringComparer.Ordinal);
            var orders = new HashSet<int>();
            foreach (var e in file.Parts ?? new List<Entry>())
            {
                if (string.IsNullOrEmpty(e.Id) || Array.IndexOf(Slots, e.Slot) < 0 || string.IsNullOrEmpty(e.Sprite))
                {
                    throw new FormatException("Robot part '" + e.Id + "' needs an id, a known slot and a sprite.");
                }

                if (!orders.Add(e.Order) || e.Order < 1 || placements.ContainsKey(e.Id))
                {
                    throw new FormatException("Robot part '" + e.Id + "' has a duplicate id or order.");
                }

                parts.Add(new RobotPart(e.Id, e.Slot, e.Order));
                placements[e.Id] = new PartPlacement(e.Sprite, e.X, e.Y, e.Width, e.Behind);
            }

            parts.Sort((a, b) => a.Order.CompareTo(b.Order));
            return new RobotPartCatalog(parts, placements);
        }

        private sealed class CatalogFile
        {
            [JsonProperty("parts")]
            public List<Entry> Parts { get; set; }
        }

        private sealed class Entry
        {
            [JsonProperty("id")] public string Id { get; set; }

            [JsonProperty("slot")] public string Slot { get; set; }

            [JsonProperty("order")] public int Order { get; set; }

            [JsonProperty("sprite")] public string Sprite { get; set; }

            [JsonProperty("x")] public float X { get; set; }

            [JsonProperty("y")] public float Y { get; set; }

            [JsonProperty("width")] public float Width { get; set; }

            [JsonProperty("behind")] public bool Behind { get; set; }
        }
    }
}
