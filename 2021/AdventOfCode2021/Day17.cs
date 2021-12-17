using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day17
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal("target area: x=20..30, y=-10..-5");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/17/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                // How Y coordinate changes depending on initial V0y:
                //
                //  0: 0, 0, -1
                //  1: 0, 1, 1, 0, -2
                //  2: 0, 2, 3, 3, 2, 0, -3
                //  3: 0, 3, 5, 6, 6, 5, 3, 0, -4
                //  ...
                //
                // Startting at Y=0 it'll return to that position with Vy = -(V0y + 1)
                // 
                // If by that point, the velocity is such that we overshoot the designated area,
                // we'll never be in that area. So the maximum Vy to consider is (-bottom.Y + 1).

                var area = Area.Parse(input.Lines().First());

                var velocities = Vector.Between(
                    minX: 0, maxX: area.BottomRight.X,
                    minY: 0, maxY: -area.BottomRight.Y + 1
                );

                var answer = velocities
                    .Select(v =>
                    {
                        var points = Simulation.Points(v, maxY: area.BottomRight.Y).ToList();
                        return new
                        {
                            inArea = points.Any(area.Contains),
                            maxY = points.Select(p => p.Y).Max()
                        };
                    })
                    .Where(d => d.inArea)
                    .Select(d => d.maxY)
                    .Max();

                Console.WriteLine(answer);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var area = Area.Parse(input.Lines().First());

                var maxY = -area.BottomRight.Y + 1;
                var velocities = Vector.Between(
                    minX:     0, maxX: area.BottomRight.X,
                    minY: -maxY, maxY: maxY
                );

                var answer = velocities
                    .Where(v => Simulation.Points(v, maxY: area.BottomRight.Y).Any(area.Contains))
                    .Count();

                Console.WriteLine(answer);
            }
        }

        private class Vector
        {
            public static readonly Vector Zero = new Vector(0, 0);

            public static IEnumerable<Vector> Between(int minX, int maxX, int minY, int maxY)
            {
                var xs = EnumerableExtensions.Between(minX, maxX);
                var ys = EnumerableExtensions.Between(minY, maxY);
                return xs.SelectMany(x => ys.Select(y => new Vector(x, y)));
            }

            public Vector(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }

            public override string ToString() => $"{X},{Y}";

            public override bool Equals(object obj) =>
                obj is Vector other ? X == other.X && Y == other.Y : false;

            public override int GetHashCode() => HashCode.Combine(X, Y);

            public Vector Add(Vector p) => new Vector(X + p.X, Y + p.Y);
        }

        private class Area
        {
            public static Area Parse(string text)
            {
                const string prefix = "target area: ";
                text = text.Substring(prefix.Length);

                var parts = text.Split(", ")
                    .Select(p => p.Substring(2)) // skips x= or y=
                    .Select(p => p.Split("..").Select(int.Parse).ToList())
                    .ToList();

                var x1 = parts[0][0];
                var y1 = parts[1][0];
                var x2 = parts[0][1];
                var y2 = parts[1][1];

                return new Area(
                    new Vector(Math.Min(x1, x2), Math.Max(y1, y2)), 
                    new Vector(Math.Max(x1, x2), Math.Min(y1, y2))
                );
            }

            public Area(Vector topLeft, Vector bottomRight)
            {
                TopLeft = topLeft;
                BottomRight = bottomRight;
            }

            public Vector TopLeft { get; }
            public Vector BottomRight { get; }

            public bool Contains(Vector p) =>
                TopLeft.X <= p.X && p.X <= BottomRight.X &&
                BottomRight.Y <= p.Y && p.Y <= TopLeft.Y;
        }

        private static class Simulation
        {
            public static IEnumerable<Vector> Points(Vector velocity, int maxY)
            {
                var point = Vector.Zero;

                while (point.Y >= maxY)
                {
                    yield return point;

                    point = point.Add(velocity);
                    velocity = new Vector(x: Math.Max(0, velocity.X - 1), y: velocity.Y - 1);
                }
            }
        }
    }
}
