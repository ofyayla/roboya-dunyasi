namespace Roboya.LevelValidator;

/// <summary>Reads content/voice/script.csv (key,text,context,level_ids) — RFC 4180 quoting.</summary>
internal sealed class VoiceScript
{
    public static readonly string[] Header = ["key", "text", "context", "level_ids"];

    private VoiceScript(Dictionary<string, string> texts, List<string> errors)
    {
        Texts = texts;
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string> Texts { get; }

    public IReadOnlyList<string> Errors { get; }

    public static VoiceScript Parse(string csv)
    {
        var texts = new Dictionary<string, string>(StringComparer.Ordinal);
        var errors = new List<string>();
        var rows = ReadRows(csv);
        if (rows.Count == 0 || !rows[0].SequenceEqual(Header))
        {
            errors.Add("script.csv header must be: " + string.Join(",", Header));
            return new VoiceScript(texts, errors);
        }

        for (int i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            int line = i + 1;
            if (row.Count == 1 && row[0].Length == 0)
            {
                continue;
            }

            if (row.Count != Header.Length)
            {
                errors.Add($"script.csv line {line}: expected {Header.Length} columns, got {row.Count}");
                continue;
            }

            if (string.IsNullOrWhiteSpace(row[0]) || string.IsNullOrWhiteSpace(row[1]))
            {
                errors.Add($"script.csv line {line}: key and text are required");
                continue;
            }

            if (!texts.TryAdd(row[0], row[1]))
            {
                errors.Add($"script.csv line {line}: duplicate key '{row[0]}'");
            }
        }

        return new VoiceScript(texts, errors);
    }

    internal static List<List<string>> ReadRows(string csv)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new System.Text.StringBuilder();
        bool quoted = false;
        for (int i = 0; i < csv.Length; i++)
        {
            char c = csv[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < csv.Length && csv[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    quoted = false;
                }
                else
                {
                    field.Append(c);
                }

                continue;
            }

            switch (c)
            {
                case '"':
                    quoted = true;
                    break;
                case ',':
                    row.Add(field.ToString());
                    field.Clear();
                    break;
                case '\r':
                    break;
                case '\n':
                    row.Add(field.ToString());
                    field.Clear();
                    rows.Add(row);
                    row = [];
                    break;
                default:
                    field.Append(c);
                    break;
            }
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }

        return rows;
    }
}
