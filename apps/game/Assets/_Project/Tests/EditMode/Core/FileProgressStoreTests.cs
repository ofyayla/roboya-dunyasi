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
}
