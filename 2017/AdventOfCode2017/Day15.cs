using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day15
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "Generator A starts with 65",
                    "Generator B starts with 8921"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/15/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var (genA, genB) = Generator.ParsePair(input.Lines().ToList());

                var count = Judge.CountEqual(40_000_000, genA.Values(), genB.Values());

                Console.WriteLine(count);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var (genA, genB) = Generator.ParsePair(input.Lines().ToList());

                var count = Judge.CountEqual(5_000_000, genA.ValuesFiltered(), genB.ValuesFiltered());

                Console.WriteLine(count);
            }
        }

        private static class Judge
        {
            public static int CountEqual(int take, IEnumerable<long> genAValues, IEnumerable<long> genBValues)
            {
                static bool Lower16BitsEqual(long a, long b)
                {
                    static long Mask(long x) => x & 0b1111_1111_1111_1111L;

                    return Mask(a) == Mask(b);
                }

                return genAValues
                    .Zip(genBValues, Lower16BitsEqual)
                    .Take(take)
                    .Count(e => e == true);
            }
        }

        private record Generator(long Start, long Factor, long Filter)
        {
            public static (Generator a, Generator b) ParsePair(IReadOnlyList<string> lines)
            {
                static long ParseStartValue(string text)
                {
                    const string prefix = "Generator ? starts with ";
                    return long.Parse(text.Substring(prefix.Length));
                }

                var startA = ParseStartValue(lines[0]);
                var startB = ParseStartValue(lines[1]);

                var a = new Generator(startA, 16807, 4);
                var b = new Generator(startB, 48271, 8);
                return (a, b);
            }

            public IEnumerable<long> ValuesFiltered() => 
                Values().Where(v => v % this.Filter == 0);

            public IEnumerable<long> Values()
            {
                var prev = this.Start;
                while (true)
                {
                    var next = (prev * this.Factor) % 2_147_483_647;
                    yield return next;
                    prev = next;
                }
            }
        }
    }
}
