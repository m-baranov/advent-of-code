using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day01
    {
        public static readonly IInput SampleInput =
            Input.Literal();

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/1/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var answer = input.Lines()
                    .Select(long.Parse)
                    .Select(mass => mass / 3 - 2)
                    .Sum();

                Console.WriteLine(answer);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var answer = input.Lines()
                    .Select(long.Parse)
                    .Select(Calculate)
                    .Sum();

                Console.WriteLine(answer);
            }

            private long Calculate(long mass)
            {
                var sum = 0L;
                do
                {
                    var fuel = mass / 3 - 2;
                    if (fuel < 0)
                    {
                        break;
                    }

                    sum += fuel;
                    mass = fuel;
                } while (true);

                return sum;
            }
        }
    }
}
