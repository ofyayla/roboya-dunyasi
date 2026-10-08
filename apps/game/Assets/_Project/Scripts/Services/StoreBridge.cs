using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roboya.Services
{
    public sealed class StoreProduct
    {
        public StoreProduct(string id, string priceText)
        {
            Id = id;
            PriceText = priceText;
        }

        public string Id { get; }

        /// <summary>The store's own, localized price text. Prices are never kept in code (CLAUDE.md §6, §12).</summary>
        public string PriceText { get; }
    }

    /// <summary>What the store gave for a purchase; the server checks it (ADR 0013). Apple: signed transaction, Google: purchase token.</summary>
    public sealed class PurchaseProof
    {
        public PurchaseProof(string store, string proof)
        {
            Store = store;
            Proof = proof;
        }

        /// <summary>"apple" or "google", as the server expects.</summary>
        public string Store { get; }

        public string Proof { get; }
    }

    public enum PurchaseOutcome
    {
        Purchased,
        Cancelled,

        /// <summary>Waiting for approval (e.g. "ask to buy"); the proof arrives later through <see cref="IStoreBridge.TransactionUpdated"/>.</summary>
        Pending,
        Failed,
    }

    public sealed class PurchaseResult
    {
        public PurchaseResult(PurchaseOutcome outcome, PurchaseProof proof = null)
        {
            Outcome = outcome;
            Proof = proof;
        }

        public PurchaseOutcome Outcome { get; }

        public PurchaseProof Proof { get; }
    }

    /// <summary>
    /// The thin native store layer (StoreKit 2 / Play Billing, ADR 0008). The game never decides what a purchase is worth:
    /// it passes the proof to the server and shows what the server grants.
    /// </summary>
    public interface IStoreBridge
    {
        /// <summary>False on builds without the native bridge (e.g. the editor or a device build before the store accounts exist).</summary>
        bool IsAvailable { get; }

        Task<IReadOnlyList<StoreProduct>> GetProductsAsync(IReadOnlyList<string> productIds);

        /// <param name="accountToken">Random id derived from the parent account (appAccountToken / obfuscatedAccountId); no personal data.</param>
        Task<PurchaseResult> PurchaseAsync(string productId, string accountToken);

        /// <summary>Current purchases of the store account, for "restore purchases" and for finishing unfinished transactions.</summary>
        Task<IReadOnlyList<PurchaseProof>> RestoreAsync();

        /// <summary>Tells the store the server has accepted the purchase.</summary>
        Task FinishAsync(PurchaseProof proof);

        /// <summary>A purchase changed outside a user action: renewal, approval of a pending purchase, family sharing.</summary>
        event Action<PurchaseProof> TransactionUpdated;
    }

    /// <summary>Used until a native bridge exists: nothing can be bought, and the screen says so.</summary>
    public sealed class NoStoreBridge : IStoreBridge
    {
        public event Action<PurchaseProof> TransactionUpdated
        {
            add { }
            remove { }
        }

        public bool IsAvailable => false;

        public Task<IReadOnlyList<StoreProduct>> GetProductsAsync(IReadOnlyList<string> productIds) =>
            Task.FromResult<IReadOnlyList<StoreProduct>>(new List<StoreProduct>());

        public Task<PurchaseResult> PurchaseAsync(string productId, string accountToken) =>
            Task.FromResult(new PurchaseResult(PurchaseOutcome.Failed));

        public Task<IReadOnlyList<PurchaseProof>> RestoreAsync() =>
            Task.FromResult<IReadOnlyList<PurchaseProof>>(new List<PurchaseProof>());

        public Task FinishAsync(PurchaseProof proof) => Task.CompletedTask;
    }

    /// <summary>A scriptable store for tests and the editor: purchase, cancel, pending, restore and renewal flows.</summary>
    public sealed class FakeStoreBridge : IStoreBridge
    {
        public List<StoreProduct> Products { get; } = new List<StoreProduct>();

        public PurchaseResult NextPurchase { get; set; } = new PurchaseResult(PurchaseOutcome.Purchased, new PurchaseProof("google", "token-0000000001"));

        public List<PurchaseProof> Owned { get; } = new List<PurchaseProof>();

        public List<PurchaseProof> Finished { get; } = new List<PurchaseProof>();

        public string LastAccountToken { get; private set; }

        public event Action<PurchaseProof> TransactionUpdated;

        public bool IsAvailable => true;

        public Task<IReadOnlyList<StoreProduct>> GetProductsAsync(IReadOnlyList<string> productIds) =>
            Task.FromResult<IReadOnlyList<StoreProduct>>(Products.FindAll(p => new List<string>(productIds).Contains(p.Id)));

        public Task<PurchaseResult> PurchaseAsync(string productId, string accountToken)
        {
            LastAccountToken = accountToken;
            if (NextPurchase.Outcome == PurchaseOutcome.Purchased)
            {
                Owned.Add(NextPurchase.Proof);
            }

            return Task.FromResult(NextPurchase);
        }

        public Task<IReadOnlyList<PurchaseProof>> RestoreAsync() =>
            Task.FromResult<IReadOnlyList<PurchaseProof>>(new List<PurchaseProof>(Owned));

        public Task FinishAsync(PurchaseProof proof)
        {
            Finished.Add(proof);
            return Task.CompletedTask;
        }

        /// <summary>Simulates a renewal or an approved "ask to buy" arriving on its own.</summary>
        public void RaiseUpdated(PurchaseProof proof) => TransactionUpdated?.Invoke(proof);
    }
}
