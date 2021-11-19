using System;
using System.IO;

namespace AdventOfCode2020
{
    static class Day01
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "1721",
                "979",
                "366",
                "299",
                "675",
                "1456"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/1/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var waitingFor = new bool[2021];

                foreach (var line in input.Lines())
                {
                    if (!int.TryParse(line, out var number))
                    {
                        continue;
                    }

                    if (!(0 <= number && number <= 2020))
                    {
                        continue;
                    }

                    var second = 2020 - number;

                    if (waitingFor[number])
                    {
                        Console.WriteLine(number * second);
                        return;
                    }

                    waitingFor[second] = true;
                }
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var seen = new bool[2021];

                foreach (var line in input.Lines())
                {
                    if (!int.TryParse(line, out var number))
                    {
                        continue;
                    }

                    if (!(0 <= number && number <= 2020))
                    {
                        continue;
                    }

                    seen[number] = true;
                }

                for (var i = 0; i < seen.Length; i++)
                {
                    if (!seen[i])
                    {
                        continue;
                    }

                    for (var j = i + 1; j < seen.Length; j++)
                    {
                        if (!seen[j])
                        {
                            continue;
                        }

                        var third = 2020 - i - j;
                        if (third > 0 && seen[third])
                        {
                            Console.WriteLine((long)i * j * third);
                            return;
                        }
                    }
                }
            }
        }
    }
}
