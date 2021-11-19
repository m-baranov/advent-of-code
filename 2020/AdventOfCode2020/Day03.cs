using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2020
{
    static class Day03
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "..##.......",
                "#...#...#..",
                ".#....#..#.",
                "..#.#...#.#",
                ".#...##..#.",
                "..#.##.....",
                ".#.#.#....#",
                ".#........#",
                "#.##...#...",
                "#...##....#",
                ".#..#...#.#"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/3/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var pos = 0;
                var count = 0;

                foreach (var line in input.Lines())
                {
                    if (line[pos] == '#')
                    {
                        count++;
                    }

                    pos = (pos + 3) % line.Length;
                }

                Console.WriteLine(count);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var lines = input.Lines().ToArray();

                var slopes = new[] { (1, 1), (3, 1), (5, 1), (7, 1), (1, 2) };

                var product = slopes
                    .Select(slope => Solve(lines, slope))
                    .Aggregate(1L, (acc, i) => acc * i);

                Console.WriteLine(product);
            }

            private int Solve(string[] lines, (int dx, int dy) slope)
            {
                var posX = 0;
                var posY = 0;
                var count = 0;

                while (posY < lines.Length)
                {
                    var line = lines[posY];

                    if (line[posX] == '#')
                    {
                        count++;
                    }

                    posX = (posX + slope.dx) % line.Length;
                    posY = posY + slope.dy;
                }

                return count;
            }
        }
    }
}
