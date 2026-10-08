using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Roboya.Core
{
    /// <summary>
    /// Device builds: levels are copied into StreamingAssets/levels at build time with an index.json listing
    /// the files. UnityWebRequest is required because Android packs StreamingAssets inside the APK.
    /// </summary>
    public sealed class StreamingAssetsLevelSource : ILevelSource
    {
        public const string Folder = "levels";
        public const string IndexFile = "index.json";

        public async Awaitable<IReadOnlyList<string>> LoadAllAsync()
        {
            var index = JsonUtility.FromJson<LevelIndex>(await ReadAsync(IndexFile));
            var result = new List<string>(index.files.Length);
            foreach (var file in index.files)
            {
                result.Add(await ReadAsync(file));
            }

            return result;
        }

        private static async Awaitable<string> ReadAsync(string relative)
        {
            string url = System.IO.Path.Combine(Application.streamingAssetsPath, Folder, relative);
            if (!url.Contains("://"))
            {
                url = "file://" + url;
            }

            using (var request = UnityWebRequest.Get(url))
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new System.IO.IOException("Cannot read " + relative + ": " + request.error);
                }

                return request.downloadHandler.text;
            }
        }

        [System.Serializable]
        public sealed class LevelIndex
        {
            public string[] files;
        }
    }
}
