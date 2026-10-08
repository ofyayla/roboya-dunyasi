namespace Roboya.CodingEngine.Scoring
{
    /// <summary>
    /// OYN-04: 1 star for solving, +1 for solving within few runs, +1 for the shortest code.
    /// Stars are only ever earned; callers keep the best result so a replay never lowers them (ILR-02).
    /// </summary>
    public static class StarRating
    {
        public const int Max = 3;

        public static int Compute(bool solved, int attempts, int cardCount, int shortestLength, int fewAttempts)
        {
            if (!solved)
            {
                return 0;
            }

            int stars = 1;
            if (attempts <= fewAttempts)
            {
                stars++;
            }

            if (shortestLength >= 0 && cardCount <= shortestLength)
            {
                stars++;
            }

            return stars;
        }

        public static int Best(int previous, int current) => current > previous ? current : previous;
    }
}
