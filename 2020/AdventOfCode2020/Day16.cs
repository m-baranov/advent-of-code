using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace AdventOfCode2020
{
    static class Day16
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "class: 1-3 or 5-7",
                "row: 6-11 or 33-44",
                "seat: 13-40 or 45-50",
                "",
                "your ticket:",
                "7,1,14",
                "",
                "nearby tickets:",
                "7,3,47",
                "40,4,50",
                "55,2,20",
                "38,6,12"
            );

        public static readonly IInput SampleInput2 =
            Input.Literal(
                "class: 0-1 or 4-19",
                "row: 0-5 or 8-19",
                "seat: 0-13 or 16-19",
                "",
                "your ticket:",
                "11,12,13",
                "",
                "nearby tickets:",
                "3,9,18",
                "15,1,5",
                "5,14,9"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/16/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var lines = input.Lines().ToList();
                var puzzle = Puzzle.Parse(lines);

                var ranges = puzzle.Rules
                    .SelectMany(r => r.Ranges)
                    .ToList();

                var solution = puzzle.OtherTickets
                    .SelectMany(t => t.Numbers)
                    .Where(n => !ranges.Any(r => r.Contains(n)))
                    .Sum();

                Console.WriteLine(solution);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var lines = input.Lines().ToList();
                var puzzle = Puzzle.Parse(lines);

                var allRanges = puzzle.Rules
                    .SelectMany(r => r.Ranges)
                    .ToList();

                var validTickets = puzzle.OtherTickets
                    .Where(t => t.Numbers.All(n => allRanges.Any(r => r.Contains(n))))
                    .ToList();

                var candidateRules = new List<List<Rule>>();
                for (var i = 0; i < puzzle.YourTicket.Numbers.Count; i++)
                {
                    var rules = validTickets
                        .Select(t => t.Numbers[i])
                        .Select(n => puzzle.Rules.Where(r => r.Matches(n)))
                        .Aggregate((acc, rules) => acc.Intersect(rules, Rule.EqualityComparer))
                        .ToList();

                    candidateRules.Add(rules);
                }

                var seenNames = new HashSet<string>();

                var finalRules = new Rule[candidateRules.Count];
                while (true)
                {
                    var madeProgress = false;

                    for (var i = 0; i < candidateRules.Count; i++)
                    {
                        var rules = candidateRules[i].Where(r => !seenNames.Contains(r.Name)).ToList();
                        if (rules.Count == 1)
                        {
                            finalRules[i] = rules[0];
                            seenNames.Add(rules[0].Name);
                            madeProgress = true;
                        }
                    }

                    if (!madeProgress)
                    {
                        break;
                    }
                }

                var result = puzzle.YourTicket.Numbers
                    .Select((number, index) => new { number, rule = finalRules[index] })
                    .Where(p => p.rule.Name.StartsWith("departure"))
                    .Select(p => (long)p.number)
                    .Aggregate((acc, n) => acc * n);

                Console.Write(result);
            }
        }

        public class Range
        {
            public static Range Parse(string text)
            {
                var (startText, endText) = Util.SplitBy(text, "-");

                var start = int.Parse(startText);
                var end = int.Parse(endText);

                return new Range(start, end);
            }

            public Range(int start, int end)
            {
                Start = start;
                End = end;
            }

            public int Start { get; }
            public int End { get; }

            public bool Contains(int number) => Start <= number && number <= End;
        }

        public class Rule
        {
            public static readonly IEqualityComparer<Rule> EqualityComparer = new RuleComparer();

            public static Rule Parse(string text)
            {
                var (name, rangesText) = Util.SplitBy(text, ": ");
                var (range1Text, range2Text) = Util.SplitBy(rangesText, " or ");

                var ranges = new[] { range1Text, range2Text }.Select(Range.Parse).ToList();

                return new Rule(name, ranges);
            }

            public Rule(string name, IReadOnlyList<Range> ranges)
            {
                Name = name;
                Ranges = ranges;
            }

            public string Name { get; }
            public IReadOnlyList<Range> Ranges { get; }

            public bool Matches(int number) => Ranges.Any(r => r.Contains(number));
        }

        public class RuleComparer : IEqualityComparer<Rule>
        {
            public bool Equals(Rule x, Rule y) => x.Name == y.Name;

            public int GetHashCode([DisallowNull] Rule obj) => obj.Name.GetHashCode();
        }

        public class Ticket
        {
            public static Ticket Parse(string text)
            {
                var numbers = text.Split(',').Select(int.Parse).ToList();
                return new Ticket(numbers);
            }

            public Ticket(IReadOnlyList<int> numbers)
            {
                Numbers = numbers;
            }

            public IReadOnlyList<int> Numbers { get; }
        }

        public class Puzzle
        {
            public static Puzzle Parse(IReadOnlyList<string> lines)
            {
                var groups = SplitLines(lines).ToList();

                var rules = groups[0].Select(Rule.Parse).ToList();
                var yourTicket = Ticket.Parse(groups[1].Skip(1).First());
                var otherTickets = groups[2].Skip(1).Select(Ticket.Parse).ToList();

                return new Puzzle(rules, yourTicket, otherTickets);
            }

            private static IEnumerable<IReadOnlyList<string>> SplitLines(IEnumerable<string> lines)
            {
                var buffer = new List<string>();
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        if (buffer.Count > 0)
                            yield return buffer;
                        
                        buffer = new List<string>();
                    }
                    else
                    {
                        buffer.Add(line);
                    }
                }

                if (buffer.Count > 0)
                    yield return buffer;
            }

            public Puzzle(
                IReadOnlyList<Rule> rules,
                Ticket yourTicket,
                IReadOnlyList<Ticket> otherTickets)
            {
                Rules = rules;
                YourTicket = yourTicket;
                OtherTickets = otherTickets;
            }

            public IReadOnlyList<Rule> Rules { get; }
            public Ticket YourTicket { get; }
            public IReadOnlyList<Ticket> OtherTickets { get; }
        }

        static class Util
        {
            public static (string, string) SplitBy(string text, string sep)
            {
                var index = text.IndexOf(sep);
                return (text.Substring(0, index), text.Substring(index + sep.Length));
            }
        }
    }
}
