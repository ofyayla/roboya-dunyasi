using Roboya.CodingEngine.Play;

namespace Roboya.Core
{
    /// <summary>Everything a scene needs, built once by <see cref="Bootstrap"/> and passed in (no singletons).</summary>
    public sealed class GameServices
    {
        public GameServices(ILevelSource levels, IVoicePlayer voice, SessionRules rules)
        {
            Levels = levels;
            Voice = voice;
            Rules = rules;
        }

        public ILevelSource Levels { get; }

        public IVoicePlayer Voice { get; }

        public SessionRules Rules { get; }
    }
}
