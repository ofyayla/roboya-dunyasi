using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Roboya.LevelValidator.Tests;

public class LevelCheckerTests
{
    private static LevelReport Check(JObject level, string path = Fixtures.LevelPath) =>
        LevelChecker.CheckJson(level.ToString(), path, Fixtures.VoiceScript());

    [Test]
    public void CheckJson_ValidLevel_NoErrors()
    {
        var report = Check(Fixtures.Level());

        Assert.That(report.Errors, Is.Empty);
        Assert.That(report.IsValid, Is.True);
        Assert.That(report.ShortestLength, Is.EqualTo(5));
        Assert.That(report.Id, Is.EqualTo("sabir-ormani.yon-avcisi.99"));
    }

    [Test]
    public void CheckJson_WrongStoredShortest_ReportsMismatch()
    {
        var level = Fixtures.Level();
        level["solution"]!["shortestLength"] = 4;

        Assert.That(Check(level).Errors.Single(), Does.Contain("solver says 5"));
    }

    [Test]
    public void CheckJson_MissingSolution_AsksForFix()
    {
        var level = Fixtures.Level();
        level.Remove("solution");

        Assert.That(Check(level).Errors.Single(), Does.Contain("--fix"));
    }

    [Test]
    public void CheckJson_Unsolvable_Reports()
    {
        var level = Fixtures.Level();
        level["cards"]!["maxProgramLength"] = 3;

        Assert.That(Check(level).Errors.Single(), Does.Contain("unsolvable"));
    }

    [Test]
    public void CheckJson_PaletteWithoutMoves_Reports()
    {
        var level = Fixtures.Level();
        level["cards"]!["palette"] = new JArray("repeat");

        Assert.That(Check(level).Errors, Has.Some.Contains("no movement cards"));
    }

    [Test]
    public void CheckJson_IntroducedCardNotInPalette_Reports()
    {
        var level = Fixtures.Level();
        level["cards"]!["introduces"] = "turn_left";

        // The fixture script has no card.turn_left.intro line either; that error is covered separately.
        Assert.That(Check(level).Errors, Has.Some.Contains("is not in the palette"));
    }

    [Test]
    public void CheckJson_UnknownVoiceKey_Reports()
    {
        var level = Fixtures.Level();
        level["voice"]!["success"] = "yon_avcisi.l99.success";
        level["voice"]!["hints"] = new JArray("roboya.hint.yok");

        Assert.That(Check(level).Errors, Has.Count.EqualTo(2));
    }

    [Test]
    public void CheckJson_WrongFileLocationAndIdPrefix_Reports()
    {
        var level = Fixtures.Level();
        level["id"] = "paylasim-koyu.yon-avcisi.99";

        var report = Check(level, "baska/yer.json");

        Assert.That(report.Errors, Has.Some.Contains("must start with"));
        Assert.That(report.Errors, Has.Some.Contains("file must live"));
    }

    [Test]
    public void CheckJson_MalformedLevel_ReportsLoaderError()
    {
        var level = Fixtures.Level();
        level["robot"]!["x"] = 9;

        Assert.That(Check(level).Errors.Single(), Does.Contain("not a floor cell"));
        Assert.That(LevelChecker.CheckJson("{", "x.json", null).Errors, Has.Count.EqualTo(1));
    }

    [Test]
    public void CheckJson_StarterProgram_ChecksLengthPaletteAndBrokenness()
    {
        var level = Fixtures.Level();
        level["game"] = "kodlama-kutusu";
        level["id"] = "sabir-ormani.kodlama-kutusu.99";
        level["starterProgram"] = JArray.Parse("""
            [{"op":"forward"},{"op":"forward"},{"op":"turn_right"},{"op":"forward"},{"op":"forward"}]
            """);

        var solved = Check(level, "sabir-ormani/kodlama-kutusu/99.json");
        Assert.That(solved.Warnings, Has.Some.Contains("already solves"));

        level["starterProgram"] = JArray.Parse("""[{"op":"repeat","times":9,"body":[{"op":"turn_left"},{"op":"forward"},{"op":"forward"},{"op":"forward"},{"op":"forward"},{"op":"forward"}]}]""");
        var bad = Check(level, "sabir-ormani/kodlama-kutusu/99.json");
        Assert.That(bad.Errors, Has.Some.Contains("plan strip"));
        Assert.That(bad.Errors, Has.Some.Contains("not in the palette"));

        level["starterProgram"] = JArray.Parse("""[{"op":"repeat","times":0,"body":[]}]""");
        Assert.That(Check(level, "sabir-ormani/kodlama-kutusu/99.json").Errors, Has.Some.Contains("starterProgram is invalid"));
    }

    [Test]
    public void CheckJson_MinikWithLongStrip_Warns()
    {
        var level = Fixtures.Level();
        level["meta"]!["ageLevels"] = new JArray("minik");
        level["cards"]!["maxProgramLength"] = 8;

        Assert.That(Check(level).Warnings, Has.Count.EqualTo(1));
    }

    [Test]
    public void WithShortestLength_WritesSolution()
    {
        var level = Fixtures.Level();
        level.Remove("solution");

        var updated = JObject.Parse(LevelChecker.WithShortestLength(level.ToString(), 7));

        Assert.That((int)updated["solution"]!["shortestLength"]!, Is.EqualTo(7));
    }

    [TestCase("YonAvcisi", "yon-avcisi")]
    [TestCase("SabirOrmani", "sabir-ormani")]
    public void Kebab_PascalCase_Converts(string input, string expected)
    {
        Assert.That(LevelChecker.Kebab(input), Is.EqualTo(expected));
    }

    [Test]
    public void CheckJson_LookOnBlockedCell_NoErrors()
    {
        var level = Fixtures.Level();
        level["grid"]!["rows"] = new JArray("...", ".#.", "...");
        level["grid"]!["looks"] = JArray.Parse("""[{ "x": 1, "y": 1, "look": "log" }]""");

        Assert.That(Check(level).Errors, Is.Empty);
    }

    [Test]
    public void CheckJson_LookOnFloorCell_Reports()
    {
        var level = Fixtures.Level();
        level["grid"]!["looks"] = JArray.Parse("""[{ "x": 1, "y": 1, "look": "tree" }]""");

        Assert.That(Check(level).Errors.Single(), Does.Contain("not a blocked"));
    }

    [Test]
    public void CheckJson_LookListedTwice_Reports()
    {
        var level = Fixtures.Level();
        level["grid"]!["rows"] = new JArray("...", ".#.", "...");
        level["grid"]!["looks"] = JArray.Parse("""[{ "x": 1, "y": 1, "look": "log" }, { "x": 1, "y": 1, "look": "rock" }]""");

        Assert.That(Check(level).Errors.Single(), Does.Contain("twice"));
    }

    [TestCase(-1, 0)]
    [TestCase(3, 2)]
    [TestCase(1, -1)]
    [TestCase(-1, 3)]
    public void CheckJson_SceneryOnOuterRing_NoErrors(int x, int y)
    {
        var level = Fixtures.Level();
        level["scenery"] = JArray.Parse($$"""[{ "x": {{x}}, "y": {{y}}, "look": "tree" }]""");

        Assert.That(Check(level).Errors, Is.Empty);
    }

    [TestCase(1, 1)]
    [TestCase(4, 0)]
    public void CheckJson_SceneryInsideOrFarOutside_Reports(int x, int y)
    {
        var level = Fixtures.Level();
        level["scenery"] = JArray.Parse($$"""[{ "x": {{x}}, "y": {{y}}, "look": "tree" }]""");

        Assert.That(Check(level).Errors.Single(), Does.Contain("scenery"));
    }

    [Test]
    public void CheckJson_IntroducedCardWithoutNarration_Reports()
    {
        var level = Fixtures.Level();
        level["cards"]!["introduces"] = "turn_right";

        Assert.That(Check(level).Errors.Single(), Does.Contain("card.turn_right.intro"));
    }
}
