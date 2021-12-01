using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day04
    {
        public static readonly IInput SampleInput =
            Input.Literal();

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/4/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var range = Range.Parse(input.Lines().First());

                var answer = range.Numbers().Where(Satisfies).Count();

                Console.WriteLine(answer);
            }

            private bool Satisfies(long candidate)
            {
                var digits = Util.Digits(candidate);

                var hasDescPair = digits.Pairwise().Any(p => p.Item1 > p.Item2);
                if (hasDescPair)
                {
                    return false;
                }

                var hasDouble = digits.Pairwise().Any(p => p.Item1 == p.Item2);
                if (!hasDouble)
                {
                    return false;
                }

                return true;
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var range = Range.Parse(input.Lines().First());

                var answer = range.Numbers().Where(Satisfies).Count();

                Console.WriteLine(answer);
            }

            private bool Satisfies(long candidate)
            {
                var digits = Util.Digits(candidate);

                var hasDescPair = digits.Pairwise().Any(p => p.Item1 > p.Item2);
                if (hasDescPair)
                {
                    return false;
                }

                var hasDouble = digits.Distinct().Select(d => digits.Where(dd => dd == d).Count()).Any(c => c == 2);
                if (!hasDouble)
                {
                    return false;
                }

                return true;
            }
        }

        public class Range
        {
            public static Range Parse(string text)
            {
                var tokens = text.Split('-');
                
                var start = long.Parse(tokens[0]);
                var end = long.Parse(tokens[1]);
                
                return new Range(start, end);
            }

            public Range(long start, long end)
            {
                Start = start;
                End = end;
            }

            public long Start { get; }
            public long End { get; }

            public IEnumerable<long> Numbers()
            {
                for (var i = Start; i <= End; i++)
                {
                    yield return i;
                }
            }
        }

        private static class Util
        {
            public static IReadOnlyList<int> Digits(long number)
            {
                var digits = new List<int>();

                while (number != 0)
                {
                    number = Math.DivRem(number, 10, out var rem);
                    digits.Add((int)rem);
                }

                digits.Reverse();

                return digits;
            }
        }
    }
}
