using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;
static class Day09
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
0 3 6 9 12 15
1 3 6 10 15 21
10 13 16 21 30 45
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/9/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var sequences = ParseMany(input.Lines());

            var sum = sequences.Select(Predict).Sum();

            Console.WriteLine(sum);
        }

        private static long Predict(IReadOnlyList<long> sequence)
        {
            var diffs = DiffAll(sequence);

            var index = diffs.Count - 1;
            var predicted = 0L;
            while (index > 0)
            {
                predicted = diffs[index - 1][^1] + predicted;
                index--;
            }

            return predicted;
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var sequences = ParseMany(input.Lines());

            var sum = sequences.Select(Predict).Sum();

            Console.WriteLine(sum);
        }

        private static long Predict(IReadOnlyList<long> sequence)
        {
            var diffs = DiffAll(sequence);

            var index = diffs.Count - 1;
            var predicted = 0L;
            while (index > 0)
            {
                predicted = diffs[index - 1][0] - predicted;
                index--;
            }

            return predicted;
        }
    }

    private static IReadOnlyList<IReadOnlyList<long>> ParseMany(IEnumerable<string> lines) =>
        lines.Select(ParseOne).ToList();

    private static IReadOnlyList<long> ParseOne(string text) =>
        text.Split(' ').Select(long.Parse).ToList();

    private static (IReadOnlyList<long>, bool) DiffOnce(IReadOnlyList<long> sequence)
    {
        var diff = new List<long>();
        var allZeroes = true;

        for (var i = 0; i < sequence.Count - 1; i++)
        {
            var value = sequence[i + 1] - sequence[i];
            diff.Add(value);

            allZeroes = allZeroes && value == 0;
        }

        return (diff, allZeroes);
    }

    private static IReadOnlyList<IReadOnlyList<long>> DiffAll(IReadOnlyList<long> sequence)
    {
        var diffs = new List<IReadOnlyList<long>>() { sequence };

        while (true)
        {
            var (diff, allZeroes) = DiffOnce(diffs[^1]);
            diffs.Add(diff);

            if (allZeroes)
            {
                break;
            }
        }

        return diffs;
    }
}
