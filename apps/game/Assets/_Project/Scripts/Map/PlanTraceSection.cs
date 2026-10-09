using Roboya.Core;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>The plan shadow on the board for the active child: on or off (default on for Minik and Kaşif).</summary>
    public sealed class PlanTraceSection : VisualElement, IRefreshable
    {
        private readonly GameServices _services;
        private readonly Button _on;
        private readonly Button _off;

        public PlanTraceSection(GameServices services)
        {
            _services = services;
            name = "trace-section";
            AddToClassList("section");
            var title = new Label(services.Strings.Get(StringKeys.TraceTitle));
            title.AddToClassList("section__title");
            Add(title);

            var row = new VisualElement();
            row.AddToClassList("editor__ages");
            _on = new Button(() => Choose(true)) { name = "trace-on", text = services.Strings.Get(StringKeys.TraceOn) };
            _off = new Button(() => Choose(false)) { name = "trace-off", text = services.Strings.Get(StringKeys.TraceOff) };
            _on.AddToClassList("editor__age");
            _off.AddToClassList("editor__age");
            row.Add(_on);
            row.Add(_off);
            Add(row);

            var note = new Label(services.Strings.Get(StringKeys.TraceNote));
            note.AddToClassList("section__note");
            note.style.color = new UnityEngine.Color(0.29f, 0.2f, 0.14f);
            Add(note);
            services.Profiles.Changed += Refresh;
            Refresh();
        }

        public void Refresh()
        {
            bool active = _services.Profiles.HasActive;
            style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
            if (!active)
            {
                return;
            }

            bool on = _services.ScreenTime.PlanTraceEnabled;
            _on.EnableInClassList("is-selected", on);
            _off.EnableInClassList("is-selected", !on);
        }

        private void Choose(bool enabled)
        {
            _services.ScreenTime.SetPlanTrace(enabled);
            Refresh();
        }
    }
}
