using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day13
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "[1,1,3,1,1]",
                    "[1,1,5,1,1]",
                    "",
                    "[[1],[2,3,4]]",
                    "[[1],4]",
                    "",
                    "[9]",
                    "[[8,7,6]]",
                    "",
                    "[[4,4],4,4]",
                    "[[4,4],4,4,4]",
                    "",
                    "[7,7,7,7]",
                    "[7,7,7]",
                    "",
                    "[]",
                    "[3]",
                    "",
                    "[[[]]]",
                    "[[]]",
                    "",
                    "[1,[2,[3,[4,[5,6,7]]]],8,9]",
                    "[1,[2,[3,[4,[5,6,0]]]],8,9]"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/13/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var pairs = ParsePairs(input.Lines());

                var sum = pairs
                    .Where(p => Node.Compare(p.left, p.right) == Ordering.Correct)
                    .Sum(p => p.index);

                Console.WriteLine(sum);
            }

            private static IReadOnlyList<(int index, Node left, Node right)> ParsePairs(IEnumerable<string> lines) =>
                lines
                    .SplitByEmptyLine()
                    .Select((ls, index) => (index + 1, Node.Parse(ls[0]), Node.Parse(ls[1])))
                    .ToList();
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var marker2 = MakeMarker(2);
                var marker6 = MakeMarker(6);

                var nodes = ParseNodes(input.Lines())
                    .Concat(new[] { marker2, marker6 })
                    .ToList();

                var orderedNodes = nodes
                    .OrderBy(n => n, Node.Comparer.Instance)
                    .Select((node, index) => (node, index: index + 1))
                    .ToList();

                var marker2Index = orderedNodes.First(p => p.node == marker2).index;
                var marker6Index = orderedNodes.First(p => p.node == marker6).index;

                Console.WriteLine(marker2Index * marker6Index);
            }

            private static IEnumerable<Node> ParseNodes(IEnumerable<string> lines) => 
                lines.Where(l => !string.IsNullOrEmpty(l)).Select(Node.Parse);

            private static Node MakeMarker(int value) =>
                new Node.List(new[] { new Node.List(new[] { new Node.Number(value) }) });
        }

        private abstract class Node
        {
            public static Node Parse(string text)
            {
                var (node, _) = Consume(text, index: 0);
                return node;
            }

            private static (Node node, int index) Consume(string text, int index) =>
                text[index] == '[' ? ConsumeList(text, index + 1) : ConsumeNumber(text, index);

            private static (Node node, int index) ConsumeNumber(string text, int index)
            {
                static bool IsDigit(char ch) => '0' <= ch && ch <= '9';

                var i = index;
                while (i < text.Length && IsDigit(text[i]))
                {
                    i++;
                }

                var value = int.Parse(text.Substring(index, i - index));
                return (new Node.Number(value), i);
            }

            private static (Node node, int index) ConsumeList(string text, int index)
            {
                var items = new List<Node>();

                while (index < text.Length)
                {
                    var ch = text[index];
                    if (ch == ',')
                    {
                        index++;
                        continue;
                    }
                    else if (ch == ']')
                    {
                        index++;
                        break;
                    }

                    var (item, nextIndex) = Consume(text, index);
                    items.Add(item);
                    index = nextIndex;
                }

                return (new Node.List(items), index);
            }

            public static Ordering Compare(Node left, Node right)
            {
                static Ordering CompareNumbers(int left, int right)
                {
                    if (left < right)
                    {
                        return Ordering.Correct;
                    }
                    else if (left > right)
                    {
                        return Ordering.Incorrect;
                    }
                    else
                    {
                        return Ordering.Undecided;
                    }
                }

                static Ordering CompareLists(IReadOnlyList<Node> lefts, IReadOnlyList<Node> rights)
                {
                    var i = 0;
                    while (i < lefts.Count && i < rights.Count)
                    {
                        var order = Compare(lefts[i], rights[i]);
                        if (order != Ordering.Undecided)
                        {
                            return order;
                        }
                        i++;
                    }

                    return CompareNumbers(lefts.Count - i, rights.Count - i);
                }

                static Node.List AsList(Node.Number numberNode) =>
                    new Node.List(new[] { numberNode });

                return (left, right) switch
                {
                    (Node.Number leftNumber, Node.Number rightNumber) =>
                        CompareNumbers(leftNumber.Value, rightNumber.Value),

                    (Node.List leftList, Node.List rightList) =>
                        CompareLists(leftList.Items, rightList.Items),

                    (Node.Number leftNumber, _) => 
                        Compare(AsList(leftNumber), right),

                    (_, Node.Number rightNumber) =>
                        Compare(left, AsList(rightNumber)),

                    _ => Ordering.Undecided
                };
            }

            public class Number : Node
            {
                public Number(int value)
                {
                    Value = value;
                }

                public int Value { get; }

                public override string ToString() => Value.ToString();
            }

            public class List : Node
            { 
                public List(IReadOnlyList<Node> items)
                {
                    Items = items;
                }

                public IReadOnlyList<Node> Items { get; }

                public override string ToString() => $"[{string.Join(',', Items.Select(i => i.ToString()))}]";
            }

            public class Comparer : IComparer<Node>
            {
                public static readonly IComparer<Node> Instance = new Comparer();

                private Comparer() { }

                public int Compare(Node x, Node y) =>
                    Node.Compare(x, y) switch
                    {
                        Ordering.Correct => -1,
                        Ordering.Incorrect => 1,
                        Ordering.Undecided or _ => 0,
                    };
            }
        }

        private enum Ordering { Undecided, Correct, Incorrect }
    }
}
