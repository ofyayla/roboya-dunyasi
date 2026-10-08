using System.Collections.Generic;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.Solving;
using Roboya.CodingEngine.World;

namespace Roboya.CodingEngine.Play
{
    /// <summary>The state of a guided plan: which card Roboya points at next, or that the plan is done.</summary>
    public readonly struct GuideStep
    {
        public GuideStep(CardType? nextCard, bool isComplete, bool needsFix)
        {
            NextCard = nextCard;
            IsComplete = isComplete;
            NeedsFix = needsFix;
        }

        /// <summary>The palette card to point at, or null when nothing is left to place.</summary>
        public CardType? NextCard { get; }

        /// <summary>The plan already reaches the goal: Roboya points at the play button.</summary>
        public bool IsComplete { get; }

        /// <summary>A card on the strip cannot lead to the goal; the guide still points at the card that would.</summary>
        public bool NeedsFix { get; }
    }

    /// <summary>
    /// Guided levels (<c>options.guided</c>, first level of a new concept): Roboya builds the code together with the
    /// child by pointing at the next card. The child still places every card; nothing is placed for them. Built on
    /// <see cref="HintAdvisor"/>, so it follows the same shortest-route reasoning as hints.
    /// </summary>
    public static class GuidedPlan
    {
        public static GuideStep Next(Level level, IReadOnlyList<CardType> program)
        {
            var hint = HintAdvisor.Analyze(level, program);
            if (!hint.HasHint)
            {
                return new GuideStep(null, isComplete: true, needsFix: false);
            }

            // A wrong card sits before the end of the plan: the suggestion is for that slot, not the next one.
            return new GuideStep(hint.SuggestedCard, isComplete: false, needsFix: hint.WrongIndex < program.Count);
        }
    }
}
