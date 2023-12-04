using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day04
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
Card 1: 41 48 83 86 17 | 83 86  6 31 17  9 48 53
Card 2: 13 32 20 16 61 | 61 30 68 82 17 32 24 19
Card 3:  1 21 53 59 44 | 69 82 63 72 16 21 14  1
Card 4: 41 92 73 84 69 | 59 84 76 51 58  5 54 83
Card 5: 87 83 26 28 32 | 88 30 70 12 93 22 82 36
Card 6: 31 18 13 56 72 | 74 77 10 23 35 67 36 11
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/4/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            static long Score(Card card)
            {
                var count = card.CountMatchingNumbers();
                return count == 0 ? 0L : (long)Math.Pow(2, count - 1);
            }

            var cards = input.Lines()
                .Select(Card.Parse)
                .ToList();

            var sum = cards
                .Select(Score)
                .Sum();

            Console.WriteLine(sum);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var cards = input.Lines()
                .Select(Card.Parse)
                .ToList();

            var counts = new int[cards.Count];

            for (var c = 0; c < cards.Count; c++)
            {
                counts[c] = 1;
            }

            for (var c = 0; c < cards.Count; c++)
            {
                var count = counts[c];
                var matches = cards[c].CountMatchingNumbers();
                for (var m = 0; m < matches; m++)
                {
                    counts[c + m + 1] += count;
                }
            }

            var sum = counts.Sum();
            Console.WriteLine(sum);
        }
    }

    private record Card(int Index, ISet<int> WinningNumbers, ISet<int> YourNumbers)
    {
        public static Card Parse(string text)
        {
            static (string left, string right) Split(string text, string sep)
            {
                var index = text.IndexOf(sep);
                return (text.Substring(0, index), text.Substring(index + sep.Length));
            }

            static int ParseCardIndex(string text)
            {
                const string Prefix = "Card ";
                return int.Parse(text.Substring(Prefix.Length));
            }

            static IEnumerable<int> ParseNumbers(string text) =>
                text
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse);
               
            var (cardText, numbersText) = Split(text, ": ");
            var (winningText, yoursText) = Split(numbersText, " | ");

            var cardIndex = ParseCardIndex(cardText);
            var winningNumbers = ParseNumbers(winningText).ToHashSet();
            var yourNumbers = ParseNumbers(yoursText).ToHashSet();

            return new Card(cardIndex, winningNumbers, yourNumbers);
        }

        public int CountMatchingNumbers() =>
            WinningNumbers.Intersect(YourNumbers).Count();
    }
}
