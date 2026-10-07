using System;
using System.Collections.Generic;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.World;

namespace Roboya.Tests.CodingEngine
{
    /// <summary>
    /// Builds levels from ASCII maps: '.' floor, '#' blocked, 'R' robot start (facing given), 'G' goal,
    /// 'a'..'z' items (id = letter, all must be collected unless <paramref name="collectAll"/> is false).
    /// </summary>
    internal static class TestLevels
    {
        public static readonly CardType[] Moves = { CardType.Forward, CardType.TurnLeft, CardType.TurnRight };

        public static Level Build(
            string[] map,
            Direction facing = Direction.North,
            CardType[] cards = null,
            int maxLength = 20,
            bool collectAll = true,
            bool requireGoalCell = true)
        {
            var rows = new string[map.Length];
            GridPosition? start = null;
            GridPosition? goal = null;
            var items = new List<Item>();
            for (int y = 0; y < map.Length; y++)
            {
                var chars = map[y].ToCharArray();
                for (int x = 0; x < chars.Length; x++)
                {
                    char c = chars[x];
                    if (c == 'R')
                    {
                        start = new GridPosition(x, y);
                        chars[x] = '.';
                    }
                    else if (c == 'G')
                    {
                        goal = new GridPosition(x, y);
                        chars[x] = '.';
                    }
                    else if (c >= 'a' && c <= 'z')
                    {
                        items.Add(new Item(c.ToString(), "flower", c == 'y' ? "yellow" : "red", new GridPosition(x, y)));
                        chars[x] = '.';
                    }
                }

                rows[y] = new string(chars);
            }

            if (!start.HasValue)
            {
                throw new ArgumentException("Map needs an 'R'.");
            }

            var must = new List<int>();
            if (collectAll)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    must.Add(i);
                }
            }

            return new Level(
                "test",
                Grid.FromRows(rows),
                new RobotState(start.Value, facing, 0),
                items,
                new Goal(requireGoalCell ? goal : null, must),
                cards ?? Moves,
                maxLength);
        }

        public static Program Prog(params CardType[] cards)
        {
            var commands = new Command[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                commands[i] = MoveCommand.For(cards[i]);
            }

            return new Program(commands);
        }

        public const CardType F = CardType.Forward;
        public const CardType B = CardType.Backward;
        public const CardType L = CardType.TurnLeft;
        public const CardType Rt = CardType.TurnRight;
    }
}
