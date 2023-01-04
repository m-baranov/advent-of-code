using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day24
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "#.######",
                    "#>>.<^<#",
                    "#.<..<<#",
                    "#>v.><>#",
                    "#<^v^^>#",
                    "######.#"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/24/input");
        }

        // The solution for both parts is a bit slow on
        // test input, but it gets the job done.

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = Grid.Parse(input.Lines().ToList());

                var time = grid.ShortestPath();
                Console.WriteLine(time);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = Grid.Parse(input.Lines().ToList());

                var time = grid.ShortestPath(floorCount: 3);
                Console.WriteLine(time);
            }
        }

        private record class Direction(int DeltaRow, int DeltaCol)
        {
            public static readonly Direction U = new(-1, 0);
            public static readonly Direction D = new(1, 0);
            public static readonly Direction L = new(0, -1);
            public static readonly Direction R = new(0, 1);
            
            public static readonly IReadOnlyList<Direction> All = new[] { U, D, L, R };

            public static Direction Parse(char ch) =>
                ch switch
                {
                    '>' => R,
                    '<' => L,
                    '^' => U,
                    'v' or _ => D,
                };

            public char ToChar()
            {
                if (this == R) return '>';
                if (this == L) return '<';
                if (this == U) return '^';
                /* if (this == D) */ return 'v';
            }
        }

        private record Position(int Row, int Col)
        {
            public Position Move(Direction direction, int times = 1) =>
                new(Row + times * direction.DeltaRow, Col + times * direction.DeltaCol);
        }

        private record Rect(int Rows, int Cols)
        {
            public bool Inside(Position p) =>
                0 <= p.Row && p.Row < this.Rows &&
                0 <= p.Col && p.Col < this.Cols;
        }

        private record Blizzard(Position Start, Direction Direction, Rect Box)
        {
            public int CycleLength() => (int)MathExtensions.Lcm(Box.Rows, Box.Cols);

            public Position At(int time)
            {
                static int Wrap(int value, int size) => 
                    value >= 0 
                        ? value % size 
                        : (size - 1) - (Math.Abs(value + 1) % size);

                var next = Start.Move(Direction, time);
                return new Position(Wrap(next.Row, Box.Rows), Wrap(next.Col, Box.Cols));
            }
        }

        private sealed class Grid
        {
            public static Grid Parse(IReadOnlyList<string> lines)
            {
                var size = new Rect(Rows: lines.Count - 2, Cols: lines[0].Length - 2); // skip border walls

                var blizzards = new List<Blizzard>();

                for (var r = 0; r < size.Rows; r++)
                {
                    for (var c = 0; c < size.Cols; c++)
                    {
                        var ch = lines[r + 1][c + 1]; // skip border walls
                        if (ch == '.')
                        {
                            continue;
                        }

                        blizzards.Add(new Blizzard(new Position(r, c), Direction.Parse(ch), size));
                    }
                }

                return new Grid(size, blizzards);
            }

            private readonly Rect size;
            private readonly Position start;
            private readonly Position end;

            private readonly IReadOnlyList<Blizzard> blizzards;
            private readonly int blizzardCycleLength;
            private readonly IReadOnlyList<IReadOnlyCollection<Position>> blizzardPositionByCycleTime;

            public Grid(Rect box, IReadOnlyList<Blizzard> blizzards)
            {
                this.size = box;

                this.start = new Position(-1, 0);
                this.end = new Position(this.size.Rows, this.size.Cols - 1);

                this.blizzards = blizzards;
                this.blizzardCycleLength = blizzards[0].CycleLength();
                this.blizzardPositionByCycleTime = PrecalculateBlizzardPositions();
            }

            private IReadOnlyList<IReadOnlyCollection<Position>> PrecalculateBlizzardPositions()
            {
                IReadOnlyCollection<Position> BlizzardPositionsAt(int time) =>
                    this.blizzards.Select(b => b.At(time)).Distinct().ToHashSet();

                return Enumerable
                    .Range(0, this.blizzardCycleLength)
                    .Select(BlizzardPositionsAt)
                    .ToList();
            }

            public int ShortestPath(int floorCount = 1)
            {
                PositionAtFloor WithFloor(Position position, int floor)
                {
                    if (position.Equals(this.start) && (floor % 2 == 1) && floor < floorCount - 1)
                    {
                        return new PositionAtFloor(position, floor + 1);
                    }
                    if (position.Equals(this.end) && (floor % 2 == 0) && floor < floorCount - 1)
                    {
                        return new PositionAtFloor(position, floor + 1);
                    }
                    return new PositionAtFloor(position, floor);
                }

                var startAtFloor = new PositionAtFloor(this.start, Floor: 0);
                var endAtFloor = new PositionAtFloor(this.end, Floor: floorCount - 1);

                var queue = new PriorityQueue<(PositionAtFloor position, int time)>();
                queue.EnqueueOrUpdate((startAtFloor, 0), 0);

                var visits = new Dictionary<(PositionAtFloor position, int cycle), int>();
                visits.Add((startAtFloor, 0), 0);

                while (!queue.IsEmpty())
                {
                    var (positionAtFloor, time) = queue.DequeueMinPriority();

                    var nextTime = time + 1;
                    var nextCycle = nextTime % this.blizzardCycleLength;

                    var blizzardPositions = this.blizzardPositionByCycleTime[nextCycle];

                    var nextPositions = Direction.All
                        .Select(d => positionAtFloor.Position.Move(d))
                        .Select(p => WithFloor(p, positionAtFloor.Floor))
                        .Append(positionAtFloor) // wait
                        .Where(p =>
                            p.Position.Equals(this.start) || p.Position.Equals(this.end) ||
                            (this.size.Inside(p.Position) && !blizzardPositions.Contains(p.Position))
                        )
                        .ToArray();


                    //var nextPositions = Direction.All
                    //    .Select(d => position.Move(d))
                    //    .Append(position) // wait
                    //    .Where(p => 
                    //        p.Equals(this.start) || p.Equals(this.end) ||
                    //        (this.size.Inside(p) && !blizzardPositions.Contains(p))
                    //    )
                    //    .Select(p => new PositionAtFloor(p, floor))
                    //    .Select(p => p.Equals(position) ? p : AdjustFloor(p))
                    //    .ToArray();

                    foreach (var nextPosition in nextPositions)
                    {
                        if (visits.TryGetValue((nextPosition, nextCycle), out var prevTime) && 
                            prevTime <= nextTime)
                        {
                            continue;
                        }

                        queue.EnqueueOrUpdate((nextPosition, nextTime), nextTime);
                        visits[(nextPosition, nextCycle)] = nextTime;
                    }
                }

                return visits.Where(p => p.Key.position.Equals(endAtFloor)).Select(p => p.Value).Min();
            }

            private record PositionAtFloor(Position Position, int Floor);

            public void Draw(int time)
            {
                static char BlizzardChar(IEnumerable<Blizzard> blizzards)
                {
                    var count = blizzards.Count();
                    return count switch
                    {
                        1 => blizzards.First().Direction.ToChar(),
                        <= 9 => count.ToString()[0],
                        _ => '*'
                    };
                }

                var blizzards = this.blizzards
                    .Select(b => (blizzard: b, position: b.At(time)))
                    .GroupBy(g => g.position)
                    .Select(g => (position: g.Key, ch: BlizzardChar(g.Select(i => i.blizzard))))
                    .ToDictionary(g => g.position, g => g.ch);

                Console.WriteLine($"time={time}");
                for (var r = 0; r < this.size.Rows; r++)
                {
                    for (var c = 0; c < this.size.Cols; c++)
                    {
                        var p = new Position(r, c);
                        var cell = blizzards.TryGetValue(p, out var ch) ? ch : '.';
                        Console.Write(cell);
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.ReadLine();
            }
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
