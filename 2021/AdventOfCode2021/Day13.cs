using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day13
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "6,10",
                    "0,14",
                    "9,10",
                    "0,3",
                    "10,4",
                    "4,11",
                    "6,0",
                    "6,12",
                    "4,1",
                    "0,13",
                    "10,12",
                    "3,4",
                    "3,0",
                    "8,4",
                    "1,10",
                    "2,14",
                    "8,10",
                    "9,0",
                    "",
                    "fold along y=7",
                    "fold along x=5"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/13/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var setup = Setup.Parse(input.Lines());

                var image = Image.Of(setup.Points, setup.Folds.Take(1));

                var count = image.Points().Distinct().Count();
                Console.WriteLine(count);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var setup = Setup.Parse(input.Lines());

                var image = Image.Of(setup.Points, setup.Folds);

                Image.Draw(image);
            }
        }

        private class Point
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
        }

        private enum Axis { X, Y }

        private class Fold
        {
            public static Fold Parse(string text)
            {
                const string prefix = "fold along ";

                var parts = text.Substring(prefix.Length).Split('=');

                var axis = parts[0] == "x" ? Axis.X : Axis.Y;
                var value = int.Parse(parts[1]);

                return new Fold(axis, value);
            }

            public Fold(Axis axis, int value)
            {
                Axis = axis;
                Value = value;
            }

            public Axis Axis { get; }
            public int Value { get; }
        }

        private interface IImage
        {
            IEnumerable<Point> Points();
        }

        private class LiteralImage : IImage
        {
            private readonly IReadOnlyList<Point> points;

            public LiteralImage(IReadOnlyList<Point> points)
            {
                this.points = points;
            }

            public IEnumerable<Point> Points() => points;
        }

        private class FoldYImage : IImage
        {
            private readonly IImage image;
            private readonly int y;

            public FoldYImage(IImage image, int y)
            {
                this.image = image;
                this.y = y;
            }

            public IEnumerable<Point> Points() =>
                image.Points().Select(p => p.Y < y ? p : new Point(p.X, y * 2 - p.Y));
        }

        private class FoldXImage : IImage
        {
            private readonly IImage image;
            private readonly int x;

            public FoldXImage(IImage image, int x)
            {
                this.image = image;
                this.x = x;
            }

            public IEnumerable<Point> Points() =>
                image.Points().Select(p => p.X < x ? p : new Point(x * 2 - p.X, p.Y));
        }

        private static class Image
        {
            public static IImage Of(IReadOnlyList<Point> points, IEnumerable<Fold> folds) =>
                folds.Aggregate(Of(points), Fold);

            public static IImage Of(IReadOnlyList<Point> points) => 
                new LiteralImage(points);

            public static IImage Fold(IImage image, Fold fold) => 
                fold.Axis == Axis.X
                    ? new FoldXImage(image, fold.Value)
                    : new FoldYImage(image, fold.Value);

            public static void Draw(IImage image)
            {
                var points = image.Points().Distinct().ToHashSet();

                var minX = points.Select(p => p.X).Min();
                var maxX = points.Select(p => p.X).Max();

                var minY = points.Select(p => p.Y).Min();
                var maxY = points.Select(p => p.Y).Max();

                for (var y = minY; y <= maxY; y++)
                {
                    for (var x = minX; x <= maxX; x++)
                    {
                        var point = new Point(x, y);
                        Console.Write(points.Contains(point) ? "██" : "  ");
                    }
                    Console.WriteLine();
                }
            }
        }

        private class Setup
        {
            public static Setup Parse(IEnumerable<string> lines)
            {
                var groups = lines.SplitByEmptyLine().ToList();

                var points = groups[0].Select(Point.Parse).ToList();
                var folds = groups[1].Select(Fold.Parse).ToList();

                return new Setup(points, folds);
            }

            public Setup(IReadOnlyList<Point> points, IReadOnlyList<Fold> folds)
            {
                Points = points;
                Folds = folds;
            }

            public IReadOnlyList<Point> Points { get; }
            public IReadOnlyList<Fold> Folds { get; }
        }
    }
}
