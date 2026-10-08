using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Roboya.CodingEngine.Parents;
using Roboya.CodingEngine.Profiles;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using static Roboya.Tests.PlayMode.YonAvcisiFlowTests;

namespace Roboya.Tests.PlayMode
{
    /// <summary>Critical flows of the map (F1-05), local progress (ILR-01/02) and the ship repair (ILR-03).</summary>
    public class MapFlowTests
    {
        private const string Level = "sabir-ormani.yon-avcisi.";

        private string _progressDir;

        [SetUp]
        public void SetUp()
        {
            _progressDir = Path.Combine(Path.GetTempPath(), "roboya-progress-" + Guid.NewGuid().ToString("N"));
            Bootstrap.ProgressFolderOverride = _progressDir;
            TestProfiles.Seed(_progressDir);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Bootstrap.ProgressFolderOverride = null;
            foreach (var boot in UnityEngine.Object.FindObjectsByType<Bootstrap>(FindObjectsSortMode.None))
            {
                UnityEngine.Object.Destroy(boot.gameObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Path_FreeTierFinished_FourthStoneAsksForGrownUp()
        {
            Seed(3);
            VisualElement map = null;
            yield return OpenMap(r => map = r);
            yield return Capture(map, "07-island");

            yield return OpenForest(map);
            yield return Capture(map, "08-path");

            Assert.IsTrue(map.Q("stone-3").ClassListContains("stone--done"));
            Assert.IsTrue(map.Q("stone-4").ClassListContains("stone--grownup"), "GLR-01: past the free tier");
            Assert.AreEqual(DisplayStyle.Flex, map.Q("stone-1").Q("stone-stars").resolvedStyle.display, "finished stones show stars");

            Tap(map.Q("stone-4"));
            yield return WaitForVoice("roboya.ask_grownup");
            Assert.AreEqual("Map", SceneManager.GetActiveScene().name, "a locked stone never opens a level");
        }

        [UnityTest]
        public IEnumerator Ship_FiveLevelsDone_FirstPartDropsOntoShipOnceAndWorkshopShowsIt()
        {
            Seed(5);
            VisualElement map = null;
            yield return OpenMap(r => map = r);

            // ILR-03: the island shows the painted ship; the earned propeller is fitted in the workshop.
            Assert.IsNull(map.Q("island-ship"), "the island ship is part of the illustration");
            yield return Capture(map, "09-island-ship");

            yield return OpenForest(map);
            Tap(map.Q("to-workshop"));
            yield return WaitUntil(() => map.Q("workshop").resolvedStyle.display == DisplayStyle.Flex, 5f);
            yield return new WaitForSeconds(0.3f);

            Assert.IsTrue(map.Q("part-tile-propeller").ClassListContains("workshop__tile--earned"));
            Assert.IsFalse(map.Q("part-tile-lights").ClassListContains("workshop__tile--earned"));
            Assert.IsNotNull(map.Q("workshop-ship").Q("ship-propeller"), "first part on the ship");
            Assert.AreEqual(1, TestProfiles.Seed(_progressDir).Book.ShipPartsSeen, "the drop-in plays once");
            Assert.IsNotNull(map.Q("workshop-ship").Q("ship-lights"), "coming parts are shown faintly in the workshop");
            yield return Capture(map, "10-workshop");
        }

        [UnityTest]
        public IEnumerator Level3_NextAfterSuccess_ReturnsToPathAndAsksForGrownUp()
        {
            Seed(2);
            VisualElement map = null;
            yield return OpenMap(r => map = r);
            yield return OpenForest(map);
            Tap(map.Q("stone-3"));

            VisualElement game = null;
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Game" && (game = FindRoot())?.Q("palette")?.childCount > 0, 10f);
            yield return PassStory(game);
            var catalog = LevelCatalog.Parse(Directory.GetFiles(FileLevelSource.RepositoryLevelsPath, "*.json", SearchOption.AllDirectories).Select(File.ReadAllText));
            foreach (var card in Roboya.CodingEngine.Solving.Solver.Solve(catalog.Find(Level + "03").Level).Solution)
            {
                Tap(game.Q("palette").Children().OfType<CardElement>().First(c => c.Card == card));
            }

            Tap(game.Q("play"));
            yield return WaitUntil(() => !game.Q("result").ClassListContains("hidden"), 30f);
            yield return null;
            Tap(game.Q("next"));

            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Map" && (map = FindRoot())?.Q("path") != null, 10f);
            yield return null;
            Assert.AreEqual(DisplayStyle.Flex, map.Q("path").resolvedStyle.display, "back on the path, not the island");
            yield return WaitForVoice("roboya.ask_grownup");
            Assert.IsTrue(map.Q("stone-3").ClassListContains("stone--done"), "progress recorded");
        }

        [UnityTest]
        public IEnumerator GuidedFirstLevel_PointsAtNextCardThenPlay_ButUnguidedLevelDoesNot()
        {
            VisualElement map = null;
            yield return OpenMap(r => map = r);
            yield return OpenForest(map);
            Tap(map.Q("stone-1"));

            VisualElement game = null;
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Game" && (game = FindRoot())?.Q("palette")?.childCount > 0, 10f);
            yield return PassStory(game);
            var forward = game.Q("palette").Children().OfType<CardElement>().First();
            Assert.IsTrue(forward.ClassListContains("card--guide"), "level 1 is guided: Roboya points at the forward card");
            Assert.IsFalse(game.Q("play").ClassListContains("icon-button--pulse"), "nothing to play yet");

            Tap(forward);
            Tap(forward);
            yield return null;
            Assert.IsFalse(forward.ClassListContains("card--guide"), "the plan is complete, nothing more to place");
            Assert.IsTrue(game.Q("play").ClassListContains("icon-button--pulse"), "Roboya points at play");
            yield return Capture(game, "12-guided");
        }

        [UnityTest]
        public IEnumerator ParentGate_WrongThenRightAnswer_OpensParentArea_AndLocksAfterThreeMisses()
        {
            VisualElement map = null;
            yield return OpenMap(r => map = r);
            Assert.AreEqual(DisplayStyle.None, map.Q("parent").resolvedStyle.display, "the parent area is closed at start");

            Tap(map.Q("to-parent"));
            yield return null;
            yield return null;
            var gate = map.Q<ParentGateView>("parent-gate");
            Assert.IsTrue(gate.IsOpen);
            Assert.AreEqual(DisplayStyle.None, map.Q("parent").resolvedStyle.display, "the gate comes first (VEL-01)");
            yield return Capture(map, "13-gate");

            // Same order as shown is wrong; the parent area stays closed.
            TypeDigits(map, string.Concat(gate.Challenge.Select(d => d.ToString())));
            Tap(map.Q("gate-confirm"));
            yield return null;
            Assert.IsTrue(gate.IsOpen);
            Assert.AreEqual(DisplayStyle.None, map.Q("parent").resolvedStyle.display);

            // The reversed numbers open the parent area.
            TypeDigits(map, Roboya.CodingEngine.Parents.ParentGate.ExpectedFor(gate.Challenge));
            Tap(map.Q("gate-confirm"));
            yield return null;
            yield return null;
            Assert.IsFalse(gate.IsOpen);
            Assert.AreEqual(DisplayStyle.Flex, map.Q("parent").resolvedStyle.display);
            yield return Capture(map, "14-parent");

            Tap(map.Q("parent-back"));
            yield return null;
            Assert.AreEqual(DisplayStyle.None, map.Q("parent").resolvedStyle.display);

            // Three misses lock the keypad.
            Tap(map.Q("to-parent"));
            yield return null;
            yield return null;
            for (int i = 0; i < 3; i++)
            {
                TypeDigits(map, "0000");
                Tap(map.Q("gate-confirm"));
                yield return null;
            }

            yield return null;
            Assert.IsFalse(map.Q("gate-pad").enabledSelf, "locked after three wrong answers");
            Assert.IsNotEmpty(map.Q<Label>("gate-message").text);
        }

        [UnityTest]
        public IEnumerator Path_KasifChild_StartsAtTheirLevelWithFreeLevelsFromThere()
        {
            var manager = ProfileManager.Load(_progressDir);
            manager.Update(manager.Active.Id, manager.Active.Nickname, manager.Active.AvatarId, AgeBand.Kasif);
            VisualElement map = null;
            yield return OpenMap(r => map = r);
            yield return OpenForest(map);

            Assert.IsTrue(map.Q("stone-6").ClassListContains("stone--current"), "F1-08: level 6 is the first meant for Kaşif");
            Assert.IsTrue(map.Q("stone-1").ClassListContains("stone--grownup"), "earlier levels are outside this child's free levels");
            Assert.IsTrue(map.Q("stone-8").ClassListContains("stone--locked"));
            Assert.IsTrue(map.Q("stone-9").ClassListContains("stone--grownup"), "three free levels counted from the start");
        }

        [UnityTest]
        public IEnumerator ParentArea_ShowsProgressSummaryAndSkillLines_WithoutScores()
        {
            Seed(3);
            VisualElement map = null;
            yield return OpenMap(r => map = r);
            Tap(map.Q("to-parent"));
            yield return null;
            yield return null;
            var gate = map.Q<ParentGateView>("parent-gate");
            TypeDigits(map, Roboya.CodingEngine.Parents.ParentGate.ExpectedFor(gate.Challenge));
            Tap(map.Q("gate-confirm"));
            yield return null;
            yield return null;

            var summary = map.Q<Label>("report-summary").text;
            StringAssert.Contains("3 tanesini bitirdi", summary);
            StringAssert.Contains("9 yıldız", summary);
            Assert.IsNotNull(map.Q("report-direction"), "direction appears in the forest levels");
            StringAssert.Contains("Yön bulma", map.Q<Label>("report-direction").text);
            map.Q<ScrollView>("parent-sections").ScrollTo(map.Q("report-section"));
            yield return null;
            yield return Capture(map, "18-report");
        }

        private sealed class ScriptedTransport : Roboya.Services.IHttpTransport
        {
            public readonly System.Collections.Generic.List<string> Urls = new System.Collections.Generic.List<string>();

            public System.Threading.Tasks.Task<Roboya.Services.HttpResponse> SendAsync(Roboya.Services.HttpRequest request)
            {
                Urls.Add(request.Url);
                string body = request.Url.EndsWith("/v1/auth/verify")
                    ? "{\"access_token\":\"a\",\"refresh_token\":\"r\",\"expires_in\":1800,\"account_id\":\"x\"}"
                    : null;
                int status = request.Url.EndsWith("/v1/auth/code") ? 202 : 200;
                return System.Threading.Tasks.Task.FromResult(new Roboya.Services.HttpResponse(status, body));
            }
        }

        [UnityTest]
        public IEnumerator ParentArea_NoServerConfigured_HidesTheAccountSection()
        {
            VisualElement map = null;
            yield return OpenMap(r => map = r);
            Assert.AreEqual(DisplayStyle.None, map.Q("account-section").resolvedStyle.display, "offline by default");
        }

        [UnityTest]
        public IEnumerator ParentArea_Account_SignInWithEmailCode_ShowsSignedIn_ThenSignOut()
        {
            var transport = new ScriptedTransport();
            Bootstrap.TransportOverride = transport;
            Environment.SetEnvironmentVariable(Roboya.Services.ApiConfig.Variable, "http://api.test");
            try
            {
                VisualElement map = null;
                yield return OpenMap(r => map = r);
                Tap(map.Q("to-parent"));
                yield return null;
                yield return null;
                var gate = map.Q<ParentGateView>("parent-gate");
                TypeDigits(map, Roboya.CodingEngine.Parents.ParentGate.ExpectedFor(gate.Challenge));
                Tap(map.Q("gate-confirm"));
                yield return null;
                yield return null;
                Assert.AreEqual(DisplayStyle.Flex, map.Q("account-section").resolvedStyle.display);

                map.Q<TextField>("account-email").value = "ayse@example.com";
                Tap(map.Q("account-send"));
                yield return null;
                yield return null;
                StringAssert.Contains("Kod gönderildi", map.Q<Label>("account-message").text);

                map.Q<TextField>("account-code").value = "123456";
                Tap(map.Q("account-verify"));
                yield return WaitUntil(() => map.Q("account-signed-in").resolvedStyle.display == DisplayStyle.Flex, 5f);
                StringAssert.Contains("ayse@example.com", map.Q<Label>("account-who").text);

                Tap(map.Q("account-sign-out"));
                yield return WaitUntil(() => map.Q("account-signed-out").resolvedStyle.display == DisplayStyle.Flex, 5f);
                Assert.Contains("http://api.test/v1/auth/code", transport.Urls);
            }
            finally
            {
                Bootstrap.TransportOverride = null;
                Environment.SetEnvironmentVariable(Roboya.Services.ApiConfig.Variable, null);
            }
        }

        [UnityTest]
        public IEnumerator DailyLimitUsedUp_MapShowsRest_StoneStaysClosed_ParentCanRaiseTheLimit()
        {
            Seed(1);
            var manager = ProfileManager.Load(_progressDir);
            var book = new ScreenTimeBook();
            book.SetLimit(manager.Active.Id, 10);
            book.AddUsage(manager.Active.Id, DateTime.Now.Date, 600);
            File.WriteAllText(Path.Combine(_progressDir, ScreenTimeService.File), book.ToJson());

            VisualElement map = null;
            yield return SceneManager.LoadSceneAsync("Boot");
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Map" && (map = FindRoot())?.Q("rest") != null, 10f);
            yield return null;
            yield return null;
            Assert.AreEqual(DisplayStyle.Flex, map.Q("rest").resolvedStyle.display, "VEL-02: Roboya rests when the day's time is used up");
            yield return WaitForVoice("rest.battery_empty");
            yield return Capture(map, "16-rest");

            // The parent opens the gate from the rest screen and raises the limit.
            Tap(map.Q("rest-parent"));
            yield return null;
            yield return null;
            var gate = map.Q<ParentGateView>("parent-gate");
            Assert.IsTrue(gate.IsOpen);
            TypeDigits(map, Roboya.CodingEngine.Parents.ParentGate.ExpectedFor(gate.Challenge));
            Tap(map.Q("gate-confirm"));
            yield return null;
            yield return null;
            map.Q<ScrollView>("parent-sections").ScrollTo(map.Q("time-0"));
            yield return null;
            yield return null;
            yield return Capture(map, "17-parent-time");
            Tap(map.Q("time-0"));
            yield return null;
            Assert.IsTrue(map.Q("time-0").ClassListContains("is-selected"));

            Tap(map.Q("parent-back"));
            yield return null;
            Assert.AreEqual(DisplayStyle.None, map.Q("rest").resolvedStyle.display, "unlimited: the island is back");
            Assert.AreEqual(DisplayStyle.Flex, map.Q("island").resolvedStyle.display);
        }

        private static void TypeDigits(VisualElement map, string digits)
        {
            foreach (char c in digits)
            {
                Tap(map.Q("gate-key-" + c));
            }
        }

        [UnityTest]
        public IEnumerator Level2_FiveFailedRuns_OffersTheEasierLevelAndTheChildCanTakeIt()
        {
            Seed(1);
            VisualElement map = null;
            yield return OpenMap(r => map = r);
            yield return OpenForest(map);
            Tap(map.Q("stone-2"));

            VisualElement game = null;
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Game" && (game = FindRoot())?.Q("palette")?.childCount > 0, 10f);
            yield return PassStory(game);
            Assert.IsTrue(game.Q("easier").ClassListContains("hidden"), "no offer before any failure");

            // Two steps are needed; one card always ends short of the turtle. Five tries, no penalty.
            var forward = game.Q("palette").Children().First();
            Tap(forward);
            for (int i = 0; i < 5; i++)
            {
                Tap(game.Q("play"));
                yield return new WaitForSeconds(0.2f);
                yield return WaitUntil(() => game.Q("play").enabledSelf, 15f);
            }

            yield return WaitUntil(() => !game.Q("easier").ClassListContains("hidden"), 10f);
            Assert.IsTrue(game.Q("result").ClassListContains("hidden"), "failing is never punished");
            yield return Capture(game, "15-easier-offer");

            var previous = game;
            Tap(game.Q("easier"));
            // The Game scene reloads under the same name: wait for the new UI root, not the old one.
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Game" && (game = FindRoot()) != previous && game?.Q("progress")?.childCount > 0, 10f);
            yield return PassStory(game);
            var dots = game.Q("progress").Children().ToList();
            Assert.IsTrue(dots[0].ClassListContains("progress__dot--current"), "the easier level is level 1");
        }

        private void Seed(int completed)
        {
            var store = TestProfiles.Seed(_progressDir);
            for (int i = 1; i <= completed; i++)
            {
                store.Book.Record(Level + i.ToString("D2"), 3);
            }

            store.Save();
        }

        private static IEnumerator OpenMap(Action<VisualElement> found)
        {
            yield return SceneManager.LoadSceneAsync("Boot");
            VisualElement map = null;
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Map" && (map = FindRoot())?.Q("region-sabir-ormani") != null, 10f);
            yield return null;
            yield return null;
            found(map);
        }

        private static IEnumerator OpenForest(VisualElement map)
        {
            Tap(map.Q("region-sabir-ormani"));
            yield return WaitUntil(() => map.Q("path").resolvedStyle.display == DisplayStyle.Flex, 5f);
            yield return null;
            yield return null;
        }

        private static IEnumerator WaitForVoice(string key)
        {
            AudioSource voice = null;
            yield return WaitUntil(
                () => (voice = UnityEngine.Object.FindAnyObjectByType<AudioSource>()) != null && voice.clip != null && voice.clip.name == key,
                5f);
        }
    }
}
