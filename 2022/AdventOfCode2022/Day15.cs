using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day15
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "Sensor at x=2, y=18: closest beacon is at x=-2, y=15",
                    "Sensor at x=9, y=16: closest beacon is at x=10, y=16",
                    "Sensor at x=13, y=2: closest beacon is at x=15, y=3",
                    "Sensor at x=12, y=14: closest beacon is at x=10, y=16",
                    "Sensor at x=10, y=20: closest beacon is at x=10, y=16",
                    "Sensor at x=14, y=17: closest beacon is at x=10, y=16",
                    "Sensor at x=8, y=7: closest beacon is at x=2, y=10",
                    "Sensor at x=2, y=0: closest beacon is at x=2, y=10",
                    "Sensor at x=0, y=11: closest beacon is at x=2, y=10",
                    "Sensor at x=20, y=14: closest beacon is at x=25, y=17",
                    "Sensor at x=17, y=20: closest beacon is at x=21, y=22",
                    "Sensor at x=16, y=7: closest beacon is at x=15, y=3",
                    "Sensor at x=14, y=3: closest beacon is at x=15, y=3",
                    "Sensor at x=20, y=1: closest beacon is at x=15, y=3"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/15/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var reports = input.Lines().Select(Report.Parse).ToList();

                //var lineY = 10; // use for sample input
                var lineY = 2_000_000;

                var combinedRanges = Scan(reports, lineY);

                var knownBotsInRange = reports
                    .Select(r => r.Beacon)
                    .Where(p => p.Y == lineY)
                    .Distinct()
                    .Where(p => combinedRanges.Any(r => r.Contains(p.X)))
                    .Count();

                var sum = combinedRanges.Sum(r => r.Length()) - knownBotsInRange;
                Console.WriteLine(sum);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var reports = input.Lines().Select(Report.Parse).ToList();

                //var box = new Range(0, 20); // use for sample input
                var box = new Range(0, 4_000_000);

                // A bit slow, but it's simple and it works :)

                Point result = null;
                var y = box.Start;
                while (y <= box.End)
                {
                    var ranges = Scan(reports, y)
                        .Select(r => r.Clamp(box))
                        .ToList();

                    var candidates = ranges
                        .SelectMany(r => new[] { r.Start - 1, r.End + 1 })
                        .Where(c => box.Contains(c))
                        .Take(1)
                        .ToList();

                    if (candidates.Count > 0)
                    {
                        var x = candidates[0];
                        result = new Point(x, y);
                        break;
                    }

                    y++;
                }

                if (result != null)
                {
                    Console.WriteLine(result);
                    Console.WriteLine((long)result.X * 4_000_000 + result.Y);
                }
                else
                {
                    Console.WriteLine("Not found");
                }
            }
        }

        private static IReadOnlyList<Range> Scan(IReadOnlyList<Report> reports, int lineY)
        {
            var ranges = reports
                .Select((report, index) => (
                    index,
                    center: report.Sensor,
                    distance: Point.ManhattanDistance(report.Sensor, report.Beacon)
                ))
                .Where(p => p.center.Y - p.distance <= lineY && lineY <= p.center.Y + p.distance)
                .Select(p =>
                {
                    var dy = Math.Abs(p.center.Y - lineY);
                    return new Range(p.center.X - p.distance + dy, p.center.X + p.distance - dy);
                })
                .ToList();

            return Range.Intersect(ranges);
        }

        private record Point(int X, int Y)
        {
            public static Point Parse(string text)
            {
                // x=20, y=1
                var coords = text.Split(", ")
                    .Select(p => p.Substring(2)) // skip x= or y=
                    .Select(int.Parse)
                    .Take(2)
                    .ToList();

                return new Point(coords[0], coords[1]);
            }

            public static int ManhattanDistance(Point a, Point b) =>
                Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private record Report(Point Sensor, Point Beacon)
        {
            public static Report Parse(string text)
            {
                // "Sensor at x=2, y=18: closest beacon is at x=-2, y=15";

                const string Prefix = "Sensor at ";
                const string Separator = ": closest beacon is at ";

                var points = text.Substring(Prefix.Length)
                    .Split(Separator)
                    .Select(Point.Parse)
                    .Take(2)
                    .ToList();

                return new Report(points[0], points[1]);
            }
        }

        private record Range(int Start, int End)
        {
            public static IReadOnlyList<Range> Intersect(IReadOnlyList<Range> ranges)
            {
                var results = new List<Range>();

                foreach (var range in ranges)
                {
                    var i = 0;
                    var candidateRange = range;
                    while (i < results.Count)
                    {
                        var resultRange = results[i];
                        
                        if (resultRange != null && 
                            TryIntersect(candidateRange, resultRange, out var combinedRange))
                        {
                            results[i] = null;

                            i = 0;
                            candidateRange = combinedRange;
                            continue;
                        }

                        i++;
                    }

                    results.Add(candidateRange);
                }

                return results.Where(r => r != null).ToList();
            }

            public static bool TryIntersect(Range a, Range b, out Range result)
            {
                if (!Intersects(a, b))
                {
                    result = default;
                    return false;
                }

                var start = Math.Min(a.Start, b.Start);
                var end = Math.Max(a.End, b.End);

                result = new Range(start, end);
                return true;
            }

            public static bool Intersects(Range a, Range b) =>
                !(a.End < b.Start || b.End < a.Start);

            public int Length() => End - Start + 1;

            public bool Contains(int value) => Start <= value && value <= End;

            public Range Clamp(Range box)
            {
                var start = Start < box.Start ? box.Start : Start;
                var end = box.End < End ? box.End : End;
                return new Range(start, end);
            }
        }

    }
}
