using UnityEngine;

namespace Roboya.Core
{
    /// <summary>Placeholder until TTS clips exist (F0-15): logs the key so testers can follow the flow.</summary>
    public sealed class LoggingVoicePlayer : IVoicePlayer
    {
        public void Play(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                Debug.Log("[voice] " + key);
            }
        }

        public void Stop()
        {
        }
    }
}
