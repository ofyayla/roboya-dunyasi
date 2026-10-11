using NUnit.Framework;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.UI;
using UnityEngine;

namespace Roboya.Tests.Core
{
    public class RegionArtLibraryTests
    {
        private RegionArt _forest;
        private RegionArt _village;
        private RegionArtLibrary _library;

        [SetUp]
        public void SetUp()
        {
            _forest = ScriptableObject.CreateInstance<RegionArt>();
            _village = ScriptableObject.CreateInstance<RegionArt>();
            _library = ScriptableObject.CreateInstance<RegionArtLibrary>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_forest);
            Object.DestroyImmediate(_village);
            Object.DestroyImmediate(_library);
        }

        [Test]
        public void For_RegisteredRegion_ReturnsItsOwnArt()
        {
            _library.Set(RegionId.PaylasimKoyu, _village);

            Assert.AreSame(_village, _library.For(RegionId.PaylasimKoyu, _forest));
        }

        [Test]
        public void For_RegionWithoutAnEntry_FallsBackToTheSceneArt()
        {
            _library.Set(RegionId.PaylasimKoyu, _village);

            Assert.AreSame(_forest, _library.For(RegionId.YardimlasmaLimani, _forest));
        }

        [Test]
        public void For_EmptyLibrary_FallsBack()
        {
            Assert.AreSame(_forest, _library.For(RegionId.SabirOrmani, _forest));
        }

        [Test]
        public void For_EntryWithoutArt_FallsBack()
        {
            _library.Set(RegionId.PaylasimKoyu, null);

            Assert.AreSame(_forest, _library.For(RegionId.PaylasimKoyu, _forest));
        }

        [Test]
        public void Set_SameRegionTwice_ReplacesTheEntry()
        {
            _library.Set(RegionId.PaylasimKoyu, _forest);
            _library.Set(RegionId.PaylasimKoyu, _village);

            Assert.AreSame(_village, _library.For(RegionId.PaylasimKoyu, null));
        }

        [Test]
        public void Pick_MissingLibrary_ReturnsTheFallback()
        {
            Assert.AreSame(_forest, RegionArtLibrary.Pick(null, RegionId.PaylasimKoyu, _forest));
            Assert.IsNull(RegionArtLibrary.Pick(null, RegionId.PaylasimKoyu, null));
        }

        [Test]
        public void PropSprite_VillageProps_ComeFromTheirOwnSlotsAndAreNullWhenUnset()
        {
            Assert.IsNull(_village.PropSprite(StoryProp.VillagerBee));
            Assert.IsNull(_village.PropSprite(StoryProp.Honeycomb));
            Assert.IsNull(_village.PropSprite(StoryProp.VillageTable));
        }

        [Test]
        public void ShippedLibrary_HasTheForestAndTheVillage()
        {
            var shipped = RegionArtLibrary.Load();

            Assert.IsNotNull(shipped, "Resources/Art/RegionArtLibrary.asset (make refresh-art)");
            var forest = shipped.For(RegionId.SabirOrmani, null);
            var village = shipped.For(RegionId.PaylasimKoyu, null);
            Assert.IsNotNull(forest);
            Assert.IsNotNull(village);
            Assert.AreNotSame(forest, village);
            Assert.AreNotSame(forest.Background, village.Background, "the village has its own backdrop");
            Assert.IsNotNull(village.PropSprite(StoryProp.VillageTable), "village props are wired");
            Assert.IsNotNull(village.FriendPoseSprite(FriendPose.Thanks), "the old village bee thanks the child");
        }
    }
}
