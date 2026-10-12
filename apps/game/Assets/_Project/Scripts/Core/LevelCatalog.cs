using System;
using System.Collections.Generic;
using Roboya.CodingEngine.Levels;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Sharing;
using Roboya.CodingEngine.World;
using UnityEngine;

namespace Roboya.Core
{
    /// <summary>A parsed level ready to play.</summary>
    public sealed class LevelEntry
    {
        public LevelEntry(LevelDto dto, Level level)
        {
            Dto = dto;
            Level = level;
        }

        /// <summary>A village square level (ADR 0031): no grid, so <see cref="Level"/> stays null.</summary>
        public LevelEntry(LevelDto dto, SharingLevel sharing)
        {
            Dto = dto;
            Sharing = sharing;
        }

        public LevelDto Dto { get; }

        /// <summary>The grid level of Yön Avcısı, Kodlama Kutusu and Bal Peşinde; null for the village square.</summary>
        public Level Level { get; }

        /// <summary>The village square level (<c>koy-meydani</c>); null for the grid games.</summary>
        public SharingLevel Sharing { get; }

        public bool IsSharing => Sharing != null;

        public string Id => Dto.Id;

        /// <summary>From the validator; -1 if unknown (third star then cannot be earned).</summary>
        public int ShortestLength => Dto.Solution != null ? (int)Dto.Solution.ShortestLength : -1;
    }

    /// <summary>Parses all levels once and groups them by game in path order.</summary>
    public sealed class LevelCatalog
    {
        private readonly List<LevelEntry> _entries;

        private LevelCatalog(List<LevelEntry> entries)
        {
            _entries = entries;
        }

        public IReadOnlyList<LevelEntry> All => _entries;

        public static LevelCatalog Parse(IEnumerable<string> jsonFiles)
        {
            var entries = new List<LevelEntry>();
            foreach (var json in jsonFiles)
            {
                try
                {
                    var dto = LevelLoader.Parse(json);
                    entries.Add(dto.Game == GameId.KoyMeydani ? new LevelEntry(dto, LevelLoader.ToSharing(dto)) : new LevelEntry(dto, LevelLoader.ToLevel(dto)));
                }
                catch (LevelValidationException e)
                {
                    // CI validates content; a broken file must not take the whole game down.
                    Debug.LogError("Skipping invalid level: " + e.Message);
                }
            }

            entries.Sort((a, b) =>
            {
                int byRegion = a.Dto.Region.CompareTo(b.Dto.Region);
                return byRegion != 0 ? byRegion : a.Dto.Order.CompareTo(b.Dto.Order);
            });
            return new LevelCatalog(entries);
        }

        public List<LevelEntry> ForGame(GameId game)
        {
            return _entries.FindAll(e => e.Dto.Game == game);
        }

        public LevelEntry Find(string id) => _entries.Find(e => string.Equals(e.Id, id, StringComparison.Ordinal));
    }
}
