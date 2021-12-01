using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day03
    {
        public static readonly IInput Sample1Input =
            Input.Literal(
                "R8,U5,L5,D3",
                "U7,R6,D4,L4"
            );

        public static readonly IInput Sample2Input =
            Input.Literal(
                "R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51",
                "U98,R91,D20,R16,D67,R40,U7,R15,U6,R7"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/3/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var paths = input.Lines().Take(2).Select(Path.Parse).ToList();

                var origin = new Point(0, 0);

                var lines1 = paths[0].ToLines(origin);
                var lines2 = paths[1].ToLines(origin);

                var answer = lines1
                    .SelectMany(l => l.Points())
                    .Where(p => lines2.Any(l => l.Contains(p)))
                    .Select(p => Point.ManhattanDistance(origin, p))
                    .Min();

                Console.WriteLine(answer);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var paths = input.Lines().Take(2).Select(Path.Parse).ToList();

                var origin = new Point(0, 0);

                var lines1 = paths[0].ToLines(origin);
                var lines2 = paths[1].ToLines(origin);

                var intersections = lines1
                    .SelectMany(l => l.Points())
                    .Where(p => lines2.Any(l => l.Contains(p)))
                    .ToList();

                var answer = intersections
                    .Select(intersection =>
                    {
                        var steps1 = StepsUntilIntersection(lines1, intersection);
                        var steps2 = StepsUntilIntersection(lines2, intersection);
                        Console.WriteLine($"{steps1} + {steps2}");
                        return steps1 + steps2;
                    })
                    .Min();
                   
                Console.WriteLine(answer);
            }

            private int StepsUntilIntersection(IReadOnlyList<Line> lines, Point intersection)
            {
                // +1 accounts for the interection point itself
                return lines.SelectMany(l => l.Points()).TakeWhile(p => !p.Equals(intersection)).Count() + 1;
            }
        }

        private enum Direction { Left, Right, Up, Down }

        private class PathSegment
        {
            public static PathSegment Parse(string line)
            {
                var direction = ParseDirection(line.Substring(0, 1));
                var length = int.Parse(line.Substring(1));
                return new PathSegment(direction, length);
            }

            private static Direction ParseDirection(string text)
            {
                if (text == "R") return Direction.Right;
                if (text == "L") return Direction.Left;
                if (text == "U") return Direction.Up;
                return Direction.Down;
            }

            public PathSegment(Direction direction, int length)
            {
                Direction = direction;
                Length = length;
            }

            public Direction Direction { get; }
            public int Length { get; }

            public (Line, Point) ToLine(Point origin)
            {
                if (Direction == Direction.Right)
                {
                    var end = new Point(origin.X + Length, origin.Y);
                    return (new Line.Horizontal(origin.Y, origin.X + 1, end.X), end);
                }
                else if (Direction == Direction.Left)
                {
                    var end = new Point(origin.X - Length, origin.Y);
                    return (new Line.Horizontal(origin.Y, origin.X - 1, end.X), end);
                }
                else if (Direction == Direction.Up)
                {
                    var end = new Point(origin.X, origin.Y + Length);
                    return (new Line.Vertical(origin.X, origin.Y + 1, end.Y), end);
                }
                else
                {
                    var end = new Point(origin.X, origin.Y - Length);
                    return (new Line.Vertical(origin.X, origin.Y - 1, end.Y), end);
                }
            }
        }

        private class Path
        {
            public static Path Parse(string line)
            {
                var segments = line.Split(',').Select(PathSegment.Parse).ToList();
                return new Path(segments);
            }

            public Path(IReadOnlyList<PathSegment> segments)
            {
                Segments = segments;
            }

            public IReadOnlyList<PathSegment> Segments { get; }

            public IReadOnlyList<Line> ToLines(Point origin)
            {
                var lines = new List<Line>();

                foreach (var segment in Segments)
                {
                    var (line, nextOrigin) = segment.ToLine(origin);

                    lines.Add(line);
                    origin = nextOrigin;
                }

                return lines;
            }
        }

        private class Point
        {
            public static int ManhattanDistance(Point a, Point b) => 
                Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }

            public override bool Equals(object obj) =>
                obj is Point p ? X == p.X && Y == p.Y : false;

            public override int GetHashCode() => HashCode.Combine(X, Y);

            public override string ToString() => $"({X},{Y})";
        }

        private abstract class Line
        {
            public abstract IEnumerable<Point> Points();
            public abstract bool Contains(Point p);

            public class Horizontal : Line
            {
                private readonly int y;
                private readonly int x1;
                private readonly int x2;

                public Horizontal(int y, int x1, int x2)
                {
                    this.y = y;
                    this.x1 = x1;
                    this.x2 = x2;
                }

                public override bool Contains(Point p)
                {
                    if (x1 < x2)
                    {
                        return this.y == p.Y && this.x1 <= p.X && p.X <= this.x2;
                    }
                    else
                    {
                        return this.y == p.Y && this.x2 <= p.X && p.X <= this.x1;
                    }
                }

                public override IEnumerable<Point> Points()
                {
                    // enumerate points in order they are on the path -- this is needed for part2
                    if (x1 < x2)
                    {
                        for (var x = x1; x <= x2; x++)
                        {
                            yield return new Point(x, this.y);
                        }
                    }
                    else
                    {
                        for (var x = x1; x >= x2; x--)
                        {
                            yield return new Point(x, this.y);
                        }
                    }
                }

                public override string ToString() => $"({x1},{y})-({x2},{y})";
            }

            public class Vertical : Line
            {
                private readonly int x;
                private readonly int y1;
                private readonly int y2;

                public Vertical(int x, int y1, int y2)
                {
                    this.x = x;
                    this.y1 = y1;
                    this.y2 = y2;
                }

                public override bool Contains(Point p)
                {
                    if (y1 < y2)
                    {
                        return this.x == p.X && this.y1 <= p.Y && p.Y <= this.y2;
                    }
                    else
                    {
                        return this.x == p.X && this.y2 <= p.Y && p.Y <= this.y1;
                    }
                }

                public override IEnumerable<Point> Points()
                {
                    // enumerate points in order they are on the path -- this is needed for part2
                    if (y1 < y2)
                    {
                        for (var y = y1; y <= y2; y++)
                        {
                            yield return new Point(this.x, y);
                        }
                    }
                    else
                    {
                        for (var y = y1; y >= y2; y--)
                        {
                            yield return new Point(this.x, y);
                        }
                    }
                }

                public override string ToString() => $"({x},{y1})-({x},{y2})";
            }
        }
    }
}
