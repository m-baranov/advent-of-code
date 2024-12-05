using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2024;

static class Day04
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
MMMSXXMASM
MSAMXMSMSA
AMXSXMAAMM
MSAMASMSMX
XMASAMXAMM
XXAMMXXAMA
SMSMSASXSS
SAXAMASAAA
MAMMMXMMMM
MXMXAXMASX
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/4/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = new Grid(input.Lines().ToArray());

            var count = (
                from pos in grid.AllPositions()
                from dir in Position.Directions
                where IsXmasAt(grid, pos, dir)
                select true
            ).Count();

            Console.WriteLine(count);
        }

        private static bool IsXmasAt(Grid grid, Position pos, Position dir)
        {
            const string Word = "XMAS";

            var current = pos;
            foreach (var ch in Word)
            {
                if (grid.At(current) != ch)
                {
                    return false;
                }

                current = current.Add(dir);
            }

            return true;
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = new Grid(input.Lines().ToArray());

            var count = (
                from pos in grid.AllPositions()
                from corners in Corners.All
                where grid.At(pos) == 'A' && MatchesCornersAt(grid, pos, corners)
                select true
            ).Count();

            Console.WriteLine(count);
        }

        private static bool MatchesCornersAt(Grid grid, Position pos, Corners corners) =>
            corners.Items.All(i => grid.At(pos.Add(i.dir)) == i.ch);

        private record Corners(
            IReadOnlyList<(Position dir, char ch)> Items)
        {
            public static IReadOnlyList<Corners> All = new Corners[]
            {
                Create(topLeft: 'M', topRight: 'M', bottomLeft: 'S', bottomRight: 'S'),
                Create(topLeft: 'S', topRight: 'M', bottomLeft: 'S', bottomRight: 'M'),
                Create(topLeft: 'S', topRight: 'S', bottomLeft: 'M', bottomRight: 'M'),
                Create(topLeft: 'M', topRight: 'S', bottomLeft: 'M', bottomRight: 'S'),
            };

            public static Corners Create(
                char topLeft,
                char topRight,
                char bottomLeft,
                char bottomRight)
            {
                return new Corners(new[]
                {
                    (new Position(-1, -1), topLeft),
                    (new Position(-1, 1), topRight),
                    (new Position(1, -1), bottomLeft),
                    (new Position(1, 1), bottomRight),
                });
            }
        }
    }

    private record Position(int Row, int Col)
    {
        public static readonly IReadOnlyList<Position> Directions = new Position[]
        {
            new(Row: -1, Col: 0),
            new(Row: -1, Col: 1),
            new(Row: 0, Col: 1),
            new(Row: 1, Col: 1),
            new(Row: 1, Col: 0),
            new(Row: 1, Col: -1),
            new(Row: 0, Col: -1),
            new(Row: -1, Col: -1),
        };

        public Position Add(Position p) =>
            new(p.Row + this.Row, p.Col + this.Col);
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
            Contains(p) ? this.cells[p.Row][p.Col] : ' ';

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
    }
}
