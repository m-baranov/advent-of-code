using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020
{
    static class Day02
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "1-3 a: abcde",
                "1-3 b: cdefg",
                "2-9 c: ccccccccc"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/2/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var validCount = 0;

                foreach (var line in input.Lines())
                {
                    var (policy, password) = Policy.Parse(line);
                    if (policy.IsValid(password))
                    {
                        validCount++;
                    }
                }

                Console.WriteLine(validCount);
            }

            private class Policy
            {
                public static (Policy, string) Parse(string text)
                {
                    var (policyText, passwordText) = SplitBy(text, ':');
                    
                    var policy = ParsePolicy(policyText);
                    
                    return (policy, passwordText);
                }

                private static Policy ParsePolicy(string text)
                {
                    var (rangeText, ch) = SplitBy(text, ' ');
                    var (minText, maxText) = SplitBy(rangeText, '-');

                    return new Policy(ch[0], int.Parse(minText), int.Parse(maxText));
                }

                private static (string, string) SplitBy(string line, char ch)
                {
                    var index = line.IndexOf(ch);
                    
                    var left = line.Substring(0, index).Trim();
                    var right = line.Substring(index + 1).Trim();

                    return (left, right);
                }

                private readonly char requiredChar;
                private readonly int min;
                private readonly int max;

                public Policy(char ch, int min, int max)
                {
                    this.requiredChar = ch;
                    this.min = min;
                    this.max = max;
                }

                public bool IsValid(string password)
                {
                    var occurrences = password.Where(c => c == this.requiredChar).Count();
                    return this.min <= occurrences && occurrences <= this.max;
                }
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var validCount = 0;

                foreach (var line in input.Lines())
                {
                    var (policy, password) = Policy.Parse(line);
                    if (policy.IsValid(password))
                    {
                        validCount++;
                    }
                }

                Console.WriteLine(validCount);
            }

            private class Policy
            {
                public static (Policy, string) Parse(string text)
                {
                    var (policyText, passwordText) = SplitBy(text, ':');

                    var policy = ParsePolicy(policyText);

                    return (policy, passwordText);
                }

                private static Policy ParsePolicy(string text)
                {
                    var (rangeText, ch) = SplitBy(text, ' ');
                    var (leftIndexText, rightIndexText) = SplitBy(rangeText, '-');

                    return new Policy(ch[0], int.Parse(leftIndexText) - 1, int.Parse(rightIndexText) - 1);
                }

                private static (string, string) SplitBy(string line, char ch)
                {
                    var index = line.IndexOf(ch);

                    var left = line.Substring(0, index).Trim();
                    var right = line.Substring(index + 1).Trim();

                    return (left, right);
                }

                private readonly char requiredChar;
                private readonly int leftIndex;
                private readonly int rightIndex;

                public Policy(char ch, int leftIndex, int rightIndex)
                {
                    this.requiredChar = ch;
                    this.leftIndex = leftIndex;
                    this.rightIndex = rightIndex;
                }

                public bool IsValid(string password)
                {
                    var leftValid = password[this.leftIndex] == this.requiredChar;
                    var rightValid = password[this.rightIndex] == this.requiredChar;
                    return leftValid ^ rightValid;
                }
            }
        }

    }
}
