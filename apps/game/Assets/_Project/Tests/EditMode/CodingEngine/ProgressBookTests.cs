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
        public void MarkShipPartsSeen_OnlyMovesForward()
        {
            var book = new ProgressBook();

            Assert.IsTrue(book.MarkShipPartsSeen(2));
            Assert.IsFalse(book.MarkShipPartsSeen(1));
            Assert.IsFalse(book.MarkShipPartsSeen(2));
            Assert.AreEqual(2, book.ShipPartsSeen);
        }

        [Test]
        public void ToJson_RoundTrip_KeepsStarsAndSeenParts()
        {
            var book = new ProgressBook();
            book.Record("sabir-ormani.yon-avcisi.01", 2);
            book.MarkShipPartsSeen(1);

            var copy = ProgressBook.FromJson(book.ToJson());

            Assert.AreEqual(2, copy.Stars("sabir-ormani.yon-avcisi.01"));
            Assert.AreEqual(1, copy.ShipPartsSeen);
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
            Assert.AreEqual(0, book.ShipPartsSeen);
        }

        [Test]
        public void FromJson_OlderFileWithEquippedParts_IsReadAndIgnoresThem()
        {
            var book = ProgressBook.FromJson("{\"version\": 1, \"stars\": {\"a\": 2}, \"equipped\": {\"hat\": \"hat-acorn\"}}");

            Assert.AreEqual(2, book.Stars("a"));
            Assert.AreEqual(0, book.ShipPartsSeen);
        }
    }
}
