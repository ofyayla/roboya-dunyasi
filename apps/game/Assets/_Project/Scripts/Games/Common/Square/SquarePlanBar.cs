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

        public VisualElement Host => _host;

        public void Bind(SharingLevel level) => _level = level;

        public void Render(SharingPlan plan)
        {
            _host.Clear();
            _cards.Clear();
            _slots.Clear();
            _badges.Clear();
            _lastOn = null;
            for (int i = 0; i < plan.Box.Count; i++)
            {
                Add(plan.Box[i], true, i);
            }

            for (int i = 0; i < plan.Tail.Count; i++)
            {
                Add(plan.Tail[i], false, i);
            }

            for (int i = plan.Count; i < _level.MaxCards; i++)
            {
                var slot = new VisualElement { pickingMode = PickingMode.Ignore };
                slot.AddToClassList("ss");
                _host.Add(slot);
                _slots.Add(slot);
            }
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

        private void Add(SharingCard card, bool inBox, int index)
        {
            var element = Face(card);
            element.RegisterCallback<PointerDownEvent>(_ => CardTapped?.Invoke(inBox, index));
            if (_level.AllowsAmounts && card.Kind == SharingCardKind.Give)
            {
                // The count: as many honey dots as the card hands over. Tap it to change; the rest of the card takes the card out.
                var badge = new VisualElement { name = "count-" + _cards.Count };
                badge.AddToClassList("sc__count");
                HoneyDots.Fill(badge, card.Count, 0, "dot--small");
                badge.RegisterCallback<PointerDownEvent>(e =>
                {
                    e.StopPropagation();
                    CountTapped?.Invoke(inBox, index);
                });
                element.Add(badge);
                _badges.Add(new KeyValuePair<SharingCard, VisualElement>(card, badge));
            }

            _host.Add(element);
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
