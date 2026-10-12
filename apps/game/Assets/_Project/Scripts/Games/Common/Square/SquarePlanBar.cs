using System;
using System.Collections.Generic;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Sharing;
using Roboya.UI;
using UnityEngine;
using Color = UnityEngine.Color;
using UnityEngine.UIElements;

namespace Roboya.Games.Square
{
    /// <summary>
    /// The plan strip of the village: one card per step. A give card shows its hive's mark and a drop, a take card the flower, the table card the
    /// table; no text. Tap a card to take it out. Everything the strip allows is decided by <see cref="SharingPlan"/>; this only draws.
    /// </summary>
    internal sealed class SquarePlanBar
    {
        private readonly VisualElement _host;
        private readonly RegionArt _art;
        private readonly List<KeyValuePair<SharingCard, VisualElement>> _cards = new List<KeyValuePair<SharingCard, VisualElement>>();
        private readonly List<VisualElement> _slots = new List<VisualElement>();
        private readonly List<KeyValuePair<SharingCard, VisualElement>> _badges = new List<KeyValuePair<SharingCard, VisualElement>>();
        private SharingLevel _level;
        private VisualElement _lastOn;

        public SquarePlanBar(VisualElement host, RegionArt art)
        {
            _host = host;
            _art = art;
        }

        /// <summary>The child tapped a card: (it is in the repeat box, its position there).</summary>
        public event Action<bool, int> CardTapped;

        /// <summary>The child tapped the count of a give card (levels whose cards carry a count): (in the repeat box, its position).</summary>
        public event Action<bool, int> CountTapped;

        /// <summary>The child tapped the − or + of the repeat box: -1 or +1 turn.</summary>
        public event Action<int> TurnsTapped;

        private VisualElement _plus;
        private VisualElement _minus;

        /// <summary>The + or − button of the repeat box, for the guiding hand; null on levels without a box.</summary>
        public VisualElement TurnsButton(bool plus) => plus ? _plus : _minus;

        public VisualElement Host => _host;

        public void Bind(SharingLevel level) => _level = level;

        public void Render(SharingPlan plan)
        {
            _host.Clear();
            _cards.Clear();
            _slots.Clear();
            _badges.Clear();
            _lastOn = null;
            _plus = null;
            _minus = null;
            var box = _level.RepeatBox;
            if (box != null)
            {
                // Corner 4: the fixed repeat box at the head of the strip, its cards inside, then what follows it (the table).
                var loop = BuildLoop(plan);
                _host.Add(loop);
                for (int i = 0; i < plan.Box.Count; i++)
                {
                    Add(plan.Box[i], true, i, loop);
                }

                for (int i = plan.Box.Count; i < box.Slots; i++)
                {
                    loop.Add(Slot());
                }

                for (int i = 0; i < plan.Tail.Count; i++)
                {
                    Add(plan.Tail[i], false, i, _host);
                }

                if (_level.HasTable && plan.Tail.Count == 0)
                {
                    _host.Add(Slot());
                }

                return;
            }

            for (int i = 0; i < plan.Tail.Count; i++)
            {
                Add(plan.Tail[i], false, i, _host);
            }

            for (int i = plan.Count; i < _level.MaxCards; i++)
            {
                _host.Add(Slot());
            }
        }

        private VisualElement Slot()
        {
            var slot = new VisualElement { pickingMode = PickingMode.Ignore };
            slot.AddToClassList("ss");
            _slots.Add(slot);
            return slot;
        }

        /// <summary>The repeat box: a purple frame with a turn arrow, − and +, the turns as dots (solid for the turns, hollow for the room) and the cards inside.</summary>
        private VisualElement BuildLoop(SharingPlan plan)
        {
            var loop = new VisualElement { name = "repeat-box" };
            loop.AddToClassList("lp");
            var head = new VisualElement { pickingMode = PickingMode.Ignore };
            head.AddToClassList("lp__head");
            head.Add(new Icon(IconKind.Retry) { Color = Color.white, name = "repeat-icon" });
            _minus = LoopButton(false);
            var turns = new VisualElement { name = "repeat-turns", pickingMode = PickingMode.Ignore };
            HoneyDots.Fill(turns, plan.Turns, _level.RepeatBox.MaxTurns - plan.Turns, "dot--small");
            _plus = LoopButton(true);
            head.Add(_minus);
            head.Add(turns);
            head.Add(_plus);
            loop.Add(head);
            return loop;
        }

        private VisualElement LoopButton(bool plus)
        {
            var button = new VisualElement { name = plus ? "repeat-plus" : "repeat-minus" };
            button.AddToClassList("lp__btn");
            var bar = new VisualElement { pickingMode = PickingMode.Ignore };
            bar.AddToClassList("lp__bar");
            button.Add(bar);
            if (plus)
            {
                var up = new VisualElement { pickingMode = PickingMode.Ignore };
                up.AddToClassList("lp__bar");
                up.AddToClassList("lp__bar--up");
                button.Add(up);
            }

            button.RegisterCallback<PointerDownEvent>(e =>
            {
                e.StopPropagation();
                TurnsTapped?.Invoke(plus ? 1 : -1);
            });
            return button;
        }

        /// <summary>The element of a card of the plan, matched by identity (two equal cards are two cards); null if it is not on the strip.</summary>
        public VisualElement ElementOf(SharingCard card)
        {
            foreach (var pair in _cards)
            {
                if (ReferenceEquals(pair.Key, card))
                {
                    return pair.Value;
                }
            }

            return null;
        }

        /// <summary>The count badge of a give card of the plan (matched by identity); null on levels without counts.</summary>
        public VisualElement BadgeOf(SharingCard card)
        {
            foreach (var pair in _badges)
            {
                if (ReferenceEquals(pair.Key, card))
                {
                    return pair.Value;
                }
            }

            return null;
        }

        public VisualElement NextSlot() => _slots.Count > 0 ? _slots[0] : null;

        /// <summary>The card a run is on right now glows; the previous one stops glowing.</summary>
        public void Glow(SharingCard card)
        {
            _lastOn?.RemoveFromClassList("sc--on");
            _lastOn = ElementOf(card);
            _lastOn?.AddToClassList("sc--on");
        }

        /// <summary>The card the run stopped on gets a honey-coloured frame, the same on every device (no red).</summary>
        public void MarkStopped(SharingCard card) => ElementOf(card)?.AddToClassList("sc--stop");

        public void ClearMarks()
        {
            _lastOn = null;
            foreach (var pair in _cards)
            {
                pair.Value.RemoveFromClassList("sc--on");
                pair.Value.RemoveFromClassList("sc--stop");
            }
        }

        public void SetLocked(bool locked)
        {
            foreach (var pair in _cards)
            {
                pair.Value.pickingMode = locked ? PickingMode.Ignore : PickingMode.Position;
            }

            foreach (var pair in _badges)
            {
                pair.Value.pickingMode = locked ? PickingMode.Ignore : PickingMode.Position;
            }
        }

        private void Add(SharingCard card, bool inBox, int index, VisualElement container)
        {
            var element = Face(card);
            element.RegisterCallback<PointerDownEvent>(_ => CardTapped?.Invoke(inBox, index));
            if (_level.AllowsAmounts && card.Kind == SharingCardKind.Give)
            {
                // The count: solid dots for what the card hands over and hollow rings for the room up to three, so the range can be seen. Tap it to change; the rest of the card takes the card out.
                var badge = new VisualElement { name = "count-" + _cards.Count };
                badge.AddToClassList("sc__count");
                HoneyDots.Fill(badge, card.Count, SharingLevel.MaxGive - card.Count, "dot--small");
                badge.RegisterCallback<PointerDownEvent>(e =>
                {
                    e.StopPropagation();
                    CountTapped?.Invoke(inBox, index);
                });
                element.Add(badge);
                _badges.Add(new KeyValuePair<SharingCard, VisualElement>(card, badge));
            }

            container.Add(element);
            _cards.Add(new KeyValuePair<SharingCard, VisualElement>(card, element));
        }

        private VisualElement Face(SharingCard card)
        {
            var element = new VisualElement { name = "card-" + card };
            element.AddToClassList("sc");
            if (card.Kind == SharingCardKind.Take)
            {
                element.AddToClassList("sc--take");
                var image = new VisualElement { pickingMode = PickingMode.Ignore };
                image.AddToClassList("sc__img");
                var sprite = _art != null ? _art.ItemFor("flower", FlowerColor(card.Flower)) : null;
                if (sprite != null)
                {
                    image.style.backgroundImage = new StyleBackground(sprite);
                }
                else
                {
                    image.Add(new Icon(IconKind.Fruit) { Color = new Color(0.9f, 0.3f, 0.25f), Accent = new Color(0.3f, 0.6f, 0.25f) });
                }

                element.Add(image);
                return element;
            }

            if (card.IsTable)
            {
                element.AddToClassList("sc--table");
                var image = new VisualElement { pickingMode = PickingMode.Ignore };
                image.AddToClassList("sc__img");
                var sprite = _art != null ? _art.PropSprite(StoryProp.VillageTable) : null;
                if (sprite != null)
                {
                    image.style.backgroundImage = new StyleBackground(sprite);
                }
                else
                {
                    image.Add(new Icon(IconKind.Box) { Color = VillagePalette.WickerDark, Accent = VillagePalette.Straw });
                }

                element.Add(image);
                return element;
            }

            element.AddToClassList("sc--give");
            var mark = new HiveMark(card.Target);
            mark.AddToClassList("sc__mark");
            element.Add(mark);
            if (!_level.AllowsAmounts)
            {
                element.Add(DropPicture.Create(_art, "sc__tag"));
            }

            return element;
        }

        private static string FlowerColor(int flower) => flower == 1 ? "purple" : flower == 2 ? "red" : "yellow";
    }
}
