using System.Collections.Generic;

namespace Roboya.CodingEngine.Play
{
    public enum RemainingKind
    {
        /// <summary>"Başta vardı": what Ari had in all.</summary>
        Before,

        /// <summary>"Verdiğim": what went to the hives and the table.</summary>
        Given,

        /// <summary>"Kalan": what is still in the basket; the answer to the question.</summary>
        Left,
    }

    /// <summary>One answer card of "Kaç kaldı?": what it stands for and how many dots it shows.</summary>
    public readonly struct RemainingChoice
    {
        public RemainingChoice(RemainingKind kind, int count)
        {
            Kind = kind;
            Count = count;
        }

        public RemainingKind Kind { get; }

        public int Count { get; }

        public bool IsCorrect => Kind == RemainingKind.Left;
    }

    /// <summary>
    /// The three cards of "Kaç kaldı?" (PK-0002) and when to ask. The cards come from <see cref="RemainingQuestion"/>; their order is
    /// a fixed permutation of the level id, so the right card is not always in the same place and a test can still predict it.
    /// </summary>
    public static class RemainingChoices
    {
        private static readonly int[][] Orders =
        {
            new[] { 0, 1, 2 }, new[] { 0, 2, 1 }, new[] { 1, 0, 2 }, new[] { 1, 2, 0 }, new[] { 2, 0, 1 }, new[] { 2, 1, 0 },
        };

        public static IReadOnlyList<RemainingChoice> For(string levelId, RemainingQuestion question)
        {
            var all = new[]
            {
                new RemainingChoice(RemainingKind.Before, question.Before),
                new RemainingChoice(RemainingKind.Given, question.Given),
                new RemainingChoice(RemainingKind.Left, question.Left),
            };
            int sum = 0;
            foreach (char c in levelId ?? string.Empty)
            {
                sum += c;
            }

            var order = Orders[sum % Orders.Length];
            return new[] { all[order[0]], all[order[1]], all[order[2]] };
        }

        /// <summary>
        /// Only a Kaşif-and-up child, only when the level asks for it, only when the step is switched on (it stays off until the child
        /// test), and only when the three answers differ, or two cards would be the same and the question a guess.
        /// </summary>
        public static bool ShouldAsk(bool levelOption, bool enabled, bool minik, RemainingQuestion question) =>
            levelOption && enabled && !minik && question.HasDistinctOptions;
    }
}
