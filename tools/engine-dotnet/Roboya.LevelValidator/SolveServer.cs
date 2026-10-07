using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Roboya.CodingEngine.Levels;
using Roboya.CodingEngine.Solving;

namespace Roboya.LevelValidator;

/// <summary>
/// Local-only HTTP endpoint for the level editor (ADR 0002): POST /solve with a level JSON body returns
/// solvability, shortest length, one solution and validation errors. Binds to 127.0.0.1 only.
/// </summary>
internal static class SolveServer
{
    public static string Handle(string body, VoiceScript? voice)
    {
        var report = LevelChecker.CheckJson(body, "editor.json", voice);
        var result = new JObject
        {
            ["valid"] = report.Errors.All(e => e.Contains("file must live", StringComparison.Ordinal) || e.Contains("shortestLength", StringComparison.Ordinal)),
            ["shortestLength"] = report.ShortestLength,
            ["errors"] = new JArray(report.Errors.Where(e => !e.Contains("file must live", StringComparison.Ordinal) && !e.Contains("shortestLength", StringComparison.Ordinal))),
            ["warnings"] = new JArray(report.Warnings),
        };

        if (report.ShortestLength.HasValue)
        {
            var solution = Solver.Solve(LevelLoader.Load(body));
            result["solution"] = new JArray(solution.Solution.Select(c => LevelChecker.Kebab(c.ToString()).Replace('-', '_')));
        }

        return result.ToString(Formatting.None);
    }

    public static async Task RunAsync(int port, VoiceScript? voice, CancellationToken token)
    {
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();
        Console.WriteLine($"LevelValidator serve: http://127.0.0.1:{port}/solve");
        using var registration = token.Register(listener.Stop);
        while (!token.IsCancellationRequested)
        {
            HttpListenerContext ctx;
            try
            {
                ctx = await listener.GetContextAsync();
            }
            catch (Exception e) when (token.IsCancellationRequested && e is HttpListenerException or ObjectDisposedException)
            {
                break;
            }

            string response;
            int status = 200;
            if (ctx.Request.HttpMethod == "POST" && ctx.Request.Url?.AbsolutePath == "/solve")
            {
                using var reader = new StreamReader(ctx.Request.InputStream, Encoding.UTF8);
                response = Handle(await reader.ReadToEndAsync(token), voice);
            }
            else
            {
                status = 404;
                response = "{\"error\":\"not found\"}";
            }

            var bytes = Encoding.UTF8.GetBytes(response);
            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/json; charset=utf-8";
            await ctx.Response.OutputStream.WriteAsync(bytes, token);
            ctx.Response.Close();
        }
    }
}
