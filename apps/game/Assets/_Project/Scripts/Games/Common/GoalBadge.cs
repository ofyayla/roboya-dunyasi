using System.Collections.Generic;
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

            style.display = _items.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
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
