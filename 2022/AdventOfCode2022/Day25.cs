using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day25
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "1=-0-2",
                    "12111",
                    "2=0=",
                    "21",
                    "2=01",
                    "111",
                    "20012",
                    "112",
                    "1=-1=",
                    "1-12",
                    "12",
                    "1=",
                    "122"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/25/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var base5s = input.Lines().ToList();

                var sum = base5s.Select(ToBase10).Sum();
                Console.WriteLine(sum);

                var base5sum = ToBase5(sum);
                Console.WriteLine(base5sum);
            }
        }

        // There is no Part 2 on Day 25

        //public class Part2 : IProblem
        //{
        //    public void Run(TextReader input)
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        private static long ToBase10(string base5)
        {
            static int DigitToInt(char digit) =>
                digit switch
                {
                    '=' => -2,
                    '-' => -1,
                    '2' => 2,
                    '1' => 1,
                    '0' or _ => 0
                };

            var pow5 = 1L;
            var sum = 0L;

            for (var i = 0; i < base5.Length; i++)
            {
                var digit = DigitToInt(base5[base5.Length - i - 1]);
                sum += digit * pow5;
                pow5 *= 5;
            }

            return sum;
        }

        private static string ToBase5(long value)
        {
            static (long div, long rem) DivRem(long x, long y)
            {
                var div = Math.DivRem(x, y, out var rem);
                return (div, rem);
            }

            static List<int> Remainders(long value)
            {
                var rems = new List<int>();

                while (value > 0)
                {
                    var (div, rem) = DivRem(value, 5);
                    rems.Add((int)rem);
                    value = div;
                }

                return rems;
            }

            static void Inc(List<int> rems, int start)
            {
                var index = start;
                while (true)
                {
                    if (index >= rems.Count)
                    {
                        rems.Add(1);
                        break;
                    }

                    var rem = rems[index];
                    if (rem == 4)
                    {
                        rems[index] = 0;
                        index++;
                    }
                    else
                    {
                        rems[index] = rem + 1;
                        break;
                    }
                }
            }

            static char IntToDigit(int value) =>
                value switch
                {
                    -2 => '=',
                    -1 => '-',
                    2 => '2',
                    1 => '1',
                    0 or _ => '0'
                };

            var rems = Remainders(value);
            for (var i = 0; i < rems.Count; i++)
            {
                var rem = rems[i];
                if (rem == 3)
                {
                    rems[i] = -2;
                    Inc(rems, i + 1);
                }
                else if (rem == 4)
                {
                    rems[i] = -1;
                    Inc(rems, i + 1);
                }
            }

            rems.Reverse();

            return string.Join(string.Empty, rems.Select(IntToDigit));
        }
    }
}
