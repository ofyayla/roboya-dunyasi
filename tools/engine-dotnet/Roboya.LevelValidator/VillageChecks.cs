using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.Execution;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Play;
using Roboya.CodingEngine.Progress;
using Roboya.CodingEngine.Solving;
using Roboya.CodingEngine.World;

namespace Roboya.LevelValidator;

/// <summary>
/// The pedagogy decisions for honey levels and Paylaşım Köyü (docs/pedagoji/kararlar/PK-0001..PK-0005) as validator rules, so
/// 24 hand-written levels cannot drift from them. Every message names the decision it enforces. Three layers: honey structure
/// (any level that uses honey), numbers and lengths (the village region) and how the village is put together (whole region).
/// </summary>
internal static class VillageChecks
{
    /// <summary>PK-0003: total honey in a level.</summary>
    public const int MaxHoney = 7;

    /// <summary>PK-0003: what a hive may ask for (4, or 5 on a corner final).</summary>
    public const int MaxNeed = 5;

    /// <summary>PK-0003: "Bırak" cards in the shortest solution.</summary>
    public const int MaxDrops = 6;

    /// <summary>PK-0003: "Bırak" cards when the level also has obstacles or turns.</summary>
    public const int MaxDropsWithObstaclesOrTurns = 4;

    /// <summary>PK-0003: Kaşif shortest solution, finals included.</summary>
    public const int MaxKasifCards = 10;

    /// <summary>PK-0005: the first corner is the Minik part of the village.</summary>
    public const int MinikLevels = 6;

    /// <summary>The village has four corners of six levels.</summary>
    public const int FullVillage = 24;

    public static bool UsesHoney(LevelDto dto) =>
        (dto.Drops?.Count ?? 0) > 0
        || dto.Goal.Deliver != null
        || dto.Goal.Share != null
        || dto.Goal.CarryAtEnd.HasValue
        || dto.Robot.Carrying.HasValue
        || dto.Cards.Palette.Contains(CardId.Drop)
        || (dto.Items?.Any(i => i.Amount.HasValue) ?? false)
        || dto.Options?.AutoDrop == true;

    public static void CheckLevel(LevelDto dto, Level level, SolveResult solve, LevelReport report)
    {
        if (UsesHoney(dto))
        {
            CheckHoney(dto, level, solve, report);
        }

        if (dto.Region == RegionId.PaylasimKoyu)
        {
            CheckVillage(dto, level, solve, report);
        }
    }

    // Layer 1: honey structure.
    private static void CheckHoney(LevelDto dto, Level level, SolveResult solve, LevelReport report)
    {
        var ages = dto.Meta.AgeLevels;
        bool minikOnly = ages.Count == 1 && ages[0] == AgeLevel.Minik;
        bool hasDropCard = dto.Cards.Palette.Contains(CardId.Drop);
        bool autoDrop = dto.Options?.AutoDrop == true;
        int totalHoney = TotalHoney(level);
        int totalNeed = level.Drops.Where(d => d.Kind == DropPointKind.Hive).Sum(d => d.Need);

        if (autoDrop)
        {
            if (!minikOnly)
            {
                report.Errors.Add("options.autoDrop is for Minik levels only: meta.ageLevels must be [minik] (PK-0001)");
            }

            if (hasDropCard)
            {
                report.Errors.Add("a Minik auto-drop level has no drop card: the hive takes the honey itself (PK-0001)");
            }

            if (totalHoney != totalNeed)
            {
                report.Errors.Add($"a Minik auto-drop level needs total honey ({totalHoney}) equal to the total hive need ({totalNeed}) (PK-0001)");
            }
        }

        if (hasDropCard && level.Drops.Count == 0)
        {
            report.Errors.Add("the drop card is in the palette but the level has no drop point (ADR 0030)");
        }

        if (level.Drops.Count > 0 && !hasDropCard && !autoDrop)
        {
            report.Errors.Add("the level has drop points but nothing can drop honey: add the drop card or options.autoDrop (ADR 0030)");
        }

        int delivered = dto.Goal.Deliver?.Sum(d => (int)d.Count) ?? 0;
        if (delivered > totalHoney)
        {
            report.Errors.Add($"goal.deliver asks for {delivered} honey but the level only has {totalHoney}");
        }

        if (dto.Goal.Share != null && totalHoney < dto.Goal.Share.Among.Count)
        {
            report.Errors.Add($"goal.share needs at least one honey per hive but the level only has {totalHoney}");
        }

        if (dto.Options?.AskRemaining == true)
        {
            CheckAskRemaining(dto, level, solve, report, ages.Contains(AgeLevel.Minik));
        }

        if (!dto.Meta.Concepts.Contains(Concept.Counting))
        {
            report.Errors.Add("a honey level must be tagged with the 'counting' concept (PK-0004)");
        }
    }

    private static void CheckAskRemaining(LevelDto dto, Level level, SolveResult solve, LevelReport report, bool hasMinik)
    {
        if (hasMinik)
        {
            report.Errors.Add("options.askRemaining is for Kaşif levels only (PK-0002)");
        }

        if ((dto.Goal.Deliver?.Count ?? 0) == 0 && !dto.Goal.CarryAtEnd.HasValue)
        {
            report.Errors.Add("options.askRemaining needs a goal.deliver or goal.carryAtEnd to ask about (PK-0002)");
        }

        if (!solve.IsSolved)
        {
            return;
        }

        var q = RemainingQuestion.From(level, Run(level, solve).FinalState);
        if (!q.HasDistinctOptions)
        {
            report.Errors.Add($"options.askRemaining needs three different answers, got had {q.Before} / gave {q.Given} / left {q.Left} (PK-0002)");
        }
    }

    // Layer 2: numbers and lengths in the village region.
    private static void CheckVillage(LevelDto dto, Level level, SolveResult solve, LevelReport report)
    {
        var ages = dto.Meta.AgeLevels;
        bool minikOnly = ages.Count == 1 && ages[0] == AgeLevel.Minik;
        int totalHoney = TotalHoney(level);

        if (totalHoney > MaxHoney)
        {
            report.Errors.Add($"a village level holds at most {MaxHoney} honey, this one has {totalHoney} (PK-0003)");
        }

        foreach (var hive in level.Drops.Where(d => d.Kind == DropPointKind.Hive && d.Need > MaxNeed))
        {
            report.Errors.Add($"hive '{hive.Id}' asks for {hive.Need} honey; the most is {MaxNeed}, and 5 only on a corner final (PK-0003)");
        }

        if (minikOnly)
        {
            foreach (var card in dto.Cards.Palette.Where(c => c != CardId.Forward && c != CardId.TurnRight).Distinct())
            {
                report.Errors.Add($"Minik village levels use only forward and turn_right, not '{card}' (PK-0005)");
            }
        }

        if (!solve.IsSolved)
        {
            return;
        }

        int drops = solve.Solution.Count(c => c == CardType.Drop);
        bool turns = solve.Solution.Any(c => c == CardType.TurnLeft || c == CardType.TurnRight);
        bool obstacles = dto.Grid.Rows.Any(r => r.Contains('#'));
        if (drops > MaxDrops)
        {
            report.Errors.Add($"the shortest solution has {drops} drop cards; the most is {MaxDrops} (PK-0003)");
        }
        else if (drops > MaxDropsWithObstaclesOrTurns && (obstacles || turns))
        {
            report.Errors.Add($"with obstacles or turns the shortest solution may have at most {MaxDropsWithObstaclesOrTurns} drop cards, this one has {drops} (PK-0003)");
        }

        if (ages.Contains(AgeLevel.Kasif) && !ages.Contains(AgeLevel.Minik))
        {
            int shortest = solve.ShortestLength;
            if (shortest > MaxKasifCards)
            {
                report.Errors.Add($"a Kaşif village level's shortest solution is at most {MaxKasifCards} cards, this one needs {shortest} (PK-0003)");
            }

            long spare = dto.Cards.MaxProgramLength - shortest;
            if (spare < 1 || spare > 2)
            {
                report.Errors.Add($"cards.maxProgramLength must be 1 or 2 above the shortest solution ({shortest}), it is {dto.Cards.MaxProgramLength} (PK-0003)");
            }
        }
    }

    // Layer 3: how the whole village is put together (PK-0005).
    public static List<string> RegionErrors(IEnumerable<LevelDto> all)
    {
        var errors = new List<string>();
        var village = all.Where(l => l.Region == RegionId.PaylasimKoyu).OrderBy(l => l.Order).ToList();
        bool full = village.Count == FullVillage;
        var starts = new HashSet<int>();
        var ends = new HashSet<int>();
        if (full)
        {
            for (int c = 1; c <= CornerRules.CornerCount(village.Count); c++)
            {
                ends.Add(CornerRules.CornerEnd(c, village.Count) + 1);
                starts.Add(c == 1 ? 1 : CornerRules.CornerEnd(c - 1, village.Count) + 2);
            }
        }

        foreach (var l in village)
        {
            int order = (int)l.Order;
            var o = l.Options;
            bool minik = order <= MinikLevels;

            if (o?.ResetEachRun != true)
            {
                errors.Add($"'{l.Id}': every village level restarts the world each run (options.resetEachRun, PK-0005)");
            }

            if (minik)
            {
                if (l.Meta.AgeLevels.Count != 1 || l.Meta.AgeLevels[0] != AgeLevel.Minik)
                {
                    errors.Add($"'{l.Id}': village levels 1–{MinikLevels} are Minik only (PK-0005)");
                }

                if (o?.ClearMemoryEachRun != true)
                {
                    errors.Add($"'{l.Id}': Minik memory clears itself after a run (options.clearMemoryEachRun, PK-0005)");
                }
            }
            else
            {
                if (l.Meta.AgeLevels.Count != 1 || l.Meta.AgeLevels[0] != AgeLevel.Kasif)
                {
                    errors.Add($"'{l.Id}': village levels from {MinikLevels + 1} on are Kaşif only (PK-0005)");
                }

                if (o?.BeeMemoryVisible != true)
                {
                    errors.Add($"'{l.Id}': Kaşif sees the bee's memory (options.beeMemoryVisible, PK-0005)");
                }
            }

            if ((order == 1 || order == 4) && o?.Guided != true && o?.GhostPath != true)
            {
                errors.Add($"'{l.Id}': village levels 1 and 4 keep a guide or the ghost path on, as Minik's memory is hidden (PK-0005)");
            }

            if (!full)
            {
                continue;
            }

            bool start = starts.Contains(order);
            if (start && (o?.Intro != true || o?.Guided != true))
            {
                errors.Add($"'{l.Id}': a corner's first level is an introduction and guided (options.intro and options.guided, PK-0005)");
            }
            else if (!start && o?.Intro == true)
            {
                errors.Add($"'{l.Id}': only a corner's first level is an introduction (PK-0005)");
            }

            if (!ends.Contains(order))
            {
                foreach (var hive in l.Drops?.Where(d => d.Kind == DropKind.Hive && (d.Need ?? 0) >= MaxNeed) ?? [])
                {
                    errors.Add($"'{l.Id}': hive '{hive.Id}' asks for {hive.Need} honey, which is only allowed on a corner final (PK-0003)");
                }
            }
        }

        return errors;
    }

    private static int TotalHoney(Level level)
    {
        int total = level.Start.Basket;
        foreach (var item in level.Items)
        {
            total += item.Kind == "honey" ? item.Amount : 0;
        }

        return total;
    }

    private static ExecutionResult Run(Level level, SolveResult solve)
    {
        var commands = solve.Solution.Select(c => c == CardType.Drop ? (Command)DropCommand.Instance : MoveCommand.For(c)).ToList();
        return Interpreter.Execute(level, new CodingEngine.Commands.Program(commands));
    }
}
