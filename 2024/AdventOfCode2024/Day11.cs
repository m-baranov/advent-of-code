using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;

namespace AdventOfCode2024;

static class Day11
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
125 17
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/11/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var nums = Parse(input.Lines().First());

            var sum = Count(nums, targetBlinks: 25);

            Console.WriteLine(sum);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var nums = Parse(input.Lines().First());

            var sum = Count(nums, targetBlinks: 75);

            Console.WriteLine(sum);
        }
    }

    private static long Count(IReadOnlyList<long> nums, int targetBlinks)
    {
        var sum = 0L;
        foreach (var num in nums)
        {
            sum += Count(num, targetBlinks);
        }
        return sum;
    }

    private static long Count(long num, int targetBlinks)
    {
        Dictionary<(long num, int blink), long> memo = new();

        long Recurse(long num, int blink)
        {
            if (blink >= targetBlinks)
            {
                return 1;
            }

            if (num == 0)
            {
                return RecurseMemoized(1, blink + 1);
            }

            var text = num.ToString();
            var (div, rem) = Math.DivRem(text.Length, 2);

            if (rem == 1)
            {
                return RecurseMemoized(num * 2024, blink + 1);
            }

            var left = text.Substring(0, div);
            var right = text.Substring(div);

            return RecurseMemoized(long.Parse(left), blink + 1) +
                RecurseMemoized(long.Parse(right), blink + 1);
        }

        long RecurseMemoized(long num, int blink)
        {
            if (memo.TryGetValue((num, blink), out var cached))
            {
                return cached;
            }

            var count = Recurse(num, blink);

            memo.Add((num, blink), count);
            return count;
        }

        return RecurseMemoized(num, blink: 0);
    }

    private static IReadOnlyList<long> Parse(string text) =>
        text.Split(' ').Select(long.Parse).ToArray();
}
