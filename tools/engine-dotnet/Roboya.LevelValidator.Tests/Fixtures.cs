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


    public const string VillagePath = "paylasim-koyu/koy-meydani/01.json";

    /// <summary>Corner 1 (Minik): two hives that ask for one honey each, two honey in the basket, F2-13 / ADR 0031.</summary>
    public static JObject Village() => JObject.Parse("""
        {
          "schemaVersion": 3,
          "id": "paylasim-koyu.koy-meydani.01",
          "region": "paylasim-koyu",
          "game": "koy-meydani",
          "order": 1,
          "meta": { "concepts": ["counting"], "value": "sharing", "difficulty": 1, "ageLevels": ["minik"] },
          "sharing": {
            "hives": [ { "need": 1 }, { "need": 1 } ],
            "basket": { "start": 2 },
            "goal": "deliver",
            "limits": { "maxCards": 3 }
          },
          "voice": { "intro": "yon_avcisi.l99.intro" },
          "solution": { "shortestLength": 2 }
        }
        """);

    /// <summary>Corner 2 (Kaşif): a basket that holds two, two flowers, one hive asks for three.</summary>
    public static JObject VillageBasket() => JObject.Parse("""
        {
          "schemaVersion": 3,
          "id": "paylasim-koyu.koy-meydani.01",
          "region": "paylasim-koyu",
          "game": "koy-meydani",
          "order": 1,
          "meta": { "concepts": ["counting"], "value": "sharing", "difficulty": 2, "ageLevels": ["kasif"] },
          "sharing": {
            "hives": [ { "need": 3 } ],
            "flowers": [2, 1],
            "basket": { "cap": 2 },
            "goal": "deliver",
            "amounts": true,
            "limits": { "maxCards": 5 }
          },
          "voice": { "intro": "yon_avcisi.l99.intro" },
          "solution": { "shortestLength": 4 }
        }
        """);

    /// <summary>Corner 4 (Kaşif): six honey, three hives, shared with the repeat box.</summary>
    public static JObject VillageRepeat() => JObject.Parse("""
        {
          "schemaVersion": 3,
          "id": "paylasim-koyu.koy-meydani.01",
          "region": "paylasim-koyu",
          "game": "koy-meydani",
          "order": 1,
          "meta": { "concepts": ["counting", "loops"], "value": "sharing", "difficulty": 3, "ageLevels": ["kasif"] },
          "sharing": {
            "hives": [ {}, {}, {} ],
            "basket": { "start": 6 },
            "goal": "share",
            "limits": { "maxCards": 3 },
            "repeatBox": { "slots": 3, "maxTurns": 3 }
          },
          "voice": { "intro": "yon_avcisi.l99.intro" },
          "solution": { "shortestLength": 3 }
        }
        """);

    public const string LevelPath = "sabir-ormani/yon-avcisi/99.json";

    public static VoiceScript VoiceScript() => LevelValidator.VoiceScript.Parse(Voice);
}
