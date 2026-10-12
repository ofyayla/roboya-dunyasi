using System;
using System.Collections.Generic;
using System.Threading;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.Sharing;
using Roboya.UI;
using UnityEngine;
using Color = UnityEngine.Color;
using UnityEngine.UIElements;

namespace Roboya.Games.Square
{
    /// <summary>Honey told in dots: groups of five, a solid disc for honey and a hollow ring for room, so the number is counted and not read.</summary>
    internal static class HoneyDots
    {
        public static void Fill(VisualElement host, int filled, int empty, string sizeClass = null)
        {
            host.Clear();
            host.AddToClassList("dots");
            VisualElement group = null;
            int total = filled + empty;
            for (int i = 0; i < total; i++)
            {
                if (i % 5 == 0)
                {
                    group = new VisualElement { pickingMode = PickingMode.Ignore };
                    group.AddToClassList("dots__group");
                    host.Add(group);
                }

                var dot = new VisualElement { pickingMode = PickingMode.Ignore };
                dot.AddToClassList("dot");
                if (sizeClass != null)
                {
                    dot.AddToClassList(sizeClass);
                }

                dot.EnableInClassList("dot--empty", i >= filled);
                group.Add(dot);
            }
        }
    }

    /// <summary>A picture of honey: the region's drop sprite, or a drop drawn in code.</summary>
    internal static class DropPicture
    {
        public static VisualElement Create(RegionArt art, string className)
        {
            var sprite = art != null ? art.ItemFor("honey", null) : null;
            if (sprite != null)
            {
                var image = new VisualElement { pickingMode = PickingMode.Ignore };
                image.AddToClassList(className);
                image.style.backgroundImage = new StyleBackground(sprite);
                return image;
            }

            var shape = new HoneyDropShape();
            shape.AddToClassList(className);
            return shape;
        }
    }

    public enum HivePose
    {
        Child,
        Happy,
        Thanks,
    }

    /// <summary>
    /// A hive family: the villager bee, the straw skep and, under it, honeycomb whose cells are the honey the family has. The bee asks for
    /// honey with a speech bubble and waits (never cries) while the hive is short.
    /// </summary>
    internal sealed class HiveView : VisualElement
    {
        private readonly RegionArt _art;
        private readonly SharingLevel _level;
        private readonly VisualElement _bee;
        private readonly VisualElement _bubble;
        private readonly VisualElement _spill;
        private readonly CombCells _comb;
        private IVisualElementScheduledItem _bob;
        private bool _waiting;

        public HiveView(int index, SharingLevel level, RegionArt art)
        {
            Index = index;
            _level = level;
            _art = art;
            name = "hive-" + index;
            AddToClassList("hv");

            _bee = new VisualElement { pickingMode = PickingMode.Ignore };
            _bee.AddToClassList("hv__bee");
            Add(_bee);
            SetPose(HivePose.Child);

            if (art != null && art.HiveSkep != null)
            {
                var skep = new VisualElement { pickingMode = PickingMode.Ignore };
                skep.AddToClassList("hv__skep");
                skep.style.backgroundImage = new StyleBackground(art.HiveSkep);
                Add(skep);
            }
            else
            {
                var skep = new SkepShape();
                skep.AddToClassList("hv__skep");
                Add(skep);
            }

            _comb = new CombCells { name = "comb-" + index };
            _comb.AddToClassList("hv__comb");
            Add(_comb);

            var mark = new HiveMark(index);
            mark.AddToClassList("hv__mark");
            Add(mark);

            _bubble = new VisualElement { name = "bubble-" + index, pickingMode = PickingMode.Ignore };
            _bubble.AddToClassList("hv__bubble");
            _bubble.Add(level.Goal == SharingGoalKind.Share ? (VisualElement)new EqualMark() : DropPicture.Create(art, "hv__bubble-drop"));
            _bubble.style.display = DisplayStyle.None;
            Add(_bubble);

            _spill = new VisualElement { pickingMode = PickingMode.Ignore };
            _spill.AddToClassList("hv__spill");
            Add(_spill);
            Show(level.Hives[index].Has);
        }

        public int Index { get; }

        public CombCells Comb => _comb;

        /// <summary>Sets the honey the hive holds: cells filled, and honey beyond what it asked for running onto the ground.</summary>
        public void Show(int count)
        {
            var spec = _level.Hives[Index];
            if (_level.Goal == SharingGoalKind.Deliver)
            {
                _comb.Set(spec.Need, Mathf.Min(count, spec.Need));
            }
            else
            {
                _comb.Set(count, count, ghost: count == 0);
            }

            _spill.Clear();
            int extra = _level.Goal == SharingGoalKind.Deliver ? Mathf.Clamp(count - spec.Need, 0, 3) : 0;
            for (int i = 0; i < extra; i++)
            {
                _spill.Add(DropPicture.Create(_art, "hv__spill-drop"));
            }
        }

        public void SetPose(HivePose pose)
        {
            Sprite sprite = null;
            if (_art != null)
            {
                sprite = pose == HivePose.Thanks ? _art.VillagerThanks : pose == HivePose.Happy ? _art.VillagerHappy : _art.VillagerChild;
            }

            _bee.style.backgroundImage = sprite != null ? new StyleBackground(sprite) : new StyleBackground(StyleKeyword.None);
            _bee.EnableInClassList("hv__bee--fallback", sprite == null);
        }

        public void ShowBubble(bool on) => _bubble.style.display = on ? DisplayStyle.Flex : DisplayStyle.None;

        /// <summary>The villager waits for honey: a ring around the family and a gentle bob, not a sad face.</summary>
        public void SetWaiting(bool waiting)
        {
            if (_waiting == waiting)
            {
                return;
            }

            _waiting = waiting;
            EnableInClassList("hv--wait", waiting);
            ShowBubble(waiting);
            if (waiting)
            {
                _bob = _bee.schedule.Execute(() => _bee.style.translate = new Translate(0f, Mathf.Abs(Mathf.Sin(Time.realtimeSinceStartup * 3.2f)) * -12f)).Every(33);
            }
            else
            {
                _bob?.Pause();
                _bee.style.translate = new Translate(0f, 0f);
            }
        }

        /// <summary>Where honey lands: the centre of the next cell to fill, in world space.</summary>
        public Vector2 LandingPoint(int cellIndex) => _comb.LocalToWorld(_comb.CellCenter(cellIndex));
    }

    /// <summary>The "=" in a speech bubble: sharing means every hive holds the same.</summary>
    internal sealed class EqualMark : VisualElement
    {
        public EqualMark()
        {
            pickingMode = PickingMode.Ignore;
            style.width = 36;
            style.height = 36;
            style.justifyContent = Justify.Center;
            for (int i = 0; i < 2; i++)
            {
                var bar = new VisualElement { pickingMode = PickingMode.Ignore };
                bar.style.height = 8;
                bar.style.marginTop = 3;
                bar.style.marginBottom = 3;
                bar.style.borderTopLeftRadius = 4;
                bar.style.borderTopRightRadius = 4;
                bar.style.borderBottomLeftRadius = 4;
                bar.style.borderBottomRightRadius = 4;
                bar.style.backgroundColor = new Color(0.29f, 0.2f, 0.14f, 1f);
                Add(bar);
            }
        }
    }

    /// <summary>A flower and the honey it holds.</summary>
    internal sealed class FlowerView : VisualElement
    {
        private static readonly string[] Colors = { "yellow", "purple", "red" };
        private readonly VisualElement _dots = new VisualElement { pickingMode = PickingMode.Ignore };

        public FlowerView(int index, int amount, RegionArt art)
        {
            Index = index;
            name = "flower-" + index;
            AddToClassList("fl");
            var image = new VisualElement { pickingMode = PickingMode.Ignore };
            image.AddToClassList("fl__img");
            var sprite = art != null ? art.ItemFor("flower", Colors[index % Colors.Length]) : null;
            if (sprite != null)
            {
                image.style.backgroundImage = new StyleBackground(sprite);
            }
            else
            {
                image.Add(new Icon(IconKind.Fruit) { Color = new Color(0.9f, 0.3f, 0.25f), Accent = new Color(0.3f, 0.6f, 0.25f) });
            }

            Add(image);
            Image = image;
            Add(_dots);
            Set(amount);
        }

        public int Index { get; }

        public VisualElement Image { get; }

        public void Set(int amount) => HoneyDots.Fill(_dots, amount, 0);
    }

    /// <summary>The village table: left-over honey is put here for everyone (corner 4).</summary>
    internal sealed class TableView : VisualElement
    {
        private readonly VisualElement _dots = new VisualElement { pickingMode = PickingMode.Ignore };

        public TableView(RegionArt art)
        {
            name = "table";
            AddToClassList("tb");
            var image = new VisualElement { pickingMode = PickingMode.Ignore };
            image.AddToClassList("tb__img");
            var sprite = art != null ? art.PropSprite(StoryProp.VillageTable) : null;
            if (sprite != null)
            {
                image.style.backgroundImage = new StyleBackground(sprite);
            }
            else
            {
                image.AddToClassList("tb__img--fallback");
            }

            Add(image);
            _dots.AddToClassList("tb__dots");
            Add(_dots);
            Set(0);
        }

        public void Set(int count) => HoneyDots.Fill(_dots, count, 0);
    }

    /// <summary>Ari's basket: big, with a mouth you can see into, and the honey in it as dots. It is empty in the drawing; only the dots say how much.</summary>
    internal sealed class BasketView : VisualElement
    {
        private readonly VisualElement _dots = new VisualElement { name = "basket-dots", pickingMode = PickingMode.Ignore };
        private readonly int _capacity;

        public BasketView(SharingLevel level, RegionArt art)
        {
            name = "basket";
            pickingMode = PickingMode.Ignore;
            AddToClassList("basket");
            _capacity = level.BasketCap > 0 ? level.BasketCap : Mathf.Max(1, level.TotalHoney);
            if (art != null && art.BasketEmpty != null)
            {
                style.backgroundImage = new StyleBackground(art.BasketEmpty);
                AddToClassList("basket--sprite");
            }
            else
            {
                var shape = new BasketShape();
                shape.style.position = Position.Absolute;
                shape.style.left = 0;
                shape.style.top = 0;
                shape.style.right = 0;
                shape.style.bottom = 0;
                Add(shape);
            }

            _dots.AddToClassList("basket__dots");
            Add(_dots);
            Set(level.BasketStart);
        }

        public int Count { get; private set; }

        public void Set(int count)
        {
            Count = count;
            HoneyDots.Fill(_dots, count, Mathf.Max(0, _capacity - count), "dot--small");
        }
    }

    /// <summary>Ari: the bee the child programs, carrying her basket.</summary>
    internal sealed class AriView : VisualElement
    {
        private readonly VisualElement _body;
        private readonly Sprite _front;
        private readonly Sprite _happy;

        public AriView(SharingLevel level, RegionArt art)
        {
            name = "ari";
            pickingMode = PickingMode.Ignore;
            AddToClassList("ari");
            var actor = art != null ? art.ActorFor(GameId.BalPesinde) : default;
            _front = actor.Front;
            _happy = actor.Happy;
            _body = new VisualElement { pickingMode = PickingMode.Ignore };
            _body.AddToClassList("ari__body");
            if (_front != null)
            {
                _body.style.backgroundImage = new StyleBackground(_front);
            }
            else
            {
                _body.Add(new Icon(IconKind.Bee) { Color = new Color(0.97f, 0.72f, 0.1f), Accent = new Color(0.25f, 0.18f, 0.1f) });
            }

            Add(_body);
            Basket = new BasketView(level, art);
            Add(Basket);
        }

        public BasketView Basket { get; }

        public void SetHappy(bool happy)
        {
            var sprite = happy && _happy != null ? _happy : _front;
            if (sprite != null)
            {
                _body.style.backgroundImage = new StyleBackground(sprite);
            }
        }
    }

    /// <summary>The scene of a village level and everything that moves in it. It draws what the engine says; it decides nothing.</summary>
    internal sealed class SquareView : VisualElement
    {
        private const float AriSize = 190f;

        private readonly RegionArt _art;
        private readonly VisualElement _hives = new VisualElement { name = "hives", pickingMode = PickingMode.Ignore };
        private readonly VisualElement _flowers = new VisualElement { name = "flowers", pickingMode = PickingMode.Ignore };
        private readonly VisualElement _fx = new VisualElement { name = "fx", pickingMode = PickingMode.Ignore };
        private readonly List<HiveView> _hiveViews = new List<HiveView>();
        private readonly List<FlowerView> _flowerViews = new List<FlowerView>();
        private SharingLevel _level;
        private Vector2 _ariTop;
        private bool _ariPlaced;
        private bool _ariAtHome;

        public SquareView(RegionArt art)
        {
            _art = art;
            name = "square";
            AddToClassList("sq");
            _hives.AddToClassList("sq__hives");
            _flowers.AddToClassList("sq__flowers");
            _fx.AddToClassList("sq__fx");
            Add(_hives);
            Add(_flowers);
            RegisterCallback<GeometryChangedEvent>(_ => OnGeometry());
        }

        public event Action<int> HiveTapped;

        public event Action<int> FlowerTapped;

        public event Action TableTapped;

        public AriView Ari { get; private set; }

        public TableView Table { get; private set; }

        public IReadOnlyList<HiveView> Hives => _hiveViews;

        public IReadOnlyList<FlowerView> Flowers => _flowerViews;

        /// <summary>Builds the scene of a level: the hives (and the table), the flowers and Ari at home.</summary>
        public void Build(SharingLevel level)
        {
            _level = level;
            _hives.Clear();
            _flowers.Clear();
            Ari?.RemoveFromHierarchy();
            _fx.RemoveFromHierarchy();
            _hiveViews.Clear();
            _flowerViews.Clear();
            Table = null;

            for (int i = 0; i < level.Hives.Count; i++)
            {
                if (i > 0 && level.Goal == SharingGoalKind.Share)
                {
                    _hives.Add(EqualSign());
                }

                var hive = new HiveView(i, level, _art);
                int index = i;
                hive.RegisterCallback<PointerDownEvent>(_ => HiveTapped?.Invoke(index));
                _hives.Add(hive);
                _hiveViews.Add(hive);
            }

            if (level.HasTable)
            {
                Table = new TableView(_art);
                Table.RegisterCallback<PointerDownEvent>(_ => TableTapped?.Invoke());
                _hives.Add(Table);
            }

            for (int f = 0; f < level.Flowers.Count; f++)
            {
                var flower = new FlowerView(f, level.Flowers[f], _art);
                int index = f;
                flower.RegisterCallback<PointerDownEvent>(_ => FlowerTapped?.Invoke(index));
                _flowers.Add(flower);
                _flowerViews.Add(flower);
            }

            Ari = new AriView(level, _art);
            Add(Ari);
            Add(_fx);
            ShowState(level.Start());
            _ariPlaced = false;
            OnGeometry();
        }

        /// <summary>Draws a state: every count, wherever the honey is.</summary>
        public void ShowState(SharingState state)
        {
            for (int i = 0; i < _hiveViews.Count; i++)
            {
                _hiveViews[i].Show(state.HiveAt(i));
            }

            for (int f = 0; f < _flowerViews.Count; f++)
            {
                _flowerViews[f].Set(state.FlowerAt(f));
            }

            Table?.Set(state.Table);
            Ari.Basket.Set(state.Basket);
        }

        public void SetInputEnabled(bool enabled)
        {
            var mode = enabled ? PickingMode.Position : PickingMode.Ignore;
            foreach (var hive in _hiveViews)
            {
                hive.pickingMode = mode;
            }

            foreach (var flower in _flowerViews)
            {
                flower.pickingMode = mode;
            }

            if (Table != null)
            {
                Table.pickingMode = mode;
            }
        }

        private static VisualElement EqualSign()
        {
            var sign = new VisualElement { pickingMode = PickingMode.Ignore };
            sign.AddToClassList("sq__equal");
            sign.Add(new VisualElement { pickingMode = PickingMode.Ignore });
            sign.Add(new VisualElement { pickingMode = PickingMode.Ignore });
            return sign;
        }

        // ---- Positions (in this view's own space) ----

        public Vector2 Home() => new Vector2((layout.width - AriSize) * 0.5f, Mathf.Max(0f, layout.height - AriSize - 8f));

        public Vector2 LocalCenter(VisualElement element) => this.WorldToLocal(element.worldBound.center);

        /// <summary>Where Ari's top-left goes to stand below a hive, next to a flower or below the table.</summary>
        public Vector2 SpotBelowHive(int hive) => SpotBelow(_hiveViews[hive]);

        public Vector2 SpotBelowTable() => SpotBelow(Table);

        public Vector2 SpotBesideFlower(int flower)
        {
            var c = LocalCenter(_flowerViews[flower].Image);
            return new Vector2(c.x + 70f, c.y - (AriSize * 0.5f));
        }

        private Vector2 SpotBelow(VisualElement target)
        {
            var c = LocalCenter(target);
            float bottom = this.WorldToLocal(new Vector2(0f, target.worldBound.yMax)).y;
            return new Vector2(c.x - (AriSize * 0.5f), Mathf.Min(layout.height - AriSize, bottom - 30f));
        }

        /// <summary>Puts Ari at home the first time the scene has a size, and keeps her where she is when it changes.</summary>
        private void OnGeometry()
        {
            if (Ari == null || layout.height <= 0f || float.IsNaN(layout.height))
            {
                return;
            }

            // At home she follows the scene's size (the first layout passes are not the final one); mid-flight she stays put.
            if (!_ariPlaced || _ariAtHome)
            {
                _ariTop = Home();
                _ariPlaced = true;
                _ariAtHome = true;
            }

            Ari.style.translate = new Translate(_ariTop.x, _ariTop.y);
        }

        // ---- Movement ----

        /// <summary>Ari flies to a spot (top-left of her box) in a small arc.</summary>
        public async Awaitable MoveAri(Vector2 to, CancellationToken token, float seconds = 0.55f)
        {
            var from = _ariTop;
            _ariAtHome = false;
            float arc = Mathf.Clamp(Vector2.Distance(from, to) * 0.18f, 18f, 70f);
            await Tween.Run(
                seconds,
                t =>
                {
                    var pos = Vector2.Lerp(from, to, t);
                    pos.y -= Mathf.Sin(t * Mathf.PI) * arc;
                    Ari.style.translate = new Translate(pos.x, pos.y);
                },
                token);
            _ariTop = to;
            Ari.style.translate = new Translate(to.x, to.y);
        }

        public async Awaitable GoHome(CancellationToken token)
        {
            await MoveAri(Home(), token);
            _ariAtHome = true;
        }

        /// <summary>Ari back at home at once, for a fresh run.</summary>
        public void SnapAriHome()
        {
            _ariTop = Home();
            _ariPlaced = true;
            _ariAtHome = true;
            Ari.style.translate = new Translate(_ariTop.x, _ariTop.y);
        }

        /// <summary>One honey flies in an arc between two world points, a drop of honey.</summary>
        public async Awaitable FlyDrop(Vector2 fromWorld, Vector2 toWorld, CancellationToken token, float seconds = 0.42f)
        {
            var drop = DropPicture.Create(_art, "fx__drop");
            _fx.Add(drop);
            var from = this.WorldToLocal(fromWorld) - new Vector2(21f, 24f);
            var to = this.WorldToLocal(toWorld) - new Vector2(21f, 24f);
            float arc = Mathf.Clamp(Vector2.Distance(from, to) * 0.25f, 30f, 110f);
            try
            {
                await Tween.Run(
                    seconds,
                    t =>
                    {
                        var pos = Vector2.Lerp(from, to, t);
                        pos.y -= Mathf.Sin(t * Mathf.PI) * arc;
                        drop.style.translate = new Translate(pos.x, pos.y);
                    },
                    token);
            }
            finally
            {
                drop.RemoveFromHierarchy();
            }
        }

        public Vector2 BasketPoint() => Ari.Basket.LocalToWorld(new Vector2(Ari.Basket.layout.width * 0.5f, Ari.Basket.layout.height * 0.4f));

        public Vector2 FlowerPoint(int flower) => _flowerViews[flower].Image.LocalToWorld(new Vector2(_flowerViews[flower].Image.layout.width * 0.5f, _flowerViews[flower].Image.layout.height * 0.4f));

        public Vector2 TablePoint() => Table.worldBound.center;

        public async Awaitable Shake(VisualElement element, CancellationToken token)
        {
            await Tween.Run(0.5f, t => element.style.translate = new Translate(Tween.Shake(t, 12f), 0f), token);
            element.style.translate = new Translate(0f, 0f);
        }

        /// <summary>A short pop of an element: the one that just received honey.</summary>
        public async Awaitable Pop(VisualElement element, CancellationToken token)
        {
            await Tween.Run(0.28f, t => element.style.scale = new Scale(Vector3.one * (1f + (0.18f * Mathf.Sin(t * Mathf.PI)))), token);
            element.style.scale = new Scale(Vector3.one);
        }

        /// <summary>Ari's shake of the head: the basket jumps (nothing to give, no room).</summary>
        public Awaitable ShakeBasket(CancellationToken token) => Shake(Ari.Basket, token);

        /// <summary>The goal told without words: every bee asks with a honey bubble and the cells breathe for a few seconds.</summary>
        public async Awaitable ShowGoal(CancellationToken token)
        {
            foreach (var hive in _hiveViews)
            {
                hive.ShowBubble(true);
            }

            try
            {
                await Tween.Run(
                    3.6f,
                    t =>
                    {
                        float s = 1f + (0.07f * Mathf.Sin(t * Mathf.PI * 7f));
                        foreach (var hive in _hiveViews)
                        {
                            hive.Comb.style.scale = new Scale(new Vector3(s, s, 1f));
                        }
                    },
                    token);
            }
            finally
            {
                foreach (var hive in _hiveViews)
                {
                    hive.Comb.style.scale = new Scale(Vector3.one);
                    hive.ShowBubble(false);
                }
            }
        }

        public async Awaitable Celebrate(CancellationToken token)
        {
            Ari.SetHappy(true);
            foreach (var hive in _hiveViews)
            {
                hive.SetWaiting(false);
                hive.SetPose(HivePose.Thanks);
            }

            for (int i = 0; i < 2; i++)
            {
                await Tween.Run(0.3f, t => Ari.style.translate = new Translate(_ariTop.x, _ariTop.y - (Mathf.Sin(t * Mathf.PI) * 26f)), token);
            }

            Ari.style.translate = new Translate(_ariTop.x, _ariTop.y);
        }

        public void ResetPoses()
        {
            Ari?.SetHappy(false);
            foreach (var hive in _hiveViews)
            {
                hive.SetPose(HivePose.Child);
                hive.SetWaiting(false);
            }
        }
    }

    /// <summary>The guiding hand: a pulsing ring on what to tap and a finger that taps it.</summary>
    internal sealed class GuideHand : VisualElement
    {
        private readonly VisualElement _ring = new VisualElement { pickingMode = PickingMode.Ignore };
        private readonly VisualElement _finger = new HandShape();
        private IVisualElementScheduledItem _loop;

        public GuideHand()
        {
            name = "guide-hand";
            pickingMode = PickingMode.Ignore;
            AddToClassList("hand");
            _ring.AddToClassList("hand__ring");
            _finger.AddToClassList("hand__finger");
            Add(_ring);
            Add(_finger);
            style.display = DisplayStyle.None;
        }

        public bool IsShown => style.display == DisplayStyle.Flex;

        /// <summary>Shows the hand over a point given in this element's parent space.</summary>
        public void PointAt(Vector2 parentPoint)
        {
            style.translate = new Translate(parentPoint.x, parentPoint.y);
            style.display = DisplayStyle.Flex;
            _loop?.Pause();
            _loop = schedule.Execute(() =>
            {
                float t = Mathf.Repeat(Time.realtimeSinceStartup, 1.2f) / 1.2f;
                float ring = 0.5f + (t * 0.9f);
                _ring.style.scale = new Scale(new Vector3(ring, ring, 1f));
                _ring.style.opacity = 1f - t;
                _finger.style.translate = new Translate(0f, Mathf.Abs(Mathf.Sin(t * Mathf.PI)) * 12f);
            }).Every(33);
        }

        public void Hide()
        {
            _loop?.Pause();
            style.display = DisplayStyle.None;
        }
    }

    /// <summary>A pointing hand drawn in code: a white finger and palm with the warm outline.</summary>
    internal sealed class HandShape : VisualElement
    {
        public HandShape()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        private void Draw(MeshGenerationContext ctx)
        {
            if (!ShapePaint.Frame(this, out var o, out float s))
            {
                return;
            }

            var p = ctx.painter2D;
            Vector2 P(float x, float y) => o + new Vector2(x * s, y * s);
            p.lineJoin = LineJoin.Round;
            p.fillColor = Color.white;
            p.strokeColor = VillagePalette.Outline;
            p.lineWidth = s * 0.05f;
            ShapePaint.RoundedRect(p, P(0.22f, 0.5f), P(0.84f, 0.96f), s * 0.14f);
            p.Fill();
            p.Stroke();
            ShapePaint.RoundedRect(p, P(0.34f, 0.04f), P(0.58f, 0.7f), s * 0.12f);
            p.Fill();
            p.Stroke();
            ShapePaint.RoundedRect(p, P(0.58f, 0.44f), P(0.72f, 0.68f), s * 0.07f);
            p.Fill();
            p.Stroke();
        }
    }
}
