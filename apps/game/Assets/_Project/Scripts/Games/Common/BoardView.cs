using System.Collections.Generic;
using System.Threading;
using Roboya.CodingEngine.Commands;
using LevelDto = Roboya.CodingEngine.Levels.Generated.LevelDto;
using ObstacleLook = Roboya.CodingEngine.Levels.Generated.ObstacleLook;
using Roboya.CodingEngine.Solving;
using Roboya.CodingEngine.World;
using Roboya.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Roboya.Games.Common
{
    /// <summary>
    /// The board seen at a gentle angle (ObliqueProjection): a code-drawn ground with the robot, goal, items and
    /// obstacles standing on it, sorted by depth. Pure view: positions come from engine state; animations are
    /// awaited by the controller one execution event at a time. Uses region sprites when available and falls back
    /// to code-drawn icons otherwise.
    /// </summary>
    public sealed class BoardView : VisualElement
    {
        public const float MoveSeconds = 0.45f;
        public const float TurnSeconds = 0.3f;
        public const float BumpSeconds = 0.5f;

        // Sprite sizes in front-cell units. Characters are a little taller than a cell so they read as standing.
        private const float RobotHeight = 1.12f;
        private const float GoalHeight = 0.95f;
        private const float ItemHeight = 0.55f;
        private const float ObstacleWidth = 0.98f;
        private const float DecorWidth = 0.9f;

        // Generated sprites keep ~3.5% transparent margin under the feet.
        private const float FeetFraction = 0.965f;

        private enum Mood
        {
            Normal,
            Laughing,
            Happy,
        }

        private sealed class Piece
        {
            public VisualElement View;
            public VisualElement Shadow;
            public Vector2 Anchor;
            public float Height;
            public float Width;
            public float Aspect = 1f;
            public float Lift;
            public int Order;
        }

        private readonly RegionArt _art;
        private readonly ObliqueGround _ground = new ObliqueGround();
        private readonly VisualElement _decals = new VisualElement { name = "decals" };
        private readonly VisualElement _layer = new VisualElement { name = "pieces" };
        private readonly List<Piece> _pieces = new List<Piece>();
        private readonly List<Piece> _items = new List<Piece>();
        private readonly List<VisualElement> _trail = new List<VisualElement>();

        private Level _level;
        private ObliqueProjection _proj;
        private Piece _robot;
        private Piece _goal;
        private Vector2 _goalHome;
        private Vector2 _robotPos;
        private float _robotAngle;
        private Direction _facing;
        private Mood _mood;
        private float _squash = 1f;
        private float _hop;
        private float _wobble;
        private int _robotDepthBucket = int.MinValue;
        private bool _hasNorthScenery;
        private ActorSprites _actor;
        private readonly FacingArrow _arrow = new FacingArrow();
        private readonly Vector2[] _arrowCells = new Vector2[FacingArrow.PointCount];

        public BoardView(RegionArt art = null)
        {
            _art = art;
            AddToClassList("board");
            _decals.AddToClassList("board__layer");
            _layer.AddToClassList("board__layer");
            _decals.pickingMode = PickingMode.Ignore;
            _layer.pickingMode = PickingMode.Ignore;
            Add(_ground);
            Add(_decals);
            Add(_layer);
            // Above the pieces: the robot is taller than a cell and would hide an arrow lying behind it.
            Add(_arrow);
            RegisterCallback<GeometryChangedEvent>(_ => Layout());
        }

        private bool UseSprites => _art != null && _art.RobotFront != null;

        /// <param name="dto">Level data for v2 looks and scenery; null draws the region's default obstacle mix.</param>
        public void Show(Level level, bool ghostPath, LevelDto dto = null)
        {
            _level = level;
            _hasNorthScenery = false;
            _actor = _art != null ? _art.ActorFor(dto != null ? dto.Game : Roboya.CodingEngine.Levels.Generated.GameId.YonAvcisi) : default;
            _decals.Clear();
            _ground.SetBox(dto != null && dto.Game == Roboya.CodingEngine.Levels.Generated.GameId.KodlamaKutusu);
            _layer.Clear();
            _pieces.Clear();
            _items.Clear();
            _trail.Clear();
            _goal = null;
            _robotDepthBucket = int.MinValue;

            // YON-03: the ghost route is drawn as a dirt path from the start through the grass.
            var path = new HashSet<GridPosition>();
            if (ghostPath)
            {
                path.Add(level.Start.Position);
                foreach (var p in GhostCells(level))
                {
                    path.Add(p);
                }
            }

            _ground.Set(level.Grid, path);

            for (int y = 0; y < level.Grid.Height; y++)
            {
                for (int x = 0; x < level.Grid.Width; x++)
                {
                    if (level.Grid[new GridPosition(x, y)] != CellType.Blocked)
                    {
                        continue;
                    }

                    var look = LookAt(dto, x, y);
                    var obstacle = UseSprites
                        ? AddLook(look, CellAnchor(x, y), (x, y))
                        : AddPiece(new VisualElement(), CellAnchor(x, y), height: 0.6f);
                    obstacle.View.AddToClassList(UseSprites ? "piece--obstacle" : "piece--block");
                }
            }

            if (UseSprites)
            {
                AddDecor(level);
                AddScenery(dto, level);
            }

            if (level.Goal.Reach.HasValue)
            {
                var g = level.Goal.Reach.Value;
                _goalHome = CellAnchor(g.X, g.Y);
                _goal = UseSprites && _art.GoalIdle != null
                    ? AddSprite(_art.GoalIdle, _goalHome, height: GoalHeight)
                    : AddPiece(new Icon(IconKind.Turtle) { Color = new Color(0.36f, 0.62f, 0.31f), Accent = new Color(0.62f, 0.8f, 0.45f) }, _goalHome, height: 0.8f);
                _goal.View.name = "goal";
                _goal.View.AddToClassList("piece--goal");
            }

            foreach (var item in level.Items)
            {
                var sprite = UseSprites ? _art.ItemFor(item.Kind, item.Color) : null;
                var anchor = CellAnchor(item.Position.X, item.Position.Y);
                var piece = sprite != null
                    ? AddSprite(sprite, anchor, height: ItemHeight)
                    : AddPiece(
                        new Icon(item.Kind == "ship-part" ? IconKind.Gear : IconKind.Fruit)
                        {
                            Color = ItemColor(item.Color),
                            Accent = item.Kind == "ship-part" ? new Color(0.35f, 0.35f, 0.4f) : new Color(0.3f, 0.6f, 0.25f),
                        },
                        anchor,
                        height: 0.5f);
                piece.View.AddToClassList("piece--item");
                _items.Add(piece);
            }

            var start = CellAnchor(level.Start.Position.X, level.Start.Position.Y);
            _robot = UseSprites
                ? AddSprite(_actor.Front, start, height: RobotHeight)
                : AddPiece(new Icon(IconKind.Robot) { Color = new Color(0.96f, 0.55f, 0.16f), Accent = Color.white }, start, height: 0.82f);
            _robot.View.name = "robot";
            _robot.View.AddToClassList("piece--robot");
            _robot.Order = 1; // in front of an item sharing its cell

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
            foreach (var item in _items)
            {
                item.View.RemoveFromClassList("piece--collected");
                item.Shadow.RemoveFromClassList("piece--collected");
            }

            if (_goal != null)
            {
                _goal.Anchor = _goalHome;
                if (UseSprites && _art.GoalIdle != null)
                {
                    SetSprite(_goal, _art.GoalIdle);
                }

                Place(_goal);
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
            await Tween.Run(0.3f, t => item.View.style.scale = new Scale(Vector3.one * (1f + 0.5f * t)), token);
            item.View.AddToClassList("piece--collected");
            item.Shadow.AddToClassList("piece--collected");
            item.View.style.scale = new Scale(Vector3.one);
        }

        /// <summary>
        /// After a run that fell short: what is still missing breathes for a moment, so the child sees where to go next. Items
        /// still on the board that the goal needs pulse; when everything is collected the goal character pulses instead.
        /// </summary>
        public async Awaitable PulseMissing(ulong collectedMask, CancellationToken token)
        {
            if (_level == null)
            {
                return;
            }

            var targets = new List<Piece>();
            ulong needed = _level.Goal.MustCollectMask;
            for (int i = 0; i < _items.Count && i < Level.MaxItems; i++)
            {
                if ((needed & (1UL << i)) != 0 && (collectedMask & (1UL << i)) == 0)
                {
                    targets.Add(_items[i]);
                }
            }

            if (targets.Count == 0 && _goal != null)
            {
                targets.Add(_goal);
            }

            if (targets.Count == 0)
            {
                return;
            }

            try
            {
                await Tween.Run(1.4f, t =>
                {
                    float s = 1f + (0.22f * Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2f)));
                    foreach (var piece in targets)
                    {
                        piece.View.style.scale = new Scale(new Vector3(s, s, 1f));
                    }
                }, token);
            }
            finally
            {
                foreach (var piece in targets)
                {
                    piece.View.style.scale = new Scale(Vector3.one);
                }
            }
        }

        public async Awaitable Celebrate(CancellationToken token)
        {
            _mood = Mood.Happy;
            if (_goal != null && UseSprites && _art.GoalHappy != null)
            {
                SetSprite(_goal, _art.GoalHappy);
            }

            // Robot and turtle meet in the same cell: the turtle steps aside so both are visible.
            bool sameCell = _goal != null && _goalHome == RobotAnchor();
            var slide = Tween.Run(0.8f, t =>
            {
                if (sameCell)
                {
                    _goal.Anchor = _goalHome + new Vector2(0.5f * Mathf.Min(1f, t * 3f), 0f);
                    Place(_goal);
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

        private Piece AddSprite(Sprite sprite, Vector2 anchor, float height = 0f, float width = 0f)
        {
            var view = new VisualElement();
            view.AddToClassList("sprite");
            var piece = AddPiece(view, anchor, height, width);
            SetSprite(piece, sprite);
            return piece;
        }

        private Piece AddPiece(VisualElement view, Vector2 anchor, float height = 0f, float width = 0f)
        {
            view.pickingMode = PickingMode.Ignore;
            view.AddToClassList("piece");
            var shadow = new VisualElement { pickingMode = PickingMode.Ignore };
            shadow.AddToClassList("piece__shadow");
            var piece = new Piece { View = view, Shadow = shadow, Anchor = anchor, Height = height, Width = width };
            view.userData = piece;
            _decals.Add(shadow);
            _layer.Add(view);
            _pieces.Add(piece);
            return piece;
        }

        private static ObstacleLook? LookAt(LevelDto dto, int x, int y)
        {
            if (dto?.Grid?.Looks == null)
            {
                return null;
            }

            foreach (var look in dto.Grid.Looks)
            {
                if (look.X == x && look.Y == y)
                {
                    return look.Look;
                }
            }

            return null;
        }

        /// <summary>An obstacle in the level's chosen look, or the region's stable mix when none is given.</summary>
        private Piece AddLook(ObstacleLook? look, Vector2 anchor, (int x, int y) mixKey)
        {
            if (look == ObstacleLook.Log)
            {
                var view = new VisualElement();
                view.Add(new LogShape());
                var log = AddPiece(view, anchor, width: ObstacleWidth);
                log.Aspect = LogShape.Aspect;
                return log;
            }

            var sprite = look.HasValue ? _art.LookSprite(look.Value) : null;
            if (sprite == null)
            {
                sprite = _art.ObstacleFor(mixKey.x, mixKey.y);
            }

            return AddSprite(sprite, anchor, width: ObstacleWidth);
        }

        /// <summary>Scenery (v2) just outside the grid, e.g. the tree the turtle waits behind in level 4.</summary>
        private void AddScenery(LevelDto dto, Level level)
        {
            if (dto?.Scenery == null)
            {
                return;
            }

            int w = level.Grid.Width;
            int h = level.Grid.Height;
            foreach (var item in dto.Scenery)
            {
                // Sit just beyond the slab edge rather than a full cell away.
                float ax = item.X < 0 ? -0.35f : item.X >= w ? w + 0.35f : item.X + 0.5f;
                float ay = item.Y < 0 ? -0.05f : item.Y >= h ? h + 0.3f : item.Y + 0.5f;
                _hasNorthScenery |= item.Y < 0;
                var piece = AddLook(item.Look, new Vector2(ax, ay), ((int)item.X, (int)item.Y));
                piece.View.AddToClassList("piece--scenery");
            }
        }

        /// <summary>A few region props beside the slab so the board sits in the forest rather than on top of it.</summary>
        private void AddDecor(Level level)
        {
            int w = level.Grid.Width;
            int h = level.Grid.Height;
            var spots = new[]
            {
                new Vector2(-0.35f, h - 0.1f),
                new Vector2(w + 0.4f, h * 0.45f),
                new Vector2(-0.3f, h * 0.3f),
            };
            for (int i = 0; i < spots.Length; i++)
            {
                var sprite = _art.DecorAt(i);
                if (sprite == null)
                {
                    continue;
                }

                var decor = AddSprite(sprite, spots[i], width: DecorWidth);
                decor.View.AddToClassList("piece--decor");
            }
        }

        private void AddTrailDot(GridPosition p)
        {
            var dot = new VisualElement { pickingMode = PickingMode.Ignore };
            dot.AddToClassList("trail-dot");
            dot.userData = CellAnchor(p.X, p.Y);
            _trail.Add(dot);
            _decals.Add(dot);
            PlaceDot(dot);
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

            // Scenery behind the back row needs more room above the board.
            _proj = new ObliqueProjection(_level.Grid.Width, _level.Grid.Height, contentRect.size, headroom: _hasNorthScenery ? 1.05f : 0.75f);
            _ground.SetProjection(_proj);
            if (!_proj.IsValid)
            {
                return;
            }

            foreach (var piece in _pieces)
            {
                Place(piece);
            }

            foreach (var dot in _trail)
            {
                PlaceDot(dot);
            }

            PlaceRobot();
            SortByDepth();
        }

        private void PlaceRobot()
        {
            if (_robot == null)
            {
                return;
            }

            _robot.Anchor = RobotAnchor();
            _robot.Lift = _hop;
            if (UseSprites)
            {
                SetSprite(_robot, RobotSprite());
                // The side sprite faces west; mirror it for east.
                bool mirror = _mood == Mood.Normal && _facing == Direction.East;
                _robot.View.style.scale = new Scale(new Vector3((mirror ? -1f : 1f) * _squash, 1f, 1f));
                _robot.View.style.rotate = new Rotate(new UnityEngine.UIElements.Angle(_wobble, AngleUnit.Degree));
            }
            else
            {
                _robot.View.style.rotate = new Rotate(new UnityEngine.UIElements.Angle(_robotAngle + _wobble, AngleUnit.Degree));
            }

            Place(_robot);
            UpdateArrow();

            // Re-sort only when the robot crosses a quarter row, not every frame.
            int bucket = Mathf.FloorToInt(_robot.Anchor.y * 4f);
            if (bucket != _robotDepthBucket)
            {
                _robotDepthBucket = bucket;
                SortByDepth();
            }
        }

        private void Place(Piece piece)
        {
            if (!_proj.IsValid)
            {
                return;
            }

            float unit = _proj.Cell * _proj.ScaleAt(piece.Anchor.y);
            float w, h;
            if (piece.Width > 0f)
            {
                w = piece.Width * unit;
                h = w / piece.Aspect;
            }
            else
            {
                h = piece.Height * unit;
                w = h * piece.Aspect;
            }

            var foot = _proj.Project(piece.Anchor.x, piece.Anchor.y);
            piece.View.style.width = w;
            piece.View.style.height = h;
            piece.View.style.translate = new Translate(foot.x - (w * 0.5f), foot.y - (h * FeetFraction) - (piece.Lift * unit));

            // The contact shadow stays on the ground and shrinks while hopping.
            float sw = Mathf.Min(w * 0.75f, unit * 0.8f) * (1f - (piece.Lift * 0.8f));
            float sh = sw * 0.32f;
            piece.Shadow.style.width = sw;
            piece.Shadow.style.height = sh;
            piece.Shadow.style.translate = new Translate(foot.x - (sw * 0.5f), foot.y - (sh * 0.5f));
        }

        /// <summary>Lays the facing arrow on the ground one step ahead of the robot; hidden off the board and at celebrations.</summary>
        private void UpdateArrow()
        {
            if (!_proj.IsValid || _level == null)
            {
                return;
            }

            float dx = _facing == Direction.East ? 1f : _facing == Direction.West ? -1f : 0f;
            float dy = _facing == Direction.South ? 1f : _facing == Direction.North ? -1f : 0f;
            // Facing north the robot's back and head cover the ground ahead, so the arrow sits a little further out.
            float ahead = _facing == Direction.North ? 1.5f : 1.0f;
            var centre = new Vector2(_robotPos.x + 0.5f + (dx * ahead), _robotPos.y + 0.5f + (dy * ahead));
            bool onBoard = centre.x > 0.15f && centre.y > 0.15f && centre.x < _level.Grid.Width - 0.15f && centre.y < _level.Grid.Height - 0.15f;
            if (!onBoard || _mood != Mood.Normal)
            {
                _arrow.Set(false);
                return;
            }

            FacingArrow.Outline(centre, dx, dy, _arrowCells);
            for (int i = 0; i < FacingArrow.PointCount; i++)
            {
                _arrow.Points[i] = _proj.Project(_arrowCells[i].x, _arrowCells[i].y);
            }

            _arrow.Set(true);
        }

        private void PlaceDot(VisualElement dot)
        {
            if (!_proj.IsValid)
            {
                return;
            }

            var anchor = (Vector2)dot.userData;
            float size = _proj.Cell * _proj.ScaleAt(anchor.y) * 0.24f;
            var at = _proj.Project(anchor.x, anchor.y);
            dot.style.width = size;
            dot.style.height = size * 0.6f;
            dot.style.translate = new Translate(at.x - (size * 0.5f), at.y - (size * 0.3f));
        }

        /// <summary>Painter's order: farther pieces first; ties broken by Order (robot over an item in its cell).</summary>
        private void SortByDepth()
        {
            _layer.Sort((a, b) =>
            {
                var pa = (Piece)a.userData;
                var pb = (Piece)b.userData;
                int c = pa.Anchor.y.CompareTo(pb.Anchor.y);
                return c != 0 ? c : pa.Order.CompareTo(pb.Order);
            });
        }

        private Vector2 RobotAnchor() => new Vector2(_robotPos.x + 0.5f, _robotPos.y + 0.5f);

        private static Vector2 CellAnchor(int x, int y) => new Vector2(x + 0.5f, y + 0.5f);

        private Sprite RobotSprite()
        {
            if (_mood == Mood.Laughing && _actor.Laughing != null)
            {
                return _actor.Laughing;
            }

            if (_mood == Mood.Happy && _actor.Happy != null)
            {
                return _actor.Happy;
            }

            // Unity objects: use explicit null checks, not ??, so unassigned slots fall back correctly.
            var view = _facing == Direction.North ? _actor.Back
                : _facing == Direction.South ? _actor.Front
                : _actor.Side;
            return view != null ? view : _actor.Front;
        }

        private static void SetSprite(Piece piece, Sprite sprite)
        {
            if (sprite == null || piece.View.style.backgroundImage.value.sprite == sprite)
            {
                return;
            }

            piece.View.style.backgroundImage = new StyleBackground(sprite);
            piece.Aspect = sprite.rect.height > 0f ? sprite.rect.width / sprite.rect.height : 1f;
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
