using System;
using Roboya.Core;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>
    /// Shows the privacy notice and asks the parent for explicit consent before the first profile is made (UYM-01).
    /// Declining leaves no profile and no data behind.
    /// </summary>
    public sealed class NoticeView : VisualElement
    {
        public NoticeView(GameServices services, Action onAccepted, Action onDeclined)
        {
            name = "notice";
            AddToClassList("editor");
            style.display = DisplayStyle.None;
            var strings = services.Strings;

            var card = new VisualElement();
            card.AddToClassList("editor__card");
            card.AddToClassList("notice__card");
            var title = new Label(strings.Get(StringKeys.OnboardingNoticeTitle));
            title.AddToClassList("parent__title");
            card.Add(title);

            var scroll = new ScrollView(ScrollViewMode.Vertical) { name = "notice-scroll" };
            scroll.AddToClassList("notice__scroll");
            var text = new Label(services.Notice.Text) { name = "notice-text" };
            text.AddToClassList("notice__text");
            scroll.Add(text);
            card.Add(scroll);

            var buttons = new VisualElement();
            buttons.AddToClassList("editor__buttons");
            var decline = new Button(() =>
            {
                Close();
                onDeclined();
            })
            {
                name = "notice-decline",
                text = strings.Get(StringKeys.OnboardingDecline),
            };
            decline.AddToClassList("gate__cancel");
            var accept = new Button(() =>
            {
                services.Profiles.RecordConsent(services.Notice.Version);
                Close();
                onAccepted();
            })
            {
                name = "notice-accept",
                text = strings.Get(StringKeys.OnboardingAccept),
            };
            accept.AddToClassList("gate__key");
            accept.AddToClassList("gate__key--confirm");
            accept.AddToClassList("editor__save");
            buttons.Add(decline);
            buttons.Add(accept);
            card.Add(buttons);
            Add(card);
        }

        public bool IsOpen => style.display == DisplayStyle.Flex;

        public void Open() => style.display = DisplayStyle.Flex;

        public void Close() => style.display = DisplayStyle.None;
    }
}
