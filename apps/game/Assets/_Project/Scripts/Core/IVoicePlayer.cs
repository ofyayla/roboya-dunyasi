using System.Collections.Generic;
using UnityEngine;

namespace Roboya.Core
{
    /// <summary>Plays narration by localization key (never by file name). Implementations swap TTS for studio audio.</summary>
    public interface IVoicePlayer
    {
        /// <summary>Stops the current line and plays <paramref name="key"/>; unknown keys are ignored.</summary>
        void Play(string key);

        void Stop();

        /// <summary>True while a line is loading or playing; story scenes wait on it before moving on.</summary>
        bool IsPlaying { get; }

        /// <summary>Loads lines ahead of time so the first tap is answered without delay.</summary>
        Awaitable PreloadAsync(IEnumerable<string> keys);
    }
}
