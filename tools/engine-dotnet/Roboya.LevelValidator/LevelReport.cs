namespace Roboya.LevelValidator;

internal sealed class LevelReport(string path)
{
    public string Path { get; } = path;

    public string? Id { get; set; }

    public int? ShortestLength { get; set; }

    public List<string> Errors { get; } = [];

    public List<string> Warnings { get; } = [];

    public bool Fixed { get; set; }

    public bool IsValid => Errors.Count == 0;
}
