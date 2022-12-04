using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day03
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "vJrwpWtwJgWrhcsFMMfFFhFp",
                    "jqHRNqRjqzjGDLGLrsFMfFZSrLrFZsSL",
                    "PmmdzqPrVvPwwTWBwg",
                    "wMqvLMZHhHMvwLHjbvcjnnSBnvTQFn",
                    "ttgJtRGJQctTZtZT",
                    "CrZsJsPPZsGzwwsLwLmpwMDw"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/3/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var sum = input.Lines()
                    .Select(line =>
                    {
                        var count = line.Length / 2;

                        var left = line.Take(count);
                        var right = line.Skip(count);

                        return left.Intersect(right).Single();
                    })
                    .Select(Priority)
                    .Sum();

                Console.WriteLine(sum);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                static char Intersect(IEnumerable<IEnumerable<char>> lists) =>
                    lists.Aggregate((acc, list) => acc.Intersect(list)).Single();

                var sum = input.Lines()
                    .Chunk(size: 3)
                    .Select(lines => Intersect(lines))
                    .Select(Priority)
                    .Sum();

                Console.WriteLine(sum);
            }
        }

        private static int Priority(char ch) =>
            ch switch
            {
                >= 'a' and <= 'z' => ch - 'a' + 1,
                >= 'A' and <= 'Z' => ch - 'A' + 27,
                _ => 0
            };
    }
}
