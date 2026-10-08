using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Roboya.Core
{
    /// <summary>One pose's measured points, as fractions of the sprite (tools/art/robot_parts.py).</summary>
    public sealed class PoseAnchors
    {
        public PoseAnchors(float headLeft, float headTop, float headRight, float headBottom, float? bulbX, float? bulbY, float bulbWidth)
        {
            HeadLeft = headLeft;
            HeadTop = headTop;
            HeadRight = headRight;
            HeadBottom = headBottom;
            HasBulb = bulbX.HasValue && bulbY.HasValue;
            BulbX = bulbX ?? 0f;
            BulbY = bulbY ?? 0f;
            BulbWidth = bulbWidth;
        }

        public float HeadLeft { get; }

        public float HeadTop { get; }

        public float HeadRight { get; }

        public float HeadBottom { get; }

        public float HeadWidth => HeadRight - HeadLeft;

        public float HeadHeight => HeadBottom - HeadTop;

        public float HeadCenterX => (HeadLeft + HeadRight) * 0.5f;

        public float HeadCenterY => (HeadTop + HeadBottom) * 0.5f;

        public bool HasBulb { get; }

        public float BulbX { get; }

        public float BulbY { get; }

        public float BulbWidth { get; }
    }

    /// <summary>content/rewards/robot-anchors.json: where the head and antenna bulb are on every Roboya pose.</summary>
    public sealed class RobotAnchors
    {
        public const string File = "rewards/robot-anchors.json";

        private readonly Dictionary<string, PoseAnchors> _poses;

        private RobotAnchors(Dictionary<string, PoseAnchors> poses)
        {
            _poses = poses;
        }

        public static RobotAnchors Empty { get; } = new RobotAnchors(new Dictionary<string, PoseAnchors>());

        public PoseAnchors For(string pose) => pose != null && _poses.TryGetValue(pose, out var a) ? a : null;

        public static RobotAnchors Parse(string json)
        {
            var raw = JsonConvert.DeserializeObject<Dictionary<string, Entry>>(json) ?? throw new FormatException("Empty robot anchors.");
            var poses = new Dictionary<string, PoseAnchors>(StringComparer.Ordinal);
            foreach (var pair in raw)
            {
                var e = pair.Value;
                if (e?.Head == null || e.Head.Length != 4 || e.Head[2] <= e.Head[0] || e.Head[3] <= e.Head[1])
                {
                    throw new FormatException("Pose '" + pair.Key + "' needs a head box [left, top, right, bottom].");
                }

                bool bulb = e.Bulb != null && e.Bulb.Length == 3;
                poses[pair.Key] = new PoseAnchors(
                    e.Head[0], e.Head[1], e.Head[2], e.Head[3],
                    bulb ? e.Bulb[0] : (float?)null, bulb ? e.Bulb[1] : (float?)null, bulb ? e.Bulb[2] : 0f);
            }

            return new RobotAnchors(poses);
        }

        private sealed class Entry
        {
            [JsonProperty("head")] public float[] Head { get; set; }

            [JsonProperty("bulb")] public float[] Bulb { get; set; }
        }
    }
}
