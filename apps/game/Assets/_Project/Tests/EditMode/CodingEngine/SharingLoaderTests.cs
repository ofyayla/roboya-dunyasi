using System;
using NUnit.Framework;
using Roboya.CodingEngine.Levels;
using Roboya.CodingEngine.Sharing;
using Roboya.CodingEngine.World;

namespace Roboya.Tests.CodingEngine
{
    public class SharingLoaderTests
    {
        internal const string Village = @"{
  ""schemaVersion"": 3,
  ""id"": ""paylasim-koyu.koy-meydani.01"",
  ""region"": ""paylasim-koyu"",
  ""game"": ""koy-meydani"",
  ""order"": 1,
  ""meta"": { ""concepts"": [""counting""], ""value"": ""sharing"", ""difficulty"": 1, ""ageLevels"": [""minik""] },
  ""sharing"": {
    ""hives"": [ { ""need"": 1 }, { ""need"": 2, ""has"": 1 } ],
    ""flowers"": [2, 1],
    ""basket"": { ""start"": 1, ""cap"": 3 },
    ""table"": true,
    ""goal"": ""deliver"",
    ""amounts"": true,
    ""limits"": { ""maxCards"": 5 },
    ""repeatBox"": { ""slots"": 2, ""maxTurns"": 3 },
    ""predict"": true
  },
  ""voice"": { ""intro"": ""koy_meydani.l01.intro"" }
}";

        [Test]
        public void ToSharing_ReadsEveryField()
        {
            var level = LevelLoader.LoadSharing(Village);

            Assert.AreEqual("paylasim-koyu.koy-meydani.01", level.Id);
            Assert.AreEqual(2, level.Hives.Count);
            Assert.AreEqual(2, level.Hives[1].Need);
            Assert.AreEqual(1, level.Hives[1].Has);
            CollectionAssert.AreEqual(new[] { 2, 1 }, level.Flowers);
            Assert.AreEqual(1, level.BasketStart);
            Assert.AreEqual(3, level.BasketCap);
            Assert.IsTrue(level.HasTable);
            Assert.AreEqual(SharingGoalKind.Deliver, level.Goal);
            Assert.IsTrue(level.AllowsAmounts);
            Assert.AreEqual(5, level.MaxCards);
            Assert.AreEqual(2, level.RepeatBox.Slots);
            Assert.AreEqual(3, level.RepeatBox.MaxTurns);
            Assert.IsTrue(level.AsksPrediction);
        }

        [Test]
        public void ToSharing_Minimal_UsesTheDefaults()
        {
            var json = @"{
  ""schemaVersion"": 3, ""id"": ""paylasim-koyu.koy-meydani.02"", ""region"": ""paylasim-koyu"", ""game"": ""koy-meydani"", ""order"": 2,
  ""meta"": { ""concepts"": [""counting""], ""value"": ""sharing"", ""difficulty"": 1, ""ageLevels"": [""minik""] },
  ""sharing"": { ""hives"": [ { }, { } ], ""goal"": ""share"", ""limits"": { ""maxCards"": 4 } },
  ""voice"": { ""intro"": ""koy_meydani.l02.intro"" }
}";

            var level = LevelLoader.LoadSharing(json);

            Assert.AreEqual(SharingGoalKind.Share, level.Goal);
            Assert.AreEqual(0, level.BasketStart);
            Assert.AreEqual(0, level.BasketCap);
            Assert.AreEqual(0, level.Flowers.Count);
            Assert.IsFalse(level.HasTable);
            Assert.IsNull(level.RepeatBox);
        }

        [Test]
        public void ToSharing_WithoutTheSharingBlock_Throws()
        {
            var dto = LevelLoader.Parse(Village);
            dto.Sharing = null;

            Assert.Throws<LevelValidationException>(() => LevelLoader.ToSharing(dto));
        }

        [Test]
        public void ToSharing_WithoutLimits_Throws()
        {
            var dto = LevelLoader.Parse(Village);
            dto.Sharing.Limits = null;

            Assert.Throws<LevelValidationException>(() => LevelLoader.ToSharing(dto));
        }

        [Test]
        public void ToSharing_Null_Throws() => Assert.Throws<ArgumentNullException>(() => LevelLoader.ToSharing(null));

        [Test]
        public void ToSharing_ImpossibleLevel_Throws()
        {
            var dto = LevelLoader.Parse(Village);
            dto.Sharing.Basket.Start = 6;

            Assert.Throws<LevelValidationException>(() => LevelLoader.ToSharing(dto));
        }

        [Test]
        public void ToLevel_VillageLevel_ThrowsBecauseItHasNoGrid() =>
            Assert.Throws<LevelValidationException>(() => LevelLoader.Load(Village));
    }
}
