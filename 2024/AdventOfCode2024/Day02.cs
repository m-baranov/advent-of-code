using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2024;

static class Day02
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
7 6 4 2 1
1 2 7 8 9
9 7 6 2 1
1 3 2 4 5
8 6 4 4 1
1 3 6 7 9
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/2/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var lines = input.Lines()
                .Select(ParseLine)
                .ToList();

            var count = lines
                .Where(IsSafe)
                .Count();

            Console.WriteLine(count);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            static IEnumerable<int> SkipNth(IEnumerable<int> nums, int n) =>
                nums.Where((_, i) => i != n);

            static IEnumerable<IEnumerable<int>> AllVariants(IReadOnlyList<int> nums) =>
                Enumerable.Range(0, nums.Count)
                    .Select(i => SkipNth(nums, i))
                    .Prepend(nums);

            var lines = input.Lines()
                .Select(ParseLine)
                .ToList();

            var count = lines
                .Where(line => AllVariants(line).Any(IsSafe))
                .Count();

            Console.WriteLine(count);
        }
    }

    private static IReadOnlyList<int> ParseLine(string line) =>
        line.Split(' ').Select(int.Parse).ToList();

    private static IEnumerable<int> Diff(IEnumerable<int> nums) =>
        nums.Zip(nums.Skip(1), (x, y) => x - y);

    private static bool AllDiffsWithinRange(IEnumerable<int> nums, int min, int max) =>
        Diff(nums).All(d => min <= d && d <= max);

    private static bool IsSafe(IEnumerable<int> nums) =>
        AllDiffsWithinRange(nums, min: 1, max: 3) ||
        AllDiffsWithinRange(nums, min: -3, max: -1);
}
