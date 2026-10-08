using System.Collections.Generic;
using UnityEngine;

namespace Roboya.Core
{
    /// <summary>Plays one narration line at a time on a dedicated AudioSource; a new line interrupts the old.</summary>
    public sealed class ClipVoicePlayer : IVoicePlayer
    {
        private readonly AudioSource _source;
        private readonly VoiceLibrary _library;
        private int _request;

        public ClipVoicePlayer(AudioSource source, VoiceLibrary library)
        {
            _source = source;
            _library = library;
        }

        public void Play(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            _ = PlayAsync(key, ++_request);
        }

        public void Stop()
        {
            _request++;
            if (_source != null)
            {
                _source.Stop();
            }
        }

        public async Awaitable PreloadAsync(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                await _library.GetAsync(key);
            }
        }

        private async Awaitable PlayAsync(string key, int request)
        {
            try
            {
                var clip = await _library.GetAsync(key);
                // A newer Play/Stop arrived while loading: drop this line.
                if (clip == null || request != _request || _source == null)
                {
                    return;
                }

                _source.Stop();
                _source.clip = clip;
                _source.Play();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
