using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day08
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal("2 3 0 3 10 11 12 1 1 0 1 99 2 1 1 2");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/8/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var root = Node.Parse(input.Lines().First());

                static int MetadataSum(Node node) => 
                    node.Metadata.Sum() + node.Children.Sum(MetadataSum);

                Console.WriteLine(MetadataSum(root));
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var root = Node.Parse(input.Lines().First());

                static int Value(Node node)
                {
                    if (node.Children.Count == 0)
                    {
                        return node.Metadata.Sum();
                    }
                    
                    return node.Metadata
                        .Select(m => m - 1)
                        .Where(i => 0 <= i && i < node.Children.Count)
                        .Select(i => node.Children[i])
                        .Sum(Value);
                }

                Console.WriteLine(Value(root));
            }
        }

        private class Node
        {
            public static Node Parse(string text)
            {
                var numbers = text.Split(' ').Select(int.Parse).ToList();
                var (node, _) = Parse(numbers, startIndex: 0);
                return node;
            }

            private static (Node, int) Parse(IReadOnlyList<int> numbers, int startIndex)
            {
                var childCount = numbers[startIndex];
                var metadataCount = numbers[startIndex + 1];

                var index = startIndex + 2;
                var children = new List<Node>();
                for (var i = 0; i < childCount; i++)
                {
                    var (child, nextIndex) = Parse(numbers, index);
                    children.Add(child);
                    index = nextIndex;
                }

                var metadata = new List<int>();

                for (var i = 0; i < metadataCount; i++)
                {
                    metadata.Add(numbers[index + i]);
                }

                return (new Node(children, metadata), index + metadataCount);
            }

            public Node(IReadOnlyList<Node> children, IReadOnlyList<int> metadata)
            {
                Children = children;
                Metadata = metadata;
            }

            public IReadOnlyList<Node> Children { get; }
            public IReadOnlyList<int> Metadata { get; }
        }
    }
}
