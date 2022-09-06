using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day02
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/2/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var ids = input.Lines().ToList();

                var (count2, count3) = ids.Aggregate((count2: 0, count3: 0), (acc, id) =>
                {
                    var letterCounts = CountLetters(id);

                    var has2 = letterCounts.Any(p => p.Value == 2);
                    var has3 = letterCounts.Any(p => p.Value == 3);

                    return (acc.count2 + (has2 ? 1 : 0), acc.count3 + (has3 ? 1 : 0));
                });

                Console.WriteLine(count2 * count3);
            }

            private IReadOnlyDictionary<char, int> CountLetters(string id) =>
                id.GroupBy(l => l).ToDictionary(g => g.Key, g => g.Count());
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var ids = input.Lines().ToList();

                var commonLetters =
                    from id1 in ids
                    from id2 in ids
                    let common = CommonLetters(id1, id2)
                    where common.Count == id1.Length - 1
                    select common;

                var result = string.Join("", commonLetters.First());
                Console.WriteLine(result);
            }

            private IReadOnlyList<char> CommonLetters(string a, string b) =>
                Enumerable.Range(0, a.Length)
                    .Where(i => a[i] == b[i])
                    .Select(i => a[i])
                    .ToList();
        }
    }
}
