using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Roboya.CodingEngine.Profiles;
using Roboya.Core;
using Roboya.Services;

namespace Roboya.Tests.Services
{
    public class PrivacyServiceTests
    {
        private const string Tokens = "{\"access_token\":\"acc\",\"refresh_token\":\"ref\",\"expires_in\":1800,\"account_id\":\"a\"}";
        private const string Version = "v-test";

        private string _dir;
        private FakeTransport _http;
        private ProfileManager _profiles;
        private AccountService _account;
        private PrivacyService _privacy;
        private SyncService _sync;
        private readonly List<string> _log = new List<string>();
        private bool _offline;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "roboya-priv-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
            _log.Clear();
            _offline = false;
            _http = new FakeTransport { Handler = Serve };
            var api = new ApiClient("http://api.test", _http);
            _profiles = ProfileManager.Load(_dir);
            _account = new AccountService(api, _dir, "android");
            _sync = new SyncService(api, _account, _profiles, new LocalNotice(Version, true, "x"), _dir);
            _privacy = new PrivacyService(api, _account, _profiles, _sync, _dir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_dir))
            {
                Directory.Delete(_dir, true);
            }
        }

        private HttpResponse Serve(HttpRequest r)
        {
            string path = r.Url.Substring("http://api.test".Length);
            if (_offline && path != "/v1/auth/verify")
            {
                return new HttpResponse(0, null);
            }

            _log.Add(r.Method + " " + path);
            switch (r.Method + " " + path)
            {
                case "POST /v1/auth/verify":
                    return new HttpResponse(200, Tokens);
                case "GET /v1/me/data":
                    return new HttpResponse(200, "{\"account\":{\"email\":\"a@b.co\"},\"consents\":[{}],\"devices\":[{},{}],\"profiles\":[{\"nickname\":\"Elif\"}],\"subscriptions\":[],\"deletion_request\":null}");
                case "GET /v1/me/data/export":
                    return new HttpResponse(200, "{\"export\":true}");
                case "POST /v1/me/deletion-request":
                    return new HttpResponse(202, "{\"requested_at\":\"2026-10-09T08:00:00Z\",\"scheduled_for\":\"2026-10-16T08:00:00Z\"}");
                case "GET /v1/me/deletion-request":
                    return new HttpResponse(404, "{\"code\":\"not_found\"}");
                default:
                    return new HttpResponse(r.Method == "GET" ? 200 : 204, "{}");
            }
        }

        private void SignIn() => Assert.IsTrue(_account.SignInAsync("a@b.co", "123456").Wait(2000));

        [Test]
        public void GetMyData_SummarisesWhatTheServerHolds()
        {
            SignIn();
            var data = _privacy.GetMyDataAsync().Result;
            Assert.AreEqual("a@b.co", data.Email);
            Assert.AreEqual(1, data.Consents);
            Assert.AreEqual(2, data.Devices);
            CollectionAssert.AreEqual(new[] { "Elif" }, data.ProfileNicknames);
            Assert.IsNull(data.DeletionScheduledUtc);
        }

        [Test]
        public void Export_SavesTheFileInTheAppFolder()
        {
            SignIn();
            string path = _privacy.ExportAsync().Result;
            Assert.AreEqual(Path.Combine(_dir, PrivacyService.ExportFile), path);
            Assert.AreEqual("{\"export\":true}", File.ReadAllText(path));
        }

        [Test]
        public void Deletion_RequestReturnsTheDate_AndNoneScheduledIsNull()
        {
            SignIn();
            Assert.IsNull(_privacy.DeletionAsync().Result);
            Assert.AreEqual(new DateTime(2026, 10, 16, 8, 0, 0, DateTimeKind.Utc), _privacy.RequestDeletionAsync().Result);
        }

        [Test]
        public void ServerCalls_SignedOut_FailWithoutANetworkCall()
        {
            var e = Assert.Throws<AggregateException>(() => _privacy.GetMyDataAsync().Wait(2000));
            Assert.AreEqual("invalid_token", ((ApiException)e.InnerException).Code);
            Assert.IsEmpty(_log);
        }

        [Test]
        public void Withdraw_RemovesConsentHere_AndOnTheServerWhenSignedIn()
        {
            _profiles.RecordConsent(Version);
            SignIn();

            _privacy.WithdrawConsentAsync().Wait();

            Assert.IsFalse(_profiles.Registry.HasConsentFor(Version));
            CollectionAssert.Contains(_log, "DELETE /v1/me/consents");
        }

        [Test]
        public void Withdraw_ServerUnreachable_StillWithdrawsHere_AndRetriesLater()
        {
            _profiles.RecordConsent(Version);
            SignIn();
            _offline = true;

            _privacy.WithdrawConsentAsync().Wait();

            Assert.IsFalse(_profiles.Registry.HasConsentFor(Version), "nothing more is sent from this device");
            Assert.IsTrue(File.Exists(Path.Combine(_dir, PrivacyService.PendingFile)));

            _offline = false;
            _privacy.FlushPendingAsync().Wait();

            CollectionAssert.Contains(_log, "DELETE /v1/me/consents");
            Assert.IsFalse(File.Exists(Path.Combine(_dir, PrivacyService.PendingFile)));
        }

        [Test]
        public void Wipe_RemovesProfilesAndProgress_TellsTheServer_AndWithdraws()
        {
            _profiles.RecordConsent(Version);
            var a = _profiles.Add("A", "robot-mavi", AgeBand.Minik);
            _profiles.ProgressOf(a.Id).Book.Record("sabir-ormani.yon-avcisi.01", 2);
            _profiles.ProgressOf(a.Id).Save();
            SignIn();
            _sync.SyncAsync().Wait();
            _log.Clear();

            _privacy.WipeDeviceAsync().Wait();

            Assert.AreEqual(0, _profiles.Registry.Profiles.Count);
            Assert.IsFalse(File.Exists(Path.Combine(_dir, a.Id + ".json")), "progress file is gone");
            CollectionAssert.Contains(_log, "DELETE /v1/me/profiles/" + a.Id);
            CollectionAssert.Contains(_log, "DELETE /v1/me/consents");
            Assert.IsFalse(_profiles.Registry.HasConsentFor(Version));
        }
    }
}
