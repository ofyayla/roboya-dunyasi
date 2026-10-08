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

            // ILR-03: the earned propeller is fitted on the island ship and remembered as seen.
            Assert.IsNotNull(map.Q("island-ship").Q("ship-propeller"), "first part on the ship");
            Assert.IsNull(map.Q("island-ship").Q("ship-lights"), "second part not earned yet");
            yield return new WaitForSeconds(1f);
            yield return Capture(map, "09-island-ship");
            Assert.AreEqual(1, new FileProgressStore(_progressDir).Book.ShipPartsSeen, "the drop-in plays once");

            yield return OpenForest(map);
            Tap(map.Q("to-workshop"));
            yield return WaitUntil(() => map.Q("workshop").resolvedStyle.display == DisplayStyle.Flex, 5f);
            yield return new WaitForSeconds(0.3f);

            Assert.IsTrue(map.Q("part-tile-propeller").ClassListContains("workshop__tile--earned"));
            Assert.IsFalse(map.Q("part-tile-lights").ClassListContains("workshop__tile--earned"));
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
