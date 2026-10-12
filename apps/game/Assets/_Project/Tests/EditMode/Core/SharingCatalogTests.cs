using NUnit.Framework;
using Roboya.Core;
using Roboya.Tests.CodingEngine;

namespace Roboya.Tests.Core
{
    public class SharingCatalogTests
    {
        [Test]
        public void Parse_VillageLevel_IsASharingEntryWithoutAGrid()
        {
            var catalog = LevelCatalog.Parse(new[] { SharingLoaderTests.Village });

            var entry = catalog.Find("paylasim-koyu.koy-meydani.01");

            Assert.IsNotNull(entry);
            Assert.IsTrue(entry.IsSharing);
            Assert.IsNull(entry.Level);
            Assert.AreEqual(2, entry.Sharing.Hives.Count);
            Assert.AreEqual(-1, entry.ShortestLength, "no solution recorded in this fixture");
        }

        [Test]
        public void ForGame_FindsVillageLevels()
        {
            var catalog = LevelCatalog.Parse(new[] { SharingLoaderTests.Village });

            Assert.AreEqual(1, catalog.ForGame(Roboya.CodingEngine.Levels.Generated.GameId.KoyMeydani).Count);
            Assert.AreEqual(0, catalog.ForGame(Roboya.CodingEngine.Levels.Generated.GameId.BalPesinde).Count);
        }
    }
}
