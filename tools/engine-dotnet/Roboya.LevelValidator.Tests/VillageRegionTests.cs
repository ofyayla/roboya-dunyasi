using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Roboya.CodingEngine.Levels;
using Roboya.CodingEngine.Levels.Generated;

namespace Roboya.LevelValidator.Tests;

public class VillageRegionTests
{
    // Corners of six: starts 1, 7, 13, 19; finals 6, 12, 18, 24.
    private static readonly int[] Starts = [1, 7, 13, 19];

    /// <summary>A whole village that follows PK-0005; only the region rules look at it, so the levels need not be solvable.</summary>
    private static List<JObject> Village(int count = 24)
    {
        var levels = new List<JObject>();
        for (int order = 1; order <= count; order++)
        {
            bool minik = order <= 6;
            var level = minik ? Fixtures.MinikAutoDrop() : Fixtures.KasifDeliver();
            level["id"] = $"paylasim-koyu.bal-pesinde.{order:00}";
            level["order"] = order;
            var options = new JObject { ["resetEachRun"] = true };
            if (minik)
            {
                options["autoDrop"] = true;
                options["clearMemoryEachRun"] = true;
            }
            else
            {
                options["beeMemoryVisible"] = true;
            }

            if (Starts.Contains(order))
            {
                options["intro"] = true;
                options["guided"] = true;
            }

            if (order == 4)
            {
                options["ghostPath"] = true;
            }

            level["options"] = options;
            levels.Add(level);
        }

        return levels;
    }

    private static List<string> Errors(IEnumerable<JObject> levels) =>
        VillageChecks.RegionErrors(levels.Select(l => LevelLoader.Parse(l.ToString())));

    private static JObject Level(List<JObject> village, int order) => village[order - 1];

    private static void HasError(List<string> errors, string part) => Assert.That(errors, Has.Some.Contains(part), string.Join("\n", errors));

    [Test]
    public void RegionErrors_WholeValidVillage_NoErrors() => Assert.That(Errors(Village()), Is.Empty);

    [Test]
    public void RegionErrors_ForestLevelsAreIgnored()
    {
        var forest = Fixtures.Level();

        Assert.That(Errors([forest]), Is.Empty);
    }

    [Test]
    public void RegionErrors_CornerStartWithoutIntro_Reports()
    {
        var village = Village();
        Level(village, 13)["options"]!["intro"] = false;

        HasError(Errors(village), "paylasim-koyu.bal-pesinde.13': a corner's first level is an introduction");
    }

    [Test]
    public void RegionErrors_CornerStartNotGuided_Reports()
    {
        var village = Village();
        Level(village, 19)["options"]!["guided"] = false;

        HasError(Errors(village), "bal-pesinde.19");
    }

    [Test]
    public void RegionErrors_IntroInTheMiddleOfACorner_Reports()
    {
        var village = Village();
        Level(village, 15)["options"]!["intro"] = true;

        HasError(Errors(village), "paylasim-koyu.bal-pesinde.15': only a corner's first level is an introduction");
    }

    [Test]
    public void RegionErrors_HiveOfFiveOutsideACornerFinal_Reports()
    {
        var village = Village();
        Level(village, 10)["drops"]![0]!["need"] = 5;

        HasError(Errors(village), "which is only allowed on a corner final");
    }

    [Test]
    public void RegionErrors_HiveOfFiveOnACornerFinal_IsAccepted()
    {
        var village = Village();
        Level(village, 12)["drops"]![0]!["need"] = 5;

        Assert.That(Errors(village), Is.Empty);
    }

    [Test]
    public void RegionErrors_MinikPartWithAKasifLevel_Reports()
    {
        var village = Village();
        Level(village, 3)["meta"]!["ageLevels"] = new JArray("kasif");

        HasError(Errors(village), "levels 1–6 are Minik only");
    }

    [Test]
    public void RegionErrors_KasifPartWithAMinikLevel_Reports()
    {
        var village = Village();
        Level(village, 9)["meta"]!["ageLevels"] = new JArray("minik");

        HasError(Errors(village), "from 7 on are Kaşif only");
    }

    [Test]
    public void RegionErrors_WorldNotResetting_Reports()
    {
        var village = Village();
        Level(village, 8)["options"] = new JObject { ["beeMemoryVisible"] = true };

        HasError(Errors(village), "restarts the world each run");
    }

    [Test]
    public void RegionErrors_MinikMemoryNotClearing_Reports()
    {
        var village = Village();
        Level(village, 2)["options"]!["clearMemoryEachRun"] = false;

        HasError(Errors(village), "Minik memory clears itself");
    }

    [Test]
    public void RegionErrors_KasifMemoryHidden_Reports()
    {
        var village = Village();
        Level(village, 8)["options"]!["beeMemoryVisible"] = false;

        HasError(Errors(village), "Kaşif sees the bee's memory");
    }

    [Test]
    public void RegionErrors_FourthLevelWithoutGuideOrGhostPath_Reports()
    {
        var village = Village();
        Level(village, 4)["options"]!["ghostPath"] = false;

        HasError(Errors(village), "levels 1 and 4 keep a guide or the ghost path on");
    }

    [Test]
    public void RegionErrors_FirstLevelGhostPathInsteadOfGuide_IsAccepted()
    {
        var village = Village();
        Level(village, 4)["options"]!["guided"] = true;
        Level(village, 4)["options"]!["ghostPath"] = false;

        Assert.That(Errors(village), Is.Empty);
    }

    [Test]
    public void RegionErrors_UnfinishedVillage_OnlyChecksTheFixedParts()
    {
        var village = Village(8);
        Level(village, 3)["options"]!["intro"] = true; // would be wrong in a whole village, unknown in a partial one

        Assert.That(Errors(village), Is.Empty);
    }

    [Test]
    public void RegionErrors_UnfinishedVillageStillKnowsMinikFromKasif()
    {
        var village = Village(8);
        Level(village, 8)["meta"]!["ageLevels"] = new JArray("minik");

        HasError(Errors(village), "from 7 on are Kaşif only");
    }
}
