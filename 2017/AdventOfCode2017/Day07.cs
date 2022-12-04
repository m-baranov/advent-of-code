using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day07
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "pbga (66)",
                    "xhth (57)",
                    "ebii (61)",
                    "havc (66)",
                    "ktlj (57)",
                    "fwft (72) -> ktlj, cntj, xhth",
                    "qoyq (66)",
                    "padx (45) -> pbga, havc, qoyq",
                    "tknk (41) -> ugml, padx, fwft",
                    "jptl (61)",
                    "ugml (68) -> gyxo, ebii, jptl",
                    "gyxo (61)",
                    "cntj (57)"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/7/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var programs = input.Lines().Select(ProgramReport.Parse).ToList();

                var bottomProgram = programs.First(p => !programs.Any(o => o.ProgramsAbove.Contains(p.Name)));

                Console.WriteLine(bottomProgram.Name);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var programs = input.Lines().Select(ProgramReport.Parse).ToList();

                var bottomProgram = programs.First(p => !programs.Any(o => o.ProgramsAbove.Contains(p.Name)));
                var programLookup = programs.ToDictionary(p => p.Name, p => p);

                var rootNode = CreateNode(bottomProgram, programLookup);

                var (node, weightDelta) = Search(rootNode, delta: 0);
                Console.WriteLine($"node={node.Name}, weight={node.Weight + weightDelta}");
            }

            private static Node CreateNode(ProgramReport program, IReadOnlyDictionary<string, ProgramReport> programLookup)
            {
                var nodesAbove = program.ProgramsAbove
                    .Select(name => CreateNode(programLookup[name], programLookup))
                    .ToList();

                var subTreeWeight = program.Weight + nodesAbove.Sum(n => n.SubTreeWeight);

                return new Node(program.Name, program.Weight, subTreeWeight, nodesAbove);
            }

            private (Node node, int delta) Search(Node node, int delta)
            {
                if (node.NodesAbove.Count < 2)
                {
                    return (node, delta);
                }

                var minIdx = 0;
                var minCount = 1;
                var maxIdx = 0;
                var maxCount = 1;

                for (var i = 1; i < node.NodesAbove.Count; i++)
                {
                    var min = node.NodesAbove[minIdx].SubTreeWeight;
                    var max = node.NodesAbove[maxIdx].SubTreeWeight;

                    var current = node.NodesAbove[i].SubTreeWeight;

                    if (current == min)
                    {
                        minCount++;
                    }
                    else if (current < min)
                    {
                        minIdx = i;
                        minCount = 1;
                    }

                    if (current == max)
                    {
                        maxCount++;
                    }
                    else if (current > max)
                    {
                        maxIdx = i;
                        maxCount = 1;
                    }
                }

                var minNode = node.NodesAbove[minIdx];
                var maxNode = node.NodesAbove[maxIdx];

                if (minNode.SubTreeWeight == maxNode.SubTreeWeight)
                {
                    return (node, delta);
                }
                
                if (minCount == 1)
                {
                    return Search(minNode, maxNode.SubTreeWeight - minNode.SubTreeWeight);
                }
                else /* if (maxCount == 1) */
                {
                    return Search(maxNode, minNode.SubTreeWeight - maxNode.SubTreeWeight);
                }
            }

            private record Node(string Name, int Weight, int SubTreeWeight, IReadOnlyList<Node> NodesAbove);
        }

        private record ProgramReport(string Name, int Weight, IReadOnlyList<string> ProgramsAbove)
        {
            public static ProgramReport Parse(string text)
            {
                static (string left, string right) SplitBy(string text, string by)
                {
                    var index = text.IndexOf(by);
                    if (index < 0)
                    {
                        return (text, string.Empty);
                    }

                    return (text.Substring(0, index), text.Substring(index + by.Length));
                }

                var (nameWeightText, nodesAboveText) = SplitBy(text, " -> ");
                var (name, weightText) = SplitBy(nameWeightText, " (");

                var weight = int.Parse(weightText.TrimEnd(')'));

                var nodesAbove = string.IsNullOrEmpty(nodesAboveText) 
                    ? Array.Empty<string>()
                    : nodesAboveText.Split(", ");

                return new ProgramReport(name, weight, nodesAbove);
            }
        }
    }
}
