using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day19
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "     |          ",
                    "     |  +--+    ",
                    "     A  |  C    ",
                    " F---|----E|--+ ",
                    "     |  |  |  D ",
                    "     +B-+  +--+ "
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/19/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = new Grid(input.Lines().ToList());

                var packet = new Packet(grid.FindEntrance(), Direction.Down);
                var letters = new List<char>();

                while (true)
                {
                    var result = grid.Move(packet);
                    if (!result.IsMoved)
                    {
                        break;
                    }

                    packet = result.Packet;

                    if (result.IsAtLetter)
                    {
                        letters.Add(result.Cell);
                    }
                }

                Console.WriteLine(string.Join("", letters));
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = new Grid(input.Lines().ToList());

                var packet = new Packet(grid.FindEntrance(), Direction.Down);
                var steps = 1; // to account for entrance move

                while (true)
                {
                    var result = grid.Move(packet);
                    if (!result.IsMoved)
                    {
                        break;
                    }

                    packet = result.Packet;
                    steps++;
                }

                Console.WriteLine(steps);
            }
        }

        private enum Direction { Up, Right, Down, Left }

        private static class DirectionUtil
        {
            public static Direction TurnCW(Direction dir) =>
                dir switch
                {
                    Direction.Up => Direction.Right,
                    Direction.Right => Direction.Down,
                    Direction.Down => Direction.Left,
                    Direction.Left => Direction.Up,

                    _ => throw new Exception("Unknown direction.")
                };

            public static Direction TurnCCW(Direction dir) =>
                dir switch
                {
                    Direction.Up => Direction.Left,
                    Direction.Left => Direction.Down,
                    Direction.Down => Direction.Right,
                    Direction.Right => Direction.Up,

                    _ => throw new Exception("Unknown direction.")
                };
        }

        private record Position(int Row, int Col)
        {
            public Position Move(Direction dir) =>
                dir switch
                {
                    Direction.Up => Up(),
                    Direction.Down => Down(),
                    Direction.Left => Left(),
                    Direction.Right => Right(),

                    _ => throw new Exception("Unknown direction.")
                };

            private Position Right() => this with { Col = Col + 1 };
            private Position Left() => this with { Col = Col - 1 };
            private Position Down() => this with { Row = Row + 1 };
            private Position Up() => this with { Row = Row - 1 };
        }

        private record Packet(Position Pos, Direction Dir);

        private sealed class Grid
        {
            private readonly IReadOnlyList<string> cells;

            public Grid(IReadOnlyList<string> cells)
            {
                this.cells = cells;
            }

            public int Rows => this.cells.Count;
            public int Cols => this.cells[0].Length;

            public char At(Position p) =>
                InBounds(p) ? this.cells[p.Row][p.Col] : ' ';

            public bool InBounds(Position p) =>
                0 <= p.Row && p.Row < this.Rows &&
                0 <= p.Col && p.Col < this.Cols;

            public Position FindEntrance()
            {
                for (var col = 0; col < this.Cols; col++)
                {
                    var position = new Position(0, col);
                    if (At(position) != ' ')
                    {
                        return position;
                    }
                }

                return default;
            }

            public MoveResult Move(Packet packet)
            {
                MoveResult TryMove(Packet packet, Direction dir)
                {
                    var nextPos = packet.Pos.Move(dir);
                    var nextCell = At(nextPos);

                    if (nextCell != ' ')
                    {
                        var nextPacket = packet with { Pos = nextPos, Dir = dir };
                        return MoveResult.Moved(nextPacket, nextCell);
                    }

                    return MoveResult.NotMoved(packet);
                }

                var result = TryMove(packet, packet.Dir);
                if (result.IsMoved)
                {
                    return result;
                }

                result = TryMove(packet, DirectionUtil.TurnCW(packet.Dir));
                if (result.IsMoved)
                {
                    return result;
                }

                return TryMove(packet, DirectionUtil.TurnCCW(packet.Dir));
            }
        }

        private record MoveResult(bool IsMoved, Packet Packet, char Cell)
        {
            public static MoveResult Moved(Packet packet, char cell = ' ') =>
                new MoveResult(IsMoved: true, Packet: packet, Cell: cell);

            public static MoveResult NotMoved(Packet packet) =>
                new MoveResult(IsMoved: false, Packet: packet, Cell: ' ');

            public bool IsAtLetter => char.IsLetter(this.Cell);
        }
    }
}
