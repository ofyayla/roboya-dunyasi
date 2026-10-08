using System.IO;
using Roboya.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Roboya.EditorTools
{
    /// <summary>Copies the shipped app content (map, rewards, localization, legal) into StreamingAssets/content before builds.</summary>
    public sealed class ContentBuildStep : IPreprocessBuildWithReport
    {
        public int callbackOrder => 2;

        public void OnPreprocessBuild(BuildReport report) => Copy();

        [MenuItem("Roboya/Copy App Content To StreamingAssets")]
        public static void Copy()
        {
            string target = Path.Combine(Application.streamingAssetsPath, ContentFiles.StreamingFolder);
            if (Directory.Exists(target))
            {
                Directory.Delete(target, true);
            }

            int count = 0;
            foreach (var folder in ContentFiles.ShippedFolders)
            {
                string source = Path.Combine(ContentFiles.RepositoryContentPath, folder);
                if (!Directory.Exists(source))
                {
                    throw new BuildFailedException("App content not found at " + source);
                }

                Directory.CreateDirectory(Path.Combine(target, folder));
                // Content is JSON; the privacy notice is a markdown file.
                foreach (var file in Directory.GetFiles(source))
                {
                    string ext = Path.GetExtension(file);
                    if (ext == ".json" || ext == ".md")
                    {
                        File.Copy(file, Path.Combine(target, folder, Path.GetFileName(file)));
                        count++;
                    }
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("[AppContent] copied " + count + " files to StreamingAssets");
        }
    }
}
