using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Roboya.EditorTools
{
    /// <summary>
    /// Applies project settings and creates the thin scenes from code, so settings are reviewable in Git
    /// and never hand-edited in YAML (CLAUDE.md §7). Idempotent; safe to re-run.
    /// Batch: Unity -batchmode -projectPath apps/game -executeMethod Roboya.EditorTools.ProjectSetup.RunBatch -quit
    /// </summary>
    public static class ProjectSetup
    {
        public const string BundleId = "com.roboyakids.roboyadunyasi";
        private const string ScenesDir = "Assets/_Project/Scenes";
        private const string SettingsDir = "Assets/_Project/Settings";
        private static readonly string[] SceneNames = { "Boot", "Map", "Game" };

        [MenuItem("Roboya/Project Setup")]
        public static void Run()
        {
            ApplyPlayerSettings();
            ApplyPrivacySettings();
            ApplyEditorSettings();
            EnsureRenderPipeline();
            EnsureScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[ProjectSetup] done");
        }

        public static void RunBatch()
        {
            Run();
            EditorApplication.Exit(0);
        }

        private static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "Roboya Kids";
            PlayerSettings.productName = "Roboya Dünyası";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BundleId);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId);

            // Tablet-first, landscape only.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            // Android 9+ (PRD); 2 GB school tablets are often 32-bit, so ship ARMv7 and ARM64.
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel28;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.targetOSVersionString = "16.0";

            // Unity 6 allows hiding the splash on all plans; the app opens with Roboya's own intro.
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;

            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.runInBackground = false;
        }

        private static void ApplyPrivacySettings()
        {
            // Kids category: no engine analytics, crash reporting or performance reporting (UYM-06).
            PlayerSettings.enableCrashReportAPI = false;
            UnityEditor.Analytics.AnalyticsSettings.enabled = false;
            UnityEditor.CrashReporting.CrashReportingSettings.enabled = false;
            UnityEditor.Analytics.PerformanceReportingSettings.enabled = false;
            PlayerSettings.iOS.cameraUsageDescription = string.Empty;
            PlayerSettings.iOS.locationUsageDescription = string.Empty;
            PlayerSettings.iOS.microphoneUsageDescription = string.Empty;
        }

        private static void ApplyEditorSettings()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            VersionControlSettings.mode = "Visible Meta Files";
            EditorSettings.enterPlayModeOptionsEnabled = true;
        }

        private static void EnsureRenderPipeline()
        {
            Directory.CreateDirectory(SettingsDir);
            string rendererPath = SettingsDir + "/Renderer2D.asset";
            string pipelinePath = SettingsDir + "/URP-2D.asset";

            var renderer = AssetDatabase.LoadAssetAtPath<Renderer2DData>(rendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<Renderer2DData>();
                AssetDatabase.CreateAsset(renderer, rendererPath);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
            }

            // Low-end tablets: no MSAA, no HDR.
            pipeline.msaaSampleCount = 1;
            pipeline.supportsHDR = false;
            EditorUtility.SetDirty(pipeline);

            GraphicsSettings.defaultRenderPipeline = pipeline;
            for (int i = 0; i < QualitySettings.count; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
        }

        private static void EnsureScenes()
        {
            Directory.CreateDirectory(ScenesDir);
            var buildScenes = new EditorBuildSettingsScene[SceneNames.Length];
            for (int i = 0; i < SceneNames.Length; i++)
            {
                string path = ScenesDir + "/" + SceneNames[i] + ".unity";
                if (!File.Exists(path))
                {
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                    EditorSceneManager.SaveScene(scene, path);
                }

                buildScenes[i] = new EditorBuildSettingsScene(path, true);
            }

            EditorBuildSettings.scenes = buildScenes;
        }
    }
}
