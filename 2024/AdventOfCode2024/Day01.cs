using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2024;

static class Day01
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
3   4
4   3
2   5
1   3
3   9
3   3
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/1/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var pairs = input.Lines()
                .Select(ParsePair)
                .ToList();

            var lefts = pairs
                .Select(p => p.left)
                .OrderBy(i => i);

            var rights = pairs
                .Select(p => p.right)
                .OrderBy(i => i);

            var sum = lefts
                .Zip(rights, (l, r) => (long)Math.Abs(l - r))
                .Sum();

            Console.WriteLine(sum);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var pairs = input.Lines()
                .Select(ParsePair)
                .ToList();

            var leftCounts = CountAppearances(
                pairs.Select(p => p.left));

            var rightCounts = CountAppearances(
                pairs.Select(p => p.right));

            var sum = leftCounts
                .Select(p =>
                {
                    var num = p.Key;

                    var leftCount = p.Value;
                    var rightCount = TryGetCount(rightCounts, num);

                    return (long)num * leftCount * rightCount;
                })
                .Sum();

            Console.WriteLine(sum);
        }

        private static IReadOnlyDictionary<int, int> CountAppearances(IEnumerable<int> nums)
        {
            var counts = new Dictionary<int, int>();

            foreach (var num in nums)
            {
                var count = TryGetCount(counts, num);
                counts[num] = count + 1;
            }

            return counts;
        }

        private static int TryGetCount(IReadOnlyDictionary<int, int> counts, int num) =>
            counts.TryGetValue(num, out var count)
                ? count
                : 0;
    }

    private static (int left, int right) ParsePair(string line)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return (int.Parse(parts[0]), int.Parse(parts[1]));
    }
}
