using System.Collections.Generic;
using Roboya.CodingEngine.Parents;
using Roboya.Core;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>The daily time limit for the active child (F1-11): 10 / 15 / 20 / 30 minutes or unlimited, with today's use.</summary>
    public sealed class ScreenTimeSection : VisualElement
    {
        private readonly GameServices _services;
        private readonly Dictionary<int, Button> _buttons = new Dictionary<int, Button>();
        private readonly Label _today = new Label { name = "time-today" };

        public ScreenTimeSection(GameServices services)
        {
            _services = services;
            name = "time-section";
            AddToClassList("section");
            var title = new Label(services.Strings.Get(StringKeys.TimeTitle));
            title.AddToClassList("section__title");
            Add(title);

            var row = new VisualElement();
            row.AddToClassList("editor__ages");
            foreach (int minutes in ScreenTimeRules.Options)
            {
                int captured = minutes;
                string text = minutes == 0
                    ? services.Strings.Get(StringKeys.TimeUnlimited)
                    : services.Strings.Format(StringKeys.TimeMinutes, minutes);
                var button = new Button(() => Choose(captured)) { name = "time-" + minutes, text = text };
                button.AddToClassList("editor__age");
                row.Add(button);
                _buttons[minutes] = button;
            }

            Add(row);
            _today.AddToClassList("profile-row__band");
            Add(_today);
            var note = new Label(services.Strings.Get(StringKeys.TimeNote));
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

            int current = _services.ScreenTime.LimitMinutes;
            foreach (var pair in _buttons)
            {
                pair.Value.EnableInClassList("is-selected", pair.Key == current);
            }

            _today.text = _services.Strings.Format(StringKeys.TimeToday, _services.ScreenTime.UsedMinutesToday);
        }

        private void Choose(int minutes)
        {
            _services.ScreenTime.SetLimit(minutes);
            Refresh();
        }
    }
}
