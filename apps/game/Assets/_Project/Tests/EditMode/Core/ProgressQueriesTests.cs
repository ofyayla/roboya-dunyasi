using NUnit.Framework;
using Roboya.Core;

namespace Roboya.Tests.Core
{
    public class ProgressQueriesTests
    {
        private static string Level(int order, string options)
        {
            return @"{
  ""schemaVersion"": 3,
  ""id"": ""sabir-ormani.yon-avcisi." + order.ToString("00") + @""",
  ""region"": ""sabir-ormani"",
  ""game"": ""yon-avcisi"",
  ""order"": " + order + @",
  ""meta"": { ""concepts"": [""direction""], ""value"": ""patience"", ""difficulty"": 1, ""ageLevels"": [""minik""] },
  ""grid"": { ""rows"": [""...."", ""....""] },
  ""robot"": { ""x"": 0, ""y"": 1, ""facing"": ""north"" },
  ""goal"": { ""reach"": { ""x"": 0, ""y"": 0 } },
  ""cards"": { ""palette"": [""forward""], ""maxProgramLength"": 3 }," + options + @"
  ""voice"": { ""intro"": ""yon_avcisi.l" + order + @".intro"" }
}";
        }

        [Test]
        public void IntroLevels_OptionsIntro_MarksLaterLevelAsTutorial()
        {
            var catalog = LevelCatalog.Parse(new[]
            {
                Level(1, string.Empty),
                Level(2, string.Empty),
                Level(3, @"""options"": { ""intro"": true },"),
            });
            var path = new[] { "sabir-ormani.yon-avcisi.01", "sabir-ormani.yon-avcisi.02", "sabir-ormani.yon-avcisi.03" };

            var flags = ProgressQueries.IntroLevels(catalog, path);

            CollectionAssert.AreEqual(new[] { true, false, true }, flags);
        }
    }
}
