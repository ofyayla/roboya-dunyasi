using System;
using System.Collections.Generic;
using Roboya.CodingEngine.Profiles;
using Roboya.Core;
using Roboya.UI;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>
    /// Create or edit a child profile (F1-10): a nickname, an avatar and an age band, nothing else. The parent fills it
    /// in; the child sees only the avatar afterwards. Targets are 72 units (adult screen, ≥ 48 dp).
    /// </summary>
    public sealed class ProfileEditorView : VisualElement
    {
        private readonly LocalizedStrings _strings;
        private readonly Action<string, string, AgeBand> _onSave;
        private readonly TextField _nickname = new TextField { name = "profile-nickname", maxLength = ChildProfileData.MaxNicknameLength };
        private readonly Dictionary<string, VisualElement> _avatars = new Dictionary<string, VisualElement>();
        private readonly Dictionary<AgeBand, Button> _ages = new Dictionary<AgeBand, Button>();
        private readonly Button _save;
        private string _avatar = AvatarCatalog.Default;
        private AgeBand _band = AgeBand.Minik;

        public ProfileEditorView(LocalizedStrings strings, Action<string, string, AgeBand> onSave, Action onCancel)
        {
            _strings = strings;
            _onSave = onSave;
            name = "profile-editor";
            AddToClassList("editor");
            style.display = DisplayStyle.None;

            var card = new VisualElement();
            card.AddToClassList("editor__card");
            card.Add(Title(strings.Get(StringKeys.ProfileTitle)));

            card.Add(Caption(strings.Get(StringKeys.ProfileNickname)));
            _nickname.AddToClassList("editor__field");
            _nickname.RegisterValueChangedCallback(_ => RefreshSave());
            card.Add(_nickname);

            card.Add(Caption(strings.Get(StringKeys.ProfileAvatar)));
            var avatars = new VisualElement();
            avatars.AddToClassList("editor__avatars");
            foreach (var id in AvatarCatalog.All)
            {
                var tile = new VisualElement { name = "avatar-" + id };
                tile.AddToClassList("editor__avatar");
                var icon = AvatarCatalog.Create(id);
                tile.Add(icon);
                string captured = id;
                tile.RegisterCallback<ClickEvent>(_ => SelectAvatar(captured));
                avatars.Add(tile);
                _avatars[id] = tile;
            }

            card.Add(avatars);

            card.Add(Caption(strings.Get(StringKeys.ProfileAge)));
            var ages = new VisualElement();
            ages.AddToClassList("editor__ages");
            AddAge(ages, AgeBand.Minik, StringKeys.ProfileAgeMinik);
            AddAge(ages, AgeBand.Kasif, StringKeys.ProfileAgeKasif);
            AddAge(ages, AgeBand.Mucit, StringKeys.ProfileAgeMucit);
            card.Add(ages);

            var buttons = new VisualElement();
            buttons.AddToClassList("editor__buttons");
            var cancel = new Button(() =>
            {
                Close();
                onCancel?.Invoke();
            })
            {
                name = "profile-cancel",
                text = strings.Get(StringKeys.ProfileCancel),
            };
            cancel.AddToClassList("gate__cancel");
            _save = new Button(Save) { name = "profile-save", text = strings.Get(StringKeys.ProfileSave) };
            _save.AddToClassList("gate__key");
            _save.AddToClassList("gate__key--confirm");
            _save.AddToClassList("editor__save");
            buttons.Add(cancel);
            buttons.Add(_save);
            card.Add(buttons);
            Add(card);
        }

        public bool IsOpen => style.display == DisplayStyle.Flex;

        private string EditingId { get; set; }

        /// <summary>Opens the editor for a new profile, or for <paramref name="existing"/>.</summary>
        public void Open(ChildProfileData existing = null)
        {
            EditingId = existing?.Id;
            _nickname.value = existing?.Nickname ?? string.Empty;
            SelectAvatar(existing?.AvatarId ?? AvatarCatalog.Default);
            SelectAge(existing?.AgeBand ?? AgeBand.Minik);
            style.display = DisplayStyle.Flex;
            RefreshSave();
        }

        public void Close() => style.display = DisplayStyle.None;

        private void AddAge(VisualElement parent, AgeBand band, string key)
        {
            var button = new Button(() => SelectAge(band)) { name = "age-" + band.ToString().ToLowerInvariant(), text = _strings.Get(key) };
            button.AddToClassList("editor__age");
            parent.Add(button);
            _ages[band] = button;
        }

        private void SelectAvatar(string id)
        {
            _avatar = id;
            foreach (var pair in _avatars)
            {
                pair.Value.EnableInClassList("is-selected", pair.Key == id);
            }
        }

        private void SelectAge(AgeBand band)
        {
            _band = band;
            foreach (var pair in _ages)
            {
                pair.Value.EnableInClassList("is-selected", pair.Key == band);
            }
        }

        private void RefreshSave() => _save.SetEnabled(!string.IsNullOrWhiteSpace(_nickname.value));

        private void Save()
        {
            string nickname = _nickname.value;
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return;
            }

            Close();
            _onSave(nickname, _avatar, _band);
        }

        /// <summary>The profile being edited, or null for a new one; the controller reads it when saving.</summary>
        public string Editing => EditingId;

        private static Label Title(string text)
        {
            var label = new Label(text);
            label.AddToClassList("parent__title");
            return label;
        }

        private static Label Caption(string text)
        {
            var label = new Label(text);
            label.AddToClassList("editor__caption");
            return label;
        }
    }
}
