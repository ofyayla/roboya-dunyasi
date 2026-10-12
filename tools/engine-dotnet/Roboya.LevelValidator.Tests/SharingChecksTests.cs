using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Roboya.LevelValidator.Tests;

public class SharingChecksTests
{
    private static LevelReport Check(JObject level, string path = Fixtures.VillagePath) =>
        LevelChecker.CheckJson(level.ToString(), path, Fixtures.VoiceScript());

    [Test]
    public void CheckJson_ValidMinikLevel_NoErrors()
    {
        var report = Check(Fixtures.Village());

        Assert.That(report.Errors, Is.Empty);
        Assert.That(report.ShortestLength, Is.EqualTo(2));
        Assert.That(report.Id, Is.EqualTo("paylasim-koyu.koy-meydani.01"));
    }

    [Test]
    public void CheckJson_ValidBasketAndRepeatLevels_NoErrors()
    {
        Assert.That(Check(Fixtures.VillageBasket()).Errors, Is.Empty);
        Assert.That(Check(Fixtures.VillageRepeat()).Errors, Is.Empty);
    }

    [Test]
    public void CheckJson_WrongStoredShortest_ReportsMismatch()
    {
        var level = Fixtures.Village();
        level["solution"]!["shortestLength"] = 3;

        Assert.That(Check(level).Errors.Single(), Does.Contain("solver says 2"));
    }

    [Test]
    public void CheckJson_MissingSolution_AsksForFix()
    {
        var level = Fixtures.Village();
        level.Remove("solution");

        Assert.That(Check(level).Errors.Single(), Does.Contain("--fix"));
    }

    [Test]
    public void CheckJson_Unsolvable_Reports()
    {
        var level = Fixtures.Village();
        level["sharing"]!["limits"]!["maxCards"] = 1;

        Assert.That(Check(level).Errors.Single(), Does.Contain("unsolvable"));
    }

    [Test]
    public void CheckJson_LongStrip_Warns()
    {
        var level = Fixtures.Village();
        level["sharing"]!["limits"]!["maxCards"] = 6;

        var report = Check(level);

        Assert.That(report.Errors, Is.Empty);
        Assert.That(report.Warnings, Has.Some.Contains("more than 2 cards longer"));
    }

    [Test]
    public void CheckJson_MinikWithAmounts_NamesTheDecision()
    {
        var level = Fixtures.Village();
        level["sharing"]!["amounts"] = true;

        Assert.That(Check(level).Errors, Has.Some.Contains("K8"));
    }

    [Test]
    public void CheckJson_MinikLevelNeedingFiveCards_IsRejected()
    {
        var level = Fixtures.Village();
        level["sharing"]!["hives"] = new JArray(new JObject { ["need"] = 1 }, new JObject { ["need"] = 2 }, new JObject { ["need"] = 2 });
        level["sharing"]!["basket"]!["start"] = 5;
        level["sharing"]!["limits"]!["maxCards"] = 6;
        level["solution"]!["shortestLength"] = 5;

        Assert.That(Check(level).Errors, Has.Some.Contains("at most 4 cards").And.Contain("K8"));
    }

    [Test]
    public void CheckJson_MinikWithALongStrip_Warns()
    {
        var level = Fixtures.Village();
        level["sharing"]!["basket"]!["start"] = 1;
        level["sharing"]!["hives"] = new JArray(new JObject { ["need"] = 1 });
        level["sharing"]!["limits"]!["maxCards"] = 7;
        level["solution"]!["shortestLength"] = 1;

        Assert.That(Check(level).Warnings, Has.Some.Contains("at most 4 cards"));
    }

    [Test]
    public void CheckJson_RepeatBoxThatIsNotNeeded_NamesTheDecision()
    {
        var level = Fixtures.VillageRepeat();
        level["sharing"]!["limits"]!["maxCards"] = 6;

        Assert.That(Check(level).Errors, Has.Some.Contains("K11"));
    }

    [Test]
    public void CheckJson_BasketLimitThatChangesNothing_NamesTheDecision()
    {
        var level = Fixtures.VillageBasket();
        level["sharing"]!["basket"]!["cap"] = 3;
        level["solution"]!["shortestLength"] = 3;

        Assert.That(Check(level).Errors, Has.Some.Contains("K9"));
    }

    [Test]
    public void CheckJson_PredictionNeedsHoneyLeftThatDiffers()
    {
        var level = Fixtures.VillageBasket();
        level["sharing"] = JObject.Parse("""
            { "hives": [ { "need": 2 } ], "basket": { "start": 5 }, "goal": "deliver", "amounts": true, "predict": true, "limits": { "maxCards": 2 } }
            """);
        level["solution"]!["shortestLength"] = 1;
        Assert.That(Check(level).Errors, Is.Empty, "5 honey, 2 given, 3 left");

        level["sharing"]!["basket"]!["start"] = 4;
        Assert.That(Check(level).Errors, Has.Some.Contains("K10"), "2 given and 2 left");

        level["sharing"]!["basket"]!["start"] = 2;
        Assert.That(Check(level).Errors, Has.Some.Contains("K10"), "nothing left");
    }

    [Test]
    public void CheckJson_PredictionInAShareLevel_IsRejected()
    {
        var level = Fixtures.VillageRepeat();
        level["sharing"]!["predict"] = true;

        Assert.That(Check(level).Errors, Has.Some.Contains("delivery levels only"));
    }

    [Test]
    public void CheckJson_GridFieldsInAVillageLevel_AreNamed()
    {
        var level = Fixtures.Village();
        level["grid"] = new JObject { ["rows"] = new JArray("...") };

        Assert.That(Check(level).Errors, Has.Some.Contains("no grid"));
    }

    [Test]
    public void CheckJson_MissingSharingBlock_Reports()
    {
        var level = Fixtures.Village();
        level.Remove("sharing");

        Assert.That(Check(level).Errors.Single(), Does.Contain("missing sharing"));
    }

    [Test]
    public void CheckJson_WrongFileLocation_Reports()
    {
        Assert.That(Check(Fixtures.Village(), "paylasim-koyu/koy-meydani/02.json").Errors, Has.Some.Contains("file must live"));
    }

    [Test]
    public void CheckJson_UnknownVoiceKey_Reports()
    {
        var level = Fixtures.Village();
        level["voice"]!["intro"] = "koy_meydani.l99.intro";

        Assert.That(Check(level).Errors.Single(), Does.Contain("koy_meydani.l99.intro"));
    }

    [Test]
    public void Handle_VillageLevel_ReturnsTheShortestPlan()
    {
        var response = JObject.Parse(SolveServer.Handle(Fixtures.Village().ToString(), Fixtures.VoiceScript()));

        Assert.That((bool)response["valid"]!, Is.True, response.ToString());
        Assert.That(response["shortestLength"]!.Value<int>(), Is.EqualTo(2));
        Assert.That(response["solution"]!.Select(t => t.ToString()), Is.EqualTo(new[] { "give(0,1)", "give(1,1)" }));
    }
}
