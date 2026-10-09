using Roboya.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>Background music on or off for this device (grown-up area).</summary>
    public sealed class MusicSection : VisualElement, IRefreshable
    {
        private readonly GameServices _services;
        private readonly Button _on;
        private readonly Button _off;

        public MusicSection(GameServices services)
        {
            _services = services;
            name = "music-section";
            AddToClassList("section");
            var title = new Label(services.Strings.Get(StringKeys.MusicTitle));
            title.AddToClassList("section__title");
            Add(title);

            var row = new VisualElement();
            row.AddToClassList("editor__ages");
            _on = new Button(() => Choose(true)) { name = "music-on", text = services.Strings.Get(StringKeys.TraceOn) };
            _off = new Button(() => Choose(false)) { name = "music-off", text = services.Strings.Get(StringKeys.TraceOff) };
            _on.AddToClassList("editor__age");
            _off.AddToClassList("editor__age");
            row.Add(_on);
            row.Add(_off);
            Add(row);

            var note = new Label(services.Strings.Get(StringKeys.MusicNote));
            note.AddToClassList("section__note");
            note.style.color = new Color(0.29f, 0.2f, 0.14f);
            Add(note);
            Refresh();
        }

        public void Refresh()
        {
            bool on = _services.Music.Enabled;
            _on.EnableInClassList("is-selected", on);
            _off.EnableInClassList("is-selected", !on);
        }

        private void Choose(bool enabled)
        {
            _services.Music.SetEnabled(enabled);
            Refresh();
        }
    }
}
