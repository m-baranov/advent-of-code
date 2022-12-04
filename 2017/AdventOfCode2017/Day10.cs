using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day10
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal("AoC 2017");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/10/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                const int n = 256;

                var lengths = input.Lines().First().Split(',').Select(int.Parse).ToList();

                var list = KnotHash.Compute(n, lengths);

                var answer = list.At(0) * list.At(1);
                Console.WriteLine(answer);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                const int n = 256;

                var text = input.Lines().First();

                var lengths = text
                    .Select(ch => (int)ch)
                    .Concat(new[] { 17, 31, 73, 47, 23 })
                    .ToList();

                var list = KnotHash.Compute(n, lengths, rounds: 64);

                Console.WriteLine(AsHexString(KnotHash.ToDenseHash(list)));
            }

            private static string AsHexString(byte[] bytes) => 
                Convert.ToHexString(bytes).ToLower();
        }

        private static class KnotHash
        {
            public static ISequence Compute(int n, IReadOnlyList<int> lengths, int rounds = 1)
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

            public static byte[] ToDenseHash(ISequence list) =>
                Sequence.AsEnumerable(list)
                    .Chunk(chunkSize: 16)
                    .Select(chunk => (byte)chunk.Aggregate((acc, n) => acc ^ n))
                    .ToArray();
        }

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
