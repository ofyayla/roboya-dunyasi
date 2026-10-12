using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Roboya.LevelValidator.Tests;

public class SolveServerTests
{
    [Test]
    public void Handle_SolvableLevel_ReturnsSolution()
    {
        var level = Fixtures.Level();
        level.Remove("solution");

        var response = JObject.Parse(SolveServer.Handle(level.ToString(), Fixtures.VoiceScript()));

        Assert.That((bool)response["valid"]!, Is.True);
        Assert.That((int)response["shortestLength"]!, Is.EqualTo(5));
        Assert.That(response["solution"]!.Select(t => t.ToString()), Is.EqualTo(new[] { "forward", "forward", "turn_right", "forward", "forward" }));
        Assert.That(response["errors"]!, Is.Empty);
    }

    [Test]
    public void Handle_UnsolvableLevel_ReturnsErrors()
    {
        var level = Fixtures.Level();
        level["cards"]!["maxProgramLength"] = 2;

        var response = JObject.Parse(SolveServer.Handle(level.ToString(), null));

        Assert.That((bool)response["valid"]!, Is.False);
        Assert.That(response["solution"], Is.Null);
        Assert.That(response["errors"]!.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task RunAsync_PostSolve_RespondsOverHttp()
    {
        using var cts = new CancellationTokenSource();
        int port = 5300 + Random.Shared.Next(500);
        var server = SolveServer.RunAsync(port, null, cts.Token);
        using var http = new HttpClient();

        string body = "";
        for (int attempt = 0; attempt < 20 && body.Length == 0; attempt++)
        {
            try
            {
                var res = await http.PostAsync($"http://127.0.0.1:{port}/solve", new StringContent(Fixtures.Level().ToString()));
                body = await res.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException)
            {
                await Task.Delay(50);
            }
        }

        var notFound = await http.GetAsync($"http://127.0.0.1:{port}/");
        cts.Cancel();
        await server;

        Assert.That((int)JObject.Parse(body)["shortestLength"]!, Is.EqualTo(5));
        Assert.That((int)notFound.StatusCode, Is.EqualTo(404));
    }
}
