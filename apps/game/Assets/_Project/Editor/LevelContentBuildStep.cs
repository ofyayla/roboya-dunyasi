using System;
using System.IO;
using Roboya.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Roboya.EditorTools
{
    /// <summary>
    /// Copies content/levels into StreamingAssets/levels with an index.json before every player build, so device
    /// builds carry the validated levels without committing copies (ADR 0006). Addressables replaces this for
    /// downloadable region packs in Faz 1.
    /// </summary>
    public sealed class LevelContentBuildStep : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report) => Copy();

        [MenuItem("Roboya/Copy Levels To StreamingAssets")]
        public static void Copy()
        {
            string source = FileLevelSource.RepositoryLevelsPath;
            string target = Path.Combine(Application.streamingAssetsPath, StreamingAssetsLevelSource.Folder);
            if (!Directory.Exists(source))
            {
                throw new BuildFailedException("Level content not found at " + source);
            }

            if (Directory.Exists(target))
            {
                Directory.Delete(target, true);
            }

            Directory.CreateDirectory(target);
            var files = Directory.GetFiles(source, "*.json", SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.Ordinal);
            var index = new StreamingAssetsLevelSource.LevelIndex { files = new string[files.Length] };
            for (int i = 0; i < files.Length; i++)
            {
                string name = i.ToString("D4") + ".json";
                File.Copy(files[i], Path.Combine(target, name));
                index.files[i] = name;
            }

            File.WriteAllText(Path.Combine(target, StreamingAssetsLevelSource.IndexFile), JsonUtility.ToJson(index));
            AssetDatabase.Refresh();
            Debug.Log("[LevelContent] copied " + files.Length + " levels to StreamingAssets");
        }
    }
}
