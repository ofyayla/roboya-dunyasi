using System;
using System.Collections.Generic;
using Roboya.CodingEngine.Commands;

namespace Roboya.CodingEngine.Play
{
    /// <summary>
    /// The child's editable plan: a fixed number of slots holding cards in order. Shared by every card game;
    /// views render it and translate drag-and-drop into <see cref="Insert"/> / <see cref="Move"/>.
    /// </summary>
    public sealed class PlanStrip
    {
        private readonly List<CardType> _cards;
        private readonly HashSet<CardType> _allowed;

        public PlanStrip(int capacity, IEnumerable<CardType> allowedCards)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            Capacity = capacity;
            _cards = new List<CardType>(capacity);
            _allowed = new HashSet<CardType>(allowedCards ?? throw new ArgumentNullException(nameof(allowedCards)));
        }

        public int Capacity { get; }

        public int Count => _cards.Count;

        public bool IsFull => _cards.Count >= Capacity;

        public bool IsEmpty => _cards.Count == 0;

        public IReadOnlyList<CardType> Cards => _cards;

        /// <summary>Increments on every change so views can skip redundant redraws.</summary>
        public int Version { get; private set; }

        public bool Append(CardType card) => Insert(_cards.Count, card);

        /// <summary>Inserts at <paramref name="index"/> (clamped). Returns false when full or the card is not allowed.</summary>
        public bool Insert(int index, CardType card)
        {
            if (IsFull || !_allowed.Contains(card))
            {
                return false;
            }

            _cards.Insert(Clamp(index, 0, _cards.Count), card);
            Version++;
            return true;
        }

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= _cards.Count)
            {
                return false;
            }

            _cards.RemoveAt(index);
            Version++;
            return true;
        }

        public bool Replace(int index, CardType card)
        {
            if (index < 0 || index >= _cards.Count || !_allowed.Contains(card))
            {
                return false;
            }

            _cards[index] = card;
            Version++;
            return true;
        }

        public bool Move(int from, int to)
        {
            if (from < 0 || from >= _cards.Count)
            {
                return false;
            }

            var card = _cards[from];
            _cards.RemoveAt(from);
            _cards.Insert(Clamp(to, 0, _cards.Count), card);
            Version++;
            return true;
        }

        public void Clear()
        {
            if (_cards.Count == 0)
            {
                return;
            }

            _cards.Clear();
            Version++;
        }

        /// <summary>Replaces the whole plan (starter programs, hints). Disallowed cards and overflow are dropped.</summary>
        public void Load(IEnumerable<CardType> cards)
        {
            _cards.Clear();
            foreach (var c in cards)
            {
                if (!IsFull && _allowed.Contains(c))
                {
                    _cards.Add(c);
                }
            }

            Version++;
        }

        public Program ToProgram()
        {
            var commands = new Command[_cards.Count];
            for (int i = 0; i < commands.Length; i++)
            {
                commands[i] = MoveCommand.For(_cards[i]);
            }

            return new Program(commands);
        }

        private static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);
    }
}
