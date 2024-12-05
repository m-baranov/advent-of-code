using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2024;

static class Day03
{
    public static class Inputs
    {
        public static readonly IInput Sample1 =
            Input.Literal(""""""
xmul(2,4)%&mul[3,7]!@^do_not_mul(5,5)+mul(32,64]then(mul(11,8)mul(8,5))
"""""");

        public static readonly IInput Sample2 =
            Input.Literal(""""""
xmul(2,4)&mul[3,7]!^don't()_mul(5,5)+mul(32,64](mul(11,8)undo()?mul(8,5))
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/3/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var text = string.Join(string.Empty, input.Lines());

            var parser = MulParser();

            var sum = 0L;
            foreach (var pair in FindAll(parser, text))
            {
                sum += pair.left * pair.right;
            }

            Console.WriteLine(sum);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var text = string.Join(string.Empty, input.Lines());

            var parser = CallParser();

            var sum = 0L;
            var enabled = true;
            foreach (var call in FindAll(parser, text))
            {
                if (call.Command == Command.Do)
                {
                    enabled = true;
                }
                else if (call.Command == Command.Dont)
                {
                    enabled = false;
                }
                else if (enabled)
                {
                    sum += call.Left * call.Right;
                }
            }

            Console.WriteLine(sum);
        }

        private static IParser<Call> CallParser()
        {
            var mulParser = Parser.Map(
                MulParser(),
                p => new Call(Command.Mul, p.left, p.right)
            );

            var doParser = Parser.Map(
                Parser.Text("do()"),
                _ => new Call(Command.Do)
            );

            var dontParser = Parser.Map(
                Parser.Text("don't()"),
                _ => new Call(Command.Dont)
            );

            return Parser.OneOf(new[]
            {
                mulParser,
                doParser,
                dontParser,
            });
        }

        private enum Command { Mul, Do, Dont };

        private record Call(Command Command, int Left = 0, int Right = 0);
    }

    private static IParser<(int left, int right)> MulParser()
    {
        var mulParser = Parser.Text("mul(");

        var digitParser = Parser.OneOf(new[]
        {
            Parser.Char('0'),
            Parser.Char('1'),
            Parser.Char('2'),
            Parser.Char('3'),
            Parser.Char('4'),
            Parser.Char('5'),
            Parser.Char('6'),
            Parser.Char('7'),
            Parser.Char('8'),
            Parser.Char('9'),
        });

        var numberParser = Parser.Map(
            Parser.Repeat(digitParser, min: 1, max: 3),
            digits => int.Parse(string.Join(string.Empty, digits))
        );

        var numberPairParser = Parser.Map(
            Parser.FollowedBy(
                Parser.FollowedBy(numberParser, Parser.Char(',')),
                numberParser
            ),
            p => (left: p.left.left, right: p.right)
        );

        return Parser.Map(
            Parser.FollowedBy(
                Parser.FollowedBy(
                    mulParser,
                    numberPairParser
                ),
                Parser.Char(')')
            ),
            p => p.left.right
        );
    }

    private static IEnumerable<T> FindAll<T>(IParser<T> parser, string text)
    {
        var index = 0;
        while (index < text.Length)
        {
            var result = parser.Parse(text, index);
            if (result.Ok)
            {
                yield return result.Value;
                index = result.NextIndex;
            }
            else
            {
                index++;
            }
        }
    }

    private record Result<T>(bool Ok, T Value, int NextIndex);

    private static class Result
    {
        public static Result<T> Ok<T>(T value, int index) =>
            new(Ok: true, Value: value, NextIndex: index);

        public static Result<T> NotOk<T>() =>
            new(Ok: false, Value: default!, NextIndex: default);
    }

    private interface IParser<T>
    {
        Result<T> Parse(string text, int index);
    }

    private static class Parser
    {
        public static IParser<string> Text(string text) =>
            Map(Sequence(text.Select(Char).ToList()), _ => text);

        public static IParser<char> Char(char ch) =>
            new CharParser(ch);

        public static IParser<(TLeft left, TRight right)> FollowedBy<TLeft, TRight>(
            IParser<TLeft> left,
            IParser<TRight> right) =>
            new FollowedByParser<TLeft, TRight>(left, right);

        public static IParser<IReadOnlyList<T>> Sequence<T>(
            IReadOnlyList<IParser<T>> parsers) =>
            new SequenceParser<T>(parsers);

        public static IParser<T> OneOf<T>(
            IReadOnlyList<IParser<T>> parsers) =>
            new OneOfParser<T>(parsers);

        public static IParser<IReadOnlyList<T>> Repeat<T>(
            IParser<T> parser,
            int min,
            int max) =>
            new RepeatParser<T>(parser, min, max);

        public static IParser<TOut> Map<TIn, TOut>(
            IParser<TIn> parser,
            Func<TIn, TOut> map) =>
            new MapParser<TIn, TOut>(parser, map);

        private class CharParser : IParser<char>
        {
            private readonly char expected;

            public CharParser(char expected)
            {
                this.expected = expected;
            }

            public Result<char> Parse(string text, int index)
            {
                if (index >= text.Length)
                {
                    return Result.NotOk<char>();
                }

                if (text[index] == this.expected)
                {
                    return Result.Ok(this.expected, index + 1);
                }

                return Result.NotOk<char>();
            }
        }

        private class FollowedByParser<TLeft, TRight> : IParser<(TLeft left, TRight right)>
        {
            private readonly IParser<TLeft> leftParser;
            private readonly IParser<TRight> rightParser;

            public FollowedByParser(
                IParser<TLeft> leftParser,
                IParser<TRight> rightParser)
            {
                this.leftParser = leftParser;
                this.rightParser = rightParser;
            }

            public Result<(TLeft left, TRight right)> Parse(string text, int index)
            {
                var leftResult = this.leftParser.Parse(text, index);
                if (!leftResult.Ok)
                {
                    return Result.NotOk<(TLeft left, TRight right)>();
                }

                var rightResult = this.rightParser.Parse(text, leftResult.NextIndex);
                if (!rightResult.Ok)
                {
                    return Result.NotOk<(TLeft left, TRight right)>();
                }

                return Result.Ok((leftResult.Value, rightResult.Value), rightResult.NextIndex);
            }
        }

        private class SequenceParser<T> : IParser<IReadOnlyList<T>>
        {
            private readonly IReadOnlyList<IParser<T>> parsers;

            public SequenceParser(IReadOnlyList<IParser<T>> parsers)
            {
                this.parsers = parsers;
            }

            public Result<IReadOnlyList<T>> Parse(string text, int index)
            {
                var results = new List<T>();
                var nextIndex = index;

                foreach (var parser in this.parsers)
                {
                    var result = parser.Parse(text, nextIndex);
                    if (!result.Ok)
                    {
                        return Result.NotOk<IReadOnlyList<T>>();
                    }

                    results.Add(result.Value);
                    nextIndex = result.NextIndex;
                }

                return Result.Ok<IReadOnlyList<T>>(results, nextIndex);
            }
        }

        private class OneOfParser<T> : IParser<T>
        {
            private readonly IReadOnlyList<IParser<T>> parsers;

            public OneOfParser(IReadOnlyList<IParser<T>> parsers)
            {
                this.parsers = parsers;
            }

            public Result<T> Parse(string text, int index)
            {
                foreach (var parser in this.parsers)
                {
                    var result = parser.Parse(text, index);
                    if (result.Ok)
                    {
                        return result;
                    }
                }

                return Result.NotOk<T>();
            }
        }

        private class RepeatParser<T> : IParser<IReadOnlyList<T>>
        {
            private readonly IParser<T> parser;
            private readonly int min;
            private readonly int max;

            public RepeatParser(
                IParser<T> parser,
                int min,
                int max)
            {
                this.parser = parser;
                this.min = min;
                this.max = max;
            }

            public Result<IReadOnlyList<T>> Parse(string text, int index)
            {
                var results = new List<T>();
                var nextIndex = index;

                for (var i = 0; i < this.max; i++)
                {
                    var result = this.parser.Parse(text, nextIndex);
                    if (!result.Ok)
                    {
                        break;
                    }

                    results.Add(result.Value);
                    nextIndex = result.NextIndex;
                }

                if (results.Count < this.min)
                {
                    return Result.NotOk<IReadOnlyList<T>>();
                }

                return Result.Ok<IReadOnlyList<T>>(results, nextIndex);
            }
        }

        private class MapParser<TIn, TOut> : IParser<TOut>
        {
            private readonly IParser<TIn> parser;
            private readonly Func<TIn, TOut> map;

            public MapParser(
                IParser<TIn> parser,
                Func<TIn, TOut> map)
            {
                this.parser = parser;
                this.map = map;
            }

            public Result<TOut> Parse(string text, int index)
            {
                var result = this.parser.Parse(text, index);
                if (result.Ok)
                {
                    return Result.Ok(this.map(result.Value), result.NextIndex);
                }
                else
                {
                    return Result.NotOk<TOut>();
                }
            }
        }
    }
}
