using System;
using System.Threading;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace Roboya.Games.Common
{
    /// <summary>
    /// The value moment (F1-24, plan C4): at the end of each corner a short silent film plays while its spoken lines run; patience
    /// closes the forest's corners (Roboya rushes, falls, the wise turtle helps him plan, he walks the plan), sharing the village's.
    /// Without the film file the region's friend alone stays on a calm card with the one-line version (nothing is spoken when
    /// even that line is not written yet). Tapping moves on after a moment; nothing is scored. The forest film is AI-generated art
    /// (docs/art/sabir-deger-karti-storyboard.md).
    /// </summary>
    public sealed class ValueCard : VisualElement
    {
        private const float FallbackSeconds = 8f;
        private const float MaxSeconds = 40f;
        private const float TapGraceSeconds = 1.5f;

        // One spoken line per scene of the film; each starts when its clip begins (clips are 6 s with 0.4 s cross-fades).
        private const float SceneSeconds = 5.6f;

        private readonly IVoicePlayer _voice;
        private readonly VisualElement _turtle;
        private readonly VisualElement _film;
        private GameObject _host;
        private VideoPlayer _player;
        private RenderTexture _texture;

        public ValueCard(IVoicePlayer voice)
        {
            _voice = voice;
            name = "value-card";
            AddToClassList("value-card");
            AddToClassList("hidden");
            _turtle = new VisualElement();
            _turtle.AddToClassList("value-card__turtle");
            _turtle.Add(new Icon(IconKind.Turtle) { Color = new Color(0.36f, 0.62f, 0.3f), Accent = Color.white });
            Add(_turtle);
            _film = new VisualElement { name = "value-film" };
            _film.AddToClassList("value-card__film");
            _film.AddToClassList("hidden");
            Add(_film);
        }

        /// <summary>True while the film (not the fallback card) is what the child sees.</summary>
        public bool PlayingFilm { get; private set; }

        /// <param name="moment">The film, its scene lines and the fallback line of the level's value.</param>
        /// <param name="friend">The region's friend as a picture for the calm card; null keeps the code-drawn turtle (the forest).</param>
        public async Awaitable ShowAsync(ValueMoment moment, Sprite friend, CancellationToken token)
        {
            _turtle.EnableInClassList("value-card__friend", friend != null);
            if (friend != null)
            {
                _turtle.Clear();
                _turtle.style.backgroundImage = new StyleBackground(friend);
            }

            RemoveFromClassList("hidden");
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                float shown = 0f;
                EventCallback<PointerDownEvent> tap = _ =>
                {
                    if (shown >= TapGraceSeconds)
                    {
                        cts.Cancel();
                    }
                };
                RegisterCallback(tap);
                try
                {
                    PlayingFilm = await TryStartFilmAsync(moment.FilmFile, cts.Token);
                    int nextScene = 0;
                    if (!PlayingFilm)
                    {
                        // No film: the one-line version over the calm turtle card.
                        _voice.Play(moment.FallbackKey);
                    }
                    _turtle.EnableInClassList("hidden", PlayingFilm);
                    _film.EnableInClassList("hidden", !PlayingFilm);
                    float limit = PlayingFilm ? MaxSeconds : FallbackSeconds;
                    // Slow breathing for the fallback; the film just runs until it ends.
                    await Tween.Run(limit, t =>
                    {
                        shown = t * limit;
                        if (PlayingFilm)
                        {
                            // Narration follows the film's own clock, so it stays with the pictures if a frame is dropped.
                            while (nextScene < moment.SceneKeys.Count && _player.time >= nextScene * SceneSeconds)
                            {
                                _voice.Play(moment.SceneKeys[nextScene]);
                                nextScene++;
                            }

                            if (shown > 1f && !_player.isPlaying)
                            {
                                cts.Cancel();
                            }
                        }
                        else
                        {
                            float s = 1f + (0.05f * Mathf.Sin(t * Mathf.PI * 2f * (FallbackSeconds / 3.2f)));
                            _turtle.style.scale = new Scale(new Vector3(s, s, 1f));
                        }
                    }, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    token.ThrowIfCancellationRequested();
                }
                finally
                {
                    UnregisterCallback(tap);
                    StopFilm();
                    PlayingFilm = false;
                    _turtle.RemoveFromClassList("hidden");
                    AddToClassList("hidden");
                }
            }
        }

        private async Awaitable<bool> TryStartFilmAsync(string filmFile, CancellationToken token)
        {
            string url = ContentFiles.UrlOf(filmFile);
            if (url == null)
            {
                return false;
            }

            try
            {
                _host = new GameObject("ValueFilm") { hideFlags = HideFlags.HideAndDontSave };
                _texture = new RenderTexture(1280, 720, 0);
                _player = _host.AddComponent<VideoPlayer>();
                _player.playOnAwake = false;
                _player.source = VideoSource.Url;
                _player.url = url;
                _player.renderMode = VideoRenderMode.RenderTexture;
                _player.targetTexture = _texture;
                _player.audioOutputMode = VideoAudioOutputMode.None;
                _player.isLooping = false;
                bool failed = false;
                _player.errorReceived += (_, _) => failed = true;
                _player.Prepare();
                float waited = 0f;
                while (!_player.isPrepared && !failed && waited < 6f)
                {
                    await Awaitable.NextFrameAsync(token);
                    waited += Time.unscaledDeltaTime;
                }

                if (failed || !_player.isPrepared)
                {
                    StopFilm();
                    return false;
                }

                _film.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(_texture));
                _player.Play();
                return true;
            }
            catch (OperationCanceledException)
            {
                StopFilm();
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Value film not played: " + ex.Message);
                StopFilm();
                return false;
            }
        }

        private void StopFilm()
        {
            if (_player != null)
            {
                _player.Stop();
            }

            if (_host != null)
            {
                UnityEngine.Object.Destroy(_host);
                _host = null;
            }

            _player = null;
            if (_texture != null)
            {
                _texture.Release();
                UnityEngine.Object.Destroy(_texture);
                _texture = null;
            }

            _film.style.backgroundImage = StyleKeyword.Null;
        }
    }
}
