using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day09
    {
        public static class Inputs
        {
            public static readonly IInput Sample1 =
                Input.Literal(
                    "R 4",
                    "U 4",
                    "L 3",
                    "D 1",
                    "R 4",
                    "D 1",
                    "L 5",
                    "R 2"
                );

            public static readonly IInput Sample2 =
                Input.Literal(
                    "R 5",
                    "U 8",
                    "L 8",
                    "D 3",
                    "R 17",
                    "D 10",
                    "L 25",
                    "U 20"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/9/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var moves = Move.Parse(input.Lines());

                var rope = Rope.Initial(length: 2);

                var tailPoints = new HashSet<Point>() { rope.Tail };

                foreach (var move in moves)
                {
                    for (var i = 0; i < move.Steps; i++)
                    {
                        rope = rope.Move(move.Direction);
                        tailPoints.Add(rope.Tail);
                    }
                }

                Console.WriteLine(tailPoints.Count);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var moves = Move.Parse(input.Lines());

                var rope = Rope.Initial(length: 10);

                var tailPoints = new HashSet<Point>() { rope.Tail };

                foreach (var move in moves)
                {
                    for (var i = 0; i < move.Steps; i++)
                    {
                        rope = rope.Move(move.Direction);
                        tailPoints.Add(rope.Tail);
                    }
                }

                Console.WriteLine(tailPoints.Count);
            }
        }

        private record Point(int X, int Y)
        {
            public static readonly Point Origin = new(0, 0);

            public Point Move(Direction direction) =>
                direction switch
                {
                    Direction.L => this with { X = X - 1 },
                    Direction.R => this with { X = X + 1 },
                    Direction.U => this with { Y = Y - 1 },
                    Direction.D or _ => this with { Y = Y + 1 }
                };
        }

        private record Rope(IReadOnlyList<Point> Points)
        {
            public static Rope Initial(int length)
            {
                var points = Enumerable.Range(0, length).Select(_ => Point.Origin).ToList();
                return new Rope(points);
            }

            public Point Head => this.Points[0];
            public Point Tail => this.Points[this.Points.Count - 1];
               
            public Rope Move(Direction direction)
            {
                static int Cap(int value) =>
                    value switch
                    {
                        0 => 0,
                        < 0 => -1,
                        > 0 => 1
                    };

                static Point NextTail(Point head, Point tail)
                {
                    var dx = head.X - tail.X;
                    var dy = head.Y - tail.Y;

                    if (Math.Abs(dx) < 2 && Math.Abs(dy) < 2)
                    {
                        return tail;
                    }
                    if (dx == 0)
                    {
                        return tail with { Y = tail.Y + Cap(dy) };
                    }
                    if (dy == 0)
                    {
                        return tail with { X = tail.X + Cap(dx) };
                    }

                    return new Point(tail.X + Cap(dx), tail.Y + Cap(dy));
                }

                var nextPoints = new List<Point>() { Head.Move(direction) };

                for (var i = 1; i < this.Points.Count; i++)
                {
                    nextPoints.Add(NextTail(nextPoints[i - 1], this.Points[i]));
                }

                return new Rope(nextPoints);
            }
        }

        private enum Direction { L, R, U, D }

        private record Move(Direction Direction, int Steps)
        {
            public static IEnumerable<Move> Parse(IEnumerable<string> lines) =>
                lines.Select(Parse);

            public static Move Parse(string line)
            {
                var parts = line.Split(' ');

                var dir = ParseDirection(parts[0]);
                var steps = int.Parse(parts[1]);

                return new Move(dir, steps);
            }

            private static Direction ParseDirection(string dir) =>
                dir switch
                {
                    "L" => Direction.L,
                    "R" => Direction.R,
                    "U" => Direction.U,
                    "D" or _ => Direction.D
                };
        }
    }
}
