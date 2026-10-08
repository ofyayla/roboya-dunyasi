namespace Roboya.CodingEngine.Play
{
    /// <summary>
    /// Tunable thresholds for a level session. Defaults follow the PRD; production values come from server
    /// configuration so pedagogy can adjust them without an app release.
    /// </summary>
    public sealed class SessionRules
    {
        public static readonly SessionRules Default = new SessionRules(3, 5, 2);

        public SessionRules(int hintAfterFailures, int alternativeAfterFailures, int fewAttemptsForStar)
        {
            HintAfterFailures = hintAfterFailures;
            AlternativeAfterFailures = alternativeAfterFailures;
            FewAttemptsForStar = fewAttemptsForStar;
        }

        /// <summary>OYN-05 / YZ-01: offer a hint after this many failed runs in a row.</summary>
        public int HintAfterFailures { get; }

        /// <summary>YZ-03: offer an easier alternative level after this many failed runs.</summary>
        public int AlternativeAfterFailures { get; }

        /// <summary>OYN-04: second star when solved within this many runs.</summary>
        public int FewAttemptsForStar { get; }
    }
}
