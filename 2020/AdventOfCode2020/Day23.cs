using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020
{
    static class Day23
    {
        public static readonly IInput SampleInput =
            Input.Literal("389125467");

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/23/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var line = input.Lines().First();
                var numbers = line.Select(ch => ch.ToString()).Select(int.Parse).ToList();

                var ring = new Ring(numbers);
                for (var i = 0; i < 100; i++)
                {
                    ring.Change();
                    //Console.WriteLine($"{i,3}: {ring}");
                }

                Console.Write(ring.ToPart1Answer());
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var line = input.Lines().First();
                var numbers = line.Select(ch => ch.ToString()).Select(int.Parse)
                    .Concat(Enumerable.Range(start: 10, count: 1_000_000 - 9))
                    .ToList();

                var ring = new Ring(numbers);
                for (var i = 0; i < 10_000_000; i++)
                {
                    ring.Change();
                    //Console.WriteLine($"{i,3}: {ring}");
                }

                Console.Write(ring.ToPart2Answer());
            }
        }

        public class Ring
        {
            private Node head;
            private readonly int maxValue;
            private readonly Dictionary<int, Node> nodeByValue;

            public Ring(IReadOnlyList<int> numbers)
            {
                head = CreateRing(numbers);
                nodeByValue = Nodes(head).ToDictionary(n => n.Value, n => n);
                maxValue = numbers.Count;
            }

            private static Node CreateRing(IReadOnlyList<int> numbers)
            {
                var nodes = numbers.Select(n => new Node(n)).ToList();

                for (var i = 0; i < nodes.Count - 1; i++)
                {
                    nodes[i].Next = nodes[i + 1];
                }
                nodes[nodes.Count - 1].Next = nodes[0];
                
                return nodes[0];
            }

            public void Change()
            {
                var currentHead = head;

                var pickedNodes = Cut(afterNode: head, count: 3);

                var destinationValue = PickDestinationValue(currentHead, pickedNodes);

                var destinationNode = FindNodeByValue(currentHead, destinationValue);

                Paste(afterNode: destinationNode, firstNode: pickedNodes.First(), lastNode: pickedNodes.Last());

                head = currentHead.Next;
            }

            private static IReadOnlyList<Node> Cut(Node afterNode, int count)
            {
                var cutNodes = new List<Node>();

                var current = afterNode;
                for (var i = 0; i < count; i++)
                {
                    cutNodes.Add(current.Next);
                    current = current.Next;
                }

                afterNode.Next = current.Next;

                return cutNodes;
            }

            private static void Paste(Node afterNode, Node firstNode, Node lastNode)
            {
                var next = afterNode.Next;
                afterNode.Next = firstNode;
                lastNode.Next = next;
            }

            private int PickDestinationValue(Node currentHead, IReadOnlyList<Node> pickedNodes)
            {
                var destinationValue = Decrement(currentHead.Value);
                while (pickedNodes.Any(n => n.Value == destinationValue))
                {
                    destinationValue = Decrement(destinationValue);
                }

                return destinationValue;
            }

            private int Decrement(int value)
            {
                var next = value - 1;
                if (next < 1)
                {
                    next = maxValue;
                }
                return next;
            }

            private Node FindNodeByValue(Node head, int value)
            {
                return nodeByValue[value];

                //var current = head;
                //while (true)
                //{
                //    if (current.Value == value)
                //    {
                //        return current;
                //    }

                //    current = current.Next;

                //    if (current == head)
                //    {
                //        return null;
                //    }
                //}
            }

            public override string ToString()
            {
                return string.Join(" ", Values(head));
            }

            public string ToPart1Answer()
            {
                return string.Join("", Values(FindNodeByValue(head, 1)).Skip(1));
            }

            public long ToPart2Answer()
            {
                var node = FindNodeByValue(head, 1);

                var one = node.Next;
                var two = one.Next;

                return (long)one.Value * (long)two.Value;
            }

            private IEnumerable<int> Values(Node head) => Nodes(head).Select(n => n.Value);

            private IEnumerable<Node> Nodes(Node head)
            {
                var current = head;
                while (true)
                {
                    yield return current;

                    current = current.Next;

                    if (current == head)
                    {
                        break;
                    }
                }
            }

            private class Node
            {
                public Node(int value)
                {
                    Value = value;
                    Next = null;
                }

                public int Value { get; }
                public Node Next { get; set; }
            }
        }
    }
}
