using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day12
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "0 <-> 2",
                    "1 <-> 1",
                    "2 <-> 0, 3, 4",
                    "3 <-> 2, 4",
                    "4 <-> 2, 3, 6",
                    "5 <-> 6",
                    "6 <-> 4, 5"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/12/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var pipes = input.Lines().Select(Pipe.Parse).ToList();

                var group = Pipe.FindGroupContaining(0, pipes);

                Console.WriteLine(group.Count);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var pipes = input.Lines().Select(Pipe.Parse).ToList();

                var groupCount = 0;
                while (pipes.Count > 0)
                {
                    var group = Pipe.FindGroupContaining(pipes[0].From, pipes);

                    pipes = pipes.Where(p => !group.Contains(p.From)).ToList();
                    groupCount++;
                }

                Console.WriteLine(groupCount);
            }
        }

        private record Pipe(int From, IReadOnlyList<int> Tos)
        {
            public static Pipe Parse(string text)
            {
                static (string left, string right) SplitBy(string text, string by)
                {
                    var index = text.IndexOf(by);
                    return (text.Substring(0, index), text.Substring(index + by.Length));
                }

                var (fromText, tosText) = SplitBy(text, " <-> ");

                var from = int.Parse(fromText);
                var tos = tosText.Split(", ").Select(int.Parse).ToList();

                return new Pipe(from, tos);
            }

            public static ISet<int> FindGroupContaining(int program, IReadOnlyList<Pipe> pipes)
            {
                var pipeLookup = pipes.ToDictionary(p => p.From);

                var visited = new HashSet<int>();

                var toVisit = new Queue<int>();
                toVisit.Enqueue(program);

                while (toVisit.Count > 0)
                {
                    var from = toVisit.Dequeue();
                    visited.Add(from);

                    var pipe = pipeLookup[from];

                    foreach (var to in pipe.Tos)
                    {
                        if (visited.Contains(to))
                        {
                            continue;
                        }

                        toVisit.Enqueue(to);
                    }
                }

                return visited;
            }
        }
    }
}
