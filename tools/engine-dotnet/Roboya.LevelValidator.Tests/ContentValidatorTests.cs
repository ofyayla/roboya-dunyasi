using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Roboya.LevelValidator.Tests;

public class ContentValidatorTests
{
    private string _root = null!;

    [SetUp]
    public void SetUp()
    {
        _root = Directory.CreateTempSubdirectory("levels-").FullName;
        File.WriteAllText(Path.Combine(_root, "script.csv"), Fixtures.Voice);
    }

    [TearDown]
    public void TearDown() => Directory.Delete(_root, recursive: true);

    private string Levels => Path.Combine(_root, "levels");

    private void Write(JObject level)
    {
        var parts = level["id"]!.ToString().Split('.');
        var dir = Path.Combine(Levels, parts[0], parts[1]);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, parts[2] + ".json"), level.ToString());
    }

    private (int Code, string Output) Run(bool fix = false)
    {
        var output = new StringWriter();
        int code = new ContentValidator(output).Run(Levels, Path.Combine(_root, "script.csv"), fix);
        return (code, output.ToString());
    }

    [Test]
    public void Run_ValidTree_ReturnsZero()
    {
        Write(Fixtures.Level());

        var (code, output) = Run();

        Assert.That(code, Is.EqualTo(0), output);
        Assert.That(output, Does.Contain("1 bölüm, 0 hata."));
    }

    [Test]
    public void Run_MissingDirectory_ReturnsTwo()
    {
        Assert.That(Run().Code, Is.EqualTo(2));
    }

    [Test]
    public void Run_Fix_WritesShortestLength()
    {
        var level = Fixtures.Level();
        level.Remove("solution");
        Write(level);

        var (code, output) = Run(fix: true);

        Assert.That(code, Is.EqualTo(0), output);
        Assert.That(output, Does.Contain("[düzeltildi]"));
        Assert.That(Run().Code, Is.EqualTo(0));
    }

    [Test]
    public void Run_CrossLevelProblems_AreReported()
    {
        var a = Fixtures.Level();
        a["alternativeLevelId"] = "sabir-ormani.yon-avcisi.77";
        Write(a);
        var b = Fixtures.Level();
        b["id"] = "sabir-ormani.yon-avcisi.98";
        b["alternativeLevelId"] = "sabir-ormani.yon-avcisi.98";
        Write(b);
        Directory.CreateDirectory(Path.Combine(Levels, "x"));
        File.WriteAllText(Path.Combine(Levels, "x", "broken.json"), "{");

        var (code, output) = Run();

        Assert.That(code, Is.EqualTo(1));
        Assert.That(output, Does.Contain("share order 99"));
        Assert.That(output, Does.Contain("missing alternativeLevelId"));
        Assert.That(output, Does.Contain("its own alternative"));
    }

    [Test]
    public void Run_DuplicateIds_AreReported()
    {
        Write(Fixtures.Level());
        var copy = Path.Combine(Levels, "kopya");
        Directory.CreateDirectory(copy);
        File.WriteAllText(Path.Combine(copy, "99.json"), Fixtures.Level().ToString());

        Assert.That(Run().Output, Does.Contain("duplicate level id"));
    }

    [Test]
    public void Run_BrokenVoiceScript_CountsErrors()
    {
        Write(Fixtures.Level());
        File.WriteAllText(Path.Combine(_root, "script.csv"), "bad\n");

        Assert.That(Run().Code, Is.EqualTo(1));
    }

    [Test]
    public void Run_RepoContent_IsValid()
    {
        var repoLevels = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../../../content/levels");
        var voice = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../../../content/voice/script.csv");
        var output = new StringWriter();

        Assert.That(new ContentValidator(output).Run(repoLevels, voice, fix: false), Is.EqualTo(0), output.ToString());
    }
}
