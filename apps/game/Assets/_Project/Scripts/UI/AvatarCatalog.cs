using System.Collections.Generic;
using UnityEngine;

namespace Roboya.UI
{
    /// <summary>
    /// The avatars a child can pick (F1-10): Roboya in six colours, drawn in code. The id is what is stored; it matches
    /// the server's allowed pattern and carries no personal data. Colour is paired with the nickname, never the only cue.
    /// </summary>
    public static class AvatarCatalog
    {
        public const string Default = "robot-turuncu";

        public static readonly IReadOnlyList<string> All = new[]
        {
            "robot-turuncu", "robot-mavi", "robot-mor", "robot-yesil", "robot-pembe", "robot-sari",
        };

        public static Color ColorOf(string avatarId)
        {
            switch (avatarId)
            {
                case "robot-mavi": return new Color(0.24f, 0.49f, 0.85f);
                case "robot-mor": return new Color(0.56f, 0.36f, 0.82f);
                case "robot-yesil": return new Color(0.33f, 0.66f, 0.33f);
                case "robot-pembe": return new Color(0.93f, 0.45f, 0.62f);
                case "robot-sari": return new Color(0.96f, 0.78f, 0.2f);
                default: return new Color(0.95f, 0.55f, 0.16f);
            }
        }

        public static Icon Create(string avatarId) =>
            new Icon(IconKind.Robot) { Color = ColorOf(avatarId), Accent = Color.white };
    }
}
