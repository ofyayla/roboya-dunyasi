using NUnit.Framework;
using Roboya.Core;

namespace Roboya.Tests.Core
{
    public class MusicTests
    {
        [Test]
        public void Render_LoopLength_IsWholeBarsAtTheTempo()
        {
            var samples = ProceduralMusic.Render();
            float seconds = (float)samples.Length / ProceduralMusic.SampleRate;
            Assert.AreEqual(ProceduralMusic.Bars * 4 * 60f / ProceduralMusic.Bpm, seconds, 0.01f);
        }

        [Test]
        public void Render_Samples_AreAudibleAndNeverClip()
        {
            float peak = 0f;
            double energy = 0;
            foreach (var s in ProceduralMusic.Render())
            {
                peak = System.Math.Max(peak, System.Math.Abs(s));
                energy += s * s;
            }

            Assert.Less(peak, 1f);
            Assert.Greater(energy, 1.0);
        }

        [Test]
        public void Render_LoopSeam_HasNoJump()
        {
            var samples = ProceduralMusic.Render();
            Assert.Less(System.Math.Abs(samples[0] - samples[samples.Length - 1]), 0.25f);
        }
    }
}
