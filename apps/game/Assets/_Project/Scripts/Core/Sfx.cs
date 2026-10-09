using System;
using UnityEngine;

namespace Roboya.Core
{
    public enum SfxKind
    {
        Step,
        Turn,
        Collect,
        Bump,
        Success,
        Place,
    }

    /// <summary>Short game sounds (walking, turning, collecting, bumping, placing a card, winning). No text, no voice.</summary>
    public interface ISfxPlayer
    {
        void Play(SfxKind kind);
    }

    public sealed class NullSfx : ISfxPlayer
    {
        public void Play(SfxKind kind)
        {
        }
    }

    /// <summary>
    /// Sounds made in code at start-up, so there is no audio file to license and no extra download. Each one is a few hundredths
    /// of a second to a third of a second of soft sine tones with a quick fade, at a modest volume under the narration.
    /// </summary>
    public sealed class ProceduralSfx : ISfxPlayer
    {
        public const int SampleRate = 22050;
        private readonly AudioSource _source;
        private readonly AudioClip[] _clips;

        public ProceduralSfx(AudioSource source, float volume = 0.5f)
        {
            _source = source;
            _source.playOnAwake = false;
            _source.volume = volume;
            var kinds = (SfxKind[])Enum.GetValues(typeof(SfxKind));
            _clips = new AudioClip[kinds.Length];
            foreach (var kind in kinds)
            {
                var samples = Render(kind);
                var clip = AudioClip.Create("sfx." + kind, samples.Length, 1, SampleRate, false);
                clip.SetData(samples, 0);
                _clips[(int)kind] = clip;
            }
        }

        public void Play(SfxKind kind) => _source.PlayOneShot(_clips[(int)kind]);

        /// <summary>The waveform of one sound; pure, so it can be tested without audio hardware.</summary>
        public static float[] Render(SfxKind kind)
        {
            switch (kind)
            {
                case SfxKind.Step: return Tone(0.07f, 330f, 260f, 0.5f);
                case SfxKind.Turn: return Tone(0.12f, 420f, 640f, 0.4f);
                case SfxKind.Place: return Tone(0.05f, 700f, 700f, 0.45f);
                case SfxKind.Bump: return Tone(0.16f, 150f, 80f, 0.7f);
                case SfxKind.Collect: return Concat(Tone(0.09f, 660f, 660f, 0.5f), Tone(0.14f, 880f, 880f, 0.5f));
                default: return Concat(Tone(0.1f, 523f, 523f, 0.5f), Tone(0.1f, 659f, 659f, 0.5f), Tone(0.1f, 784f, 784f, 0.5f), Tone(0.28f, 1047f, 1047f, 0.5f));
            }
        }

        private static float[] Tone(float seconds, float startHz, float endHz, float gain)
        {
            int n = Mathf.RoundToInt(seconds * SampleRate);
            var data = new float[n];
            double phase = 0;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                double hz = startHz + ((endHz - startHz) * t);
                phase += 2.0 * Math.PI * hz / SampleRate;
                // Quick attack and a smooth fade: no clicks.
                float env = Mathf.Min(1f, t * 20f) * (1f - t) * (1f - t);
                data[i] = (float)Math.Sin(phase) * env * gain;
            }

            return data;
        }

        private static float[] Concat(params float[][] parts)
        {
            int total = 0;
            foreach (var p in parts)
            {
                total += p.Length;
            }

            var all = new float[total];
            int at = 0;
            foreach (var p in parts)
            {
                Array.Copy(p, 0, all, at, p.Length);
                at += p.Length;
            }

            return all;
        }
    }
}
