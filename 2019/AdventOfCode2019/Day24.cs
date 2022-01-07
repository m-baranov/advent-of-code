using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdventOfCode2019
{
    static class Day24
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "....#",
                "#..#.",
                "#..##",
                "..#..",
                "#...."
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/24/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = Grid.Parse(input.Lines());

                var seenGrids = new HashSet<Grid>() { grid };

                Grid duplicateGrid = null;
                while (duplicateGrid == null)
                {
                    grid = grid.Next();

                    if (seenGrids.Contains(grid))
                    {
                        duplicateGrid = grid;
                    }
                    else
                    {
                        seenGrids.Add(grid);
                    }
                }

                Console.WriteLine(duplicateGrid.BiodivercityRating());
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = RecursiveGrid.Parse(input.Lines().ToList());

                for (var i = 0; i < 200; i++)
                {
                    grid = grid.Next();
                }

                Console.WriteLine(grid.CountBugs());
            }
        }

        private static class Cell
        {
            public const bool Empty = false;
            public const bool Bug = true;
        }

        private class Grid
        {
            public static Grid Parse(IEnumerable<string> lines)
            {
                var cells = lines
                    .Select(l => l.Select(c => c == '#' ? Cell.Bug : Cell.Empty).ToList())
                    .ToList();

                return new Grid(cells);
            }

            private readonly IReadOnlyList<IReadOnlyList<bool>> cells;

            public Grid(IReadOnlyList<IReadOnlyList<bool>> cells)
            {
                this.cells = cells;
            }

            public int Rows => cells.Count;
            public int Cols => cells[0].Count;

            public override string ToString()
            {
                var builder = new StringBuilder();

                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Cols; col++)
                    {
                        builder.Append(cells[row][col] == Cell.Bug ? '#' : '.');
                    }
                    builder.AppendLine();
                }

                return builder.ToString();
            }

            public override bool Equals(object obj)
            {
                if (obj is not Grid other)
                {
                    return false;
                }

                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Cols; col++)
                    {
                        if (cells[row][col] != other.cells[row][col])
                        {
                            return false;
                        }
                    }
                }
                
                return true;
            }

            public override int GetHashCode()
            {
                var hashCode = new HashCode();

                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Cols; col++)
                    {
                        hashCode.Add(cells[row][col]);
                    }
                }

                return hashCode.ToHashCode();
            }

            public bool InBounds(int row, int col) =>
                0 <= row && row < Rows &&
                0 <= col && col < Cols;

            public bool At(int row, int col) =>
                InBounds(row, col) ? cells[row][col] : Cell.Empty;

            public Grid Next()
            {
                int CountAdjacentBugs(int row, int col)
                {
                    var deltas = new[]
                    {
                        (dr: -1, dc: 0),
                        (dr: 0, dc: 1),
                        (dr: 1, dc: 0),
                        (dr: 0, dc: -1)
                    };

                    return deltas
                        .Select(d => At(row + d.dr, col + d.dc))
                        .Count(c => c == Cell.Bug);
                }

                bool NextCell(int row, int col)
                {
                    var adjacentCount = CountAdjacentBugs(row, col);
                    var current = At(row, col);

                    if (current == Cell.Bug && adjacentCount != 1)
                    {
                        return false;
                    }
                    if (current == Cell.Empty && (adjacentCount == 1 || adjacentCount == 2))
                    {
                        return true;
                    }
                    return current;
                }

                var newRows = new List<IReadOnlyList<bool>>();

                for (var row = 0; row < Rows; row++)
                {
                    var newRow = new List<bool>();

                    for (var col = 0; col < Cols; col++)
                    {
                        newRow.Add(NextCell(row, col));
                    }

                    newRows.Add(newRow);
                }

                return new Grid(newRows);
            }

            public long BiodivercityRating()
            {
                long rating = 0;

                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Cols; col++)
                    {
                        if (cells[row][col] == Cell.Bug)
                        {
                            var pow = row * Rows + col;
                            rating += 1L << pow;
                        }
                    }
                }

                return rating;
            }
        }

        private class RecursiveGrid
        {
            public static RecursiveGrid Parse(IReadOnlyList<string> lines)
            {
                var cells = lines
                    .SelectMany(l => l.Select(c => c == '#' ? Cell.Bug : Cell.Empty))
                    .ToList();

                var size = lines.Count;

                return new RecursiveGrid(size, 0 /* levelOffset */, cells);
            }

            private readonly int size;
            private readonly int levelOffset;
            private readonly IReadOnlyList<bool> cells;

            public RecursiveGrid(int size, int levelOffset, IReadOnlyList<bool> cells)
            {
                this.size = size;
                this.levelOffset = levelOffset;
                this.cells = cells;
            }

            public int Rows => size;
            public int Cols => size;
            public int Levels => cells.Count / (Rows * Cols);

            public override string ToString()
            {
                var builder = new StringBuilder();

                for (var levelIndex = 0; levelIndex < Levels; levelIndex++)
                {
                    var level = levelIndex + levelOffset;

                    builder.AppendLine($"Level {level}:");

                    for (var row = 0; row < Rows; row++)
                    {
                        for (var col = 0; col < Cols; col++)
                        {
                            if (row == size / 2 && col == size / 2)
                            {
                                builder.Append('?');
                            }
                            else
                            {
                                builder.Append(At(level, row, col) == Cell.Bug ? '#' : '.');
                            }
                        }
                        builder.AppendLine();
                    }

                    builder.AppendLine();
                }

                return builder.ToString();
            }

            public int CountBugs()
            {
                var count = 0;

                for (var levelIndex = 0; levelIndex < Levels; levelIndex++)
                {
                    var level = levelIndex + levelOffset;
                    for (var row = 0; row < Rows; row++)
                    {
                        for (var col = 0; col < Cols; col++)
                        {
                            if (At(level, row, col) == Cell.Bug)
                            {
                                count++;
                            }
                        }
                    }
                }

                return count;
            }

            public bool At(int level, int row, int col)
            {
                var index = (level - levelOffset) * size * size + row * size + col;
                return 0 <= index && index < cells.Count ? cells[index] : Cell.Empty;
            }

            public RecursiveGrid Next()
            {
                int CountAdjacentBugs(int level, int row, int col)
                {
                    return Neighbours(level, row, col)
                        .Select(c => At(c.level, c.row, c.col))
                        .Count(c => c == Cell.Bug);
                }

                bool NextCell(int level, int row, int col)
                {
                    var adjacentCount = CountAdjacentBugs(level, row, col);
                    var current = At(level, row, col);

                    if (current == Cell.Bug && adjacentCount != 1)
                    {
                        return false;
                    }
                    if (current == Cell.Empty && (adjacentCount == 1 || adjacentCount == 2))
                    {
                        return true;
                    }
                    return current;
                }

                var newCells = new List<bool>();
                var newLevelOffset = levelOffset - 1;

                var middleRow = size / 2;
                var middleCol = middleRow;

                for (var levelIndex = 0; levelIndex < Levels + 2; levelIndex++)
                {
                    var level = levelIndex + newLevelOffset;
                    for (var row = 0; row < Rows; row++)
                    {
                        for (var col = 0; col < Cols; col++)
                        {
                            if (row == middleRow && col == middleCol)
                            {
                                newCells.Add(Cell.Empty);
                            }
                            else
                            {
                                newCells.Add(NextCell(level, row, col));
                            }
                        }
                    }
                }

                return new RecursiveGrid(size, newLevelOffset, newCells);
            }

            private IEnumerable<(int level, int row, int col)> Neighbours(int level, int row, int col)
            {
                var middleRow = size / 2;
                var middleCol = middleRow;
                
                if (row == middleRow && col == middleCol)
                {
                    yield break;
                }

                var deltas = new[]
                {
                    (dr: -1, dc: 0),
                    (dr: 0, dc: 1),
                    (dr: 1, dc: 0),
                    (dr: 0, dc: -1)
                };

                foreach (var d in deltas)
                {
                    var n = (level, row: row + d.dr, col: col + d.dc);

                    if (n.row < 0)
                    {
                        yield return (level - 1, middleRow - 1, middleCol);
                    }
                    else if (n.row >= Rows)
                    {
                        yield return (level - 1, middleRow + 1, middleCol);
                    }
                    else if (n.col < 0)
                    {
                        yield return (level - 1, middleRow, middleCol - 1);
                    }
                    else if (n.col >= Cols)
                    {
                        yield return (level - 1, middleRow, middleCol + 1);
                    }
                    else if (n.row == middleRow && n.col == middleCol)
                    {
                        if (d.dc == 1)
                        {
                            for (var r = 0; r < Rows; r++)
                            {
                                yield return (level + 1, r, 0);
                            }
                        }
                        else if (d.dc == -1)
                        {
                            for (var r = 0; r < Rows; r++)
                            {
                                yield return (level + 1, r, Cols - 1);
                            }
                        }
                        else if (d.dr == 1)
                        {
                            for (var c = 0; c < Cols; c++)
                            {
                                yield return (level + 1, 0, c);
                            }
                        }
                        else
                        {
                            for (var c = 0; c < Cols; c++)
                            {
                                yield return (level + 1, Rows - 1, c);
                            }
                        }
                    }
                    else
                    {
                        yield return n;
                    }
                }
            } 
        }
    }
}
