using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day11
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
...#......
.......#..
#.........
..........
......#...
.#........
.........#
..........
.......#..
#...#.....
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/11/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var sum = Solve(grid, expansionMultiplier: 2);

            Console.WriteLine(sum);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var sum = Solve(grid, expansionMultiplier: 1_000_000);

            Console.WriteLine(sum);
        }
    }

    private static long Solve(Grid grid, int expansionMultiplier)
    {
        var emptyRows = grid.FindEmptyRows();
        var emptyCols = grid.FindEmptyCols();
        var positions = grid.FindGalaxyPositions();

        return positions
            .AllPossiblePairs()
            .Select(pair =>
                (long)Position.ManhattanDistance(pair.a, pair.b) +
                (expansionMultiplier - 1) * emptyRows.CountBetween(pair.a.Row, pair.b.Row) +
                (expansionMultiplier - 1) * emptyCols.CountBetween(pair.a.Col, pair.b.Col)
            )
            .Sum();
    }

    private static IEnumerable<(T a, T b)> AllPossiblePairs<T>(this IReadOnlyList<T> items)
    {
        for (var i = 0; i < items.Count - 1; i++)
        {
            for (var j = i + 1; j < items.Count; j++)
            {
                yield return (items[i], items[j]);
            }
        }
    }

    private record Position(int Row, int Col)
    {
        public static int ManhattanDistance(Position a, Position b) =>
            Math.Abs(a.Row - b.Row) + Math.Abs(a.Col - b.Col);
    }

    private sealed class OrderedIndexList
    {
        private readonly List<int> indexes;

        public OrderedIndexList(List<int> indexes)
        {
            this.indexes = indexes;
        }

        public int CountBetween(int a, int b)
        {
            static int IndexOf(List<int> items, int value)
            {
                var result = items.BinarySearch(value);
                return result >= 0 ? result : ~result;
            }

            var min = Math.Min(a, b);
            var max = Math.Max(a, b);

            var count = IndexOf(this.indexes, max) - IndexOf(this.indexes, min);
            return count < 0 ? 0 : count;
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

        public OrderedIndexList FindEmptyRows()
        {
            var rows = Enumerable.Range(0, Rows)
                .Where(row => IsEmpty(Row(row)))
                .ToList();
            return new OrderedIndexList(rows);
        }

        public OrderedIndexList FindEmptyCols()
        {
            var cols = Enumerable.Range(0, Cols)
                .Where(col => IsEmpty(Column(col)))
                .ToList();
            return new OrderedIndexList(cols);
        }

        private static bool IsEmpty(IEnumerable<char> cells) =>
            cells.All(cell => cell == '.');

        public IEnumerable<Position> FindPositions(Func<char, bool> predicate)
        {
            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Cols; col++)
                {
                    if (predicate(this.cells[row][col]))
                    {
                        yield return new Position(row, col);
                    }
                }
            }
        }

        public IReadOnlyList<Position> FindGalaxyPositions() =>
            FindPositions(cell => cell == '#').ToList();
    }
}
