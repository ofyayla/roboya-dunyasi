using System;
using System.Threading.Tasks;
using Roboya.Core;
using Roboya.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>
    /// Privacy centre in the parent area (F1-13, UYM-03): read the notice, see and save the data the account holds, withdraw consent,
    /// wipe this device, ask for deletion. Destructive actions need a second tap. Local actions work offline.
    /// </summary>
    public sealed class PrivacySection : VisualElement, IRefreshable
    {
        private readonly GameServices _services;
        private readonly Action _onRead;
        private readonly Action _onConsentGone;
        private readonly Label _message = new Label { name = "privacy-message" };
        private readonly VisualElement _server = new VisualElement { name = "privacy-server" };
        private readonly Button _delete;
        private readonly Button _cancelDelete;
        private string _confirming;
        private bool _busy;

        public PrivacySection(GameServices services, Action onRead, Action onConsentGone)
        {
            _services = services;
            _onRead = onRead;
            _onConsentGone = onConsentGone;
            name = "privacy-section";
            AddToClassList("section");
            var s = services.Strings;
            var title = new Label(s.Get(StringKeys.PrivacyTitle));
            title.AddToClassList("section__title");
            Add(title);

            Add(Make("privacy-read", StringKeys.PrivacyRead, () => _onRead()));
            Add(Make("privacy-withdraw", StringKeys.PrivacyWithdraw, () => Confirm("withdraw", StringKeys.PrivacyWithdrawConfirm, WithdrawAsync)));
            Add(Make("privacy-wipe", StringKeys.PrivacyWipe, () => Confirm("wipe", StringKeys.PrivacyWipeConfirm, WipeAsync)));

            _server.Add(Make("privacy-view", StringKeys.PrivacyView, () => _ = RunAsync(ViewAsync)));
            _server.Add(Make("privacy-export", StringKeys.PrivacyExport, () => _ = RunAsync(ExportAsync)));
            _delete = Make("privacy-delete", StringKeys.PrivacyDelete, () => Confirm("delete", StringKeys.PrivacyDeleteConfirm, DeleteAsync));
            _server.Add(_delete);
            _cancelDelete = Make("privacy-delete-cancel", StringKeys.PrivacyDeleteCancel, () => _ = RunAsync(CancelDeleteAsync));
            _server.Add(_cancelDelete);
            Add(_server);

            _message.AddToClassList("report__line");
            Add(_message);
            services.Account.Changed += Refresh;
            Refresh();
        }

        public void Refresh()
        {
            bool signedIn = _services.Privacy.CanUseServer;
            _server.style.display = signedIn ? DisplayStyle.Flex : DisplayStyle.None;
            _confirming = null;
            if (!signedIn && _services.Account.IsConfigured)
            {
                _message.text = _services.Strings.Get(StringKeys.PrivacyNeedAccount);
            }
            else if (!_busy)
            {
                _message.text = string.Empty;
            }
        }

        private Button Make(string id, string key, Action action)
        {
            var button = new Button(action) { name = id, text = _services.Strings.Get(key) };
            button.AddToClassList("editor__age");
            button.style.marginTop = 6;
            return button;
        }

        /// <summary>First tap asks, second tap of the same button does it.</summary>
        private void Confirm(string id, string confirmKey, Func<Task> action)
        {
            if (_confirming == id)
            {
                _confirming = null;
                _ = RunAsync(action);
                return;
            }

            _confirming = id;
            _message.text = _services.Strings.Get(confirmKey);
        }

        private async Task RunAsync(Func<Task> action)
        {
            if (_busy)
            {
                return;
            }

            _busy = true;
            try
            {
                await action();
            }
            catch (ApiException)
            {
                _message.text = _services.Strings.Get(StringKeys.PrivacyError);
            }
            finally
            {
                _busy = false;
            }
        }

        private async Task WithdrawAsync()
        {
            await _services.Privacy.WithdrawConsentAsync();
            _onConsentGone();
        }

        private async Task WipeAsync()
        {
            await _services.Privacy.WipeDeviceAsync();
            _onConsentGone();
        }

        private async Task ViewAsync()
        {
            var data = await _services.Privacy.GetMyDataAsync();
            var s = _services.Strings;
            string text = s.Format(
                StringKeys.PrivacyViewSummary, data.Email, data.ProfileNicknames.Count, data.Devices, data.Consents, data.Subscriptions);
            if (data.ProfileNicknames.Count > 0)
            {
                text += " " + s.Format(StringKeys.PrivacyViewProfiles, string.Join(", ", data.ProfileNicknames));
            }

            _message.text = text;
        }

        private async Task ExportAsync()
        {
            string path = await _services.Privacy.ExportAsync();
            _message.text = _services.Strings.Format(StringKeys.PrivacyExportDone, path);
        }

        private async Task DeleteAsync()
        {
            var when = await _services.Privacy.RequestDeletionAsync();
            _message.text = _services.Strings.Format(StringKeys.PrivacyDeletePending, when.ToLocalTime().ToString("d MMMM yyyy"));
        }

        private async Task CancelDeleteAsync()
        {
            await _services.Privacy.CancelDeletionAsync();
            _message.text = _services.Strings.Get(StringKeys.PrivacyDeleteCancelled);
        }
    }
}
