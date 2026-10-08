using System;
using NUnit.Framework;
using Roboya.CodingEngine.Parents;
using Roboya.CodingEngine.Profiles;

namespace Roboya.Tests.CodingEngine
{
    public class ScreenTimeBookTests
    {
        private static readonly DateTime Today = new DateTime(2026, 10, 8, 18, 30, 0);

        [TestCase(AgeBand.Minik, 10)]
        [TestCase(AgeBand.Kasif, 15)]
        [TestCase(AgeBand.Mucit, 20)]
        public void LimitMinutes_NoChoice_UsesTheRecommendationForTheAge(AgeBand band, int expected)
        {
            Assert.AreEqual(expected, new ScreenTimeBook().LimitMinutes("p", band));
        }

        [Test]
        public void SetLimit_OverridesTheDefaultForThatProfileOnly()
        {
            var book = new ScreenTimeBook();

            book.SetLimit("a", 30);

            Assert.AreEqual(30, book.LimitMinutes("a", AgeBand.Minik));
            Assert.AreEqual(10, book.LimitMinutes("b", AgeBand.Minik));
            Assert.IsTrue(book.HasChosenLimit("a"));
            Assert.IsFalse(book.HasChosenLimit("b"));
        }

        [Test]
        public void SetLimit_RejectsValuesThatWereNotOffered()
        {
            var book = new ScreenTimeBook();

            Assert.Throws<ArgumentOutOfRangeException>(() => book.SetLimit("a", 25));
            Assert.Throws<ArgumentOutOfRangeException>(() => book.SetLimit("a", -1));
            Assert.Throws<ArgumentException>(() => book.SetLimit("", 10));
        }

        [Test]
        public void IsExhausted_AfterTheLimit_ButNotBefore()
        {
            var book = new ScreenTimeBook();
            book.AddUsage("a", Today, 9 * 60 + 59);

            Assert.IsFalse(book.IsExhausted("a", AgeBand.Minik, Today));
            Assert.AreEqual(1.0, book.RemainingSeconds("a", AgeBand.Minik, Today), 1e-6);

            book.AddUsage("a", Today, 1);
            Assert.IsTrue(book.IsExhausted("a", AgeBand.Minik, Today));
            Assert.AreEqual(0.0, book.RemainingSeconds("a", AgeBand.Minik, Today));
        }

        [Test]
        public void Usage_ResetsEachDayAndEachProfileHasItsOwn()
        {
            var book = new ScreenTimeBook();
            book.AddUsage("a", Today, 600);

            Assert.IsTrue(book.IsExhausted("a", AgeBand.Minik, Today));
            Assert.IsFalse(book.IsExhausted("a", AgeBand.Minik, Today.AddDays(1)), "a new day starts fresh");
            Assert.IsFalse(book.IsExhausted("b", AgeBand.Minik, Today), "another child is not affected");
        }

        [Test]
        public void Unlimited_NeverRunsOut()
        {
            var book = new ScreenTimeBook();
            book.SetLimit("a", 0);
            book.AddUsage("a", Today, 100000);

            Assert.IsFalse(book.IsExhausted("a", AgeBand.Minik, Today));
            Assert.IsTrue(double.IsPositiveInfinity(book.RemainingSeconds("a", AgeBand.Minik, Today)));
        }

        [TestCase(-5.0)]
        [TestCase(0.0)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void AddUsage_IgnoresNonsense(double seconds)
        {
            var book = new ScreenTimeBook();

            book.AddUsage("a", Today, seconds);
            book.AddUsage(null, Today, 10);

            Assert.AreEqual(0.0, book.UsedSeconds("a", Today));
        }

        [Test]
        public void AddUsage_KeepsOnlyTheLastThirtyDays()
        {
            var book = new ScreenTimeBook();
            for (int i = 0; i < 40; i++)
            {
                book.AddUsage("a", Today.AddDays(-i), 5);
            }

            Assert.AreEqual(0.0, book.UsedSeconds("a", Today.AddDays(-39)));
            Assert.AreEqual(5.0, book.UsedSeconds("a", Today.AddDays(-5)));
        }

        [Test]
        public void Forget_RemovesLimitAndUsage()
        {
            var book = new ScreenTimeBook();
            book.SetLimit("a", 30);
            book.AddUsage("a", Today, 100);

            book.Forget("a");
            book.Forget(null);

            Assert.IsFalse(book.HasChosenLimit("a"));
            Assert.AreEqual(0.0, book.UsedSeconds("a", Today));
        }

        [Test]
        public void Retain_DropsEveryoneNotInTheList()
        {
            var book = new ScreenTimeBook();
            book.SetLimit("a", 30);
            book.SetLimit("b", 30);
            book.AddUsage("a", Today, 50);
            book.AddUsage("b", Today, 60);

            book.Retain(new[] { "b" });

            Assert.IsFalse(book.HasChosenLimit("a"));
            Assert.AreEqual(0.0, book.UsedSeconds("a", Today));
            Assert.IsTrue(book.HasChosenLimit("b"));
            Assert.AreEqual(60.0, book.UsedSeconds("b", Today));
        }

        [Test]
        public void Json_RoundTrip_KeepsLimitsAndUsage()
        {
            var book = new ScreenTimeBook();
            book.SetLimit("a", 20);
            book.AddUsage("a", Today, 123.5);

            var copy = ScreenTimeBook.FromJson(book.ToJson());

            Assert.AreEqual(20, copy.LimitMinutes("a", AgeBand.Minik));
            Assert.AreEqual(123.5, copy.UsedSeconds("a", Today), 1e-9);
        }

        [TestCase("{ nope")]
        [TestCase("null")]
        [TestCase("{\"version\": 9}")]
        public void FromJson_BadInput_ThrowsFormat(string json)
        {
            Assert.Throws<FormatException>(() => ScreenTimeBook.FromJson(json));
        }

        [Test]
        public void FromJson_MissingMaps_StartsEmpty()
        {
            var book = ScreenTimeBook.FromJson("{\"version\":1}");

            Assert.AreEqual(10, book.LimitMinutes("a", AgeBand.Minik));
        }
    }
}
