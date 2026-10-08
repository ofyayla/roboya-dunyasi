using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Roboya.CodingEngine.Profiles;
using Roboya.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using static Roboya.Tests.PlayMode.YonAvcisiFlowTests;

namespace Roboya.Tests.PlayMode
{
    /// <summary>A1: first launch (grown-up gate → notice and consent → child profile) and profile management (F1-10).</summary>
    public class OnboardingTests
    {
        private string _dir;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "roboya-onboarding-" + Guid.NewGuid().ToString("N"));
            Bootstrap.ProgressFolderOverride = _dir;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Environment.SetEnvironmentVariable(DevEntitlements.Variable, null);
            Bootstrap.ProgressFolderOverride = null;
            foreach (var boot in UnityEngine.Object.FindObjectsByType<Bootstrap>(FindObjectsSortMode.None))
            {
                UnityEngine.Object.Destroy(boot.gameObject);
            }

            yield return null;
        }

        private static IEnumerator OpenMap(Action<VisualElement> found, string waitFor)
        {
            yield return SceneManager.LoadSceneAsync("Boot");
            VisualElement map = null;
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Map" && (map = FindRoot())?.Q(waitFor) != null, 10f);
            yield return null;
            yield return null;
            found(map);
        }

        private static void AnswerGate(VisualElement map)
        {
            var gate = map.Q<Roboya.UI.ParentGateView>("parent-gate");
            foreach (char c in Roboya.CodingEngine.Parents.ParentGate.ExpectedFor(gate.Challenge))
            {
                Tap(map.Q("gate-key-" + c));
            }

            Tap(map.Q("gate-confirm"));
        }

        [UnityTest]
        public IEnumerator FirstLaunch_GateThenNoticeThenProfile_LandsOnTheIsland()
        {
            VisualElement map = null;
            yield return OpenMap(r => map = r, "welcome");
            Assert.AreEqual(DisplayStyle.Flex, map.Q("welcome").resolvedStyle.display, "a new device starts with the grown-up");
            Assert.AreEqual(DisplayStyle.None, map.Q("island").resolvedStyle.display);
            yield return Capture(map, "16-welcome");

            Tap(map.Q("welcome-start"));
            yield return null;
            yield return null;
            AnswerGate(map);
            yield return null;
            yield return null;
            Assert.AreEqual(DisplayStyle.Flex, map.Q("notice").resolvedStyle.display, "consent comes before any profile");
            StringAssert.Contains("yurt dışına aktarılır", map.Q<Label>("notice-text").text);
            yield return Capture(map, "17-notice");

            Tap(map.Q("notice-accept"));
            yield return null;
            yield return null;
            Assert.AreEqual(DisplayStyle.Flex, map.Q("profile-editor").resolvedStyle.display);
            Assert.IsFalse(map.Q("profile-save").enabledSelf, "a nickname is needed");
            map.Q<TextField>("profile-nickname").value = "  Elif  ";
            Tap(map.Q("avatar-robot-mor"));
            Tap(map.Q("age-kasif"));
            yield return null;
            yield return Capture(map, "18-profile");
            Assert.IsTrue(map.Q("profile-save").enabledSelf);
            Tap(map.Q("profile-save"));
            yield return null;
            yield return null;

            Assert.AreEqual(DisplayStyle.Flex, map.Q("island").resolvedStyle.display);
            var saved = ProfileManager.Load(_dir);
            Assert.AreEqual("Elif", saved.Active.Nickname);
            Assert.AreEqual("robot-mor", saved.Active.AvatarId);
            Assert.AreEqual(AgeBand.Kasif, saved.Active.AgeBand);
            Assert.IsTrue(saved.Registry.HasConsentFor(TestProfiles.CurrentNoticeVersion()));
        }

        [UnityTest]
        public IEnumerator FirstLaunch_DecliningTheNotice_LeavesNoProfileAndNoConsent()
        {
            VisualElement map = null;
            yield return OpenMap(r => map = r, "welcome");
            Tap(map.Q("welcome-start"));
            yield return null;
            yield return null;
            AnswerGate(map);
            yield return null;
            yield return null;

            Tap(map.Q("notice-decline"));
            yield return null;
            yield return null;

            Assert.AreEqual(DisplayStyle.Flex, map.Q("welcome").resolvedStyle.display);
            var saved = ProfileManager.Load(_dir);
            Assert.IsFalse(saved.HasActive);
            Assert.IsNull(saved.Registry.Consent);
        }

        [UnityTest]
        public IEnumerator FirstLaunch_CancellingTheGateOrTheEditor_StaysOnTheWelcomeScreen()
        {
            VisualElement map = null;
            yield return OpenMap(r => map = r, "welcome");
            Tap(map.Q("welcome-start"));
            yield return null;
            yield return null;

            Tap(map.Q("gate-cancel"));
            yield return null;

            Assert.AreEqual(DisplayStyle.Flex, map.Q("welcome").resolvedStyle.display);
            Assert.IsFalse(map.Q<Roboya.UI.ParentGateView>("parent-gate").IsOpen);
        }

        [UnityTest]
        public IEnumerator ExistingConsentWithoutAProfile_SkipsTheNotice()
        {
            var manager = ProfileManager.Load(_dir);
            manager.RecordConsent(TestProfiles.CurrentNoticeVersion());
            VisualElement map = null;
            yield return OpenMap(r => map = r, "welcome");

            Tap(map.Q("welcome-start"));
            yield return null;
            yield return null;
            AnswerGate(map);
            yield return null;
            yield return null;

            Assert.AreEqual(DisplayStyle.None, map.Q("notice").resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.Flex, map.Q("profile-editor").resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator ExistingProfileWithoutConsent_AcceptingTheNotice_GoesStraightToTheIsland()
        {
            // An older install: the child's progress exists, the parent's consent does not.
            ProfileManager.Load(_dir).Add("Mucit", "robot-mavi", AgeBand.Minik);
            VisualElement map = null;
            yield return OpenMap(r => map = r, "welcome");

            Tap(map.Q("welcome-start"));
            yield return null;
            yield return null;
            AnswerGate(map);
            yield return null;
            yield return null;
            Tap(map.Q("notice-accept"));
            yield return null;
            yield return null;

            Assert.AreEqual(DisplayStyle.None, map.Q("notice").resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.None, map.Q("profile-editor").resolvedStyle.display, "the free tier already holds its one profile");
            Assert.AreEqual(DisplayStyle.Flex, map.Q("island").resolvedStyle.display, "the child must not be left on an empty screen");
        }

        [UnityTest]
        public IEnumerator ChangedNotice_AsksForConsentAgain()
        {
            var manager = ProfileManager.Load(_dir);
            manager.Add("Ali", "robot-mavi", AgeBand.Minik);
            manager.RecordConsent("an-older-version");
            VisualElement map = null;

            yield return OpenMap(r => map = r, "welcome");

            Assert.AreEqual(DisplayStyle.Flex, map.Q("welcome").resolvedStyle.display, "a new notice version needs new consent");
        }

        [UnityTest]
        public IEnumerator ParentArea_FreeTierHoldsOneProfile_PremiumAddsMore_AndRemovingNeedsTwoTaps()
        {
            TestProfiles.Seed(_dir, nickname: "Ali");
            Environment.SetEnvironmentVariable(DevEntitlements.Variable, "1"); // stands in for the server's premium
            VisualElement map = null;
            yield return OpenMap(r => map = r, "to-parent");
            Tap(map.Q("to-parent"));
            yield return null;
            yield return null;
            AnswerGate(map);
            yield return null;
            yield return null;
            Assert.AreEqual(1, map.Query<VisualElement>(className: "profile-row").ToList().Count);
            Assert.IsTrue(map.Q("profile-add").enabledSelf, "premium allows more than one");
            yield return Capture(map, "19-profiles");

            Tap(map.Q("profile-add"));
            yield return null;
            map.Q<TextField>("profile-nickname").value = "Ece";
            Tap(map.Q("avatar-robot-yesil"));
            Tap(map.Q("profile-save"));
            yield return null;
            yield return null;
            var rows = map.Query<VisualElement>(className: "profile-row").ToList();
            Assert.AreEqual(2, rows.Count);
            Assert.AreEqual("Ece", ProfileManager.Load(_dir).Registry.Profiles[1].Nickname);

            var ece = ProfileManager.Load(_dir).Registry.Profiles[1];
            Tap(map.Q("profile-select-" + ece.Id));
            yield return null;
            Assert.AreEqual(ece.Id, ProfileManager.Load(_dir).Active.Id, "the child picked is saved");

            Tap(map.Q("profile-remove-" + ece.Id));
            yield return null;
            Assert.AreEqual(2, ProfileManager.Load(_dir).Registry.Profiles.Count, "the first tap only asks");
            Tap(map.Q("profile-remove-" + ece.Id));
            yield return null;
            yield return null;
            Assert.AreEqual(1, ProfileManager.Load(_dir).Registry.Profiles.Count);
            Assert.AreEqual("Ali", ProfileManager.Load(_dir).Active.Nickname);
        }

        [UnityTest]
        public IEnumerator ParentArea_FreeTier_CannotAddASecondProfile()
        {
            TestProfiles.Seed(_dir);
            VisualElement map = null;
            yield return OpenMap(r => map = r, "to-parent");
            Tap(map.Q("to-parent"));
            yield return null;
            yield return null;
            AnswerGate(map);
            yield return null;
            yield return null;

            Assert.IsFalse(map.Q("profile-add").enabledSelf);
            StringAssert.Contains("1", map.Q<Label>("profile-limit").text);
        }
    }
}
