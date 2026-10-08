using System;
using Roboya.Core;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>Read-only view of the privacy notice for the parent area (no consent buttons).</summary>
    public sealed class NoticeReader : VisualElement
    {
        public NoticeReader(GameServices services, Action onClose)
        {
            name = "notice-reader";
            AddToClassList("map-view");
            AddToClassList("parent");
            style.display = DisplayStyle.None;
            var title = new Label(services.Strings.Get(StringKeys.OnboardingNoticeTitle));
            title.AddToClassList("parent__title");
            Add(title);
            var scroll = new ScrollView(ScrollViewMode.Vertical) { name = "notice-reader-scroll" };
            scroll.AddToClassList("notice__scroll");
            var text = new Label(services.Notice.Text) { name = "notice-reader-text" };
            text.AddToClassList("notice__text");
            scroll.Add(text);
            Add(scroll);
            var close = new Button(onClose) { name = "notice-reader-close", text = services.Strings.Get(StringKeys.PrivacyClose) };
            close.AddToClassList("parent__back");
            Add(close);
        }
    }
}
