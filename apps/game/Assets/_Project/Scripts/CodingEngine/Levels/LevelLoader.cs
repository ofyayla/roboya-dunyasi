using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.World;

namespace Roboya.CodingEngine.Levels
{
    /// <summary>Turns level JSON (schema v1–v3) into engine objects. Structural JSON-schema checks run separately in CI.</summary>
    public static class LevelLoader
    {
        /// <summary>Oldest version still read; v2 only added the optional story block, v3 the honey fields (ADR 0030).</summary>
        public const int MinSchemaVersion = 1;

        public const int SupportedSchemaVersion = 3;

        public static LevelDto Parse(string json)
        {
            LevelDto dto;
            try
            {
                dto = LevelDto.FromJson(json);
            }
            catch (JsonException e)
            {
                throw new LevelValidationException("Level JSON is malformed: " + e.Message);
            }

            if (dto == null)
            {
                throw new LevelValidationException("Level JSON is empty.");
            }

            if (dto.SchemaVersion < MinSchemaVersion || dto.SchemaVersion > SupportedSchemaVersion)
            {
                throw new LevelValidationException("Unsupported schemaVersion " + dto.SchemaVersion + ".");
            }

            return dto;
        }

        public static Level Load(string json) => ToLevel(Parse(json));

        public static Level ToLevel(LevelDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (dto.Grid?.Rows == null || dto.Robot == null || dto.Goal == null || dto.Cards?.Palette == null)
            {
                throw new LevelValidationException("Level '" + dto.Id + "' is missing grid, robot, goal or cards.");
            }

            Grid grid;
            try
            {
                grid = Grid.FromRows(dto.Grid.Rows.ToArray());
            }
            catch (ArgumentException e)
            {
                throw new LevelValidationException("Level '" + dto.Id + "' grid is invalid: " + e.Message);
            }

            var items = new List<Item>();
            var indexById = new Dictionary<string, int>();
            if (dto.Items != null)
            {
                foreach (var i in dto.Items)
                {
                    indexById[i.Id] = items.Count;
                    items.Add(new Item(i.Id, ToSchemaString(i.Kind), i.Color.HasValue ? ToSchemaString(i.Color.Value) : null, new GridPosition((int)i.X, (int)i.Y), (int)(i.Amount ?? 1)));
                }
            }

            var mustCollect = new List<int>();
            if (dto.Goal.Collect != null)
            {
                foreach (var id in dto.Goal.Collect)
                {
                    if (!indexById.TryGetValue(id, out int index))
                    {
                        throw new LevelValidationException("Goal collects unknown item '" + id + "'.");
                    }

                    mustCollect.Add(index);
                }
            }

            GridPosition? reach = null;
            if (dto.Goal.Reach != null)
            {
                reach = new GridPosition((int)dto.Goal.Reach.X, (int)dto.Goal.Reach.Y);
            }

            var drops = new List<DropPoint>();
            var dropIndexById = new Dictionary<string, int>();
            if (dto.Drops != null)
            {
                foreach (var d in dto.Drops)
                {
                    dropIndexById[d.Id] = drops.Count;
                    drops.Add(new DropPoint(d.Id, d.Kind == Generated.DropKind.Table ? DropPointKind.Table : DropPointKind.Hive, new GridPosition((int)d.X, (int)d.Y), (int)(d.Need ?? 0)));
                }
            }

            var goal = new Goal(reach, mustCollect, ToDeliveries(dto.Goal.Deliver, dropIndexById), ToShare(dto.Goal.Share, dropIndexById), dto.Goal.CarryAtEnd.HasValue ? (int)dto.Goal.CarryAtEnd.Value : (int?)null);

            var cards = new List<CardType>();
            foreach (var c in dto.Cards.Palette)
            {
                cards.Add(ToCard(c));
            }

            var start = new RobotState(new GridPosition((int)dto.Robot.X, (int)dto.Robot.Y), ToDirection(dto.Robot.Facing), 0)
                .WithBasket((int)(dto.Robot.Carrying ?? 0));
            return new Level(dto.Id, grid, start, items, goal, cards, (int)dto.Cards.MaxProgramLength, drops, dto.Options?.AutoDrop == true);
        }

        private static IReadOnlyList<Delivery> ToDeliveries(List<DeliverDto> deliver, Dictionary<string, int> dropIndexById)
        {
            var result = new List<Delivery>();
            if (deliver != null)
            {
                foreach (var d in deliver)
                {
                    result.Add(new Delivery(DropIndex(d.To, dropIndexById), (int)d.Count));
                }
            }

            return result;
        }

        private static ShareGoal ToShare(ShareDto share, Dictionary<string, int> dropIndexById)
        {
            if (share == null)
            {
                return null;
            }

            var among = new List<int>();
            foreach (var id in share.Among)
            {
                among.Add(DropIndex(id, dropIndexById));
            }

            return new ShareGoal(among, share.LeftoverTo != null ? DropIndex(share.LeftoverTo, dropIndexById) : -1);
        }

        private static int DropIndex(string id, Dictionary<string, int> dropIndexById)
        {
            if (!dropIndexById.TryGetValue(id, out int index))
            {
                throw new LevelValidationException("Goal refers to unknown drop point '" + id + "'.");
            }

            return index;
        }

        public static Program ToProgram(IReadOnlyList<CommandDto> commands)
        {
            return new Program(ToBlock(commands));
        }

        public static CardType ToCard(CardId card)
        {
            switch (card)
            {
                case CardId.Drop: return CardType.Drop;
                case CardId.Forward: return CardType.Forward;
                case CardId.Backward: return CardType.Backward;
                case CardId.TurnLeft: return CardType.TurnLeft;
                case CardId.TurnRight: return CardType.TurnRight;
                case CardId.Repeat: return CardType.Repeat;
                case CardId.If: return CardType.If;
                case CardId.Call: return CardType.Call;
                default: return CardType.Action;
            }
        }

        public static Direction ToDirection(Facing facing)
        {
            switch (facing)
            {
                case Facing.North: return Direction.North;
                case Facing.East: return Direction.East;
                case Facing.South: return Direction.South;
                default: return Direction.West;
            }
        }

        private static IReadOnlyList<Command> ToBlock(IReadOnlyList<CommandDto> commands)
        {
            if (commands == null)
            {
                return Array.Empty<Command>();
            }

            var block = new Command[commands.Count];
            for (int i = 0; i < block.Length; i++)
            {
                block[i] = ToCommand(commands[i]);
            }

            return block;
        }

        private static Command ToCommand(CommandDto c)
        {
            switch (c.Op)
            {
                case CardId.Forward:
                case CardId.Backward:
                case CardId.TurnLeft:
                case CardId.TurnRight:
                    return MoveCommand.For(ToCard(c.Op));
                case CardId.Repeat:
                    return new RepeatCommand((int)(c.Times ?? 0), ToBlock(c.Body));
                case CardId.If:
                    return new IfCommand(ToCondition(c.Condition), ToBlock(c.Then), ToBlock(c.Else));
                case CardId.Call:
                    return new CallCommand(c.Name);
                case CardId.Drop:
                    return DropCommand.Instance;
                default:
                    return new ActionCommand(c.Id);
            }
        }

        private static Condition ToCondition(ConditionDto c)
        {
            if (c == null)
            {
                throw new LevelValidationException("'if' command needs a condition.");
            }

            switch (c.Type)
            {
                case TypeEnum.PathBlocked: return PathBlockedAhead.Instance;
                case TypeEnum.OnColor: return new OnItemColor(c.Color);
                default: return new Not(ToCondition(c.Inner));
            }
        }

        private static string ToSchemaString(Kind kind)
        {
            switch (kind)
            {
                case Kind.Flower: return "flower";
                case Kind.Honey: return "honey";
                case Kind.Gear: return "gear";
                case Kind.Star: return "star";
                case Kind.Fruit: return "fruit";
                default: return "ship-part";
            }
        }

        private static string ToSchemaString(Color color)
        {
            switch (color)
            {
                case Color.Red: return "red";
                case Color.Yellow: return "yellow";
                case Color.Blue: return "blue";
                case Color.Green: return "green";
                case Color.Purple: return "purple";
                default: return "orange";
            }
        }
    }
}
