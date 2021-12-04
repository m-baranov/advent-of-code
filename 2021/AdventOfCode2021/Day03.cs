using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day03
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "00100",
                    "11110",
                    "10110",
                    "10111",
                    "10101",
                    "01111",
                    "00111",
                    "11100",
                    "10000",
                    "11001",
                    "00010",
                    "01010"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/3/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var numbers = input.Lines().ToList();

                var gammaRateDigits = Enumerable.Range(0, numbers[0].Length)
                    .Select(i => Util.MostCommonDigit(numbers, i))
                    .ToList();

                var epsilonRateDigits = gammaRateDigits
                    .Select(Util.InvertDigit)
                    .ToList();

                var gammaRate = Util.ToDecimal(gammaRateDigits);
                var epsilonRate = Util.ToDecimal(epsilonRateDigits);

                Console.WriteLine(gammaRate * epsilonRate);
            }

        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var numbers = input.Lines().ToList();

                var oxygenGeneratorRating = Filter(numbers, useMostCommonDigit: true);
                var co2ScrubberRating = Filter(numbers, useMostCommonDigit: false);

                Console.WriteLine(oxygenGeneratorRating * co2ScrubberRating);
            }

            private long Filter(IReadOnlyList<string> numbers, bool useMostCommonDigit)
            {
                var index = 0;
                while (numbers.Count > 1)
                {
                    var digit = Util.MostCommonDigit(numbers, index);
                    if (!useMostCommonDigit)
                    {
                        digit = Util.InvertDigit(digit);
                    }

                    numbers = numbers.Where(n => n[index] == digit).ToList();
                    index++;
                }

                return Util.ToDecimal(numbers[0]);
            }
        }

        private static class Util
        {
            public static char MostCommonDigit(IReadOnlyList<string> numbers, int index)
            {
                var zeros = numbers.Select(n => n[index]).Where(ch => ch == '0').Count();
                return zeros > numbers.Count / 2 ? '0' : '1';
            }

            public static char InvertDigit(char digit) => digit == '0' ? '1' : '0';

            public static long ToDecimal(IEnumerable<char> digits) =>
                Convert.ToInt64(string.Join("", digits), fromBase: 2);
        }
    }
}
