using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using static Roboya.Tests.PlayMode.YonAvcisiFlowTests;

namespace Roboya.Tests.PlayMode
{
    /// <summary>Critical flows of the map (F1-05), local progress (ILR-01/02) and the garage (ILR-03).</summary>
    public class MapFlowTests
    {
        private const string Level = "sabir-ormani.yon-avcisi.";

        private string _progressDir;

        [SetUp]
        public void SetUp()
        {
            _progressDir = Path.Combine(Path.GetTempPath(), "roboya-progress-" + Guid.NewGuid().ToString("N"));
            Bootstrap.ProgressFolderOverride = _progressDir;
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
        public IEnumerator Garage_FiveLevelsDone_FirstPartCanBeWornAndIsSaved()
        {
            Seed(5);
            VisualElement map = null;
            yield return OpenMap(r => map = r);
            yield return OpenForest(map);
            Tap(map.Q("to-garage"));
            yield return WaitUntil(() => map.Q("garage").resolvedStyle.display == DisplayStyle.Flex, 5f);
            yield return null;

            var tile = map.Q("part-tile-antenna-star");
            Assert.IsTrue(tile.ClassListContains("garage__tile--earned"), "a part every 5 levels");
            Assert.IsFalse(map.Q("part-tile-hat-acorn").ClassListContains("garage__tile--earned"));

            Tap(tile);
            yield return null;
            yield return null;
            Assert.IsTrue(tile.ClassListContains("garage__tile--worn"));
            Assert.IsNotNull(map.Q("garage-robot").Q("part-antenna-star"), "Roboya wears it");
            yield return Capture(map, "09-garage");

            Assert.AreEqual("antenna-star", new FileProgressStore(_progressDir).Book.Equipped("antenna"), "saved on the device");
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
        public IEnumerator WornParts_ShowInStorySceneAndOnBoard()
        {
            var store = new FileProgressStore(_progressDir);
            store.Book.Equip("hat", "hat-acorn");
            store.Book.Equip("wings", "wings-leaf");
            store.Book.Equip("antenna", "antenna-star");
            store.Save();
            VisualElement map = null;
            yield return OpenMap(r => map = r);
            yield return OpenForest(map);
            Tap(map.Q("stone-1"));

            VisualElement game = null;
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Game" && (game = FindRoot())?.Q("palette")?.childCount > 0, 10f);
            yield return WaitUntil(() => game.Q<StoryStage>("story").IsOpen, 10f);
            yield return new WaitForSeconds(StoryStage.EnterSeconds + 0.3f);
            var storyRobot = game.Q("story-robot");
            Assert.IsNotNull(storyRobot.Q("part-hat-acorn"), "hat in the story scene");
            Assert.IsNotNull(storyRobot.Q("part-wings-leaf"), "wings in the story scene");
            yield return Capture(game, "10-story-worn");

            Tap(game.Q("story-continue"));
            yield return WaitUntil(() => !game.Q<StoryStage>("story").IsOpen, 5f);
            yield return null;
            Assert.IsNotNull(game.Q("robot").Q("part-hat-acorn"), "hat on the board");
            Assert.IsNotNull(game.Q("robot").Q("part-antenna-star"), "antenna on the board");
            yield return Capture(game, "11-board-worn");
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

        private void Seed(int completed)
        {
            var store = new FileProgressStore(_progressDir);
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
