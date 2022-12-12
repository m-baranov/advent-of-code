using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day12
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "Sabqponm",
                    "abcryxxl",
                    "accszExk",
                    "acctuvwj",
                    "abdefghi"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/12/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var (grid, start, end) = Grid.Parse(input.Lines().ToList());

                var distance = grid.ShortestDistance(new[] { start }, end);
                Console.WriteLine(distance);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var (grid, _, end) = Grid.Parse(input.Lines().ToList());

                var starts = grid.PositionsWithNoElevation(); 

                var distance = grid.ShortestDistance(starts, end);
                Console.WriteLine(distance);
            }
        }

        private record Position(int Row, int Col)
        {
            public static readonly Position Origin = new(0, 0);

            public IEnumerable<Position> Neighbors() => 
                new[]
                {
                    this with { Col = Col - 1 },
                    this with { Row = Row - 1 },
                    this with { Col = Col + 1 },
                    this with { Row = Row + 1 }
                };
        }

        private class Grid
        {
            public static (Grid grid, Position start, Position end) Parse(IReadOnlyList<string> lines)
            {
                static int HeightOf(char ch) => ch - 'a';

                var cells = new List<IReadOnlyList<int>>();
                var start = Position.Origin;
                var end = Position.Origin;

                for (var row = 0; row < lines.Count; row++)
                {
                    var line = lines[row];
                    var heights = new List<int>();

                    for (var col = 0; col < line.Length; col++)
                    {
                        var ch = line[col];

                        if (ch == 'S')
                        {
                            start = new Position(row, col);
                            heights.Add(HeightOf('a'));
                        }
                        else if (ch == 'E')
                        {
                            end = new Position(row, col);
                            heights.Add(HeightOf('z'));
                        }
                        else
                        {
                            heights.Add(HeightOf(ch));
                        }
                    }

                    cells.Add(heights);
                }

                return (new Grid(cells), start, end);
            }

            private IReadOnlyList<IReadOnlyList<int>> cells;

            public Grid(IReadOnlyList<IReadOnlyList<int>> cells)
            {
                this.cells = cells;
            }

            public int Rows => this.cells.Count;
            public int Cols => this.cells[0].Count;

            public bool InBounds(Position p) =>
                0 <= p.Row && p.Row < this.Rows &&
                0 <= p.Col && p.Col < this.Cols;

            public int At(Position p) => this.cells[p.Row][p.Col];

            public IReadOnlyList<Position> PositionsWithNoElevation()
            {
                var positions = new List<Position>();
                
                for (var row = 0; row < this.Rows; row++)
                {
                    for (var col = 0; col < this.Cols; col++)
                    {
                        var pos = new Position(row, col);
                        if (At(pos) == 0)
                        {
                            positions.Add(pos);
                        }
                    }
                }

                return positions;
            }

            public long ShortestDistance(IReadOnlyList<Position> starts, Position end)
            {
                var toVisit = new PriorityQueue<Position>();
                var distances = new Dictionary<Position, long>();
                
                foreach (var start in starts)
                {
                    toVisit.EnqueueOrUpdate(start, 0);
                    distances.Add(start, 0);
                }

                while (!toVisit.IsEmpty())
                {
                    var position = toVisit.DequeueMinPriority();
                    var distance = distances[position];
                    var height = At(position);

                    foreach (var neighbour in position.Neighbors().Where(InBounds))
                    {
                        if (At(neighbour) > height + 1)
                        {
                            continue;
                        }

                        var newDistance = distance + 1;

                        if (distances.TryGetValue(neighbour, out var currentDistance) &&
                            currentDistance < newDistance)
                        {
                            continue;
                        }

                        distances[neighbour] = newDistance;
                        toVisit.EnqueueOrUpdate(neighbour, newDistance);
                    }
                }

                return distances[end];
            }

            // Poor man's "min-priority queue". Completely inefficient, but works.
            private class PriorityQueue<T>
            {
                private readonly Dictionary<T, long> _priorities;

                public PriorityQueue()
                {
                    _priorities = new Dictionary<T, long>();
                }

                public bool IsEmpty() => _priorities.Count == 0;

                public void EnqueueOrUpdate(T item, long priority)
                {
                    _priorities[item] = priority;
                }

                public T DequeueMinPriority()
                {
                    var pair = _priorities.MinBy(p => p.Value);
                    _priorities.Remove(pair.Key);
                    return pair.Key;
                }
            }
        }
    }
}
