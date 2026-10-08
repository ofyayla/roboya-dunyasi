using Newtonsoft.Json.Linq;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.Execution;
using Roboya.CodingEngine.Levels;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Solving;
using Roboya.CodingEngine.World;

namespace Roboya.LevelValidator;

/// <summary>
/// Semantic checks the JSON schema cannot express: solvability, stored shortest length, voice keys,
/// file location, unique ids and order, alternative levels. JSON-schema checks run in `npm run validate`.
/// </summary>
internal static class LevelChecker
{
    public static LevelReport CheckJson(string json, string path, VoiceScript? voice)
    {
        var report = new LevelReport(path);
        LevelDto dto;
        Level level;
        try
        {
            dto = LevelLoader.Parse(json);
            report.Id = dto.Id;
            level = LevelLoader.ToLevel(dto);
        }
        catch (Exception e) when (e is LevelValidationException or ArgumentException or InvalidOperationException)
        {
            report.Errors.Add(e.Message);
            return report;
        }

        var solve = Solver.Solve(level);
        switch (solve.Status)
        {
            case SolveStatus.Unsolvable:
                report.Errors.Add($"unsolvable within {level.MaxProgramLength} cards using palette [{string.Join(", ", dto.Cards.Palette)}]");
                break;
            case SolveStatus.SearchLimitReached:
                report.Errors.Add("solver search limit reached; simplify the level");
                break;
            default:
                report.ShortestLength = solve.ShortestLength;
                if (dto.Solution == null)
                {
                    report.Errors.Add($"missing solution.shortestLength (expected {solve.ShortestLength}; run with --fix)");
                }
                else if (dto.Solution.ShortestLength != solve.ShortestLength)
                {
                    report.Errors.Add($"solution.shortestLength is {dto.Solution.ShortestLength}, solver says {solve.ShortestLength} (run with --fix)");
                }

                break;
        }

        bool hasPrimitive = level.AvailableCards.Any(c => c.IsPrimitiveMove());
        if (!hasPrimitive)
        {
            report.Errors.Add("palette has no movement cards");
        }

        if (dto.Cards.Introduces.HasValue && !dto.Cards.Palette.Contains(dto.Cards.Introduces.Value))
        {
            report.Errors.Add($"cards.introduces '{dto.Cards.Introduces}' is not in the palette");
        }

        if (dto.StarterProgram != null)
        {
            CheckStarterProgram(dto, level, report);
        }

        CheckLooks(dto, level, report);
        CheckScenery(dto, level, report);
        CheckLocation(dto, path, report);

        if (voice != null)
        {
            foreach (var key in VoiceKeys(dto))
            {
                if (!voice.Texts.ContainsKey(key))
                {
                    report.Errors.Add($"voice key '{key}' is not in content/voice/script.csv");
                }
            }
        }

        if (dto.Meta.AgeLevels.Contains(AgeLevel.Minik) && level.MaxProgramLength > 6)
        {
            report.Warnings.Add("Minik levels should need at most 4 commands; plan strip is longer than 6 slots");
        }

        return report;
    }

    /// <summary>Obstacle looks (v2) must dress a blocked cell, once.</summary>
    private static void CheckLooks(LevelDto dto, Level level, LevelReport report)
    {
        var seen = new HashSet<(long, long)>();
        foreach (var look in dto.Grid.Looks ?? [])
        {
            var at = new GridPosition((int)look.X, (int)look.Y);
            if (!level.Grid.Contains(at) || level.Grid[at] != CellType.Blocked)
            {
                report.Errors.Add($"grid.looks ({look.X},{look.Y}) is not a blocked '#' cell");
            }

            if (!seen.Add((look.X, look.Y)))
            {
                report.Errors.Add($"grid.looks ({look.X},{look.Y}) is listed twice");
            }
        }
    }

    /// <summary>Scenery (v2) sits just outside the playable cells, so it can never be mistaken for a path.</summary>
    private static void CheckScenery(LevelDto dto, Level level, LevelReport report)
    {
        int w = level.Grid.Width;
        int h = level.Grid.Height;
        foreach (var item in dto.Scenery ?? [])
        {
            bool inRange = item.X >= -1 && item.X <= w && item.Y >= -1 && item.Y <= h;
            bool outside = item.X == -1 || item.X == w || item.Y == -1 || item.Y == h;
            if (!inRange || !outside)
            {
                report.Errors.Add($"scenery ({item.X},{item.Y}) must be on the ring just outside the grid (x or y = -1 or the grid size)");
            }
        }
    }

    /// <summary>Writes the solver's shortest length into the file, preserving key order.</summary>
    public static string WithShortestLength(string json, int shortest)
    {
        var obj = JObject.Parse(json);
        obj["solution"] = new JObject { ["shortestLength"] = shortest };
        return obj.ToString(Newtonsoft.Json.Formatting.Indented) + "\n";
    }

    public static IEnumerable<string> VoiceKeys(LevelDto dto)
    {
        yield return dto.Voice.Intro;
        if (dto.Voice.Success != null)
        {
            yield return dto.Voice.Success;
        }

        if (dto.Cards?.Introduces is { } introduced)
        {
            yield return LevelStory.CardIntroVoice(introduced);
        }

        foreach (var hint in dto.Voice.Hints ?? [])
        {
            yield return hint;
        }
    }

    private static void CheckStarterProgram(LevelDto dto, Level level, LevelReport report)
    {
        Roboya.CodingEngine.Commands.Program starter;
        try
        {
            starter = LevelLoader.ToProgram(dto.StarterProgram);
        }
        catch (Exception e) when (e is ArgumentException or LevelValidationException)
        {
            report.Errors.Add("starterProgram is invalid: " + e.Message);
            return;
        }

        if (starter.CardCount > level.MaxProgramLength)
        {
            report.Errors.Add($"starterProgram uses {starter.CardCount} cards but the plan strip has {level.MaxProgramLength}");
        }

        foreach (var card in Cards(dto.StarterProgram))
        {
            if (!level.AvailableCards.Contains(card))
            {
                report.Errors.Add($"starterProgram uses card '{card}' that is not in the palette");
                break;
            }
        }

        // A bug-hunt level must start broken, otherwise there is nothing to find (KUT-01).
        if (dto.Game == GameId.KodlamaKutusu && Interpreter.Execute(level, starter).IsSuccess)
        {
            report.Warnings.Add("starterProgram already solves the level");
        }
    }

    private static IEnumerable<CardType> Cards(IEnumerable<CommandDto> block)
    {
        foreach (var c in block)
        {
            yield return LevelLoader.ToCard(c.Op);
            foreach (var nested in Cards((c.Body ?? []).Concat(c.Then ?? []).Concat(c.Else ?? [])))
            {
                yield return nested;
            }
        }
    }

    private static void CheckLocation(LevelDto dto, string path, LevelReport report)
    {
        var parts = dto.Id.Split('.');
        var region = Kebab(dto.Region.ToString());
        var game = Kebab(dto.Game.ToString());
        if (parts[0] != region || parts[1] != game)
        {
            report.Errors.Add($"id '{dto.Id}' must start with '{region}.{game}.'");
        }

        var expected = System.IO.Path.Combine(region, game, parts[^1] + ".json");
        var normalized = path.Replace('\\', '/');
        if (!normalized.EndsWith(expected.Replace('\\', '/'), StringComparison.Ordinal))
        {
            report.Errors.Add($"file must live at .../{expected}");
        }
    }

    // GameId.YonAvcisi -> "yon-avcisi"
    internal static string Kebab(string pascal) =>
        string.Concat(pascal.Select((ch, i) => char.IsUpper(ch) && i > 0 ? "-" + char.ToLowerInvariant(ch) : char.ToLowerInvariant(ch).ToString()));
}
