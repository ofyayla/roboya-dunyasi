namespace Roboya.Core
{
    /// <summary>Plays narration by localization key (never by file name). Implementations swap TTS for studio audio.</summary>
    public interface IVoicePlayer
    {
        void Play(string key);

        void Stop();
    }
}
