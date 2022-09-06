using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day06
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "1, 1",
                    "1, 6",
                    "8, 3",
                    "3, 4",
                    "5, 5",
                    "8, 9"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/6/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var points = input.Lines().Select(Point.Parse).ToList();

                var bounds = Rect.BoundingBox(points);

                var sizes = new int[points.Count];
                var infinites = new HashSet<int>();

                foreach (var innerPoint in bounds.Points())
                {
                    var distances = points
                        .Select((point, index) => new
                        {
                            index = index,
                            distance = Point.ManhattanDistance(point, innerPoint)
                        })
                        .ToList();

                    var minDistance = distances.Min(d => d.distance);

                    var minDistances = distances
                        .Where(d => d.distance == minDistance)
                        .Take(2)
                        .ToList();

                    if (minDistances.Count == 1)
                    {
                        var index = minDistances[0].index;

                        sizes[index]++;

                        if (bounds.AtEdge(innerPoint))
                        {
                            infinites.Add(index);
                        }
                    }
                }

                var answer = points
                    .Select((point, index) => new { index, size = sizes[index] })
                    .Where(p => !infinites.Contains(p.index))
                    .Max(p => p.size);

                Console.WriteLine(answer);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var points = input.Lines().Select(Point.Parse).ToList();

                var bounds = Rect.BoundingBox(points);

                var answer = bounds.Points()
                    .Select(ip => points.Sum(p => Point.ManhattanDistance(p, ip)))
                    .Count(d => d < 10000);

                Console.WriteLine(answer);
            }
        }

        private class Point
        {
            public static Point Parse(string text)
            {
                var parts = text.Split(", ").Select(int.Parse).ToArray();
                return new Point(parts[0], parts[1]);
            }

            public static int ManhattanDistance(Point a, Point b) => 
                Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }
        }

        private class Rect
        {
            public static Rect BoundingBox(IEnumerable<Point> points)
            {
                var minX = points.Select(p => p.X).Min();
                var minY = points.Select(p => p.Y).Min();
                var topLeft = new Point(minX, minY);

                var maxX = points.Select(p => p.X).Max();
                var maxY = points.Select(p => p.Y).Max();
                var bottomRight = new Point(maxX, maxY);

                return new Rect(topLeft, bottomRight);
            }

            public Rect(Point topLeft, Point bottomRight)
            {
                TopLeft = topLeft;
                BottomRight = bottomRight;
            }

            public Point TopLeft { get; }
            public Point BottomRight { get; }

            public bool AtEdge(Point p) =>
                TopLeft.X == p.X || p.X == BottomRight.X ||
                TopLeft.Y == p.Y || p.Y == BottomRight.Y;

            public IEnumerable<Point> Points()
            {
                for (var x = TopLeft.X; x <= BottomRight.X; x++)
                {
                    for (var y = TopLeft.Y; y <= BottomRight.Y; y++)
                    {
                        yield return new Point(x, y);
                    }
                }
            }
        }
    }
}
