using System;
using System.IO;
using NUnit.Framework;
using Roboya.CodingEngine.Profiles;
using Roboya.Core;
using UnityEngine.TestTools;

namespace Roboya.Tests.Core
{
    public class ScreenTimeServiceTests
    {
        private string _dir;
        private DateTime _now;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "roboya-time-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
            _now = new DateTime(2026, 10, 8, 10, 0, 0);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_dir))
            {
                Directory.Delete(_dir, true);
            }
        }

        private ScreenTimeService Create(out ProfileManager profiles)
        {
            profiles = ProfileManager.Load(_dir);
            if (!profiles.HasActive)
            {
                profiles.Add("Test", "robot-mavi", AgeBand.Minik);
            }

            return new ScreenTimeService(_dir, profiles, () => _now);
        }

        [Test]
        public void Tick_PastTheRecommendedLimit_ExhaustsTheDay()
        {
            var time = Create(out _);
            Assert.IsFalse(time.IsExhausted);
            for (int i = 0; i < 601; i++)
            {
                time.Tick(1f);
            }

            Assert.IsTrue(time.IsExhausted, "Minik default is 10 minutes");
            Assert.AreEqual(10, time.UsedMinutesToday);
        }

        [Test]
        public void Tick_LongFrame_CountsOnlyOneSecond()
        {
            var time = Create(out _);
            time.Tick(3600f);
            Assert.AreEqual(0, time.UsedMinutesToday);
            Assert.IsFalse(time.IsExhausted);
        }

        [Test]
        public void IsExhausted_NextDay_StartsFresh()
        {
            var time = Create(out _);
            for (int i = 0; i < 601; i++)
            {
                time.Tick(1f);
            }

            _now = _now.AddDays(1);
            Assert.IsFalse(time.IsExhausted);
        }

        [Test]
        public void SetLimit_Unlimited_NeverExhausts()
        {
            var time = Create(out _);
            time.SetLimit(0);
            for (int i = 0; i < 3000; i++)
            {
                time.Tick(1f);
            }

            Assert.IsFalse(time.IsExhausted);
        }

        [Test]
        public void Flush_ThenReload_KeepsLimitAndUsage()
        {
            var time = Create(out _);
            time.SetLimit(30);
            for (int i = 0; i < 120; i++)
            {
                time.Tick(1f);
            }

            time.Flush();

            var again = Create(out _);
            Assert.AreEqual(30, again.LimitMinutes);
            Assert.AreEqual(2, again.UsedMinutesToday);
        }

        [Test]
        public void Constructor_CorruptFile_StartsFreshAndKeepsTheBrokenFile()
        {
            File.WriteAllText(Path.Combine(_dir, ScreenTimeService.File), "{ not json");
            LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("Screen time file unreadable"));
            var time = Create(out _);
            Assert.IsFalse(time.IsExhausted);
            Assert.AreEqual(1, Directory.GetFiles(_dir, "screentime.json.corrupt-*").Length);
        }

        [Test]
        public void RemovingTheProfile_ForgetsItsTime()
        {
            var time = Create(out var profiles);
            time.SetLimit(15);
            string id = profiles.Active.Id;
            profiles.Remove(id);
            Assert.IsFalse(File.ReadAllText(Path.Combine(_dir, ScreenTimeService.File)).Contains(id));
        }
    }
}
