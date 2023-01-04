using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day23
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "....#..",
                    "..###.#",
                    "#...#.#",
                    ".#...##",
                    "#.###..",
                    "##.#.##",
                    ".#..#.."
                );

            public static readonly IInput Sample0 =
                Input.Literal(
                    "##",
                    "#.",
                    "..",
                    "##"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/23/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = Grid.Parse(input.Lines().ToList());
                var rules = new RuleList();

                for (var round = 0; round < 10; round++)
                {
                    var result = grid.Round(rules);

                    rules.Advance();

                    if (result.touchedEdge)
                    {
                        grid = grid.Extend();
                    }
                }

                Console.WriteLine(grid.EmptyTilesInMinElfRectangle());
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = Grid.Parse(input.Lines().ToList());
                var rules = new RuleList();

                var round = 0;
                while (true)
                {
                    var result = grid.Round(rules);
                    round++;

                    if (!result.changed)
                    {
                        break;
                    }

                    rules.Advance();

                    if (result.touchedEdge)
                    {
                        grid = grid.Extend();
                    }
                }

                Console.WriteLine(round);
            }
        }

        private record class Direction(int DeltaRow, int DeltaCol)
        {
            public static readonly Direction N = new(-1, 0);
            public static readonly Direction S = new(1, 0);
            public static readonly Direction W = new(0, -1);
            public static readonly Direction E = new(0, 1);
            public static readonly Direction NW = new(-1, -1);
            public static readonly Direction NE = new(-1, 1);
            public static readonly Direction SW = new(1, -1);
            public static readonly Direction SE = new(1, 1);

            public static readonly IReadOnlyList<Direction> All = new[] { N, S, W, E, NW, NE, SW, SE };
        }

        private record Position(int Row, int Col)
        {
            public Position Move(Direction direction) =>
                new(Row + direction.DeltaRow, Col + direction.DeltaCol);
        }

        private record Rule(IReadOnlyList<Direction> LookAt, Direction Move)
        {
            public static readonly Rule N = new(new[] { Direction.N, Direction.NE, Direction.NW }, Direction.N);
            public static readonly Rule S = new(new[] { Direction.S, Direction.SE, Direction.SW }, Direction.S);
            public static readonly Rule W = new(new[] { Direction.W, Direction.NW, Direction.SW }, Direction.W);
            public static readonly Rule E = new(new[] { Direction.E, Direction.NE, Direction.SE }, Direction.E);
        }

        private class RuleList
        {
            private readonly IReadOnlyList<Rule> Rules = new[] { Rule.N, Rule.S, Rule.W, Rule.E };

            private int start;

            public RuleList()
            {
                this.start = 0;
            }

            public void Advance()
            {
                this.start = (this.start + 1) % this.Rules.Count;
            }

            public IEnumerable<Rule> Items()
            {
                for (var i = 0; i < this.Rules.Count; i++)
                {
                    var index = (this.start + i) % this.Rules.Count;
                    yield return this.Rules[index];
                }
            }
        }

        private sealed class Grid
        {
            public static Grid Parse(IReadOnlyList<string> lines)
            {
                var rows = lines.Count;
                var cols = lines[0].Length;

                var grid = CreatedExtendedGrid(rows, cols);

                for (var r = 0; r < rows; r++)
                {
                    var line = lines[r];
                    for (var c = 0; c < cols; c++)
                    {
                        var ch = line[c];
                        grid.Set(new Position(r, c), ch == '#' ? '#' : default);
                    }
                }

                return grid;
            }

            private static Grid CreatedExtendedGrid(int rows, int cols)
            {
                var minRow = -rows;
                var maxRow = 2 * rows - 1;

                var minCol = -cols;
                var maxCol = 2 * cols - 1;

                return new Grid(minRow, minCol, maxRow, maxCol);
            }

            private readonly char[,] cells;
            private readonly int minRow;
            private readonly int maxRow;
            private readonly int minCol;
            private readonly int maxCol;
            private readonly int rows;
            private readonly int cols;

            public Grid(int minRow, int minCol, int maxRow, int maxCol)
            {
                this.minRow = minRow;
                this.maxRow = maxRow;
                this.rows = this.maxRow - this.minRow + 1;

                this.minCol = minCol;
                this.maxCol = maxCol;
                this.cols = this.maxCol - this.minCol + 1;

                this.cells = new char[this.rows, this.cols]; 
            }

            public char At(Position p)
            {
                var row = p.Row - this.minRow;
                var col = p.Col - this.minCol;
                return this.cells[row, col];
            }

            public void Set(Position p, char value)
            {
                var row = p.Row - this.minRow;
                var col = p.Col - this.minCol;
                this.cells[row, col] = value;
            }

            public (bool touchedEdge, bool changed) Round(RuleList rules)
            {
                bool HasElfsAround(Position position, IEnumerable<Direction> directions) =>
                    directions.Select(position.Move).Select(At).Any(ch => ch == '#');

                IReadOnlyList<(Position current, Position next)> GenerateProposals()
                {
                    var proposals = new List<(Position current, Position next)>();

                    foreach (var position in PositionsOf('#'))
                    {
                        if (!HasElfsAround(position, Direction.All))
                        {
                            continue;
                        }

                        var rule = rules.Items().FirstOrDefault(r => !HasElfsAround(position, r.LookAt));
                        if (rule == null)
                        {
                            continue;
                        }

                        proposals.Add((position, position.Move(rule.Move)));
                    }

                    return proposals;
                }

                bool IsAtEdge(Position position) =>
                    this.minRow == position.Row || (this.minRow + 1) == position.Row ||
                    this.maxRow == position.Row || (this.maxRow - 1) == position.Row ||
                    this.minCol == position.Col || (this.minCol + 1) == position.Col ||
                    this.maxCol == position.Col || (this.maxCol - 1) == position.Col;

                bool ApplyProposals(IReadOnlyList<(Position current, Position next)> proposals)
                {
                    var touchedEdge = false;

                    foreach (var (current, next) in proposals)
                    {
                        Set(current, default);
                        Set(next, '#');

                        if (IsAtEdge(next))
                        {
                            touchedEdge = true;
                        }
                    }

                    return touchedEdge;
                }

                var proposals = GenerateProposals();

                var validProposals = proposals
                    .GroupBy(p => p.next)
                    .Where(g => g.Count() == 1)
                    .Select(g => g.First())
                    .ToList();

                if (validProposals.Count == 0)
                {
                    return (touchedEdge: false, changed: false);
                }

                var touchedEdge = ApplyProposals(validProposals);
                return (touchedEdge, changed: true);
            }

            public Grid Extend()
            {
                var newGrid = CreatedExtendedGrid(this.rows, this.cols);

                for (var r = this.minRow; r <= this.maxRow; r++)
                {
                    for (var c = this.minCol; c <= this.maxCol; c++)
                    {
                        var p = new Position(r, c);
                        newGrid.Set(p, this.At(p));
                    }
                }

                return newGrid;
            }

            public int EmptyTilesInMinElfRectangle()
            {
                static (int min, int max) MinMax(IEnumerable<int> numbers) =>
                    numbers.Aggregate(
                        (min: int.MaxValue, max: int.MinValue),
                        (acc, num) => (min: Math.Min(acc.min, num), max: Math.Max(acc.max, num))
                    );

                var positions = PositionsOf('#').ToList();

                var (minRow, maxRow) = MinMax(positions.Select(p => p.Row));
                var (minCol, maxCol) = MinMax(positions.Select(p => p.Col));

                var rows = maxRow - minRow + 1;
                var cols = maxCol - minCol + 1;

                return rows * cols - positions.Count;
            }

            private IEnumerable<Position> PositionsOf(char value)
            {
                for (var r = this.minRow; r <= this.maxRow; r++)
                {
                    for (var c = this.minCol; c <= this.maxCol; c++)
                    {
                        var p = new Position(r, c);

                        if (this.At(p) == value)
                        {
                            yield return p;
                        }
                    }
                }
            }

            public void Draw()
            {
                for (var r = this.minRow; r <= this.maxRow; r++)
                {
                    for (var c = this.minCol; c <= this.maxCol; c++)
                    {
                        var p = new Position(r, c);
                        var ch = this.At(p);
                        Console.Write(ch == default ? '.' : ch);
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
        }
    }
}
