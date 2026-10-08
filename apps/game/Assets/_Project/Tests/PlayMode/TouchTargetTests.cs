using System;
using System.Collections;
using System.Collections.Generic;
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
    /// <summary>
    /// OYN-06 / CLAUDE.md §7: every tappable thing on a child screen is at least 64 dp. The panel scales with the
    /// screen from a 1280×800 reference, so panel units are the unit that scales with the device.
    /// </summary>
    public class TouchTargetTests
    {
        private const float MinSize = 64f;

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
        public IEnumerator Map_IslandPathAndGarage_AllTapTargetsAreAtLeast64()
        {
            yield return SceneManager.LoadSceneAsync("Boot");
            VisualElement map = null;
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Map" && (map = FindRoot())?.Q("region-sabir-ormani") != null, 10f);
            yield return null;
            yield return null;
            AssertAll(map, "island", map.Query<VisualElement>(className: "island__region").ToList());

            Tap(map.Q("region-sabir-ormani"));
            yield return WaitUntil(() => map.Q("path").resolvedStyle.display == DisplayStyle.Flex, 5f);
            yield return null;
            yield return null;
            AssertAll(map, "path", map.Query<Button>().ToList().Cast<VisualElement>().Concat(map.Query<VisualElement>(className: "stone").ToList()));

            Tap(map.Q("to-garage"));
            yield return WaitUntil(() => map.Q("garage").resolvedStyle.display == DisplayStyle.Flex, 5f);
            yield return null;
            yield return null;
            AssertAll(map, "garage", map.Query<Button>().ToList().Cast<VisualElement>().Concat(map.Query<VisualElement>(className: "garage__tile").ToList()));
        }

        [UnityTest]
        public IEnumerator Game_BoardScreen_AllTapTargetsAreAtLeast64()
        {
            VisualElement game = null;
            yield return StartGame(r => game = r);
            yield return PassStory(game);
            yield return null;
            yield return null;
            var targets = new List<VisualElement>();
            targets.AddRange(game.Query<Button>().ToList().Where(Visible));
            targets.AddRange(game.Q("palette").Children().OfType<CardElement>());
            targets.AddRange(game.Q("plan").Query<VisualElement>(className: "slot").ToList());
            AssertAll(game, "game", targets);
        }

        private static bool Visible(VisualElement e) => e.resolvedStyle.display != DisplayStyle.None && e.resolvedStyle.visibility == Visibility.Visible;

        private static void AssertAll(VisualElement root, string screen, IEnumerable<VisualElement> targets)
        {
            int checkedCount = 0;
            foreach (var t in targets)
            {
                float w = t.worldBound.width;
                float h = t.worldBound.height;
                if (w <= 0f || h <= 0f)
                {
                    continue;
                }

                // Lock icons inside a region are not targets themselves; the region is.
                checkedCount++;
                Assert.GreaterOrEqual(Mathf.Min(w, h), MinSize, screen + ": '" + (t.name ?? t.GetType().Name) + "' is " + w + "×" + h);
            }

            Assert.Greater(checkedCount, 0, screen + ": found no tap targets to check");
        }
    }
}
