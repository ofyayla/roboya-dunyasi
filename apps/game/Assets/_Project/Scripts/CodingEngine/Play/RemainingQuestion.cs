using Roboya.CodingEngine.World;

namespace Roboya.CodingEngine.Play
{
    /// <summary>
    /// The numbers behind "Kaç kaldı?" (PK-0002, <c>options.askRemaining</c>): how much honey the bee had, how much it gave and
    /// how much is left. The three answers on the cards come from here, so the level validator and the view cannot disagree.
    /// </summary>
    public readonly struct RemainingQuestion
    {
        private RemainingQuestion(int before, int given, int left)
        {
            Before = before;
            Given = given;
            Left = left;
        }

        /// <summary>"Başta vardı": the basket at the start plus every honey picked up on the way.</summary>
        public int Before { get; }

        /// <summary>"Verdiğim": honey that reached a drop point.</summary>
        public int Given { get; }

        /// <summary>"Kalan": honey still in the basket.</summary>
        public int Left { get; }

        /// <summary>
        /// The three answers must differ, or two cards would be the same and the question would be a guess. Since
        /// <c>Before = Given + Left</c>, that means something was given, something is left, and the two are not equal.
        /// </summary>
        public bool HasDistinctOptions => Before != Given && Before != Left && Given != Left;

        public static RemainingQuestion From(Level level, RobotState finalState)
        {
            int before = level.Start.Basket;
            for (int i = 0; i < level.Items.Count; i++)
            {
                if (finalState.HasCollected(i) && level.Items[i].Kind == "honey")
                {
                    before += level.Items[i].Amount;
                }
            }

            int given = 0;
            for (int i = 0; i < level.Drops.Count; i++)
            {
                given += finalState.DeliveredAt(i);
            }

            return new RemainingQuestion(before, given, finalState.Basket);
        }
    }
}
