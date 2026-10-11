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

        [Test]
        public void ClipPathOf_ForestKeepsItsPath_OtherRegionsUseTheirOwnName()
        {
            Assert.AreEqual(ProceduralMusic.ClipPath, ProceduralMusic.ClipPathOf(Roboya.CodingEngine.Levels.Generated.RegionId.SabirOrmani));
            Assert.AreEqual("Music/paylasim_koyu", ProceduralMusic.ClipPathOf(Roboya.CodingEngine.Levels.Generated.RegionId.PaylasimKoyu));
        }

        [Test]
        public void SetRegion_NullPlayer_DoesNothing()
        {
            Assert.DoesNotThrow(() => new NullMusic().SetRegion(Roboya.CodingEngine.Levels.Generated.RegionId.PaylasimKoyu));
        }

        [Test]
        public void SetRegion_RegionWithoutARecording_KeepsTheForestLoopAndTheOffChoice()
        {
            var go = new UnityEngine.GameObject("music-test");
            var folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "roboya-music-" + System.Guid.NewGuid().ToString("N"));
            try
            {
                var music = new ProceduralMusic(go.AddComponent<UnityEngine.AudioSource>(), folder);
                music.SetEnabled(false);
                var source = go.GetComponent<UnityEngine.AudioSource>();
                var forestClip = source.clip;

                music.SetRegion(Roboya.CodingEngine.Levels.Generated.RegionId.PaylasimKoyu);

                Assert.AreSame(forestClip, source.clip, "no village recording yet: the forest loop plays");
                Assert.IsFalse(music.Enabled);
                Assert.IsFalse(source.isPlaying);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                if (System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.Delete(folder, true);
                }
            }
        }
    }
}
