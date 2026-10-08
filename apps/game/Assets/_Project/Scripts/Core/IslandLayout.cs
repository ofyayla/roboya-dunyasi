using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Roboya.Core
{
    /// <summary>content/map/island.json: region hotspots on the island illustration, as fractions of the image.</summary>
    public sealed class IslandLayout
    {
        public const string File = "map/island.json";

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("regions")]
        public List<Region> Regions { get; set; } = new List<Region>();

        [JsonProperty("ship")]
        public Spot Ship { get; set; }

        public static IslandLayout Parse(string json)
        {
            var layout = JsonConvert.DeserializeObject<IslandLayout>(json) ?? throw new FormatException("Empty island layout.");
            foreach (var r in layout.Regions)
            {
                if (string.IsNullOrEmpty(r.Id) || r.X < 0f || r.X > 1f || r.Y < 0f || r.Y > 1f || r.Radius <= 0f)
                {
                    throw new FormatException("Island region '" + r.Id + "' needs an id and a position inside the image.");
                }
            }

            return layout;
        }

        public sealed class Region : Spot
        {
            /// <summary>Wire id of the region (same as level files, e.g. sabir-ormani).</summary>
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("radius")]
            public float Radius { get; set; }
        }

        public class Spot
        {
            [JsonProperty("x")]
            public float X { get; set; }

            [JsonProperty("y")]
            public float Y { get; set; }
        }
    }
}
