using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day18
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "2,2,2",
                    "1,2,2",
                    "3,2,2",
                    "2,1,2",
                    "2,3,2",
                    "2,2,1",
                    "2,2,3",
                    "2,2,4",
                    "2,2,6",
                    "1,2,5",
                    "3,2,5",
                    "2,1,5",
                    "2,3,5"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/18/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var points = input.Lines().Select(Point.Parse).ToList();

                var area = SurfaceArea.Compute(points);
                Console.WriteLine(area);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var points = input.Lines().Select(Point.Parse).ToList();

                var grid = new Grid(points);
                var innerAreas = grid.FindAllInnerFreeAreas();
                var pointsWithInnerAreasFilled = innerAreas.SelectMany(a => a).Concat(points).ToList();

                var area = SurfaceArea.Compute(pointsWithInnerAreasFilled);
                Console.WriteLine(area);
            }

            private class Grid
            {
                private const byte FREE = 0;
                private const byte OCCUPIED = 1;
                private const byte FREE_MARKED = 2;

                private readonly byte[,,] cells;

                private readonly (int min, int max) rangeX;
                private readonly (int min, int max) rangeY;
                private readonly (int min, int max) rangeZ;

                private readonly int lengthX;
                private readonly int lengthY;
                private readonly int lengthZ;

                public Grid(IReadOnlyList<Point> points)
                {
                    this.rangeX = MinMax(points.Select(p => p.X));
                    this.rangeY = MinMax(points.Select(p => p.Y));
                    this.rangeZ = MinMax(points.Select(p => p.Z));

                    this.lengthX = this.rangeX.max - this.rangeX.min + 1;
                    this.lengthY = this.rangeY.max - this.rangeY.min + 1;
                    this.lengthZ = this.rangeZ.max - this.rangeZ.min + 1;

                    this.cells = new byte[lengthX, lengthY, lengthZ];

                    SetAll(points, OCCUPIED);
                }

                public bool InBounds(Point point) =>
                    this.rangeX.min <= point.X && point.X <= this.rangeX.max &&
                    this.rangeY.min <= point.Y && point.Y <= this.rangeY.max &&
                    this.rangeZ.min <= point.Z && point.Z <= this.rangeZ.max;

                public byte Get(Point point)
                {
                    var x = point.X - this.rangeX.min;
                    var y = point.Y - this.rangeY.min;
                    var z = point.Z - this.rangeZ.min;
                    return this.cells[x, y, z];
                }

                public void Set(Point point, byte value)
                {
                    var x = point.X - this.rangeX.min;
                    var y = point.Y - this.rangeY.min;
                    var z = point.Z - this.rangeZ.min;
                    this.cells[x, y, z] = value;
                }

                private void SetAll(IReadOnlyCollection<Point> points, byte value)
                {
                    foreach (var point in points)
                    {
                        Set(point, value);
                    }
                }

                public IReadOnlyList<IReadOnlyCollection<Point>> FindAllInnerFreeAreas()
                {
                    bool Outside(Point p) =>
                        p.X == this.rangeX.min || p.X == this.rangeX.max ||
                        p.Y == this.rangeY.min || p.Y == this.rangeY.max ||
                        p.Z == this.rangeZ.min || p.Z == this.rangeZ.max;

                    return FindAllFreeAreas()
                        .Where(a => !a.Any(Outside))
                        .ToList();
                }

                private IReadOnlyList<IReadOnlyCollection<Point>> FindAllFreeAreas()
                {
                    var areas = new List<IReadOnlyCollection<Point>>();

                    for (var x = this.rangeX.min; x <= this.rangeX.max; x++)
                    {
                        for (var y = this.rangeY.min; y <= this.rangeY.max; y++)
                        {
                            for (var z = this.rangeZ.min; z <= this.rangeZ.max; z++)
                            {
                                var point = new Point(x, y, z);

                                if (Get(point) == FREE)
                                {
                                    var area = FindFreeArea(point);
                                    areas.Add(area);

                                    SetAll(area, FREE_MARKED);
                                }
                            }
                        }
                    }

                    return areas;
                }

                private IReadOnlyCollection<Point> FindFreeArea(Point start)
                {
                    static IEnumerable<Point> AdjacentTo(Point p)
                    {
                        yield return p with { X = p.X + 1 };
                        yield return p with { X = p.X - 1 };

                        yield return p with { Y = p.Y + 1 };
                        yield return p with { Y = p.Y - 1 };

                        yield return p with { Z = p.Z + 1 };
                        yield return p with { Z = p.Z - 1 };
                    }

                    var queue = new Queue<Point>();
                    queue.Enqueue(start);

                    var points = new HashSet<Point>();

                    while (queue.Count > 0)
                    {
                        var point = queue.Dequeue();

                        if (!InBounds(point) || Get(point) != FREE || points.Contains(point))
                        {
                            continue;
                        }
                        points.Add(point);

                        foreach (var adjacentPoint in AdjacentTo(point))
                        {
                            queue.Enqueue(adjacentPoint);
                        }
                    }

                    return points;
                }
            }
            private static (int min, int max) MinMax(IEnumerable<int> items) =>
                items.Aggregate(
                    (min: int.MaxValue, max: int.MinValue),
                    (acc, i) => (min: Math.Min(acc.min, i), max: Math.Max(acc.max, i))
                );
        }

        private static class SurfaceArea
        {
            public static int Compute(IReadOnlyList<Point> points)
            {
                static IEnumerable<(Plane, Point)> SidesOf(Point p)
                {
                    yield return new(Plane.XY, p);
                    yield return new(Plane.XY, p with { Z = p.Z + 1 });

                    yield return new(Plane.YZ, p);
                    yield return new(Plane.YZ, p with { X = p.X + 1 });

                    yield return new(Plane.XZ, p);
                    yield return new(Plane.XZ, p with { Y = p.Y + 1 });
                }

                var sides = new Dictionary<(Plane, Point), int>();

                foreach (var point in points)
                {
                    foreach (var side in SidesOf(point))
                    {
                        sides.TryGetValue(side, out var count);
                        sides[side] = count + 1;
                    }
                }

                return sides.Count(p => p.Value == 1);
            }

            private enum Plane { XY, YZ, XZ }
        }

        private record Point(int X, int Y, int Z)
        {
            public static Point Parse(string text)
            {
                var parts = text.Split(',').Select(int.Parse).Take(3).ToList();
                return new Point(parts[0], parts[1], parts[2]);
            }
        }
    }
}
