using System.Collections.Generic;
using System.Threading;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.Solving;
using Roboya.CodingEngine.World;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Games.YonAvcisi
{
    /// <summary>
    /// The grid, robot, goal and items. Pure view: positions come from engine state; animations are awaited
    /// by the controller one execution event at a time. Uses region sprites when available and falls back to
    /// code-drawn icons otherwise.
    /// </summary>
    public sealed class BoardView : VisualElement
    {
        public const float MoveSeconds = 0.45f;
        public const float TurnSeconds = 0.3f;
        public const float BumpSeconds = 0.5f;

        private enum Mood
        {
            Normal,
            Laughing,
            Happy,
        }

        private readonly RegionArt _art;
        private readonly VisualElement _grid = new VisualElement { name = "grid" };
        private readonly VisualElement _layer = new VisualElement { name = "pieces" };
        private readonly List<VisualElement> _items = new List<VisualElement>();
        private readonly List<VisualElement> _ghost = new List<VisualElement>();
        private readonly List<VisualElement> _trail = new List<VisualElement>();

        private Level _level;
        private VisualElement _robot;
        private VisualElement _goal;
        private float _cell;
        private Vector2 _origin;
        private Vector2 _robotPos;
        private float _robotAngle;
        private Direction _facing;
        private Mood _mood;
        private float _squash = 1f;
        private float _hop;
        private float _wobble;

        public BoardView(RegionArt art = null)
        {
            _art = art;
            AddToClassList("board");
            _grid.AddToClassList("board__grid");
            _layer.AddToClassList("board__pieces");
            _layer.pickingMode = PickingMode.Ignore;
            Add(_grid);
            Add(_layer);
            RegisterCallback<GeometryChangedEvent>(_ => Layout());
        }

        private bool UseSprites => _art != null && _art.RobotFront != null;

        public void Show(Level level, bool ghostPath)
        {
            _level = level;
            _grid.Clear();
            _layer.Clear();
            _items.Clear();
            _ghost.Clear();
            _trail.Clear();
            _grid.EnableInClassList("board__grid--sprites", UseSprites);

            var ghostCells = ghostPath ? new HashSet<GridPosition>(GhostCells(level)) : new HashSet<GridPosition>();
            for (int y = 0; y < level.Grid.Height; y++)
            {
                for (int x = 0; x < level.Grid.Width; x++)
                {
                    var p = new GridPosition(x, y);
                    bool blocked = level.Grid[p] == CellType.Blocked;
                    var cell = new VisualElement();
                    cell.AddToClassList("cell");
                    if (UseSprites)
                    {
                        // YON-03: the ghost route is drawn as a dirt path through the grass.
                        SetSprite(cell, ghostCells.Contains(p) && _art.TilePath != null ? _art.TilePath : _art.TileFloor);
                        if (blocked)
                        {
                            var obstacle = new VisualElement();
                            obstacle.AddToClassList("cell__obstacle");
                            SetSprite(obstacle, _art.ObstacleFor(x, y));
                            cell.Add(obstacle);
                        }
                    }
                    else if (blocked)
                    {
                        cell.AddToClassList("cell--blocked");
                    }

                    _grid.Add(cell);
                }
            }

            if (!UseSprites)
            {
                foreach (var p in ghostCells)
                {
                    var dot = new VisualElement();
                    dot.AddToClassList("ghost-dot");
                    _ghost.Add(dot);
                    dot.userData = p;
                    _layer.Add(dot);
                }
            }

            if (level.Goal.Reach.HasValue)
            {
                _goal = UseSprites && _art.GoalIdle != null
                    ? SpritePiece(_art.GoalIdle)
                    : new Icon(IconKind.Turtle) { Color = new Color(0.36f, 0.62f, 0.31f), Accent = new Color(0.62f, 0.8f, 0.45f) };
                _goal.name = "goal";
                _goal.AddToClassList("piece");
                _goal.AddToClassList("piece--goal");
                _goal.userData = level.Goal.Reach.Value;
                _layer.Add(_goal);
            }

            foreach (var item in level.Items)
            {
                var sprite = UseSprites ? _art.ItemFor(item.Kind, item.Color) : null;
                VisualElement piece = sprite != null
                    ? SpritePiece(sprite)
                    : new Icon(item.Kind == "ship-part" ? IconKind.Gear : IconKind.Fruit)
                    {
                        Color = ItemColor(item.Color),
                        Accent = item.Kind == "ship-part" ? new Color(0.35f, 0.35f, 0.4f) : new Color(0.3f, 0.6f, 0.25f),
                    };
                piece.AddToClassList("piece");
                piece.AddToClassList("piece--item");
                piece.userData = item.Position;
                _items.Add(piece);
                _layer.Add(piece);
            }

            _robot = UseSprites
                ? SpritePiece(_art.RobotFront)
                : new Icon(IconKind.Robot) { Color = new Color(0.96f, 0.55f, 0.16f), Accent = Color.white };
            _robot.name = "robot";
            _robot.AddToClassList("piece");
            _robot.AddToClassList("piece--robot");
            _layer.Add(_robot);

            ResetRobot(level.Start);
            Layout();
        }

        public void ResetRobot(RobotState state)
        {
            _robotPos = new Vector2(state.Position.X, state.Position.Y);
            _robotAngle = Angle(state.Facing);
            _facing = state.Facing;
            _mood = Mood.Normal;
            _squash = 1f;
            _hop = 0f;
            _wobble = 0f;
            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].RemoveFromClassList("piece--collected");
            }

            if (_goal != null && UseSprites && _art.GoalIdle != null)
            {
                SetSprite(_goal, _art.GoalIdle);
            }

            if (_goal != null && _cell > 0f)
            {
                Position(_goal, (GridPosition)_goal.userData, UseSprites ? 0.92f : 0.8f);
            }

            ClearTrail();
            PlaceRobot();
        }

        public async Awaitable AnimateMove(GridPosition from, GridPosition to, CancellationToken token)
        {
            var a = new Vector2(from.X, from.Y);
            var b = new Vector2(to.X, to.Y);
            AddTrailDot(from);
            await Tween.Run(MoveSeconds, t =>
            {
                _robotPos = Vector2.LerpUnclamped(a, b, t);
                _hop = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2f)) * 0.06f; // little walking bob
                PlaceRobot();
            }, token);
            _hop = 0f;
            PlaceRobot();
        }

        public async Awaitable AnimateTurn(Direction from, Direction to, CancellationToken token)
        {
            if (UseSprites)
            {
                // Sprites show facing by view (front/back/side), so a turn is a quick squash-and-swap.
                await Tween.Run(TurnSeconds, t =>
                {
                    if (t >= 0.5f)
                    {
                        _facing = to;
                    }

                    _squash = Mathf.Abs(1f - 2f * t) * 0.8f + 0.2f;
                    PlaceRobot();
                }, token);
                _facing = to;
                _squash = 1f;
                PlaceRobot();
                return;
            }

            float a = Angle(from);
            float b = a + Mathf.DeltaAngle(a, Angle(to));
            await Tween.Run(TurnSeconds, t =>
            {
                _robotAngle = Mathf.LerpUnclamped(a, b, t);
                PlaceRobot();
            }, token);
            _robotAngle = Angle(to);
        }

        /// <summary>Roboya bumps, laughs and wobbles; no penalty, just feedback (OYN-03).</summary>
        public async Awaitable AnimateBump(Direction facing, CancellationToken token)
        {
            var off = facing.Offset();
            var home = _robotPos;
            _mood = Mood.Laughing;
            await Tween.Run(BumpSeconds, t =>
            {
                float push = Mathf.Sin(t * Mathf.PI) * 0.18f;
                _robotPos = home + new Vector2(off.X, off.Y) * push;
                _wobble = Tween.Shake(t, UseSprites ? 8f : 2f);
                PlaceRobot();
            }, token);
            _robotPos = home;
            _wobble = 0f;
            PlaceRobot();
            await Tween.Delay(0.4f, token);
            _mood = Mood.Normal;
            PlaceRobot();
        }

        public async Awaitable AnimateCollect(int itemIndex, CancellationToken token)
        {
            if (itemIndex < 0 || itemIndex >= _items.Count)
            {
                return;
            }

            var item = _items[itemIndex];
            await Tween.Run(0.3f, t => item.style.scale = new Scale(Vector3.one * (1f + 0.5f * t)), token);
            item.AddToClassList("piece--collected");
            item.style.scale = new Scale(Vector3.one);
        }

        public async Awaitable Celebrate(CancellationToken token)
        {
            _mood = Mood.Happy;
            if (_goal != null && UseSprites && _art.GoalHappy != null)
            {
                SetSprite(_goal, _art.GoalHappy);
            }

            // Robot and turtle meet in the same cell: the turtle steps aside so both are visible.
            Vector3 goalHome = _goal != null ? _goal.resolvedStyle.translate : Vector3.zero;
            var slide = Tween.Run(0.8f, t =>
            {
                if (_goal != null && _goal.userData is GridPosition g && new Vector2(g.X, g.Y) == _robotPos)
                {
                    _goal.style.translate = new Translate(goalHome.x + _cell * 0.45f * Mathf.Min(1f, t * 3f), goalHome.y);
                }
            }, token);
            await Tween.Run(0.8f, t =>
            {
                _hop = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 3f)) * 0.25f;
                PlaceRobot();
            }, token);
            await slide;
            _hop = 0f;
            PlaceRobot();
        }

        private void AddTrailDot(GridPosition p)
        {
            var dot = new VisualElement();
            dot.AddToClassList("trail-dot");
            dot.userData = p;
            _trail.Add(dot);
            _layer.Insert(0, dot);
            Position(dot, p, 0.24f);
        }

        private void ClearTrail()
        {
            foreach (var dot in _trail)
            {
                dot.RemoveFromHierarchy();
            }

            _trail.Clear();
        }

        private void Layout()
        {
            if (_level == null)
            {
                return;
            }

            var size = contentRect.size;
            _cell = Mathf.Floor(Mathf.Min(size.x / _level.Grid.Width, size.y / _level.Grid.Height));
            if (_cell <= 0f)
            {
                return;
            }

            _origin = new Vector2((size.x - _cell * _level.Grid.Width) * 0.5f, (size.y - _cell * _level.Grid.Height) * 0.5f);
            _grid.style.left = _origin.x;
            _grid.style.top = _origin.y;
            _grid.style.width = _cell * _level.Grid.Width;
            _grid.style.height = _cell * _level.Grid.Height;
            foreach (var child in _grid.Children())
            {
                child.style.width = _cell;
                child.style.height = _cell;
            }

            foreach (var dot in _ghost)
            {
                Position(dot, (GridPosition)dot.userData, 0.22f);
            }

            foreach (var dot in _trail)
            {
                Position(dot, (GridPosition)dot.userData, 0.24f);
            }

            if (_goal != null)
            {
                Position(_goal, (GridPosition)_goal.userData, UseSprites ? 0.92f : 0.8f);
            }

            foreach (var item in _items)
            {
                Position(item, (GridPosition)item.userData, 0.6f);
            }

            float robotSize = UseSprites ? 0.96f : 0.82f;
            _robot.style.width = _cell * robotSize;
            _robot.style.height = _cell * robotSize;
            PlaceRobot();
        }

        private void PlaceRobot()
        {
            if (_robot == null)
            {
                return;
            }

            float size = _robot.resolvedStyle.width > 0f ? _robot.resolvedStyle.width : _cell * 0.9f;
            float pad = (_cell - size) * 0.5f;
            // Sprites stand up in their cell (drawn slightly higher so feet sit on the tile).
            float lift = UseSprites ? _cell * 0.12f : 0f;
            _robot.style.translate = new Translate(
                _origin.x + _robotPos.x * _cell + pad,
                _origin.y + (_robotPos.y - _hop) * _cell + pad - lift);

            if (!UseSprites)
            {
                _robot.style.rotate = new Rotate(new UnityEngine.UIElements.Angle(_robotAngle + _wobble, AngleUnit.Degree));
                return;
            }

            // The side sprite faces west; mirror it for east.
            bool mirror = _mood == Mood.Normal && _facing == Direction.East;
            _robot.style.scale = new Scale(new Vector3((mirror ? -1f : 1f) * _squash, 1f, 1f));
            _robot.style.rotate = new Rotate(new UnityEngine.UIElements.Angle(_wobble, AngleUnit.Degree));
            SetSprite(_robot, RobotSprite());
        }

        private Sprite RobotSprite()
        {
            if (_mood == Mood.Laughing && _art.RobotLaughing != null)
            {
                return _art.RobotLaughing;
            }

            if (_mood == Mood.Happy && _art.RobotHappy != null)
            {
                return _art.RobotHappy;
            }

            // Unity objects: use explicit null checks, not ??, so unassigned slots fall back correctly.
            var view = _facing == Direction.North ? _art.RobotBack
                : _facing == Direction.South ? _art.RobotFront
                : _art.RobotSide;
            return view != null ? view : _art.RobotFront;
        }

        private void Position(VisualElement e, GridPosition p, float fraction)
        {
            float size = _cell * fraction;
            float pad = (_cell - size) * 0.5f;
            e.style.width = size;
            e.style.height = size;
            e.style.translate = new Translate(_origin.x + p.X * _cell + pad, _origin.y + p.Y * _cell + pad);
        }

        private static VisualElement SpritePiece(Sprite sprite)
        {
            var e = new VisualElement { pickingMode = PickingMode.Ignore };
            e.AddToClassList("sprite");
            SetSprite(e, sprite);
            return e;
        }

        private static void SetSprite(VisualElement e, Sprite sprite)
        {
            if (sprite != null && e.style.backgroundImage.value.sprite != sprite)
            {
                e.style.backgroundImage = new StyleBackground(sprite);
            }
        }

        /// <summary>YON-03: cells along one shortest route.</summary>
        private static IEnumerable<GridPosition> GhostCells(Level level)
        {
            var solve = Solver.Solve(level);
            var state = level.Start;
            foreach (var card in solve.Solution)
            {
                Solver.TryApply(level, state, card, out var next);
                if (card == CardType.Forward || card == CardType.Backward)
                {
                    yield return next.Position;
                }

                state = next;
            }
        }

        private static float Angle(Direction d) => (int)d * 90f;

        private static Color ItemColor(string color)
        {
            switch (color)
            {
                case "red": return new Color(0.86f, 0.24f, 0.2f);
                case "yellow": return new Color(0.98f, 0.8f, 0.2f);
                case "blue": return new Color(0.25f, 0.5f, 0.9f);
                case "green": return new Color(0.3f, 0.7f, 0.35f);
                case "purple": return new Color(0.6f, 0.35f, 0.8f);
                case "orange": return new Color(0.98f, 0.55f, 0.15f);
                default: return new Color(0.75f, 0.72f, 0.65f);
            }
        }
    }
}
