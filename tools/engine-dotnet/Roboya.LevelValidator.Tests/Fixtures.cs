using Newtonsoft.Json.Linq;

namespace Roboya.LevelValidator.Tests;

internal static class Fixtures
{
    public const string Voice =
        "key,text,context,level_ids\n" +
        "yon_avcisi.l99.intro,\"Merhaba, \"\"Roboya\"\" burada!\",giriş,sabir-ormani.yon-avcisi.99\n";

    public static JObject Level() => JObject.Parse("""
        {
          "schemaVersion": 1,
          "id": "sabir-ormani.yon-avcisi.99",
          "region": "sabir-ormani",
          "game": "yon-avcisi",
          "order": 99,
          "meta": { "concepts": ["direction"], "value": "patience", "difficulty": 1, "ageLevels": ["kasif"] },
          "grid": { "rows": ["...", "...", "..."] },
          "robot": { "x": 0, "y": 2, "facing": "north" },
          "goal": { "reach": { "x": 2, "y": 0 } },
          "cards": { "palette": ["forward", "turn_right"], "maxProgramLength": 6 },
          "voice": { "intro": "yon_avcisi.l99.intro" },
          "solution": { "shortestLength": 5 }
        }
        """);

    public const string LevelPath = "sabir-ormani/yon-avcisi/99.json";

    public static VoiceScript VoiceScript() => LevelValidator.VoiceScript.Parse(Voice);
}
