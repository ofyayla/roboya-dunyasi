using System;
using Roboya.Core;
using Roboya.UI;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>
    /// The adult area behind the parental gate (VEL-01). It is a shell: later features add their sections here
    /// (daily time limit, progress report, privacy centre, subscription) and every one of them is protected by
    /// the same gate because they are only reachable through this view.
    /// </summary>
    public sealed class ParentView : VisualElement
    {
        private readonly ScrollView _sections = new ScrollView { name = "parent-sections" };
        private readonly Label _empty;

        public ParentView(LocalizedStrings strings, Action onBack)
        {
            name = "parent";
            AddToClassList("parent");
            style.display = DisplayStyle.None;
            var title = new Label(strings.Get(StringKeys.ParentTitle));
            title.AddToClassList("parent__title");
            _empty = new Label(strings.Get(StringKeys.ParentEmpty)) { name = "parent-empty" };
            _empty.AddToClassList("parent__empty");
            _sections.AddToClassList("parent__sections");
            var back = new Button(onBack) { name = "parent-back", text = strings.Get(StringKeys.ParentBack) };
            back.AddToClassList("parent__back");
            Add(title);
            Add(_sections);
            Add(_empty);
            Add(back);
        }

        /// <summary>Adds a section (called by features as they arrive); hides the placeholder line.</summary>
        public void AddSection(VisualElement section)
        {
            _sections.Add(section);
            _empty.style.display = DisplayStyle.None;
        }
    }
}
