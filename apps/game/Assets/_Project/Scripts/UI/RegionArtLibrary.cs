using System;
using Roboya.CodingEngine.Levels.Generated;
using UnityEngine;

namespace Roboya.UI
{
    /// <summary>
    /// Which <see cref="RegionArt"/> a region uses (plan C3). One asset in Resources/Art, loaded by name, so the thin scene entries
    /// pick a region's pictures without a new serialized field in the scenes (CLAUDE.md §2.6). A region without an entry, and a missing
    /// library, fall back to the art the scene was built with (the forest's).
    /// </summary>
    [CreateAssetMenu(menuName = "Roboya/Region Art Library")]
    public sealed class RegionArtLibrary : ScriptableObject
    {
        public const string ResourcePath = "Art/RegionArtLibrary";

        [SerializeField] private Entry[] entries = new Entry[0];

        /// <summary>Region name (the <see cref="RegionId"/> name, not its number, so adding a region never shifts old assets) and its art.</summary>
        [Serializable]
        public struct Entry
        {
            public string region;
            public RegionArt art;
        }

        public static RegionArtLibrary Load() => Resources.Load<RegionArtLibrary>(ResourcePath);

        /// <summary>The region's own art, or <paramref name="fallback"/> when it has none.</summary>
        public RegionArt For(RegionId region, RegionArt fallback)
        {
            string name = region.ToString();
            foreach (var entry in entries)
            {
                if (entry.region == name && entry.art != null)
                {
                    return entry.art;
                }
            }

            return fallback;
        }

        /// <summary>Adds or replaces a region's art (editor setup and tests).</summary>
        public void Set(RegionId region, RegionArt art)
        {
            string name = region.ToString();
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].region == name)
                {
                    entries[i].art = art;
                    return;
                }
            }

            Array.Resize(ref entries, entries.Length + 1);
            entries[entries.Length - 1] = new Entry { region = name, art = art };
        }

        /// <summary>Null-safe lookup for callers that may not have a library.</summary>
        public static RegionArt Pick(RegionArtLibrary library, RegionId region, RegionArt fallback) =>
            library != null ? library.For(region, fallback) : fallback;
    }
}
