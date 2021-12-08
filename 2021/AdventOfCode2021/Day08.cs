using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day08
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "be cfbegad cbdgef fgaecd cgeb fdcge agebfd fecdb fabcd edb | fdgacbe cefdb cefbgd gcbe",
                    "edbfga begcd cbg gc gcadebf fbgde acbgfd abcde gfcbed gfec | fcgedb cgb dgebacf gc",
                    "fgaebd cg bdaec gdafb agbcfd gdcbef bgcad gfac gcb cdgabef | cg cg fdcagb cbg",
                    "fbegcd cbd adcefb dageb afcb bc aefdc ecdab fgdeca fcdbega | efabcd cedba gadfec cb",
                    "aecbfdg fbg gf bafeg dbefa fcge gcbea fcaegb dgceab fcbdga | gecf egdcabf bgf bfgea",
                    "fgeab ca afcebg bdacfeg cfaedg gcfdb baec bfadeg bafgc acf | gebdcfa ecba ca fadegcb",
                    "dbcfg fgd bdegcaf fgec aegbdf ecdfab fbedc dacgb gdcebf gf | cefg dcbef fcge gbcadfe",
                    "bdfegc cbegaf gecbf dfcage bdacg ed bedf ced adcbefg gebcd | ed bcgafe cdgba cbgef",
                    "egadfb cdbfeg cegd fecab cgb gbdefca cg fgcdab egfdb bfceg | gbdfcae bgc cg cgb",
                    "gcafb gcf dcaebfg ecagb gf abcdeg gaef cafbge fdbac fegbdc | fgae cfgab fg bagce"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/8/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var displays = input.Lines().Select(Display.Parse).ToList();

                var answer = displays
                    .SelectMany(d => d.Digits)
                    .Where(d => d.Length == SegmentedDigit.One.Segments.Count ||
                                d.Length == SegmentedDigit.Four.Segments.Count || 
                                d.Length == SegmentedDigit.Seven.Segments.Count || 
                                d.Length == SegmentedDigit.Eight.Segments.Count)
                    .Count();

                Console.WriteLine(answer);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var displays = input.Lines().Select(Display.Parse).ToList();

                var answer = displays
                    .Select(DisplayedNumber)
                    .Sum();

                Console.WriteLine(answer);
            }

            private long DisplayedNumber(Display display)
            {
                var map = MapSegmentsToSignals(display.Patterns);

                var digits = display.Digits.Select(d => SegmentedDigit.Classify(d, map));

                return DigitsToNumber(digits);
            }

            private IReadOnlyDictionary<Segment, char> MapSegmentsToSignals(IReadOnlyList<string> patterns)
            {
                var pattern1 = patterns.First(p => p.Length == SegmentedDigit.One.Segments.Count);
                var pattern4 = patterns.First(p => p.Length == SegmentedDigit.Four.Segments.Count);
                var pattern7 = patterns.First(p => p.Length == SegmentedDigit.Seven.Segments.Count);
                var pattern8 = patterns.First(p => p.Length == SegmentedDigit.Eight.Segments.Count);

                var top = pattern7.Except(pattern1).First();

                var pattern9 = patterns
                    .Where(p => p.Length == SegmentedDigit.Nine.Segments.Count)
                    .First(p => p.Contains(top) && pattern4.All(p.Contains));
                
                var pattern6 = patterns
                    .Where(p => p.Length == SegmentedDigit.Six.Segments.Count)
                    .First(p => pattern1.Intersect(p).Count() == 1);

                var pattern0 = patterns
                    .Where(p => p.Length == SegmentedDigit.Zero.Segments.Count)
                    .First(p => p != pattern9 && p != pattern6);

                var bottomLeft = pattern8.Except(pattern9).First();
                var topRight = pattern1.Except(pattern6).First();
                var bottomRight = pattern1.Except(new[] { topRight }).First();
                var bottom = pattern6.Except(pattern4.Concat(new[] { top, bottomLeft })).First();
                var middle = pattern8.Except(pattern0).First();
                var topLeft = pattern4.Except(new[] { middle, topRight, bottomRight }).First();

                return new Dictionary<Segment, char>()
                {
                    [Segment.Top] = top,
                    [Segment.TopLeft] = topLeft,
                    [Segment.TopRight] = topRight,
                    [Segment.Middle] = middle,
                    [Segment.BottomLeft] = bottomLeft,
                    [Segment.BottomRight] = bottomRight,
                    [Segment.Bottom] = bottom,
                };
            }

            private long DigitsToNumber(IEnumerable<int> digits) => digits.Aggregate((acc, digit) => acc * 10 + digit);
        }

        public enum Segment
        {
            Top,
            TopLeft,
            TopRight,
            Middle,
            BottomLeft,
            BottomRight,
            Bottom
        }

        public class SegmentedDigit
        {
            public static readonly SegmentedDigit Zero = new SegmentedDigit(
                0, 
                new[] { Segment.Top, Segment.TopLeft, Segment.TopRight, Segment.BottomLeft, Segment.BottomRight, Segment.Bottom }
            );

            public static readonly SegmentedDigit One = new SegmentedDigit(
                1, 
                new[] { Segment.TopRight, Segment.BottomRight }
            );

            public static readonly SegmentedDigit Two = new SegmentedDigit(
                2, 
                new[] { Segment.Top, Segment.TopRight, Segment.Middle, Segment.BottomLeft, Segment.Bottom }
            );

            public static readonly SegmentedDigit Three = new SegmentedDigit(
                3, 
                new[] { Segment.Top, Segment.TopRight, Segment.Middle, Segment.BottomRight, Segment.Bottom }
            );

            public static readonly SegmentedDigit Four = new SegmentedDigit(
                4, 
                new[] { Segment.TopLeft, Segment.Middle, Segment.TopRight, Segment.BottomRight }
            );

            public static readonly SegmentedDigit Five = new SegmentedDigit(
                5, 
                new[] { Segment.Top, Segment.TopLeft, Segment.Middle, Segment.BottomRight, Segment.Bottom }
            );

            public static readonly SegmentedDigit Six = new SegmentedDigit(
                6, 
                new[] { Segment.Top, Segment.TopLeft, Segment.Middle, Segment.BottomLeft, Segment.BottomRight, Segment.Bottom }
            );

            public static readonly SegmentedDigit Seven = new SegmentedDigit(
                7, 
                new[] { Segment.Top, Segment.TopRight, Segment.BottomRight }
            );

            public static readonly SegmentedDigit Eight = new SegmentedDigit(
                8, 
                new[] { Segment.Top, Segment.TopLeft, Segment.TopRight, Segment.Middle, Segment.BottomLeft, Segment.BottomRight, Segment.Bottom 
            });

            public static readonly SegmentedDigit Nine = new SegmentedDigit(
                9, 
                new[] { Segment.Top, Segment.TopLeft, Segment.TopRight, Segment.Middle, Segment.BottomRight, Segment.Bottom }
            );
            
            public static readonly IReadOnlyList<SegmentedDigit> All = 
                new[] { Zero, One, Two, Three, Four, Five, Six, Seven, Eight, Nine };

            public static int Classify(string signals, IReadOnlyDictionary<Segment, char> segmentToSignalMap)
            {
                bool HasAll(string signals, IReadOnlyList<Segment> segments)
                {
                    if (segments.Count != signals.Length)
                    {
                        return false;
                    }

                    return segments.Select(s => segmentToSignalMap[s]).All(signals.Contains);
                }

                return SegmentedDigit.All.First(d => HasAll(signals, d.Segments)).Digit;
            }

            private SegmentedDigit(int digit, IReadOnlyList<Segment> segments)
            {
                Digit = digit;
                Segments = segments;
            }

            public int Digit { get; }
            public IReadOnlyList<Segment> Segments { get; }
        }

        public class Display
        {
            public static Display Parse(string text)
            {
                var parts = text.Split(" | ");

                var patterns = parts[0].Split(' ');
                var digits = parts[1].Split(' ');

                return new Display(patterns, digits);
            }

            public Display(IReadOnlyList<string> patterns, IReadOnlyList<string> digits)
            {
                Patterns = patterns;
                Digits = digits;
            }

            public IReadOnlyList<string> Patterns { get; }
            public IReadOnlyList<string> Digits { get; }
        }
    }
}
