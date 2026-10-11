using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Progress;
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
    /// The real Paylaşım Köyü content (plan E1): every level of the village path is solved through the UI with the solver's shortest
    /// plan, the way <c>YonAvcisiFlowTests</c> walks the forest. VillageFlowTests keeps hand-made fixtures for the single-rule cases.
    /// </summary>
    public class VillageContentFlowTests
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

        // The whole 24-level path takes minutes through the UI: the development run plays the first levels, ROBOYA_E2E=1 all of them.
        [Timeout(900000)]
        [UnityTest]
        public IEnumerator AllVillageLevels_SolverPlanTappedThroughUi_EachCompletesWithThreeStars()
        {
            var files = Directory.GetFiles(FileLevelSource.RepositoryLevelsPath, "*.json", SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.Ordinal);
            var catalog = LevelCatalog.Parse(files.Select(File.ReadAllText));
            var store = TestProfiles.Seed(_progressDir, Roboya.CodingEngine.Profiles.AgeBand.Minik);
            foreach (var entry in catalog.All.Where(e => e.Dto.Region == RegionId.SabirOrmani))
            {
                store.Book.Record(entry.Id, 3);
            }

            store.Book.MarkShipPartsSeen(99);
            store.Save();

            var levels = catalog.All.Where(l => l.Dto.Region == RegionId.PaylasimKoyu).ToList();
            Assert.AreEqual(24, levels.Count, "the village is 24 levels: four corners of six");
            int pathCount = levels.Count;
            levels = levels.Take(LevelsToPlay(levels.Count)).ToList();

            // The village is premium content; the editor-only switch stands in for the server's grant (ADR 0009).
            Environment.SetEnvironmentVariable(DevEntitlements.Variable, "1");
            VisualElement map = null;
            yield return VillageFlowTests.OpenVillagePath(r => map = r);
            Tap(map.Q("path-play"));
            VisualElement root = null;
            yield return WaitUntil(
                () => SceneManager.GetActiveScene().name == "Game" && (root = FindRoot()) != null && root.Q("palette")?.childCount > 0,
                10f);

            for (int i = 0; i < levels.Count; i++)
            {
                var entry = levels[i];
                var solution = Roboya.CodingEngine.Solving.Solver.Solve(entry.Level).Solution;
                Debug.Log("[test] playing " + entry.Id);
                yield return PassStory(root, null, waitForCard: entry.Dto.Cards.Introduces != null);

                foreach (var card in solution)
                {
                    var paletteCard = root.Q("palette").Children().OfType<CardElement>().FirstOrDefault(c => c.Card == card);
                    Assert.IsNotNull(paletteCard, entry.Id + ": palette has no " + card + " (palette: " +
                        string.Join(",", root.Q("palette").Children().OfType<CardElement>().Select(c => c.Card)) + ")");
                    Tap(paletteCard);
                }

                Tap(root.Q("play"));
                if (CornerRules.IsCornerEnd(i, pathCount))
                {
                    // The end of a village corner earns a ship part and closes with the sharing value moment (no film yet: the calm card).
                    yield return WaitUntil(() => root.Q("story-reward") != null, 30f);
                    yield return WaitUntil(() => !root.Q("value-card").ClassListContains("hidden"), 60f);
                    yield return new WaitForSeconds(1f);
                    Tap(root.Q("value-card"));
                }

                yield return WaitUntil(() => !root.Q("result").ClassListContains("hidden"), 30f);
                yield return null; // the overlay is laid out one frame after it becomes visible
                int stars = root.Q("stars").Children().OfType<Icon>().Count(x => x.Kind == IconKind.Star);
                Assert.AreEqual(3, stars, entry.Id + " should earn 3 stars with the shortest plan on the first try");
                if (i == levels.Count - 1)
                {
                    yield return new WaitForSeconds(0.4f);
                    yield return Capture(root, "24-village-last-level-result");
                    break;
                }

                Tap(root.Q("next"));
                yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Map" && (map = FindRoot())?.Q("path") != null, 10f);
                yield return null;
                if (map.Q("path").resolvedStyle.display != DisplayStyle.Flex)
                {
                    // After an earned ship part the map may open on the island: walk back into the village.
                    Tap(map.Q("region-paylasim-koyu"));
                    yield return WaitUntil(() => map.Q("path").resolvedStyle.display == DisplayStyle.Flex, 5f);
                    yield return null;
                }

                map.Q("path").Q<ScrollView>().ScrollTo(map.Q("stone-" + (i + 2)));
                yield return null;
                yield return null;
                Tap(map.Q("stone-" + (i + 2)));
                yield return WaitUntil(
                    () => SceneManager.GetActiveScene().name == "Game" && (root = FindRoot()) != null && root.Q("palette")?.childCount > 0,
                    10f);
            }
        }
    }
}
