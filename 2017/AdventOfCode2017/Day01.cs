using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day01
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal("123123");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/1/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var text = input.Lines().First();

                var sum = NumbersOf(text).Where(p => p.current == p.next).Select(p => p.current).Sum();

                Console.WriteLine(sum);
            }

            private static IEnumerable<(int current, int next)> NumbersOf(string text)
            {
                static int ToNumber(char ch) => ch - '0';

                char current, next;

                for (var i = 0; i < text.Length - 1; i++)
                {
                    current = text[i];
                    next = text[i + 1];

                    yield return (ToNumber(current), ToNumber(next));
                }

                current = text[text.Length - 1];
                next = text[0];
                yield return (ToNumber(current), ToNumber(next));
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var text = input.Lines().First();

                var sum = NumbersOf(text).Where(p => p.current == p.next).Select(p => p.current).Sum();

                Console.WriteLine(sum);
            }

            private static IEnumerable<(int current, int next)> NumbersOf(string text)
            {
                static int ToNumber(char ch) => ch - '0';

                for (var i = 0; i < text.Length; i++)
                {
                    var current = text[i];
                    var next = text[(i + text.Length / 2) % text.Length];

                    yield return (ToNumber(current), ToNumber(next));
                }
            }
        }
    }
}
