using Roboya.CodingEngine.Commands;

namespace Roboya.CodingEngine.Play
{
    public readonly struct HintResult
    {
        public HintResult(HintTier tier, int slotIndex, CardType? card)
        {
            Tier = tier;
            SlotIndex = slotIndex;
            Card = card;
        }

        public HintTier Tier { get; }

        /// <summary>Slot to highlight (tier 2+), or -1.</summary>
        public int SlotIndex { get; }

        /// <summary>Card to show in that slot (tier 3), or null.</summary>
        public CardType? Card { get; }
    }
}
