using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdventOfCode2023;

static class Day03
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
467..114..
...*......
..35..633.
......#...
617*......
.....+.58.
..592.....
......755.
...$.*....
.664.598..
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/3/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var spans = FindNumberSpans(grid);

            var sum = spans
                .Where(span => IsAdjacentToSymbol(span, grid))
                .Select(span => span.ToNumber(grid))
                .Sum();

            Console.WriteLine(sum);
        }

        private static IReadOnlyList<NumberSpan> FindNumberSpans(Grid grid)
        {
            var spans = new List<NumberSpan>();

            for (var r = 0; r < grid.Rows; r++)
            {
                var start = -1;

                for (var c = 0; c < grid.Cols; c++)
                {
                    var ch = grid.At(r, c);

                    if (Cell.IsDigit(ch))
                    {
                        if (start < 0)
                        {
                            start = c;
                        }
                    }
                    else
                    {
                        if (start >= 0)
                        {
                            spans.Add(new NumberSpan(r, start, c - 1));
                            start = -1;
                        }
                    }
                }

                if (start >= 0)
                {
                    spans.Add(new NumberSpan(r, start, grid.Cols - 1));
                }
            }

            return spans;
        }

        private static bool IsAdjacentToSymbol(NumberSpan span, Grid grid)
        {
            static IEnumerable<(int r, int c)> CoordinatesAround(NumberSpan span)
            {
                yield return (span.Row, span.ColStart - 1);

                for (var c = span.ColStart - 1; c <= span.ColEnd + 1; c++)
                {
                    yield return (span.Row - 1, c);
                }

                yield return (span.Row, span.ColEnd + 1);

                for (var c = span.ColStart - 1; c <= span.ColEnd + 1; c++)
                {
                    yield return (span.Row + 1, c);
                }
            }

            return CoordinatesAround(span)
                .Where(c => grid.IsInBounds(c.r, c.c))
                .Select(c => grid.At(c.r, c.c))
                .Any(Cell.IsSymbol);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var gears = FindGears(grid);

            var sum = gears
                .Select(gear => FindAdjacentNumberSpans(grid, gear.r, gear.c))
                .Where(spans => spans.Count == 2)
                .Select(spans => spans[0].ToNumber(grid) * spans[1].ToNumber(grid))
                .Sum();

            Console.WriteLine(sum);
        }

        private static IReadOnlyList<(int r, int c)> FindGears(Grid grid)
        {
            var gears = new List<(int r, int c)>();

            for (var r = 0; r < grid.Rows; r++)
            {
                for (var c = 0; c < grid.Cols; c++)
                {
                    var ch = grid.At(r, c);

                    if (Cell.IsGear(ch))
                    {
                        gears.Add((r, c));
                    }
                }
            }

            return gears;
        }

        private static IReadOnlyList<NumberSpan> FindAdjacentNumberSpans(Grid grid, int row, int col)
        {
            static IEnumerable<(int r, int c)> CoordinatesAround(int row, int col)
            {
                yield return (row, col - 1);
                yield return (row - 1, col - 1);
                yield return (row - 1, col);
                yield return (row - 1, col + 1);
                yield return (row, col + 1);
                yield return (row + 1, col + 1);
                yield return (row + 1, col);
                yield return (row + 1, col - 1);
            }

            return CoordinatesAround(row, col)
                .Select(c => TryFindNumberSpan(grid, c.r, c.c))
                .Where(span => span is not null)
                .Distinct()
                .Select(span => span!)
                .ToList();
        }

        private static NumberSpan? TryFindNumberSpan(Grid grid, int row, int col)
        {
            if (!grid.IsInBounds(row, col))
            {
                return null;
            }

            if (!Cell.IsDigit(grid.At(row, col)))
            {
                return null;
            }

            var start = col;
            while (0 <= start && Cell.IsDigit(grid.At(row, start)))
            {
                start--;
            }

            var end = col;
            while (end < grid.Cols && Cell.IsDigit(grid.At(row, end)))
            {
                end++;
            }

            return new NumberSpan(row, start + 1, end - 1);
        }
    }

    private sealed class Grid
    {
        public static Grid Parse(IEnumerable<string> lines) =>
            new(lines.ToList());

        private readonly IReadOnlyList<string> cells;

        public Grid(IReadOnlyList<string> cells)
        {
            this.cells = cells;
        }

        public int Rows => this.cells.Count;
        public int Cols => this.cells[0].Length;

        public bool IsInBounds(int r, int c) =>
            0 <= r && r < Rows &&
            0 <= c && c < Cols;

        public char At(int r, int c) =>
            this.cells[r][c];
    }

    private static class Cell
    {
        public static bool IsDigit(char ch) => '0' <= ch && ch <= '9';

        public static bool IsSymbol(char ch) => ch != '.' && !IsDigit(ch);

        public static bool IsGear(char ch) => ch == '*';
    }

    private record NumberSpan(int Row, int ColStart, int ColEnd)
    {
        public long ToNumber(Grid grid)
        {
            var builder = new StringBuilder();

            for (var c = ColStart; c <= ColEnd; c++)
            {
                builder.Append(grid.At(Row, c));
            }

            return int.Parse(builder.ToString());
        }
    }
}
