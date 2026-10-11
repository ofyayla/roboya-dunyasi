using System;
using System.Collections.Generic;
using Roboya.CodingEngine.Levels.Generated;

namespace Roboya.Core
{
    /// <summary>What plays at the end of a corner for one value: a silent film, one spoken line per film scene, and a one-line fallback.</summary>
    public sealed class ValueMoment
    {
        public ValueMoment(ValueId value, string filmFile, string fallbackKey, IReadOnlyList<string> sceneKeys)
        {
            Value = value;
            FilmFile = filmFile;
            FallbackKey = fallbackKey;
            SceneKeys = sceneKeys;
        }

        public ValueId Value { get; }

        /// <summary>Content-relative file of the film (content/video); may not exist yet.</summary>
        public string FilmFile { get; }

        /// <summary>The single line spoken over the calm card when there is no film.</summary>
        public string FallbackKey { get; }

        /// <summary>Spoken lines in film order, one per scene; empty when none are written.</summary>
        public IReadOnlyList<string> SceneKeys { get; }
    }

    /// <summary>
    /// The value moment of a level's value (F1-24, plan C4): patience closes the forest's corners, sharing the village's, and so on.
    /// Names follow one pattern, so a new value's film and lines are content only: <c>video/value_&lt;value&gt;.mp4</c>,
    /// <c>value.&lt;value&gt;</c> and <c>value.&lt;value&gt;.1</c>, <c>.2</c>... as many as the script has.
    /// </summary>
    public static class ValueMoments
    {
        /// <summary>A film has this many scenes at most (clips are about 6 s and the card gives up at 40 s).</summary>
        public const int MaxScenes = 8;

        /// <summary>The value's name as content files spell it: "patience", "sharing"...</summary>
        public static string Wire(ValueId value) => value.ToString().ToLowerInvariant();

        public static ValueMoment Of(ValueId value, Func<string, bool> hasLine)
        {
            string wire = Wire(value);
            var scenes = new List<string>();
            for (int i = 1; i <= MaxScenes; i++)
            {
                string key = "value." + wire + "." + i;
                if (hasLine == null || !hasLine(key))
                {
                    break;
                }

                scenes.Add(key);
            }

            return new ValueMoment(value, "video/value_" + wire + ".mp4", "value." + wire, scenes);
        }
    }
}
