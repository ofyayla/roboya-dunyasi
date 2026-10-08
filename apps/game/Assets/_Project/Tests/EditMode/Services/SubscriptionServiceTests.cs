using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Roboya.Core;
using Roboya.Services;

namespace Roboya.Tests.Services
{
    public class SubscriptionServiceTests
    {
        private const string Tokens = "{\"access_token\":\"acc\",\"refresh_token\":\"ref\",\"expires_in\":1800,\"account_id\":\"11111111-2222-3333-4444-555555555555\"}";

        private string _dir;
        private DateTime _now;
        private FakeTransport _http;
        private FakeStoreBridge _store;
        private AccountService _account;
        private EntitlementService _entitlement;
        private SubscriptionService _sub;
        private readonly List<string> _log = new List<string>();
        private HttpResponse _receiptReply;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "roboya-sub-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
            _now = new DateTime(2026, 10, 9, 8, 0, 0, DateTimeKind.Utc);
            _log.Clear();
            _receiptReply = new HttpResponse(200, Premium());
            _http = new FakeTransport { Handler = Serve };
            var api = new ApiClient("http://api.test", _http);
            _account = new AccountService(api, _dir, "android", () => _now);
            _entitlement = new EntitlementService(api, _account, _dir, () => _now);
            _store = new FakeStoreBridge();
            _store.Products.Add(new StoreProduct("m", "₺99,99"));
            _store.Products.Add(new StoreProduct("y", "₺1.019,90"));
            _sub = new SubscriptionService(api, _account, _store, _entitlement, new StoreProducts { Monthly = "m", Yearly = "y", YearlyTrialDays = 7 });
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_dir))
            {
                Directory.Delete(_dir, true);
            }
        }

        private string Premium() =>
            "{\"tier\":\"premium\",\"source\":\"store\",\"status\":\"active\",\"expires_at\":\"" + _now.AddDays(30).ToString("o") + "\",\"cache_until\":\"" + _now.AddHours(72).ToString("o") + "\"}";

        private HttpResponse Serve(HttpRequest r)
        {
            _log.Add(r.Method + " " + r.Url.Substring("http://api.test".Length));
            return r.Url.EndsWith("/v1/auth/verify") ? new HttpResponse(200, Tokens) : _receiptReply;
        }

        private void SignIn() => Assert.IsTrue(_account.SignInAsync("a@b.co", "123456").Wait(2000));

        [Test]
        public void LoadProducts_ReturnsTheStorePrices()
        {
            var products = _sub.LoadProductsAsync().Result;
            Assert.AreEqual(2, products.Count);
            Assert.AreEqual("₺99,99", products[0].PriceText);
        }

        [Test]
        public void Purchase_SignedOut_NeedsTheParentAccount_AndNeverTouchesTheStore()
        {
            Assert.AreEqual(SubscriptionResult.NeedsAccount, _sub.PurchaseAsync("m").Result);
            Assert.IsNull(_store.LastAccountToken);
        }

        [Test]
        public void Purchase_NoNativeBridge_IsUnavailable()
        {
            var api = new ApiClient("http://api.test", _http);
            var sub = new SubscriptionService(api, _account, new NoStoreBridge(), _entitlement, new StoreProducts());
            Assert.AreEqual(SubscriptionResult.StoreUnavailable, sub.PurchaseAsync("m").Result);
        }

        [Test]
        public void Purchase_Success_SendsProofToTheServer_CachesTheGrant_ThenFinishes()
        {
            SignIn();

            var result = _sub.PurchaseAsync("m").Result;

            Assert.AreEqual(SubscriptionResult.Done, result);
            CollectionAssert.Contains(_log, "POST /v1/store/receipts");
            StringAssert.Contains("token-0000000001", _http.Sent[1].Body);
            StringAssert.Contains("\"store\":\"google\"", _http.Sent[1].Body);
            Assert.IsTrue(_entitlement.HasPremium);
            Assert.AreEqual(1, _store.Finished.Count, "finished only after the server accepted it");
            Assert.AreNotEqual("11111111-2222-3333-4444-555555555555", _store.LastAccountToken, "the store never sees the account id");
        }

        [Test]
        public void Purchase_Cancelled_ChangesNothing()
        {
            SignIn();
            _store.NextPurchase = new PurchaseResult(PurchaseOutcome.Cancelled);

            Assert.AreEqual(SubscriptionResult.Cancelled, _sub.PurchaseAsync("m").Result);
            Assert.IsFalse(_entitlement.HasPremium);
            Assert.AreEqual(1, _http.Sent.Count, "only the sign-in call");
        }

        [Test]
        public void Purchase_Pending_WaitsForApproval_ThenALaterUpdateLinksIt()
        {
            SignIn();
            _store.NextPurchase = new PurchaseResult(PurchaseOutcome.Pending);
            Assert.AreEqual(SubscriptionResult.Pending, _sub.PurchaseAsync("m").Result);
            Assert.IsFalse(_entitlement.HasPremium);

            _store.RaiseUpdated(new PurchaseProof("google", "token-approved-0001"));

            Assert.IsTrue(_entitlement.HasPremium);
        }

        [Test]
        public void Purchase_ServerUnreachable_KeepsTheStorePurchaseUnfinished_AndRecoveryLinksIt()
        {
            SignIn();
            _receiptReply = new HttpResponse(0, null);

            Assert.AreEqual(SubscriptionResult.Offline, _sub.PurchaseAsync("m").Result);
            Assert.AreEqual(0, _store.Finished.Count, "a paid purchase is never lost");
            Assert.IsFalse(_entitlement.HasPremium);

            _receiptReply = new HttpResponse(200, Premium());
            _sub.RecoverAsync().Wait();

            Assert.IsTrue(_entitlement.HasPremium);
            Assert.AreEqual(1, _store.Finished.Count);
        }

        [Test]
        public void Purchase_AlreadyLinkedToAnotherAccount_IsReported()
        {
            SignIn();
            _receiptReply = new HttpResponse(409, "{\"code\":\"purchase_linked\"}");

            Assert.AreEqual(SubscriptionResult.LinkedElsewhere, _sub.PurchaseAsync("m").Result);
            Assert.IsFalse(_entitlement.HasPremium);
        }

        [Test]
        public void Purchase_ServerRejectsTheProof_NoPremium_AndNotFinished()
        {
            SignIn();
            _receiptReply = new HttpResponse(401, "{\"code\":\"invalid_signature\"}");

            Assert.AreEqual(SubscriptionResult.Failed, _sub.PurchaseAsync("m").Result);
            Assert.IsFalse(_entitlement.HasPremium);
            Assert.AreEqual(0, _store.Finished.Count);
        }

        [Test]
        public void Restore_LinksEverythingTheStoreAccountOwns()
        {
            SignIn();
            _store.Owned.Add(new PurchaseProof("apple", "jws-transaction-1"));

            Assert.AreEqual(SubscriptionResult.Done, _sub.RestoreAsync().Result);
            Assert.IsTrue(_entitlement.HasPremium);
        }

        [Test]
        public void AccountToken_IsStable_AndDiffersPerAccount()
        {
            Assert.AreEqual(SubscriptionService.AccountToken("a"), SubscriptionService.AccountToken("a"));
            Assert.AreNotEqual(SubscriptionService.AccountToken("a"), SubscriptionService.AccountToken("b"));
            Assert.IsTrue(Guid.TryParse(SubscriptionService.AccountToken("a"), out _));
        }
    }
}
