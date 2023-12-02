using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day02
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green
Game 2: 1 blue, 2 green; 3 green, 4 blue, 1 red; 1 green, 1 blue
Game 3: 8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red
Game 4: 1 green, 3 red, 6 blue; 3 green, 6 red; 3 green, 15 blue, 14 red
Game 5: 6 red, 1 blue, 3 green; 2 blue, 1 red, 2 green
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/2/input");
    }

    public class Part1 : IProblem
    {
        private static readonly Set Bag = new(Red: 12, Green: 13, Blue: 14);

        public void Run(TextReader input)
        {
            static bool IsSetPossible(Set set, Set bag) =>
                set.Red <= bag.Red &&
                set.Green <= bag.Green &&
                set.Blue <= bag.Blue;

            static bool IsGamePossible(Game game, Set bag) =>
                game.Sets.All(set => IsSetPossible(set, bag));

            var games = input.Lines().Select(Game.Parse).ToList();

            var sum = games
                .Where(game => IsGamePossible(game, Bag))
                .Select(game => game.Id)
                .Sum();

            Console.WriteLine(sum);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            static Set Max(Set a, Set b) =>
                new(
                    Red: Math.Max(a.Red, b.Red),
                    Green: Math.Max(a.Green, b.Green),
                    Blue: Math.Max(a.Blue, b.Blue)
                );

            static Set MinBag(Game game) =>
                game.Sets.Aggregate(Set.Zero, Max);

            static long Power(Set set) =>
                (long)set.Red * set.Green * set.Blue;

            var games = input.Lines().Select(Game.Parse).ToList();

            var sum = games.Select(MinBag).Select(Power).Sum();

            Console.WriteLine(sum);
        }
    }

    private record Set(int Red, int Green, int Blue)
    {
        public static readonly Set Zero = new(0, 0, 0);

        public static Set Parse(string text)
        {
            static (string kind, int count) ParseOne(string text)
            {
                var parts = text.Split(' ');
                return (kind: parts[1], count: int.Parse(parts[0]));
            }

            static Set Add(Set set, string kind, int count) =>
                kind switch
                {
                    "red" => set with { Red = set.Red + count },
                    "green" => set with { Green = set.Green + count },
                    "blue" => set with { Blue = set.Blue + count },
                    _ => set
                };

            var parts = text.Split(", ");

            return parts
                .Select(ParseOne)
                .Aggregate(Zero, (result, pair) => Add(result, pair.kind, pair.count));
        }
    }
   
    private record Game(int Id, IReadOnlyList<Set> Sets)
    {
        public static Game Parse(string text)
        {
            static (string left, string right) Split(string text, string sep)
            {
                var index = text.IndexOf(sep);
                return (
                    left: text.Substring(0, index), 
                    right: text.Substring(index + sep.Length)
                );
            }

            static int ParseId(string text)
            {
                const string Prefix = "Game ";
                return int.Parse(text.Substring(Prefix.Length));
            }

            var (idText, setsText) = Split(text, ": ");
            var setTexts = setsText.Split("; ");

            var id = ParseId(idText);
            var sets = setTexts.Select(Set.Parse).ToList();

            return new Game(id, sets);
        }
    }
}
