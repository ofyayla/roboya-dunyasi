using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Roboya.Services;

namespace Roboya.Tests.Services
{
    internal sealed class FakeTransport : IHttpTransport
    {
        public readonly List<HttpRequest> Sent = new List<HttpRequest>();
        public readonly Queue<HttpResponse> Replies = new Queue<HttpResponse>();

        public Task<HttpResponse> SendAsync(HttpRequest request)
        {
            Sent.Add(request);
            return Task.FromResult(Replies.Count > 0 ? Replies.Dequeue() : new HttpResponse(0, null));
        }
    }

    public class AccountServiceTests
    {
        private const string Tokens = "{\"access_token\":\"acc1\",\"refresh_token\":\"ref1\",\"expires_in\":1800,\"account_id\":\"a-1\"}";
        private const string Tokens2 = "{\"access_token\":\"acc2\",\"refresh_token\":\"ref2\",\"expires_in\":1800,\"account_id\":\"a-1\"}";

        private string _dir;
        private DateTime _now;
        private FakeTransport _http;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "roboya-account-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
            _now = new DateTime(2026, 10, 9, 8, 0, 0, DateTimeKind.Utc);
            _http = new FakeTransport();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_dir))
            {
                Directory.Delete(_dir, true);
            }
        }

        private AccountService Create() =>
            new AccountService(new ApiClient("http://api.test/", _http), _dir, "android", () => _now);

        private static T Wait<T>(Task<T> task)
        {
            Assert.IsTrue(task.Wait(2000));
            return task.Result;
        }

        [Test]
        public void New_IsSignedOutWithARandomDeviceId_KeptAcrossRestarts()
        {
            var first = Create();
            Assert.IsFalse(first.IsSignedIn);
            Assert.IsNotEmpty(first.DeviceId);
            Assert.AreEqual(first.DeviceId, Create().DeviceId);
        }

        [Test]
        public void SignIn_SendsEmailCodeAndDevice_StoresTokens()
        {
            _http.Replies.Enqueue(new HttpResponse(200, Tokens));
            var account = Create();

            Assert.IsTrue(account.SignInAsync(" ayse@example.com ", "123456").Wait(2000));

            Assert.IsTrue(account.IsSignedIn);
            Assert.AreEqual("ayse@example.com", account.Email);
            var sent = _http.Sent[0];
            Assert.AreEqual("http://api.test/v1/auth/verify", sent.Url);
            StringAssert.Contains("\"code\":\"123456\"", sent.Body);
            StringAssert.Contains("\"platform\":\"android\"", sent.Body);
            StringAssert.Contains(account.DeviceId, sent.Body);
            Assert.IsTrue(Create().IsSignedIn, "the session survives a restart");
        }

        [Test]
        public void SignIn_WrongCode_ThrowsTheServerCode_AndStaysSignedOut()
        {
            _http.Replies.Enqueue(new HttpResponse(401, "{\"code\":\"invalid_code\"}"));
            var account = Create();

            var e = Assert.Throws<AggregateException>(() => account.SignInAsync("a@b.co", "000000").Wait(2000));

            Assert.AreEqual("invalid_code", ((ApiException)e.InnerException).Code);
            Assert.IsFalse(account.IsSignedIn);
        }

        [Test]
        public void SignIn_ServerUnreachable_ThrowsNetwork()
        {
            _http.Replies.Enqueue(new HttpResponse(0, null));
            var e = Assert.Throws<AggregateException>(() => Create().RequestCodeAsync("a@b.co").Wait(2000));
            Assert.IsTrue(((ApiException)e.InnerException).IsNetwork);
        }

        [Test]
        public void GetAccessToken_StillValid_ReturnsItWithoutANetworkCall()
        {
            _http.Replies.Enqueue(new HttpResponse(200, Tokens));
            var account = Create();
            account.SignInAsync("a@b.co", "123456").Wait(2000);

            _now = _now.AddMinutes(10);

            Assert.AreEqual("acc1", Wait(account.GetAccessTokenAsync()));
            Assert.AreEqual(1, _http.Sent.Count);
        }

        [Test]
        public void GetAccessToken_AboutToExpire_RefreshesAndRotatesTheRefreshToken()
        {
            _http.Replies.Enqueue(new HttpResponse(200, Tokens));
            _http.Replies.Enqueue(new HttpResponse(200, Tokens2));
            var account = Create();
            account.SignInAsync("a@b.co", "123456").Wait(2000);

            _now = _now.AddSeconds(1800 - 30);

            Assert.AreEqual("acc2", Wait(account.GetAccessTokenAsync()));
            StringAssert.Contains("ref1", _http.Sent[1].Body);
            Assert.AreEqual("http://api.test/v1/auth/refresh", _http.Sent[1].Url);
        }

        [Test]
        public void GetAccessToken_RefusedRefreshToken_EndsTheSession()
        {
            _http.Replies.Enqueue(new HttpResponse(200, Tokens));
            _http.Replies.Enqueue(new HttpResponse(401, "{\"code\":\"invalid_token\"}"));
            var account = Create();
            account.SignInAsync("a@b.co", "123456").Wait(2000);
            _now = _now.AddHours(1);

            Assert.IsNull(Wait(account.GetAccessTokenAsync()));
            Assert.IsFalse(account.IsSignedIn);
        }

        [Test]
        public void GetAccessToken_Offline_KeepsTheSession()
        {
            _http.Replies.Enqueue(new HttpResponse(200, Tokens));
            _http.Replies.Enqueue(new HttpResponse(0, null));
            var account = Create();
            account.SignInAsync("a@b.co", "123456").Wait(2000);
            _now = _now.AddHours(1);

            Assert.IsNull(Wait(account.GetAccessTokenAsync()));
            Assert.IsTrue(account.IsSignedIn);
        }

        [Test]
        public void SignOut_ClearsLocallyEvenWhenTheServerIsUnreachable()
        {
            _http.Replies.Enqueue(new HttpResponse(200, Tokens));
            _http.Replies.Enqueue(new HttpResponse(0, null));
            var account = Create();
            account.SignInAsync("a@b.co", "123456").Wait(2000);

            Assert.IsTrue(account.SignOutAsync().Wait(2000));

            Assert.IsFalse(account.IsSignedIn);
            Assert.IsNull(account.Email);
            Assert.IsFalse(File.ReadAllText(Path.Combine(_dir, AccountService.File)).Contains("ref1"));
        }

        [Test]
        public void ApiConfig_ReadsServerJson_AndNothingMeansOffline()
        {
            Assert.IsNull(ApiConfig.Resolve(_dir));
            File.WriteAllText(Path.Combine(_dir, ApiConfig.File), "{\"apiUrl\":\" http://10.0.0.5:8000 \"}");
            Assert.AreEqual("http://10.0.0.5:8000", ApiConfig.Resolve(_dir));
        }
    }
}
