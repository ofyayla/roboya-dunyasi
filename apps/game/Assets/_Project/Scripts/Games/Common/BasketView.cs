using System.Collections.Generic;
using Roboya.CodingEngine.World;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Games.Common
{
    /// <summary>
    /// Ari's honey basket (Paylaşım Köyü, ADR 0030): the basket picture and one dot per honey it can hold in this level, solid for
    /// honey carried and a hollow ring for room left, so the number is seen and counted without a digit or colour doing the work.
    /// Hidden on levels without drop points or a starting basket.
    /// </summary>
    public sealed class BasketView : VisualElement
    {
        private readonly RegionArt _art;
        private readonly VisualElement _icon = new VisualElement { name = "basket-icon", pickingMode = PickingMode.Ignore };
        private readonly VisualElement _dots = new VisualElement { name = "basket-dots", pickingMode = PickingMode.Ignore };
        private readonly List<VisualElement> _slots = new List<VisualElement>();

        public BasketView(RegionArt art)
        {
            _art = art;
            name = "basket-view";
            pickingMode = PickingMode.Ignore;
            AddToClassList("basket-view");
            _icon.AddToClassList("basket-view__icon");
            _dots.AddToClassList("basket-view__dots");
            Add(_icon);
            Add(_dots);
            style.display = DisplayStyle.None;
        }

        /// <summary>Honey in the basket now.</summary>
        public int Count { get; private set; }

        /// <summary>One dot per honey the level has (the start basket plus every honey item), at least one.</summary>
        public int Capacity => _slots.Count;

        public void Show(Level level)
        {
            _dots.Clear();
            _slots.Clear();
            Count = 0;
            bool used = level.Drops.Count > 0 || level.Start.Basket > 0;
            style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
            if (!used)
            {
                return;
            }

            int capacity = level.Start.Basket;
            foreach (var item in level.Items)
            {
                capacity += item.Kind == "honey" ? item.Amount : 0;
            }

            var sprite = _art != null ? _art.HoneyBasket : null;
            if (sprite != null)
            {
                _icon.style.backgroundImage = new StyleBackground(sprite);
                _icon.RemoveFromClassList("basket-view__icon--fallback");
            }
            else
            {
                _icon.style.backgroundImage = StyleKeyword.Null;
                _icon.AddToClassList("basket-view__icon--fallback");
            }

            for (int i = 0; i < Mathf.Max(1, capacity); i++)
            {
                var dot = new VisualElement { pickingMode = PickingMode.Ignore };
                dot.AddToClassList("basket-view__dot");
                _dots.Add(dot);
                _slots.Add(dot);
            }

            Set(level.Start.Basket);
        }

        /// <summary>A short pulse that shows the basket: "Bırak" with nothing to drop points the child at what Ari is carrying.</summary>
        public async Awaitable Nudge(System.Threading.CancellationToken token)
        {
            await Tween.Run(0.5f, t => style.scale = new Scale(Vector3.one * (1f + (0.25f * Mathf.Sin(t * Mathf.PI)))), token);
            style.scale = new Scale(Vector3.one);
        }

        /// <summary>Shows <paramref name="count"/> dots solid and the rest hollow.</summary>
        public void Set(int count)
        {
            Count = count;
            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].EnableInClassList("basket-view__dot--filled", i < count);
            }
        }
    }
}
