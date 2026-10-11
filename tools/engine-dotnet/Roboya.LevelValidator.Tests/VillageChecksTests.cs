using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Roboya.LevelValidator.Tests;

public class VillageChecksTests
{
    private static LevelReport Check(JObject level, string path) =>
        LevelChecker.CheckJson(level.ToString(), path, Fixtures.VoiceScript());

    private static LevelReport Kasif(Action<JObject>? change = null)
    {
        var level = Fixtures.KasifDeliver();
        change?.Invoke(level);
        return Check(level, Fixtures.KasifPath);
    }

    private static LevelReport Minik(Action<JObject>? change = null)
    {
        var level = Fixtures.MinikAutoDrop();
        change?.Invoke(level);
        return Check(level, Fixtures.MinikPath);
    }

    private static LevelReport Share(Action<JObject>? change = null)
    {
        var level = Fixtures.KasifShare();
        change?.Invoke(level);
        return Check(level, Fixtures.SharePath);
    }

    private static void HasError(LevelReport report, string part) =>
        Assert.That(report.Errors, Has.Some.Contains(part), string.Join("\n", report.Errors));

    // The three valid fixtures.

    [Test]
    public void CheckJson_ValidKasifDeliverLevel_NoErrors() => Assert.That(Kasif().Errors, Is.Empty);

    [Test]
    public void CheckJson_ValidMinikAutoDropLevel_NoErrors() => Assert.That(Minik().Errors, Is.Empty);

    [Test]
    public void CheckJson_ValidKasifShareLevel_NoErrors() => Assert.That(Share().Errors, Is.Empty);

    [Test]
    public void CheckJson_LevelWithoutHoney_IsNotJudgedByHoneyRules()
    {
        Assert.That(Check(Fixtures.Level(), Fixtures.LevelPath).Errors, Is.Empty);
    }

    // Layer 1: honey structure.

    [Test]
    public void CheckJson_AutoDropOnKasifLevel_Reports()
    {
        HasError(Minik(l => l["meta"]!["ageLevels"] = new JArray("kasif")), "autoDrop is for Minik levels only");
    }

    [Test]
    public void CheckJson_AutoDropWithDropCard_Reports()
    {
        HasError(Minik(l => l["cards"]!["palette"] = new JArray("forward", "drop")), "no drop card");
    }

    [Test]
    public void CheckJson_AutoDropHoneyDoesNotMatchNeed_Reports()
    {
        HasError(Minik(l => l["items"]![0]!["amount"] = 3), "equal to the total hive need");
    }

    [Test]
    public void CheckJson_DropCardWithoutDropPoint_Reports()
    {
        HasError(Kasif(l =>
        {
            l.Remove("drops");
            l["goal"] = JObject.Parse("""{ "reach": { "x": 1, "y": 0 } }""");
        }), "no drop point");
    }

    [Test]
    public void CheckJson_DropPointsButNothingDropsHoney_Reports()
    {
        HasError(Kasif(l => l["cards"]!["palette"] = new JArray("forward")), "nothing can drop honey");
    }

    [Test]
    public void CheckJson_DeliverMoreThanTheLevelHas_Reports()
    {
        HasError(Kasif(l =>
        {
            l["robot"]!["carrying"] = 1;
            l["goal"]!["deliver"]![0]!["count"] = 2;
        }), "asks for 2 honey but the level only has 1");
    }

    [Test]
    public void CheckJson_ShareWithFewerHoneyThanHives_Reports()
    {
        HasError(Share(l => l["robot"]!["carrying"] = 1), "at least one honey per hive");
    }

    [Test]
    public void CheckJson_AskRemainingOnMinikLevel_Reports()
    {
        HasError(Minik(l => l["options"]!["askRemaining"] = true), "askRemaining is for Kaşif levels only");
    }

    [Test]
    public void CheckJson_AskRemainingWithoutQuantityGoal_Reports()
    {
        HasError(Share(l => l["options"]!["askRemaining"] = true), "needs a goal.deliver or goal.carryAtEnd");
    }

    [Test]
    public void CheckJson_AskRemainingWithThreeDifferentAnswers_IsAccepted()
    {
        Assert.That(Kasif(l => l["options"]!["askRemaining"] = true).Errors, Is.Empty);
    }

    [Test]
    public void CheckJson_AskRemainingWithoutItsSpokenLines_Reports()
    {
        var level = Fixtures.KasifDeliver();
        level["options"]!["askRemaining"] = true;

        // A script that has the level's own line but none of the three step lines.
        var script = VoiceScript.Parse("key,text,context,level_ids\nyon_avcisi.l99.intro,Merhaba,giriş,\n");
        var report = LevelChecker.CheckJson(level.ToString(), Fixtures.KasifPath, script);

        Assert.That(report.Errors, Is.EquivalentTo(LevelChecker.RemainingVoiceKeys.Select(k => $"voice key '{k}' is not in content/voice/script.csv")));
    }

    [Test]
    public void CheckJson_AskRemainingWithTwoEqualAnswers_Reports()
    {
        HasError(Kasif(l =>
        {
            l["options"]!["askRemaining"] = true;
            l["robot"]!["carrying"] = 4;
        }), "had 4 / gave 2 / left 2");
    }

    [Test]
    public void CheckJson_HoneyLevelWithoutCountingConcept_Reports()
    {
        HasError(Kasif(l => l["meta"]!["concepts"] = new JArray("direction")), "'counting' concept");
    }

    // Layer 2: numbers and lengths in the village.

    [Test]
    public void CheckJson_MoreThanSevenHoney_Reports()
    {
        HasError(Kasif(l => l["robot"]!["carrying"] = 8), "at most 7 honey");
    }

    [Test]
    public void CheckJson_HiveAsksForSix_Reports()
    {
        HasError(Kasif(l =>
        {
            l["robot"]!["carrying"] = 6;
            l["drops"]![0]!["need"] = 6;
            l["goal"]!["deliver"]![0]!["count"] = 6;
        }), "asks for 6 honey");
    }

    [Test]
    public void CheckJson_SevenDropCards_Reports()
    {
        var report = Share(l =>
        {
            l["robot"]!["carrying"] = 7;
            l["drops"]![0]!["need"] = 3;
            l["drops"]![1]!["need"] = 3;
            l["cards"]!["maxProgramLength"] = 12;
        });

        HasError(report, "7 drop cards; the most is 6");
    }

    [Test]
    public void CheckJson_FiveDropCardsOnAnOpenStraightLine_IsAccepted()
    {
        var report = Kasif(l =>
        {
            l["robot"]!["carrying"] = 5;
            l["drops"]![0]!["need"] = 5;
            l["goal"]!["deliver"]![0]!["count"] = 5;
            l["cards"]!["maxProgramLength"] = 8;
            l["solution"]!["shortestLength"] = 7;
        });

        Assert.That(report.Errors, Is.Empty);
    }

    [Test]
    public void CheckJson_FiveDropCardsWithAnObstacle_Reports()
    {
        var report = Kasif(l =>
        {
            l["robot"]!["carrying"] = 5;
            l["drops"]![0]!["need"] = 5;
            l["goal"]!["deliver"]![0]!["count"] = 5;
            l["grid"]!["rows"] = new JArray("....", ".#..");
            l["cards"]!["maxProgramLength"] = 8;
            l["solution"]!["shortestLength"] = 7;
        });

        HasError(report, "with obstacles or turns");
    }

    [Test]
    public void CheckJson_KasifSolutionLongerThanTenCards_Reports()
    {
        var report = Kasif(l =>
        {
            l["grid"]!["rows"] = new JArray("............", "............");
            l["drops"]![0]!["x"] = 9;
            l["cards"]!["maxProgramLength"] = 12;
            l["solution"]!["shortestLength"] = 11;
        });

        HasError(report, "shortest solution is at most 10 cards, this one needs 11");
    }

    [Test]
    public void CheckJson_StripMoreThanTwoAboveTheShortest_Reports()
    {
        HasError(Kasif(l => l["cards"]!["maxProgramLength"] = 7), "must be 1 or 2 above the shortest solution (4)");
    }

    [Test]
    public void CheckJson_StripEqualToTheShortest_Reports()
    {
        HasError(Kasif(l => l["cards"]!["maxProgramLength"] = 4), "must be 1 or 2 above");
    }

    [Test]
    public void CheckJson_MinikLevelWithTurnLeft_Reports()
    {
        HasError(Minik(l => l["cards"]!["palette"] = new JArray("forward", "turn_left")), "only forward and turn_right, not 'TurnLeft'");
    }

    [Test]
    public void CheckJson_VillageRulesDoNotApplyToTheForest()
    {
        var level = Kasif(l =>
        {
            l["region"] = "sabir-ormani";
            l["id"] = "sabir-ormani.bal-pesinde.07";
            l["robot"]!["carrying"] = 8;
        });

        Assert.That(level.Errors, Has.None.Contains("at most 7 honey"));
    }
}
