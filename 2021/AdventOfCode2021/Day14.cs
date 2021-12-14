using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day14
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "NNCB",
                    "",
                    "CH -> B",
                    "HH -> N",
                    "CB -> H",
                    "NH -> C",
                    "HB -> C",
                    "HC -> B",
                    "HN -> C",
                    "NN -> C",
                    "BH -> H",
                    "NC -> B",
                    "NB -> B",
                    "BN -> B",
                    "BB -> N",
                    "BC -> B",
                    "CC -> N",
                    "CN -> C"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/14/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var setup = Setup.Parse(input.Lines());

                IEnumerable<char> elements = setup.Template;
                for (var i = 0; i < 10; i++)
                {
                    elements = Apply(elements, setup.Rules);
                }

                var elementCounts = elements
                    .GroupBy(c => c)
                    .ToDictionary(g => g.Key, g => g.Count());

                DisplayResults(elementCounts);
            }

            private IEnumerable<char> Apply(
                IEnumerable<char> elements,
                IReadOnlyDictionary<Pair, char> rules)
            {
                var left = ' ';
                foreach (var right in elements)
                {
                    if (left != ' ')
                    {
                        var middle = rules[new Pair(left, right)];
                        yield return middle;
                    }

                    yield return right;

                    left = right;
                }
            }

            private static void DisplayResults(IReadOnlyDictionary<char, int> elementCounts)
            {
                var leastCommon = elementCounts.MinBy(c => c.Value);
                var mostCommon = elementCounts.MaxBy(c => c.Value);

                Console.WriteLine($"min: {leastCommon.Key}, {leastCommon.Value}");
                Console.WriteLine($"max: {mostCommon.Key}, {mostCommon.Value}");

                Console.WriteLine(mostCommon.Value - leastCommon.Value);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var setup = Setup.Parse(input.Lines());

                // Track counts of unique pairs produced on each step, instead of generating all
                // produced elements/pairs one by one. 

                var pairCounts = setup
                    .Template.Pairs()
                    .Select(p => new Pair(p.first, p.second))
                    .ToCounts();

                for (var i = 0; i < 40; i++)
                {
                    pairCounts = pairCounts
                        .SelectMany(pc => pc.SelectMany(pair => pair.ProducedPairs(middle: setup.Rules[pair])))
                        .SumCounts();
                }

                // Each element in the resulting sequence, except the first and the last ones,
                // appears in two pairs. I.e. for "ABC", the B appears in (A,B) and (B,C).
                var elementCounts = pairCounts
                    .SelectMany(pc => pc.SelectMany(pair => pair.Elements()))
                    .SumCounts()
                    .ToDictionary(ec => ec.Value, ec => ec.Count / 2);

                // Account for the first and the last element, that appear in one less pair, 
                // compared to all other elements.
                elementCounts[setup.Template.First()]++;
                elementCounts[setup.Template.Last()]++;

                DisplayResults(elementCounts);
            }

            private static void DisplayResults(IReadOnlyDictionary<char, long> elementCounts)
            {
                var leastCommon = elementCounts.MinBy(c => c.Value);
                var mostCommon = elementCounts.MaxBy(c => c.Value);

                Console.WriteLine($"min: {leastCommon.Key}, {leastCommon.Value}");
                Console.WriteLine($"max: {mostCommon.Key}, {mostCommon.Value}");

                Console.WriteLine(mostCommon.Value - leastCommon.Value);
            }
        }

        private class Setup
        {
            public static Setup Parse(IEnumerable<string> lines)
            {
                var groups = lines.SplitByEmptyLine().ToList();

                var template = groups[0][0];
                var rules = groups[1].Select(ParseRule).ToDictionary(r => r.pair, r => r.middle);

                return new Setup(template, rules);
            }

            private static (Pair pair, char middle) ParseRule(string text)
            {
                var parts = text.Split(" -> ");

                var left = parts[0][0];
                var right = parts[0][1];
                var middle = parts[1][0];

                return (new Pair(left, right), middle);
            }

            public Setup(string template, IReadOnlyDictionary<Pair, char> rules)
            {
                Template = template;
                Rules = rules;
            }

            public string Template { get; }
            public IReadOnlyDictionary<Pair, char> Rules { get; }
        }

        private class Pair
        {
            public Pair(char left, char right)
            {
                Left = left;
                Right = right;
            }

            public char Left { get; }
            public char Right { get; }

            public override bool Equals(object obj) =>
                obj is Pair other ? Left == other.Left && Right == other.Right : false;

            public override int GetHashCode() => HashCode.Combine(Left, Right);

            public IEnumerable<char> Elements()
            {
                yield return Left;
                yield return Right;
            }

            public IEnumerable<Pair> ProducedPairs(char middle)
            {
                yield return new Pair(Left, middle);
                yield return new Pair(middle, Right);
            }
        }
    }

    public static class Counter
    {
        public static Counter<T> Of<T>(T value, long count) => 
            new Counter<T>(value, count);
    }

    public class Counter<T>
    {
        public Counter(T value, long count)
        {
            Value = value;
            Count = count;
        }

        public T Value { get; }
        public long Count { get; }
    }

    public static class CounterExtensions
    {
        public static IReadOnlyList<Counter<T>> ToCounts<T>(this IEnumerable<T> values)
        {
            return values
                .GroupBy(v => v)
                .Select(g => new Counter<T>(g.Key, g.Count()))
                .ToList();
        }

        public static IReadOnlyList<Counter<T>> SumCounts<T>(this IEnumerable<Counter<T>> counters)
        {
            return counters
                .GroupBy(c => c.Value)
                .Select(g => new Counter<T>(g.Key, g.Select(c => c.Count).Sum()))
                .ToList();
        }

        public static IEnumerable<Counter<R>> SelectMany<T, R>(
            this Counter<T> counter, 
            Func<T, IEnumerable<R>> projection)
        {
            return projection(counter.Value)
                .Select(v => new Counter<R>(v, counter.Count));
        }
    }
}
