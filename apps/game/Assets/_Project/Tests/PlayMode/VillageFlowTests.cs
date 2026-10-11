using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using static Roboya.Tests.PlayMode.YonAvcisiFlowTests;

namespace Roboya.Tests.PlayMode
{
    /// <summary>
    /// The second region on the map and in a level (plan C): Paylaşım Köyü draws its own backdrop and friend. The repository has no
    /// village levels yet (E1), so small fixtures stand in; the board shows no hives until D1.
    /// </summary>
    public class VillageFlowTests
    {
        private string _progressDir;

        [SetUp]
        public void SetUp()
        {
            _progressDir = Path.Combine(Path.GetTempPath(), "roboya-progress-" + Guid.NewGuid().ToString("N"));
            Bootstrap.ProgressFolderOverride = _progressDir;
            Bootstrap.ExtraLevelsOverride = Enumerable.Range(1, 8).Select(Level).ToList();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Bootstrap.ProgressFolderOverride = null;
            Bootstrap.ExtraLevelsOverride = null;
            Environment.SetEnvironmentVariable(DevEntitlements.Variable, null);
            foreach (var boot in UnityEngine.Object.FindObjectsByType<Bootstrap>(FindObjectsSortMode.None))
            {
                UnityEngine.Object.Destroy(boot.gameObject);
            }

            yield return null;
        }

        /// <summary>Minik levels 1–6 give a hive its honey by themselves (auto-drop); Kaşif levels 7–8 only collect.</summary>
        private static string Level(int order)
        {
            bool minik = order <= 6;
            string age = minik ? "minik" : "kasif";
            string honey = minik ? @"""amount"": 2, " : string.Empty;
            string drops = minik ? @"""drops"": [ { ""id"": ""petek-1"", ""kind"": ""hive"", ""x"": 3, ""y"": 0, ""need"": 2 } ]," : string.Empty;
            string goal = minik ? @"{ ""collect"": [""bal-1""], ""deliver"": [ { ""to"": ""petek-1"", ""count"": 2 } ] }" : @"{ ""collect"": [""bal-1""] }";
            string options = minik ? @"""options"": { ""autoDrop"": true, ""resetEachRun"": true, ""clearMemoryEachRun"": true }," : string.Empty;
            return @"{
  ""schemaVersion"": 3,
  ""id"": ""paylasim-koyu.bal-pesinde." + order.ToString("00") + @""",
  ""region"": ""paylasim-koyu"",
  ""game"": ""bal-pesinde"",
  ""order"": " + order + @",
  ""meta"": { ""concepts"": [""counting""], ""value"": ""sharing"", ""difficulty"": 1, ""ageLevels"": [""" + age + @"""] },
  ""grid"": { ""rows"": [""....."", "".....""] },
  ""robot"": { ""x"": 0, ""y"": 0, ""facing"": ""east"" },
  ""items"": [ { ""id"": ""bal-1"", " + honey + @"""kind"": ""honey"", ""x"": 2, ""y"": 0 } ],
  " + drops + @"
  ""goal"": " + goal + @",
  ""cards"": { ""palette"": [""forward"", ""turn_right""], ""maxProgramLength"": 4 },
  " + options + @"
  ""voice"": { ""intro"": ""bal_pesinde.l01.intro"" }
}";
        }

        /// <summary>Finishes the whole forest, so the village opens (the region before it must be finished).</summary>
        private void FinishForest()
        {
            var catalog = LevelCatalog.Parse(Directory.GetFiles(FileLevelSource.RepositoryLevelsPath, "*.json", SearchOption.AllDirectories).Select(File.ReadAllText));
            var store = TestProfiles.Seed(_progressDir);
            foreach (var entry in catalog.All)
            {
                store.Book.Record(entry.Id, 3);
            }

            store.Book.MarkShipPartsSeen(99);
            store.Save();
        }

        private static IEnumerator OpenVillagePath(Action<VisualElement> found)
        {
            yield return SceneManager.LoadSceneAsync("Boot");
            VisualElement map = null;
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Map" && (map = FindRoot())?.Q("region-paylasim-koyu") != null, 10f);
            yield return null;
            Tap(map.Q("region-paylasim-koyu"));
            yield return WaitUntil(() => map.Q("path").resolvedStyle.display == DisplayStyle.Flex, 5f);
            yield return null;
            yield return null;
            found(map);
        }

        [UnityTest]
        public IEnumerator VillagePath_AfterTheForest_UsesTheVillageBackdropAndShowsItsStones()
        {
            FinishForest();
            // The village is premium content: no free tier (plan C1). The dev switch stands in for the server's grant.
            Environment.SetEnvironmentVariable(DevEntitlements.Variable, "1");
            VisualElement map = null;
            yield return OpenVillagePath(r => map = r);
            yield return Capture(map, "18-village-path");

            var village = RegionArtLibrary.Load().For(RegionId.PaylasimKoyu, null);
            var forest = RegionArtLibrary.Load().For(RegionId.SabirOrmani, null);
            Assert.AreNotSame(village.Background, forest.Background);
            Assert.AreSame(village.Background, map.Q("path").resolvedStyle.backgroundImage.sprite, "the trail is painted with the village backdrop");
            Assert.IsNotNull(map.Q("stone-8"), "one stone per village level");
            Assert.IsNull(map.Q("stone-9"));
            Assert.IsNotNull(map.Q("corner-4"), "the village has four corners of two levels here");
        }

        [UnityTest]
        public IEnumerator VillagePath_WithoutPremium_AsksAGrownUpOnEveryStone()
        {
            FinishForest();
            VisualElement map = null;
            yield return OpenVillagePath(r => map = r);

            for (int i = 1; i <= 8; i++)
            {
                Assert.IsTrue(map.Q("stone-" + i).ClassListContains("stone--grownup"), "village stone " + i + " is premium: no free tier outside the forest");
            }
        }

        [UnityTest]
        public IEnumerator VillageLevel_Opened_UsesTheVillageBackdrop()
        {
            FinishForest();
            Environment.SetEnvironmentVariable(DevEntitlements.Variable, "1");
            VisualElement map = null;
            yield return OpenVillagePath(r => map = r);

            Tap(map.Q("path-play"));
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Game" && FindRoot()?.Q("screen") != null, 10f);
            yield return null;
            yield return null;
            var game = FindRoot();
            yield return Capture(game, "19-village-level");

            var village = RegionArtLibrary.Load().For(RegionId.PaylasimKoyu, null);
            Assert.AreSame(village.Background, game.Q("screen").resolvedStyle.backgroundImage.sprite, "the level screen shows the village backdrop");
            Assert.IsTrue(game.Q("screen").ClassListContains("screen--art"));
        }

        [UnityTest]
        public IEnumerator VillageLevel_AutoDrop_BringsTheHoneyToTheHiveAndEmptiesTheBasket()
        {
            FinishForest();
            Environment.SetEnvironmentVariable(DevEntitlements.Variable, "1");
            VisualElement map = null;
            yield return OpenVillagePath(r => map = r);

            Tap(map.Q("path-play"));
            VisualElement game = null;
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Game" && (game = FindRoot())?.Q("palette")?.childCount > 0, 10f);
            yield return PassStory(game);

            Assert.IsNotNull(game.Q("hive-petek-1"), "the hive stands on the board");
            Assert.AreEqual(2, game.Q("hive-petek-1").Query(className: "hive__slot").ToList().Count, "one slot per honey it asks for");
            Assert.AreEqual(0, game.Q("hive-petek-1").Query(className: "hive__slot--filled").ToList().Count);
            Assert.AreEqual(DisplayStyle.Flex, game.Q("basket-view").resolvedStyle.display);
            Assert.AreEqual(0, game.Q("basket-view").Query(className: "basket-view__dot--filled").ToList().Count, "an empty basket");
            yield return Capture(game, "20a-village-hive-before");

            var forward = game.Q("palette").Children().First();
            Tap(forward);
            Tap(forward);
            Tap(forward);
            yield return null;
            Tap(game.Q("play"));

            // The bee picks the two honey up on its second step, then the hive takes them as it arrives.
            yield return WaitUntil(() => game.Q("basket-view").Query(className: "basket-view__dot--filled").ToList().Count == 2, 10f);
            yield return Capture(game, "20b-village-basket");
            yield return WaitUntil(() => game.Q("hive-petek-1").Query(className: "hive__slot--filled").ToList().Count == 2, 10f);
            yield return new WaitForSeconds(0.5f);
            yield return Capture(game, "20c-village-hive-full");

            Assert.AreEqual(0, game.Q("basket-view").Query(className: "basket-view__dot--filled").ToList().Count, "the basket is empty again");
            yield return WaitUntil(() => !game.Q("result").ClassListContains("hidden"), 15f);
        }
    }
}
