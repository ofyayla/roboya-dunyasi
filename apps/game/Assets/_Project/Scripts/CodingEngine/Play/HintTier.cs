namespace Roboya.CodingEngine.Play
{
    /// <summary>YZ-01 hint tiers, escalated one step per request.</summary>
    public enum HintTier
    {
        None = 0,

        /// <summary>Roboya's spoken nudge.</summary>
        Voice = 1,

        /// <summary>The first wrong card is highlighted.</summary>
        HighlightWrongCard = 2,

        /// <summary>The correct card for that slot is shown.</summary>
        ShowCorrectCard = 3,
    }
}
