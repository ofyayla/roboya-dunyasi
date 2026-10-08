using System;
using System.Text.RegularExpressions;

namespace Roboya.Core
{
    /// <summary>
    /// The privacy notice shown before the first profile is made (UYM-01), read from content/legal. The text comes with
    /// a version marker; the parent's consent is kept against that version. Markdown marks are turned into plain text.
    /// </summary>
    public sealed class LocalNotice
    {
        public const string File = "legal/aydinlatma-metni.tr.md";
        private static readonly Regex VersionMark = new Regex(@"<!--\s*version:\s*(\S+)\s*-->", RegexOptions.Compiled);
        private static readonly Regex StatusMark = new Regex(@"<!--\s*status:\s*(\S+)\s*-->", RegexOptions.Compiled);

        public LocalNotice(string version, bool isFinal, string text)
        {
            Version = version;
            IsFinal = isFinal;
            Text = text;
        }

        public string Version { get; }

        /// <summary>False while the text is a draft awaiting the lawyer; a build for real users must not ship a draft.</summary>
        public bool IsFinal { get; }

        public string Text { get; }

        public static LocalNotice Parse(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                throw new FormatException("The notice is empty.");
            }

            var version = VersionMark.Match(markdown);
            if (!version.Success)
            {
                throw new FormatException("The notice has no version marker.");
            }

            var status = StatusMark.Match(markdown);
            string body = StatusMark.Replace(VersionMark.Replace(markdown, string.Empty), string.Empty).Trim();
            return new LocalNotice(version.Groups[1].Value, status.Success && status.Groups[1].Value == "final", ToPlainText(body));
        }

        private static string ToPlainText(string markdown)
        {
            string text = Regex.Replace(markdown, @"^#+\s*", string.Empty, RegexOptions.Multiline);
            text = text.Replace("**", string.Empty);
            return Regex.Replace(text, @"\n{3,}", "\n\n").Trim();
        }
    }
}
