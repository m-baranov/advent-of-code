using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day01
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
1abc2
pqr3stu8vwx
a1b2c3d4e5f
treb7uchet
"""""");

        public static readonly IInput Sample2 =
    Input.Literal(""""""
two1nine
eightwothree
abcone2threexyz
xtwone3four
4nineeightseven2
zoneight234
7pqrstsixteen
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/1/input");
    }

    public class Part1 : IProblem
    {
        private static readonly char[] DigitChars =
            new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public void Run(TextReader input)
        {
            static int DigitCharToInt(char ch) => ch - '0';

            static int FirstDigit(string text)
            {
                var index = text.IndexOfAny(DigitChars);
                return DigitCharToInt(text[index]);
            }

            static int LastDigit(string text)
            {
                var index = text.LastIndexOfAny(DigitChars);
                return DigitCharToInt(text[index]);
            }

            static int ValueOf(string text)
            {
                var first = FirstDigit(text);
                var last = LastDigit(text);
                return first * 10 + last;
            }

            var sum = input.Lines()
                .Select(ValueOf)
                .Aggregate(0L, (sum, num) => sum + num);

            Console.WriteLine(sum);
        }
    }

    public class Part2 : IProblem
    {
        private static readonly string[] DigitWords =
            new[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

        public void Run(TextReader input)
        {
            static bool ContainsAt(string text, string substring, int index)
            {
                if (index + substring.Length > text.Length)
                {
                    return false;
                }

                for (var i = 0; i < substring.Length; i++)
                {
                    if (text[i + index] != substring[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            static int? DigitWordAt(string text, int index)
            {
                for (var i = 0; i < DigitWords.Length; i++)
                {
                    if (ContainsAt(text, DigitWords[i], index))
                    {
                        return i + 1; // words start with "one"
                    }
                }
                return null;
            }

            static int? DigitCharAt(string text, int index)
            {
                var ch = text[index];
                return '0' <= ch && ch <= '9' ? ch - '0' : null;
            }

            static int? DigitAt(string text, int index) =>
               DigitCharAt(text, index) ?? DigitWordAt(text, index);
            
            static int FirstDigit(string text)
            {
                for (var i = 0; i < text.Length; i++)
                {
                    var digit = DigitAt(text, i);
                    if (digit is not null)
                    {
                        return digit.Value;
                    }
                }

                throw new Exception("impossible");
            }

            static int LastDigit(string text)
            {
                for (var i = text.Length - 1; i >= 0; i--)
                {
                    var digit = DigitAt(text, i);
                    if (digit is not null)
                    {
                        return digit.Value;
                    }
                }

                throw new Exception("impossible");
            }

            static int ValueOf(string text)
            {
                var first = FirstDigit(text);
                var last = LastDigit(text);
                return first * 10 + last;
            }

            var sum = input.Lines()
                .Select(ValueOf)
                .Aggregate(0L, (sum, num) => sum + num);

            Console.WriteLine(sum);
        }
    }
}
