using System;
using System.IO;
using UnityEngine;

namespace Roboya.Core
{
    /// <summary>Background music that a grown-up can switch off. No text, no voice; stays well under the narration.</summary>
    public interface IMusicPlayer
    {
        bool Enabled { get; }

        void SetEnabled(bool enabled);
    }

    public sealed class NullMusic : IMusicPlayer
    {
        public bool Enabled => false;

        public void SetEnabled(bool enabled)
        {
        }
    }

    /// <summary>
    /// Region music composed in code (marimba-like plucks over a slow C-A-F-G pentatonic pattern), rendered once at start-up and looped.
    /// Original by construction: no audio file, no licence, no download. The choice is kept in a marker file in the progress folder.
    /// </summary>
    public sealed class ProceduralMusic : IMusicPlayer
    {
        public const int SampleRate = 22050;
        public const float Bpm = 84f;
        public const int Bars = 8;
        private const string OffMarker = "music.off";

        private readonly AudioSource _source;
        private readonly string _folder;

        public ProceduralMusic(AudioSource source, string folder, float volume = 0.16f)
        {
            _source = source;
            _folder = folder;
            _source.playOnAwake = false;
            _source.loop = true;
            _source.volume = volume;
            var samples = Render();
            var clip = AudioClip.Create("music.sabir_ormani", samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            _source.clip = clip;
            Enabled = !File.Exists(Path.Combine(folder, OffMarker));
            Apply();
        }

        public bool Enabled { get; private set; }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            try
            {
                string marker = Path.Combine(_folder, OffMarker);
                if (enabled)
                {
                    if (File.Exists(marker))
                    {
                        File.Delete(marker);
                    }
                }
                else
                {
                    Directory.CreateDirectory(_folder);
                    File.WriteAllText(marker, "off");
                }
            }
            catch (IOException ex)
            {
                // The choice still holds for this session; only the memory of it is lost.
                Debug.LogWarning("Music setting not saved: " + ex.Message);
            }

            Apply();
        }

        private void Apply()
        {
            if (Enabled && !Application.isBatchMode)
            {
                if (!_source.isPlaying)
                {
                    _source.Play();
                }
            }
            else
            {
                _source.Stop();
            }
        }

        /// <summary>The loop's waveform; pure, so tests can check length, level and the seam without audio hardware.</summary>
        public static float[] Render()
        {
            float beat = 60f / Bpm;
            int total = Mathf.RoundToInt(Bars * 4 * beat * SampleRate);
            var data = new float[total];

            // Chord roots (MIDI) per 2 bars: C, A, F, G; arpeggio picks from the major pentatonic of each root.
            int[] roots = { 60, 57, 53, 55 };
            int[] arp = { 0, 4, 7, 12, 7, 4, 9, 7 };
            for (int bar = 0; bar < Bars; bar++)
            {
                int root = roots[bar / 2];
                float barStart = bar * 4 * beat;
                AddPluck(data, barStart, root - 24, beat * 1.9f, 0.5f);
                AddPluck(data, barStart + (2f * beat), root - 24 + 7, beat * 1.9f, 0.32f);
                for (int i = 0; i < 8; i++)
                {
                    // Every other bar rests on a few steps, so the loop breathes.
                    if ((bar & 1) == 1 && (i == 3 || i == 6))
                    {
                        continue;
                    }

                    int note = root + arp[i] + (i >= 4 ? 12 : 0) - (i >= 4 && arp[i] == 12 ? 12 : 0);
                    AddPluck(data, barStart + (i * beat * 0.5f), note, beat * 1.4f, 0.28f);
                }
            }

            // Soft limiter so layered plucks never clip.
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (float)Math.Tanh(data[i]) * 0.9f;
            }

            return data;
        }

        // A plucked bell-like tone: sine plus quieter harmonics with an exponential decay. The tail wraps round to the start, so the loop has no seam.
        private static void AddPluck(float[] data, float startSeconds, int midi, float seconds, float gain)
        {
            double hz = 440.0 * Math.Pow(2.0, (midi - 69) / 12.0);
            int start = Mathf.RoundToInt(startSeconds * SampleRate);
            int n = Mathf.RoundToInt(seconds * SampleRate);
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / SampleRate;
                float attack = Mathf.Min(1f, i / 120f);
                float env = attack * Mathf.Exp(-4.2f * i / n);
                double w = 2.0 * Math.PI * hz * t;
                float v = (float)(Math.Sin(w) + (0.35 * Math.Sin(2 * w)) + (0.12 * Math.Sin(3 * w)));
                data[(start + i) % data.Length] += v * env * gain * 0.55f;
            }
        }
    }
}
