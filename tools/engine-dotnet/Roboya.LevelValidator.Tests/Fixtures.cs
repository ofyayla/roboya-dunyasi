using Newtonsoft.Json.Linq;

namespace Roboya.LevelValidator.Tests;

internal static class Fixtures
{
    public const string Voice =
        "key,text,context,level_ids\n" +
        "yon_avcisi.l99.intro,\"Merhaba, \"\"Roboya\"\" burada!\",giriş,sabir-ormani.yon-avcisi.99\n";

    public static JObject Level() => JObject.Parse("""
        {
          "schemaVersion": 3,
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

    // Paylaşım Köyü levels. All three are valid on their own; tests break one rule at a time.
    public const string KasifPath = "paylasim-koyu/bal-pesinde/07.json";
    public const string MinikPath = "paylasim-koyu/bal-pesinde/02.json";
    public const string SharePath = "paylasim-koyu/bal-pesinde/08.json";

    /// <summary>Kaşif: carries 5, gives a hive 2 (F F D D), keeps 3. Had 5 / gave 2 / left 3.</summary>
    public static JObject KasifDeliver() => JObject.Parse("""
        {
          "schemaVersion": 3,
          "id": "paylasim-koyu.bal-pesinde.07",
          "region": "paylasim-koyu",
          "game": "bal-pesinde",
          "order": 7,
          "meta": { "concepts": ["counting"], "value": "sharing", "difficulty": 2, "ageLevels": ["kasif"] },
          "grid": { "rows": ["....", "...."] },
          "robot": { "x": 0, "y": 0, "facing": "east", "carrying": 5 },
          "drops": [ { "id": "petek-1", "kind": "hive", "x": 2, "y": 0, "need": 2 } ],
          "goal": { "deliver": [ { "to": "petek-1", "count": 2 } ] },
          "cards": { "palette": ["forward", "drop"], "maxProgramLength": 5 },
          "options": { "resetEachRun": true, "beeMemoryVisible": true },
          "voice": { "intro": "yon_avcisi.l99.intro" },
          "solution": { "shortestLength": 4 }
        }
        """);

    /// <summary>Minik: one honey flower, one hive that needs 2 (auto-drop, so F F is the whole plan).</summary>
    public static JObject MinikAutoDrop() => JObject.Parse("""
        {
          "schemaVersion": 3,
          "id": "paylasim-koyu.bal-pesinde.02",
          "region": "paylasim-koyu",
          "game": "bal-pesinde",
          "order": 2,
          "meta": { "concepts": ["counting"], "value": "sharing", "difficulty": 1, "ageLevels": ["minik"] },
          "grid": { "rows": ["....", "...."] },
          "robot": { "x": 0, "y": 0, "facing": "east" },
          "items": [ { "id": "bal-1", "kind": "honey", "x": 1, "y": 0, "amount": 2 } ],
          "drops": [ { "id": "petek-1", "kind": "hive", "x": 2, "y": 0, "need": 2 } ],
          "goal": { "collect": ["bal-1"], "deliver": [ { "to": "petek-1", "count": 2 } ] },
          "cards": { "palette": ["forward"], "maxProgramLength": 3 },
          "options": { "autoDrop": true, "resetEachRun": true, "clearMemoryEachRun": true, "guided": true },
          "voice": { "intro": "yon_avcisi.l99.intro" },
          "solution": { "shortestLength": 2 }
        }
        """);

    /// <summary>Kaşif share: 5 honey, two hives of 2, the left-over on the table (F D D F D D F D).</summary>
    public static JObject KasifShare() => JObject.Parse("""
        {
          "schemaVersion": 3,
          "id": "paylasim-koyu.bal-pesinde.08",
          "region": "paylasim-koyu",
          "game": "bal-pesinde",
          "order": 8,
          "meta": { "concepts": ["counting"], "value": "sharing", "difficulty": 3, "ageLevels": ["kasif"] },
          "grid": { "rows": ["......", "......"] },
          "robot": { "x": 0, "y": 0, "facing": "east", "carrying": 5 },
          "drops": [
            { "id": "petek-1", "kind": "hive", "x": 1, "y": 0, "need": 2 },
            { "id": "petek-2", "kind": "hive", "x": 2, "y": 0, "need": 2 },
            { "id": "sofra", "kind": "table", "x": 3, "y": 0 }
          ],
          "goal": { "share": { "among": ["petek-1", "petek-2"], "leftoverTo": "sofra" } },
          "cards": { "palette": ["forward", "drop"], "maxProgramLength": 9 },
          "options": { "resetEachRun": true, "beeMemoryVisible": true },
          "voice": { "intro": "yon_avcisi.l99.intro" },
          "solution": { "shortestLength": 8 }
        }
        """);

    public const string LevelPath = "sabir-ormani/yon-avcisi/99.json";

    public static VoiceScript VoiceScript() => LevelValidator.VoiceScript.Parse(Voice);
}
