using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdventOfCode2020
{
    static class Day19
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "0: 4 1 5",
                "1: 2 3 | 3 2",
                "2: 4 4 | 5 5",
                "3: 4 5 | 5 4",
                "4: \"a\"",
                "5: \"b\"",
                "",
                "ababbb",
                "bababa",
                "abbbab",
                "aaabbb",
                "aaaabbb"
            );

        public static readonly IInput SampleInput2 =
            Input.Literal(
                "42: 9 14 | 10 1",
                "9: 14 27 | 1 26",
                "10: 23 14 | 28 1",
                "1: \"a\"",
                "11: 42 31",
                "5: 1 14 | 15 1",
                "19: 14 1 | 14 14",
                "12: 24 14 | 19 1",
                "16: 15 1 | 14 14",
                "31: 14 17 | 1 13",
                "6: 14 14 | 1 14",
                "2: 1 24 | 14 4",
                "0: 8 11",
                "13: 14 3 | 1 12",
                "15: 1 | 14",
                "17: 14 2 | 1 7",
                "23: 25 1 | 22 14",
                "28: 16 1",
                "4: 1 1",
                "20: 14 14 | 1 15",
                "3: 5 14 | 16 1",
                "27: 1 6 | 14 18",
                "14: \"b\"",
                "21: 14 1 | 1 14",
                "25: 1 1 | 1 14",
                "22: 14 14",
                "8: 42",
                "26: 14 22 | 1 20",
                "18: 15 15",
                "7: 14 5 | 1 21",
                "24: 14 1",
                "",
                "abbbbbabbbaaaababbaabbbbabababbbabbbbbbabaaaa",
                "bbabbbbaabaabba",
                "babbbbaabbbbbabbbbbbaabaaabaaa",
                "aaabbbbbbaaaabaababaabababbabaaabbababababaaa",
                "bbbbbbbaaaabbbbaaabbabaaa",
                "bbbababbbbaaaaaaaabbababaaababaabab",
                "ababaaaaaabaaab",
                "ababaaaaabbbaba",
                "baabbaaaabbaaaababbaababb",
                "abbbbabbbbaaaababbbbbbaaaababb",
                "aaaaabbaabaaaaababaa",
                "aaaabbaaaabbaaa",
                "aaaabbaabbaaaaaaabbbabbbaaabbaabaaa",
                "babaaabbbaaabaababbaabababaaab",
                "aabbbbbaabbbaaaaaabbbbbababaaaaabbaaabba"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/19/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var lines = input.Lines().ToList();
                var puzzle = Puzzle.Parse(lines);

                var parser = BuildParser(puzzle.RuleSet, puzzle.RuleSet.At(0));

                var solution = puzzle.Tests
                    .Where(text => Parser.Matches(parser, text))
                    .Count();

                Console.WriteLine(solution);
            }

            private Parser<string> BuildParser(RuleSet ruleSet, Rule rule)
            {
                if (rule is Rule.Literal literalRule)
                {
                    return new LiteralParser(literalRule.Value);
                }
                else if (rule is Rule.Sequence sequenceRule)
                {
                    var parsers = sequenceRule.RuleIndexes
                        .Select(index => ruleSet.At(index))
                        .Select(rule => BuildParser(ruleSet, rule))
                        .ToList();

                    return new SequenceParser(parsers);
                }
                else if (rule is Rule.Alternatives alternativesRule)
                {
                    var parsers = alternativesRule.Rules
                        .Select(rule => BuildParser(ruleSet, rule))
                        .ToList();

                    return new AlternatesParser(parsers);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var lines = input.Lines().ToList();
                var puzzle = Puzzle.Parse(lines, (index, rule) =>
                {
                    if (index == 0)
                    {
                        return new Rule.Rule0();
                    }
                    return null; // use default parsing
                });

                var parser = BuildParser(puzzle.RuleSet, puzzle.RuleSet.At(0));

                var solution = puzzle.Tests
                    .Where(text => Parser.Matches(parser, text))
                    //.Select(text => { Console.WriteLine(text); return text; })
                    .Count();

                Console.WriteLine(solution);
            }

            private Parser<string> BuildParser(RuleSet ruleSet, Rule rule)
            {
                if (rule is Rule.Rule0 rule0)
                {
                    var leftParser = BuildParser(ruleSet, ruleSet.At(42));
                    var rightParser = BuildParser(ruleSet, ruleSet.At(31));
                    return new Rule0Parser(leftParser, rightParser);
                }
                else if (rule is Rule.Literal literalRule)
                {
                    return new LiteralParser(literalRule.Value);
                }
                else if (rule is Rule.Sequence sequenceRule)
                {
                    var parsers = sequenceRule.RuleIndexes
                        .Select(index => ruleSet.At(index))
                        .Select(rule => BuildParser(ruleSet, rule))
                        .ToList();

                    return new SequenceParser(parsers);
                }
                else if (rule is Rule.Alternatives alternativesRule)
                {
                    var parsers = alternativesRule.Rules
                        .Select(rule => BuildParser(ruleSet, rule))
                        .ToList();

                    return new AlternatesParser(parsers);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        abstract class Rule 
        {
            public static Rule Parse(string text)
            {
                if (text.StartsWith("\""))
                {
                    var value = text.Substring(1, text.Length - 2);
                    return new Literal(value);
                }
                
                if (text.Contains("|"))
                {
                    var rules = text
                        .Split(" | ", StringSplitOptions.None)
                        .Select(Parse)
                        .ToList();

                    return new Alternatives(rules);
                }

                var indexes = text.Split(" ", StringSplitOptions.None)
                    .Select(int.Parse)
                    .ToList();

                return new Sequence(indexes);
            }

            public class Literal : Rule
            {
                public Literal(string value)
                {
                    Value = value;
                }

                public string Value { get; }

                public override string ToString()
                {
                    return $"\"{Value}\"";
                }
            }

            public class Sequence : Rule
            {
                public Sequence(IReadOnlyList<int> ruleIndexes)
                {
                    RuleIndexes = ruleIndexes;
                }

                public IReadOnlyList<int> RuleIndexes { get; }

                public override string ToString()
                {
                    return string.Join(" ", RuleIndexes);
                }
            }

            public class Alternatives : Rule
            {
                public Alternatives(IReadOnlyList<Rule> rules)
                {
                    Rules = rules;
                }

                public IReadOnlyList<Rule> Rules { get; }

                public override string ToString()
                {
                    return string.Join(" | ", Rules);
                }
            }

            // special-case for Rule 0 in Part 2 
            public class Rule0 : Rule { } 
        }

        class RuleSet
        {
            public static RuleSet Parse(IReadOnlyList<string> lines, Func<int, string, Rule> transformFn = null)
            {
                transformFn = transformFn ?? ((i, s) => null);

                var pairs = lines
                    .Select(line =>
                    {
                        var (indexText, ruleText) = SplitBy(line, ": ");

                        var index = int.Parse(indexText);

                        var transformed = transformFn(index, ruleText);
                        var rule = transformed ?? Rule.Parse(ruleText);

                        return new { index, rule };
                    })
                    .ToList();

                var max = pairs.Select(p => p.index).Max();

                var rules = new Rule[max + 1];
                foreach (var pair in pairs)
                {
                    rules[pair.index] = pair.rule;
                }

                if (rules.Any(r => r == null))
                {
                    Console.WriteLine("WARN: missing rule indexes detected.");
                }

                return new RuleSet(rules);
            }

            public static (string, string) SplitBy(string text, string sep)
            {
                var index = text.IndexOf(sep);
                return (text.Substring(0, index), text.Substring(index + sep.Length));
            }

            public RuleSet(IReadOnlyList<Rule> rules)
            {
                Rules = rules;
            }

            public IReadOnlyList<Rule> Rules { get; }

            public Rule At(int index) => Rules[index];
        }

        class Puzzle
        {
            public static Puzzle Parse(IReadOnlyList<string> lines, Func<int, string, Rule> transformRule = null)
            {
                var groups = SplitLines(lines).ToList();

                var ruleSet = RuleSet.Parse(groups[0], transformRule);
                var tests = groups[1];

                return new Puzzle(ruleSet, tests);
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

            public Puzzle(RuleSet ruleSet, IReadOnlyList<string> tests)
            {
                RuleSet = ruleSet;
                Tests = tests;
            }

            public RuleSet RuleSet { get; }
            public IReadOnlyList<string> Tests { get; }
        }

        static class Result
        {
            public static Result<T> Some<T>(T value) => new Result<T>.Some(value);

            public static Result<T> None<T>() => new Result<T>.None();
        }

        abstract class Result<T>
        {
            public class Some : Result<T>
            {
                public Some(T value)
                {
                    Value = value;
                }

                public T Value { get; }
            }

            public class None : Result<T> { }
        }

        interface Parser<T>
        {
            (Result<T>, string) Parse(string text); 
        }

        static class Parser
        {
            public static bool Matches(Parser<string> parser, string text)
            {
                var (result, remaining) = parser.Parse(text);
                return result is Result<string>.Some && remaining.Length == 0;
            }
        }

        class LiteralParser : Parser<string>
        {
            public LiteralParser(string value)
            {
                Value = value;
            }

            public string Value { get; }

            public (Result<string>, string) Parse(string text)
            {
                if (text.StartsWith(Value))
                {
                    return (Result.Some(Value), text.Substring(Value.Length));
                }

                return (Result.None<string>(), text);
            }
        }

        class SequenceParser : Parser<string>
        {
            public SequenceParser(IReadOnlyList<Parser<string>> parsers)
            {
                Parsers = parsers;
            }

            public IReadOnlyList<Parser<string>> Parsers { get; }

            public (Result<string>, string) Parse(string text)
            {
                var result = new StringBuilder();
                var remaining = text;

                foreach (var parser in Parsers)
                {
                    var (currentResult, currentRemaining) = parser.Parse(remaining);
                    if (currentResult is Result<string>.None)
                    {
                        return (Result.None<string>(), text);
                    }
                    else if (currentResult is Result<string>.Some someResult)
                    {
                        result.Append(someResult.Value);
                        remaining = currentRemaining;
                    }
                }

                return (Result.Some(result.ToString()), remaining);
            }
        }

        class AlternatesParser : Parser<string>
        {
            public AlternatesParser(IReadOnlyList<Parser<string>> parsers)
            {
                Parsers = parsers;
            }

            public IReadOnlyList<Parser<string>> Parsers { get; }

            public (Result<string>, string) Parse(string text)
            {
                foreach (var parser in Parsers)
                {
                    var (result, remaining) = parser.Parse(text);

                    if (result is Result<string>.Some)
                    {
                        return (result, remaining);
                    }
                }

                return (Result.None<string>(), text);
            }
        }

        class RepeatParser : Parser<(string, int)>
        {
            public RepeatParser(Parser<string> parser)
            {
                Parser = parser;
            }

            public Parser<string> Parser { get; }

            public (Result<(string, int)>, string) Parse(string text)
            {
                var count = 0;
                var result = new StringBuilder();
                var remaining = text;

                while (true)
                {
                    var (currentResult, currentRemaining) = Parser.Parse(remaining);
                    if (currentResult is Result<string>.None)
                    {
                        break;
                    }
                    else if (currentResult is Result<string>.Some someResult)
                    {
                        count++;
                        result.Append(someResult.Value);
                        remaining = currentRemaining;
                    }
                }

                return (Result.Some((result.ToString(), count)), remaining);
            }
        }

        class Rule0Parser : Parser<string>
        {
            public Rule0Parser(Parser<string> left, Parser<string> right)
            {
                Left = left;
                Right = right;
            }

            public Parser<string> Left { get; }
            public Parser<string> Right { get; }

            public (Result<string>, string) Parse(string text)
            {
                var (leftResult, remainingLeft) = new RepeatParser(Left).Parse(text);
                if (leftResult is Result<(string, int)>.Some someLeft)
                {
                    var (leftString, leftCount) = someLeft.Value;
                    if (leftCount > 1)
                    {
                        var (rightResult, remainingRight) = new RepeatParser(Right).Parse(remainingLeft);
                        if (rightResult is Result<(string, int)>.Some someRight)
                        {
                            var (rightString, rightCount) = someRight.Value;
                            if (rightCount > 0 && leftCount > rightCount)
                            {
                                return (Result.Some(leftString + rightString), remainingRight);
                            }
                        }
                    }
                }

                return (Result.None<string>(), text);
            }
        }
    }
}
