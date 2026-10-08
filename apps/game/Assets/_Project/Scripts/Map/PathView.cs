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
    /// Patience Forest path (F1-05): one stepping stone per level along a winding dirt trail. Finished stones show
    /// their stars, the next one pulses with Roboya on it, later ones are faded, and stones past the free tier
    /// carry a padlock that asks the child to fetch a grown-up (GLR-01). Tapping never sells anything.
    /// </summary>
    public sealed class PathView : VisualElement, IRefreshable
    {
        private const float StoneSize = 104f;
        private const float MinSpacing = 132f;

        private readonly GameServices _services;
        private readonly List<string> _path;
        private readonly ScrollView _scroll = new ScrollView(ScrollViewMode.Horizontal) { name = "path-scroll" };
        private readonly PathTrack _track = new PathTrack();
        private readonly List<VisualElement> _stones = new List<VisualElement>();
        private readonly RobotAvatar _robot;
        private NodeState[] _states = new NodeState[0];
        private float _time;

        public PathView(GameServices services, RegionArt art, RobotWardrobe wardrobe, Action onIsland, Action onGarage)
        {
            _services = services;
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
            Add(_scroll);

            for (int i = 0; i < _path.Count; i++)
            {
                var stone = BuildStone(i);
                _stones.Add(stone);
                _scroll.Add(stone);
            }

            _robot = new RobotAvatar(wardrobe, art != null ? art.RobotFront : null) { name = "path-robot" };
            _robot.AddToClassList("path__robot");
            _scroll.Add(_robot);

            var bar = new VisualElement();
            bar.AddToClassList("map-bar");
            var island = new IconButton(IconKind.Island, onIsland) { name = "to-island" };
            var garage = new IconButton(IconKind.Garage, onGarage) { name = "to-garage" };
            garage.AddToClassList("map-bar__garage");
            bar.Add(island);
            bar.Add(garage);
            Add(bar);

            RegisterCallback<GeometryChangedEvent>(_ => Layout());
            schedule.Execute(Pulse).Every(33);
        }

        public IReadOnlyList<string> Path => _path;

        public void Refresh()
        {
            _states = ProgressQueries.PathStates(_services, _path);
            var book = _services.Progress.Book;
            for (int i = 0; i < _stones.Count; i++)
            {
                var stone = _stones[i];
                var state = _states[i];
                foreach (var c in new[] { "stone--done", "stone--current", "stone--locked", "stone--grownup" })
                {
                    stone.RemoveFromClassList(c);
                }

                stone.AddToClassList(state == NodeState.Completed ? "stone--done"
                    : state == NodeState.Current ? "stone--current"
                    : state == NodeState.Locked ? "stone--locked"
                    : "stone--grownup");

                stone.Q<Icon>("stone-icon").Kind = state == NodeState.NeedsGrownUp ? IconKind.Lock : IconKind.Forward;
                var stars = stone.Q("stone-stars");
                int earned = book.Stars(_path[i]);
                for (int s = 0; s < 3; s++)
                {
                    ((Icon)stars[s]).Kind = s < earned ? IconKind.Star : IconKind.StarEmpty;
                }

                stars.style.display = state == NodeState.Completed ? DisplayStyle.Flex : DisplayStyle.None;
            }

            _robot.Refresh();
            Layout();
        }

        private VisualElement BuildStone(int index)
        {
            var stone = new VisualElement { name = "stone-" + (index + 1) };
            stone.AddToClassList("stone");
            var face = new VisualElement();
            face.AddToClassList("stone__face");
            face.Add(new Icon(IconKind.Forward) { name = "stone-icon", Color = Color.white, Accent = new Color(0.45f, 0.36f, 0.3f) });
            stone.Add(face);
            var stars = new VisualElement { name = "stone-stars" };
            stars.AddToClassList("stone__stars");
            for (int s = 0; s < 3; s++)
            {
                var star = new Icon(IconKind.StarEmpty) { Color = new Color(1f, 0.78f, 0.15f) };
                star.AddToClassList("stone__star");
                stars.Add(star);
            }

            stone.Add(stars);
            stone.RegisterCallback<ClickEvent>(_ => OnStone(index));
            return stone;
        }

        private void OnStone(int index)
        {
            if (index >= _states.Length)
            {
                return;
            }

            switch (_states[index])
            {
                case NodeState.Completed:
                case NodeState.Current:
                    _services.Navigator.PlayLevel(_path[index]);
                    break;
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
            if (r.width <= 0f || r.height <= 0f || n == 0)
            {
                return;
            }

            // An S-shaped trail from lower left to the right; scrolls sideways once the region grows past the screen.
            float margin = 150f;
            float spacing = n > 1 ? Mathf.Max(MinSpacing, (r.width - (2f * margin)) / (n - 1)) : 0f;
            float width = Mathf.Max(r.width, (2f * margin) + (spacing * (n - 1)));
            _scroll.contentContainer.style.width = width;
            _scroll.contentContainer.style.height = r.height;
            var points = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                float x = margin + (spacing * i);
                float y = r.height * (0.66f - (0.08f * i / Mathf.Max(1, n - 1)) + (0.13f * Mathf.Sin(i * 1.15f)));
                points[i] = new Vector2(x, y);
                var stone = _stones[i];
                stone.style.left = x - (StoneSize * 0.5f);
                stone.style.top = y - (StoneSize * 0.5f);
            }

            _track.SetPoints(points, width, r.height);

            int current = Array.IndexOf(_states, NodeState.Current);
            if (current < 0)
            {
                current = Mathf.Max(0, Array.LastIndexOf(_states, NodeState.Completed));
            }

            float rh = Mathf.Min(170f, r.height * 0.24f);
            _robot.style.width = rh * 0.95f;
            _robot.style.height = rh;
            _robot.style.left = points[current].x - (rh * 0.48f);
            _robot.style.top = points[current].y - (StoneSize * 0.3f) - rh;
        }

        private void Pulse()
        {
            _time += 0.033f;
            float s = 1f + (Mathf.Max(0f, Mathf.Sin(_time * 4f)) * 0.08f);
            for (int i = 0; i < _stones.Count && i < _states.Length; i++)
            {
                _stones[i].style.scale = _states[i] == NodeState.Current ? new Scale(new Vector3(s, s, 1f)) : new Scale(Vector3.one);
            }

            _robot.style.translate = new Translate(0f, -Mathf.Abs(Mathf.Sin(_time * 2.2f)) * 6f);
        }

        /// <summary>The dirt trail under the stones, drawn with the board palette (ObliqueGround).</summary>
        private sealed class PathTrack : VisualElement
        {
            private static readonly Color Edge = new Color(0.83f, 0.68f, 0.43f);
            private static readonly Color Dirt = new Color(0.906f, 0.788f, 0.561f);
            private Vector2[] _points = new Vector2[0];

            public PathTrack()
            {
                pickingMode = PickingMode.Ignore;
                style.position = Position.Absolute;
                style.left = 0;
                style.top = 0;
                generateVisualContent += Draw;
            }

            public void SetPoints(Vector2[] points, float width, float height)
            {
                _points = points;
                style.width = width;
                style.height = height;
                MarkDirtyRepaint();
            }

            private void Draw(MeshGenerationContext mgc)
            {
                if (_points.Length < 2)
                {
                    return;
                }

                var p = mgc.painter2D;
                p.lineJoin = LineJoin.Round;
                p.lineCap = LineCap.Round;
                Stroke(p, Edge, 60f);
                Stroke(p, Dirt, 46f);
            }

            private void Stroke(Painter2D p, Color color, float width)
            {
                p.strokeColor = color;
                p.lineWidth = width;
                p.BeginPath();
                p.MoveTo(_points[0]);
                for (int i = 1; i < _points.Length - 1; i++)
                {
                    // Smooth corners: curve through each stone towards the midpoint of the next segment.
                    p.QuadraticCurveTo(_points[i], (_points[i] + _points[i + 1]) * 0.5f);
                }

                p.LineTo(_points[_points.Length - 1]);
                p.Stroke();
            }
        }
    }
}
