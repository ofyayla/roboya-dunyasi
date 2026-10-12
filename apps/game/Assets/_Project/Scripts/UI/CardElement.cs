using Roboya.CodingEngine.Commands;
using UnityEngine.UIElements;

namespace Roboya.UI
{
    /// <summary>A program card tile. Size comes from USS (.card ≥ 64 dp touch target, OYN-06).</summary>
    public sealed class CardElement : VisualElement
    {
        public CardElement(CardType card)
        {
            Card = card;
            AddToClassList("card");
            AddToClassList("card--" + ClassSuffix(card));
            Icon = new Icon(IconFor(card));
            Add(Icon);
        }

        public CardType Card { get; }

        public Icon Icon { get; }

        public static IconKind IconFor(CardType card)
        {
            switch (card)
            {
                case CardType.Forward: return IconKind.Forward;
                case CardType.Backward: return IconKind.Backward;
                case CardType.TurnLeft: return IconKind.TurnLeft;
                case CardType.TurnRight: return IconKind.TurnRight;
                default: return IconKind.None;
            }
        }

        public static string ClassSuffix(CardType card)
        {
            switch (card)
            {
                case CardType.Forward: return "forward";
                case CardType.Backward: return "backward";
                case CardType.TurnLeft: return "turn-left";
                case CardType.TurnRight: return "turn-right";
                default: return "other";
            }
        }
    }
}
