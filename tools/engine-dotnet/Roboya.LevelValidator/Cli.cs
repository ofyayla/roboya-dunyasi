namespace Roboya.LevelValidator;

internal static class Cli
{
    private const string Usage =
        "Usage:\n" +
        "  Roboya.LevelValidator <levels-dir> [--voice script.csv] [--manifest manifest.json] [--fix]\n" +
        "  Roboya.LevelValidator serve [--port 5199] [--voice script.csv]";

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine(Usage);
            return 2;
        }

        string? voicePath = Option(args, "--voice");
        if (voicePath != null && !File.Exists(voicePath))
        {
            Console.Error.WriteLine($"error: voice script '{voicePath}' not found");
            return 2;
        }

        if (args[0] == "serve")
        {
            int port = int.TryParse(Option(args, "--port"), out var p) ? p : 5199;
            var voice = voicePath != null ? VoiceScript.Parse(File.ReadAllText(voicePath)) : null;
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };
            await SolveServer.RunAsync(port, voice, cts.Token);
            return 0;
        }

        return new ContentValidator(Console.Out).Run(args[0], voicePath, args.Contains("--fix"), Option(args, "--manifest"));
    }

    private static string? Option(string[] args, string name)
    {
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }
}
