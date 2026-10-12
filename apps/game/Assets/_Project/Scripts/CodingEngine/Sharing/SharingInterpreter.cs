using System.Collections.Generic;

namespace Roboya.CodingEngine.Sharing
{
    public enum SharingEventKind
    {
        /// <summary>Ari took honey from a flower into the basket.</summary>
        Took,

        /// <summary>One honey went from the basket to a hive or the table.</summary>
        Gave,

        /// <summary>A hive holds more than it asked for: the extra honey runs onto the ground. Nothing is lost for the child.</summary>
        Spilled,

        /// <summary>The plan stopped on this card (see <see cref="SharingStopReason"/>). No penalty: the card is only marked.</summary>
        Stopped,
    }

    public enum SharingStopReason
    {
        None,

        /// <summary>A take card with no room in the basket.</summary>
        BasketFull,

        /// <summary>A take card at a flower with no honey left.</summary>
        FlowerEmpty,

        /// <summary>A give card with an empty basket.</summary>
        BasketEmpty,
    }

    /// <summary>One thing that happened during a run, in order, for the view to animate.</summary>
    public readonly struct SharingEvent
    {
        public SharingEvent(SharingEventKind kind, SharingCard card, int flower, int target, int amount, int basketAfter, int targetCountAfter, SharingStopReason reason)
        {
            Kind = kind;
            Card = card;
            Flower = flower;
            Target = target;
            Amount = amount;
            BasketAfter = basketAfter;
            TargetCountAfter = targetCountAfter;
            Reason = reason;
        }

        public SharingEventKind Kind { get; }

        /// <summary>The program card that caused it, the same object the plan strip holds.</summary>
        public SharingCard Card { get; }

        public int Flower { get; }

        public int Target { get; }

        /// <summary>The honey that moved (1 for a give).</summary>
        public int Amount { get; }

        public int BasketAfter { get; }

        /// <summary>The honey in the hive or on the table that received one, after it did.</summary>
        public int TargetCountAfter { get; }

        public SharingStopReason Reason { get; }
    }

    /// <summary>The outcome of a run.</summary>
    public sealed class SharingResult
    {
        public SharingResult(IReadOnlyList<SharingEvent> events, SharingState final, bool isSuccess, SharingStopReason stopReason, SharingCard stoppedCard, bool spilled)
        {
            Events = events;
            Final = final;
            IsSuccess = isSuccess;
            StopReason = stopReason;
            StoppedCard = stoppedCard;
            Spilled = spilled;
        }

        public IReadOnlyList<SharingEvent> Events { get; }

        public SharingState Final { get; }

        public bool IsSuccess { get; }

        public bool Stopped => StopReason != SharingStopReason.None;

        public SharingStopReason StopReason { get; }

        /// <summary>The card the run stopped on, null when it ran to the end.</summary>
        public SharingCard StoppedCard { get; }

        public bool Spilled { get; }
    }

    /// <summary>Runs a plan on a level. The honey moves in one place, <see cref="Step"/>, which the solver and the hints use too.</summary>
    public static class SharingInterpreter
    {
        public static SharingResult Run(SharingLevel level, SharingProgram program)
        {
            var events = new List<SharingEvent>();
            var state = level.Start();
            var stop = SharingStopReason.None;
            SharingCard stoppedCard = null;
            bool spilled = false;
            foreach (var card in program.Expand())
            {
                state = Step(level, state, card, events, out stop, ref spilled);
                if (stop != SharingStopReason.None)
                {
                    stoppedCard = card;
                    break;
                }
            }

            return new SharingResult(events, state, SharingRules.IsGoalMet(level, state), stop, stoppedCard, spilled);
        }

        /// <summary>
        /// Does one card. A take card takes all the flower holds that fits. A give card hands over its count one honey at a time; if the
        /// basket runs empty half way the run stops on it. <paramref name="events"/> may be null.
        /// </summary>
        public static SharingState Step(SharingLevel level, SharingState state, SharingCard card, List<SharingEvent> events, out SharingStopReason stop, ref bool spilled)
        {
            stop = SharingStopReason.None;
            if (card.Kind == SharingCardKind.Take)
            {
                int room = level.RoomIn(state);
                if (state.FlowerAt(card.Flower) <= 0)
                {
                    stop = SharingStopReason.FlowerEmpty;
                }
                else if (room <= 0)
                {
                    stop = SharingStopReason.BasketFull;
                }

                if (stop != SharingStopReason.None)
                {
                    events?.Add(new SharingEvent(SharingEventKind.Stopped, card, card.Flower, 0, 0, state.Basket, 0, stop));
                    return state;
                }

                int amount = System.Math.Min(room, state.FlowerAt(card.Flower));
                var taken = state.Took(card.Flower, amount);
                events?.Add(new SharingEvent(SharingEventKind.Took, card, card.Flower, 0, amount, taken.Basket, 0, SharingStopReason.None));
                return taken;
            }

            var current = state;
            for (int k = 0; k < card.Count; k++)
            {
                if (current.Basket <= 0)
                {
                    stop = SharingStopReason.BasketEmpty;
                    events?.Add(new SharingEvent(SharingEventKind.Stopped, card, -1, card.Target, 0, current.Basket, current.At(card.Target), stop));
                    return current;
                }

                current = current.Gave(card.Target, 1);
                int held = current.At(card.Target);
                events?.Add(new SharingEvent(SharingEventKind.Gave, card, -1, card.Target, 1, current.Basket, held, SharingStopReason.None));
                if (card.Target != SharingLevel.Table && level.Goal == SharingGoalKind.Deliver && held > level.Hives[card.Target].Need)
                {
                    spilled = true;
                    events?.Add(new SharingEvent(SharingEventKind.Spilled, card, -1, card.Target, 1, current.Basket, held, SharingStopReason.None));
                }
            }

            return current;
        }

        /// <summary>The state after a card that can be done in full, or false when it would stop: used by the solver and the hints.</summary>
        public static bool TryDo(SharingLevel level, SharingState state, SharingCard card, out SharingState next)
        {
            bool spilled = false;
            next = Step(level, state, card, null, out var stop, ref spilled);
            return stop == SharingStopReason.None;
        }
    }
}
