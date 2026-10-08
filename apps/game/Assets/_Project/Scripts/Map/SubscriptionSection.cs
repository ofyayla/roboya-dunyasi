using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Roboya.Core;
using Roboya.Services;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>
    /// Aile Premium in the parent area (F1-16, GLR-01/02): status, store prices, the plain terms, buy and restore.
    /// Only reachable behind the parental gate. Prices come from the store, never from code.
    /// </summary>
    public sealed class SubscriptionSection : VisualElement, IRefreshable, IOpened
    {
        private readonly GameServices _services;
        private readonly Label _status = new Label { name = "subscription-status" };
        private readonly Label _message = new Label { name = "subscription-message" };
        private readonly VisualElement _buy = new VisualElement { name = "subscription-buy" };
        private readonly Button _monthly;
        private readonly Button _yearly;
        private readonly Label _trial = new Label { name = "subscription-trial" };
        private bool _busy;

        public SubscriptionSection(GameServices services)
        {
            _services = services;
            name = "subscription-section";
            AddToClassList("section");
            var s = services.Strings;
            var title = new Label(s.Get(StringKeys.SubscriptionTitle));
            title.AddToClassList("section__title");
            Add(title);
            _status.AddToClassList("report__line");
            Add(_status);

            _monthly = new Button(() => _ = BuyAsync(services.Subscription.Products.Monthly)) { name = "subscription-monthly" };
            _yearly = new Button(() => _ = BuyAsync(services.Subscription.Products.Yearly)) { name = "subscription-yearly" };
            var restore = new Button(() => _ = RunAsync(RestoreAsync)) { name = "subscription-restore", text = s.Get(StringKeys.SubscriptionRestore) };
            foreach (var b in new[] { _monthly, _yearly, restore })
            {
                b.AddToClassList("editor__age");
                b.style.marginTop = 6;
            }

            _buy.Add(_monthly);
            _buy.Add(_yearly);
            _trial.AddToClassList("report__line");
            _buy.Add(_trial);
            _buy.Add(restore);
            Add(_buy);

            var terms = new Label(s.Get(StringKeys.SubscriptionTerms)) { name = "subscription-terms" };
            terms.AddToClassList("report__line");
            Add(terms);
            _message.AddToClassList("section__note");
            Add(_message);
            services.Account.Changed += Refresh;
            Refresh();
        }

        public void OnOpened()
        {
            if (!_services.Entitlements.HasPremium)
            {
                _services.Analytics.Track("paywall_view", null, AnalyticsService.Props("source", "parent"));
            }
        }

        public void Refresh()
        {
            var s = _services.Strings;
            if (_services.Entitlements is EntitlementService e && e.HasPremium)
            {
                string until = e.ExpiresUtc.HasValue ? e.ExpiresUtc.Value.ToLocalTime().ToString("d MMMM yyyy") : string.Empty;
                _status.text = e.Status == "grace"
                    ? s.Get(StringKeys.SubscriptionStatusGrace)
                    : s.Format(e.Status == "trial" ? StringKeys.SubscriptionStatusTrial : StringKeys.SubscriptionStatusActive, until);
                _buy.style.display = DisplayStyle.None;
                return;
            }

            _status.text = s.Format(StringKeys.SubscriptionStatusFree, _services.ProgressRules.FreeLevelCount);
            _buy.style.display = DisplayStyle.Flex;
            _monthly.style.display = DisplayStyle.None;
            _yearly.style.display = DisplayStyle.None;
            _trial.text = string.Empty;
            if (!_services.Subscription.StoreAvailable)
            {
                _message.text = s.Get(StringKeys.SubscriptionStoreMissing);
                _buy.style.display = DisplayStyle.None;
                return;
            }

            _message.text = _services.Account.IsSignedIn ? string.Empty : s.Get(StringKeys.SubscriptionNeedAccount);
            _ = ShowPricesAsync();
        }

        private async Task ShowPricesAsync()
        {
            var s = _services.Strings;
            var products = await _services.Subscription.LoadProductsAsync();
            foreach (var p in products)
            {
                if (p.Id == _services.Subscription.Products.Monthly)
                {
                    _monthly.text = s.Format(StringKeys.SubscriptionMonthly, p.PriceText);
                    _monthly.style.display = DisplayStyle.Flex;
                }
                else if (p.Id == _services.Subscription.Products.Yearly)
                {
                    _yearly.text = s.Format(StringKeys.SubscriptionYearly, p.PriceText);
                    _yearly.style.display = DisplayStyle.Flex;
                    _trial.text = s.Format(StringKeys.SubscriptionTrialNote, _services.Subscription.Products.YearlyTrialDays);
                }
            }
        }

        private async Task BuyAsync(string productId) => await RunAsync(async () => Show(await _services.Subscription.PurchaseAsync(productId)));

        private async Task RestoreAsync() => Show(await _services.Subscription.RestoreAsync());

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
            finally
            {
                _busy = false;
            }

            Refresh();
        }

        private void Show(SubscriptionResult result)
        {
            var s = _services.Strings;
            switch (result)
            {
                case SubscriptionResult.Done: _message.text = s.Get(StringKeys.SubscriptionDone); break;
                case SubscriptionResult.Cancelled: _message.text = s.Get(StringKeys.SubscriptionCancelled); break;
                case SubscriptionResult.Pending: _message.text = s.Get(StringKeys.SubscriptionPending); break;
                case SubscriptionResult.NeedsAccount: _message.text = s.Get(StringKeys.SubscriptionNeedAccount); break;
                case SubscriptionResult.StoreUnavailable: _message.text = s.Get(StringKeys.SubscriptionStoreMissing); break;
                case SubscriptionResult.LinkedElsewhere: _message.text = s.Get(StringKeys.SubscriptionLinked); break;
                case SubscriptionResult.Offline: _message.text = s.Get(StringKeys.SubscriptionOffline); break;
                default: _message.text = s.Get(StringKeys.SubscriptionFailed); break;
            }
        }
    }
}
