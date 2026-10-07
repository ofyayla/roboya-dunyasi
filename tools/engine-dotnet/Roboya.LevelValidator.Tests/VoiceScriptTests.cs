using NUnit.Framework;

namespace Roboya.LevelValidator.Tests;

public class VoiceScriptTests
{
    [Test]
    public void Parse_QuotedFields_UnescapesQuotes()
    {
        var script = VoiceScript.Parse(Fixtures.Voice);

        Assert.That(script.Errors, Is.Empty);
        Assert.That(script.Texts["yon_avcisi.l99.intro"], Is.EqualTo("Merhaba, \"Roboya\" burada!"));
    }

    [Test]
    public void Parse_WrongHeader_ReportsError()
    {
        Assert.That(VoiceScript.Parse("anahtar,metin\n").Errors, Has.Count.EqualTo(1));
    }

    [Test]
    public void Parse_DuplicateKeyMissingTextWrongColumns_ReportsEach()
    {
        var csv = "key,text,context,level_ids\r\n" +
                  "a.b,x,,\r\n" +
                  "a.b,y,,\r\n" +
                  "c.d,,,\r\n" +
                  "e.f,only-two\r\n" +
                  "\r\n" +
                  "g.h,\"multi\nline\",,";

        var script = VoiceScript.Parse(csv);

        Assert.That(script.Errors, Has.Count.EqualTo(3));
        Assert.That(script.Texts["g.h"], Is.EqualTo("multi\nline"));
    }

    [Test]
    public void Parse_RepoScript_HasNoErrors()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../../../content/voice/script.csv");

        Assert.That(VoiceScript.Parse(File.ReadAllText(path)).Errors, Is.Empty);
    }
}
