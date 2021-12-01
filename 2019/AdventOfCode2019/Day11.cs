using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day11
    {
        public static readonly IInput SampleInput =
            Input.Literal();

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/11/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();
                var computer = Day09.Computer.Of(program);

                var board = new Board();
                var robot = new Robot(Position.Origin, Direction.Up);

                Simulation.Run(computer, robot, board);

                Console.WriteLine(board.Count());
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();
                var computer = Day09.Computer.Of(program);

                var board = new Board();
                var robot = new Robot(Position.Origin, Direction.Up);

                board.Set(robot.Position, Color.White);

                Simulation.Run(computer, robot, board);

                Display(board.ToBitmap());
            }

            private void Display(IReadOnlyList<IReadOnlyList<Color>> bitmap)
            {
                foreach (var row in bitmap)
                {
                    foreach (var color in row.Reverse()) // <-- 
                    {
                        Console.Write(color == Color.Black ? "██" : "  ");
                    }
                    Console.WriteLine();
                }
            }
        }

        private enum Direction { Up, Left, Down, Right }

        private enum Rotation { Left, Right }

        private static class DirectionUtil
        {
            public static Direction Rotate(Direction dir, Rotation rotation)
            {
                return rotation == Rotation.Left ? RotateLeft(dir) : RotateRight(dir);
            } 

            public static Direction RotateRight(Direction dir)
            {
                if (dir == Direction.Up) return Direction.Left;
                if (dir == Direction.Left) return Direction.Down;
                if (dir == Direction.Down) return Direction.Right;
                return Direction.Up;
            }

            public static Direction RotateLeft(Direction dir)
            {
                if (dir == Direction.Up) return Direction.Right;
                if (dir == Direction.Left) return Direction.Up;
                if (dir == Direction.Down) return Direction.Left;
                return Direction.Down;
            }
        }

        private class Position
        {
            public static readonly Position Origin = new Position(0, 0);

            public Position(int row, int col)
            {
                Row = row;
                Col = col;
            }

            public int Row { get; }
            public int Col { get; }

            public override bool Equals(object obj) =>
                obj is Position pos ? Row == pos.Row && Col == pos.Col : false;

            public override int GetHashCode() =>
                HashCode.Combine(Row, Col);

            public Position Move(Direction dir)
            {
                if (dir == Direction.Up) return new Position(Row - 1, Col);
                if (dir == Direction.Left) return new Position(Row, Col - 1);
                if (dir == Direction.Down) return new Position(Row + 1, Col);
                return new Position(Row, Col + 1);
            }
        }

        private class Robot
        {
            public Robot(Position position, Direction direction)
            {
                Position = position;
                Direction = direction;
            }

            public Position Position { get; }
            public Direction Direction { get; }

            public Robot RotateAndMove(Rotation rotation)
            {
                var nextDirection = DirectionUtil.Rotate(Direction, rotation);
                var nextPosition = Position.Move(nextDirection);
                return new Robot(nextPosition, nextDirection);
            }
        }

        private enum Color { Black, White }

        private class Board
        {
            private Dictionary<Position, Color> cells;

            public Board()
            {
                cells = new Dictionary<Position, Color>();
            }

            public int Count() => cells.Count;

            public Color Get(Position pos)
            {
                if (cells.TryGetValue(pos, out var color))
                {
                    return color;
                }
                else
                {
                    return Color.Black;
                }
            }

            public void Set(Position pos, Color color)
            {
                cells[pos] = color;
            }

            public IReadOnlyList<IReadOnlyList<Color>> ToBitmap()
            {
                var minRow = cells.Keys.Select(pos => pos.Row).Min();
                var maxRow = cells.Keys.Select(pos => pos.Row).Max();

                var minCol = cells.Keys.Select(pos => pos.Col).Min();
                var maxCol = cells.Keys.Select(pos => pos.Col).Max();

                var lines = new List<IReadOnlyList<Color>>();
                
                for (var row = minRow; row <= maxRow; row++)
                {
                    var line = new List<Color>();
                    for (var col = minCol; col <= maxCol; col++)
                    {
                        line.Add(Get(new Position(row, col)));
                    }
                    lines.Add(line);
                }

                return lines;
            }
        }

        private static class Simulation
        {
            public static void Run(Day09.Computer computer, Robot robot, Board board)
            {
                while (true)
                {
                    computer.Input.Enter(board.Get(robot.Position) == Color.Black ? 0 : 1);

                    var result = computer.Execute();
                    if (result is Day09.Computer.Result.Halt)
                    {
                        break;
                    }
                    else if (result is Day09.Computer.Result.WaitingForInput)
                    {
                        var output = computer.Output.Values().TakeLast(2).ToArray();

                        board.Set(robot.Position, output[0] == 0 ? Color.Black : Color.White);
                        robot = robot.RotateAndMove(output[1] == 0 ? Rotation.Left : Rotation.Right);
                    }
                    else
                    {
                        Console.WriteLine("ERROR: unexpected execution result.");
                        break;
                    }
                }
            }
        }
    }
}
