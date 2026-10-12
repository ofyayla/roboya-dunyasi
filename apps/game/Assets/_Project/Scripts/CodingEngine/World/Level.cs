using System;
using System.Collections.Generic;
using Roboya.CodingEngine.Commands;

namespace Roboya.CodingEngine.World
{
    /// <summary>A validated, immutable level ready for execution and solving. Built from level JSON by <c>LevelLoader</c>.</summary>
    public sealed class Level
    {
        /// <summary>Collected items are tracked in a 64-bit mask.</summary>
        public const int MaxItems = 64;

        private readonly int[] _itemIndexByCell;

        public Level(
            string id,
            Grid grid,
            RobotState start,
            IReadOnlyList<Item> items,
            Goal goal,
            IReadOnlyCollection<CardType> availableCards,
            int maxProgramLength)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
            Items = items ?? Array.Empty<Item>();
            Goal = goal ?? throw new ArgumentNullException(nameof(goal));
            AvailableCards = new HashSet<CardType>(availableCards ?? throw new ArgumentNullException(nameof(availableCards)));
            MaxProgramLength = maxProgramLength;
            Start = start;

            if (!grid.IsWalkable(start.Position))
            {
                throw new LevelValidationException("Robot start " + start.Position + " is not a floor cell.");
            }

            if (start.CollectedMask != 0)
            {
                throw new LevelValidationException("Robot must start with no collected items.");
            }

            if (Items.Count > MaxItems)
            {
                throw new LevelValidationException("A level may contain at most " + MaxItems + " items.");
            }

            if (maxProgramLength < 1)
            {
                throw new LevelValidationException("maxProgramLength must be at least 1.");
            }

            if (AvailableCards.Count == 0)
            {
                throw new LevelValidationException("At least one card must be available.");
            }

            _itemIndexByCell = new int[grid.CellCount];
            for (int i = 0; i < _itemIndexByCell.Length; i++)
            {
                _itemIndexByCell[i] = -1;
            }

            var ids = new HashSet<string>();
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                if (!ids.Add(item.Id))
                {
                    throw new LevelValidationException("Duplicate item id '" + item.Id + "'.");
                }

                if (!grid.IsWalkable(item.Position))
                {
                    throw new LevelValidationException("Item '" + item.Id + "' is not on a floor cell.");
                }

                if (item.Position == start.Position)
                {
                    throw new LevelValidationException("Item '" + item.Id + "' is on the robot start cell.");
                }

                int cell = grid.Index(item.Position);
                if (_itemIndexByCell[cell] >= 0)
                {
                    throw new LevelValidationException("Two items share cell " + item.Position + ".");
                }

                _itemIndexByCell[cell] = i;
            }

            if (goal.Reach.HasValue && !grid.IsWalkable(goal.Reach.Value))
            {
                throw new LevelValidationException("Goal " + goal.Reach.Value + " is not a floor cell.");
            }

            foreach (int index in goal.MustCollect)
            {
                if (index < 0 || index >= Items.Count)
                {
                    throw new LevelValidationException("Goal references unknown item index " + index + ".");
                }
            }

            if (!goal.Reach.HasValue && goal.MustCollect.Count == 0)
            {
                throw new LevelValidationException("Goal must require reaching a cell or collecting items.");
            }
        }

        public string Id { get; }

        public Grid Grid { get; }

        public RobotState Start { get; }

        public IReadOnlyList<Item> Items { get; }

        public Goal Goal { get; }

        public ISet<CardType> AvailableCards { get; }

        /// <summary>Number of card slots on the plan strip (repeat bodies count too).</summary>
        public int MaxProgramLength { get; }

        /// <summary>Index of the item on <paramref name="p"/>, or -1.</summary>
        public int ItemIndexAt(GridPosition p) => Grid.Contains(p) ? _itemIndexByCell[Grid.Index(p)] : -1;
    }
}
