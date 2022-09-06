using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day14
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal("2018");

            public static readonly IInput Sample2 =
                Input.Literal("59414");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/14/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var n = int.Parse(input.Lines().First());

                var numbers = new List<int>() { 3, 7 };

                var ix = 0;
                var iy = 1;

                while (numbers.Count < n + 10)
                {
                    var nx = numbers[ix];
                    var ny = numbers[iy];

                    numbers.AddRange(Util.Digits(nx + ny));

                    ix = (ix + nx + 1) % numbers.Count;
                    iy = (iy + ny + 1) % numbers.Count;
                }

                var ansewer = string.Join("", numbers.Skip(n).Take(10));
                Console.WriteLine(ansewer);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var n = input.Lines().First();
                var nlen = n.Length;

                var numbers = new List<int>() { 3, 7 };

                var ix = 0;
                var iy = 1;

                while (true)
                {
                    var nx = numbers[ix];
                    var ny = numbers[iy];

                    var digits = Util.Digits(nx + ny).ToList();
                    numbers.AddRange(digits);

                    if (numbers.Count > nlen)
                    {
                        var attempt1 = string.Join("", numbers.TakeLast(nlen));
                        if (attempt1 == n)
                        {
                            Console.WriteLine(numbers.Count - nlen);
                            break;
                        }
                    }

                    if (digits.Count > 1 && numbers.Count > nlen + 1)
                    {
                        var attempt2 = string.Join("", numbers.TakeLast(nlen + 1).Take(nlen));
                        if (attempt2 == n)
                        {
                            Console.WriteLine(numbers.Count - nlen - 1);
                            break;
                        }
                    }

                    ix = (ix + nx + 1) % numbers.Count;
                    iy = (iy + ny + 1) % numbers.Count;
                }
            }
        }

        public static class Util
        {
            public static IEnumerable<int> Digits(int number)
            {
                static IEnumerable<int> DigitsReverse(int number)
                {
                    if (number == 0)
                    {
                        yield return 0;
                        yield break;
                    }

                    while (number > 0)
                    {
                        number = Math.DivRem(number, 10, out var digit);
                        yield return digit;
                    }
                }

                return DigitsReverse(number).Reverse();
            }
        }
    }
}
