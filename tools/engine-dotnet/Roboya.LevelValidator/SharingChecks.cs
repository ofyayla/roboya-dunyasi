using Roboya.CodingEngine.Levels;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Sharing;
using Roboya.CodingEngine.World;

namespace Roboya.LevelValidator;

/// <summary>
/// Checks of a village square level (<c>koy-meydani</c>, ADR 0031): it can be won, its stored shortest length is right, the repeat box
/// and the basket limit really change the plan, a prediction has something to ask, and the Minik levels stay simple. The pedagogy rules
/// are pending decisions K8–K12 (docs/pedagoji/bekleyen-kararlar.md); each message names the decision so changing it is a one-line edit.
/// </summary>
internal static class SharingChecks
{
    public static LevelReport Check(LevelDto dto, string path, VoiceScript? voice, LevelReport report)
    {
        SharingLevel level;
        try
        {
            level = LevelLoader.ToSharing(dto);
        }
        catch (Exception e) when (e is LevelValidationException or ArgumentException or InvalidOperationException)
        {
            report.Errors.Add(e.Message);
            return report;
        }

        var solution = SharingSolver.Solve(level);
        if (!solution.IsSolvable)
        {
            report.Errors.Add($"unsolvable within {level.MaxCards} cards");
        }
        else
        {
            report.ShortestLength = solution.Length;
            if (dto.Solution == null)
            {
                report.Errors.Add($"missing solution.shortestLength (expected {solution.Length}; run with --fix)");
            }
            else if (dto.Solution.ShortestLength != solution.Length)
            {
                report.Errors.Add($"solution.shortestLength is {dto.Solution.ShortestLength}, solver says {solution.Length} (run with --fix)");
            }

            if (level.MaxCards > solution.Length + 2)
            {
                report.Warnings.Add($"the plan strip ({level.MaxCards}) is more than 2 cards longer than the shortest plan ({solution.Length})");
            }
        }

        CheckRepeatBox(level, report);
        CheckBasketCap(level, report);
        if (solution.IsSolvable)
        {
            CheckPrediction(level, solution, report);
        }

        CheckMinik(dto, level, solution, report);
        CheckStoryAndGrid(dto, report);
        LevelChecker.CheckLocation(dto, path, report);
        if (voice != null)
        {
            foreach (var key in LevelChecker.VoiceKeys(dto))
            {
                if (!voice.Texts.ContainsKey(key))
                {
                    report.Errors.Add($"voice key '{key}' is not in content/voice/script.csv");
                }
            }
        }

        return report;
    }

    /// <summary>K11: a repeat box that a plan without it could do as well teaches nothing.</summary>
    private static void CheckRepeatBox(SharingLevel level, LevelReport report)
    {
        if (level.RepeatBox != null && SharingSolver.Solve(level, allowRepeat: false).IsSolvable)
        {
            report.Errors.Add("the repeat box is not needed: a plan without it fits the plan strip (K11, ADR 0031)");
        }
    }

    /// <summary>K9: a basket limit that does not make the plan longer only makes the level look harder.</summary>
    private static void CheckBasketCap(SharingLevel level, LevelReport report)
    {
        if (level.BasketCap == 0 || level.RepeatBox != null)
        {
            return;
        }

        var hives = new List<HiveSpec>(level.Hives);
        var unlimited = new SharingLevel(level.Id, hives, level.Flowers, level.BasketStart, 0, level.HasTable, level.Goal, level.AllowsAmounts, level.MaxCards);
        int withCap = SharingSolver.Solve(level).Length;
        int without = SharingSolver.Solve(unlimited).Length;
        if (withCap >= 0 && without >= 0 && withCap <= without)
        {
            report.Errors.Add($"the basket limit changes nothing: the shortest plan is {withCap} cards with it and {without} without (K9, ADR 0031)");
        }
    }

    /// <summary>K10: the question needs honey left in the basket, and an answer that is not also "what was given" or "what there was".</summary>
    private static void CheckPrediction(SharingLevel level, SharingSolution solution, LevelReport report)
    {
        if (!level.AsksPrediction)
        {
            return;
        }

        if (level.Goal != SharingGoalKind.Deliver)
        {
            report.Errors.Add("a prediction is asked in delivery levels only (K10, ADR 0031)");
            return;
        }

        int left = SharingPrediction.LeftAfter(level, solution.Programs[0]);
        int before = level.TotalHoney;
        if (left < 1 || left == before || left * 2 == before)
        {
            report.Errors.Add($"the prediction needs honey left that differs from what was given and what there was (left {left} of {before}) (K10, ADR 0031)");
        }
    }

    /// <summary>The first corner is for the youngest: plain "give to each hive", no counts, no limit, no box, no guess (K8).</summary>
    private static void CheckMinik(LevelDto dto, SharingLevel level, SharingSolution solution, LevelReport report)
    {
        if (!dto.Meta.AgeLevels.Contains(AgeLevel.Minik))
        {
            return;
        }

        if (level.AllowsAmounts || level.BasketCap > 0 || level.RepeatBox != null || level.AsksPrediction || level.Flowers.Count > 0 || level.Goal != SharingGoalKind.Deliver)
        {
            report.Errors.Add("Minik village levels use only plain give cards: no counts, basket limit, flowers, repeat box, prediction or sharing goal (K8, ADR 0031)");
        }

        if (solution.IsSolvable && solution.Length > 4)
        {
            report.Errors.Add($"a Minik village level needs at most 4 cards, this one needs {solution.Length} (PK-0005 madde 1, K8)");
        }

        if (level.MaxCards > 6)
        {
            report.Warnings.Add("Minik levels should need at most 4 cards; the plan strip is longer than 6 slots");
        }
    }

    /// <summary>The village has no grid or direction: the fields of the grid games make no sense here (the schema forbids them; this names the mistake).</summary>
    private static void CheckStoryAndGrid(LevelDto dto, LevelReport report)
    {
        if (dto.Grid != null || dto.Robot != null || dto.Goal != null || dto.Cards != null)
        {
            report.Errors.Add("a koy-meydani level has no grid, robot, goal or cards (ADR 0031)");
        }
    }
}
