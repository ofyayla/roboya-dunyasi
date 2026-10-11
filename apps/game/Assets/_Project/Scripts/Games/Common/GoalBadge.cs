using System.Collections.Generic;
using Roboya.CodingEngine.Play;
using Roboya.CodingEngine.World;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Games.Common
{
    /// <summary>
    /// What to collect, shown as pictures at the top of the board (no text): one small picture per item the goal needs, bright
    /// once collected. The spoken instruction names the items; this keeps them in sight so a 4 year old need not remember.
    /// </summary>
    public sealed class GoalBadge : VisualElement
    {
        private readonly RegionArt _art;
        private readonly List<(int index, VisualElement view)> _items = new List<(int, VisualElement)>();
        private readonly List<(int drop, int count, VisualElement view)> _deliveries = new List<(int, int, VisualElement)>();

        public GoalBadge(RegionArt art)
        {
            _art = art;
            name = "goal-badge";
            pickingMode = PickingMode.Ignore;
            AddToClassList("goal-badge");
            style.display = DisplayStyle.None;
        }

        public int Count => _items.Count;

        /// <summary>How many of the wanted items are collected so far.</summary>
        public int CollectedCount
        {
            get
            {
                int n = 0;
                foreach (var (_, view) in _items)
                {
                    if (view.ClassListContains("goal-badge__item--done"))
                    {
                        n++;
                    }
                }

                return n;
            }
        }

        public bool IsCollected(int itemIndex)
        {
            foreach (var (index, view) in _items)
            {
                if (index == itemIndex)
                {
                    return view.ClassListContains("goal-badge__item--done");
                }
            }

            return false;
        }

        /// <summary>Builds the badge for a level; hidden when the goal needs nothing collected.</summary>
        public void Show(Level level)
        {
            Clear();
            _items.Clear();
            _deliveries.Clear();
            ulong mask = level.Goal.MustCollectMask;
            for (int i = 0; i < level.Items.Count; i++)
            {
                if ((mask & (1UL << i)) == 0)
                {
                    continue;
                }

                var item = level.Items[i];
                var view = new VisualElement { name = "goal-item-" + item.Id, pickingMode = PickingMode.Ignore };
                view.AddToClassList("goal-badge__item");
                var sprite = _art != null ? _art.ItemFor(item.Kind, item.Color) : null;
                if (sprite != null)
                {
                    view.style.backgroundImage = new StyleBackground(sprite);
                }
                else
                {
                    view.Add(new Icon(IconKind.Fruit) { Color = new Color(0.9f, 0.3f, 0.25f), Accent = new Color(0.3f, 0.6f, 0.25f) });
                }

                Add(view);
                _items.Add((i, view));
            }

            bool honey = AddHoneyGoal(level);
            style.display = _items.Count > 0 || honey ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Honey goals as pictures (plan D3, no text): a delivery shows its hive with one slot per honey wanted, an equal share shows the
        /// sharing hives side by side with an "equal" sign between them, and a basket count shows the basket with the dots to keep.
        /// </summary>
        private bool AddHoneyGoal(Level level)
        {
            var goal = level.Goal;
            bool any = false;
            foreach (var d in goal.Deliver)
            {
                var view = HiveCard("goal-hive-" + level.Drops[d.DropIndex].Id);
                view.Add(SlotsRow(d.Count, 0));
                Add(view);
                _deliveries.Add((d.DropIndex, d.Count, view));
                any = true;
            }

            if (goal.Share != null)
            {
                for (int i = 0; i < goal.Share.Among.Count; i++)
                {
                    if (i > 0)
                    {
                        var equal = new VisualElement { name = "goal-equal", pickingMode = PickingMode.Ignore };
                        equal.AddToClassList("goal-badge__equal");
                        equal.Add(new VisualElement { pickingMode = PickingMode.Ignore });
                        equal.Add(new VisualElement { pickingMode = PickingMode.Ignore });
                        Add(equal);
                    }

                    Add(HiveCard("goal-share-" + level.Drops[goal.Share.Among[i]].Id));
                }

                any = true;
            }

            if (goal.CarryAtEnd.HasValue)
            {
                var basket = new VisualElement { name = "goal-basket", pickingMode = PickingMode.Ignore };
                basket.AddToClassList("goal-badge__item");
                var sprite = _art != null ? _art.HoneyBasket : null;
                if (sprite != null)
                {
                    basket.style.backgroundImage = new StyleBackground(sprite);
                }

                basket.Add(SlotsRow(goal.CarryAtEnd.Value, goal.CarryAtEnd.Value));
                Add(basket);
                any = true;
            }

            return any;
        }

        private VisualElement HiveCard(string viewName)
        {
            var view = new VisualElement { name = viewName, pickingMode = PickingMode.Ignore };
            view.AddToClassList("goal-badge__item");
            view.AddToClassList("goal-badge__hive");
            var sprite = _art != null ? _art.HiveFor(HiveState.Empty) : null;
            if (sprite != null)
            {
                view.style.backgroundImage = new StyleBackground(sprite);
            }
            else
            {
                view.AddToClassList("goal-badge__hive--fallback");
            }

            return view;
        }

        private static VisualElement SlotsRow(int capacity, int filled)
        {
            var row = new VisualElement { pickingMode = PickingMode.Ignore };
            row.AddToClassList("goal-badge__slots");
            for (int i = 0; i < capacity; i++)
            {
                var slot = new VisualElement { pickingMode = PickingMode.Ignore };
                slot.AddToClassList("hive__slot");
                slot.AddToClassList("goal-badge__slot");
                slot.EnableInClassList("hive__slot--filled", i < filled);
                row.Add(slot);
            }

            return row;
        }

        /// <summary>Fills each delivery hive's slots with what it holds in <paramref name="state"/>; a hive with exactly its share glows like a collected item.</summary>
        public void SetHoney(RobotState state)
        {
            foreach (var (drop, count, view) in _deliveries)
            {
                int held = state.DeliveredAt(drop);
                view.EnableInClassList("goal-badge__item--done", held == count);
                int i = 0;
                foreach (var slot in view.Q(className: "goal-badge__slots").Children())
                {
                    slot.EnableInClassList("hive__slot--filled", i < held);
                    i++;
                }
            }
        }

        /// <summary>Marks collected items; <paramref name="collectedMask"/> is the robot's collected set (0 clears all).</summary>
        public void Set(ulong collectedMask)
        {
            foreach (var (index, view) in _items)
            {
                view.EnableInClassList("goal-badge__item--done", (collectedMask & (1UL << index)) != 0);
            }
        }

        public void MarkCollected(int itemIndex)
        {
            foreach (var (index, view) in _items)
            {
                if (index == itemIndex)
                {
                    view.AddToClassList("goal-badge__item--done");
                }
            }
        }
    }
}
