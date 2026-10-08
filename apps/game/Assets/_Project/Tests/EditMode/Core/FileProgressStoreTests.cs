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
            store.Book.Equip("hat", "hat-acorn");
            store.Save();

            var again = new FileProgressStore(_dir);

            Assert.AreEqual(store.ProfileId, again.ProfileId);
            Assert.AreEqual(2, again.Book.Stars("a"));
            Assert.AreEqual("hat-acorn", again.Book.Equipped("hat"));
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
        public void RobotPartCatalog_ShippedFile_IsConsistent()
        {
            var catalog = RobotPartCatalog.Parse(File.ReadAllText(Path.Combine(ContentFiles.RepositoryContentPath, RobotPartCatalog.File)));

            Assert.GreaterOrEqual(catalog.Parts.Count, 4);
            for (int i = 0; i < catalog.Parts.Count; i++)
            {
                Assert.AreEqual(i + 1, catalog.Parts[i].Order, "orders are 1..n without gaps");
                Assert.IsNotNull(catalog.PlacementOf(catalog.Parts[i].Id));
            }
        }

        [TestCase("{\"parts\":[{\"id\":\"a\",\"slot\":\"tail\",\"order\":1,\"sprite\":\"s\"}]}")]
        [TestCase("{\"parts\":[{\"id\":\"a\",\"slot\":\"hat\",\"order\":1,\"sprite\":\"s\"},{\"id\":\"b\",\"slot\":\"hat\",\"order\":1,\"sprite\":\"s\"}]}")]
        [TestCase("null")]
        public void RobotPartCatalog_BadEntries_Throw(string json)
        {
            Assert.Throws<FormatException>(() => RobotPartCatalog.Parse(json));
        }

        [Test]
        public void IslandLayout_ShippedFile_HasPatienceForestInsideImage()
        {
            var layout = IslandLayout.Parse(File.ReadAllText(Path.Combine(ContentFiles.RepositoryContentPath, IslandLayout.File)));

            Assert.IsTrue(layout.Regions.Exists(r => r.Id == "sabir-ormani"));
            Assert.AreEqual(6, layout.Regions.Count);
        }

        [Test]
        public void IslandLayout_RegionOutsideImage_Throws()
        {
            Assert.Throws<FormatException>(() => IslandLayout.Parse("{\"regions\":[{\"id\":\"x\",\"x\":1.4,\"y\":0.5,\"radius\":0.1}]}"));
        }
    }
}
