using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day05
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "0",
                    "3",
                    "0",
                    "1",
                    "-3"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/5/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var jumps = input.Lines().Select(int.Parse).ToList();

                var ip = 0;
                var steps = 0;
                while (0 <= ip && ip < jumps.Count)
                {
                    var jump = jumps[ip];
                    jumps[ip]++;
                    ip += jump;

                    steps++;
                }

                Console.WriteLine(steps);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var jumps = input.Lines().Select(int.Parse).ToList();

                var ip = 0;
                var steps = 0;
                while (0 <= ip && ip < jumps.Count)
                {
                    var jump = jumps[ip];

                    if (jump >= 3)
                    {
                        jumps[ip]--;
                    }
                    else
                    {
                        jumps[ip]++;
                    }
                    
                    ip += jump;

                    steps++;
                }

                Console.WriteLine(steps);
            }
        }
    }
}
