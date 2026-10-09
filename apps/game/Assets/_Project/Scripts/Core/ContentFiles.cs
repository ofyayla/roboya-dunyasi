using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Roboya.Core
{
    /// <summary>
    /// Small app content outside levels and voice (content/map, content/rewards). The editor reads the repository;
    /// device builds read StreamingAssets/content, copied by ContentBuildStep (ADR 0006).
    /// </summary>
    public static class ContentFiles
    {
        public const string StreamingFolder = "content";

        /// <summary>Folders under content/ that ship with the app.</summary>
        public static readonly string[] ShippedFolders = { "map", "rewards", "localization", "legal", "store", "video" };

        public static string RepositoryContentPath =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "..", "content"));

        /// <summary>A playable URL for a shipped content file (video): the repository file in the editor, StreamingAssets on a device. Null when missing.</summary>
        public static string UrlOf(string relative)
        {
#if UNITY_EDITOR
            string local = Path.Combine(RepositoryContentPath, relative);
            return File.Exists(local) ? "file://" + local : null;
#else
            string url = Path.Combine(Application.streamingAssetsPath, StreamingFolder, relative);
            return url.Contains("://") ? url : "file://" + url;
#endif
        }

        public static async Awaitable<string> ReadAsync(string relative)
        {
#if UNITY_EDITOR
            string local = Path.Combine(RepositoryContentPath, relative);
            if (File.Exists(local))
            {
                return File.ReadAllText(local);
            }
#endif
            string url = Path.Combine(Application.streamingAssetsPath, StreamingFolder, relative);
            if (!url.Contains("://"))
            {
                url = "file://" + url;
            }

            using (var request = UnityWebRequest.Get(url))
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new IOException("Cannot read content " + relative + ": " + request.error);
                }

                return request.downloadHandler.text;
            }
        }
    }
}
