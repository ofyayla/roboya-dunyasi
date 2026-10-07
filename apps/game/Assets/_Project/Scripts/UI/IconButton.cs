using System;
using UnityEngine.UIElements;

namespace Roboya.UI
{
    /// <summary>Text-free round button for child screens (Minik level has no on-screen text, OYN-01).</summary>
    public sealed class IconButton : Button
    {
        public IconButton(IconKind kind, Action onClick)
            : base(onClick)
        {
            AddToClassList("icon-button");
            Icon = new Icon(kind);
            Add(Icon);
        }

        public Icon Icon { get; }
    }
}
