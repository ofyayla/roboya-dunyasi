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
    /// by the controller one execution event at a time.
    /// </summary>
    public sealed class BoardView : VisualElement
    {
        public const float MoveSeconds = 0.45f;
        public const float TurnSeconds = 0.3f;
        public const float BumpSeconds = 0.5f;

        private readonly VisualElement _grid = new VisualElement { name = "grid" };
        private readonly VisualElement _layer = new VisualElement { name = "pieces" };
        private readonly List<VisualElement> _items = new List<VisualElement>();
        private readonly List<VisualElement> _ghost = new List<VisualElement>();
        private readonly List<VisualElement> _trail = new List<VisualElement>();

        private Level _level;
        private Icon _robot;
        private Icon _goal;
        private float _cell;
        private Vector2 _origin;
        private Vector2 _robotPos;
        private float _robotAngle;

        public BoardView()
        {
            AddToClassList("board");
            _grid.AddToClassList("board__grid");
            _layer.AddToClassList("board__pieces");
            _layer.pickingMode = PickingMode.Ignore;
            Add(_grid);
            Add(_layer);
            RegisterCallback<GeometryChangedEvent>(_ => Layout());
        }

        public void Show(Level level, bool ghostPath)
        {
            _level = level;
            _grid.Clear();
            _layer.Clear();
            _items.Clear();
            _ghost.Clear();
            _trail.Clear();

            for (int y = 0; y < level.Grid.Height; y++)
            {
                for (int x = 0; x < level.Grid.Width; x++)
                {
                    var cell = new VisualElement();
                    cell.AddToClassList("cell");
                    if (level.Grid[new GridPosition(x, y)] == CellType.Blocked)
                    {
                        cell.AddToClassList("cell--blocked");
                    }

                    _grid.Add(cell);
                }
            }

            if (ghostPath)
            {
                foreach (var p in GhostCells(level))
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
                _goal = new Icon(IconKind.Turtle) { name = "goal" };
                _goal.AddToClassList("piece");
                _goal.AddToClassList("piece--goal");
                _goal.Color = new Color(0.36f, 0.62f, 0.31f);
                _goal.Accent = new Color(0.62f, 0.8f, 0.45f);
                _goal.userData = level.Goal.Reach.Value;
                _layer.Add(_goal);
            }

            foreach (var item in level.Items)
            {
                var icon = new Icon(item.Kind == "ship-part" ? IconKind.Gear : IconKind.Fruit);
                icon.AddToClassList("piece");
                icon.AddToClassList("piece--item");
                icon.Color = ItemColor(item.Color);
                icon.Accent = item.Kind == "ship-part" ? new Color(0.35f, 0.35f, 0.4f) : new Color(0.3f, 0.6f, 0.25f);
                icon.userData = item.Position;
                _items.Add(icon);
                _layer.Add(icon);
            }

            _robot = new Icon(IconKind.Robot) { name = "robot" };
            _robot.AddToClassList("piece");
            _robot.AddToClassList("piece--robot");
            _robot.Color = new Color(0.96f, 0.55f, 0.16f);
            _robot.Accent = Color.white;
            _layer.Add(_robot);

            ResetRobot(level.Start);
            Layout();
        }

        public void ResetRobot(RobotState state)
        {
            _robotPos = new Vector2(state.Position.X, state.Position.Y);
            _robotAngle = Angle(state.Facing);
            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].RemoveFromClassList("piece--collected");
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
                PlaceRobot();
            }, token);
        }

        public async Awaitable AnimateTurn(Direction from, Direction to, CancellationToken token)
        {
            float a = Angle(from);
            float b = a + Mathf.DeltaAngle(a, Angle(to));
            await Tween.Run(TurnSeconds, t =>
            {
                _robotAngle = Mathf.LerpUnclamped(a, b, t);
                PlaceRobot();
            }, token);
            _robotAngle = Angle(to);
        }

        /// <summary>Roboya bumps and wobbles; no penalty, just feedback (OYN-03).</summary>
        public async Awaitable AnimateBump(Direction facing, CancellationToken token)
        {
            var off = facing.Offset();
            var home = _robotPos;
            await Tween.Run(BumpSeconds, t =>
            {
                float push = Mathf.Sin(t * Mathf.PI) * 0.18f;
                _robotPos = home + new Vector2(off.X, off.Y) * push;
                _robotAngle += Tween.Shake(t, 2f);
                PlaceRobot();
            }, token);
            _robotPos = home;
            _robotAngle = Mathf.Round(_robotAngle / 90f) * 90f;
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
            await Tween.Run(0.8f, t =>
            {
                float hop = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 3f)) * 0.25f;
                _robot.style.translate = new Translate(_origin.x + _robotPos.x * _cell, _origin.y + (_robotPos.y - hop) * _cell);
            }, token);
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
                Position(_goal, (GridPosition)_goal.userData, 0.8f);
            }

            foreach (var item in _items)
            {
                Position(item, (GridPosition)item.userData, 0.6f);
            }

            _robot.style.width = _cell * 0.82f;
            _robot.style.height = _cell * 0.82f;
            PlaceRobot();
        }

        private void PlaceRobot()
        {
            if (_robot == null)
            {
                return;
            }

            float pad = _cell * 0.09f;
            _robot.style.translate = new Translate(_origin.x + _robotPos.x * _cell + pad, _origin.y + _robotPos.y * _cell + pad);
            _robot.style.rotate = new Rotate(new UnityEngine.UIElements.Angle(_robotAngle, AngleUnit.Degree));
        }

        private void Position(VisualElement e, GridPosition p, float fraction)
        {
            float size = _cell * fraction;
            float pad = (_cell - size) * 0.5f;
            e.style.width = size;
            e.style.height = size;
            e.style.translate = new Translate(_origin.x + p.X * _cell + pad, _origin.y + p.Y * _cell + pad);
        }

        /// <summary>YON-03: cells along one shortest route, shown faintly.</summary>
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
