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
        private readonly int[] _dropIndexByCell;

        public Level(
            string id,
            Grid grid,
            RobotState start,
            IReadOnlyList<Item> items,
            Goal goal,
            IReadOnlyCollection<CardType> availableCards,
            int maxProgramLength,
            IReadOnlyList<DropPoint> drops = null,
            bool autoDrop = false)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
            Items = items ?? Array.Empty<Item>();
            Goal = goal ?? throw new ArgumentNullException(nameof(goal));
            AvailableCards = new HashSet<CardType>(availableCards ?? throw new ArgumentNullException(nameof(availableCards)));
            MaxProgramLength = maxProgramLength;
            Start = start;
            Drops = drops ?? Array.Empty<DropPoint>();
            AutoDrop = autoDrop;

            if (!grid.IsWalkable(start.Position))
            {
                throw new LevelValidationException("Robot start " + start.Position + " is not a floor cell.");
            }

            if (start.CollectedMask != 0 || (start.Honey & ~0xFUL) != 0)
            {
                throw new LevelValidationException("Robot must start with no collected items and nothing delivered.");
            }

            if (Drops.Count > RobotState.MaxDropPoints)
            {
                throw new LevelValidationException("A level may have at most " + RobotState.MaxDropPoints + " drop points.");
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

            _dropIndexByCell = new int[grid.CellCount];
            for (int i = 0; i < _dropIndexByCell.Length; i++)
            {
                _dropIndexByCell[i] = -1;
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

            ValidateDrops(grid, start);

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

            if (!goal.Reach.HasValue && goal.MustCollect.Count == 0 && goal.Deliver.Count == 0 && goal.Share == null)
            {
                throw new LevelValidationException("Goal must require reaching a cell, collecting items, delivering or sharing honey.");
            }

            ValidateHoneyGoal(goal);
        }

        public string Id { get; }

        public Grid Grid { get; }

        public RobotState Start { get; }

        public IReadOnlyList<Item> Items { get; }

        public Goal Goal { get; }

        public ISet<CardType> AvailableCards { get; }

        /// <summary>Number of card slots on the plan strip (repeat bodies count too).</summary>
        public int MaxProgramLength { get; }

        /// <summary>Where the bee can leave honey (hives and the table); empty outside Paylaşım Köyü.</summary>
        public IReadOnlyList<DropPoint> Drops { get; }

        /// <summary>Minik: a hive takes the honey it needs as soon as the bee enters its cell (PK-0001).</summary>
        public bool AutoDrop { get; }

        /// <summary>Index of the drop point on <paramref name="p"/>, or -1.</summary>
        public int DropIndexAt(GridPosition p) => Grid.Contains(p) ? _dropIndexByCell[Grid.Index(p)] : -1;

        /// <summary>Index of the item on <paramref name="p"/>, or -1.</summary>
        public int ItemIndexAt(GridPosition p) => Grid.Contains(p) ? _itemIndexByCell[Grid.Index(p)] : -1;

        private void ValidateDrops(Grid grid, RobotState start)
        {
            int honey = start.Basket;
            foreach (var item in Items)
            {
                honey += item.Kind == "honey" ? item.Amount : 0;
            }

            if (honey > RobotState.MaxCount)
            {
                throw new LevelValidationException("A level may hold at most " + RobotState.MaxCount + " honey in total.");
            }

            var dropIds = new HashSet<string>();
            for (int i = 0; i < Drops.Count; i++)
            {
                var drop = Drops[i];
                if (!dropIds.Add(drop.Id))
                {
                    throw new LevelValidationException("Duplicate drop point id '" + drop.Id + "'.");
                }

                if (!grid.IsWalkable(drop.Position))
                {
                    throw new LevelValidationException("Drop point '" + drop.Id + "' is not on a floor cell.");
                }

                if (drop.Position == start.Position)
                {
                    throw new LevelValidationException("Drop point '" + drop.Id + "' is on the robot start cell.");
                }

                if (ItemIndexAt(drop.Position) >= 0)
                {
                    throw new LevelValidationException("Drop point '" + drop.Id + "' shares a cell with an item.");
                }

                if (drop.Kind == DropPointKind.Hive && (drop.Need < 1 || drop.Need >= RobotState.MaxCount))
                {
                    throw new LevelValidationException("Hive '" + drop.Id + "' needs between 1 and " + (RobotState.MaxCount - 1) + " honey.");
                }

                int cell = grid.Index(drop.Position);
                if (_dropIndexByCell[cell] >= 0)
                {
                    throw new LevelValidationException("Two drop points share cell " + drop.Position + ".");
                }

                _dropIndexByCell[cell] = i;
            }
        }

        private void ValidateHoneyGoal(Goal goal)
        {
            foreach (var d in goal.Deliver)
            {
                if (d.DropIndex < 0 || d.DropIndex >= Drops.Count || d.Count < 1 || d.Count > RobotState.MaxCount)
                {
                    throw new LevelValidationException("Goal delivers to an unknown drop point or to an impossible count.");
                }
            }

            if (goal.CarryAtEnd.HasValue && (goal.CarryAtEnd.Value < 0 || goal.CarryAtEnd.Value > RobotState.MaxCount))
            {
                throw new LevelValidationException("carryAtEnd must be between 0 and " + RobotState.MaxCount + ".");
            }

            var share = goal.Share;
            if (share == null)
            {
                return;
            }

            if (share.Among.Count < 2)
            {
                throw new LevelValidationException("A share goal needs at least two hives.");
            }

            foreach (int hive in share.Among)
            {
                if (hive < 0 || hive >= Drops.Count || Drops[hive].Kind != DropPointKind.Hive)
                {
                    throw new LevelValidationException("A share goal can only share among hives.");
                }
            }

            if (share.LeftoverTo >= 0 && (share.LeftoverTo >= Drops.Count || Drops[share.LeftoverTo].Kind != DropPointKind.Table))
            {
                throw new LevelValidationException("Left-over honey can only go to a table.");
            }

            // A share goal is only fair once all the honey is in play: otherwise 2 + 2 could be "shared" with a flower left unpicked.
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Kind == "honey" && (goal.MustCollectMask & (1UL << i)) == 0)
                {
                    throw new LevelValidationException("A share goal must require collecting all the honey ('" + Items[i].Id + "' is not required).");
                }
            }
        }
    }
}
