using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day05
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "0,9 -> 5,9",
                    "8,0 -> 0,8",
                    "9,4 -> 3,4",
                    "2,2 -> 2,1",
                    "7,0 -> 7,4",
                    "6,4 -> 2,0",
                    "0,9 -> 2,9",
                    "3,4 -> 1,4",
                    "0,0 -> 8,8",
                    "5,5 -> 8,2"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/5/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var lines = input.Lines().Select(Line.Parse).ToList();

                var answer = Line.CountIntersectionPoints(
                    lines.Where(l => l.IsHorizontal() || l.IsVertical()));
                
                Console.WriteLine(answer);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var lines = input.Lines().Select(Line.Parse).ToList();

                var answer = Line.CountIntersectionPoints(lines);

                Console.WriteLine(answer);
            }
        }

        public class Point
        {
            public static Point Parse(string text)
            {
                var parts = text.Split(',');

                var x = int.Parse(parts[0]);
                var y = int.Parse(parts[1]);

                return new Point(x, y);
            }

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }

            public override bool Equals(object obj) =>
                obj is Point other ? X == other.X && Y == other.Y : false;

            public override int GetHashCode() => HashCode.Combine(X, Y);

            public Point Add(int dx, int dy) => new Point(X + dx, Y + dy);
        }

        public class Line
        {
            public static Line Parse(string text)
            {
                var parts = text.Split(" -> ");

                var start = Point.Parse(parts[0]);
                var end = Point.Parse(parts[1]);

                return new Line(start, end);
            }

            public static int CountIntersectionPoints(IEnumerable<Line> lines)
            {
                return  lines
                    .SelectMany(l => l.Points())
                    .GroupBy(p => p)
                    .Where(g => g.Count() > 1)
                    .Count();
            }

            public Line(Point start, Point end)
            {
                Start = start;
                End = end;
            }

            public Point Start { get; }
            public Point End { get; }

            public bool IsHorizontal() => Start.X == End.X;
            public bool IsVertical() => Start.Y == End.Y;

            public IEnumerable<Point> Points()
            {
                var dx = Math.Sign(End.X - Start.X);
                var dy = Math.Sign(End.Y - Start.Y);

                var point = Start;
                while (!point.Equals(End))
                {
                    yield return point;
                    point = point.Add(dx, dy);
                }

                yield return End;
            }
        }
    }
}
