using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day06
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
Time:      7  15   30
Distance:  9  40  200
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/6/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var races = ParseRaces(input.Lines().ToList());

            var mul = races
                .Select(race => race.WaysToWin())
                .Aggregate(1L, (acc, num) => acc * num);

            Console.WriteLine(mul);
        }

        private static IReadOnlyList<Race> ParseRaces(IReadOnlyList<string> lines)
        {
            static IReadOnlyList<int> ParseNumbers(string text, string prefix) =>
                TrimPrefix(text, prefix)
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

            var times = ParseNumbers(lines[0], "Time:");
            var distances = ParseNumbers(lines[1], "Distance: ");

            return times
                .Zip(distances, (time, distance) => new Race(time, distance))
                .ToList();
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var race = ParseRace(input.Lines().ToList());

            var ways = race.WaysToWin();

            Console.WriteLine(ways);
        }

        private static Race ParseRace(IReadOnlyList<string> lines)
        {
            static long ParseNumber(string text, string prefix) =>
                long.Parse(TrimPrefix(text, prefix).Replace(" ", string.Empty));

            var time = ParseNumber(lines[0], "Time:");
            var distance = ParseNumber(lines[1], "Distance: ");

            return new Race(time, distance);
        }
    }

    private record Race(long Time, long RecordDistance)
    {
        public long WaysToWin()
        {
            var xs = MathEx.SolveQuadraticEquation(a: 1, b: -Time, c: RecordDistance);
            if (xs.Count < 2)
            {
                return 0;
            }

            var start = MathEx.SmalestIntegerLargerThan(xs[0]);
            var end = MathEx.LargestIntegestSmallerThan(xs[1]);

            if (start > end)
            {
                return 0;
            }

            return end - start + 1;
        }
    }

    private static class MathEx
    {
        // a*x^2 + b*x + c = 0
        public static IReadOnlyList<double> SolveQuadraticEquation(double a, double b, double c)
        {
            var d = b * b - 4 * a * c;
            if (d < 0)
            {
                return Array.Empty<double>();
            }

            if (d == 0)
            {
                var x = -b / 2 / a;
                return new[] { x };
            }

            var x1 = (-b - Math.Sqrt(d)) / 2 / a;
            var x2 = (-b + Math.Sqrt(d)) / 2 / a;
            return new[] { x1, x2 };
        }

        public static bool IsIntegral(double value) =>
                Math.Truncate(value) == value;

        public static long SmalestIntegerLargerThan(double value) =>
            IsIntegral(value) ? (long)value + 1 : (long)Math.Ceiling(value);

        public static long LargestIntegestSmallerThan(double value) =>
            IsIntegral(value) ? (long)value - 1 : (long)Math.Floor(value);

    }

    private static string TrimPrefix(string text, string prefix) =>
        text.Substring(prefix.Length);
}
