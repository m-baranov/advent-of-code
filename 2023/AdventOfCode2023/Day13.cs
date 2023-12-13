using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day13
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
#.##..##.
..#.##.#.
##......#
##......#
..#.##.#.
..##..##.
#.#.##.#.

#...##..#
#....#..#
..##..###
#####.##.
#####.##.
..##..###
#....#..#
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/13/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var grids = Grid.ParseMany(input.Lines());

            var sum = grids
                .Select(ReflectionSummary)
                .Sum();

            Console.WriteLine(sum);
        }

        private static int ReflectionSummary(Grid grid)
        {
            var vertical = VerticalReflectionCols(grid).Select(col => col + 1);

            var horizontal = HorizontalReflectionRows(grid).Select(row => (row + 1) * 100);

            return vertical.Concat(horizontal).Sum();
        }

        private static IEnumerable<int> VerticalReflectionCols(Grid grid) =>
            Enumerable.Range(start: 0, count: grid.Cols - 1)
                .Where(col => IsVerticalReflectionAt(grid, col));

        private static IEnumerable<int> HorizontalReflectionRows(Grid grid) =>
            Enumerable.Range(start: 0, count: grid.Rows - 1)
                .Where(row => IsHorizontalReflectionAt(grid, row));

        private static bool IsVerticalReflectionAt(Grid grid, int col)
        {
            var left = col;
            var right = col + 1;

            var size = Math.Min(left + 1, grid.Cols - right);
            for (var i = 0; i < size; i++)
            {
                if (!ColsEqual(grid, left, right))
                {
                    return false;
                }

                left--;
                right++;
            }

            return true;
        }

        private static bool IsHorizontalReflectionAt(Grid grid, int row)
        {
            var left = row;
            var right = row + 1;

            var size = Math.Min(left + 1, grid.Rows - right);
            for (var i = 0; i < size; i++)
            {
                if (!RowsEqual(grid, left, right))
                {
                    return false;
                }

                left--;
                right++;
            }

            return true;
        }

        private static bool ColsEqual(Grid grid, int index1, int index2) =>
            grid.Column(index1)
                .Zip(grid.Column(index2), (c1, c2) => c1 == c2)
                .All(eq => eq);

        private static bool RowsEqual(Grid grid, int index1, int index2) =>
            grid.Row(index1)
                .Zip(grid.Row(index2), (c1, c2) => c1 == c2)
                .All(eq => eq);
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var grids = Grid.ParseMany(input.Lines());

            var sum = grids
                .Select(ReflectionSummary)
                .Sum();

            Console.WriteLine(sum);
        }

        private static int ReflectionSummary(Grid grid)
        {
            var vertical = VerticalReflectionCols(grid).Select(col => col + 1);

            var horizontal = HorizontalReflectionRows(grid).Select(row => (row + 1) * 100);

            return vertical.Concat(horizontal).Sum();
        }

        private static IEnumerable<int> VerticalReflectionCols(Grid grid) =>
            Enumerable.Range(start: 0, count: grid.Cols - 1)
                .Where(col => IsVerticalReflectionAt(grid, col));

        private static IEnumerable<int> HorizontalReflectionRows(Grid grid) =>
            Enumerable.Range(start: 0, count: grid.Rows - 1)
                .Where(row => IsHorizontalReflectionAt(grid, row));

        private static bool IsVerticalReflectionAt(Grid grid, int col)
        {
            var left = col;
            var right = col + 1;

            var diff = 0;
            var size = Math.Min(left + 1, grid.Cols - right);
            for (var i = 0; i < size; i++)
            {
                diff += ColsDiff(grid, left, right);
                if (diff > 1)
                {
                    break;
                }

                left--;
                right++;
            }

            return diff == 1;
        }

        private static bool IsHorizontalReflectionAt(Grid grid, int row)
        {
            var left = row;
            var right = row + 1;

            var diff = 0;
            var size = Math.Min(left + 1, grid.Rows - right);
            for (var i = 0; i < size; i++)
            {
                diff += RowsDiff(grid, left, right);
                if (diff > 1)
                {
                    break;
                }

                left--;
                right++;
            }

            return diff == 1;
        }

        private static int ColsDiff(Grid grid, int index1, int index2)
        {
            var pairs = grid.Column(index1)
                .Zip(grid.Column(index2), (c1, c2) => (c1, c2));
            return CountDiffs(pairs);
        }

        private static int RowsDiff(Grid grid, int index1, int index2)
        {
            var pairs = grid.Row(index1)
                .Zip(grid.Row(index2), (c1, c2) => (c1, c2));
            return CountDiffs(pairs);
        }

        private static int CountDiffs(IEnumerable<(char c1, char c2)> pairs)
        {
            var diffs = 0;
            foreach (var (c1, c2) in pairs)
            {
                if (c1 != c2)
                {
                    diffs++;

                    if (diffs > 1)
                    {
                        break;
                    }
                }
            }
            return diffs;
        }
    }

    private sealed class Grid
    {
        public static IReadOnlyList<Grid> ParseMany(IEnumerable<string> lines)
        {
            return lines
                .SplitByEmptyLine()
                .Select(cells => new Grid(cells))
                .ToList();
        }

        private readonly IReadOnlyList<string> cells;

        public Grid(IReadOnlyList<string> cells)
        {
            this.cells = cells;
        }

        public int Rows => this.cells.Count;
        public int Cols => this.cells[0].Length;
        
        public IEnumerable<char> Row(int row)
        {
            for (var col = 0; col < Cols; col++)
            {
                yield return this.cells[row][col];
            }
        }

        public IEnumerable<char> Column(int col)
        {
            for (var row = 0; row < Rows; row++)
            {
                yield return this.cells[row][col];
            }
        }
    }
}
