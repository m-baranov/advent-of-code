using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day01
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/1/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var changes = input.Lines().Select(long.Parse).ToList();

                var frequency = changes.Aggregate(0L, (freq, change) => freq + change);

                Console.WriteLine(frequency);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var changes = input.Lines().Select(long.Parse).ToList();

                var frequency = 0L;
                var seen = new HashSet<long>() { frequency };

                foreach (var change in changes.RepeatInfinitely())
                {
                    frequency = frequency + change;
                    
                    if (seen.Contains(frequency))
                    {
                        break;
                    }
                    else
                    {
                        seen.Add(frequency);
                    }
                }

                Console.WriteLine(frequency);
            }
        }
    }
}
