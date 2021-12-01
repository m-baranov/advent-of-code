using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day16
    {
        public static readonly IInput Sample0Input =
            Input.Literal("12345678");

        public static readonly IInput Sample1Input =
            Input.Literal("80871224585914546619083218645595");

        public static readonly IInput Sample2Input =
            Input.Literal("19617804207202209144916044189917");

        public static readonly IInput Sample3Input =
            Input.Literal("69317163492948606335995924319873");

        public static readonly IInput Sample4Input =
            Input.Literal("03036732577212944063491565474664");

        public static readonly IInput Sample5Input =
            Input.Literal("02935109699940807407585447034323");

        public static readonly IInput Sample6Input =
            Input.Literal("03081770884921959731165446850517");

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/16/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var initial = input.Lines().First().Select(ch => ch.ToString()).Select(int.Parse).ToArray();
                var patternBase = new[] { 0, 1, 0, -1 };

                var output = initial;
                for (var phase = 0; phase < 100; phase++)
                {
                    output = Enumerable.Range(0, count: initial.Length)
                        .Select(step =>
                        {
                            var sum = output.Zip(Util.Pattern(patternBase, step), (a, b) => a * b).Sum();
                            return Math.Abs(sum % 10);
                        })
                        .ToArray();
                }

                Console.WriteLine(string.Join("", output.Take(8)));
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                // NOTE 1: Calculating N-th digit on phase X, will use digits 1 through N-1 on phase X - 1
                //         so can ignore these first N-1 digits at the beginning of each phase.
                
                // NOTE 2: The patterns for digits with indices N = from COUNT/2 to COUNT is such that it 
                //         has N zeros followed by N ones. Meaning that such N-th digit is the last digit
                //         of a sum of last N numbers from the previous phase.
                //
                //         All 3 of the provided examples and the test input ask to find numbers that are
                //         from the second half of the list. This solution takes advantage of that. It
                //         WILL NOT work in general case.

                var text = input.Lines().First();
                var skip = int.Parse(text.Substring(0, 7));
                var values = text.Select(ch => ch.ToString()).Select(int.Parse).ToArray();
                var repeat = 10000;

                var buffer = values.Repeat(repeat).Skip(skip).ToArray();
                for (var phase = 0; phase < 100; phase++)
                {
                    var next = new int[buffer.Length];
                    var sum = 0L;
                    for (var i = 0; i < buffer.Length; i++)
                    {
                        sum += buffer[buffer.Length - 1 - i];
                        next[buffer.Length - 1 - i] = (int)Math.Abs(sum % 10);
                    }

                    buffer = next;
                }

                Console.WriteLine(string.Join("", buffer.Take(8)));
            }

            //private interface ISequence
            //{
            //    int Count { get; }
            //    int At(int index);
            //}

            //private class RepeatedSequence : ISequence
            //{
            //    private readonly IReadOnlyList<int> values;
            //    private readonly int repeatTimes;

            //    public RepeatedSequence(IReadOnlyList<int> values, int repeatTimes)
            //    {
            //        this.values = values;
            //        this.repeatTimes = repeatTimes;
            //    }

            //    public int Count => values.Count * repeatTimes;

            //    public int At(int index) => values[index % values.Count];
            //}

            //private class CachedSequence : ISequence
            //{
            //    private readonly ISequence sequence;
            //    private readonly Dictionary<int, int> cache;

            //    public CachedSequence(ISequence sequence)
            //    {
            //        this.sequence = sequence;
            //        this.cache = new Dictionary<int, int>();
            //    }

            //    public int Count => sequence.Count;

            //    public int At(int index)
            //    {
            //        if (cache.TryGetValue(index, out var cached))
            //        {
            //            return cached;
            //        }

            //        var value = sequence.At(index);
            //        cache[index] = value;
            //        return value;
            //    }
            //}

            //private class ComputedSequence : ISequence
            //{
            //    private readonly ISequence previous;
                
            //    public ComputedSequence(ISequence previous)
            //    {
            //        this.previous = previous;
            //    }

            //    public int Count => previous.Count;

            //    public int At(int index)
            //    {
            //        var sum = 0L;

            //        var repeat = index + 1;

            //        var i = repeat - 1;
            //        while (i < Count)
            //        {
            //            var start = i;
            //            var end = Math.Min(start + repeat - 1, Count - 1);

            //            for (var j = start; j <= end; j++) 
            //            { 
            //                sum += previous.At(j); 
            //            }

            //            i += repeat * 4;
            //        }

            //        i = 3 * repeat - 1;
            //        while (i < Count)
            //        {
            //            var start = i;
            //            var end = Math.Min(start + repeat - 1, Count - 1);

            //            for (var j = start; j <= end; j++)
            //            {
            //                sum -= previous.At(j);
            //            }

            //            i += repeat * 4;
            //        }

            //        return (int)Math.Abs(sum % 10);
            //    }
            //}
        }

        private static class Util
        {
            public static IEnumerable<int> Pattern(IReadOnlyList<int> values, int step)
            {
                return values
                    .SelectMany(v => Enumerable.Range(0, step + 1).Select(_ => v))
                    .RepeatInfinitely()
                    .Skip(1);
            }

        }
    }

    public static class Day16EnumerableExtensions
    {
        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> items, int times)
        {
            for (var i = 0; i < times; i++)
            {
                foreach (var item in items)
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<T> RepeatInfinitely<T>(this IEnumerable<T> items)
        {
            while (true)
            {
                foreach (var item in items)
                {
                    yield return item;
                }
            }
        }
    }
}
