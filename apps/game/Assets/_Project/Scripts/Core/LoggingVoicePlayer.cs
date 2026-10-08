using System.Collections.Generic;
using UnityEngine;

namespace Roboya.Core
{
    /// <summary>Fallback when no voice manifest is available: logs the key so testers can follow the flow.</summary>
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

        public async Awaitable PreloadAsync(IEnumerable<string> keys)
        {
            await Awaitable.NextFrameAsync();
        }
    }
}
