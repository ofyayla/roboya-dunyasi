using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Roboya.CodingEngine.Profiles;
using Roboya.Core;
using Roboya.Services;

namespace Roboya.Tests.Services
{
    public class AnalyticsServiceTests
    {
        private const string Version = "v-test";

        private string _dir;
        private FakeTransport _http;
        private ProfileManager _profiles;
        private ApiClient _api;
        private ChildProfileData _child;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "roboya-events-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
            _http = new FakeTransport { Handler = _ => new HttpResponse(202, "{\"accepted\":1,\"dropped\":0}") };
            _api = new ApiClient("http://api.test", _http);
            _profiles = ProfileManager.Load(_dir);
            _child = _profiles.Add("Elif", "robot-mavi", AgeBand.Minik);
            _profiles.RecordConsent(Version);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_dir))
            {
                Directory.Delete(_dir, true);
            }
        }

        private AnalyticsService Create(ApiClient api = null, bool noApi = false) =>
            new AnalyticsService(noApi ? null : api ?? _api, _profiles, new LocalNotice(Version, true, "x"), _dir, "0.1.0", "android");

        [Test]
        public void Track_NoServerConfigured_RecordsNothing()
        {
            var a = Create(noApi: true);
            a.Track("app_open");
            Assert.AreEqual(0, a.QueuedCount);
        }

        [Test]
        public void Track_WithoutConsent_RecordsNothing()
        {
            _profiles.WithdrawConsent();
            var a = Create();
            a.Track("app_open");
            Assert.AreEqual(0, a.QueuedCount);
        }

        [Test]
        public void Track_UnknownNameOrProperty_IsRejected()
        {
            var a = Create();
            Assert.Throws<ArgumentException>(() => a.Track("purchase"));
            Assert.Throws<ArgumentException>(() => a.Track("app_open", null, AnalyticsService.Props("nickname", "Elif")));
            Assert.Throws<ArgumentException>(() => a.Track("level_start", null, AnalyticsService.Props("level_band", "baby")));
            Assert.Throws<ArgumentException>(() => a.Track("level_complete", null, AnalyticsService.Props("stars", "3")));
        }

        [Test]
        public void Flush_SendsOneBatchWithAnAnonymousIdThatIsNotTheProfileId()
        {
            var a = Create();
            a.Track("level_start", "sabir-ormani.yon-avcisi.01", AnalyticsService.Props("level_band", "minik"));
            a.Track("level_complete", "sabir-ormani.yon-avcisi.01", AnalyticsService.Props("stars", 3, "attempts", 2));

            a.FlushAsync().Wait();

            Assert.AreEqual(1, _http.Sent.Count);
            var body = _http.Sent[0].Body;
            Assert.AreEqual("http://api.test/v1/events", _http.Sent[0].Url);
            StringAssert.Contains("\"name\":\"level_complete\"", body);
            StringAssert.Contains("\"app_version\":\"0.1.0\"", body);
            StringAssert.Contains("\"platform\":\"android\"", body);
            StringAssert.DoesNotContain(_child.Id, body, "events are not linked to the account's profile ids");
            StringAssert.DoesNotContain("Elif", body);
            Assert.AreEqual(0, a.QueuedCount);
        }

        [Test]
        public void Flush_Offline_KeepsEventsAndSendsThemLater()
        {
            var a = Create();
            a.Track("app_open");
            _http.Handler = _ => new HttpResponse(0, null);
            a.FlushAsync().Wait();
            Assert.AreEqual(1, a.QueuedCount);

            _http.Handler = _ => new HttpResponse(202, "{}");
            Create().FlushAsync().Wait();
            Assert.AreEqual(0, Create().QueuedCount, "the queue survives a restart and is emptied");
        }

        [Test]
        public void Flush_RejectedBatch_IsDroppedSoItCannotBlockTheQueue()
        {
            var a = Create();
            a.Track("app_open");
            _http.Handler = _ => new HttpResponse(422, "{}");
            a.FlushAsync().Wait();
            Assert.AreEqual(0, a.QueuedCount);
        }

        [Test]
        public void Flush_ConsentWithdrawnMeanwhile_ClearsTheQueueWithoutSending()
        {
            var a = Create();
            a.Track("app_open");
            _profiles.WithdrawConsent();

            a.FlushAsync().Wait();

            Assert.AreEqual(0, a.QueuedCount);
            Assert.IsEmpty(_http.Sent);
        }

        [Test]
        public void Track_ManyEvents_SendsInBatchesOfAtMostAHundred()
        {
            _http.Handler = _ => new HttpResponse(0, null);
            var a = Create();
            for (int i = 0; i < 250; i++)
            {
                a.Track("app_open");
            }

            _http.Handler = _ => new HttpResponse(202, "{}");
            _http.Sent.Clear();
            a.FlushAsync().Wait();

            Assert.AreEqual(3, _http.Sent.Count);
            Assert.AreEqual(0, a.QueuedCount);
        }

        [Test]
        public void Track_Overflow_DropsTheOldest()
        {
            _http.Handler = _ => new HttpResponse(0, null);
            var a = Create();
            for (int i = 0; i < AnalyticsService.MaxQueued + 5; i++)
            {
                a.Track("app_open");
            }

            Assert.AreEqual(AnalyticsService.MaxQueued, a.QueuedCount);
        }

        [Test]
        public void RemovingTheProfile_DropsItsWaitingEventsAndAnonymousId()
        {
            _http.Handler = _ => new HttpResponse(0, null);
            var other = _profiles.Add("Ali", "robot-mor", AgeBand.Minik);
            var a = Create();
            a.Track("app_open");
            _profiles.SetActive(other.Id);
            a.Track("app_open");
            Assert.AreEqual(2, a.QueuedCount);

            _profiles.Remove(_child.Id);

            Assert.AreEqual(1, a.QueuedCount);
            Assert.IsFalse(File.ReadAllText(Path.Combine(_dir, AnalyticsService.File)).Contains(_child.Id));
        }

        [Test]
        public void TheSameChild_KeepsTheSameAnonymousId()
        {
            _http.Handler = _ => new HttpResponse(0, null);
            var a = Create();
            a.Track("app_open");
            a.Track("app_open");
            _http.Handler = _ => new HttpResponse(202, "{}");
            a.FlushAsync().Wait();
            Assert.AreEqual(1, _http.Sent.Count, "both events share one anonymous id, so one batch");
        }
    }
}
