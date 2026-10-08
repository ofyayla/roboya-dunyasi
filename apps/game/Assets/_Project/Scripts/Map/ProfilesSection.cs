using System;
using Roboya.CodingEngine.Profiles;
using Roboya.Core;
using Roboya.UI;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>
    /// The profile list in the parent area (F1-10): switch the active child, edit, remove (two taps) and add, up to the
    /// limit the entitlement allows (1 free, 4 premium). Everything here sits behind the parental gate.
    /// </summary>
    public sealed class ProfilesSection : VisualElement
    {
        private readonly GameServices _services;
        private readonly Action<ChildProfileData> _onEdit;
        private readonly Action _onAdd;
        private readonly VisualElement _rows = new VisualElement { name = "profile-rows" };
        private readonly Button _add;
        private readonly Label _limit;
        private string _pendingRemove;

        public ProfilesSection(GameServices services, Action<ChildProfileData> onEdit, Action onAdd)
        {
            _services = services;
            _onEdit = onEdit;
            _onAdd = onAdd;
            name = "profiles-section";
            AddToClassList("section");
            var title = new Label(services.Strings.Get(StringKeys.ProfileListTitle));
            title.AddToClassList("section__title");
            Add(title);
            Add(_rows);
            _add = new Button(() => _onAdd()) { name = "profile-add", text = services.Strings.Get(StringKeys.ProfileAdd) };
            _add.AddToClassList("gate__key");
            _add.AddToClassList("section__add");
            Add(_add);
            _limit = new Label { name = "profile-limit" };
            _limit.AddToClassList("section__note");
            Add(_limit);
            services.Profiles.Changed += Refresh;
            Refresh();
        }

        public int Limit => _services.ProgressRules.ProfileLimit(_services.Entitlements.HasPremium);

        public void Refresh()
        {
            _rows.Clear();
            var registry = _services.Profiles.Registry;
            foreach (var profile in registry.Profiles)
            {
                _rows.Add(Row(profile, registry.Active?.Id == profile.Id));
            }

            bool full = registry.Profiles.Count >= Limit;
            _add.SetEnabled(!full);
            _limit.text = full ? _services.Strings.Format(StringKeys.ProfileLimit, Limit) : string.Empty;
            _limit.style.display = full ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private VisualElement Row(ChildProfileData profile, bool active)
        {
            var row = new VisualElement { name = "profile-row-" + profile.Id };
            row.AddToClassList("profile-row");
            row.EnableInClassList("is-active", active);
            row.RegisterCallback<ClickEvent>(e =>
            {
                if (e.target == row)
                {
                    _services.Profiles.SetActive(profile.Id);
                }
            });

            var avatar = AvatarCatalog.Create(profile.AvatarId);
            avatar.AddToClassList("profile-row__avatar");
            row.Add(avatar);

            var text = new VisualElement();
            text.AddToClassList("profile-row__text");
            text.pickingMode = PickingMode.Ignore;
            var nickname = new Label(profile.Nickname) { pickingMode = PickingMode.Ignore };
            nickname.AddToClassList("profile-row__name");
            var band = new Label(AgeLabel(profile.AgeBand) + (active ? " · " + _services.Strings.Get(StringKeys.ProfileActive) : string.Empty))
            {
                pickingMode = PickingMode.Ignore,
            };
            band.AddToClassList("profile-row__band");
            text.Add(nickname);
            text.Add(band);
            row.Add(text);

            var select = new Button(() => _services.Profiles.SetActive(profile.Id)) { name = "profile-select-" + profile.Id, text = "✓" };
            select.AddToClassList("profile-row__button");
            select.SetEnabled(!active);
            var edit = new Button(() => _onEdit(profile)) { name = "profile-edit-" + profile.Id, text = "✎" };
            edit.AddToClassList("profile-row__button");
            var remove = new Button(() => OnRemove(profile)) { name = "profile-remove-" + profile.Id, text = _services.Strings.Get(StringKeys.ProfileRemove) };
            remove.AddToClassList("profile-row__button");
            remove.AddToClassList("profile-row__button--danger");
            row.Add(select);
            row.Add(edit);
            row.Add(remove);
            return row;
        }

        private void OnRemove(ChildProfileData profile)
        {
            // A first tap only asks; the second removes the profile and its progress for good.
            if (_pendingRemove == profile.Id)
            {
                _pendingRemove = null;
                _services.Profiles.Remove(profile.Id);
                return;
            }

            _pendingRemove = profile.Id;
            _limit.text = _services.Strings.Get(StringKeys.ProfileRemoveConfirm);
            _limit.style.display = DisplayStyle.Flex;
        }

        private string AgeLabel(AgeBand band)
        {
            switch (band)
            {
                case AgeBand.Kasif: return _services.Strings.Get(StringKeys.ProfileAgeKasif);
                case AgeBand.Mucit: return _services.Strings.Get(StringKeys.ProfileAgeMucit);
                default: return _services.Strings.Get(StringKeys.ProfileAgeMinik);
            }
        }
    }
}
