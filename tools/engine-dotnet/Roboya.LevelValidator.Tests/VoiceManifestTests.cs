using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Roboya.LevelValidator.Tests;

public class VoiceManifestTests
{
    private string _dir = null!;

    [SetUp]
    public void SetUp() => _dir = Directory.CreateTempSubdirectory("voice-").FullName;

    [TearDown]
    public void TearDown() => Directory.Delete(_dir, recursive: true);

    private string Manifest(JObject entries)
    {
        var path = Path.Combine(_dir, "manifest.json");
        File.WriteAllText(path, new JObject { ["version"] = 1, ["entries"] = entries }.ToString());
        return path;
    }

    private JObject Entry(string key, string text, string source = "tts", bool writeFile = true)
    {
        var rel = $"audio/tr/{key}.mp3";
        if (writeFile)
        {
            Directory.CreateDirectory(Path.Combine(_dir, "audio/tr"));
            File.WriteAllBytes(Path.Combine(_dir, rel), [1]);
        }

        return new JObject { ["file"] = rel, ["source"] = source, ["textHash"] = VoiceManifest.TextHash(text) };
    }

    private static VoiceScript Script(params (string Key, string Text)[] lines) =>
        VoiceScript.Parse("key,text,context,level_ids\n" + string.Concat(lines.Select(l => $"{l.Key},{l.Text},,\n")));

    [Test]
    public void Check_UpToDate_NoErrors()
    {
        var path = Manifest(new JObject { ["a.b"] = Entry("a.b", "Merhaba") });

        Assert.That(VoiceManifest.Check(Script(("a.b", "Merhaba")), path), Is.Empty);
    }

    [Test]
    public void Check_MissingManifest_ReportsOne()
    {
        Assert.That(VoiceManifest.Check(Script(("a.b", "x")), Path.Combine(_dir, "yok.json")), Has.Count.EqualTo(1));
    }

    [Test]
    public void Check_MissingStaleOrphanEntries_AreReported()
    {
        var path = Manifest(new JObject
        {
            ["a.b"] = Entry("a.b", "eski"),
            ["c.d"] = Entry("c.d", "x", writeFile: false),
            ["s.t"] = Entry("s.t", "eski", source: "studio"),
            ["o.p"] = Entry("o.p", "z"),
        });

        var errors = VoiceManifest.Check(Script(("a.b", "yeni"), ("c.d", "x"), ("e.f", "y"), ("s.t", "yeni")), path);

        Assert.That(errors, Has.Some.Contains("'a.b' audio is stale"));
        Assert.That(errors, Has.Some.Contains("'c.d' audio file"));
        Assert.That(errors, Has.Some.Contains("'e.f' has no audio"));
        Assert.That(errors, Has.Some.Contains("studio recording is stale"));
        Assert.That(errors, Has.Some.Contains("'o.p' which is not in script.csv"));
    }

    [Test]
    public void TextHash_MatchesPythonGenerator()
    {
        // python3 -c "import hashlib;print(hashlib.sha256('Merhaba, Roboya!'.encode()).hexdigest()[:16])"
        Assert.That(VoiceManifest.TextHash("Merhaba, Roboya!"), Is.EqualTo("c7b33a84114d60da"));
    }
}
