using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day11
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "5483143223",
                    "2745854711",
                    "5264556173",
                    "6141336146",
                    "6357385478",
                    "4167524645",
                    "2176841721",
                    "6882881134",
                    "4846848554",
                    "5283751526"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/11/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = Grid.Parse(input.Lines());

                var sum = 0L;
                for (var step = 0; step < 100; step++)
                {
                    sum += grid.Step();
                }

                Console.WriteLine(sum);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = Grid.Parse(input.Lines());

                var step = 0;
                while (true)
                {
                    step++;

                    var flashes = grid.Step();
                    if (flashes == grid.Rows * grid.Cols)
                    {
                        break;
                    }
                }

                Console.WriteLine(step);
            }
        }

        private class Position
        {
            public Position(int row, int col)
            {
                Row = row;
                Col = col;
            }

            public int Row { get; }
            public int Col { get; }

            public override bool Equals(object obj) =>
                obj is Position other ? Row == other.Row && Col == other.Col : false;

            public override int GetHashCode() => HashCode.Combine(Row, Col);

            public IEnumerable<Position> Neighbors()
            {
                var deltas = new[]
                {
                    (dr: 0, dc: -1),
                    (dr: -1, dc: -1),
                    (dr: -1, dc: 0),
                    (dr: -1, dc: 1),
                    (dr: 0, dc: 1),
                    (dr: 1, dc: 1),
                    (dr: 1, dc: 0),
                    (dr: 1, dc: -1)
                };

                return deltas.Select(d => new Position(Row + d.dr, Col + d.dc));
            }
        }

        private class Grid
        {
            public static Grid Parse(IEnumerable<string> lines)
            {
                var cells = lines
                    .Select(l => l.Select(d => int.Parse(d.ToString())).ToList())
                    .ToList();

                return new Grid(cells);
            }

            private readonly List<List<int>> cells;

            public Grid(List<List<int>> cells)
            {
                this.cells = cells;
            }

            public int Rows => cells.Count;
            public int Cols => cells[0].Count;

            private bool InBounds(Position p) =>
                0 <= p.Row && p.Row < Rows &&
                0 <= p.Col && p.Col < Cols;

            public int Step()
            {
                var toVisit = new Queue<Position>();
                toVisit.EnqueueRange(Positions());

                var flashedAt = new HashSet<Position>();

                while (toVisit.Count > 0)
                {
                    var current = toVisit.Dequeue();

                    if (flashedAt.Contains(current))
                    {
                        continue;
                    }

                    var flashed = IncrementAt(current);
                    if (flashed)
                    {
                        flashedAt.Add(current);
                        toVisit.EnqueueRange(current.Neighbors().Where(InBounds));
                    }
                }

                return flashedAt.Count;
            }

            private bool IncrementAt(Position pos)
            {
                cells[pos.Row][pos.Col]++;

                if (cells[pos.Row][pos.Col] > 9)
                {
                    cells[pos.Row][pos.Col] = 0;
                    return true;
                }

                return false;
            }

            private IEnumerable<Position> Positions()
            {
                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Cols; col++)
                    {
                        yield return new Position(row, col);
                    }
                }
            }
        } 
    }
}
