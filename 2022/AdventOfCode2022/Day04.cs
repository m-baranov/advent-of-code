using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day04
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "2-4,6-8",
                    "2-3,4-5",
                    "5-7,7-9",
                    "2-8,3-7",
                    "6-6,4-6",
                    "2-6,4-8"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/4/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var count = input.Lines()
                    .Select(Range.ParsePair)
                    .Where(p => p.left.Contains(p.right) || p.right.Contains(p.left))
                    .Count();

                Console.WriteLine(count);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var count = input.Lines()
                    .Select(Range.ParsePair)
                    .Where(p => p.left.Overlaps(p.right))
                    .Count();

                Console.WriteLine(count);
            }
        }

        private record Range(int Start, int End)
        {
            public static (Range left, Range right) ParsePair(string text)
            {
                var parts = text.Split(',').Select(Parse).ToList();
                return (parts[0], parts[1]);
            }

            public static Range Parse(string text)
            {
                var parts = text.Split('-').Select(int.Parse).ToList();
                return new Range(parts[0], parts[1]);
            }

            public bool Contains(int point) => Start <= point && point <= End;

            public bool Contains(Range other) => Contains(other.Start) && Contains(other.End);

            public bool Overlaps(Range other) => !((other.End < Start) || (End < other.Start));
        }
    }
}
