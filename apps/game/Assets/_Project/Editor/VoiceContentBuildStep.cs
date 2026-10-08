using System.IO;
using Roboya.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Roboya.EditorTools
{
    /// <summary>Copies content/voice (manifest + audio) into StreamingAssets/voice before every player build (ADR 0006).</summary>
    public sealed class VoiceContentBuildStep : IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;

        public void OnPreprocessBuild(BuildReport report) => Copy();

        [MenuItem("Roboya/Copy Voice To StreamingAssets")]
        public static void Copy()
        {
            string source = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "..", "content", "voice"));
            string manifest = Path.Combine(source, VoiceLibrary.ManifestFile);
            if (!File.Exists(manifest))
            {
                throw new BuildFailedException("Voice manifest not found at " + manifest + " (run: make tts)");
            }

            string target = Path.Combine(Application.streamingAssetsPath, VoiceLibrary.StreamingFolder);
            if (Directory.Exists(target))
            {
                Directory.Delete(target, true);
            }

            Directory.CreateDirectory(target);
            File.Copy(manifest, Path.Combine(target, VoiceLibrary.ManifestFile));
            int count = 0;
            foreach (var file in VoiceLibrary.Parse(File.ReadAllText(manifest)).Values)
            {
                string dest = Path.Combine(target, file);
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                File.Copy(Path.Combine(source, file), dest);
                count++;
            }

            AssetDatabase.Refresh();
            Debug.Log("[VoiceContent] copied " + count + " lines to StreamingAssets");
        }
    }
}
