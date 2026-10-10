using System;
using System.Collections.Generic;
using RegionId = Roboya.CodingEngine.Levels.Generated.RegionId;
using Roboya.CodingEngine.Progress;
using Roboya.Core;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Map
{
    /// <summary>
    /// Patience Forest path (F1-05): one stepping stone per level along a winding dirt trail. Every stone has the same
    /// shape and carries its number; what changes is its state. Walked stones are cream with their stars above them and
    /// the trail behind Roboya turns golden. The next stone is golden, larger and gently pulsing with Roboya on it. Stones
    /// still ahead are small and grey; past the free tier they carry a padlock that asks the child to fetch a grown-up
    /// (GLR-01). Ship parts wait on the trail, faint until earned (ILR-03), and a tree stump with the wise turtle closes
    /// each forest corner. A big play button starts the next level. Tapping never sells anything.
    /// </summary>
    public sealed class PathView : VisualElement, IRefreshable
    {
        private const float StoneWidth = 126f;
        private const float StoneHeight = 84f;
        private const float MinSpacing = 200f;
        private const float PartSize = 64f;
        private const float CornerWidth = 132f;
        private const float CornerHeight = 90f;

        private readonly GameServices _services;
        private readonly RegionArt _art;
        private readonly Action _onWorkshop;
        private readonly Action _onRest;
        private readonly List<string> _path;
        private readonly ScrollView _scroll = new ScrollView(ScrollViewMode.Horizontal) { name = "path-scroll" };
        private readonly PathTrack _track = new PathTrack();
        private readonly List<VisualElement> _stones = new List<VisualElement>();
        private readonly List<(VisualElement Element, int Order, int AfterStone)> _parts = new List<(VisualElement, int, int)>();
        private readonly List<VisualElement> _corners = new List<VisualElement>();
        private readonly VisualElement _robot = new VisualElement { name = "path-robot" };
        private readonly IconButton _play;
        private NodeState[] _states = new NodeState[0];
        private int _current = -1;
        private float _time;
        private bool _needsCentering = true;
        // Scroll target waiting for the widened trail to be laid out; negative when nothing is pending.
        private float _centerTarget = -1f;
        private int _centerTries;
        // The view size Roboya was last centred for; a new size (the screen settling at start-up) centres again.
        private Vector2 _centeredFor;

        public PathView(GameServices services, RegionArt art, PartArt partArt, Action onIsland, Action onWorkshop, Action onRest)
        {
            _services = services;
            _art = art;
            _onWorkshop = onWorkshop;
            _onRest = onRest;
            name = "path";
            AddToClassList("map-view");
            AddToClassList("path");
            if (art != null && art.Background != null)
            {
                style.backgroundImage = new StyleBackground(art.Background);
            }

            _path = ProgressQueries.PathOf(services.Catalog, RegionId.SabirOrmani);
            _scroll.AddToClassList("path__scroll");
            _scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _scroll.contentContainer.AddToClassList("path__content");
            _scroll.Add(_track);
            _scroll.contentContainer.RegisterCallback<GeometryChangedEvent>(_ => ApplyCentering());
            Add(_scroll);

            // Each forest corner (F1-24 value moment) is a stump beside the trail where the wise turtle waits.
            for (int c = 1; c <= CornerCount(_path.Count); c++)
            {
                var corner = new VisualElement { name = "corner-" + c, pickingMode = PickingMode.Ignore };
                corner.AddToClassList("path__corner");
                corner.Add(Face(StoneLook.Corner, art != null ? art.StoneCorner : null));
                var turtle = new Icon(IconKind.Turtle) { Color = new Color(0.36f, 0.62f, 0.3f), Accent = Color.white };
                turtle.AddToClassList("path__turtle");
                corner.Add(turtle);
                _corners.Add(corner);
                _scroll.Add(corner);
            }

            BuildParts(partArt);
            for (int i = 0; i < _path.Count; i++)
            {
                var stone = BuildStone(i);
                _stones.Add(stone);
                _scroll.Add(stone);
            }

            _robot.pickingMode = PickingMode.Ignore;
            _robot.AddToClassList("path__robot");
            if (art != null && art.RobotFront != null)
            {
                _robot.style.backgroundImage = new StyleBackground(art.RobotFront);
            }

            _scroll.Add(_robot);

            var bar = new VisualElement();
            bar.AddToClassList("map-bar");
            bar.Add(new IconButton(IconKind.Island, onIsland) { name = "to-island" });
            bar.Add(new IconButton(IconKind.Ship, onWorkshop) { name = "to-workshop" });
            Add(bar);

            // One obvious action for the youngest: play the next level without finding its stone first.
            _play = new IconButton(IconKind.Play, () => OnStone(_current)) { name = "path-play" };
            _play.AddToClassList("path__play");
            Add(_play);

            RegisterCallback<GeometryChangedEvent>(_ => Layout());
            schedule.Execute(Pulse).Every(33);
        }

        private static int CornerCount(int count) => count / ProgressQueries.CornerLength;

        public IReadOnlyList<string> Path => _path;

        public void Refresh()
        {
            _states = ProgressQueries.PathStates(_services, _path);
            _current = Array.IndexOf(_states, NodeState.Current);
            var book = _services.Progress.Book;
            for (int i = 0; i < _stones.Count; i++)
            {
                var stone = _stones[i];
                var state = _states[i];
                foreach (var c in new[] { "stone--done", "stone--current", "stone--locked", "stone--grownup", "stone--open" })
                {
                    stone.RemoveFromClassList(c);
                }

                stone.AddToClassList(state == NodeState.Completed ? "stone--done"
                    : state == NodeState.Current ? "stone--current"
                    : state == NodeState.Locked ? "stone--locked"
                    : state == NodeState.Open ? "stone--open"
                    : "stone--grownup");

                // Open stones (an older child's earlier levels, all-open builds) can be played but are not walked yet.
                var look = state == NodeState.Completed ? StoneLook.Done : state == NodeState.Current ? StoneLook.Current : StoneLook.Ahead;
                SetLook(stone.Q("stone-face"), look);
                stone.Q("stone-lock").style.display = state == NodeState.NeedsGrownUp ? DisplayStyle.Flex : DisplayStyle.None;

                var stars = stone.Q("stone-stars");
                int earned = book.Stars(_path[i]);
                for (int s = 0; s < 3; s++)
                {
                    SetStar(stars[s], s < earned);
                }

                stars.style.display = state == NodeState.Completed ? DisplayStyle.Flex : DisplayStyle.None;
            }

            int partsEarned = ProgressQueries.EarnedParts(_services);
            foreach (var (element, order, _) in _parts)
            {
                element.EnableInClassList("path__part--earned", order <= partsEarned);
            }

            _play.style.display = _current >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _needsCentering = true;
            Layout();
        }

        private VisualElement BuildStone(int index)
        {
            var stone = new VisualElement { name = "stone-" + (index + 1) };
            stone.AddToClassList("stone");
            stone.Add(Face(StoneLook.Ahead, null));
            var number = new Label((index + 1).ToString()) { name = "stone-number", pickingMode = PickingMode.Ignore };
            number.AddToClassList("stone__number");
            stone.Add(number);

            var padlock = new Icon(IconKind.Lock) { name = "stone-lock", Color = new Color(0.45f, 0.36f, 0.3f), Accent = Color.white };
            padlock.AddToClassList("stone__lock");
            stone.Add(padlock);

            var stars = new VisualElement { name = "stone-stars", pickingMode = PickingMode.Ignore };
            stars.AddToClassList("stone__stars");
            for (int s = 0; s < 3; s++)
            {
                // The region's star sprites when present, else the code-drawn star.
                var star = _art != null && _art.StarFull != null
                    ? new VisualElement { pickingMode = PickingMode.Ignore }
                    : new Icon(IconKind.StarEmpty) { Color = new Color(1f, 0.78f, 0.15f), Accent = new Color(0.93f, 0.9f, 0.85f) };
                star.name = "stone-star-" + (s + 1);
                star.AddToClassList("stone__star");
                star.EnableInClassList("stone__star--middle", s == 1);
                stars.Add(star);
            }

            stone.Add(stars);
            stone.RegisterCallback<ClickEvent>(_ => OnStone(index));
            return stone;
        }

        private void SetStar(VisualElement star, bool earned)
        {
            star.EnableInClassList("stone__star--earned", earned);
            if (star is Icon icon)
            {
                icon.Kind = earned ? IconKind.Star : IconKind.StarEmpty;
            }
            else
            {
                star.style.backgroundImage = new StyleBackground(earned ? _art.StarFull : _art.StarEmpty);
            }
        }

        /// <summary>The stone picture: the region's sprite for that look when there is one, else the code-drawn stone.</summary>
        private static VisualElement Face(StoneLook look, Sprite sprite)
        {
            var face = new VisualElement { name = "stone-face", pickingMode = PickingMode.Ignore };
            face.AddToClassList("stone__face");
            var shape = new StoneShape(look) { name = "stone-shape" };
            shape.AddToClassList("stone__shape");
            face.Add(shape);
            if (sprite != null)
            {
                face.style.backgroundImage = new StyleBackground(sprite);
                shape.style.display = DisplayStyle.None;
            }

            return face;
        }

        private void SetLook(VisualElement face, StoneLook look)
        {
            var shape = face.Q<StoneShape>("stone-shape");
            shape.Look = look;
            face.EnableInClassList("stone__face--done", look == StoneLook.Done);
            face.EnableInClassList("stone__face--current", look == StoneLook.Current);
            face.EnableInClassList("stone__face--ahead", look == StoneLook.Ahead);
            var sprite = _art == null ? null
                : look == StoneLook.Done ? _art.StoneDone
                : look == StoneLook.Current ? _art.StoneCurrent
                : _art.StoneAhead;
            face.style.backgroundImage = sprite != null ? new StyleBackground(sprite) : new StyleBackground(StyleKeyword.None);
            shape.style.display = sprite != null ? DisplayStyle.None : DisplayStyle.Flex;
        }

        /// <summary>Every few levels the ship gets a part (ILR-03): that part waits on the trail after the stone that earns it.</summary>
        private void BuildParts(PartArt partArt)
        {
            int every = Math.Max(1, _services.ProgressRules.LevelsPerPart);
            foreach (var part in _services.Parts.Parts)
            {
                int after = (part.Order * every) - 1;
                if (after >= _path.Count - 1)
                {
                    continue;
                }

                var layers = _services.Parts.LayersOf(part.Id);
                var sprite = layers.Count > 0 && partArt != null ? partArt.Find(layers[0].Sprite) : null;
                VisualElement element;
                if (sprite != null)
                {
                    element = new VisualElement();
                    element.style.backgroundImage = new StyleBackground(sprite);
                }
                else
                {
                    element = new Icon(IconKind.Gear) { Color = new Color(0.45f, 0.45f, 0.5f), Accent = Color.white };
                    element.pickingMode = PickingMode.Position;
                }

                element.name = "path-part-" + part.Order;
                element.AddToClassList("path__part");
                element.RegisterCallback<ClickEvent>(_ =>
                {
                    if (element.ClassListContains("path__part--earned"))
                    {
                        _onWorkshop();
                    }
                    else
                    {
                        _services.Voice.Play(MapController.LockedWaitVoice);
                    }
                });
                _parts.Add((element, part.Order, after));
                _scroll.Add(element);
            }
        }

        private void OnStone(int index)
        {
            if (index < 0 || index >= _states.Length)
            {
                return;
            }

            if (PathRules.CanPlay(_states[index]))
            {
                if (_services.ScreenTime.IsExhausted)
                {
                    _onRest();
                    return;
                }

                _services.Navigator.PlayLevel(_path[index]);
                return;
            }

            switch (_states[index])
            {
                case NodeState.NeedsGrownUp:
                    _services.Voice.Play(MapController.AskGrownUpVoice);
                    break;
                default:
                    _services.Voice.Play(MapController.LockedWaitVoice);
                    break;
            }
        }

        private void Layout()
        {
            var r = contentRect;
            int n = _stones.Count;
            // Before the first layout pass the rect is NaN, which slips past a "<= 0" test and would spend the centering.
            if (!(r.width > 0f) || !(r.height > 0f) || n == 0)
            {
                return;
            }

            if (Vector2.Distance(r.size, _centeredFor) > 1f)
            {
                _needsCentering = true;
            }

            // An S-shaped trail from lower left to the right; scrolls sideways once the region grows past the screen.
            float margin = 160f;
            float spacing = n > 1 ? Mathf.Max(MinSpacing, (r.width - (2f * margin)) / (n - 1)) : 0f;
            float width = Mathf.Max(r.width, (2f * margin) + (spacing * (n - 1)));
            _scroll.contentContainer.style.width = width;
            _scroll.contentContainer.style.height = r.height;
            var points = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                float x = margin + (spacing * i);
                float y = r.height * (0.64f - (0.06f * i / Mathf.Max(1, n - 1)) + (0.12f * Mathf.Sin(i * 1.15f)));
                points[i] = new Vector2(x, y);
                var stone = _stones[i];
                stone.style.left = x - (StoneWidth * 0.5f);
                stone.style.top = y - (StoneHeight * StoneShape.TopCentre);
            }

            // The walked trail runs up to Roboya's stone; with nothing next, up to the last finished one.
            int walked = _current >= 0 ? _current : Array.LastIndexOf(_states, NodeState.Completed);
            int robotAt = Mathf.Max(0, walked);
            _track.SetPoints(points, walked, width, r.height);

            foreach (var (element, _, after) in _parts)
            {
                var mid = _track.Between(after);
                element.style.left = mid.x - (PartSize * 0.5f);
                element.style.top = mid.y - (PartSize * 0.5f);
            }

            for (int c = 0; c < _corners.Count; c++)
            {
                int last = ((c + 1) * ProgressQueries.CornerLength) - 1;
                var at = last + 1 < n ? _track.Between(last) : points[last] + new Vector2(100f, 0f);
                // Above the trail where it dips, below where it climbs, so the stump never covers a stone.
                bool above = at.y > r.height * 0.55f;
                _corners[c].style.left = at.x - (CornerWidth * 0.5f);
                _corners[c].style.top = above ? at.y - CornerHeight - 70f : at.y + 50f;
                bool reached = last < _states.Length && _states[last] == NodeState.Completed;
                _corners[c].EnableInClassList("path__corner--reached", reached);
            }

            float rh = Mathf.Min(160f, r.height * 0.22f);
            _robot.style.width = rh * 0.95f;
            _robot.style.height = rh;
            _robot.style.left = points[robotAt].x - (rh * 0.48f);
            // Feet on the back edge of the stone, so its number stays readable in front of Roboya.
            _robot.style.top = points[robotAt].y - rh - 10f;

            if (_needsCentering)
            {
                // Roboya always sits on the next stone; bring that stretch of the trail into view.
                _needsCentering = false;
                _centeredFor = r.size;
                _centerTarget = Mathf.Max(0f, points[robotAt].x - (r.width * 0.5f));
                _centerTries = 0;
                ApplyCentering();
            }
        }

        /// <summary>
        /// The scroller clamps its offset to the range it last laid out. Right after the trail widens that range is
        /// still the old one, so an early offset snaps back to the first stone; wait until the new width is in place.
        /// </summary>
        private void ApplyCentering()
        {
            if (_centerTarget < 0f)
            {
                return;
            }

            float range = _scroll.contentContainer.layout.width - _scroll.contentViewport.layout.width;
            if ((float.IsNaN(range) || range + 0.5f < _centerTarget) && _centerTries++ < 30)
            {
                _scroll.schedule.Execute(ApplyCentering);
                return;
            }

            if (float.IsNaN(range))
            {
                range = 0f;
            }

            _scroll.scrollOffset = new Vector2(Mathf.Clamp(_centerTarget, 0f, Mathf.Max(0f, range)), 0f);
            _centerTarget = -1f;
        }

        private void Pulse()
        {
            _time += 0.033f;
            float wave = Mathf.Max(0f, Mathf.Sin(_time * 3.5f));
            float beat = 1.18f + (wave * 0.06f);
            for (int i = 0; i < _stones.Count && i < _states.Length; i++)
            {
                var state = _states[i];
                float s = state == NodeState.Current ? beat : state == NodeState.Completed ? 1f : 0.84f;
                _stones[i].style.scale = new Scale(new Vector3(s, s, 1f));
            }

            _robot.style.translate = new Translate(0f, -Mathf.Abs(Mathf.Sin(_time * 2.2f)) * 6f);
            float p = 1f + (wave * 0.05f);
            _play.style.scale = new Scale(new Vector3(p, p, 1f));
        }

        /// <summary>
        /// The dirt trail under the stones (board palette, ObliqueGround). The stretch already walked is golden, the
        /// rest is pale, and a dotted line runs down the middle the whole way.
        /// </summary>
        private sealed class PathTrack : VisualElement
        {
            private const int Steps = 12;
            private static readonly Color Edge = new Color(0.83f, 0.68f, 0.43f, 0.7f);
            private static readonly Color Dirt = new Color(0.94f, 0.86f, 0.68f);
            private static readonly Color Walked = new Color(0.98f, 0.76f, 0.35f);
            private static readonly Color WalkedEdge = new Color(0.9f, 0.6f, 0.2f);
            private static readonly Color Dots = new Color(1f, 0.97f, 0.9f, 0.9f);
            private readonly List<Vector2> _curve = new List<Vector2>();
            private int _walked = -1;

            public PathTrack()
            {
                pickingMode = PickingMode.Ignore;
                style.position = Position.Absolute;
                style.left = 0;
                style.top = 0;
                generateVisualContent += Draw;
            }

            /// <summary>Trail through <paramref name="points"/>; the part up to stone <paramref name="walked"/> is golden (none when below 1).</summary>
            public void SetPoints(Vector2[] points, int walked, float width, float height)
            {
                _curve.Clear();
                for (int i = 0; i < points.Length - 1; i++)
                {
                    // Catmull-Rom through the stones, so stone i sits exactly on the trail at sample i * Steps.
                    var p0 = points[Mathf.Max(0, i - 1)];
                    var p1 = points[i];
                    var p2 = points[i + 1];
                    var p3 = points[Mathf.Min(points.Length - 1, i + 2)];
                    for (int s = 0; s < Steps; s++)
                    {
                        _curve.Add(CatmullRom(p0, p1, p2, p3, s / (float)Steps));
                    }
                }

                if (points.Length > 0)
                {
                    _curve.Add(points[points.Length - 1]);
                }

                _walked = walked;
                style.width = width;
                style.height = height;
                MarkDirtyRepaint();
            }

            /// <summary>The middle of the trail between stone <paramref name="index"/> and the next.</summary>
            public Vector2 Between(int index)
            {
                int i = (index * Steps) + (Steps / 2);
                return i >= 0 && i < _curve.Count ? _curve[i] : Vector2.zero;
            }

            private static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
            {
                float t2 = t * t;
                float t3 = t2 * t;
                return 0.5f * ((2f * p1) + ((p2 - p0) * t) + (((2f * p0) - (5f * p1) + (4f * p2) - p3) * t2) + (((3f * p1) - p0 - (3f * p2) + p3) * t3));
            }

            private void Draw(MeshGenerationContext mgc)
            {
                if (_curve.Count < 2)
                {
                    return;
                }

                var p = mgc.painter2D;
                p.lineJoin = LineJoin.Round;
                p.lineCap = LineCap.Round;
                Stroke(p, Edge, 58f, _curve.Count - 1);
                Stroke(p, Dirt, 46f, _curve.Count - 1);
                if (_walked > 0)
                {
                    int end = Mathf.Min(_curve.Count - 1, _walked * Steps);
                    Stroke(p, WalkedEdge, 58f, end);
                    Stroke(p, Walked, 46f, end);
                }

                // Small dots every few samples down the middle.
                p.fillColor = Dots;
                for (int i = 2; i < _curve.Count; i += 4)
                {
                    p.BeginPath();
                    p.Arc(_curve[i], 3.5f, Angle.Degrees(0f), Angle.Degrees(360f));
                    p.Fill();
                }
            }

            private void Stroke(Painter2D p, Color color, float width, int last)
            {
                p.strokeColor = color;
                p.lineWidth = width;
                p.BeginPath();
                p.MoveTo(_curve[0]);
                for (int i = 1; i <= last; i++)
                {
                    p.LineTo(_curve[i]);
                }

                p.Stroke();
            }
        }
    }
}
