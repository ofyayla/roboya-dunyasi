using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Roboya.CodingEngine.Progress;

namespace Roboya.Core
{
    /// <summary>One sprite of a ship part, placed as fractions of the ship body sprite.</summary>
    public sealed class ShipLayer
    {
        public ShipLayer(string sprite, float x, float y, float width, bool behind)
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

        /// <summary>Drawn behind the body (fins, legs, nozzle).</summary>
        public bool Behind { get; }
    }

    /// <summary>
    /// content/rewards/ship-parts.json: the fixed earning order of ship repair parts (ILR-03) and where each part's
    /// sprites sit on the broken ship body.
    /// </summary>
    public sealed class ShipPartCatalog
    {
        public const string File = "rewards/ship-parts.json";

        private readonly Dictionary<string, IReadOnlyList<ShipLayer>> _layers;

        private ShipPartCatalog(string baseSprite, List<ShipPart> parts, Dictionary<string, IReadOnlyList<ShipLayer>> layers)
        {
            BaseSprite = baseSprite;
            Parts = parts;
            _layers = layers;
        }

        /// <summary>The broken ship body every part is placed on.</summary>
        public string BaseSprite { get; }

        public IReadOnlyList<ShipPart> Parts { get; }

        public IReadOnlyList<ShipLayer> LayersOf(string partId) =>
            partId != null && _layers.TryGetValue(partId, out var l) ? l : Array.Empty<ShipLayer>();

        public static ShipPartCatalog Parse(string json)
        {
            var file = JsonConvert.DeserializeObject<CatalogFile>(json) ?? throw new FormatException("Empty ship part catalog.");
            if (string.IsNullOrEmpty(file.Base))
            {
                throw new FormatException("Ship part catalog needs a base sprite.");
            }

            var parts = new List<ShipPart>();
            var layers = new Dictionary<string, IReadOnlyList<ShipLayer>>(StringComparer.Ordinal);
            var orders = new HashSet<int>();
            foreach (var e in file.Parts ?? new List<Entry>())
            {
                if (string.IsNullOrEmpty(e.Id) || e.Layers == null || e.Layers.Count == 0)
                {
                    throw new FormatException("Ship part '" + e.Id + "' needs an id and at least one layer.");
                }

                if (!orders.Add(e.Order) || e.Order < 1 || layers.ContainsKey(e.Id))
                {
                    throw new FormatException("Ship part '" + e.Id + "' has a duplicate id or order.");
                }

                var list = new List<ShipLayer>();
                foreach (var l in e.Layers)
                {
                    if (string.IsNullOrEmpty(l.Sprite) || l.Width <= 0f)
                    {
                        throw new FormatException("Ship part '" + e.Id + "' has a layer without sprite or width.");
                    }

                    list.Add(new ShipLayer(l.Sprite, l.X, l.Y, l.Width, l.Behind));
                }

                parts.Add(new ShipPart(e.Id, e.Order));
                layers[e.Id] = list;
            }

            parts.Sort((a, b) => a.Order.CompareTo(b.Order));
            return new ShipPartCatalog(file.Base, parts, layers);
        }

        private sealed class CatalogFile
        {
            [JsonProperty("base")] public string Base { get; set; }

            [JsonProperty("parts")] public List<Entry> Parts { get; set; }
        }

        private sealed class Entry
        {
            [JsonProperty("id")] public string Id { get; set; }

            [JsonProperty("order")] public int Order { get; set; }

            [JsonProperty("layers")] public List<LayerEntry> Layers { get; set; }
        }

        private sealed class LayerEntry
        {
            [JsonProperty("sprite")] public string Sprite { get; set; }

            [JsonProperty("x")] public float X { get; set; }

            [JsonProperty("y")] public float Y { get; set; }

            [JsonProperty("width")] public float Width { get; set; }

            [JsonProperty("behind")] public bool Behind { get; set; }
        }
    }
}
