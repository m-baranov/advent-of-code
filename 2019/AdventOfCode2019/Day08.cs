using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day08
    {
        public static readonly IInput SampleInput =
            Input.Literal();

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/8/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var text = input.Lines().First();

                var w = 25;
                var h = 6;

                var layers = text.Chunk(w * h)
                    .Select(layer => layer.Chunk(w).ToList())
                    .ToList();

                var layer = layers.MinBy(l => Count(l, '0'));

                var ones = Count(layer, '1');
                var twos = Count(layer, '2');

                Console.WriteLine(ones * twos);
            }

            private int Count(IReadOnlyList<IReadOnlyList<char>> layer, char ch)
            {
                return layer.SelectMany(r => r).Where(c => c == ch).Count();
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var text = input.Lines().First();

                var w = 25;
                var h = 6;

                var layers = text.Chunk(w * h)
                    .Select(layer => layer.Chunk(w).ToList())
                    .ToList();

                var combined = Enumerable.Range(0, count: h)
                    .Select(row =>
                    {
                        return Enumerable.Range(0, count: w)
                            .Select(col => layers.Select(l => l[row][col]).First(ch => ch != '2'))
                            .ToList();
                    })
                    .ToList();

                foreach (var row in combined)
                {
                    Console.WriteLine(string.Join(string.Empty, row.Select(ch => ch == '1' ? "██" : "  ")));
                }
            }
        }
    }
}
