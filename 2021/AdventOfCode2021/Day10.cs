using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day10
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "[({(<(())[]>[[{[]{<()<>>",
                    "[(()[<>])]({[<{<<[]>>(",
                    "{([(<{}[<>[]}>{[]{[(<()>",
                    "(((({<>}<{<{<>}{[]{[]{}",
                    "[[<[([]))<([[{}[[()]]]",
                    "[{[{({}]{}}([{[{{{}}([]",
                    "{<[[]]>}<{[{[{[]{()[[[]",
                    "[<(<(<(<{}))><([]([]()",
                    "<{([([[(<>()){}]>(<<{{",
                    "<{([{{}}[<[[[<>{}]]]>[]]"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/10/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var chunks = input.Lines();

                var answer = chunks
                    .Select(Analyser.Analyse)
                    .OfType<AnalysisResult.Error>()
                    .Select(r => ScoreError(r.UnexpectedClosing))
                    .Sum();

                Console.WriteLine(answer);
            }

            private long ScoreError(char closing) =>
                closing switch
                {
                    ')' => 3,
                    ']' => 57,
                    '}' => 1197,
                    '>' => 25137,
                    _ => 0
                };
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var chunks = input.Lines();

                var scores = chunks
                    .Select(Analyser.Analyse)
                    .OfType<AnalysisResult.Correct>()
                    .Select(r => Score(r.RemainingClosing))
                    .OrderBy(s => s)
                    .ToList();

                var score = scores[scores.Count / 2];
                Console.WriteLine(score);
            }

            private long Score(IReadOnlyList<char> closings) => 
                closings.Aggregate(0L, (acc, ch) => acc * 5 + Score(ch));

            private long Score(char closing) =>
                closing switch
                {
                    ')' => 1,
                    ']' => 2,
                    '}' => 3,
                    '>' => 4,
                    _ => 0
                };
        }

        private class Brackets
        {
            public static readonly IReadOnlyList<Brackets> All = new[] 
            {
                new Brackets('(', ')'),
                new Brackets('[', ']'),
                new Brackets('{', '}'),
                new Brackets('<', '>')
            };

            public static bool IsOpening(char ch) => All.Any(b => b.Opening == ch);

            public static char ClosingFor(char opening) => All.First(b => b.Opening == opening).Closing;

            public Brackets(char opnening, char closing)
            {
                Opening = opnening;
                Closing = closing;
            }

            public char Opening { get; }
            public char Closing { get; }
        }

        private static class Analyser
        {
            public static AnalysisResult Analyse(string chunk)
            {
                var openings = new Stack<char>();

                foreach (var ch in chunk)
                {
                    if (Brackets.IsOpening(ch))
                    {
                        openings.Push(ch);
                    }
                    else if (openings.Count == 0)
                    {
                        return new AnalysisResult.Error(ch);
                    }
                    else
                    {
                        var opening = openings.Pop();
                        if (ch != Brackets.ClosingFor(opening))
                        {
                            return new AnalysisResult.Error(ch);
                        }
                    }
                }

                var remainingClosings = openings.Select(Brackets.ClosingFor).ToList();
                return new AnalysisResult.Correct(remainingClosings);
            }
        }

        private abstract class AnalysisResult
        {
            public class Error : AnalysisResult
            {
                public Error(char unexpectedClosing)
                {
                    UnexpectedClosing = unexpectedClosing;
                }

                public char UnexpectedClosing { get; }
            }

            public class Correct : AnalysisResult
            {
                public Correct(IReadOnlyList<char> remainingClosing)
                {
                    RemainingClosing = remainingClosing;
                }

                public IReadOnlyList<char> RemainingClosing { get; }
            }
        }
    }
}
