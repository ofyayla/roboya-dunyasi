using System;
using System.Collections.Generic;
using System.Threading;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.Play;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Games.Common
{
    /// <summary>
    /// Card palette and plan strip with drag-and-drop. Tap a palette card to append it, tap a planned card to
    /// remove it, or drag: a dropped card snaps to the nearest slot (OYN-06); dragging a card off the strip
    /// removes it. All changes go through <see cref="PlanStrip"/>.
    /// </summary>
    public sealed class CardTray
    {
        private const float DragThreshold = 14f;
        private const float DropMargin = 60f;

        private readonly VisualElement _palette;
        private readonly VisualElement _plan;
        private readonly VisualElement _dragLayer;
        private readonly List<VisualElement> _slots = new List<VisualElement>();

        private PlanStrip _strip;
        private Drag _drag;
        private bool _locked;

        private CardElement _guide;
        private IVisualElementScheduledItem _guideLoop;

        public CardTray(VisualElement palette, VisualElement plan, VisualElement dragLayer)
        {
            _palette = palette;
            _plan = plan;
            _dragLayer = dragLayer;
            _dragLayer.pickingMode = PickingMode.Ignore;
        }

        public event Action Changed;

        /// <summary>
        /// Guided levels: the palette card Roboya points at gently grows and shrinks, with a white ring so the cue is
        /// never colour alone. Pass null to stop pointing.
        /// </summary>
        public void Guide(CardType? card)
        {
            _guide = null;
            foreach (var child in _palette.Children())
            {
                if (child is CardElement element)
                {
                    bool pointed = card.HasValue && element.Card == card.Value;
                    element.EnableInClassList("card--guide", pointed);
                    element.style.scale = new Scale(Vector3.one);
                    if (pointed)
                    {
                        _guide = element;
                    }
                }
            }

            if (_guideLoop == null)
            {
                _guideLoop = _palette.schedule.Execute(() =>
                {
                    if (_guide != null)
                    {
                        float s = 1f + (Mathf.Max(0f, Mathf.Sin(Time.realtimeSinceStartup * 5f)) * 0.14f);
                        _guide.style.scale = new Scale(new Vector3(s, s, 1f));
                    }
                }).Every(33);
            }
        }

        public void Bind(PlanStrip strip, IEnumerable<CardType> palette)
        {
            _strip = strip;
            _palette.Clear();
            foreach (var card in Ordered(palette))
            {
                var element = new CardElement(card);
                element.AddToClassList("card--palette");
                RegisterDrag(element, card, fromSlot: -1);
                _palette.Add(element);
            }

            Refresh();
        }

        /// <summary>Disables editing while the plan runs (YON-02: the plan runs as a whole).</summary>
        public void SetLocked(bool locked)
        {
            _locked = locked;
            _palette.SetEnabled(!locked);
            _plan.EnableInClassList("plan--locked", locked);
            if (locked)
            {
                CancelDrag();
            }
        }

        public void Refresh()
        {
            _plan.Clear();
            _slots.Clear();
            for (int i = 0; i < _strip.Capacity; i++)
            {
                var slot = new VisualElement();
                slot.AddToClassList("slot");
                if (i < _strip.Count)
                {
                    var element = new CardElement(_strip.Cards[i]);
                    RegisterDrag(element, _strip.Cards[i], fromSlot: i);
                    slot.Add(element);
                }

                _slots.Add(slot);
                _plan.Add(slot);
            }
        }

        public void HighlightSlot(int index)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].EnableInClassList("slot--active", i == index);
            }
        }

        public void MarkHint(int index, CardType? suggested)
        {
            ClearHint();
            if (index < 0 || index >= _slots.Count)
            {
                return;
            }

            _slots[index].AddToClassList("slot--hint");
            if (suggested.HasValue)
            {
                var ghost = new CardElement(suggested.Value) { name = "hint-card" };
                ghost.AddToClassList("card--hint");
                ghost.pickingMode = PickingMode.Ignore;
                _slots[index].Add(ghost);
            }
        }

        public void ClearHint()
        {
            foreach (var slot in _slots)
            {
                slot.RemoveFromClassList("slot--hint");
                slot.Q("hint-card")?.RemoveFromHierarchy();
            }
        }

        /// <summary>The card that made Roboya stop trembles (OYN-03).</summary>
        public async Awaitable ShakeSlot(int index, CancellationToken token)
        {
            if (index < 0 || index >= _slots.Count)
            {
                return;
            }

            var slot = _slots[index];
            slot.AddToClassList("slot--error");
            await Tween.Run(0.6f, t => slot.style.translate = new Translate(Tween.Shake(t, 10f), 0f), token);
            slot.style.translate = new Translate(0f, 0f);
        }

        public void ClearMarks()
        {
            foreach (var slot in _slots)
            {
                slot.RemoveFromClassList("slot--active");
                slot.RemoveFromClassList("slot--error");
            }

            ClearHint();
        }

        private void RegisterDrag(VisualElement element, CardType card, int fromSlot)
        {
            element.RegisterCallback<PointerDownEvent>(e =>
            {
                if (_locked || _drag != null)
                {
                    return;
                }

                _drag = new Drag { Source = element, Card = card, FromSlot = fromSlot, PointerId = e.pointerId, Start = e.position };
                element.CapturePointer(e.pointerId);
                e.StopPropagation();
            });
            element.RegisterCallback<PointerMoveEvent>(e =>
            {
                if (_drag == null || _drag.PointerId != e.pointerId)
                {
                    return;
                }

                if (_drag.Ghost == null && Vector2.Distance(e.position, _drag.Start) > DragThreshold)
                {
                    _drag.Ghost = new CardElement(card);
                    _drag.Ghost.AddToClassList("card--dragging");
                    _drag.Ghost.pickingMode = PickingMode.Ignore;
                    _dragLayer.Add(_drag.Ghost);
                    element.AddToClassList("card--lifted");
                }

                if (_drag.Ghost != null)
                {
                    var local = _dragLayer.WorldToLocal(e.position);
                    _drag.Ghost.style.translate = new Translate(local.x - 44f, local.y - 44f);
                    ShowInsertMarker(DropIndex(e.position));
                }
            });
            element.RegisterCallback<PointerUpEvent>(e =>
            {
                if (_drag == null || _drag.PointerId != e.pointerId)
                {
                    return;
                }

                var drag = _drag;
                EndDrag();
                if (drag.Ghost == null)
                {
                    // Tap: palette card appends, planned card is removed.
                    Apply(drag.FromSlot < 0 ? _strip.Append(drag.Card) : _strip.RemoveAt(drag.FromSlot));
                    return;
                }

                int index = DropIndex(e.position);
                if (drag.FromSlot < 0)
                {
                    Apply(index >= 0 && _strip.Insert(index, drag.Card));
                }
                else if (index < 0)
                {
                    Apply(_strip.RemoveAt(drag.FromSlot));
                }
                else
                {
                    Apply(_strip.Move(drag.FromSlot, index > drag.FromSlot ? index - 1 : index));
                }
            });
            element.RegisterCallback<PointerCaptureOutEvent>(_ => CancelDrag());
        }

        /// <summary>Insertion index nearest to the pointer, or -1 when the pointer is away from the strip.</summary>
        private int DropIndex(Vector2 pointer)
        {
            var bounds = _plan.worldBound;
            if (pointer.y < bounds.yMin - DropMargin || pointer.y > bounds.yMax + DropMargin ||
                pointer.x < bounds.xMin - DropMargin || pointer.x > bounds.xMax + DropMargin)
            {
                return -1;
            }

            int filled = _strip.Count;
            int best = filled;
            float bestDistance = float.MaxValue;
            for (int i = 0; i <= filled && i < _slots.Count; i++)
            {
                var r = _slots[i].worldBound;
                float d = Mathf.Abs(pointer.x - r.xMin);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = i;
                }
            }

            return best;
        }

        private void ShowInsertMarker(int index)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].EnableInClassList("slot--insert", i == index);
            }
        }

        private void Apply(bool changed)
        {
            Refresh();
            if (changed)
            {
                Changed?.Invoke();
            }
        }

        private void EndDrag()
        {
            var drag = _drag;
            if (drag == null)
            {
                return;
            }

            // Clear first: releasing capture raises PointerCaptureOutEvent, which re-enters here.
            _drag = null;
            drag.Ghost?.RemoveFromHierarchy();
            drag.Source.RemoveFromClassList("card--lifted");
            if (drag.Source.HasPointerCapture(drag.PointerId))
            {
                drag.Source.ReleasePointer(drag.PointerId);
            }

            ShowInsertMarker(-1);
        }

        private void CancelDrag() => EndDrag();

        private static IEnumerable<CardType> Ordered(IEnumerable<CardType> palette)
        {
            // Stable, child-friendly order regardless of JSON order.
            var order = new[] { CardType.Forward, CardType.TurnLeft, CardType.TurnRight, CardType.Backward };
            foreach (var c in order)
            {
                foreach (var p in palette)
                {
                    if (p == c)
                    {
                        yield return c;
                    }
                }
            }
        }

        private sealed class Drag
        {
            public VisualElement Source;
            public CardType Card;
            public int FromSlot;
            public int PointerId;
            public Vector2 Start;
            public CardElement Ghost;
        }
    }
}
