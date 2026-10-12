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
    }

    public static class CardTypeExtensions
    {
        /// <summary>Cards that move or turn the robot by exactly one step; the solver searches over these.</summary>
        public static bool IsPrimitiveMove(this CardType card) =>
            card == CardType.Forward || card == CardType.Backward || card == CardType.TurnLeft || card == CardType.TurnRight;
    }
}
