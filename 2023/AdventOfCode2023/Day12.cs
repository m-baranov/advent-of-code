using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day12
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
???.### 1,1,3
.??..??...?##. 1,1,3
?#?#?#?#?#?#?#? 1,3,1,6
????.#...#... 4,1,1
????.######..#####. 1,6,5
?###???????? 3,2,1
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/12/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var rows = input.Lines().Select(Row.Parse).ToList();

            var sum = rows.Select(CountArrangements).Sum();

            Console.WriteLine(sum);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var rows = input.Lines().Select(Row.Parse).ToList();

            var sum = rows
                .Select(row => row.Repeat(times: 5))
                .Select(CountArrangements)
                .Sum();

            Console.WriteLine(sum);
        }
    }

    private static long CountArrangements(Row row)
    {
        // required for part 2, without memoization it'll be too slow
        var memo = new Dictionary<(int index, int start), long>(); 

        var spans = ComputeSpans(row);
        return CountArrangements(row, spans, index: 0, start: 0, memo: memo);
    }

    private static IReadOnlyList<Span> ComputeSpans(Row row)
    {
        var spans = new List<Span>();

        var width = row.Cells.Length;

        for (var i = 0; i < row.Lengths.Count; i++)
        {
            var start = 0;
            for (var j = 0; j < i; j++)
            {
                start += row.Lengths[j] + 1;
            }

            var end = width;
            for (var j = i + 1; j < row.Lengths.Count; j++)
            {
                end -= row.Lengths[j] + 1;
            }
            end -= row.Lengths[i];

            spans.Add(new Span(start, end, row.Lengths[i]));
        }

        return spans;
    }

    private static long CountArrangements(
        Row row, 
        IReadOnlyList<Span> spans, 
        int index, 
        int start,
        Dictionary<(int index, int start), long> memo)
    {
        if (memo.TryGetValue((index, start), out var memoCount))
        {
            return memoCount;
        }

        if (index >= spans.Count)
        {
            if (AcceptableNoSpan(row, start, row.Cells.Length))
            {
                return 1;
            }
            return 0;
        }

        var span = spans[index];

        var sum = 0L;
        for (var i = Math.Max(start, span.Start); i <= span.End; i++)
        {
            var acceptable =
                AcceptableNoSpan(row, start, i) &&
                AcceptableSpan(row, i, span.Length);

            if (!acceptable)
            {
                continue;
            }

            var nextStart = i + span.Length + 1;
            sum += CountArrangements(row, spans, index + 1, nextStart, memo);
        }

        memo.Add((index, start), sum);
        return sum;
    }

    private static bool AcceptableNoSpan(Row row, int start, int end)
    {
        for (var i = start; i < end; i++)
        {
            var cell = row.Cells[i];
            if (cell == '#')
            {
                return false;
            }
        }
        return true;
    }

    private static bool AcceptableSpan(Row row, int start, int length)
    {
        for (var i = 0; i < length; i++)
        {
            var cell = row.Cells[start + i];
            if (cell == '.')
            {
                return false;
            }
        }

        if (start + length < row.Cells.Length)
        {
            return AcceptableNoSpan(row, start + length, start + length + 1);
        }

        return true;
    }

    private record Row(string Cells, IReadOnlyList<int> Lengths)
    {
        public static Row Parse(string text)
        {
            var parts = text.Split(' ');

            var cells = parts[0];
            var lengths = parts[1].Split(',').Select(int.Parse).ToList();

            return new Row(cells, lengths);
        }

        public Row Repeat(int times)
        {
            var repeatedCells = string.Join(
                "?",
                Enumerable.Range(0, times).Select(_ => Cells)
            );

            var repeatedLengths = Enumerable.Range(0, times)
                .SelectMany(_ => Lengths)
                .ToList();

            return new Row(repeatedCells, repeatedLengths);
        }
    }

    private record Span(int Start, int End, int Length);
}
