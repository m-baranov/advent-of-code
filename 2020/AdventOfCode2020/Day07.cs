using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2020
{
    static class Day07
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "light red bags contain 1 bright white bag, 2 muted yellow bags.",
                "dark orange bags contain 3 bright white bags, 4 muted yellow bags.",
                "bright white bags contain 1 shiny gold bag.",
                "muted yellow bags contain 2 shiny gold bags, 9 faded blue bags.",
                "shiny gold bags contain 1 dark olive bag, 2 vibrant plum bags.",
                "dark olive bags contain 3 faded blue bags, 4 dotted black bags.",
                "vibrant plum bags contain 5 faded blue bags, 6 dotted black bags.",
                "faded blue bags contain no other bags.",
                "dotted black bags contain no other bags."
            );

        public static readonly IInput SampleInput2 =
            Input.Literal(
                "shiny gold bags contain 2 dark red bags.",
                "dark red bags contain 2 dark orange bags.",
                "dark orange bags contain 2 dark yellow bags.",
                "dark yellow bags contain 2 dark green bags.",
                "dark green bags contain 2 dark blue bags.",
                "dark blue bags contain 2 dark violet bags.",
                "dark violet bags contain no other bags."
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/7/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var rules = input.Lines().Select(Rule.Parse).ToList();

                var toVisit = new Queue<string>();
                toVisit.Enqueue("shiny gold");

                var result = new HashSet<string>();

                while (toVisit.Count > 0)
                {
                    var visiting = toVisit.Dequeue();

                    var containing = rules
                        .Where(r => r.CanContain(visiting))
                        .Where(r => !result.Contains(r.BagColor));

                    foreach (var c in containing)
                    {
                        toVisit.Enqueue(c.BagColor);
                        result.Add(c.BagColor);
                    }
                }

                Console.WriteLine(result.Count);
                Console.WriteLine(string.Join(", ", result));
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var rules = input.Lines().Select(Rule.Parse).ToList();
                var memo = new Dictionary<string, int>();

                Console.WriteLine(Count("shiny gold", rules, memo));
            }

            private int Count(
                string bagColor, 
                IReadOnlyList<Rule> rules,
                IDictionary<string, int> memo)
            {
                if (memo.TryGetValue(bagColor, out var memoCount))
                {
                    return memoCount;
                }

                var rule = rules.First(r => r.BagColor == bagColor);

                var count = rule.Contents
                    .Select(c => c.MaxCount * (1 + Count(c.BagColor, rules, memo)))
                    .Sum();

                memo.Add(bagColor, count);
                return count;
            }
        }

        public class Rule
        {
            public static Rule Parse(string text)
            {
                var (bagColor, contentsText) = SplitBagColorAndContents(text);
                var contents = ParseContents(contentsText);
                return new Rule(bagColor, contents);
            }

            private static (string bagColor, string contentsText) SplitBagColorAndContents(string text)
            {
                const string separator = " bags contain ";
                var index = text.IndexOf(separator);
                return (text.Substring(0, index), text.Substring(index + separator.Length));
            }

            private static IReadOnlyList<ContentItem> ParseContents(string text)
            {
                if (text == "no other bags.")
                {
                    return Array.Empty<ContentItem>();
                }

                return text.Split(new[] { ", " }, StringSplitOptions.None)
                    .Select(ParseContentItem)
                    .ToList();
            }

            private static ContentItem ParseContentItem(string text)
            {
                // ab cde f
                // 01234567

                // f = 2, l = 6   (l - f - 1) = 3

                var firstIndex = text.IndexOf(' ');
                var lastIndex = text.LastIndexOf(' ');

                var maxCountText = text.Substring(0, firstIndex);
                var maxCount = int.Parse(maxCountText);

                var bagColor = text.Substring(firstIndex + 1, lastIndex - firstIndex - 1);

                return new ContentItem(bagColor, maxCount);
            }

            public bool CanContain(string bagColor)
            {
                return Contents.Any(c => c.BagColor == bagColor);
            }

            public Rule(string bagColor, IReadOnlyList<ContentItem> contents)
            {
                BagColor = bagColor;
                Contents = contents;
            }

            public string BagColor { get; }
            public IReadOnlyList<ContentItem> Contents { get; }
        }

        public class ContentItem
        {
            public ContentItem(string bagColor, int maxCount)
            {
                BagColor = bagColor;
                MaxCount = maxCount;
            }

            public string BagColor { get; }
            public int MaxCount { get; }
        }
    }
}
