using System;
using System.Threading.Tasks;
using Roboya.Core;
using Roboya.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>Parent account sign-in with an e-mail code (F1-14 client). Hidden when no server is configured.</summary>
    public sealed class AccountSection : VisualElement, IRefreshable
    {
        private readonly GameServices _services;
        private readonly AccountService _account;
        private readonly VisualElement _signedOut = new VisualElement { name = "account-signed-out" };
        private readonly VisualElement _signedIn = new VisualElement { name = "account-signed-in" };
        private readonly TextField _email = new TextField { name = "account-email" };
        private readonly TextField _code = new TextField { name = "account-code", maxLength = 6 };
        private readonly Button _send;
        private readonly Button _verify;
        private readonly Label _who = new Label { name = "account-who" };
        private readonly Label _message = new Label { name = "account-message" };
        private bool _busy;

        public AccountSection(GameServices services)
        {
            _services = services;
            _account = services.Account;
            name = "account-section";
            AddToClassList("section");
            var s = services.Strings;
            var title = new Label(s.Get(StringKeys.AccountTitle));
            title.AddToClassList("section__title");
            Add(title);
            var hint = new Label(s.Get(StringKeys.AccountHint));
            hint.AddToClassList("report__line");
            Add(hint);

            _email.label = s.Get(StringKeys.AccountEmail);
            _email.keyboardType = TouchScreenKeyboardType.EmailAddress;
            _code.label = s.Get(StringKeys.AccountCode);
            _code.keyboardType = TouchScreenKeyboardType.NumberPad;
            _send = new Button(() => _ = SendCodeAsync()) { name = "account-send", text = s.Get(StringKeys.AccountSendCode) };
            _verify = new Button(() => _ = SignInAsync()) { name = "account-verify", text = s.Get(StringKeys.AccountSignIn) };
            foreach (var b in new[] { _send, _verify })
            {
                b.AddToClassList("editor__age");
            }

            _signedOut.Add(_email);
            _signedOut.Add(_send);
            _signedOut.Add(_code);
            _signedOut.Add(_verify);
            Add(_signedOut);

            _who.AddToClassList("report__line");
            _signedIn.Add(_who);
            var signOut = new Button(() => _ = _account.SignOutAsync()) { name = "account-sign-out", text = s.Get(StringKeys.AccountSignOut) };
            signOut.AddToClassList("editor__age");
            _signedIn.Add(signOut);
            Add(_signedIn);

            _message.AddToClassList("section__note");
            Add(_message);
            _account.Changed += Refresh;
            Refresh();
        }

        public void Refresh()
        {
            style.display = _account.IsConfigured ? DisplayStyle.Flex : DisplayStyle.None;
            _signedIn.style.display = _account.IsSignedIn ? DisplayStyle.Flex : DisplayStyle.None;
            _signedOut.style.display = _account.IsSignedIn ? DisplayStyle.None : DisplayStyle.Flex;
            _who.text = _services.Strings.Format(StringKeys.AccountSignedIn, _account.Email);
            if (_account.IsSignedIn)
            {
                _message.text = string.Empty;
            }
        }

        private async Task SendCodeAsync()
        {
            await Run(async () =>
            {
                await _account.RequestCodeAsync(_email.value);
                _message.text = _services.Strings.Get(StringKeys.AccountCodeSent);
            });
        }

        private async Task SignInAsync()
        {
            await Run(async () =>
            {
                await _account.SignInAsync(_email.value, _code.value);
                _code.value = string.Empty;
            });
        }

        private async Task Run(Func<Task> action)
        {
            if (_busy)
            {
                return;
            }

            _busy = true;
            _message.text = string.Empty;
            try
            {
                await action();
            }
            catch (ApiException e)
            {
                _message.text = _services.Strings.Get(ErrorKey(e));
            }
            finally
            {
                _busy = false;
            }
        }

        private static string ErrorKey(ApiException e)
        {
            switch (e.Code)
            {
                case "invalid_code": return StringKeys.AccountErrorCode;
                case "too_many_requests": return StringKeys.AccountErrorMany;
                case "device_limit": return StringKeys.AccountErrorDevice;
                case ApiException.NetworkCode: return StringKeys.AccountErrorNetwork;
                default: return StringKeys.AccountErrorGeneric;
            }
        }
    }
}
