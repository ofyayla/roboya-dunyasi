using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Roboya.Services;

namespace Roboya.Core
{
    public enum SubscriptionResult
    {
        Done,
        Cancelled,

        /// <summary>The store is waiting for approval; access starts when it arrives.</summary>
        Pending,

        /// <summary>A purchase needs the parent account (GLR-03).</summary>
        NeedsAccount,
        StoreUnavailable,

        /// <summary>This store purchase already belongs to another parent account.</summary>
        LinkedElsewhere,

        /// <summary>The store took the purchase but the server could not be reached; it is linked on the next start.</summary>
        Offline,
        Failed,
    }

    /// <summary>Product ids and the trial length, from content/store/products.json (no prices).</summary>
    public sealed class StoreProducts
    {
        public const string File = "store/products.json";

        [JsonProperty("monthly")]
        public string Monthly { get; set; }

        [JsonProperty("yearly")]
        public string Yearly { get; set; }

        [JsonProperty("yearlyTrialDays")]
        public int YearlyTrialDays { get; set; }

        public static StoreProducts Parse(string json) => JsonConvert.DeserializeObject<StoreProducts>(json);
    }

    /// <summary>
    /// Buying Aile Premium (F1-16, ADR 0008/0013): the native bridge buys, the proof goes to the server, and what the server
    /// grants is cached by <see cref="EntitlementService"/>. The client never grants premium itself. A purchase is finished
    /// with the store only after the server accepted it, so a crash or lost connection cannot lose a paid purchase.
    /// </summary>
    public sealed class SubscriptionService
    {
        private readonly ApiClient _api;
        private readonly AccountService _account;
        private readonly IStoreBridge _bridge;
        private readonly EntitlementService _entitlements;

        public SubscriptionService(
            ApiClient api, AccountService account, IStoreBridge bridge, EntitlementService entitlements, StoreProducts products)
        {
            _api = api;
            _account = account;
            _bridge = bridge;
            _entitlements = entitlements;
            Products = products;
            _bridge.TransactionUpdated += proof => _ = LinkAsync(proof);
        }

        public StoreProducts Products { get; }

        public bool StoreAvailable => _bridge.IsAvailable;

        public Task<IReadOnlyList<StoreProduct>> LoadProductsAsync() =>
            _bridge.GetProductsAsync(new List<string> { Products.Monthly, Products.Yearly });

        public async Task<SubscriptionResult> PurchaseAsync(string productId)
        {
            if (!_bridge.IsAvailable || _api == null)
            {
                return SubscriptionResult.StoreUnavailable;
            }

            if (!_account.IsSignedIn)
            {
                return SubscriptionResult.NeedsAccount;
            }

            var result = await _bridge.PurchaseAsync(productId, AccountToken(_account.AccountId));
            switch (result.Outcome)
            {
                case PurchaseOutcome.Cancelled: return SubscriptionResult.Cancelled;
                case PurchaseOutcome.Pending: return SubscriptionResult.Pending;
                case PurchaseOutcome.Failed: return SubscriptionResult.Failed;
                default: return await LinkAsync(result.Proof);
            }
        }

        /// <summary>"Restore purchases": links whatever the store account owns to this parent account (idempotent).</summary>
        public async Task<SubscriptionResult> RestoreAsync()
        {
            if (!_bridge.IsAvailable || _api == null)
            {
                return SubscriptionResult.StoreUnavailable;
            }

            if (!_account.IsSignedIn)
            {
                return SubscriptionResult.NeedsAccount;
            }

            var owned = await _bridge.RestoreAsync();
            var last = SubscriptionResult.Done;
            foreach (var proof in owned)
            {
                last = await LinkAsync(proof);
                if (last == SubscriptionResult.Offline)
                {
                    break;
                }
            }

            return last;
        }

        /// <summary>At start-up: finish what a previous run could not (paid in the store, not yet linked).</summary>
        public async Task RecoverAsync()
        {
            if (_bridge.IsAvailable && _account.IsSignedIn)
            {
                await RestoreAsync();
            }
        }

        private async Task<SubscriptionResult> LinkAsync(PurchaseProof proof)
        {
            if (_api == null || !_account.IsSignedIn)
            {
                return SubscriptionResult.NeedsAccount;
            }

            try
            {
                string token = await _account.GetAccessTokenAsync();
                if (token == null)
                {
                    return SubscriptionResult.Offline;
                }

                var reply = await _api.SendAsync<EntitlementService.Reply>(
                    "POST", "/v1/store/receipts", new { store = proof.Store, proof = proof.Proof }, token);
                _entitlements.Adopt(reply);
                await _bridge.FinishAsync(proof);
                return SubscriptionResult.Done;
            }
            catch (ApiException e) when (e.IsNetwork)
            {
                return SubscriptionResult.Offline;
            }
            catch (ApiException e) when (e.Code == "purchase_linked")
            {
                return SubscriptionResult.LinkedElsewhere;
            }
            catch (ApiException)
            {
                return SubscriptionResult.Failed;
            }
        }

        /// <summary>A stable random-looking id for the store (appAccountToken / obfuscatedAccountId); not the account id, no personal data.</summary>
        public static string AccountToken(string accountId)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes("roboya-store:" + accountId));
                var bytes = new byte[16];
                Array.Copy(hash, bytes, 16);
                return new Guid(bytes).ToString();
            }
        }
    }
}
