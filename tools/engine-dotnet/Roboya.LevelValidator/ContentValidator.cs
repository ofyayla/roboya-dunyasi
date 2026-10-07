using Roboya.CodingEngine.Levels;

namespace Roboya.LevelValidator;

/// <summary>Validates a whole content tree: per-level checks plus cross-level rules.</summary>
internal sealed class ContentValidator(TextWriter output)
{
    public int Run(string levelsDir, string? voicePath, bool fix)
    {
        if (!Directory.Exists(levelsDir))
        {
            output.WriteLine($"error: levels directory '{levelsDir}' not found");
            return 2;
        }

        VoiceScript? voice = null;
        int errors = 0;
        if (voicePath != null)
        {
            voice = VoiceScript.Parse(File.ReadAllText(voicePath));
            foreach (var e in voice.Errors)
            {
                output.WriteLine("✗ " + e);
                errors++;
            }
        }

        var files = Directory.GetFiles(levelsDir, "*.json", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal).ToList();
        var reports = new List<LevelReport>();
        foreach (var file in files)
        {
            var json = File.ReadAllText(file);
            var report = LevelChecker.CheckJson(json, Path.GetRelativePath(levelsDir, file), voice);
            if (fix && report.ShortestLength.HasValue && report.Errors.Any(e => e.Contains("shortestLength", StringComparison.Ordinal)))
            {
                File.WriteAllText(file, LevelChecker.WithShortestLength(json, report.ShortestLength.Value));
                report.Errors.RemoveAll(e => e.Contains("shortestLength", StringComparison.Ordinal));
                report.Fixed = true;
            }

            reports.Add(report);
        }

        var crossErrors = CrossLevelErrors(levelsDir, files, reports);

        foreach (var r in reports)
        {
            string status = r.IsValid ? "✓" : "✗";
            string shortest = r.ShortestLength.HasValue ? $" (en kısa: {r.ShortestLength})" : "";
            string fixedNote = r.Fixed ? " [düzeltildi]" : "";
            output.WriteLine($"{status} {r.Path}{shortest}{fixedNote}");
            foreach (var e in r.Errors)
            {
                output.WriteLine("    hata: " + e);
            }

            foreach (var w in r.Warnings)
            {
                output.WriteLine("    uyarı: " + w);
            }

            errors += r.Errors.Count;
        }

        foreach (var e in crossErrors)
        {
            output.WriteLine("✗ " + e);
        }

        errors += crossErrors.Count;
        output.WriteLine($"{files.Count} bölüm, {errors} hata.");
        return errors == 0 ? 0 : 1;
    }

    private static List<string> CrossLevelErrors(string levelsDir, List<string> files, List<LevelReport> reports)
    {
        var errors = new List<string>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var orders = new Dictionary<string, string>(StringComparer.Ordinal);
        var alternatives = new List<(string Id, string Alt)>();

        foreach (var file in files)
        {
            Roboya.CodingEngine.Levels.Generated.LevelDto dto;
            try
            {
                dto = LevelLoader.Parse(File.ReadAllText(file));
            }
            catch (Exception)
            {
                continue; // Already reported per level.
            }

            if (!ids.Add(dto.Id))
            {
                errors.Add($"duplicate level id '{dto.Id}'");
            }

            var orderKey = dto.Region + "#" + dto.Order;
            if (orders.TryGetValue(orderKey, out var other))
            {
                errors.Add($"'{dto.Id}' and '{other}' share order {dto.Order} in region {LevelChecker.Kebab(dto.Region.ToString())}");
            }
            else
            {
                orders[orderKey] = dto.Id;
            }

            if (dto.AlternativeLevelId != null)
            {
                alternatives.Add((dto.Id, dto.AlternativeLevelId));
            }
        }

        foreach (var (id, alt) in alternatives)
        {
            if (!ids.Contains(alt))
            {
                errors.Add($"'{id}' points to missing alternativeLevelId '{alt}'");
            }
            else if (alt == id)
            {
                errors.Add($"'{id}' cannot be its own alternative");
            }
        }

        return errors;
    }
}
