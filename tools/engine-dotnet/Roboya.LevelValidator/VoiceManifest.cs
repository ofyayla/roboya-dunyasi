using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Roboya.LevelValidator;

/// <summary>
/// Checks content/voice/manifest.json against script.csv: every line has an audio file generated from its
/// current text (tools/tts/generate.py writes the manifest). Studio recordings use the same keys.
/// </summary>
internal static class VoiceManifest
{
    public static string TextHash(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)))[..16].ToLowerInvariant();

    public static List<string> Check(VoiceScript script, string manifestPath)
    {
        var errors = new List<string>();
        if (!File.Exists(manifestPath))
        {
            errors.Add($"voice manifest '{manifestPath}' not found (run: make tts)");
            return errors;
        }

        var root = Path.GetDirectoryName(Path.GetFullPath(manifestPath))!;
        var entries = JObject.Parse(File.ReadAllText(manifestPath))["entries"] as JObject ?? new JObject();
        foreach (var (key, text) in script.Texts.OrderBy(k => k.Key, StringComparer.Ordinal))
        {
            if (entries[key] is not JObject entry)
            {
                errors.Add($"voice '{key}' has no audio (run: make tts)");
                continue;
            }

            var file = entry.Value<string>("file") ?? "";
            if (!File.Exists(Path.Combine(root, file)))
            {
                errors.Add($"voice '{key}' audio file '{file}' is missing");
            }
            else if (entry.Value<string>("textHash") != TextHash(text))
            {
                var source = entry.Value<string>("source");
                errors.Add(source == "studio"
                    ? $"voice '{key}' studio recording is stale: text changed, re-record it"
                    : $"voice '{key}' audio is stale: text changed (run: make tts)");
            }
        }

        foreach (var property in entries.Properties())
        {
            if (!script.Texts.ContainsKey(property.Name))
            {
                errors.Add($"voice manifest has '{property.Name}' which is not in script.csv");
            }
        }

        return errors;
    }
}
