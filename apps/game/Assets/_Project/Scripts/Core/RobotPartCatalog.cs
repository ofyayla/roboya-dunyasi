using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Roboya.CodingEngine.Progress;

namespace Roboya.Core
{
    /// <summary>The point on a pose that a part is placed from (see <see cref="RobotAnchors"/>).</summary>
    public enum PartAnchor
    {
        None,

        /// <summary>The antenna bulb; the part replaces it.</summary>
        Bulb,

        /// <summary>Top centre of the head; hats sit here.</summary>
        HeadTop,

        /// <summary>Centre of the head; wings spread from here.</summary>
        HeadCenter,
    }

    /// <summary>
    /// How a part is drawn on any pose: from an anchor, sized relative to the head width (the bulb width for bulb
    /// parts), nudged down by <see cref="Dy"/> head heights. Colour parts name a sprite per pose with {pose}.
    /// </summary>
    public sealed class PartPlacement
    {
        public PartPlacement(string sprite, PartAnchor anchor, float scale, float dy, bool behind)
        {
            Sprite = sprite;
            Anchor = anchor;
            Scale = scale;
            Dy = dy;
            Behind = behind;
        }

        public string Sprite { get; }

        public PartAnchor Anchor { get; }

        public float Scale { get; }

        public float Dy { get; }

        /// <summary>Drawn behind the body (wings).</summary>
        public bool Behind { get; }

        public bool IsColourVariant => Anchor == PartAnchor.None;

        /// <summary>The sprite name for a pose; colour variants are drawn per pose.</summary>
        public string SpriteFor(string pose) => Sprite.Replace("{pose}", pose);
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

                var anchor = ParseAnchor(e.Anchor);
                if ((e.Slot == ColorSlot) != (anchor == PartAnchor.None) || e.Scale < 0f)
                {
                    throw new FormatException("Robot part '" + e.Id + "': only colour parts omit the anchor, and scale cannot be negative.");
                }

                if (!orders.Add(e.Order) || e.Order < 1 || placements.ContainsKey(e.Id))
                {
                    throw new FormatException("Robot part '" + e.Id + "' has a duplicate id or order.");
                }

                parts.Add(new RobotPart(e.Id, e.Slot, e.Order));
                placements[e.Id] = new PartPlacement(e.Sprite, anchor, e.Scale, e.Dy, e.Behind);
            }

            parts.Sort((a, b) => a.Order.CompareTo(b.Order));
            return new RobotPartCatalog(parts, placements);
        }

        private static PartAnchor ParseAnchor(string value)
        {
            switch (value)
            {
                case null:
                case "":
                    return PartAnchor.None;
                case "bulb": return PartAnchor.Bulb;
                case "head-top": return PartAnchor.HeadTop;
                case "head-center": return PartAnchor.HeadCenter;
                default: throw new FormatException("Unknown part anchor '" + value + "'.");
            }
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

            [JsonProperty("anchor")] public string Anchor { get; set; }

            [JsonProperty("scale")] public float Scale { get; set; }

            [JsonProperty("dy")] public float Dy { get; set; }

            [JsonProperty("behind")] public bool Behind { get; set; }
        }
    }
}
