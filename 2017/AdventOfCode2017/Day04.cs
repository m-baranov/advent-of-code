using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day04
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "aa bb cc dd ee",
                    "aa bb cc dd aa",
                    "aa bb cc dd aaa"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/4/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var passphrases = input.Lines();

                var count = passphrases.Count(IsValid);
                
                Console.WriteLine(count);
            }

            public static bool IsValid(string passphrase)
            {
                var words = passphrase.Split(' ');

                var seen = new HashSet<string>();
                foreach (var word in words)
                {
                    if (seen.Contains(word))
                    {
                        return false;
                    }
                    seen.Add(word);
                }
                return true;
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var passphrases = input.Lines();

                var count = passphrases.Count(IsValid);

                Console.WriteLine(count);
            }

            public static bool IsValid(string passphrase)
            {
                static string Normalize(string word) =>
                    string.Join("", word.OrderBy(ch => ch));

                var words = passphrase.Split(' ');

                var seen = new HashSet<string>();
                foreach (var word in words)
                {
                    var normalized = Normalize(word);
                    if (seen.Contains(normalized))
                    {
                        return false;
                    }
                    seen.Add(normalized);
                }
                return true;
            }
        }
    }
}
