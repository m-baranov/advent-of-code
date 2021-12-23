using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdventOfCode2021
{
    static class Day23
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "#############",
                    "#...........#",
                    "###B#C#B#D###",
                    "  #A#D#C#A#  ",
                    "  #########  "
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/23/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var burrow = Burrow.Parse(input.Lines());

                var cost = Burrow.Solve(burrow);
                Console.WriteLine(cost);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var lines = input.Lines().ToList();
                lines.InsertRange(3, new[]
                {
                    "#D#C#B#A#",
                    "#D#B#A#C#"
                });

                var burrow = Burrow.Parse(lines);

                var cost = Burrow.Solve(burrow);
                Console.WriteLine(cost);
            }
        }

        private class Burrow
        {
            public static Burrow Parse(IEnumerable<string> lines)
            {
                var roomLines = lines
                    .Skip(2)
                    .Select(l => l.Replace("#", string.Empty).Replace(" ", string.Empty))
                    .ToList();

                roomLines.RemoveAt(roomLines.Count - 1); // skip last line

                var roomCount = roomLines[0].Length;
                var roomSize = roomLines.Count;

                var cells = new List<char>();
                for (var room = 0; room < roomCount; room++)
                {
                    foreach (var roomLine in roomLines)
                    {
                        cells.Add(roomLine[room]);
                    }
                }

                return new Burrow(Hallways.Empty, new Rooms(cells, roomSize));
            }

            public static long Solve(Burrow burrow) =>
                Solve(burrow, currentCost: 0, bestCost: long.MaxValue, level: 0);

            private static long Solve(Burrow burrow, long currentCost, long bestCost, long level)
            {
                if (burrow.Done())
                {
                    return Math.Min(currentCost, bestCost);
                }

                foreach (var move in burrow.PossibleMoves())
                {
                    var nextCost = currentCost + burrow.MoveCost(move);
                    if (nextCost < bestCost)
                    {
                        var nextBurrow = burrow.Move(move);
                        bestCost = Solve(nextBurrow, nextCost, bestCost, level + 1);
                    }
                }

                return bestCost;
            }

            private readonly Hallways hallways;
            private readonly Rooms rooms;

            public Burrow(Hallways hallways, Rooms rooms)
            {
                this.hallways = hallways;
                this.rooms = rooms;
            }

            public override string ToString()
            {
                var builder = new StringBuilder();

                char h(int hall) => hallways.At(hall);
                char r(int room, int cell) => rooms.At(room, cell);

                builder.AppendLine("#############");
                builder.AppendLine($"#{h(0)}{h(1)}.{h(2)}.{h(3)}.{h(4)}.{h(5)}{h(6)}#");

                for (var cell = 0; cell < rooms.RoomSize; cell++) 
                {
                    builder.Append(cell == 0 ? "##" : "  ");

                    for (var room = 0; room < rooms.Count; room++)
                    {
                        builder.Append($"#{r(room, cell)}");
                    }

                    builder.AppendLine(cell == 0 ? "###" : "#  ");
                }

                builder.AppendLine("  #########  ");
                builder.AppendLine();

                return builder.ToString();
            }

            public bool Done()
            {
                for (var room = 0; room < rooms.Count; room++)
                {
                    if (!rooms.Done(room))
                    {
                        return false;
                    }
                }
                return true;
            }

            public Burrow Move(Move move)
            {
                if (move.Exit)
                {
                    var newRooms = rooms.Set(move.Room, move.Cell, Cell.Blank);
                    var newHallways = hallways.Set(move.Hallway, move.Ch);
                    return new Burrow(newHallways, newRooms);
                }
                else
                {
                    var newHallways = hallways.Set(move.Hallway, Cell.Blank);
                    var newRooms = rooms.Set(move.Room, move.Cell, move.Ch);
                    return new Burrow(newHallways, newRooms);
                }
            }

            public IEnumerable<Move> PossibleMoves()
            {
                var enters = PossibleEnters().ToList();
                if (enters.Count > 0)
                {
                    return enters;
                }

                return PossibleExits();
            }

            private IEnumerable<Move> PossibleEnters()
            {
                for (var hallway = 0; hallway < hallways.Count; hallway++)
                {
                    var ch = hallways.At(hallway);
                    if (ch == Cell.Blank)
                    {
                        continue;
                    }

                    var targetRoom = Cell.TargetRoom(ch);
                    if (!CanGoBetween(targetRoom, hallway, excludeHallway: true))
                    {
                        continue;
                    }

                    if (!rooms.TryEnter(targetRoom, ch, out var cell))
                    {
                        continue;
                    }

                    yield return new Move(targetRoom, cell, hallway, ch, exit: false);
                }
            }

            private IEnumerable<Move> PossibleExits()
            {
                for (var room = 0; room < rooms.Count; room++)
                {
                    if (!rooms.TryExit(room, out var cell))
                    {
                        continue;
                    }

                    var ch = rooms.At(room, cell);

                    for (var hallway = 0; hallway < hallways.Count; hallway++)
                    {
                        if (!CanGoBetween(room, hallway))
                        {
                            continue;
                        }

                        yield return new Move(room, cell, hallway, ch, exit: true);
                    }
                }
            }

            private int MoveCost(Move move) => Steps(move) * Cell.CostPerStep(move.Ch);

            public int Steps(Move move) => Steps(move.Room, move.Cell, move.Hallway);

            public int Steps(int room, int cell, int hallway)
            {
                var steps = Math.Abs(HallwayPos(hallway) - RoomPos(room)) + (cell + 1);
                if (hallway == 0 || hallway == hallways.Count - 1)
                {
                    return steps - 1;
                }
                return steps;
            }

            public bool CanGoBetween(int room, int hallway, bool excludeHallway = false)
            {
                bool allHallwaysFree(int posStart, int posEnd, int posToIgnore)
                {
                    for (var pos = posStart; pos <= posEnd; pos++)
                    {
                        if (pos == posToIgnore || pos % 2 == 1)
                        {
                            continue;
                        }

                        var hallway = pos / 2;
                        if (hallways.At(hallway) != Cell.Blank)
                        {
                            return false;
                        }
                    }
                    return true;
                }

                var posStart = RoomPos(room);
                var posEnd = HallwayPos(hallway);

                var posToIgnore = excludeHallway ? posEnd : -1;

                return posStart < posEnd
                    ? allHallwaysFree(posStart, posEnd, posToIgnore)
                    : allHallwaysFree(posEnd, posStart, posToIgnore);
            }

            private int RoomPos(int room) => (room + 1) * 2 + 1;

            private int HallwayPos(int hallway) => hallway * 2;
        }

        private class Hallways
        {
            public static readonly Hallways Empty = 
                new Hallways(Enumerable.Range(0, 7).Select(_ => Cell.Blank).ToArray());

            private readonly IReadOnlyList<char> cells;

            public Hallways(IReadOnlyList<char> cells)
            {
                this.cells = cells;
            }

            public int Count => cells.Count;

            public char At(int hall) => cells[hall];

            public Hallways Set(int hall, char ch)
            {
                var clone = this.cells.ToArray();
                clone[hall] = ch;
                return new Hallways(clone);
            }
        }

        private class Rooms
        {
            private readonly IReadOnlyList<char> cells;
            private readonly int roomSize;

            public Rooms(IReadOnlyList<char> cells, int roomSize)
            {
                this.cells = cells;
                this.roomSize = roomSize;
            }

            public int Count => cells.Count / this.roomSize;
            public int RoomSize => this.roomSize; 

            public char At(int room, int cell) => cells[room * roomSize + cell];

            public Rooms Set(int room, int cell, char ch)
            {
                var clone = this.cells.ToArray();
                clone[room * roomSize + cell] = ch;
                return new Rooms(clone, roomSize);
            }

            public bool Done(int room)
            {
                var target = Cell.TargetChar(room);

                for (var cell = 0; cell < RoomSize; cell++)
                {
                    if (At(room, cell) != target)
                    {
                        return false;
                    }
                }
                return true;
            }

            public bool TryEnter(int room, char ch, out int cell)
            {
                var target = Cell.TargetChar(room);

                cell = default;

                if (ch != target)
                {
                    return false;
                }

                var c = 0;
                while (c < RoomSize && At(room, c) == Cell.Blank)
                {
                    c++;
                }

                if (c == 0)
                {
                    return false;
                }

                cell = c - 1;

                while (c < RoomSize && At(room, c) == target)
                {
                    c++;
                }

                return c >= RoomSize;
            }

            public bool TryExit(int room, out int cell)
            {
                var target = Cell.TargetChar(room);

                cell = default;
                
                var c = 0;
                while (c < RoomSize && At(room, c) == Cell.Blank)
                {
                    c++;
                }

                if (c >= RoomSize)
                {
                    return false;
                }

                cell = c;

                while (c < RoomSize && At(room, c) == target)
                {
                    c++;
                }

                return c < RoomSize;
            }
        }

        private static class Cell
        {
            public const char Blank = '.';

            public static int TargetRoom(char ch) => (int)(ch - 'A');

            public static char TargetChar(int room) => (char)('A' + room);

            private static int[] EnergyCosts = new[] { 1, 10, 100, 1000 };

            public static int CostPerStep(char ch) => EnergyCosts[TargetRoom(ch)];
        }

        private class Move
        {
            public Move(int room, int cell, int hallway, char ch, bool exit)
            {
                Room = room;
                Cell = cell;
                Hallway = hallway;
                Ch = ch;
                Exit = exit;
            }

            public int Room { get; }
            public int Cell { get; }
            public int Hallway { get; }
            public char Ch { get; }
            public bool Exit { get; }

            public override string ToString()
            {
                return Exit 
                    ? $"Exit: from room {Room}-{Cell} to hallway {Hallway}" 
                    : $"Enter: from hallway {Hallway} to room {Room}-{Cell}";
            }
        }
    }
}
