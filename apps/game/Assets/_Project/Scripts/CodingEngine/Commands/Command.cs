using System;
using System.Collections.Generic;

namespace Roboya.CodingEngine.Commands
{
    /// <summary>
    /// One card in a program. The command model is a small tree so later games can extend it without forking
    /// the interpreter: Döngü Dansı uses <see cref="RepeatCommand"/> and <see cref="ActionCommand"/>,
    /// Robot Atölyesi uses <see cref="IfCommand"/>, Mucit level uses <see cref="CallCommand"/>.
    /// </summary>
    public abstract class Command
    {
        protected Command(CardType card)
        {
            Card = card;
        }

        public CardType Card { get; }

        /// <summary>Number of cards this command occupies on the plan strip, including nested bodies.</summary>
        public abstract int CardCount { get; }

        protected static int Count(IReadOnlyList<Command> block)
        {
            int n = 0;
            for (int i = 0; i < block.Count; i++)
            {
                n += block[i].CardCount;
            }

            return n;
        }

        protected static IReadOnlyList<Command> Copy(IReadOnlyList<Command> block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            var copy = new Command[block.Count];
            for (int i = 0; i < copy.Length; i++)
            {
                copy[i] = block[i] ?? throw new ArgumentException("Block contains a null command.", nameof(block));
            }

            return copy;
        }
    }

    /// <summary>Forward, backward, turn left, turn right.</summary>
    public sealed class MoveCommand : Command
    {
        public static readonly MoveCommand Forward = new MoveCommand(CardType.Forward);
        public static readonly MoveCommand Backward = new MoveCommand(CardType.Backward);
        public static readonly MoveCommand TurnLeft = new MoveCommand(CardType.TurnLeft);
        public static readonly MoveCommand TurnRight = new MoveCommand(CardType.TurnRight);

        private MoveCommand(CardType card)
            : base(card)
        {
        }

        public override int CardCount => 1;

        public static MoveCommand For(CardType card)
        {
            switch (card)
            {
                case CardType.Forward: return Forward;
                case CardType.Backward: return Backward;
                case CardType.TurnLeft: return TurnLeft;
                case CardType.TurnRight: return TurnRight;
                default: throw new ArgumentException(card + " is not a move card.", nameof(card));
            }
        }
    }

    /// <summary>"Tekrarla": runs <see cref="Body"/> <see cref="Times"/> times.</summary>
    public sealed class RepeatCommand : Command
    {
        public const int MaxTimes = 9;

        public RepeatCommand(int times, IReadOnlyList<Command> body)
            : base(CardType.Repeat)
        {
            if (times < 1 || times > MaxTimes)
            {
                throw new ArgumentOutOfRangeException(nameof(times), "Repeat count must be between 1 and " + MaxTimes + ".");
            }

            Times = times;
            Body = Copy(body);
        }

        public int Times { get; }

        public IReadOnlyList<Command> Body { get; }

        public override int CardCount => 1 + Count(Body);
    }

    /// <summary>"Eğer": runs <see cref="Then"/> when the condition holds, otherwise <see cref="Else"/>.</summary>
    public sealed class IfCommand : Command
    {
        public IfCommand(Condition condition, IReadOnlyList<Command> then, IReadOnlyList<Command> otherwise)
            : base(CardType.If)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Then = Copy(then);
            Else = Copy(otherwise ?? Array.Empty<Command>());
        }

        public Condition Condition { get; }

        public IReadOnlyList<Command> Then { get; }

        public IReadOnlyList<Command> Else { get; }

        public override int CardCount => 1 + Count(Then) + Count(Else);
    }

    /// <summary>Calls a named procedure defined on the <see cref="Program"/> (function card).</summary>
    public sealed class CallCommand : Command
    {
        public CallCommand(string procedure)
            : base(CardType.Call)
        {
            if (string.IsNullOrEmpty(procedure))
            {
                throw new ArgumentException("Procedure name is required.", nameof(procedure));
            }

            Procedure = procedure;
        }

        public string Procedure { get; }

        public override int CardCount => 1;
    }

    /// <summary>A non-moving action such as a dance move or a light; the view interprets <see cref="ActionId"/>.</summary>
    public sealed class ActionCommand : Command
    {
        public ActionCommand(string actionId)
            : base(CardType.Action)
        {
            if (string.IsNullOrEmpty(actionId))
            {
                throw new ArgumentException("Action id is required.", nameof(actionId));
            }

            ActionId = actionId;
        }

        public string ActionId { get; }

        public override int CardCount => 1;
    }
}
