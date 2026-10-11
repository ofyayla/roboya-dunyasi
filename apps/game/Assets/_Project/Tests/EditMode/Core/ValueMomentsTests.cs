using System.Collections.Generic;
using NUnit.Framework;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.Core;

namespace Roboya.Tests.Core
{
    public class ValueMomentsTests
    {
        private static System.Func<string, bool> Lines(params string[] keys)
        {
            var set = new HashSet<string>(keys);
            return set.Contains;
        }

        [Test]
        public void Wire_Values_AreTheContentNames()
        {
            Assert.AreEqual("patience", ValueMoments.Wire(ValueId.Patience));
            Assert.AreEqual("sharing", ValueMoments.Wire(ValueId.Sharing));
            Assert.AreEqual("responsibility", ValueMoments.Wire(ValueId.Responsibility));
        }

        [Test]
        public void Of_Patience_KeepsTheForestFilmAndItsFiveSceneLines()
        {
            var moment = ValueMoments.Of(ValueId.Patience, Lines("value.patience.1", "value.patience.2", "value.patience.3", "value.patience.4", "value.patience.5"));

            Assert.AreEqual("video/value_patience.mp4", moment.FilmFile);
            Assert.AreEqual("value.patience", moment.FallbackKey);
            CollectionAssert.AreEqual(
                new[] { "value.patience.1", "value.patience.2", "value.patience.3", "value.patience.4", "value.patience.5" }, moment.SceneKeys);
        }

        [Test]
        public void Of_SharingWithoutAnyLines_HasNoScenesAndItsOwnFilmName()
        {
            var moment = ValueMoments.Of(ValueId.Sharing, Lines());

            Assert.AreEqual("video/value_sharing.mp4", moment.FilmFile);
            Assert.AreEqual("value.sharing", moment.FallbackKey);
            Assert.AreEqual(0, moment.SceneKeys.Count);
        }

        [Test]
        public void Of_SceneLines_StopAtTheFirstGap()
        {
            var moment = ValueMoments.Of(ValueId.Sharing, Lines("value.sharing.1", "value.sharing.2", "value.sharing.4"));

            CollectionAssert.AreEqual(new[] { "value.sharing.1", "value.sharing.2" }, moment.SceneKeys);
        }

        [Test]
        public void Of_EveryLineExists_StopsAtTheSceneCap()
        {
            var moment = ValueMoments.Of(ValueId.Sharing, _ => true);

            Assert.AreEqual(ValueMoments.MaxScenes, moment.SceneKeys.Count);
        }

        [Test]
        public void Of_NoLookup_HasNoSceneLines()
        {
            Assert.AreEqual(0, ValueMoments.Of(ValueId.Patience, null).SceneKeys.Count);
        }

        [Test]
        public void Of_ShippedScript_HasPatienceSceneLinesForTheForestFilm()
        {
            var script = System.IO.File.ReadAllText(System.IO.Path.Combine(ContentFiles.RepositoryContentPath, "voice/script.csv"));

            var moment = ValueMoments.Of(ValueId.Patience, key => script.Contains("\n" + key + ","));

            Assert.AreEqual(5, moment.SceneKeys.Count, "the forest film's five lines are in the script");
            StringAssert.Contains("\nvalue.patience,", script);
        }
    }
}
