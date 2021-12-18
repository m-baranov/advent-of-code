using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdventOfCode2021
{
    static class Day18
    {
        public static class Inputs
        {
            public static readonly IInput Sample1 =
                Input.Literal(
                    "[1,1]",
                    "[2,2]",
                    "[3,3]",
                    "[4,4]"
                );

            public static readonly IInput Sample2 =
                Input.Literal(
                    "[1,1]",
                    "[2,2]",
                    "[3,3]",
                    "[4,4]",
                    "[5,5]"
                );

            public static readonly IInput Sample3 =
                Input.Literal(
                    "[1,1]",
                    "[2,2]",
                    "[3,3]",
                    "[4,4]",
                    "[5,5]",
                    "[6,6]"
                );

            public static readonly IInput Sample4 =
                Input.Literal(
                    "[[[0,[4,5]],[0,0]],[[[4,5],[2,6]],[9,5]]]",
                    "[7,[[[3,7],[4,3]],[[6,3],[8,8]]]]",
                    "[[2,[[0,8],[3,4]]],[[[6,7],1],[7,[1,6]]]]",
                    "[[[[2,4],7],[6,[0,5]]],[[[6,8],[2,8]],[[2,1],[4,5]]]]",
                    "[7,[5,[[3,8],[1,4]]]]",
                    "[[2,[2,2]],[8,[8,1]]]",
                    "[2,9]",
                    "[1,[[[9,3],9],[[9,0],[0,7]]]]",
                    "[[[5,[7,4]],7],1]",
                    "[[[[4,2],2],6],[8,7]]"
                );

            public static readonly IInput Sample5 =
                Input.Literal(
                    "[[[0,[5,8]],[[1,7],[9,6]]],[[4,[1,2]],[[1,4],2]]]",
                    "[[[5,[2,8]],4],[5,[[9,9],0]]]",
                    "[6,[[[6,2],[5,6]],[[7,6],[4,7]]]]",
                    "[[[6,[0,7]],[0,9]],[4,[9,[9,0]]]]",
                    "[[[7,[6,4]],[3,[1,3]]],[[[5,5],1],9]]",
                    "[[6,[[7,3],[3,2]]],[[[3,8],[5,7]],4]]",
                    "[[[[5,4],[7,7]],8],[[8,3],8]]",
                    "[[9,3],[[9,9],[6,[4,9]]]]",
                    "[[2,[[7,7],7]],[[5,8],[[9,3],[0,2]]]]",
                    "[[[[5,2],5],[8,[3,7]]],[[5,[7,5]],[4,4]]]"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/18/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var nodes = input.Lines().Select(Node.Parse).ToList();

                var sum = Node.Sum(nodes);
                Console.WriteLine(Node.ToString(sum));
                Console.Write(Node.Magnitude(sum));
            }

            //public static void TestExplosions()
            //{
            //    var tests = new[]
            //    {
            //        "[[[[[9,8],1],2],3],4]",
            //        "[7,[6,[5,[4,[3,2]]]]]",
            //        "[[6,[5,[4,[3,2]]]],1]",
            //        "[[3,[2,[1,[7,3]]]],[6,[5,[4,[3,2]]]]]",
            //        "[[3,[2,[8,0]]],[9,[5,[4,[3,2]]]]]"
            //    };

            //    foreach (var test in tests)
            //    {
            //        var node = Node.Parse(test);
            //        var exploded = Node.Explode(node, out var applied);
            //        Console.WriteLine($"{applied}: {Node.ToString(exploded)}");
            //    }
            //}

            //public static void TestSum()
            //{
            //    var x = Node.Parse("[[[[4,3],4],4],[7,[[8,4],9]]]");
            //    var y = Node.Parse("[1,1]");
            //    var sum = Node.Sum(x, y);
            //    Console.Write(Node.ToString(sum));
            //}
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var nodes = input.Lines().Select(Node.Parse).ToList();

                var answer = nodes
                    .SelectMany(x => nodes.Select(y => (x, y)))
                    .Where(p => p.x != p.y)
                    .Select(p => Node.Sum(p.x, p.y))
                    .Select(Node.Magnitude)
                    .Max();

                Console.WriteLine(answer);
            }
        }

        private interface INode { }

        private class NumberNode : INode 
        {
            public NumberNode(int value)
            {
                Value = value;
            }

            public int Value { get; }

            public NumberNode Increment(int incrementBy) => 
                new NumberNode(Value + incrementBy);

            public PairNode Split()
            {
                var left = Value / 2;
                var right = Value - left;
                return new PairNode(new NumberNode(left), new NumberNode(right));
            }
        }

        private class PairNode : INode
        {
            public PairNode(INode left, INode right)
            {
                Left = left;
                Right = right;
            }

            public INode Left { get; }
            public INode Right { get; }
        }

        private static class Node
        {
            public static INode Parse(string text)
            {
                static (INode, string) ParseNode(string text)
                {
                    return text.StartsWith('[') ? ParsePair(text) : ParseNumber(text);
                }

                static (INode, string) ParseNumber(string text)
                {
                    var index = 0;
                    while (char.IsDigit(text[index]))
                    {
                        index++;
                    }

                    var value = int.Parse(text.Substring(0, index));
                    var rest = text.Substring(index);
                    return (new NumberNode(value), rest);
                }

                static (INode, string) ParsePair(string text)
                {
                    var (left, rest1) = ParseNode(text.Substring(1)); // skip [
                    var (right, rest2) = ParseNode(rest1.Substring(1)); // skip ,
                    var rest = rest2.Substring(1); // skip ]
                    return (new PairNode(left, right), rest);
                }

                var (node, _) = ParseNode(text);
                return node;
            }

            public static string ToString(INode root)
            {
                static void Append(StringBuilder builder, INode node)
                {
                    switch (node)
                    {
                        case NumberNode number:
                            builder.Append(number.Value);
                            break;
                        
                        case PairNode pair:
                            builder.Append('[');
                            Append(builder, pair.Left);
                            builder.Append(',');
                            Append(builder, pair.Right);
                            builder.Append(']');
                            break;
                    }
                }

                var builder = new StringBuilder();
                Append(builder, root);
                return builder.ToString();
            }

            public static long Magnitude(INode root) => 
                root switch
                {
                    NumberNode number => number.Value,
                    PairNode pair => 3 * Magnitude(pair.Left) + 2 * Magnitude(pair.Right),
                    _ => 0
                };

            public static INode Sum(IEnumerable<INode> nodes) => nodes.Aggregate(Sum);

            public static INode Sum(INode a, INode b) => Reduce(new PairNode(a, b));

            private static INode Reduce(INode root)
            {
                bool reduced;
                do
                {
                    root = Explode(root, out reduced);
                    if (!reduced)
                    {
                        root = Split(root, out reduced);
                    }
                } while (reduced);

                return root;
            }

            public static INode Split(INode root, out bool applied)
            {
                var nodeToSplit = Find(root, node => node is NumberNode number && number.Value >= 10);
                if (nodeToSplit is NumberNode numberToSplit)
                {
                    applied = true;
                    return Replace(root, nodeToSplit, numberToSplit.Split());
                }

                applied = false;
                return root;
            }

            public static INode Explode(INode root, out bool applied)
            {
                static Exposion FindExplosion(INode node, INode root, int parentCount) =>
                    node switch
                    {
                        PairNode pair =>
                            TryExplode(pair.Left, root, parentCount + 1) ??
                            TryExplode(pair.Right, root, parentCount + 1) ??
                            FindExplosion(pair.Left, root, parentCount + 1) ??
                            FindExplosion(pair.Right, root, parentCount + 1),

                        _ => null
                    };

                static Exposion TryExplode(INode node, INode root, int parentCount)
                {
                    if (parentCount < 4)
                    {
                        return null;
                    }

                    if (!(node is PairNode pair && pair.Left is NumberNode left && pair.Right is NumberNode right))
                    {
                        return null;
                    }

                    var numberToLeft = FindNumberToLeftOf(root, number: left);
                    var leftIncrement = numberToLeft != null ? new Increment(numberToLeft, left.Value) : null;

                    var numberToRight = FindNumberToRightOf(root, number: right);
                    var rightIncrement = numberToRight != null ? new Increment(numberToRight, right.Value) : null;

                    return new Exposion(pair, leftIncrement, rightIncrement);
                }

                static INode ApplyExplosion(INode root, Exposion exposion)
                {
                    root = Replace(root, exposion.ExplodingNode, new NumberNode(0));
                    root = ApplyIncrement(root, exposion.LeftIncrement);
                    root = ApplyIncrement(root, exposion.RightIncrement);

                    return root;
                }

                static INode ApplyIncrement(INode root, Increment increment) =>
                    increment != null
                        ? Replace(root, increment.Number, increment.Number.Increment(increment.By))
                        : root;
                
                var explosion = FindExplosion(root, root, parentCount: 0);
                if (explosion != null)
                {
                    applied = true;
                    return ApplyExplosion(root, explosion);
                }

                applied = false;
                return root;
            }

            public static NumberNode FindNumberToLeftOf(INode root, NumberNode number)
            {
                var (_, leftNumber) = FindNumberWithPrevious(root, null, (current, previous) => current == number);
                return leftNumber;
            }

            public static NumberNode FindNumberToRightOf(INode root, NumberNode number)
            {
                var (rightNumber, _) = FindNumberWithPrevious(root, null, (current, previous) => previous == number);
                return rightNumber;
            }

            private static (NumberNode found, NumberNode previous) FindNumberWithPrevious(
                INode root,
                NumberNode currentPrevious,
                Func<NumberNode, NumberNode, bool> predicate)
            {
                if (root is NumberNode number)
                {
                    return predicate(number, currentPrevious)
                        ? (found: number, previous: currentPrevious)
                        : (found: null, previous: number);
                }

                if (root is PairNode pair)
                {
                    var (found, previous) = FindNumberWithPrevious(pair.Left, currentPrevious, predicate);
                    return found != null
                        ? (found, previous)
                        : FindNumberWithPrevious(pair.Right, previous, predicate);
                }

                return (null, null);
            }

            private static INode Find(INode root, Func<INode, bool> predicate) =>
                root switch
                {
                    var node when predicate(node) => node,

                    PairNode pair => 
                        Find(pair.Left, predicate) ?? 
                        Find(pair.Right, predicate),

                    _ => null
                };

            private static INode Replace(INode root, INode toReplace, INode replacement) =>
                root switch
                {
                    var node when node == toReplace => replacement,

                    PairNode pair => 
                        new PairNode(
                            Replace(pair.Left, toReplace, replacement),
                            Replace(pair.Right, toReplace, replacement)
                        ),
                    
                    _ => root
                };
        }

        private class Exposion
        {
            public Exposion(PairNode explodingNode, Increment leftIncrement, Increment rightIncrement)
            {
                ExplodingNode = explodingNode;
                LeftIncrement = leftIncrement;
                RightIncrement = rightIncrement;
            }

            public PairNode ExplodingNode { get; }
            public Increment LeftIncrement { get; }
            public Increment RightIncrement { get; }
        }

        private class Increment
        {
            public Increment(NumberNode number, int by)
            {
                Number = number;
                By = by;
            }

            public NumberNode Number { get; }
            public int By { get; }
        }
    }
}
