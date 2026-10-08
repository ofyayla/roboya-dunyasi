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
        private bool _loading;

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

            _loading = true;
            _ = PlayAsync(key, ++_request);
        }

        public bool IsPlaying => _loading || (_source != null && _source.isPlaying);

        public void Stop()
        {
            _request++;
            _loading = false;
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
                if (request != _request)
                {
                    return;
                }

                _loading = false;
                if (clip == null || _source == null)
                {
                    return;
                }

                _source.Stop();
                _source.clip = clip;
                _source.Play();
            }
            catch (System.Exception e)
            {
                if (request == _request)
                {
                    _loading = false;
                }

                Debug.LogException(e);
            }
        }
    }
}
