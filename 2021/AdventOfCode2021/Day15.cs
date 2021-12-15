using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day15
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "1163751742",
                    "1381373672",
                    "2136511328",
                    "3694931569",
                    "7463417111",
                    "1319128137",
                    "1359912421",
                    "3125421639",
                    "1293138521",
                    "2311944581"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/15/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = Grid.Parse(input.Lines(), repeatTimes: 1);

                var distance = grid.ShortestDistance(grid.TopLeftPosition(), grid.BottomRightPosition());

                Console.WriteLine(distance);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = Grid.Parse(input.Lines(), repeatTimes: 5);

                var distance = grid.ShortestDistance(grid.TopLeftPosition(), grid.BottomRightPosition());

                Console.WriteLine(distance);
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

            public override string ToString() => $"({Row}, {Col})";

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
                    (dr: 1, dc: 0),
                };

                return deltas.Select(d => new Position(Row + d.dr, Col + d.dc));
            }
        }

        private class Grid
        {
            public static Grid Parse(IEnumerable<string> lines, int repeatTimes = 1)
            {
                var cells = lines
                    .Select(l => l.Select(c => int.Parse(c.ToString())).ToList())
                    .ToList();

                return new Grid(cells, repeatTimes);
            }

            private readonly IReadOnlyList<IReadOnlyList<int>> cells;
            private readonly int repeatTimes;

            public Grid(
                IReadOnlyList<IReadOnlyList<int>> cells,
                int repeatTimes)
            {
                this.cells = cells;
                this.repeatTimes = repeatTimes;
            }

            private int ActualRows => cells.Count;
            public int Rows => ActualRows * repeatTimes;
            private int ActualCols => cells[0].Count;
            public int Cols => ActualCols * repeatTimes;

            public Position TopLeftPosition() => new Position(0, 0);
            public Position BottomRightPosition() => new Position(Rows - 1, Cols - 1);

            private bool InBounds(Position p) =>
                0 <= p.Row && p.Row < Rows &&
                0 <= p.Col && p.Col < Cols;

            private int At(Position p) 
            {
                var incrRow = Math.DivRem(p.Row, ActualRows, out var actualRow);
                var incrCol = Math.DivRem(p.Col, ActualCols, out var actualCol);

                var actual = cells[actualRow][actualCol];

                return (actual + incrRow + incrCol - 1) % 9 + 1;
            }

            // Implements Dijkstra algorithm using a priority queue.
            // https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm#Using_a_priority_queue
            //
            // Using a normal queue takes too much time to complete the Part 2.
            public long ShortestDistance(Position start, Position end)
            {
                var toVisit = new PriorityQueue<Position>();
                toVisit.EnqueueOrUpdate(start, 0);

                var distances = new Dictionary<Position, long>();
                distances.Add(start, 0);

                while (!toVisit.IsEmpty())
                {
                    var position = toVisit.DequeueMinPriority();
                    var distance = distances[position];

                    foreach (var neighbour in position.Neighbors().Where(InBounds))
                    {
                        var newDistance = distance + At(neighbour);

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
        }


        // Poor man's "min-priority queue". Completely inefficient, but works well for
        // this challenge. .NET does not have an actual priority queue implementation
        // until .NET 6. 
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
