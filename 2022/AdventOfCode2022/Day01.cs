using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day01
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "1000",
                    "2000",
                    "3000",
                    "",
                    "4000",
                    "",
                    "5000",
                    "6000",
                    "",
                    "7000",
                    "8000",
                    "9000",
                    "",
                    "10000"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/1/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var (index, sum) = input.Lines()
                    .SplitByEmptyLine()
                    .Select((lines, index) => (index, sum: lines.Select(long.Parse).Sum()))
                    .MaxBy(p => p.sum);

                Console.WriteLine(sum);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var sum = input.Lines()
                    .SplitByEmptyLine()
                    .Select(lines => lines.Select(long.Parse).Sum())
                    .OrderByDescending(sum => sum)
                    .Take(3)
                    .Sum();

                Console.WriteLine(sum);
            }
        }
    }
}
