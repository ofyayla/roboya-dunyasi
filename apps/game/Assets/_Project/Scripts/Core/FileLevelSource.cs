using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Roboya.Core
{
    /// <summary>Editor and desktop: reads level files straight from the repository's content/levels.</summary>
    public sealed class FileLevelSource : ILevelSource
    {
        private readonly string _root;

        public FileLevelSource(string root)
        {
            _root = root;
        }

        /// <summary>apps/game/Assets → repository root → content/levels.</summary>
        public static string RepositoryLevelsPath =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "..", "content", "levels"));

        public async Awaitable<IReadOnlyList<string>> LoadAllAsync()
        {
            await Awaitable.BackgroundThreadAsync();
            var files = Directory.GetFiles(_root, "*.json", SearchOption.AllDirectories);
            System.Array.Sort(files, System.StringComparer.Ordinal);
            var result = new List<string>(files.Length);
            foreach (var f in files)
            {
                result.Add(File.ReadAllText(f));
            }

            await Awaitable.MainThreadAsync();
            return result;
        }
    }
}
