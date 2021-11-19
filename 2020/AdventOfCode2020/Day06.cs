using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020
{
    static class Day06
    {
        public static readonly IInput SampleInput =
           Input.Literal(
               "abc",
               "",
               "a",
               "b",
               "c",
               "",
               "ab",
               "ac",
               "",
               "a",
               "a",
               "a",
               "a",
               "",
               "b"
           );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/6/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var result = Util.Partition(input.Lines())
                    .Select(lines => lines.SelectMany(l => l).Distinct().Count())
                    .Sum();

                Console.WriteLine(result);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var result = Util.Partition(input.Lines())
                    .Select(lines => lines.Cast<IEnumerable<char>>().Aggregate((acc, set) => acc.Intersect(set)).Count())
                    .Sum();

                Console.WriteLine(result);
            }
        }

        private static class Util
        {
            public static IEnumerable<IReadOnlyList<string>> Partition(IEnumerable<string> lines)
            {
                var partition = new List<string>();

                foreach (var line in lines)
                {
                    if (line == string.Empty)
                    {
                        if (partition.Count > 0)
                        {
                            yield return partition;
                        }

                        partition = new List<string>();
                    }
                    else
                    {
                        partition.Add(line);
                    }
                }

                if (partition.Count > 0)
                {
                    yield return partition;
                }
            }
        }
    }
}
