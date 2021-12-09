using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day09
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "2199943210",
                    "3987894921",
                    "9856789892",
                    "8767896789",
                    "9899965678"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/9/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = Grid.Parse(input.Lines());

                var sum = grid.LowPoints()
                    .Select(p => grid.At(p) + 1)
                    .Sum();

                Console.WriteLine(sum);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = Grid.Parse(input.Lines());

                var sum = grid.LowPoints()
                    .Select(p => grid.BasinSizeAt(p))
                    .OrderByDescending(l => l)
                    .Take(3)
                    .Aggregate(1L, (acc, l) => acc * l);

                Console.WriteLine(sum);
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
                    (dr: -1, dc: 0),
                    (dr: 0, dc: 1),
                    (dr: 1, dc: 0)
                };

                return deltas.Select(d => new Position(Row + d.dr, Col + d.dc));
            }
        }

        private class Grid
        {
            public static Grid Parse(IEnumerable<string> lines)
            {
                var cells = lines
                    .Select(l => l.Select(c => int.Parse(c.ToString())).ToList())
                    .ToList();

                return new Grid(cells);
            }

            private readonly IReadOnlyList<IReadOnlyList<int>> cells;

            public Grid(IReadOnlyList<IReadOnlyList<int>> cells)
            {
                this.cells = cells;
            }

            public int Rows => cells.Count;
            public int Cols => cells[0].Count;

            public int At(Position p) => cells[p.Row][p.Col];

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

            private IEnumerable<Position> Neighbours(Position p) =>
                p.Neighbors().Where(InRange);

            private bool InRange(Position p) =>
                0 <= p.Row && p.Row < Rows &&
                0 <= p.Col && p.Col < Cols;

            public IEnumerable<Position> LowPoints()
            {
                return Positions()
                    .Where(p =>
                    {
                        var value = At(p);
                        return Neighbours(p).All(n => At(n) > value);
                    });
            }

            public int BasinSizeAt(Position lowPoint)
            {
                var toVisit = new Queue<Position>();
                toVisit.Enqueue(lowPoint);

                var basin = new HashSet<Position>();
                basin.Add(lowPoint);

                while (toVisit.Count > 0)
                {
                    var pos = toVisit.Dequeue();
                    var value = At(pos);

                    var neighbours = Neighbours(pos)
                        .Where(npos => !basin.Contains(npos))
                        .Where(npos =>
                        {
                            var neighbour = At(npos);
                            return neighbour < 9 && neighbour > value;
                        });

                    foreach (var neighbour in neighbours)
                    {
                        toVisit.Enqueue(neighbour);
                        basin.Add(neighbour);
                    }
                }

                return basin.Count;
            }
        }
    }
}
