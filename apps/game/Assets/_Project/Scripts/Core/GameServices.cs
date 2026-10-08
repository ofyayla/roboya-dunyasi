using Roboya.CodingEngine.Play;
using Roboya.CodingEngine.Progress;

namespace Roboya.Core
{
    /// <summary>Everything a scene needs, built once by <see cref="Bootstrap"/> and passed in (no singletons).</summary>
    public sealed class GameServices
    {
        public GameServices(
            LevelCatalog catalog,
            IVoicePlayer voice,
            SessionRules rules,
            ProgressRules progressRules,
            IProgressStore progress,
            IEntitlementSource entitlements,
            RobotPartCatalog parts,
            IslandLayout island,
            ISceneNavigator navigator)
        {
            Catalog = catalog;
            Voice = voice;
            Rules = rules;
            ProgressRules = progressRules;
            Progress = progress;
            Entitlements = entitlements;
            Parts = parts;
            Island = island;
            Navigator = navigator;
        }

        /// <summary>All levels, parsed once at start-up.</summary>
        public LevelCatalog Catalog { get; }

        public IVoicePlayer Voice { get; }

        public SessionRules Rules { get; }

        public ProgressRules ProgressRules { get; }

        public IProgressStore Progress { get; }

        /// <summary>Server-backed premium flag; never computed on the client (golden rule 3).</summary>
        public IEntitlementSource Entitlements { get; }

        public RobotPartCatalog Parts { get; }

        public IslandLayout Island { get; }

        public ISceneNavigator Navigator { get; }
    }
}
