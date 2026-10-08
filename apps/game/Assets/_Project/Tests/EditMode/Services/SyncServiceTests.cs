using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Roboya.CodingEngine.Profiles;
using Roboya.Core;
using Roboya.Services;

namespace Roboya.Tests.Services
{
    public class SyncServiceTests
    {
        private const string Tokens = "{\"access_token\":\"acc\",\"refresh_token\":\"ref\",\"expires_in\":1800,\"account_id\":\"a\"}";
        private const string Version = "v-test";

        private string _dir;
        private FakeTransport _http;
        private ProfileManager _profiles;
        private AccountService _account;
        private ApiClient _api;
        private readonly Dictionary<string, Dictionary<string, int>> _server = new Dictionary<string, Dictionary<string, int>>();
        private readonly List<string> _log = new List<string>();
        private string _failProfile;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "roboya-sync-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
            _server.Clear();
            _log.Clear();
            _failProfile = null;
            _http = new FakeTransport { Handler = Serve };
            _api = new ApiClient("http://api.test", _http);
            _profiles = ProfileManager.Load(_dir);
            _account = new AccountService(_api, _dir, "android");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_dir))
            {
                Directory.Delete(_dir, true);
            }
        }

        // A tiny in-memory server: the highest stars win, like the real one.
        private HttpResponse Serve(HttpRequest r)
        {
            string path = r.Url.Substring("http://api.test".Length);
            _log.Add(r.Method + " " + path);
            if (path == "/v1/auth/verify")
            {
                return new HttpResponse(200, Tokens);
            }

            if (path == "/v1/me/consents")
            {
                return new HttpResponse(204, null);
            }

            string[] parts = path.Split('/');
            string id = parts[4];
            if (r.Method == "PUT")
            {
                if (id == _failProfile)
                {
                    return new HttpResponse(409, "{\"code\":\"profile_limit\"}");
                }

                if (!_server.ContainsKey(id))
                {
                    _server[id] = new Dictionary<string, int>();
                }

                return new HttpResponse(200, "{}");
            }

            if (r.Method == "DELETE")
            {
                _server.Remove(id);
                return new HttpResponse(204, null);
            }

            var sent = Newtonsoft.Json.Linq.JObject.Parse(r.Body)["stars"];
            foreach (var p in ((Newtonsoft.Json.Linq.JObject)sent).Properties())
            {
                int v = (int)p.Value;
                _server[id][p.Name] = Math.Max(v, _server[id].TryGetValue(p.Name, out var old) ? old : 0);
            }

            return new HttpResponse(200, Newtonsoft.Json.JsonConvert.SerializeObject(new { stars = _server[id] }));
        }

        private SyncService Create() => new SyncService(_api, _account, _profiles, new LocalNotice(Version, true, "x"), _dir);

        private void SignIn() => Assert.IsTrue(_account.SignInAsync("a@b.co", "123456").Wait(2000));

        [Test]
        public void Sync_NoServerConfigured_IsOffline()
        {
            var sync = new SyncService(null, new AccountService(null, _dir, "android"), _profiles, new LocalNotice(Version, true, "x"), _dir);
            Assert.AreEqual(SyncResult.Offline, sync.SyncAsync().Result);
        }

        [Test]
        public void Sync_SignedOut_SendsNothing()
        {
            _profiles.RecordConsent(Version);
            Assert.AreEqual(SyncResult.NotSignedIn, Create().SyncAsync().Result);
            Assert.IsEmpty(_log);
        }

        [Test]
        public void Sync_NoLocalConsent_SendsNoChildData()
        {
            _profiles.Add("Elif", "robot-mavi", AgeBand.Minik);
            SignIn();
            _log.Clear();

            Assert.AreEqual(SyncResult.ConsentNeeded, Create().SyncAsync().Result);
            Assert.IsEmpty(_log, "UYM-01: nothing about a child is sent without consent");
        }

        [Test]
        public void Sync_PushesProfileAndStars_AndPullsTheHigherOnes()
        {
            _profiles.RecordConsent(Version);
            var elif = _profiles.Add("Elif", "robot-mavi", AgeBand.Kasif);
            _profiles.ProgressOf(elif.Id).Book.Record("sabir-ormani.yon-avcisi.01", 2);
            SignIn();
            _server[elif.Id] = new Dictionary<string, int> { { "sabir-ormani.yon-avcisi.02", 3 } };

            var result = Create().SyncAsync().Result;

            Assert.AreEqual(SyncResult.Done, result);
            var book = _profiles.ProgressOf(elif.Id).Book;
            Assert.AreEqual(2, book.Stars("sabir-ormani.yon-avcisi.01"));
            Assert.AreEqual(3, book.Stars("sabir-ormani.yon-avcisi.02"), "stars from another device arrive");
            Assert.AreEqual(2, _server[elif.Id]["sabir-ormani.yon-avcisi.01"]);
            CollectionAssert.Contains(_log, "PUT /v1/me/profiles/" + elif.Id);
        }

        [Test]
        public void Sync_ServerRefusesAProfile_ReportsPartlyDone_AndKeepsTheLocalOne()
        {
            _profiles.RecordConsent(Version);
            var a = _profiles.Add("A", "robot-mavi", AgeBand.Minik);
            var b = _profiles.Add("B", "robot-mor", AgeBand.Minik);
            _failProfile = b.Id;
            SignIn();

            Assert.AreEqual(SyncResult.PartlyDone, Create().SyncAsync().Result);
            Assert.IsNotNull(_profiles.Registry.Find(b.Id));
            Assert.IsTrue(_server.ContainsKey(a.Id));
        }

        [Test]
        public void Sync_ProfileRemovedLocally_IsRemovedOnTheServerNextTime()
        {
            _profiles.RecordConsent(Version);
            var a = _profiles.Add("A", "robot-mavi", AgeBand.Minik);
            var b = _profiles.Add("B", "robot-mor", AgeBand.Minik);
            SignIn();
            Assert.AreEqual(SyncResult.Done, Create().SyncAsync().Result);
            Assert.IsTrue(_server.ContainsKey(b.Id));

            _profiles.Remove(b.Id);
            Assert.AreEqual(SyncResult.Done, Create().SyncAsync().Result);

            Assert.IsFalse(_server.ContainsKey(b.Id));
            Assert.IsTrue(_server.ContainsKey(a.Id));
        }

        [Test]
        public void Sync_ServerUnreachable_IsOffline_AndLocalDataIsUntouched()
        {
            _profiles.RecordConsent(Version);
            var a = _profiles.Add("A", "robot-mavi", AgeBand.Minik);
            _profiles.ProgressOf(a.Id).Book.Record("sabir-ormani.yon-avcisi.01", 3);
            SignIn();
            _http.Handler = _ => new HttpResponse(0, null);

            Assert.AreEqual(SyncResult.Offline, Create().SyncAsync().Result);
            Assert.AreEqual(3, _profiles.ProgressOf(a.Id).Book.Stars("sabir-ormani.yon-avcisi.01"));
        }
    }
}
