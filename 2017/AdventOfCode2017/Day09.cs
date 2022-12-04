using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day09
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal("{<{o\"i!a,<{i<a>}");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/9/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var text = input.Lines().First();

                var root = Node.Parse(text);

                var score = Score(root, level: 1);
                Console.WriteLine(score);
            }

            private int Score(Node node, int level)
            {
                if (node is not Node.Group group)
                {
                    return 0;
                }

                return level + group.Children.Sum(n => Score(n, level + 1));
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var text = input.Lines().First();

                var root = Node.Parse(text);

                var length = GarbageLength(root);
                Console.WriteLine(length);
            }

            private int GarbageLength(Node node)
            {
                if (node is Node.Garbage garbage)
                {
                    return garbage.EffectiveLength();
                }

                if (node is Node.Group group)
                {
                    return group.Children.Sum(GarbageLength);
                }

                return 0;
            }
        }

        private record Range(int Start, int End);

        private abstract class Node
        {
            public static Node Parse(string text) => ConsumeGroup(text, start: 0);

            private static Group ConsumeGroup(string text, int start)
            {
                Debug.Assert(text[start] == '{');

                var children = new List<Node>();

                var i = start + 1;
                while (i < text.Length)
                {
                    var ch = text[i];
                    if (ch == '}')
                    {
                        i++;
                        break;
                    }
                    else if (ch == '{')
                    {
                        var child = ConsumeGroup(text, i);
                        children.Add(child);
                        i = child.Range.End;
                    }
                    else if (ch == '<')
                    {
                        var child = ConsumeGarbage(text, i);
                        children.Add(child);
                        i = child.Range.End;
                    }
                    else
                    {
                        Debug.Assert(ch == ',');
                        i++;
                    }
                }

                return new Group(new Range(start, i), children);
            }

            private static Garbage ConsumeGarbage(string text, int start)
            {
                Debug.Assert(text[start] == '<');

                var i = start + 1;
                var skipped = 0;
                while (i < text.Length)
                {
                    var ch = text[i];
                    if (ch == '!')
                    {
                        skipped++;
                        i += 2; // skip next character
                    }
                    else if (ch == '>')
                    {
                        i++;
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }

                return new Garbage(new Range(start, i), skipped);
            }

            public Node(Range range)
            {
                Range = range;
            }

            public Range Range { get; }

            public sealed class Group : Node
            {
                public Group(Range range, IReadOnlyList<Node> children)
                    : base(range)
                {
                    Children = children;
                }

                public IReadOnlyList<Node> Children { get; }
            }

            public sealed class Garbage : Node
            {
                public Garbage(Range range, int skippedCharacters)
                    : base(range)
                {
                    SkippedCharacters = skippedCharacters;
                }

                public int SkippedCharacters { get; }

                public int EffectiveLength() =>
                    (Range.End - Range.Start)       // total length 
                        - SkippedCharacters * 2     // excludes ! and following character
                        - 2;                        // excludes surrounding < and >
            }
        }
    }
}
