using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day05
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal("dabAcCaCBAcCcaDA");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/5/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var polymers = input.Lines().First();

                var material = new Material(polymers);
                material.React();

                Console.WriteLine(material.PolymerCount());
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var polymers = input.Lines().First();

                var shortestCount = PolymerTypes()
                    .Select(lower =>
                    {
                        var upper = char.ToUpper(lower);
                        return polymers.Where(p => p != lower && p != upper);
                    })
                    .Select(polymers =>
                    {
                        var material = new Material(polymers);
                        material.React();
                        return material.PolymerCount();
                    })
                    .Min();

                Console.WriteLine(shortestCount);
            }

            private static IEnumerable<char> PolymerTypes() =>
                Enumerable.Range((int)'a', count: 26).Select(i => (char)i);
        }

        private class Material
        {
            private readonly List<char> polymers;

            public Material(IEnumerable<char> polymers)
            {
                this.polymers = polymers.ToList();
            }

            public void React()
            {
                static int IndexOfNext(IReadOnlyList<char> polymers, int start)
                {
                    var i = start;
                    while (i < polymers.Count && polymers[i] == ' ')
                    {
                        i++;
                    }
                    return i;
                }

                static int IndexOfPrev(IReadOnlyList<char> polymers, int start)
                {
                    var i = start;
                    while (i >= 0 && polymers[i] == ' ')
                    {
                        i--;
                    }
                    return i;
                }

                static bool WillReact(char x, char y) => char.ToLower(x) == char.ToLower(y) && x != y;

                var i = 0;
                while (i < polymers.Count - 1)
                {
                    var current = polymers[i];
                    if (current == ' ')
                    {
                        i++;
                        continue;
                    }

                    var j = IndexOfNext(polymers, i + 1);
                    if (j >= polymers.Count)
                    {
                        break;
                    }

                    var next = polymers[j];

                    if (WillReact(current, next))
                    {
                        polymers[i] = ' ';
                        polymers[j] = ' ';

                        var k = IndexOfPrev(polymers, i);
                        if (k >= 0)
                        {
                            i = k;
                        }
                    }
                    else
                    {
                        i = j;
                    }
                }
            }

            public int PolymerCount() => polymers.Count(p => p != ' ');
        }
    }
}
