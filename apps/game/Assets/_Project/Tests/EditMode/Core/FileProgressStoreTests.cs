using System;
using System.IO;
using NUnit.Framework;
using Roboya.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace Roboya.Tests.Core
{
    public class FileProgressStoreTests
    {
        private string _dir;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "roboya-store-" + Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_dir))
            {
                Directory.Delete(_dir, true);
            }
        }

        [Test]
        public void Save_ThenReopen_KeepsStarsAndProfile()
        {
            var store = new FileProgressStore(_dir);
            store.Book.Record("a", 2);
            store.Book.MarkShipPartsSeen(1);
            store.Save();

            var again = new FileProgressStore(_dir);

            Assert.AreEqual(store.ProfileId, again.ProfileId);
            Assert.AreEqual(2, again.Book.Stars("a"));
            Assert.AreEqual(1, again.Book.ShipPartsSeen);
        }

        [Test]
        public void Save_Twice_ReplacesAtomicallyWithoutTempLeftovers()
        {
            var store = new FileProgressStore(_dir);
            store.Book.Record("a", 1);
            store.Save();
            store.Book.Record("a", 3);
            store.Save();

            Assert.AreEqual(3, new FileProgressStore(_dir).Book.Stars("a"));
            Assert.IsEmpty(Directory.GetFiles(_dir, "*.tmp"));
        }

        [Test]
        public void Open_CorruptFile_StartsFreshAndKeepsCopy()
        {
            var store = new FileProgressStore(_dir);
            store.Save();
            File.WriteAllText(Path.Combine(_dir, store.ProfileId + ".json"), "{ broken");
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Progress file unreadable"));

            var again = new FileProgressStore(_dir);

            Assert.AreEqual(0, again.Book.CompletedCount);
            Assert.AreEqual(1, Directory.GetFiles(_dir, "*.corrupt-*").Length);
        }

        [Test]
        public void Open_InvalidProfileFile_CreatesNewProfileId()
        {
            Directory.CreateDirectory(_dir);
            File.WriteAllText(Path.Combine(_dir, "active-profile.txt"), "not-a-guid");

            var store = new FileProgressStore(_dir);

            Assert.IsTrue(Guid.TryParse(store.ProfileId, out _));
        }
    }

    public class ContentCatalogTests
    {
        [Test]
        public void ShipPartCatalog_ShippedFile_IsConsistent()
        {
            var catalog = ShipPartCatalog.Parse(File.ReadAllText(Path.Combine(ContentFiles.RepositoryContentPath, ShipPartCatalog.File)));

            Assert.AreEqual("ship_base", catalog.BaseSprite);
            Assert.GreaterOrEqual(catalog.Parts.Count, 3, "enough parts for 10 levels plus the region end");
            for (int i = 0; i < catalog.Parts.Count; i++)
            {
                Assert.AreEqual(i + 1, catalog.Parts[i].Order, "orders are 1..n without gaps");
                Assert.Greater(catalog.LayersOf(catalog.Parts[i].Id).Count, 0);
            }

            Assert.AreEqual(0, catalog.LayersOf("unknown").Count);
        }

        [TestCase("{\"parts\":[]}")]
        [TestCase("{\"base\":\"b\",\"parts\":[{\"id\":\"a\",\"order\":1,\"layers\":[]}]}")]
        [TestCase("{\"base\":\"b\",\"parts\":[{\"id\":\"a\",\"order\":1,\"layers\":[{\"sprite\":\"s\",\"width\":0.3}]},{\"id\":\"b\",\"order\":1,\"layers\":[{\"sprite\":\"s\",\"width\":0.3}]}]}")]
        [TestCase("{\"base\":\"b\",\"parts\":[{\"id\":\"a\",\"order\":1,\"layers\":[{\"sprite\":\"s\",\"width\":0}]}]}")]
        [TestCase("null")]
        public void ShipPartCatalog_BadEntries_Throw(string json)
        {
            Assert.Throws<FormatException>(() => ShipPartCatalog.Parse(json));
        }

        [Test]
        public void IslandLayout_ShippedFile_HasPatienceForestInsideImage()
        {
            var layout = IslandLayout.Parse(File.ReadAllText(Path.Combine(ContentFiles.RepositoryContentPath, IslandLayout.File)));

            Assert.IsTrue(layout.Regions.Exists(r => r.Id == "sabir-ormani"));
            Assert.AreEqual(6, layout.Regions.Count);
        }

        [Test]
        public void LocalizedStrings_ShippedTable_HasEveryKeyTheScreensUse()
        {
            var table = LocalizedStrings.Parse(File.ReadAllText(Path.Combine(ContentFiles.RepositoryContentPath, LocalizedStrings.File)));

            foreach (var key in StringKeys.All)
            {
                Assert.IsTrue(table.Has(key), key);
                Assert.IsNotEmpty(table.Get(key), key);
            }

            StringAssert.Contains("30", table.Format(StringKeys.GateLocked, 30));
        }

        [Test]
        public void LocalizedStrings_MissingKey_ShowsKeyAndWarnsOnce()
        {
            var table = new LocalizedStrings(new System.Collections.Generic.Dictionary<string, string>());
            LogAssert.Expect(LogType.Warning, "Missing string key: nope");

            Assert.AreEqual("[nope]", table.Get("nope"));
            Assert.AreEqual("[nope]", table.Get("nope"), "second lookup does not warn again");
            Assert.Throws<ArgumentNullException>(() => new LocalizedStrings(null));
            Assert.Throws<FormatException>(() => LocalizedStrings.Parse("null"));
        }

        [Test]
        public void IslandLayout_RegionOutsideImage_Throws()
        {
            Assert.Throws<FormatException>(() => IslandLayout.Parse("{\"regions\":[{\"id\":\"x\",\"x\":1.4,\"y\":0.5,\"radius\":0.1}]}"));
        }
    }

    public class SupportPlannerTests
    {
        private static string Level(int order, string options, string alternative = null, string introduces = null)
        {
            string alt = alternative == null ? string.Empty : ",\"alternativeLevelId\": \"sabir-ormani.yon-avcisi." + alternative + "\"";
            string intro = introduces == null ? string.Empty : ",\"introduces\": \"" + introduces + "\"";
            return @"{
  ""schemaVersion"": 2,
  ""id"": ""sabir-ormani.yon-avcisi." + order.ToString("D2") + @""",
  ""region"": ""sabir-ormani"", ""game"": ""yon-avcisi"", ""order"": " + order + @",
  ""meta"": { ""concepts"": [""sequencing""], ""value"": ""patience"", ""difficulty"": 1, ""ageLevels"": [""minik""] },
  ""grid"": { ""rows"": ["".."", ""..""] },
  ""robot"": { ""x"": 0, ""y"": 1, ""facing"": ""north"" },
  ""goal"": { ""reach"": { ""x"": 0, ""y"": 0 } },
  ""cards"": { ""palette"": [""forward""], ""maxProgramLength"": 3" + intro + @" },
  ""options"": " + options + @",
  ""voice"": { ""intro"": ""yon_avcisi.x.intro"" }" + alt + @",
  ""solution"": { ""shortestLength"": 1 }
}";
        }

        private static LevelCatalog Catalog() => LevelCatalog.Parse(new[]
        {
            Level(1, "{ \"ghostPath\": true, \"guided\": true }"),
            Level(2, "{ \"ghostPath\": true, \"guided\": true }", alternative: "01"),
            Level(3, "{ \"ghostPath\": true, \"guided\": true }", alternative: "02"),
            Level(4, "{ \"ghostPath\": true, \"guided\": true }", alternative: "03"),
            Level(5, "{ \"ghostPath\": true, \"guided\": true }", introduces: "forward"),
        });

        [Test]
        public void For_NewChild_KeepsAuthoredHelp()
        {
            var catalog = Catalog();

            var support = SupportPlanner.For(catalog, catalog.Find("sabir-ormani.yon-avcisi.04"), new Roboya.CodingEngine.Progress.ProgressBook());

            Assert.IsTrue(support.GhostPath);
            Assert.IsTrue(support.Guided);
        }

        [Test]
        public void For_ThreeThreeStarLevels_RemovesHelpOnTheNextLevel()
        {
            var catalog = Catalog();
            var book = new Roboya.CodingEngine.Progress.ProgressBook();
            for (int i = 1; i <= 3; i++)
            {
                book.Record("sabir-ormani.yon-avcisi.0" + i, 3);
            }

            var support = SupportPlanner.For(catalog, catalog.Find("sabir-ormani.yon-avcisi.04"), book);

            Assert.IsFalse(support.GhostPath);
            Assert.IsFalse(support.Guided);
        }

        [Test]
        public void For_CardIntroduction_AlwaysKeepsHelp()
        {
            var catalog = Catalog();
            var book = new Roboya.CodingEngine.Progress.ProgressBook();
            for (int i = 1; i <= 4; i++)
            {
                book.Record("sabir-ormani.yon-avcisi.0" + i, 3);
            }

            var support = SupportPlanner.For(catalog, catalog.Find("sabir-ormani.yon-avcisi.05"), book);

            Assert.IsTrue(support.Guided);
        }

        [Test]
        public void AlternativeFor_ReturnsThePlayableEasierLevelOrNull()
        {
            var catalog = Catalog();
            var level3 = catalog.Find("sabir-ormani.yon-avcisi.03");

            Assert.AreEqual("sabir-ormani.yon-avcisi.02", SupportPlanner.AlternativeFor(catalog, level3, id => true).Id);
            Assert.IsNull(SupportPlanner.AlternativeFor(catalog, level3, id => false), "not playable yet");
            Assert.IsNull(SupportPlanner.AlternativeFor(catalog, catalog.Find("sabir-ormani.yon-avcisi.01"), id => true), "level 1 has none");
        }
    }
}
