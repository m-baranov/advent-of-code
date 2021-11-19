using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020
{
    static class Day17
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                ".#.",
                "..#",
                "###"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/17/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var lines = input.Lines().ToList();

                var current = new Qube();

                for (var y = 0; y < lines.Count; y++)
                {
                    var line = lines[y];
                    for (var x = 0; x < line.Length; x++)
                    {
                        current[(0, y, x)] = line[x] == '#';
                    }
                }

                var next = new Qube();
                for (var i = 0; i < 6; i++)
                {
                    foreach (var index in current.Indices())
                    {
                        var cell = current[index];
                        var neighbours = current.Neighbors(index);
                        var activeNeighbors = neighbours.Where(n => n == Cell.Active).Count();

                        bool nextCell;
                        if (cell == Cell.Active)
                        {
                            nextCell = activeNeighbors == 2 || activeNeighbors == 3
                                ? Cell.Active : Cell.Inactive;
                        }
                        else /* if (cell == Cell.Inactive) */
                        {
                            nextCell = activeNeighbors == 3
                                ? Cell.Active : Cell.Inactive;
                        }

                        next[index] = nextCell;
                    }

                    var temp = current;
                    current = next;
                    next = temp;
                }

                var count = 0;
                foreach (var index in current.Indices())
                {
                    if (current[index] == Cell.Active) count++;
                }

                Console.WriteLine(count);
            }

            class Qube
            {
                private const int Size = 30;
                private const int Half = Size / 2;
                private const int MinBound = -Half;
                private const int MaxBound = Half - 1;

                private static readonly IReadOnlyList<(int dz, int dy, int dx)> Deltas =
                    CalculateDeltas();

                private static IReadOnlyList<(int dz, int dy, int dx)> CalculateDeltas()
                {
                    var values = new[] { -1, 0, 1 };

                    return (
                        from dz in values
                        from dy in values
                        from dx in values
                        where !(dz == 0 && dy == 0 && dx == 0)
                        select (dz, dy, dx)
                    ).ToList();
                }

                private readonly bool[,,] cells;

                public Qube()
                {
                    cells = new bool[Size + 2, Size + 2, Size + 2]; // +2 is for an always inavtive boder
                }

                public bool this[(int z, int y, int x) index]
                {
                    get => cells[index.z + Half + 1, index.y + Half + 1, index.x + Half + 1];
                    set => cells[index.z + Half + 1, index.y + Half + 1, index.x + Half + 1] = value;
                }

                public IEnumerable<bool> Neighbors((int z, int y, int x) index)
                {
                    foreach (var (dz, dy, dx) in Deltas)
                    {
                        yield return this[(dz + index.z, dy + index.y, dx + index.x)];
                    }
                }

                public IEnumerable<(int z, int y, int x)> Indices()
                {
                    for (var z = MinBound; z <= MaxBound; z++)
                    {
                        for (var y = MinBound; y <= MaxBound; y++)
                        {
                            for (var x = MinBound; x <= MaxBound; x++)
                            {
                                yield return (z, y, x);
                            }
                        }
                    }
                }
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var lines = input.Lines().ToList();

                var current = new Qube();

                for (var y = 0; y < lines.Count; y++)
                {
                    var line = lines[y];
                    for (var x = 0; x < line.Length; x++)
                    {
                        current[(0, 0, y, x)] = line[x] == '#';
                    }
                }

                var next = new Qube();
                for (var i = 0; i < 6; i++)
                {
                    foreach (var index in current.Indices())
                    {
                        var cell = current[index];
                        var neighbours = current.Neighbors(index);
                        var activeNeighbors = neighbours.Where(n => n == Cell.Active).Count();

                        bool nextCell;
                        if (cell == Cell.Active)
                        {
                            nextCell = activeNeighbors == 2 || activeNeighbors == 3
                                ? Cell.Active : Cell.Inactive;
                        }
                        else /* if (cell == Cell.Inactive) */
                        {
                            nextCell = activeNeighbors == 3
                                ? Cell.Active : Cell.Inactive;
                        }

                        next[index] = nextCell;
                    }

                    var temp = current;
                    current = next;
                    next = temp;
                }

                var count = 0;
                foreach (var index in current.Indices())
                {
                    if (current[index] == Cell.Active) count++;
                }

                Console.WriteLine(count);
            }

            class Qube
            {
                private const int Size = 30;
                private const int Half = Size / 2;
                private const int MinBound = -Half;
                private const int MaxBound = Half - 1;

                private static readonly IReadOnlyList<(int dw, int dz, int dy, int dx)> Deltas =
                    CalculateDeltas();

                private static IReadOnlyList<(int dw, int dz, int dy, int dx)> CalculateDeltas()
                {
                    var values = new[] { -1, 0, 1 };

                    return (
                        from dw in values
                        from dz in values
                        from dy in values
                        from dx in values
                        where !(dw == 0 && dz == 0 && dy == 0 && dx == 0)
                        select (dw, dz, dy, dx)
                    ).ToList();
                }

                private readonly bool[,,,] cells;

                public Qube()
                {
                    cells = new bool[Size + 2, Size + 2, Size + 2, Size + 2]; // +2 is for an always inavtive boder
                }

                public bool this[(int w, int z, int y, int x) index]
                {
                    get => cells[index.w + Half + 1, index.z + Half + 1, index.y + Half + 1, index.x + Half + 1];
                    set => cells[index.w + Half + 1, index.z + Half + 1, index.y + Half + 1, index.x + Half + 1] = value;
                }

                public IEnumerable<bool> Neighbors((int w, int z, int y, int x) index)
                {
                    foreach (var (dw, dz, dy, dx) in Deltas)
                    {
                        yield return this[(dw + index.w, dz + index.z, dy + index.y, dx + index.x)];
                    }
                }

                public IEnumerable<(int w, int z, int y, int x)> Indices()
                {
                    for (var w = MinBound; w <= MaxBound; w++)
                    {
                        for (var z = MinBound; z <= MaxBound; z++)
                        {
                            for (var y = MinBound; y <= MaxBound; y++)
                            {
                                for (var x = MinBound; x <= MaxBound; x++)
                                {
                                    yield return (w, z, y, x);
                                }
                            }
                        }
                    }
                }
            }
        }

        static class Cell
        {
            public const bool Inactive = false;
            public const bool Active = true;
        }
    }
}
