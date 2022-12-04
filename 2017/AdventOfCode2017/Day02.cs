using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day02
    {
        public static class Inputs
        {
            public static readonly IInput Sample1 =
                Input.Literal(
                    "5 1 9 5",
                    "7 5 3",
                    "2 4 6 8"
                );

            public static readonly IInput Sample2 =
                Input.Literal(
                    "5 9 2 8",
                    "9 4 7 3",
                    "3 8 6 5"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/2/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                static IEnumerable<int> Parse(string line) =>
                    line
                        .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse);

                static (int min, int max) MinMaxOf(IEnumerable<int> numbers) =>
                    numbers
                        .Aggregate(
                            (min: int.MaxValue, max: int.MinValue), 
                            (acc, num) => (min: Math.Min(acc.min, num), max: Math.Max(acc.max, num))
                        );

                var rows = input.Lines();

                var sum = rows
                    .Select(Parse)
                    .Select(MinMaxOf)
                    .Select(p => p.max - p.min)
                    .Sum();

                Console.WriteLine(sum);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                static IReadOnlyList<int> Parse(string line) =>
                    line
                        .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();

                static IEnumerable<int> Between(int from, int to) =>
                    Enumerable.Range(from, to - from);

                static IEnumerable<(int a, int b)> AllPossiblePairs(IReadOnlyList<int> nums) =>
                    from ai in Between(0, nums.Count)
                    from bi in Between(ai + 1, nums.Count)
                    let a = nums[ai]
                    let b = nums[bi]
                    select (Math.Max(a, b), Math.Min(a, b));

                var rows = input.Lines();

                var sum = rows
                    .Select(Parse)
                    .Select(AllPossiblePairs)
                    .Select(ps => ps.First(p => p.a % p.b == 0))
                    .Select(p => p.a / p.b)
                    .Sum();

                Console.WriteLine(sum);
            }
        }
    }
}
