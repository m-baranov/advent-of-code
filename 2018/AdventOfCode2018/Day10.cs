using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day10
    {
        public static class Inputs
        {
            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/10/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = Grid.Parse(input.Lines());

                var prevGrid = grid;
                var area = long.MaxValue;
                while (true)
                {
                    var nextArea = grid.Area();

                    if (nextArea > area)
                    {
                        prevGrid.Display();
                        break;
                    }

                    prevGrid = grid;
                    grid = grid.Advance();
                    area = nextArea;
                }
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = Grid.Parse(input.Lines());

                var prevGrid = grid;
                var area = long.MaxValue;
                var time = 0;
                while (true)
                {
                    var nextArea = grid.Area();

                    if (nextArea > area)
                    {
                        prevGrid.Display();
                        break;
                    }

                    prevGrid = grid;
                    grid = grid.Advance();
                    area = nextArea;
                    time++;
                }

                Console.WriteLine(time - 1);
            }
        }

        private class Grid
        {
            public static Grid Parse(IEnumerable<string> lines)
            {
                var points = lines.Select(Point.Parse).ToList();
                return new Grid(points);
            }

            private IReadOnlyList<Point> points;

            public Grid(IReadOnlyList<Point> points)
            {
                this.points = points;
            }

            public long Area()
            {
                var (minX, maxX, minY, maxY) = Dimensions();
                
                var w = maxX - minX + 1;
                var h = maxY - minY + 1;

                return (long)w * h;
            }

            private (int minX, int maxX, int minY, int maxY) Dimensions()
            {
                var minX = points.Select(p => p.Position.X).Min();
                var maxX = points.Select(p => p.Position.X).Max();

                var minY = points.Select(p => p.Position.Y).Min();
                var maxY = points.Select(p => p.Position.Y).Max();

                return (minX, maxX, minY, maxY);
            }

            public void Display()
            {
                var set = this.points.Select(p => p.Position).ToHashSet();

                var (minX, maxX, minY, maxY) = Dimensions();

                for (var y = minY; y <= maxY; y++)
                {
                    for (var x = minX; x <= maxX; x++)
                    {
                        var ch = set.Contains(new Position(x, y)) ? '#' : '.';
                        Console.Write(ch);
                    }
                    Console.WriteLine();
                }
            }

            public Grid Advance()
            {
                var points = this.points.Select(p => p.Advance()).ToList();
                return new Grid(points);
            }
        }

        private struct Point
        {
            public static Point Parse(string text)
            {
                var (posText, rest) = SplitCoords(text);
                var (velText, _) = SplitCoords(rest);

                var (px, py) = ParseCoords(posText);
                var (vx, vy) = ParseCoords(velText);

                return new Point(new Position(px, py), new Velocity(vx, vy));
            }

            private static (string coords, string rest) SplitCoords(string text)
            {
                var start = text.IndexOf('<');
                var end = text.IndexOf('>');

                // 0123456789
                // <12, 34> v

                var coords = text.Substring(start + 1, end - start - 1);
                var rest = end < text.Length ? text.Substring(end + 1) : string.Empty;

                return (coords, rest);
            }

            private static (int x, int y) ParseCoords(string text)
            {
                var words = text.Split(',')
                    .Select(w => w.Trim())
                    .Select(int.Parse)
                    .Take(2)
                    .ToList();

                return (words[0], words[1]);
            }

            public Point(Position position, Velocity velocity)
            {
                Position = position;
                Velocity = velocity;
            }

            public Position Position { get; }
            public Velocity Velocity { get; }

            public Point Advance() => new Point(Position.Add(Velocity), Velocity);
        }

        private struct Position
        {
            public Position(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }

            public Position Add(Velocity v) => new Position(this.X + v.Dx, this.Y + v.Dy);

            public override bool Equals([NotNullWhen(true)] object obj) =>
                obj is Position p && (this.X == p.X && this.Y == p.Y);

            public override int GetHashCode() => HashCode.Combine(this.X, this.Y);
        }

        private struct Velocity
        {
            public Velocity(int dx, int dy)
            {
                Dx = dx;
                Dy = dy;
            }

            public int Dx { get; }
            public int Dy { get; }
        }
    }
}
