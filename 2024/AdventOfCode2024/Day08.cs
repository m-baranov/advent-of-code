using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks.Dataflow;

namespace AdventOfCode2024;

static class Day08
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
............
........0...
.....0......
.......0....
....0.......
......A.....
............
............
........A...
.........A..
............
............
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/8/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = new Grid(input.Lines().ToArray());

            var count = grid.UniqueFrequences()
                .SelectMany(freq => AntinodesOf(grid, freq))
                .Distinct()
                .Where(p => grid.Contains(p))
                .Count();

            Console.WriteLine(count);
        }

        private static IReadOnlyList<Position> AntinodesOf(Grid grid, char freq)
        {
            var positions = grid.PositionsOfFrequency(freq);

            return AllUniquePairs(positions)
                .SelectMany(p => AntinodesOf(p.first, p.second))
                .ToList();
        }

        private static IEnumerable<Position> AntinodesOf(Position a, Position b)
        {
            var delta = b.Sub(a);
            yield return b.Add(delta);
            yield return a.Sub(delta);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = new Grid(input.Lines().ToArray());

            var count = grid.UniqueFrequences()
                .SelectMany(freq => AntinodesOf(grid, freq))
                .Distinct()
                .Count();

            Console.WriteLine(count);
        }

        private static IReadOnlyList<Position> AntinodesOf(Grid grid, char freq)
        {
            var positions = grid.PositionsOfFrequency(freq);

            return AllUniquePairs(positions)
                .SelectMany(p => AntinodesOf(grid, p.first, p.second))
                .ToList();
        }

        private static IEnumerable<Position> AntinodesOf(Grid grid, Position a, Position b)
        {
            return Trace(grid, b, b.Sub(a))
                .Concat(Trace(grid, a, a.Sub(b)));
        }

        private static IEnumerable<Position> Trace(Grid grid, Position pos, Position delta)
        {
            var next = pos;
            while (grid.Contains(next))
            {
                yield return next;
                next = next.Add(delta);
            }
        }
    }

    private static IEnumerable<(T first, T second)> AllUniquePairs<T>(IReadOnlyList<T> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            for (var j = i + 1; j < items.Count; j++)
            {
                yield return (items[i], items[j]);
            }
        }
    }

    private record Position(int Row, int Col)
    {
        public Position Add(Position p) =>
            new(this.Row + p.Row, this.Col + p.Col);

        public Position Sub(Position p) =>
            new(this.Row - p.Row, this.Col - p.Col);
    }

    private class Grid
    {
        private readonly IReadOnlyList<string> cells;

        public Grid(IReadOnlyList<string> cells)
        {
            this.cells = cells;
        }

        public int Rows => this.cells.Count;
        public int Cols => this.cells[0].Length;

        public bool Contains(Position p) =>
            0 <= p.Row && p.Row < Rows &&
            0 <= p.Col && p.Col < Cols;

        public char At(Position p) =>
            this.cells[p.Row][p.Col];

        public IEnumerable<Position> AllPositions()
        {
            for (var row = 0; row < this.Rows; row++)
            {
                for (var col = 0; col < this.Cols; col++)
                {
                    yield return new Position(row, col);
                }
            }
        }

        public ISet<char> UniqueFrequences()
        {
            return this.AllPositions()
                .Select(p => this.At(p))
                .Where(ch => ch != '.')
                .ToHashSet();
        }

        public IReadOnlyList<Position> PositionsOfFrequency(char freq)
        {
            return this.AllPositions()
                .Where(p => this.At(p) == freq)
                .ToList();
        }
    }
}
