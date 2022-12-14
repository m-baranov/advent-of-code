using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day14
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "498,4 -> 498,6 -> 496,6",
                    "503,4 -> 502,4 -> 502,9 -> 494,9"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/14/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var paths = input.Lines().Select(Path.Parse).ToList();

                var origin = new Point(500, 0);
                var grid = Grid.Create(origin, paths);

                while (true)
                {
                    var ok = SimulateUnit(grid, origin);
                    if (!ok)
                    {
                        break;
                    }
                }

                var count = grid.Count(Cell.Sand);
                Console.WriteLine(count);
            }

            private static bool SimulateUnit(Grid grid, Point origin)
            {
                var point = origin;
                while (true)
                {
                    var result = SimulateStep(grid, point, out var next);
                    if (result == Result.OutOfBounds)
                    {
                        return false;
                    }

                    if (result == Result.Blocked)
                    {
                        grid.Set(point, Cell.Sand);
                        return true;
                    }

                    point = next;
                }
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var paths = input.Lines().Select(Path.Parse).ToList();

                var origin = new Point(500, 0);
                var grid = Grid.Create(origin, paths);

                while (true)
                {
                    var point = SimulateUnit(grid, origin);
                    if (point == origin)
                    {
                        break;
                    }

                    if (point.X == grid.MinX)
                    {
                        grid = grid.ExpandHorizontally(left: grid.Width / 2, right: 0);
                    }
                    else if (point.X == grid.MaxX)
                    {
                        grid = grid.ExpandHorizontally(left: 0, right: grid.Width / 2);
                    }
                }

                var count = grid.Count(Cell.Sand);
                Console.WriteLine(count);
            }

            private static Point SimulateUnit(Grid grid, Point origin)
            {
                var point = origin;
                while (true)
                {
                    var result = SimulateStep(grid, point, out var next);
                    if (result == Result.OutOfBounds || result == Result.Blocked)
                    {
                        grid.Set(point, Cell.Sand);
                        return point;
                    }

                    point = next;
                }
            }
        }

        private static Result SimulateStep(Grid grid, Point point, out Point next)
        {
            next = point with { Y = point.Y + 1 };
            if (!grid.InBounds(next))
            {
                return Result.OutOfBounds;
            }

            if (grid.At(next) == Cell.Air)
            {
                return Result.Advanced;
            }

            next = next with { X = point.X - 1 };
            if (grid.InBounds(next) && grid.At(next) == Cell.Air)
            {
                return Result.Advanced;
            }

            next = next with { X = point.X + 1 };
            if (grid.InBounds(next) && grid.At(next) == Cell.Air)
            {
                return Result.Advanced;
            }

            return Result.Blocked;
        }

        private enum Result { Advanced, Blocked, OutOfBounds }

        private record Point(int X, int Y)
        {
            public static Point Parse(string text)
            {
                var parts = text.Split(',');

                var x = int.Parse(parts[0]);
                var y = int.Parse(parts[1]);

                return new Point(x, y);
            }

            public static (int minX, int minY, int maxX, int maxY) BoundingBox(IEnumerable<Point> points)
            {
                static (int min, int max) MinMax(IEnumerable<int> items) =>
                    items.Aggregate(
                        (min: int.MaxValue, max: int.MinValue),
                        (acc, item) => (min: Math.Min(item, acc.min), max: Math.Max(item, acc.max))
                    );

                var (minX, maxX) = MinMax(points.Select(p => p.X));
                var (minY, maxY) = MinMax(points.Select(p => p.Y));

                return (minX, minY, maxX, maxY);
            }
        }

        private record Line(Point A, Point B)
        {
            public IEnumerable<Point> Points()
            {
                if (A.X == B.X)
                {
                    var minY = Math.Min(A.Y, B.Y);
                    var maxY = Math.Max(A.Y, B.Y);

                    for (var y = minY; y <= maxY; y++)
                    {
                        yield return new Point(A.X, y);
                    }
                }
                else if (A.Y == B.Y)
                {
                    var minX = Math.Min(A.X, B.X);
                    var maxX = Math.Max(A.X, B.X);

                    for (var x = minX; x <= maxX; x++)
                    {
                        yield return new Point(x, A.Y);
                    }
                }
                else
                {
                    Debug.Assert(false, "Should not be possible.");
                }
            }
        }

        private record Path(IReadOnlyList<Line> Lines)
        {
            public static IEnumerable<Point> PointsOf(IReadOnlyList<Path> paths) =>
                paths
                    .SelectMany(p => p.Lines)
                    .SelectMany(l => new[] { l.A, l.B });

            public static Path Parse(string text)
            {
                var lines = text
                    .Split(" -> ")
                    .Select(Point.Parse)
                    .Pairs()
                    .Select(p => new Line(p.first, p.second))
                    .ToList();

                return new Path(lines);
            }
        }

        private enum Cell { Air, Rock, Sand }

        private class Grid
        {
            public static Grid Create(Point origin, IReadOnlyList<Path> paths)
            {
                var points = Path.PointsOf(paths).Append(origin);
                var (minX, minY, maxX, maxY) = Point.BoundingBox(points);

                var grid = new Grid(minX - 1, minY - 1, maxX + 1, maxY + 1);
                grid.Set(paths, Cell.Rock);

                return grid;
            }

            private readonly Cell[,] cells;

            public Grid(int minX, int minY, int maxX, int maxY)
            {
                this.MinX = minX;
                this.MinY = minY;

                this.Width = maxX - minX + 1;
                this.Height = maxY - minY + 1;

                this.cells = new Cell[Height, Width]; 
            }

            public int Width { get; }
            public int Height { get; }
            public int MinX { get; }
            public int MinY { get; }
            public int MaxX => this.MinX + this.Width - 1;
            public int MaxY => this.MinY + this.Height - 1;

            public Grid ExpandHorizontally(int left, int right)
            {
                var newMinX = this.MinX - left;
                var newMaxX = this.MaxX + right;

                var newGrid = new Grid(newMinX, MinY, newMaxX, this.MaxY);

                for (var y = this.MinY; y <= this.MaxY; y++)
                {
                    for (var x = this.MinX; x <= this.MaxX; x++)
                    {
                        var p = new Point(x, y);
                        newGrid.Set(p, this.At(p));
                    }
                }

                return newGrid;
            }

            public bool InBounds(Point p)
            {
                var x = p.X - this.MinX;
                var y = p.Y - this.MinY;

                return 0 <= x && x < Width
                    && 0 <= y && y < Height;
            }

            public Cell At(Point p)
            {
                var x = p.X - this.MinX;
                var y = p.Y - this.MinY;
                return this.cells[y, x];
            }

            public void Set(Point p, Cell value)
            {
                var x = p.X - this.MinX;
                var y = p.Y - this.MinY;
                this.cells[y, x] = value;
            }

            public void Set(Line line, Cell value)
            {
                foreach (var point in line.Points())
                {
                    Set(point, value);
                }
            }

            public void Set(Path path, Cell value)
            {
                foreach (var line in path.Lines)
                {
                    Set(line, value);
                }
            }

            public void Set(IEnumerable<Path> paths, Cell value)
            {
                foreach (var path in paths)
                {
                    Set(path, value);
                }
            }

            public int Count(Cell value)
            {
                var count = 0;

                for (var y = 0; y < this.Height; y++)
                {
                    for (var x = 0; x < this.Width; x++)
                    {
                        if (this.cells[y, x] == value)
                        {
                            count++;
                        }
                    }
                }

                return count;
            }
        }
    }
}
