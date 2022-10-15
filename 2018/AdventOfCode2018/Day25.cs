using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day25
    {
        public static class Inputs
        {
            public static readonly IInput Sample1 =
                Input.Literal(
                    " 0,0,0,0",
                    " 3,0,0,0",
                    " 0,3,0,0",
                    " 0,0,3,0",
                    " 0,0,0,3",
                    " 0,0,0,6",
                    " 9,0,0,0",
                    "12,0,0,0"
                );

            public static readonly IInput Sample2 =
                Input.Literal(
                    "-1,2,2,0",
                    "0,0,2,-2",
                    "0,0,0,-2",
                    "-1,2,0,0",
                    "-2,-2,-2,2",
                    "3,0,2,-1",
                    "-1,3,2,2",
                    "-1,0,-1,0",
                    "0,2,1,-2",
                    "3,0,0,0"
                );

            public static readonly IInput Sample3 =
                Input.Literal(
                    "1,-1,0,1",
                    "2,0,-1,0",
                    "3,2,-1,0",
                    "0,0,3,1",
                    "0,0,-1,-1",
                    "2,3,-2,0",
                    "-2,2,0,0",
                    "2,-2,0,-1",
                    "1,-1,0,-1",
                    "3,2,0,2"
                );

            public static readonly IInput Sample4 =
                Input.Literal(
                    "1,-1,-1,-2",
                    "-2,-2,0,1",
                    "0,2,1,3",
                    "-2,3,-2,1",
                    "0,2,3,-2",
                    "-1,-1,1,-2",
                    "0,-2,-1,0",
                    "-2,2,3,-1",
                    "1,2,2,0",
                    "-1,-2,0,-2"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/25/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var points = input.Lines().Select(Point.Parse).ToList();

                IReadOnlyList<Point> remainingPoints = points;
                var count = 0;
                while (remainingPoints.Count > 0)
                {
                    remainingPoints = RemoveConstellation(remainingPoints);
                    count++;
                }

                Console.WriteLine(count);
            }

            private static IReadOnlyList<Point> RemoveConstellation(IReadOnlyList<Point> initialPoints)
            {
                var points = initialPoints.ToList();

                var constellation = new List<Point>() { points[0] };
                points[0] = null;

                while (constellation.Count > 0)
                {
                    var nextConstellation = new List<Point>();
                    for (var i = 0; i < points.Count; i++)
                    {
                        var point = points[i];
                        if (point == null)
                        {
                            continue;
                        }

                        if (constellation.Any(p => Point.ManhattanDistance(p, point) <= 3))
                        {
                            nextConstellation.Add(point);
                            points[i] = null;
                        }
                    }

                    constellation = nextConstellation;
                }

                return points.Where(p => p != null).ToList();
            }
        }

        // There is no part 2 on Day 25.

        //public class Part2 : IProblem
        //{
        //    public void Run(TextReader input)
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        private record Point(int A, int B, int C, int D)
        {
            public static Point Parse(string line)
            {
                var parts = line.Split(',').Select(int.Parse).ToList();
                return new Point(parts[0], parts[1], parts[2], parts[3]);
            }

            public static int ManhattanDistance(Point p1, Point p2) =>
                Math.Abs(p1.A - p2.A) + Math.Abs(p1.B - p2.B) + Math.Abs(p1.C - p2.C) + Math.Abs(p1.D - p2.D);
        }
    }
}
