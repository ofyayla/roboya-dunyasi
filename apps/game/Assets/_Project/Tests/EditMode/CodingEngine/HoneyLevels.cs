using System;
using System.Collections.Generic;
using System.Linq;
using Roboya.CodingEngine.Commands;
using Roboya.CodingEngine.World;

namespace Roboya.Tests.CodingEngine
{
    /// <summary>
    /// Builds Paylaşım Köyü levels from ASCII maps: '.' floor, '#' blocked, 'R' bee start, 'G' reach cell, '1'..'9' a honey
    /// item with that amount, 'f' a flower, 'A'..'D' hives (need from <c>needs</c>, default 2) and 'T' the shared table.
    /// Drop points are numbered in label order (A, B, C, D, T); items in reading order.
    /// </summary>
    internal static class HoneyLevels
    {
        public static readonly CardType[] MovesAndDrop = { CardType.Forward, CardType.Backward, CardType.TurnLeft, CardType.TurnRight, CardType.Drop };

        public static Level Build(
            string[] map,
            Direction facing = Direction.East,
            int[] needs = null,
            bool autoDrop = false,
            int carrying = 0,
            CardType[] cards = null,
            int maxLength = 20,
            (char Hive, int Count)[] deliver = null,
            string shareAmong = null,
            char? leftover = null,
            int? carryAtEnd = null,
            bool collectAll = true,
            bool reachG = false)
        {
            if (map.Length == 1)
            {
                // A grid needs two rows; a wall row keeps a one-row corridor honest.
                map = new[] { map[0], new string('#', map[0].Length) };
            }

            var rows = new string[map.Length];
            GridPosition? start = null;
            GridPosition? goalCell = null;
            var items = new List<Item>();
            var labelled = new SortedDictionary<char, GridPosition>();
            for (int y = 0; y < map.Length; y++)
            {
                var chars = map[y].ToCharArray();
                for (int x = 0; x < chars.Length; x++)
                {
                    char c = chars[x];
                    var here = new GridPosition(x, y);
                    if (c == 'R')
                    {
                        start = here;
                    }
                    else if (c == 'G')
                    {
                        goalCell = here;
                    }
                    else if (c >= '1' && c <= '9')
                    {
                        items.Add(new Item("h" + items.Count, "honey", null, here, c - '0'));
                    }
                    else if (c == 'f')
                    {
                        items.Add(new Item("f" + items.Count, "flower", "red", here));
                    }
                    else if ((c >= 'A' && c <= 'D') || c == 'T')
                    {
                        labelled[c] = here;
                    }

                    if (c != '#')
                    {
                        chars[x] = '.';
                    }
                }

                rows[y] = new string(chars);
            }

            var drops = new List<DropPoint>();
            foreach (var pair in labelled)
            {
                int need = pair.Key == 'T' ? 0 : (needs != null && pair.Key - 'A' < needs.Length ? needs[pair.Key - 'A'] : 2);
                drops.Add(new DropPoint(pair.Key.ToString(), pair.Key == 'T' ? DropPointKind.Table : DropPointKind.Hive, pair.Value, need));
            }

            int Idx(char label) => drops.FindIndex(d => d.Id == label.ToString());
            var must = collectAll ? Enumerable.Range(0, items.Count).ToList() : new List<int>();
            var deliveries = (deliver ?? Array.Empty<(char, int)>()).Select(d => new Delivery(Idx(d.Item1), d.Item2)).ToList();
            var share = shareAmong != null ? new ShareGoal(shareAmong.Select(Idx).ToList(), leftover.HasValue ? Idx(leftover.Value) : -1) : null;
            GridPosition? reach = reachG ? goalCell : null;
            if (reach == null && must.Count == 0 && deliveries.Count == 0 && share == null)
            {
                // Rule tests do not care about the goal, but a level needs one.
                reach = start;
            }

            var goal = new Goal(reach, must, deliveries, share, carryAtEnd);
            var begin = new RobotState(start.Value, facing, 0).WithBasket(carrying);
            return new Level("village", Grid.FromRows(rows), begin, items, goal, cards ?? MovesAndDrop, maxLength, drops, autoDrop);
        }

        public static int Idx(Level level, char label) => level.Drops.ToList().FindIndex(d => d.Id == label.ToString());

        public static Program Prog(params CardType[] cards)
        {
            var commands = new Command[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                commands[i] = cards[i] == CardType.Drop ? (Command)DropCommand.Instance : MoveCommand.For(cards[i]);
            }

            return new Program(commands);
        }

        public const CardType F = CardType.Forward;
        public const CardType D = CardType.Drop;
        public const CardType Rt = CardType.TurnRight;
    }
}
