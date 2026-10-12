using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Progress;
using Roboya.CodingEngine.Sharing;
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
    /// The village square (ADR 0031) played through the real screen: no grid, the child taps hives and presses play. Uses the real corner 1
    /// levels in content/levels/paylasim-koyu/koy-meydani; the single-rule cases of the engine are in the EditMode tests.
    /// </summary>
    public class SquareFlowTests
    {
        private string _progressDir;

        [SetUp]
        public void SetUp()
        {
            _progressDir = Path.Combine(Path.GetTempPath(), "roboya-progress-" + Guid.NewGuid().ToString("N"));
            Bootstrap.ProgressFolderOverride = _progressDir;
            Bootstrap.ExtraLevelsOverride = null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Bootstrap.ProgressFolderOverride = null;
            Environment.SetEnvironmentVariable(DevEntitlements.Variable, null);
            foreach (var boot in UnityEngine.Object.FindObjectsByType<Bootstrap>(FindObjectsSortMode.None))
            {
                UnityEngine.Object.Destroy(boot.gameObject);
            }

            yield return null;
        }

        private LevelCatalog FinishForest(int villageDone = 0)
        {
            var files = Directory.GetFiles(FileLevelSource.RepositoryLevelsPath, "*.json", SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.Ordinal);
            var catalog = LevelCatalog.Parse(files.Select(File.ReadAllText));
            var store = TestProfiles.Seed(_progressDir, Roboya.CodingEngine.Profiles.AgeBand.Minik);
            foreach (var entry in catalog.All.Where(e => e.Dto.Region == RegionId.SabirOrmani))
            {
                store.Book.Record(entry.Id, 3);
            }

            for (int order = 1; order <= villageDone; order++)
            {
                store.Book.Record("paylasim-koyu.koy-meydani." + order.ToString("00"), 3);
            }

            store.Book.MarkShipPartsSeen(99);
            store.Save();
            return catalog;
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

        /// <summary>The village is premium content; the editor-only switch stands in for the server's grant (ADR 0009).</summary>
        private IEnumerator OpenFirstSquareLevel(Action<VisualElement> found, bool passStory = true, int villageDone = 0)
        {
            FinishForest(villageDone);
            Environment.SetEnvironmentVariable(DevEntitlements.Variable, "1");
            VisualElement map = null;
            yield return OpenVillagePath(r => map = r);
            Tap(map.Q("path-play"));
            VisualElement game = null;
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Game" && (game = FindRoot())?.Q("square") != null, 10f);
            if (passStory)
            {
                yield return PassStory(game);
            }

            yield return null;
            found(game);
        }

        private static int Filled(VisualElement game, int hive) => game.Q<CombCells>("comb-" + hive).Filled;

        private static int StarCount(VisualElement game) => game.Q("stars").Children().OfType<Icon>().Count(x => x.Kind == IconKind.Star);

        [UnityTest]
        public IEnumerator SquareLevel_Opened_ShowsHivesBasketAndAriAndNoGrid()
        {
            VisualElement game = null;
            yield return OpenFirstSquareLevel(r => game = r, passStory: false);
            yield return null;
            yield return null;
            yield return Capture(game, "25-square-story");
            yield return PassStory(game);
            yield return new WaitForSeconds(0.6f);
            yield return Capture(game, "26-square-start");

            var village = RegionArtLibrary.Load().For(RegionId.PaylasimKoyu, null);
            Assert.AreSame(village.Background, game.Q("screen").resolvedStyle.backgroundImage.sprite, "the level screen shows the village backdrop");
            Assert.IsNotNull(game.Q("hive-0"));
            Assert.IsNotNull(game.Q("hive-1"));
            Assert.IsNull(game.Q("hive-2"), "level 1 has two hives");
            Assert.IsNotNull(game.Q("ari"));
            Assert.IsNotNull(game.Q("basket"));
            Assert.AreEqual(2, game.Q("basket-dots").Query(className: "dot").ToList().Count, "two honey in the basket, told in dots");
            Assert.AreEqual(1, Count(game.Q("hive-0"), "hv__comb"), "one comb per hive");
            Assert.AreEqual(1, game.Q<CombCells>("comb-0").Total, "the hive asks for one honey: one cell");
            Assert.AreEqual(0, Filled(game, 0));
            Assert.AreEqual(DisplayStyle.None, game.Q("palette").resolvedStyle.display, "no card palette in the village");
        }

        private static int Count(VisualElement root, string cls) => root.Query(className: cls).ToList().Count;

        [UnityTest]
        public IEnumerator SquareLevel_CornerFinale_ShowsCombsOfOneOneAndTwoCellsAndFourHoney()
        {
            VisualElement game = null;
            yield return OpenFirstSquareLevel(r => game = r, passStory: false, villageDone: 5);
            yield return PassStory(game);
            yield return new WaitForSeconds(0.6f);
            yield return Capture(game, "33-square-finale");

            Assert.AreEqual("paylasim-koyu.koy-meydani.06", CurrentLevelId());
            Assert.AreEqual(1, game.Q<CombCells>("comb-0").Total);
            Assert.AreEqual(1, game.Q<CombCells>("comb-1").Total);
            Assert.AreEqual(2, game.Q<CombCells>("comb-2").Total);
            Assert.AreEqual(4, game.Q("basket-dots").Query(className: "dot").ToList().Count, "four honey in the basket");
        }

        [UnityTest]
        public IEnumerator SquareLevel_TapHives_AddsACardEachAndTappingACardTakesItOut()
        {
            VisualElement game = null;
            yield return OpenFirstSquareLevel(r => game = r);

            Tap(game.Q("hive-0"));
            yield return null;
            Assert.AreEqual(1, Count(game.Q("plan"), "sc"), "a card for the first hive");
            Tap(game.Q("hive-1"));
            yield return null;
            Assert.AreEqual(2, Count(game.Q("plan"), "sc"));
            yield return Capture(game, "27-square-plan");

            Tap(game.Q("plan").Query(className: "sc").First());
            yield return null;
            Assert.AreEqual(1, Count(game.Q("plan"), "sc"), "tapping a card takes it out");
            Assert.AreEqual(2, Count(game.Q("plan"), "ss"), "and its slot is free again");
        }

        [UnityTest]
        public IEnumerator SquareLevel_GivingToBothHives_FillsTheCellsAndWinsWithThreeStars()
        {
            VisualElement game = null;
            yield return OpenFirstSquareLevel(r => game = r);

            Tap(game.Q("hive-0"));
            Tap(game.Q("hive-1"));
            yield return null;
            Tap(game.Q("play"));

            yield return WaitUntil(() => Filled(game, 0) == 1, 15f);
            yield return Capture(game, "28-square-running");
            yield return WaitUntil(() => Filled(game, 1) == 1, 15f);
            yield return WaitUntil(() => !game.Q("result").ClassListContains("hidden"), 30f);
            yield return null;
            Assert.AreEqual(3, StarCount(game), "the shortest plan on the first try earns three stars");
            Assert.AreEqual(0, game.Q("basket-dots").Query(className: "dot").ToList().Count(d => !d.ClassListContains("dot--empty")), "the basket is empty again");
            yield return Capture(game, "29-square-result");
        }

        [UnityTest]
        public IEnumerator SquareLevel_ForgottenHive_WaitsGentlyAndTheChildCanFixThePlan()
        {
            VisualElement game = null;
            yield return OpenFirstSquareLevel(r => game = r);

            Tap(game.Q("hive-0"));
            yield return null;
            Tap(game.Q("play"));
            yield return WaitUntil(() => game.Q("hive-1").ClassListContains("hv--wait"), 20f);
            yield return new WaitForSeconds(0.4f);
            yield return Capture(game, "30-square-waiting");

            Assert.IsTrue(game.Q("result").ClassListContains("hidden"), "an incomplete run is not a win and nothing is taken away");
            Assert.IsFalse(game.Q("hive-0").ClassListContains("hv--wait"), "only the hive without honey waits");
            Assert.AreEqual(1, Filled(game, 0), "the honey stays where it went so the child can see it");
            yield return WaitUntil(() => game.Q("play").enabledSelf, 10f);

            Tap(game.Q("hive-1"));
            yield return null;
            Assert.IsFalse(game.Q("hive-1").ClassListContains("hv--wait"), "editing starts from a clean scene");
            Assert.AreEqual(0, Filled(game, 0));
            Tap(game.Q("play"));
            yield return WaitUntil(() => !game.Q("result").ClassListContains("hidden"), 30f);
            yield return null;
            Assert.AreEqual(3, StarCount(game), "two runs and the fewest cards still earn three stars: a forgotten hive costs nothing");
        }

        [UnityTest]
        public IEnumerator SquareLevel_FirstLevelOfACorner_HandShowsTheNextStep()
        {
            VisualElement game = null;
            yield return OpenFirstSquareLevel(r => game = r);
            var hand = game.Q("guide-hand");

            yield return WaitUntil(() => hand.resolvedStyle.display == DisplayStyle.Flex, 6f);
            yield return null;
            yield return Capture(game, "31-square-hand");
            var first = game.Q("hive-0").worldBound.center;
            Assert.Less(Vector2.Distance(hand.worldBound.center, first), 160f, "the hand points at the first hive");

            Tap(game.Q("hive-0"));
            Tap(game.Q("hive-1"));
            yield return null;
            yield return null;
            var play = game.Q("play").worldBound.center;
            Assert.Less(Vector2.Distance(hand.worldBound.center, play), 120f, "with the plan right, the hand points at play");
        }

        [UnityTest]
        public IEnumerator SquareScreen_TouchTargets_AreAtLeast64Units()
        {
            VisualElement game = null;
            yield return OpenFirstSquareLevel(r => game = r);
            Tap(game.Q("hive-0"));
            yield return null;

            foreach (var name in new[] { "play", "home", "listen", "clear" })
            {
                var button = game.Q(name);
                Assert.IsNotNull(button, name);
                Assert.GreaterOrEqual(button.resolvedStyle.width, 64f, name + " width");
                Assert.GreaterOrEqual(button.resolvedStyle.height, 64f, name + " height");
            }

            foreach (var card in game.Q("plan").Query(className: "sc").ToList())
            {
                Assert.GreaterOrEqual(card.resolvedStyle.width, 64f, "card width");
                Assert.GreaterOrEqual(card.resolvedStyle.height, 64f, "card height");
            }
        }

        /// <summary>Taps a plan through the real screen: flowers, hives and the table by name, a card's count by tapping its badge.</summary>
        private static IEnumerator TapPlan(VisualElement game, SharingProgram plan)
        {
            foreach (var card in plan.Tail)
            {
                string target = card.Kind == SharingCardKind.Take ? "flower-" + card.Flower : card.IsTable ? "table" : "hive-" + card.Target;
                Tap(game.Q(target));
                yield return null;
                for (int k = 1; card.Kind == SharingCardKind.Give && k < card.Count; k++)
                {
                    Tap(game.Q("plan").Query(className: "sc__count").ToList().Last());
                    yield return null;
                }
            }
        }

        [UnityTest]
        public IEnumerator SquareCorner2_FirstLevel_TakeFromTheFlowerThenGiveTwiceWins()
        {
            VisualElement game = null;
            yield return OpenFirstSquareLevel(r => game = r, villageDone: 6);
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual("paylasim-koyu.koy-meydani.07", CurrentLevelId());
            Assert.IsNotNull(game.Q("flower-0"), "this level has a flower");
            Assert.AreEqual(2, game.Q("flower-0").Query(className: "dot").ToList().Count, "two honey on the flower");
            Assert.AreEqual(0, game.Q("basket-dots").Query(className: "dot").ToList().Count(d => !d.ClassListContains("dot--empty")), "the basket starts empty");
            yield return Capture(game, "34-square-corner2-start");

            Tap(game.Q("flower-0"));
            Tap(game.Q("hive-0"));
            Tap(game.Q("hive-0"));
            yield return null;
            Tap(game.Q("play"));
            yield return WaitUntil(() => !game.Q("result").ClassListContains("hidden"), 40f);
            yield return null;
            Assert.AreEqual(3, StarCount(game));
            Assert.AreEqual(2, Filled(game, 0), "the hive got both honey");
        }

        [UnityTest]
        public IEnumerator SquareCorner2_CountBadge_GoesOneTwoThreeAndOneAgain()
        {
            VisualElement game = null;
            yield return OpenFirstSquareLevel(r => game = r, villageDone: 7);
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual("paylasim-koyu.koy-meydani.08", CurrentLevelId());

            Tap(game.Q("flower-0"));
            Tap(game.Q("hive-0"));
            yield return null;
            Assert.AreEqual(1, game.Q("plan").Query(className: "sc__count").ToList().Count, "a give card carries a count");
            Assert.AreEqual(1, Count(game.Q("plan").Query(className: "sc__count").ToList().Last(), "dot"));
            Tap(game.Q("plan").Query(className: "sc__count").ToList().Last());
            yield return null;
            Assert.AreEqual(2, Count(game.Q("plan").Query(className: "sc__count").ToList().Last(), "dot"));
            Tap(game.Q("plan").Query(className: "sc__count").ToList().Last());
            yield return null;
            Assert.AreEqual(3, Count(game.Q("plan").Query(className: "sc__count").ToList().Last(), "dot"));
            yield return Capture(game, "35-square-corner2-count");
            Tap(game.Q("plan").Query(className: "sc__count").ToList().Last());
            yield return null;
            Assert.AreEqual(1, Count(game.Q("plan").Query(className: "sc__count").ToList().Last(), "dot"), "and back to one");
            Tap(game.Q("plan").Query(className: "sc__count").ToList().Last());
            Tap(game.Q("plan").Query(className: "sc__count").ToList().Last());
            yield return null;
            Tap(game.Q("play"));
            yield return WaitUntil(() => !game.Q("result").ClassListContains("hidden"), 40f);
            yield return null;
            Assert.AreEqual(3, StarCount(game), "three honey in one card is the shortest plan");
        }

        [UnityTest]
        public IEnumerator SquareCorner2_BasketOfTwo_TakingBothFlowersStopsGentlyWithTheBasketFull()
        {
            VisualElement game = null;
            yield return OpenFirstSquareLevel(r => game = r, villageDone: 9);
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual("paylasim-koyu.koy-meydani.10", CurrentLevelId());
            Assert.AreEqual(2, game.Q("basket-dots").Query(className: "dot").ToList().Count, "the basket holds two: two places");

            Tap(game.Q("flower-0"));
            Tap(game.Q("flower-1"));
            Tap(game.Q("hive-0"));
            yield return null;
            Tap(game.Q("play"));
            yield return WaitUntil(() => game.Q("plan").Query(className: "sc--stop").ToList().Count == 1, 30f);
            yield return new WaitForSeconds(0.6f);
            yield return Capture(game, "36-square-corner2-basket-full");
            Assert.IsTrue(game.Q("result").ClassListContains("hidden"), "a stopped plan is not a win");
            Assert.AreEqual(2, game.Q("basket-dots").Query(className: "dot").ToList().Count(d => !d.ClassListContains("dot--empty")), "the first flower filled the basket");
            Assert.AreEqual(1, game.Q("flower-1").Query(className: "dot").ToList().Count, "the second flower kept its honey");
            yield return WaitUntil(() => game.Q("play").enabledSelf, 10f);
        }

        // The whole village path through the UI: the development run plays the first levels of each part, ROBOYA_E2E=1 all of them.
        [Timeout(1800000)]
        [UnityTest]
        public IEnumerator SquareVillage_SolverPlanTappedThroughUi_EachLevelCompletes()
        {
            var catalog = FinishForest();
            var all = catalog.All.Where(l => l.Dto.Region == RegionId.PaylasimKoyu).ToList();
            Assert.AreEqual(12, all.Count, "corners 1 and 2: twelve levels");
            yield return WalkAsync(all.Take(LevelsToPlay(6)).ToList(), 0, all.Count);
        }

        [Timeout(1800000)]
        [UnityTest]
        public IEnumerator SquareCorner2_SolverPlanTappedThroughUi_EachLevelCompletes()
        {
            var catalog = FinishForest(villageDone: 6);
            var all = catalog.All.Where(l => l.Dto.Region == RegionId.PaylasimKoyu).ToList();
            yield return WalkAsync(all.Skip(6).Take(LevelsToPlay(6)).ToList(), 6, all.Count);
        }

        private IEnumerator WalkAsync(System.Collections.Generic.List<LevelEntry> levels, int firstIndex, int pathCount)
        {
            Environment.SetEnvironmentVariable(DevEntitlements.Variable, "1");
            VisualElement map = null;
            yield return OpenVillagePath(r => map = r);
            Tap(map.Q("path-play"));
            VisualElement game = null;
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Game" && (game = FindRoot())?.Q("square") != null, 10f);

            for (int i = 0; i < levels.Count; i++)
            {
                var entry = levels[i];
                var plan = SharingSolver.Solve(entry.Sharing).Programs[0];
                Debug.Log("[test] playing " + entry.Id);
                yield return PassStory(game);
                yield return new WaitForSeconds(0.3f);
                yield return TapPlan(game, plan);
                Tap(game.Q("play"));
                if (CornerRules.IsCornerEnd(firstIndex + i, pathCount))
                {
                    yield return WaitUntil(() => game.Q("value-card") != null && !game.Q("value-card").ClassListContains("hidden"), 90f);
                    yield return new WaitForSeconds(1f);
                    Tap(game.Q("value-card"));
                }

                yield return WaitUntil(() => !game.Q("result").ClassListContains("hidden"), 40f);
                yield return null;
                Assert.AreEqual(3, StarCount(game), entry.Id + " should earn 3 stars with the shortest plan");
                if (i == levels.Count - 1)
                {
                    yield return new WaitForSeconds(0.4f);
                    yield return Capture(game, "32-square-last-level-result");
                    break;
                }

                Tap(game.Q("next"));
                yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Map" && (map = FindRoot())?.Q("path") != null, 10f);
                yield return null;
                if (map.Q("path").resolvedStyle.display != DisplayStyle.Flex)
                {
                    Tap(map.Q("region-paylasim-koyu"));
                    yield return WaitUntil(() => map.Q("path").resolvedStyle.display == DisplayStyle.Flex, 5f);
                    yield return null;
                }

                int stone = firstIndex + i + 2;
                map.Q("path").Q<ScrollView>().ScrollTo(map.Q("stone-" + stone));
                yield return null;
                yield return null;
                Tap(map.Q("stone-" + stone));
                yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Game" && (game = FindRoot())?.Q("square") != null, 10f);
            }
        }
    }
}
