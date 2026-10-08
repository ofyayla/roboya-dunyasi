using System;
using NUnit.Framework;
using Roboya.CodingEngine.Profiles;

namespace Roboya.Tests.CodingEngine
{
    public class ProfileRegistryTests
    {
        [Test]
        public void Add_FirstProfile_BecomesActiveAndTrimsNickname()
        {
            var registry = new ProfileRegistry();

            var p = registry.Add("  Elif  ", "robot-mavi", AgeBand.Minik);

            Assert.AreEqual("Elif", p.Nickname);
            Assert.AreSame(p, registry.Active);
            Assert.AreEqual(32, p.Id.Length);
        }

        [Test]
        public void Add_SecondProfile_DoesNotChangeTheActiveOne()
        {
            var registry = new ProfileRegistry();
            var first = registry.Add("Ali", "robot-mavi", AgeBand.Minik);

            registry.Add("Ece", "robot-mor", AgeBand.Kasif);

            Assert.AreSame(first, registry.Active);
            Assert.AreEqual(2, registry.Profiles.Count);
        }

        [TestCase("", "robot-mavi", "nickname_empty")]
        [TestCase("   ", "robot-mavi", "nickname_empty")]
        [TestCase("123456789012345678901234x", "robot-mavi", "nickname_too_long")]
        [TestCase("Elif", "Robot Mavi", "avatar_invalid")]
        [TestCase("Elif", "", "avatar_invalid")]
        [TestCase("Elif", null, "avatar_invalid")]
        public void Add_BadFields_Throw(string nickname, string avatar, string expected)
        {
            var ex = Assert.Throws<ArgumentException>(() => new ProfileRegistry().Add(nickname, avatar, AgeBand.Minik));

            Assert.AreEqual(expected, ex.Message);
        }

        [Test]
        public void Add_DuplicateId_Throws()
        {
            var registry = new ProfileRegistry();
            registry.Add("Ali", "robot-mavi", AgeBand.Minik, "abc");

            Assert.Throws<ArgumentException>(() => registry.Add("Ece", "robot-mor", AgeBand.Minik, "abc"));
        }

        [Test]
        public void Update_ChangesFieldsAndRejectsUnknownOrInvalid()
        {
            var registry = new ProfileRegistry();
            var p = registry.Add("Ali", "robot-mavi", AgeBand.Minik);

            registry.Update(p.Id, " Ali Can ", "robot-yesil", AgeBand.Kasif);

            Assert.AreEqual("Ali Can", p.Nickname);
            Assert.AreEqual("robot-yesil", p.AvatarId);
            Assert.AreEqual(AgeBand.Kasif, p.AgeBand);
            Assert.Throws<ArgumentException>(() => registry.Update("nope", "A", "robot-mavi", AgeBand.Minik));
            Assert.Throws<ArgumentException>(() => registry.Update(p.Id, "", "robot-mavi", AgeBand.Minik));
        }

        [Test]
        public void SetActive_SwitchesAndRejectsUnknown()
        {
            var registry = new ProfileRegistry();
            registry.Add("Ali", "robot-mavi", AgeBand.Minik);
            var second = registry.Add("Ece", "robot-mor", AgeBand.Kasif);

            registry.SetActive(second.Id);

            Assert.AreSame(second, registry.Active);
            Assert.Throws<ArgumentException>(() => registry.SetActive("nope"));
        }

        [Test]
        public void Remove_ActiveProfile_HandsOverToTheFirstRemaining()
        {
            var registry = new ProfileRegistry();
            var first = registry.Add("Ali", "robot-mavi", AgeBand.Minik);
            var second = registry.Add("Ece", "robot-mor", AgeBand.Kasif);

            Assert.IsTrue(registry.Remove(first.Id));
            Assert.AreSame(second, registry.Active);
            Assert.IsTrue(registry.Remove(second.Id));
            Assert.IsNull(registry.Active);
            Assert.IsFalse(registry.Remove("nope"));
        }

        [Test]
        public void Consent_IsPerNoticeVersionAndCanBeWithdrawn()
        {
            var registry = new ProfileRegistry();
            Assert.IsFalse(registry.HasConsentFor("v1"));

            registry.RecordConsent("v1", new DateTime(2026, 10, 8, 12, 0, 0, DateTimeKind.Utc));

            Assert.IsTrue(registry.HasConsentFor("v1"));
            Assert.IsFalse(registry.HasConsentFor("v2"), "a new notice version needs new consent");
            Assert.IsFalse(registry.HasConsentFor(null));
            registry.WithdrawConsent();
            Assert.IsFalse(registry.HasConsentFor("v1"));
            Assert.Throws<ArgumentException>(() => registry.RecordConsent("", DateTime.UtcNow));
        }

        [Test]
        public void Json_RoundTrip_KeepsProfilesActiveAndConsent()
        {
            var registry = new ProfileRegistry();
            registry.Add("Ali", "robot-mavi", AgeBand.Minik, "id1");
            registry.Add("Ece", "robot-mor", AgeBand.Kasif, "id2");
            registry.SetActive("id2");
            registry.RecordConsent("v1", new DateTime(2026, 10, 8, 12, 0, 0, DateTimeKind.Utc));

            var json = registry.ToJson();
            var copy = ProfileRegistry.FromJson(json);

            StringAssert.Contains("\"kasif\"", json, "age bands are written as words");
            Assert.AreEqual(2, copy.Profiles.Count);
            Assert.AreEqual("id2", copy.Active.Id);
            Assert.AreEqual(AgeBand.Kasif, copy.Active.AgeBand);
            Assert.IsTrue(copy.HasConsentFor("v1"));
        }

        [TestCase("{ nope")]
        [TestCase("null")]
        [TestCase("{\"version\": 99}")]
        public void FromJson_BadInput_ThrowsFormat(string json)
        {
            Assert.Throws<FormatException>(() => ProfileRegistry.FromJson(json));
        }

        [Test]
        public void FromJson_ActiveIdOfAMissingProfile_FallsBackToTheFirst()
        {
            var copy = ProfileRegistry.FromJson("{\"version\":1,\"profiles\":[{\"id\":\"a\",\"nickname\":\"Ali\",\"avatarId\":\"robot-mavi\",\"ageBand\":\"minik\"}],\"activeId\":\"zzz\"}");

            Assert.AreEqual("a", copy.Active.Id);
            Assert.IsNull(ProfileRegistry.FromJson("{\"version\":1}").Active);
        }
    }
}
