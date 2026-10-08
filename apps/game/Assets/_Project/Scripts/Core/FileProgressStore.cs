using System;
using System.IO;
using Roboya.CodingEngine.Progress;
using UnityEngine;

namespace Roboya.Core
{
    /// <summary>
    /// Progress as a small JSON file per local profile (ADR 0009). The profile id is a random GUID created on first
    /// launch; no nickname or other personal data is stored. Writes are atomic (temp file + replace) so a crash or
    /// power loss never leaves half a file; an unreadable file is kept aside and the game starts fresh.
    /// </summary>
    public sealed class FileProgressStore : IProgressStore
    {
        private const string ProfileFile = "active-profile.txt";

        private readonly string _path;

        /// <summary>Legacy form: one anonymous profile whose id is kept in active-profile.txt.</summary>
        public FileProgressStore(string folder)
            : this(folder, LoadOrCreateProfileId(folder))
        {
        }

        public FileProgressStore(string folder, string profileId)
        {
            Directory.CreateDirectory(folder);
            ProfileId = profileId;
            _path = Path.Combine(folder, ProfileId + ".json");
            Book = Load(_path);
        }

        /// <summary>The profile id a pre-profiles install used, or null; lets those installs keep their progress.</summary>
        public static string LegacyProfileId(string folder)
        {
            string file = Path.Combine(folder, ProfileFile);
            if (!File.Exists(file))
            {
                return null;
            }

            string id = File.ReadAllText(file).Trim();
            return Guid.TryParse(id, out _) ? id : null;
        }

        /// <summary>Deletes a profile's progress file (profile removed by the parent).</summary>
        public static void Delete(string folder, string profileId)
        {
            string path = Path.Combine(folder, profileId + ".json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static string DefaultFolder => Path.Combine(Application.persistentDataPath, "progress");

        public string ProfileId { get; }

        public ProgressBook Book { get; }

        public void Save()
        {
            string temp = _path + ".tmp";
            File.WriteAllText(temp, Book.ToJson());
            if (File.Exists(_path))
            {
                File.Replace(temp, _path, null);
            }
            else
            {
                File.Move(temp, _path);
            }
        }

        private static ProgressBook Load(string path)
        {
            if (!File.Exists(path))
            {
                return new ProgressBook();
            }

            try
            {
                return ProgressBook.FromJson(File.ReadAllText(path));
            }
            catch (FormatException e)
            {
                // Keep the bad file for support, never crash the child's game over it.
                string aside = path + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                File.Move(path, aside);
                Debug.LogWarning("Progress file unreadable, starting fresh: " + e.Message);
                return new ProgressBook();
            }
        }

        private static string LoadOrCreateProfileId(string folder)
        {
            Directory.CreateDirectory(folder);
            string file = Path.Combine(folder, ProfileFile);
            if (File.Exists(file))
            {
                string id = File.ReadAllText(file).Trim();
                if (Guid.TryParse(id, out _))
                {
                    return id;
                }
            }

            string created = Guid.NewGuid().ToString("N");
            File.WriteAllText(file, created);
            return created;
        }
    }
}
