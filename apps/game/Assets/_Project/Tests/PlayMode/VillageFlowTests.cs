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
    /// village levels here: small hand-made fixtures stand in for the real village (Bootstrap.ExtraLevelsOverride replaces its repository levels), so each test controls the numbers. VillageContentFlowTests plays the real levels.
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
            Bootstrap.AskRemainingOverride = false;
            Environment.SetEnvironmentVariable(DevEntitlements.Variable, null);
            foreach (var boot in UnityEngine.Object.FindObjectsByType<Bootstrap>(FindObjectsSortMode.None))
            {
                UnityEngine.Object.Destroy(boot.gameObject);
            }

            yield return null;
        }

        /// <summary>
        /// Minik levels 1–6 collect two honey and the hive takes them by itself (auto-drop); Kaşif levels 7–8 start with three honey in
        /// the basket (the hive asks for two) and need the "Bırak" card (F F Bırak Bırak).
        /// </summary>
        private static string Level(int order)
        {
            if (order == 8)
            {
                return ShareLevel();
            }

            bool minik = order <= 6;
            string age = minik ? "minik" : "kasif";
            string robot = minik ? @"{ ""x"": 0, ""y"": 0, ""facing"": ""east"" }" : @"{ ""x"": 0, ""y"": 0, ""facing"": ""east"", ""carrying"": 3 }";
            string items = minik ? @"""items"": [ { ""id"": ""bal-1"", ""amount"": 2, ""kind"": ""honey"", ""x"": 2, ""y"": 0 } ]," : string.Empty;
            string hive = minik ? @"{ ""id"": ""petek-1"", ""kind"": ""hive"", ""x"": 3, ""y"": 0, ""need"": 2 }" : @"{ ""id"": ""petek-1"", ""kind"": ""hive"", ""x"": 2, ""y"": 0, ""need"": 2 }";
            string goal = minik ? @"{ ""collect"": [""bal-1""], ""deliver"": [ { ""to"": ""petek-1"", ""count"": 2 } ] }" : @"{ ""deliver"": [ { ""to"": ""petek-1"", ""count"": 2 } ] }";
            string palette = minik ? @"[""forward"", ""turn_right""]" : @"[""forward"", ""drop""]";
            string options = minik
                ? @"{ ""autoDrop"": true, ""resetEachRun"": true, ""clearMemoryEachRun"": true }"
                : @"{ ""resetEachRun"": true, ""beeMemoryVisible"": true, ""askRemaining"": true }";
            return @"{
  ""schemaVersion"": 3,
  ""id"": ""paylasim-koyu.bal-pesinde." + order.ToString("00") + @""",
  ""region"": ""paylasim-koyu"",
  ""game"": ""bal-pesinde"",
  ""order"": " + order + @",
  ""meta"": { ""concepts"": [""counting""], ""value"": ""sharing"", ""difficulty"": 1, ""ageLevels"": [""" + age + @"""] },
  ""grid"": { ""rows"": [""....."", "".....""] },
  ""robot"": " + robot + @",
  " + items + @"
  ""drops"": [ " + hive + @" ],
  ""goal"": " + goal + @",
  ""cards"": { ""palette"": " + palette + @", ""maxProgramLength"": 5 },
  ""options"": " + options + @",
  ""voice"": { ""intro"": ""bal_pesinde.l01.intro"" },
  ""solution"": { ""shortestLength"": " + (minik ? 3 : 4) + @" }
}";
        }

        /// <summary>Level 8 shares five honey between two hives of two; the one left over goes on the table (F F D D F D D F D).</summary>
        private static string ShareLevel() => @"{
  ""schemaVersion"": 3,
  ""id"": ""paylasim-koyu.bal-pesinde.08"",
  ""region"": ""paylasim-koyu"",
  ""game"": ""bal-pesinde"",
  ""order"": 8,
  ""meta"": { ""concepts"": [""counting""], ""value"": ""sharing"", ""difficulty"": 2, ""ageLevels"": [""kasif""] },
  ""grid"": { ""rows"": [""....."", "".....""] },
  ""robot"": { ""x"": 0, ""y"": 0, ""facing"": ""east"", ""carrying"": 5 },
  ""drops"": [
    { ""id"": ""petek-1"", ""kind"": ""hive"", ""x"": 2, ""y"": 0, ""need"": 2 },
    { ""id"": ""petek-2"", ""kind"": ""hive"", ""x"": 3, ""y"": 0, ""need"": 2 },
    { ""id"": ""sofra"", ""kind"": ""table"", ""x"": 4, ""y"": 0 }
  ],
  ""goal"": { ""share"": { ""among"": [""petek-1"", ""petek-2""], ""leftoverTo"": ""sofra"" } },
  ""cards"": { ""palette"": [""forward"", ""drop""], ""maxProgramLength"": 10 },
  ""options"": { ""resetEachRun"": true, ""beeMemoryVisible"": true },
  ""voice"": { ""intro"": ""bal_pesinde.l01.intro"" }
}";

        /// <summary>Finishes the whole forest, so the village opens (the region before it must be finished).</summary>
        private void FinishForest(Roboya.CodingEngine.Profiles.AgeBand band = Roboya.CodingEngine.Profiles.AgeBand.Minik, int villageDone = 0)
        {
            var catalog = LevelCatalog.Parse(Directory.GetFiles(FileLevelSource.RepositoryLevelsPath, "*.json", SearchOption.AllDirectories).Select(File.ReadAllText));
            var store = TestProfiles.Seed(_progressDir, band);
            foreach (var entry in catalog.All.Where(e => e.Dto.Region == RegionId.SabirOrmani))
            {
                store.Book.Record(entry.Id, 3);
            }

            for (int order = 1; order <= villageDone; order++)
            {
                store.Book.Record("paylasim-koyu.bal-pesinde." + order.ToString("00"), 3);
            }

            store.Book.MarkShipPartsSeen(99);
            store.Save();
        }

        internal static IEnumerator OpenVillagePath(Action<VisualElement> found)
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
            Assert.AreEqual(2, game.Q("item-amount-bal-1").Query(className: "hive__slot--filled").ToList().Count, "a spot worth two honey shows two discs, so the amount can be counted");
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

        /// <summary>A Kaşif child who finished the Minik part of the village: level 7 is next, in the game scene with the basket full.</summary>
        private IEnumerator OpenKasifLevel(Action<VisualElement> found, int villageDone = 6)
        {
            FinishForest(Roboya.CodingEngine.Profiles.AgeBand.Kasif, villageDone);
            Environment.SetEnvironmentVariable(DevEntitlements.Variable, "1");
            VisualElement map = null;
            yield return OpenVillagePath(r => map = r);
            Tap(map.Q("path-play"));
            VisualElement game = null;
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Game" && (game = FindRoot())?.Q("palette")?.childCount > 0, 10f);
            yield return PassStory(game);
            found(game);
        }

        private static void PlanAndPlay(VisualElement game, params Roboya.CodingEngine.Commands.CardType[] cards)
        {
            foreach (var card in cards)
            {
                Tap(game.Q("palette").Children().OfType<CardElement>().First(c => c.Card == card));
            }

            Tap(game.Q("play"));
        }

        [UnityTest]
        public IEnumerator KasifLevel_DropCardIsInThePaletteAndBigEnough_AndTheRightPlanWins()
        {
            VisualElement game = null;
            yield return OpenKasifLevel(r => game = r);

            var drop = game.Q("palette").Children().OfType<CardElement>().FirstOrDefault(c => c.Card == Roboya.CodingEngine.Commands.CardType.Drop);
            Assert.IsNotNull(drop, "the Bırak card is offered");
            Assert.GreaterOrEqual(drop.resolvedStyle.width, 64f, "child target at least 64 dp");
            Assert.GreaterOrEqual(drop.resolvedStyle.height, 64f);
            Assert.AreEqual(3, game.Q("basket-view").Query(className: "basket-view__dot--filled").ToList().Count, "Ari starts with three honey");
            yield return Capture(game, "21a-village-drop-card");

            var C = Roboya.CodingEngine.Commands.CardType.Forward;
            var D = Roboya.CodingEngine.Commands.CardType.Drop;
            PlanAndPlay(game, C, C, D, D);
            yield return WaitUntil(() => !game.Q("result").ClassListContains("hidden"), 20f);
            Assert.AreEqual(2, game.Q("hive-petek-1").Query(className: "hive__slot--filled").ToList().Count, "the hive got what it asked for");
            Assert.AreEqual(1, game.Q("basket-view").Query(className: "basket-view__dot--filled").ToList().Count, "one honey stays in the basket");
        }

        [UnityTest]
        public IEnumerator KasifLevel_OneDropTooMany_StopsOnThatCardAndTheHiveSpills()
        {
            VisualElement game = null;
            yield return OpenKasifLevel(r => game = r);

            var C = Roboya.CodingEngine.Commands.CardType.Forward;
            var D = Roboya.CodingEngine.Commands.CardType.Drop;
            PlanAndPlay(game, C, C, D, D, D);
            yield return WaitUntil(() => game.Q("hive-petek-1").ClassListContains("piece--hive-spill"), 15f);
            yield return new WaitForSeconds(0.8f);
            yield return Capture(game, "21b-village-spill");

            yield return WaitUntil(() => game.Q("play").enabledSelf, 10f);
            Assert.IsTrue(game.Q("result").ClassListContains("hidden"), "an overflowing run is not a win, and nothing is taken away");
        }

        [UnityTest]
        public IEnumerator KasifLevel_DropWithNothingToDrop_StopsGentlyAndTheChildCanTryAgain()
        {
            VisualElement game = null;
            yield return OpenKasifLevel(r => game = r);

            var C = Roboya.CodingEngine.Commands.CardType.Forward;
            var D = Roboya.CodingEngine.Commands.CardType.Drop;
            // Dropping while still away from the hive: nothing lands anywhere.
            PlanAndPlay(game, D, C, C, D, D);
            yield return WaitUntil(() => game.Q("play").enabledSelf, 15f);
            Assert.IsTrue(game.Q("result").ClassListContains("hidden"));
            Assert.AreEqual(0, game.Q("hive-petek-1").Query(className: "hive__slot--filled").ToList().Count, "the world started over and the hive is empty");
            Assert.AreEqual(3, game.Q("basket-view").Query(className: "basket-view__dot--filled").ToList().Count, "so is the basket");
        }

        [UnityTest]
        public IEnumerator MinikLevel_PaletteHasNoDropCard()
        {
            FinishForest();
            Environment.SetEnvironmentVariable(DevEntitlements.Variable, "1");
            VisualElement map = null;
            yield return OpenVillagePath(r => map = r);
            Tap(map.Q("path-play"));
            VisualElement game = null;
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Game" && (game = FindRoot())?.Q("palette")?.childCount > 0, 10f);

            Assert.IsFalse(game.Q("palette").Children().OfType<CardElement>().Any(c => c.Card == Roboya.CodingEngine.Commands.CardType.Drop), "the hive takes the honey itself");
        }

        private static int Count(VisualElement root, string cls) => root.Query(className: cls).ToList().Count;

        [UnityTest]
        public IEnumerator ShareLevel_GoalBadgeShowsTheSharingHivesWithAnEqualSign()
        {
            VisualElement game = null;
            yield return OpenKasifLevel(r => game = r, villageDone: 7);

            Assert.IsNotNull(game.Q("goal-share-petek-1"));
            Assert.IsNotNull(game.Q("goal-share-petek-2"));
            Assert.IsNotNull(game.Q("goal-equal"), "an equal sign between them");
            yield return Capture(game, "22a-village-share-goal");
        }

        [UnityTest]
        public IEnumerator ShareLevel_ForgottenHive_ShowsTheSadBeeNextToTheGladOne()
        {
            VisualElement game = null;
            yield return OpenKasifLevel(r => game = r, villageDone: 7);

            var F = Roboya.CodingEngine.Commands.CardType.Forward;
            var D = Roboya.CodingEngine.Commands.CardType.Drop;
            PlanAndPlay(game, F, F, D, D);
            yield return WaitUntil(() => game.Q("hive-compare").resolvedStyle.display == DisplayStyle.Flex, 20f);
            yield return new WaitForSeconds(0.4f);
            yield return Capture(game, "22b-village-forgotten");

            Assert.AreEqual(1, Count(game, "hive-compare__villager--sad"), "the hive that got nothing");
            Assert.AreEqual(1, Count(game, "hive-compare__villager--happy"), "the one that got its share");
            Assert.IsNull(game.Q("family-table"), "nothing on the table");
            yield return WaitUntil(() => game.Q("play").enabledSelf, 15f);
            Assert.IsTrue(game.Q("result").ClassListContains("hidden"), "no penalty, the child just tries again");
        }

        [UnityTest]
        public IEnumerator ShareLevel_UnequalHives_TheSmallerOneIsSad()
        {
            VisualElement game = null;
            yield return OpenKasifLevel(r => game = r, villageDone: 7);

            var F = Roboya.CodingEngine.Commands.CardType.Forward;
            var D = Roboya.CodingEngine.Commands.CardType.Drop;
            PlanAndPlay(game, F, F, D, D, F, D);
            yield return WaitUntil(() => game.Q("hive-compare").resolvedStyle.display == DisplayStyle.Flex, 20f);

            Assert.AreEqual(1, Count(game, "hive-compare__villager--sad"));
            Assert.AreEqual(1, Count(game, "hive-compare__villager--happy"));
        }

        [UnityTest]
        public IEnumerator ShareLevel_RightPlan_EveryBeeIsGladAndTheLeftoverIsOnTheTable()
        {
            VisualElement game = null;
            yield return OpenKasifLevel(r => game = r, villageDone: 7);

            var F = Roboya.CodingEngine.Commands.CardType.Forward;
            var D = Roboya.CodingEngine.Commands.CardType.Drop;
            PlanAndPlay(game, F, F, D, D, F, D, D, F, D);
            yield return WaitUntil(() => game.Q("hive-compare").resolvedStyle.display == DisplayStyle.Flex, 30f);
            yield return new WaitForSeconds(0.4f);
            yield return Capture(game, "22c-village-shared");

            Assert.AreEqual(2, Count(game, "hive-compare__villager--thanks"));
            Assert.AreEqual(0, Count(game, "hive-compare__villager--sad"));
            Assert.IsNotNull(game.Q("family-table"), "the left-over honey lies on the table for everyone");
            yield return WaitUntil(() => !game.Q("result").ClassListContains("hidden"), 30f);
        }

        private static int Stars(VisualElement game) => game.Q("stars").Children().OfType<Icon>().Count(i => i.Kind == IconKind.Star);

        /// <summary>Level 7 with the right plan (F F Bırak Bırak): 3 honey, 2 given, 1 left.</summary>
        private IEnumerator WinLevelSevenAndWaitForTheQuestion(Action<VisualElement> found)
        {
            Bootstrap.AskRemainingOverride = true;
            VisualElement game = null;
            yield return OpenKasifLevel(r => game = r);
            var F = Roboya.CodingEngine.Commands.CardType.Forward;
            var D = Roboya.CodingEngine.Commands.CardType.Drop;
            PlanAndPlay(game, F, F, D, D);
            yield return WaitUntil(() => game.Q("remaining-view").resolvedStyle.display == DisplayStyle.Flex, 30f);
            yield return new WaitForSeconds(0.3f);
            found(game);
        }

        [UnityTest]
        public IEnumerator Remaining_SwitchedOff_TheLevelEndsWithoutTheQuestion()
        {
            VisualElement game = null;
            yield return OpenKasifLevel(r => game = r);
            var F = Roboya.CodingEngine.Commands.CardType.Forward;
            var D = Roboya.CodingEngine.Commands.CardType.Drop;
            PlanAndPlay(game, F, F, D, D);

            yield return WaitUntil(() => !game.Q("result").ClassListContains("hidden"), 30f);
            Assert.AreEqual(DisplayStyle.None, game.Q("remaining-view").resolvedStyle.display, "the level asks but the switch is off until the child test");
        }

        [UnityTest]
        public IEnumerator Remaining_RightCard_ShowsThreeDotCardsAndGoesOnWithAllStars()
        {
            VisualElement game = null;
            yield return WinLevelSevenAndWaitForTheQuestion(r => game = r);
            yield return Capture(game, "23a-village-remaining");

            Assert.AreEqual(3, Count(game, "remaining__card"));
            Assert.AreEqual(3, game.Q("remaining-before").Query(className: "remaining__dot").ToList().Count);
            Assert.AreEqual(2, game.Q("remaining-given").Query(className: "remaining__dot").ToList().Count);
            Assert.AreEqual(1, game.Q("remaining-left").Query(className: "remaining__dot").ToList().Count);

            Tap(game.Q("remaining-left"));
            yield return WaitUntil(() => !game.Q("result").ClassListContains("hidden"), 20f);
            Assert.AreEqual(3, Stars(game), "the question does not touch the stars");
        }

        [UnityTest]
        public IEnumerator Remaining_WrongCard_AriCountsTheRightOneAndTheStarsStay()
        {
            VisualElement game = null;
            yield return WinLevelSevenAndWaitForTheQuestion(r => game = r);

            Tap(game.Q("remaining-before"));
            yield return WaitUntil(() => Count(game, "remaining__dot--counted") >= 1, 10f);
            Assert.AreEqual(1, game.Q("remaining-left").Query(className: "remaining__dot--counted").ToList().Count, "the basket's honey is counted together");
            yield return WaitUntil(() => !game.Q("result").ClassListContains("hidden"), 20f);
            Assert.AreEqual(3, Stars(game), "a wrong card costs nothing");
        }

        [UnityTest]
        public IEnumerator Remaining_Arrow_LetsTheChildGoOnWithoutAnswering()
        {
            VisualElement game = null;
            yield return WinLevelSevenAndWaitForTheQuestion(r => game = r);

            Tap(game.Q("remaining-skip"));
            yield return WaitUntil(() => !game.Q("result").ClassListContains("hidden"), 20f);
            Assert.AreEqual(3, Stars(game));
        }
    }
}
