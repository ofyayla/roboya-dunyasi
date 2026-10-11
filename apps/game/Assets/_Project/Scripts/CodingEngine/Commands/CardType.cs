namespace Roboya.CodingEngine.Commands
{
    /// <summary>Cards a level can offer on its palette. Persisted in level JSON as snake_case strings.</summary>
    public enum CardType
    {
        Forward,
        Backward,
        TurnLeft,
        TurnRight,
        Repeat,
        If,
        Call,
        Action,

        /// <summary>"Bırak" (Paylaşım Köyü, ADR 0030): leaves one honey where the bee stands. Last in the list so older values keep their numbers.</summary>
        Drop,
    }

    public static class CardTypeExtensions
    {
        /// <summary>Cards that move or turn the robot by exactly one step; the solver searches over these.</summary>
        public static bool IsPrimitiveMove(this CardType card) =>
            card == CardType.Forward || card == CardType.Backward || card == CardType.TurnLeft || card == CardType.TurnRight;

        /// <summary>Cards the solver searches over: the primitive moves and "Bırak", which changes the honey but not the position.</summary>
        public static bool IsSolverStep(this CardType card) => card.IsPrimitiveMove() || card == CardType.Drop;
    }
}
