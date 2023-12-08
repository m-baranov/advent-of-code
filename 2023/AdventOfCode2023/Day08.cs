using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day08
{
    public static class Inputs
    {
        public static readonly IInput Sample1 =
            Input.Literal(""""""
RL

AAA = (BBB, CCC)
BBB = (DDD, EEE)
CCC = (ZZZ, GGG)
DDD = (DDD, DDD)
EEE = (EEE, EEE)
GGG = (GGG, GGG)
ZZZ = (ZZZ, ZZZ)
"""""");

        public static readonly IInput Sample2 =
            Input.Literal(""""""
LLR

AAA = (BBB, BBB)
BBB = (AAA, ZZZ)
ZZZ = (ZZZ, ZZZ)
"""""");


        public static readonly IInput Sample3 =
            Input.Literal(""""""
LR

11A = (11B, XXX)
11B = (XXX, 11Z)
11Z = (11B, XXX)
22A = (22B, XXX)
22B = (22C, 22C)
22C = (22Z, 22Z)
22Z = (22B, 22B)
XXX = (XXX, XXX)
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/8/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            static bool IsEnd(Node node) => node.Name == "ZZZ";

            var map = Map.Parse(input.Lines().ToList());

            var start = map.Nodes["AAA"];
            var steps = map.CountSteps(start, IsEnd);

            Console.WriteLine(steps);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            static bool IsStart(Node node) => node.Name.EndsWith('A');

            static bool IsEnd(Node node) => node.Name.EndsWith('Z');

            var map = Map.Parse(input.Lines().ToList());

            var steps = map.Nodes.Values
                .Where(IsStart)
                .Select(start => (long)map.CountSteps(start, IsEnd))
                .ToList();

            var total = MathExtensions.Lcm(steps);

            Console.WriteLine(total);
        }
    }

    private enum Direction { L, R }

    private static class DirectionUtil 
    {
        public static Direction Parse(char ch) =>
            ch == 'L' ? Direction.L : Direction.R;

        public static IReadOnlyList<Direction> ParseMany(string text) =>
            text.Select(Parse).ToList();
    }

    private record Node(string Name, string Left, string Right)
    {
        public static Node Parse(string text)
        {
            var (name, rest) = SplitBy(text, " = ");
            var (left, right) = SplitBy(rest, ", ");

            left = TrimPrefix(left, "(");
            right = TrimSuffix(right, ")");

            return new Node(name, left, right);
        }

        public string Next(Direction direction) =>
            direction == Direction.L ? Left : Right;
    }

    private record Map(
        IReadOnlyList<Direction> Directions,
        IReadOnlyDictionary<string, Node> Nodes)
    {
        public static Map Parse(IReadOnlyList<string> lines)
        {
            var directions = DirectionUtil.ParseMany(lines[0]);
            var nodes = lines.Skip(2).Select(Node.Parse).ToList();
            
            return new Map(directions, nodes.ToDictionary(n => n.Name));
        }

        public IEnumerable<Direction> RepeatDirections()
        {
            while (true)
            {
                foreach (var direction in Directions)
                {
                    yield return direction;
                }
            }
        }

        public int CountSteps(Node start, Func<Node, bool> isEnd)
        {
            var node = start;
            var steps = 0;

            foreach (var direction in RepeatDirections())
            {
                node = Nodes[node.Next(direction)];
                steps++;

                if (isEnd(node))
                {
                    break;
                }
            }

            return steps;
        }
    }

    private static (string left, string right) SplitBy(string text, string sep)
    {
        var index = text.IndexOf(sep);
        return (text.Substring(0, index), text.Substring(index + sep.Length));
    }

    private static string TrimPrefix(string text, string prefix) =>
        text.Substring(prefix.Length);

    private static string TrimSuffix(string text, string suffix) =>
        text.Substring(0, text.Length - suffix.Length);
}
