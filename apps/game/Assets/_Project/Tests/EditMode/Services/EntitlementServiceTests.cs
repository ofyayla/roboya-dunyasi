using System;
using System.IO;
using NUnit.Framework;
using Roboya.Core;
using Roboya.Services;

namespace Roboya.Tests.Services
{
    public class EntitlementServiceTests
    {
        private const string Tokens = "{\"access_token\":\"acc\",\"refresh_token\":\"ref\",\"expires_in\":1800,\"account_id\":\"a\"}";

        private string _dir;
        private DateTime _now;
        private FakeTransport _http;
        private ApiClient _api;
        private AccountService _account;
        private string _entitlement;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "roboya-ent-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
            _now = new DateTime(2026, 10, 9, 8, 0, 0, DateTimeKind.Utc);
            _entitlement = Premium(_now.AddDays(30), _now.AddHours(72));
            _http = new FakeTransport
            {
                Handler = r => r.Url.EndsWith("/v1/auth/verify")
                    ? new HttpResponse(200, Tokens)
                    : new HttpResponse(200, _entitlement),
            };
            _api = new ApiClient("http://api.test", _http);
            _account = new AccountService(_api, _dir, "android", () => _now);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_dir))
            {
                Directory.Delete(_dir, true);
            }
        }

        private static string Premium(DateTime expires, DateTime cacheUntil) =>
            "{\"tier\":\"premium\",\"source\":\"store\",\"status\":\"active\",\"expires_at\":\"" + expires.ToString("o") + "\",\"cache_until\":\"" + cacheUntil.ToString("o") + "\"}";

        private EntitlementService Create() => new EntitlementService(_api, _account, _dir, () => _now);

        private void SignIn() => Assert.IsTrue(_account.SignInAsync("a@b.co", "123456").Wait(2000));

        [Test]
        public void New_IsFree()
        {
            Assert.IsFalse(Create().HasPremium);
            Assert.AreEqual("free", Create().Status);
        }

        [Test]
        public void Refresh_SignedOut_AsksNothing()
        {
            var e = Create();
            Assert.IsFalse(e.RefreshAsync().Result);
            Assert.IsEmpty(_http.Sent);
        }

        [Test]
        public void Refresh_ServerSaysPremium_HasPremiumUntilTheCacheEnds()
        {
            SignIn();
            var e = Create();

            Assert.IsTrue(e.RefreshAsync().Result);
            Assert.IsTrue(e.HasPremium);
            Assert.AreEqual("active", e.Status);

            _now = _now.AddHours(71);
            Assert.IsTrue(e.HasPremium, "offline use inside the cache window");
            _now = _now.AddHours(2);
            Assert.IsFalse(e.HasPremium, "past cache_until the client no longer trusts it");
        }

        [Test]
        public void Refresh_Cache_SurvivesARestart()
        {
            SignIn();
            Create().RefreshAsync().Wait();
            Assert.IsTrue(Create().HasPremium);
        }

        [Test]
        public void Refresh_ServerSaysFree_RemovesPremium()
        {
            SignIn();
            var e = Create();
            e.RefreshAsync().Wait();
            _entitlement = "{\"tier\":\"free\",\"source\":\"none\",\"status\":\"free\",\"expires_at\":null,\"cache_until\":\"" + _now.AddHours(72).ToString("o") + "\"}";

            e.RefreshAsync().Wait();

            Assert.IsFalse(e.HasPremium);
        }

        [Test]
        public void Refresh_Offline_KeepsTheOldAnswer()
        {
            SignIn();
            var e = Create();
            e.RefreshAsync().Wait();
            _http.Handler = _ => new HttpResponse(0, null);

            Assert.IsFalse(e.RefreshAsync().Result);
            Assert.IsTrue(e.HasPremium);
        }

        [Test]
        public void SignOut_DropsThePremiumCache()
        {
            SignIn();
            var e = Create();
            e.RefreshAsync().Wait();

            _account.SignOutAsync().Wait();

            Assert.IsFalse(e.HasPremium);
            Assert.IsFalse(File.Exists(Path.Combine(_dir, EntitlementService.File)));
        }

        [Test]
        public void Load_DamagedFile_StartsFree()
        {
            File.WriteAllText(Path.Combine(_dir, EntitlementService.File), "{{ nope");
            Assert.IsFalse(Create().HasPremium);
        }

        [Test]
        public void Load_TamperedFileWithPastCacheEnd_DoesNotGrantPremium()
        {
            File.WriteAllText(Path.Combine(_dir, EntitlementService.File),
                "{\"Tier\":\"premium\",\"Status\":\"active\",\"CacheUntilUtc\":\"2020-01-01T00:00:00Z\"}");
            Assert.IsFalse(Create().HasPremium);
        }
    }
}
