using System;
using NUnit.Framework;
using Roboya.CodingEngine.Progress;

namespace Roboya.Tests.CodingEngine
{
    public class ProgressBookTests
    {
        [Test]
        public void Record_HigherStars_Improves()
        {
            var book = new ProgressBook();

            Assert.IsTrue(book.Record("a", 2));
            Assert.IsTrue(book.Record("a", 3));
            Assert.AreEqual(3, book.Stars("a"));
        }

        [Test]
        public void Record_LowerOrEqualStars_KeepsBest()
        {
            var book = new ProgressBook();
            book.Record("a", 3);

            Assert.IsFalse(book.Record("a", 1));
            Assert.IsFalse(book.Record("a", 3));
            Assert.AreEqual(3, book.Stars("a"));
        }

        [Test]
        public void Record_OutOfRange_IsClamped()
        {
            var book = new ProgressBook();
            book.Record("a", 9);
            book.Record("b", -2);

            Assert.AreEqual(3, book.Stars("a"));
            Assert.AreEqual(0, book.Stars("b"));
            Assert.IsFalse(book.IsCompleted("b"));
        }

        [Test]
        public void Record_EmptyId_Throws()
        {
            Assert.Throws<ArgumentException>(() => new ProgressBook().Record("", 1));
        }

        [Test]
        public void CompletedCount_CountsLevelsWithStars()
        {
            var book = new ProgressBook();
            book.Record("a", 1);
            book.Record("b", 3);

            Assert.AreEqual(2, book.CompletedCount);
            Assert.AreEqual(0, book.Stars(null));
        }

        [Test]
        public void Equip_SetAndClear_TracksSlot()
        {
            var book = new ProgressBook();
            book.Equip("hat", "hat-leaf");

            Assert.AreEqual("hat-leaf", book.Equipped("hat"));
            book.Equip("hat", null);
            Assert.IsNull(book.Equipped("hat"));
            Assert.IsNull(book.Equipped(null));
            Assert.Throws<ArgumentException>(() => book.Equip("", "x"));
        }

        [Test]
        public void ToJson_RoundTrip_KeepsStarsAndParts()
        {
            var book = new ProgressBook();
            book.Record("sabir-ormani.yon-avcisi.01", 2);
            book.Equip("wings", "wings-leaf");

            var copy = ProgressBook.FromJson(book.ToJson());

            Assert.AreEqual(2, copy.Stars("sabir-ormani.yon-avcisi.01"));
            Assert.AreEqual("wings-leaf", copy.Equipped("wings"));
            Assert.AreEqual(1, copy.AllEquipped.Count);
            Assert.AreEqual(ProgressBook.CurrentVersion, copy.Version);
        }

        [TestCase("{ not json")]
        [TestCase("null")]
        [TestCase("{\"version\": 99}")]
        public void FromJson_BadInput_ThrowsFormat(string json)
        {
            Assert.Throws<FormatException>(() => ProgressBook.FromJson(json));
        }

        [Test]
        public void FromJson_MissingMaps_StartsEmpty()
        {
            var book = ProgressBook.FromJson("{\"version\": 1, \"stars\": null}");

            Assert.AreEqual(0, book.CompletedCount);
            Assert.IsNull(book.Equipped("hat"));
        }
    }
}
