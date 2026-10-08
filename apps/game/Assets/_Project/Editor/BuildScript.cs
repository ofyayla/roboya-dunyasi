using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Roboya.EditorTools
{
    /// <summary>
    /// Command-line builds (make android-apk). Content and voice are copied by the preprocess steps.
    /// Internal test builds use Unity's debug keystore; store signing comes with the release pipeline (Faz 1).
    /// </summary>
    public static class BuildScript
    {
        public const string Version = "0.1.0";

        public static void BuildAndroidApk()
        {
            bool development = Array.IndexOf(Environment.GetCommandLineArgs(), "-devBuild") >= 0;
            string output = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "Android",
                "roboya-" + Version + (development ? "-dev" : string.Empty) + ".apk"));
            Directory.CreateDirectory(Path.GetDirectoryName(output));

            ProjectSetup.Run();
            PlayerSettings.bundleVersion = Version;
            PlayerSettings.Android.bundleVersionCode = VersionCode();
            EditorUserBuildSettings.buildAppBundle = false;

            var options = new BuildPlayerOptions
            {
                scenes = Array.ConvertAll(EditorBuildSettings.scenes, s => s.path),
                locationPathName = output,
                target = BuildTarget.Android,
                options = development ? BuildOptions.Development : BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            Debug.Log("[Build] " + summary.result + " " + output + " " + (summary.totalSize / (1024 * 1024)) + " MB in " + summary.totalTime);
            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        /// <summary>CI passes its run number; local builds use minutes since 2026-01-01 so codes always increase.</summary>
        private static int VersionCode()
        {
            var fromEnv = Environment.GetEnvironmentVariable("ROBOYA_BUILD_NUMBER");
            if (int.TryParse(fromEnv, out int n))
            {
                return n;
            }

            return (int)((DateTime.UtcNow - new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMinutes);
        }
    }
}
