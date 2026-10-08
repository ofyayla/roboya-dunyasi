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
        public void RobotAnchors_ShippedFile_CoversEveryPoseWithAUsableHead()
        {
            var anchors = RobotAnchors.Parse(File.ReadAllText(Path.Combine(ContentFiles.RepositoryContentPath, RobotAnchors.File)));

            foreach (var pose in new[] { "front", "back", "side", "happy", "laughing", "curious", "surprised", "proud" })
            {
                var a = anchors.For(pose);
                Assert.IsNotNull(a, pose);
                Assert.Greater(a.HeadWidth, 0.3f, pose);
                Assert.Greater(a.HeadHeight, 0.3f, pose);
            }

            Assert.IsNull(anchors.For("sleeping"));
            Assert.IsNull(anchors.For(null));
        }

        [Test]
        public void RobotAnchors_BulbOptionalAndBadHeadRejected()
        {
            var a = RobotAnchors.Parse("{\"front\":{\"head\":[0.2,0.2,0.8,0.7],\"bulb\":null}}").For("front");

            Assert.IsFalse(a.HasBulb);
            Assert.AreEqual(0.5f, a.HeadCenterX, 1e-4f);
            Assert.Throws<FormatException>(() => RobotAnchors.Parse("{\"front\":{\"head\":[0.8,0.2,0.2,0.7]}}"));
            Assert.Throws<FormatException>(() => RobotAnchors.Parse("null"));
        }

        [Test]
        public void RobotPartCatalog_ShippedParts_ColourHasNoAnchorOthersDo()
        {
            var catalog = RobotPartCatalog.Parse(File.ReadAllText(Path.Combine(ContentFiles.RepositoryContentPath, RobotPartCatalog.File)));

            foreach (var part in catalog.Parts)
            {
                var place = catalog.PlacementOf(part.Id);
                Assert.AreEqual(part.Slot == RobotPartCatalog.ColorSlot, place.IsColourVariant, part.Id);
            }

            Assert.AreEqual("roboya_happy_blue", catalog.PlacementOf("color-blue").SpriteFor("happy"));
            Assert.AreEqual(PartAnchor.Bulb, catalog.PlacementOf("antenna-star").Anchor);
            Assert.IsTrue(catalog.PlacementOf("wings-leaf").Behind);
        }

        [TestCase("{\"parts\":[{\"id\":\"h\",\"slot\":\"hat\",\"order\":1,\"sprite\":\"s\"}]}")]
        [TestCase("{\"parts\":[{\"id\":\"h\",\"slot\":\"hat\",\"order\":1,\"sprite\":\"s\",\"anchor\":\"foot\"}]}")]
        [TestCase("{\"parts\":[{\"id\":\"c\",\"slot\":\"color\",\"order\":1,\"sprite\":\"s\",\"anchor\":\"bulb\"}]}")]
        public void RobotPartCatalog_AnchorRules_Enforced(string json)
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
