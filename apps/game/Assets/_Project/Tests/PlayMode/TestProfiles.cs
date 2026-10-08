using System.IO;
using Roboya.CodingEngine.Profiles;
using Roboya.Core;

namespace Roboya.Tests.PlayMode
{
    /// <summary>Gives a temporary folder one profile and the parent's consent, as if onboarding had been done.</summary>
    internal static class TestProfiles
    {
        /// <summary>Creates the registry (profile + consent) and returns the active profile's progress store.</summary>
        public static FileProgressStore Seed(string folder, AgeBand band = AgeBand.Minik, bool consent = true, string nickname = "Test")
        {
            var manager = ProfileManager.Load(folder);
            if (!manager.HasActive)
            {
                manager.Add(nickname, "robot-mavi", band);
            }

            if (consent)
            {
                manager.RecordConsent(CurrentNoticeVersion());
            }

            return new FileProgressStore(folder, manager.Active.Id);
        }

        public static string CurrentNoticeVersion() =>
            LocalNotice.Parse(File.ReadAllText(Path.Combine(ContentFiles.RepositoryContentPath, LocalNotice.File))).Version;
    }
}
