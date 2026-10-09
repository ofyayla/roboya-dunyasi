using System;
using NUnit.Framework;
using Roboya.Core;

namespace Roboya.Tests.Core
{
    public class SfxTests
    {
        [Test]
        public void Render_EverySound_IsAudibleSoftAndShort()
        {
            foreach (SfxKind kind in Enum.GetValues(typeof(SfxKind)))
            {
                var samples = ProceduralSfx.Render(kind);

                Assert.Greater(samples.Length, ProceduralSfx.SampleRate / 40, kind + " is not empty");
                Assert.Less(samples.Length, ProceduralSfx.SampleRate, kind + " stays under a second");
                float peak = 0f;
                foreach (var s in samples)
                {
                    peak = Math.Max(peak, Math.Abs(s));
                }

                Assert.Greater(peak, 0.1f, kind + " is audible");
                Assert.LessOrEqual(peak, 0.8f, kind + " does not clip or startle");
                Assert.Less(Math.Abs(samples[samples.Length - 1]), 0.02f, kind + " fades out without a click");
                Assert.Less(Math.Abs(samples[0]), 0.02f, kind + " starts without a click");
            }
        }
    }
}
