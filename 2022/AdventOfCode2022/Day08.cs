using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day08
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "30373",
                    "25512",
                    "65332",
                    "33549",
                    "35390"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/8/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = new Grid(input.Lines().ToList());

                var count = Position.AllInGrid(grid.Rows, grid.Cols)
                    .Where(grid.IsVisibleAt)
                    .Count();

                Console.WriteLine(count);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = new Grid(input.Lines().ToList());

                var max = Position.AllInGrid(grid.Rows, grid.Cols)
                    .Select(grid.ScenicScoreOf)
                    .Max();

                Console.WriteLine(max);
            }
        }

        public record Position(int Row, int Col)
        {
            public static IEnumerable<Position> LeftOf(Position p)
            {
                var col = p.Col - 1;
                while (col >= 0)
                {
                    yield return p with { Col = col };
                    col--;
                }
            }

            public static IEnumerable<Position> RightOf(Position p, int cols)
            {
                var col = p.Col + 1;
                while (col < cols)
                {
                    yield return p with { Col = col };
                    col++;
                }
            }

            public static IEnumerable<Position> TopOf(Position p)
            {
                var row = p.Row - 1;
                while (row >= 0)
                {
                    yield return p with { Row = row };
                    row--;
                }
            }

            public static IEnumerable<Position> BottomOf(Position p, int rows)
            {
                var row = p.Row + 1;
                while (row < rows)
                {
                    yield return p with { Row = row };
                    row++;
                }
            }

            public static IEnumerable<Position> AllInGrid(int rows, int cols)
            {
                for (var row = 0; row < rows; row++)
                {
                    for (var col = 0; col < cols; col++)
                    {
                        yield return new Position(row, col);
                    }
                }
            }
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

            public int HeightAt(Position p) => this.cells[p.Row][p.Col] - '0';

            public bool IsVisibleAt(Position p)
            {
                var height = HeightAt(p);

                var directions = new[]
                {
                    Position.LeftOf(p),
                    Position.RightOf(p, this.Cols),
                    Position.TopOf(p),
                    Position.BottomOf(p, this.Rows)
                };

                return directions.Any(d => d.Select(HeightAt).All(h => h < height));
            }

            public int ScenicScoreOf(Position p)
            {
                static int Multiply(IEnumerable<int> nums) => nums.Aggregate(1, (acc, n) => acc * n);

                int CountVisible(int height, IEnumerable<Position> positions)
                {
                    var count = 0;
                    foreach (var p in positions)
                    {
                        var h = HeightAt(p);
                        count++;
                        if (h >= height)
                        {
                            break;
                        }
                    }
                    return count;
                }

                var height = HeightAt(p);

                var directions = new[]
                {
                    Position.LeftOf(p),
                    Position.RightOf(p, this.Cols),
                    Position.TopOf(p),
                    Position.BottomOf(p, this.Rows)
                };

                return Multiply(directions.Select(d => CountVisible(height, d)));
            }
        }
    }
}
