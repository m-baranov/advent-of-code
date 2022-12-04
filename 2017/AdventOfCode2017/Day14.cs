using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day14
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal("flqrgnkx");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/14/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var keyPrefix = input.Lines().First();

                var count = Enumerable.Range(0, 128)
                    .Select(row => $"{keyPrefix}-{row}")
                    .Select(key => KnotHash.ComputeAsBinaryString(key))
                    .Select(hash => hash.Count(ch => ch == '1'))
                    .Sum();

                Console.WriteLine(count);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var keyPrefix = input.Lines().First();

                var lines = Enumerable.Range(0, 128)
                    .Select(row => $"{keyPrefix}-{row}")
                    .Select(key => KnotHash.ComputeAsBinaryString(key))
                    .ToList();

                var grid = new Grid(lines);

                var count = 0;
                while (grid.TryFindFirstUsed(out var p))
                {
                    grid.RemoveRegion(p);
                    count++;
                }

                Console.WriteLine(count);
            }

            private enum Cell { Free, Used }

            private class Grid
            {
                private readonly Cell[][] cells;

                public Grid(IReadOnlyList<string> lines)
                {
                    this.cells = lines
                        .Select(l => l.Select(c => c == '1' ? Cell.Used : Cell.Free).ToArray())
                        .ToArray();
                }

                public int Rows => this.cells.Length;
                public int Cols => this.cells[0].Length;

                private bool InBounds((int row, int col) p) =>
                    0 <= p.row && p.row < Rows &&
                    0 <= p.col && p.col < Cols;

                private Cell At((int row, int col) p) => this.cells[p.row][p.col];

                private void Set((int row, int col) p, Cell cell) => this.cells[p.row][p.col] = cell;

                public bool TryFindFirstUsed(out (int row, int col) p)
                {
                    for (var row = 0; row < Rows; row++)
                    {
                        for (var col = 0; col < Cols; col++)
                        {
                            if (At((row, col)) == Cell.Used)
                            {
                                p = (row, col);
                                return true;
                            }
                        }
                    }

                    p = default;
                    return false;
                }

                public void RemoveRegion((int row, int col) start)
                {
                    static IReadOnlyList<(int row, int col)> NeighboursOf((int row, int col) p) =>
                        new[]
                        {
                            (p.row - 1, p.col),
                            (p.row, p.col + 1),
                            (p.row + 1, p.col),
                            (p.row, p.col - 1),
                        };

                    var toVisit = new Queue<(int row, int col)>();
                    toVisit.Enqueue(start);

                    var visited = new HashSet<(int row, int col)>();

                    while (toVisit.Count > 0)
                    {
                        var p = toVisit.Dequeue();

                        if (InBounds(p) && At(p) == Cell.Used && !visited.Contains(p))
                        {
                            visited.Add(p);

                            foreach (var np in NeighboursOf(p))
                            {
                                toVisit.Enqueue(np);
                            }
                        } 
                    }

                    foreach (var p in visited)
                    {
                        Set(p, Cell.Free);
                    }
                }
            }
        }

        private static class KnotHash
        {
            public static string ComputeAsBinaryString(string key)
            {
                static string ToBinaryString(byte[] bytes) =>
                    string.Join("", bytes.Select(b => Convert.ToString(b, toBase: 2).PadLeft(8, '0')));

                return ToBinaryString(Compute(key));
            }

            public static byte[] Compute(string key)
            {
                var lengths = key
                    .Select(ch => (int)ch)
                    .Concat(new[] { 17, 31, 73, 47, 23 })
                    .ToList();

                var sparseHash = ComputeSparseHash(lengths);
                var denseHash = ComputeDenseHash(sparseHash);
                
                return denseHash;
            }

            private static ISequence ComputeSparseHash(IReadOnlyList<int> lengths, int n = 256, int rounds = 64)
            {
                static int WrapIndex(int index, int count)
                {
                    if (index >= count)
                    {
                        return index % count;
                    }
                    else if (index < 0)
                    {
                        return count - (-index % count);
                    }
                    else
                    {
                        return index;
                    }
                }

                ISequence list = new InitialSequence(count: n);

                var current = 0;
                var skip = 0;
                for (var round = 0; round < rounds; round++)
                {
                    foreach (var length in lengths)
                    {
                        list = Sequence.Materialize(new ReversedSequence(list, current, length));

                        current = WrapIndex(current + length + skip, list.Count);

                        skip++;
                    }
                }

                return list;
            }

            private static byte[] ComputeDenseHash(ISequence list) =>
                Sequence.AsEnumerable(list)
                    .Chunk(chunkSize: 16)
                    .Select(chunk => (byte)chunk.Aggregate((acc, n) => acc ^ n))
                    .ToArray();

            private interface ISequence
            {
                int Count { get; }
                int At(int index);
            }

            private static class Sequence
            {
                public static IEnumerable<int> AsEnumerable(ISequence list)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        yield return list.At(i);
                    }
                }

                public static LiteralSequence Materialize(ISequence sequence)
                {
                    var values = new List<int>();
                    for (var i = 0; i < sequence.Count; i++)
                    {
                        values.Add(sequence.At(i));
                    }

                    return new LiteralSequence(values);
                }
            }

            private sealed class InitialSequence : ISequence
            {
                public InitialSequence(int count)
                {
                    Count = count;
                }

                public int Count { get; }

                public int At(int index) => index;
            }

            private sealed class LiteralSequence : ISequence
            {
                private readonly IReadOnlyList<int> values;

                public LiteralSequence(IReadOnlyList<int> values)
                {
                    this.values = values;
                }


                public int Count => this.values.Count;

                public int At(int index) => this.values[index];
            }

            private sealed class ReversedSequence : ISequence
            {
                private readonly ISequence inner;
                private readonly int start;
                private readonly int length;

                public ReversedSequence(ISequence inner, int start, int length)
                {
                    this.inner = inner;
                    this.start = start;
                    this.length = length;
                }

                public int Count => this.inner.Count;

                public int At(int index)
                {
                    var innerIndex = IsReversed(index) ? ReverseIndex(index) : index;
                    return this.inner.At(innerIndex);
                }
                private bool IsReversed(int index)
                {
                    var end = this.start + this.length;

                    if (end <= this.inner.Count)
                    {
                        return this.start <= index && index < end;
                    }

                    return (0 <= index && index < (end - this.inner.Count))
                        || (this.start <= index && index < this.inner.Count);
                }

                private int ReverseIndex(int index) =>
                    (this.start * 2 + this.length - 1 - index) % this.inner.Count;
            }
        }
    }
}
