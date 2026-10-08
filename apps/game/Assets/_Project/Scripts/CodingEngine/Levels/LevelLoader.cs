using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.Levels.Generated;
using Roboya.CodingEngine.World;

namespace Roboya.CodingEngine.Levels
{
    /// <summary>Turns level JSON (schema v1–v2) into engine objects. Structural JSON-schema checks run separately in CI.</summary>
    public static class LevelLoader
    {
        /// <summary>Oldest version still read; v2 only added the optional story block.</summary>
        public const int MinSchemaVersion = 1;

        public const int SupportedSchemaVersion = 2;

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
                    items.Add(new Item(i.Id, ToSchemaString(i.Kind), i.Color.HasValue ? ToSchemaString(i.Color.Value) : null, new GridPosition((int)i.X, (int)i.Y)));
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

            var cards = new List<CardType>();
            foreach (var c in dto.Cards.Palette)
            {
                cards.Add(ToCard(c));
            }

            var start = new RobotState(new GridPosition((int)dto.Robot.X, (int)dto.Robot.Y), ToDirection(dto.Robot.Facing), 0);
            return new Level(dto.Id, grid, start, items, new Goal(reach, mustCollect), cards, (int)dto.Cards.MaxProgramLength);
        }

        public static Program ToProgram(IReadOnlyList<CommandDto> commands)
        {
            return new Program(ToBlock(commands));
        }

        public static CardType ToCard(CardId card)
        {
            switch (card)
            {
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
