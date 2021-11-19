using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2020
{
    static class Day09
    {
        public const int SamplePreambleLength = 5;

        public static readonly IInput SampleInput =
            Input.Literal(
                "35",
                "20",
                "15",
                "25",
                "47",
                "40",
                "62",
                "55",
                "65",
                "95",
                "102",
                "117",
                "150",
                "182",
                "127",
                "219",
                "299",
                "277",
                "309",
                "576"
            );

        public const int TestPreambleLength = 25;

        public static readonly IInput TestInput =
           Input.Http("https://adventofcode.com/2020/day/9/input");

        public class Part1 : IProblem
        {
            private readonly int preambleLength;

            public Part1(int preambleLength)
            {
                this.preambleLength = preambleLength;
            }

            public void Run(TextReader input)
            {
                var numbers = input.Lines().Select(long.Parse).ToList();

                var number = Util.FindInvalidNumber(numbers, preambleLength);

                Console.WriteLine(number);
            }
        }

        public class Part2 : IProblem
        {
            private readonly int preambleLength;

            public Part2(int preambleLength)
            {
                this.preambleLength = preambleLength;
            }

            public void Run(TextReader input)
            {
                var numbers = input.Lines().Select(long.Parse).ToList();

                var invalidNumber = Util.FindInvalidNumber(numbers, preambleLength);

                for (var i = 0; i < numbers.Count - 1; i++)
                {
                    var sum = numbers[i];
                    for (var j = i + 1; j < numbers.Count; j++)
                    {
                        sum += numbers[j];

                        if (sum == invalidNumber)
                        {
                            var seq = numbers.Skip(i).Take(j - i + 1).ToList();

                            Console.WriteLine(seq.Min() + seq.Max());
                            return;
                        }
                        if (sum > invalidNumber)
                        {
                            break;
                        }
                    }
                }
            }
        }

        static class Util
        {
            public static long FindInvalidNumber(IReadOnlyList<long> numbers, int preambleLength)
            {
                for (var i = preambleLength; i < numbers.Count; i++)
                {
                    var preambleStart = i - preambleLength;
                    var preambleEnd = i - 1;

                    var number = numbers[i];

                    if (!IsSum(number, numbers, preambleStart, preambleEnd))
                    {
                        return number;
                    }
                }
                return -1;
            }

            public static bool IsSum(long number, IReadOnlyList<long> numbers, int preambleStart, int preambleEnd)
            {
                for (var i = preambleStart; i <= preambleEnd; i++)
                {
                    for (var j = i + 1; j <= preambleEnd; j++)
                    {
                        if (number == numbers[i] + numbers[j])
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }
}
