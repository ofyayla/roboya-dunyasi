using System.Collections.Generic;

namespace Roboya.CodingEngine.Sharing
{
    /// <summary>
    /// The plan strip of a village level: the cards the child placed. A level with a repeat box keeps hive and flower cards in the box
    /// and only table cards after it; a level without one has just a tail. Everything the strip allows is decided here, so the view only draws.
    /// </summary>
    public sealed class SharingPlan
    {
        private readonly SharingLevel _level;
        private readonly List<SharingCard> _box = new List<SharingCard>();
        private readonly List<SharingCard> _tail = new List<SharingCard>();

        public SharingPlan(SharingLevel level)
        {
            _level = level;
        }

        public IReadOnlyList<SharingCard> Box => _box;

        public IReadOnlyList<SharingCard> Tail => _tail;

        /// <summary>How often the repeat box runs, 1 to the level's most.</summary>
        public int Turns { get; private set; } = 1;

        /// <summary>The cards placed; a card in the repeat box counts once.</summary>
        public int Count => _box.Count + _tail.Count;

        public bool IsEmpty => Count == 0;

        public bool IsFull => Count >= _level.MaxCards;

        /// <summary>True when the repeat box has no cards left to place.</summary>
        public bool BoxIsFull => _level.RepeatBox != null && _box.Count >= _level.RepeatBox.Slots;

        /// <summary>Adds a card where it belongs. False when there is no room; nothing changes then.</summary>
        public bool Add(SharingCard card)
        {
            if (card == null || IsFull)
            {
                return false;
            }

            if (_level.RepeatBox == null || card.IsTable)
            {
                _tail.Add(card);
                return true;
            }

            if (BoxIsFull)
            {
                return false;
            }

            _box.Add(card);
            return true;
        }

        public bool RemoveBoxAt(int index) => RemoveAt(_box, index);

        public bool RemoveTailAt(int index) => RemoveAt(_tail, index);

        /// <summary>Changes how much a give card hands over (1 to <see cref="SharingLevel.MaxGive"/>), only on levels whose cards carry a count.</summary>
        public bool SetCount(bool inBox, int index, int count)
        {
            var list = inBox ? _box : _tail;
            if (!_level.AllowsAmounts || index < 0 || index >= list.Count || list[index].Kind != SharingCardKind.Give || count < 1 || count > SharingLevel.MaxGive)
            {
                return false;
            }

            list[index] = list[index].WithCount(count);
            return true;
        }

        /// <summary>Sets the turns of the repeat box (1 to its most). False on a level without a box.</summary>
        public bool SetTurns(int turns)
        {
            if (_level.RepeatBox == null || turns < 1 || turns > _level.RepeatBox.MaxTurns)
            {
                return false;
            }

            Turns = turns;
            return true;
        }

        public void Clear()
        {
            _box.Clear();
            _tail.Clear();
            Turns = 1;
        }

        public SharingProgram ToProgram() => new SharingProgram(Turns, _box.ToArray(), _tail.ToArray());

        private static bool RemoveAt(List<SharingCard> list, int index)
        {
            if (index < 0 || index >= list.Count)
            {
                return false;
            }

            list.RemoveAt(index);
            return true;
        }
    }
}
