using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day1
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "199",
                    "200",
                    "208",
                    "210",
                    "200",
                    "207",
                    "240",
                    "269",
                    "260",
                    "263"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/1/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var depths = input.Lines().Select(long.Parse).ToList();

                var increases = depths
                    .Pairs()
                    .Where(pair => pair.first < pair.second)
                    .Count();

                Console.WriteLine(increases);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var depths = input.Lines().Select(long.Parse).ToList();

                var threes = depths
                    .Threes()
                    .Select(three => three.first + three.second + three.third)
                    .ToList();

                var increases = threes
                    .Pairs()
                    .Where(pair => pair.first < pair.second)
                    .Count();

                Console.WriteLine(increases);
            }
        }
    }
}
