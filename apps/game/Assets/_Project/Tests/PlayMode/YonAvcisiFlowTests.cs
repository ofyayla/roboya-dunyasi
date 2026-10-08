using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Roboya.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Roboya.Tests.PlayMode
{
    /// <summary>Critical flow (CLAUDE.md §7): open level → story → build plan → play → finish with stars.</summary>
    public class YonAvcisiFlowTests
    {
        private static readonly string ShotsDir = Path.Combine(Application.dataPath, "..", "TestResults", "screens");

        private string _progressDir;

        [SetUp]
        public void SetUp()
        {
            // Every test starts with a fresh local profile.
            _progressDir = Path.Combine(Path.GetTempPath(), "roboya-progress-" + Guid.NewGuid().ToString("N"));
            Roboya.Core.Bootstrap.ProgressFolderOverride = _progressDir;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Environment.SetEnvironmentVariable(Roboya.Core.DevEntitlements.Variable, null);
            Roboya.Core.Bootstrap.ProgressFolderOverride = null;
            foreach (var boot in UnityEngine.Object.FindObjectsByType<Roboya.Core.Bootstrap>(FindObjectsSortMode.None))
            {
                UnityEngine.Object.Destroy(boot.gameObject);
            }

            yield return null;
        }

        /// <summary>Boot → island → Patience Forest → first stone → Game scene.</summary>
        private static IEnumerator StartGame(Action<VisualElement> found)
        {
            yield return SceneManager.LoadSceneAsync("Boot");
            VisualElement map = null;
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "Map" && (map = FindRoot())?.Q("region-sabir-ormani") != null, 10f);
            yield return null;
            Tap(map.Q("region-sabir-ormani"));
            yield return WaitUntil(() => map.Q("path").resolvedStyle.display == DisplayStyle.Flex, 5f);
            yield return null;
            Tap(map.Q("stone-1"));
            VisualElement root = null;
            yield return WaitUntil(
                () => SceneManager.GetActiveScene().name == "Game" && (root = FindRoot()) != null && root.Q("palette")?.childCount > 0,
                10f);
            found(root);
        }

        [UnityTest]
        public IEnumerator FirstLevel_TwoForwardCardsAndPlay_ShowsThreeStars()
        {
            VisualElement root = null;
            yield return StartGame(r => root = r);
            yield return null;
            AudioSource voice = null;
            yield return WaitUntil(
                () => (voice = UnityEngine.Object.FindAnyObjectByType<AudioSource>()) != null && voice.clip != null
                    && voice.clip.name == "yon_avcisi.l01.intro", // the map's welcome line may play first
                10f);
            Assert.AreEqual("yon_avcisi.l01.intro", voice.clip.name, "intro narration is loaded by key");
            Assert.Greater(voice.clip.length, 1f);
            yield return PassStory(root, "01-level1-story");
            yield return Capture(root, "01-level1-start");

            var forward = root.Q("palette").Children().First();
            Tap(forward);
            Tap(forward);
            yield return null;
            Assert.AreEqual(2, root.Q("plan").Query(className: "card").ToList().Count, "two cards on the plan strip");
            yield return Capture(root, "02-level1-planned");

            Tap(root.Q("play"));
            yield return WaitUntil(() => !root.Q("result").ClassListContains("hidden"), 15f);
            yield return null; // the overlay is laid out one frame after it becomes visible
            Assert.IsTrue(Story(root).IsOpen, "the outro scene stays behind the result");
            yield return new WaitForSeconds(0.4f);
            yield return Capture(root, "03-level1-result");

            var stars = root.Q("stars").Children().OfType<Icon>().Count(i => i.Kind == IconKind.Star);
            Assert.AreEqual(3, stars);

            Tap(root.Q("next"));
            yield return null;
            Assert.IsTrue(root.Q("result").ClassListContains("hidden"));
            yield return PassStory(root);
            Assert.AreEqual(0, root.Q("plan").Query(className: "card").ToList().Count);
            var dots = root.Q("progress").Children().ToList();
            Assert.IsTrue(dots[1].ClassListContains("progress__dot--current"), "next button opens level 2");
        }

        [UnityTest]
        public IEnumerator FirstLevel_TooShortPlan_ReturnsToPlanningWithoutPenalty()
        {
            VisualElement root = null;
            yield return StartGame(r => root = r);
            yield return PassStory(root);

            Tap(root.Q("palette").Children().First());
            Tap(root.Q("play"));
            yield return new WaitForSeconds(0.2f);
            Assert.IsFalse(root.Q("play").enabledSelf, "plan is locked while running");
            yield return WaitUntil(() => root.Q("play").enabledSelf, 10f);

            Assert.IsTrue(root.Q("result").ClassListContains("hidden"));
            Assert.AreEqual(1, root.Q("plan").Query(className: "card").ToList().Count, "plan is kept so the child can fix it");
        }

        [UnityTest]
        public IEnumerator AllPrototypeLevels_SolverSolutionTappedThroughUi_EachCompletes()
        {
            var files = Directory.GetFiles(Roboya.Core.FileLevelSource.RepositoryLevelsPath, "*.json", SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.Ordinal);
            var catalog = Roboya.Core.LevelCatalog.Parse(files.Select(File.ReadAllText));
            var levels = catalog.ForGame(Roboya.CodingEngine.Levels.Generated.GameId.YonAvcisi);

            // Levels past the free tier need premium; the editor-only switch stands in for the server (ADR 0009).
            Environment.SetEnvironmentVariable(Roboya.Core.DevEntitlements.Variable, "1");
            VisualElement root = null;
            yield return StartGame(r => root = r);

            for (int i = 0; i < levels.Count; i++)
            {
                var solution = Roboya.CodingEngine.Solving.Solver.Solve(levels[i].Level).Solution;
                yield return PassStory(root, StoryShot(i), waitForCard: i == 2);
                if (i == 3 || i == 6)
                {
                    yield return Capture(root, "06-level" + (i + 1) + "-board"); // looks and scenery match the narration
                }

                if (i == 8)
                {
                    yield return Capture(root, "04-level9-start");
                }

                foreach (var card in solution)
                {
                    var paletteCard = root.Q("palette").Children().OfType<CardElement>().FirstOrDefault(c => c.Card == card);
                    Assert.IsNotNull(paletteCard, levels[i].Id + ": palette has no " + card + " (palette: " +
                        string.Join(",", root.Q("palette").Children().OfType<CardElement>().Select(c => c.Card)) + ")");
                    Tap(paletteCard);
                }

                Tap(root.Q("play"));
                yield return WaitUntil(() => !root.Q("result").ClassListContains("hidden"), 30f);
                yield return null; // the overlay is laid out one frame after it becomes visible
                int stars = root.Q("stars").Children().OfType<Icon>().Count(x => x.Kind == IconKind.Star);
                Assert.AreEqual(3, stars, levels[i].Id + " should earn 3 stars with the shortest plan on the first try");
                if (i == 4)
                {
                    // ILR-03: the fifth level earns the first robot part, shown after the success line.
                    yield return WaitUntil(() => root.Q("story-reward") != null, 20f);
                    yield return new WaitForSeconds(0.6f);
                    yield return Capture(root, "05-level5-reward");
                }

                if (i == levels.Count - 1)
                {
                    yield return new WaitForSeconds(0.4f);
                    yield return Capture(root, "05-level10-result");
                }
                else
                {
                    Tap(root.Q("next"));
                    yield return null;
                }
            }
        }

        [UnityTest]
        public IEnumerator FirstLevel_RetryAfterSuccess_SkipsStoryAndKeepsBoard()
        {
            VisualElement root = null;
            yield return StartGame(r => root = r);
            yield return PassStory(root);

            var forward = root.Q("palette").Children().First();
            Tap(forward);
            Tap(forward);
            Tap(root.Q("play"));
            yield return WaitUntil(() => !root.Q("result").ClassListContains("hidden"), 15f);
            yield return null;

            Tap(root.Q("retry"));
            yield return new WaitForSeconds(0.5f);

            Assert.IsFalse(Story(root).IsOpen, "retry goes straight to the board");
            Assert.IsTrue(root.Q("result").ClassListContains("hidden"));
            Assert.IsTrue(root.Q("progress").Children().First().ClassListContains("progress__dot--current"), "same level");
        }

        private static StoryStage Story(VisualElement root) => root.Q<StoryStage>("story");

        private static string StoryShot(int levelIndex)
        {
            switch (levelIndex)
            {
                case 2: return "04-level3-new-card";
                case 3: return "04-level4-story";
                case 6: return "04-level7-story";
                case 7: return "04-level8-story";
                default: return null;
            }
        }

        /// <summary>Waits for the intro scene, optionally captures it, then taps continue and waits for the board.</summary>
        internal static IEnumerator PassStory(VisualElement root, string shot = null, bool waitForCard = false)
        {
            var story = Story(root);
            Assert.IsNotNull(story, "story stage exists");
            yield return WaitUntil(() => story.IsOpen, 10f);
            yield return new WaitForSeconds(StoryStage.EnterSeconds + 0.2f);
            if (waitForCard)
            {
                // YON-01: the new card pops up after the level line, with its own narration.
                var card = root.Q("story-card");
                yield return WaitUntil(() => card.resolvedStyle.display == DisplayStyle.Flex, 20f);
                Assert.IsNotNull(card.Q<CardElement>(), "the introduced card is shown");
                yield return new WaitForSeconds(0.8f);
            }

            if (shot != null)
            {
                yield return Capture(root, shot);
            }

            Tap(root.Q("story-continue"));
            yield return WaitUntil(() => !story.IsOpen, 5f);
        }

        internal static VisualElement FindRoot()
        {
            var doc = UnityEngine.Object.FindAnyObjectByType<UIDocument>();
            return doc != null ? doc.rootVisualElement : null;
        }

        internal static void Tap(VisualElement element)
        {
            // Events sent to the panel's visual tree are hit-tested in panel coordinates.
            Vector2 pos = element.worldBound.center;
            Send(element, EventType.MouseDown, pos);
            Send(element, EventType.MouseUp, pos);
        }

        private static void Send(VisualElement element, EventType type, Vector2 pos)
        {
            var e = new Event { type = type, mousePosition = pos, button = 0, clickCount = 1 };
            EventBase evt = type == EventType.MouseDown ? PointerDownEvent.GetPooled(e) : (EventBase)PointerUpEvent.GetPooled(e);
            using (evt)
            {
                element.panel.visualTree.SendEvent(evt);
            }
        }

        internal static IEnumerator WaitUntil(Func<bool> condition, float timeout)
        {
            float end = Time.realtimeSinceStartup + timeout;
            while (!condition())
            {
                if (Time.realtimeSinceStartup > end)
                {
                    Assert.Fail("Timed out after " + timeout + "s");
                }

                yield return null;
            }
        }

        /// <summary>
        /// Renders the UI panel into a texture and saves a PNG for visual review. WaitForEndOfFrame never fires in
        /// batch mode, so the panel is redirected to a RenderTexture for two frames instead. Skipped without a GPU.
        /// </summary>
        internal static IEnumerator Capture(VisualElement root, string name)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                yield break;
            }

            var settings = UnityEngine.Object.FindAnyObjectByType<UIDocument>().panelSettings;
            var rt = new RenderTexture(1280, 800, 24, RenderTextureFormat.ARGB32);
            settings.targetTexture = rt;
            yield return null;
            yield return null;

            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            var shot = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            shot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            shot.Apply();
            RenderTexture.active = previous;
            settings.targetTexture = null;

            Directory.CreateDirectory(ShotsDir);
            File.WriteAllBytes(Path.Combine(ShotsDir, name + ".png"), shot.EncodeToPNG());
            UnityEngine.Object.Destroy(shot);
            rt.Release();
            UnityEngine.Object.Destroy(rt);
            yield return null;
        }
    }
}
