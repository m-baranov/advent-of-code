using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day07
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal("16,1,2,0,4,2,7,1,2,14");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/7/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var positions = input.Lines().First().Split(',').Select(int.Parse).ToList();

                var answer = positions.Range().Enumerate().Min(n => CostTo(positions, n));

                Console.WriteLine(answer);
            }

            private long CostTo(IReadOnlyList<int> positions, int target) =>
                positions.Select(pos => Math.Abs(target - pos)).Sum();
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var positions = input.Lines().First().Split(',').Select(int.Parse).ToList();

                var answer = positions.Range().Enumerate().Min(n => CostTo(positions, n));

                Console.WriteLine(answer);
            }

            private long CostTo(IReadOnlyList<int> positions, int target) =>
                positions.Select(pos => SumOfN(Math.Abs(target - pos))).Sum();

            private long SumOfN(long n) => n * (n + 1) / 2;
        }
    }

    public static class Day07Extensions
    {
        public static (int min, int max) Range(this IEnumerable<int> numbers) => 
            (min: numbers.Min(), max: numbers.Max());

        public static IEnumerable<int> Enumerate(this (int min, int max) range) =>
            Enumerable.Range(range.min, count: range.max - range.min + 1);
    }
}
