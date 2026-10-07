using System;
using System.Collections.Generic;
using Roboya.CodingEngine.Levels;
using Roboya.CodingEngine.Levels.Generated;
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

        public LevelDto Dto { get; }

        public Level Level { get; }

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
                    entries.Add(new LevelEntry(dto, LevelLoader.ToLevel(dto)));
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
