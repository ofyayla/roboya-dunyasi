using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Roboya.Core
{
    /// <summary>
    /// Maps narration keys to audio files through content/voice/manifest.json (written by tools/tts).
    /// The same keys later point at studio recordings; no code changes (CLAUDE.md §10).
    /// </summary>
    public sealed class VoiceLibrary
    {
        public const string StreamingFolder = "voice";
        public const string ManifestFile = "manifest.json";

        private readonly string _baseUrl;
        private readonly Dictionary<string, string> _files;
        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();

        private VoiceLibrary(string baseUrl, Dictionary<string, string> files)
        {
            _baseUrl = baseUrl;
            _files = files;
        }

        public int Count => _files.Count;

        /// <summary>Editor: repository content/voice. Device: StreamingAssets/voice (copied at build).</summary>
        public static string DefaultBaseUrl
        {
            get
            {
#if UNITY_EDITOR
                return ToUrl(System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", "..", "..", "content", "voice")));
#else
                return ToUrl(System.IO.Path.Combine(Application.streamingAssetsPath, StreamingFolder));
#endif
            }
        }

        public static async Awaitable<VoiceLibrary> LoadAsync(string baseUrl)
        {
            using (var request = UnityWebRequest.Get(baseUrl + "/" + ManifestFile))
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new System.IO.IOException("Voice manifest not readable: " + request.error);
                }

                return new VoiceLibrary(baseUrl, Parse(request.downloadHandler.text));
            }
        }

        public static Dictionary<string, string> Parse(string manifestJson)
        {
            var files = new Dictionary<string, string>();
            var entries = JObject.Parse(manifestJson)["entries"] as JObject;
            if (entries != null)
            {
                foreach (var p in entries.Properties())
                {
                    var file = p.Value.Value<string>("file");
                    if (!string.IsNullOrEmpty(file))
                    {
                        files[p.Name] = file;
                    }
                }
            }

            return files;
        }

        public bool Has(string key) => key != null && _files.ContainsKey(key);

        /// <summary>Returns the cached clip or loads it; null when the key has no audio.</summary>
        public async Awaitable<AudioClip> GetAsync(string key)
        {
            if (!Has(key))
            {
                return null;
            }

            if (_clips.TryGetValue(key, out var cached))
            {
                return cached;
            }

            using (var request = UnityWebRequestMultimedia.GetAudioClip(_baseUrl + "/" + _files[key], AudioType.MPEG))
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("Voice line '" + key + "' failed to load: " + request.error);
                    return null;
                }

                var clip = DownloadHandlerAudioClip.GetContent(request);
                clip.name = key;
                _clips[key] = clip;
                return clip;
            }
        }

        private static string ToUrl(string path) => path.Contains("://") ? path : "file://" + path;
    }
}
