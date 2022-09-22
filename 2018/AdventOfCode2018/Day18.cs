using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day18
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    ".#.#...|#.",
                    ".....#|##|",
                    ".|..|...#.",
                    "..|#.....#",
                    "#.#|||#|#|",
                    "...#.||...",
                    ".|....|...",
                    "||...#|.#|",
                    "|.||||..|.",
                    "...#.|..|."
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/18/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var initialGrid = Grid.Parse(input.Lines().ToList());

                var finalGrid = Grid.Simulate(initialGrid, totalMinutes: 10);

                Console.WriteLine($"Answer = {finalGrid.TotalResourceValue()}");
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var initialGrid = Grid.Parse(input.Lines().ToList());

                var goalMinute = 1_000_000_000;

                var (duplicateMinute, duplicates) = Grid.SimulateUntilRepeating(initialGrid);

                var duplicateIndex = (goalMinute - duplicateMinute - 1) % duplicates.Count;
                var answer = duplicates[duplicateIndex].TotalResourceValue();
                Console.Write($"Answer={answer}");
            }
        }

        private class Grid
        {
            public static Grid Parse(IReadOnlyList<string> lines)
            {
                static Cell CellOf(char ch) =>
                    ch switch
                    {
                        '|' => Cell.Trees,
                        '#' => Cell.Lumber,
                        '.' or _ => Cell.Open
                    };

                var rows = lines.Count;
                var cols = lines[0].Length;

                var cells = new Cell[rows, cols];

                var row = 0;
                foreach (var line in lines)
                {
                    var col = 0;
                    foreach (var ch in line)
                    {
                        cells[row, col] = CellOf(ch);

                        col++;
                    }
                    row++;
                }

                return new Grid(cells);
            }

            private static Cell NextCell(Cell current, IReadOnlyList<Cell> adjacent)
            {
                if (current == Cell.Open)
                {
                    return adjacent.Count(c => c == Cell.Trees) >= 3 ? Cell.Trees : Cell.Open;
                }
                if (current == Cell.Trees)
                {
                    return adjacent.Count(c => c == Cell.Lumber) >= 3 ? Cell.Lumber : Cell.Trees;
                }
                if (current == Cell.Lumber)
                {
                    var hasLumber = adjacent.Any(c => c == Cell.Lumber);
                    var hasTree = adjacent.Any(c => c == Cell.Trees);
                    return hasLumber && hasTree ? Cell.Lumber : Cell.Open;
                }
                return current;
            }

            private static void PopulateNext(Grid current, Grid next)
            {
                for (var row = 0; row < current.Rows; row++)
                {
                    for (var col = 0; col < current.Cols; col++)
                    {
                        var cell = current.At(row, col);
                        var adjacent = current.AdjacentCells(row, col);
                        next.Set(row, col, NextCell(cell, adjacent));
                    }
                }
            }

            public static Grid Simulate(Grid initialGrid, int totalMinutes)
            {
                var gridA = initialGrid;
                var gridB = new Grid(gridA.Rows, gridA.Cols);

                for (var minute = 0; minute < totalMinutes; minute++)
                {
                    PopulateNext(gridA, gridB);
                    (gridA, gridB) = (gridB, gridA);
                }

                return gridA;
            }

            public static (int duplicateMinute, IReadOnlyList<Grid> duplicates) SimulateUntilRepeating(
                Grid initialGrid)
            {
                var gridA = initialGrid;
                var gridB = new Grid(gridA.Rows, gridA.Cols);
                var seen = new List<(Grid grid, int minute)>();

                var minute = 0;
                var previousMinute = 0;
                while (true)
                {
                    PopulateNext(gridA, gridB);
                    (gridA, gridB) = (gridB, gridA);

                    var duplicateIndex = seen.IndexOf(p => p.grid.IsSame(gridA));
                    if (duplicateIndex >= 0)
                    {
                        previousMinute = seen[duplicateIndex].minute;
                        break;
                    }
                    
                    seen.Add((gridA.Clone(), minute));
                    minute++;
                }

                // starts repeating at time=minute, previously seen geid at time=previousMinute

                var duplicates = new List<Grid>() { gridA.Clone() };

                for (var i = 0; i < minute - previousMinute - 1; i++)
                {
                    PopulateNext(gridA, gridB);
                    (gridA, gridB) = (gridB, gridA);

                    duplicates.Add(gridA.Clone());
                }

                return (minute, duplicates);
            }


            private readonly Cell[,] cells;

            public Grid(int rows, int cols) : this(new Cell[rows, cols]) { }

            public Grid(Cell[,] cells)
            {
                this.cells = cells;
            }

            public int Rows => cells.GetLength(0);
            public int Cols => cells.GetLength(1);

            public Cell At(int row, int col) => this.cells[row, col];

            public void Set(int row, int col, Cell cell) => this.cells[row, col] = cell;

            public int TotalResourceValue() => Count(Cell.Trees) * Count(Cell.Lumber);

            public int Count(Cell type)
            {
                var count = 0;

                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Cols; col++)
                    {
                        if (this.cells[row, col] == type)
                        {
                            count++;
                        }
                    }
                }

                return count;
            } 

            public bool InBounds(int row, int col) =>
                0 <= row && row < Rows &&
                0 <= col && col < Cols;

            private static readonly IReadOnlyList<(int r, int c)> AdjacentDeltas = 
                new[]
                {
                    (-1, -1),
                    (-1, 0),
                    (-1, 1),
                    (0, 1),
                    (1, 1),
                    (1, 0),
                    (1, -1),
                    (0, -1)
                };

            public IReadOnlyList<Cell> AdjacentCells(int row, int col) =>
                AdjacentDeltas
                    .Select(d => (r: row + d.r, c: col + d.c))
                    .Where(p => InBounds(p.r, p.c))
                    .Select(p => At(p.r, p.c))
                    .ToList();

            public void Draw()
            {
                static char CellToChar(Cell cell) =>
                    cell switch
                    {
                        Cell.Trees => '|',
                        Cell.Lumber => '#',
                        Cell.Open or _ => '.'
                    };

                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Cols; col++)
                    {
                        Console.Write(CellToChar(this.cells[row, col]));
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.ReadLine();
            }

            public Grid Clone()
            {
                var clone = new Cell[Rows, Cols];
                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Cols; col++)
                    {
                        clone[row, col] = this.cells[row, col];
                    }
                }
                return new Grid(clone);
            }

            public bool IsSame(Grid other)
            {
                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Cols; col++)
                    {
                        if (other.cells[row, col] != this.cells[row, col])
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        
        private enum Cell { Open, Trees, Lumber }
    }
}
