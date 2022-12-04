using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day02
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "A Y",
                    "B X",
                    "C Z"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/2/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var moves = input.Lines().Select(ParseMove);

                var score = moves
                    .Select(m => Game.Score(m.Shape, m.Response))
                    .Sum();

                Console.WriteLine(score);
            }

            private record Move(Shape Shape, Shape Response);

            private static Move ParseMove(string text)
            {
                var chars = text.Split(' ');
                return new Move(Game.ParseShape(chars[0][0]), Game.ParseShape(chars[1][0]));
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var moves = input.Lines().Select(ParseMove);

                var score = moves
                    .Select(m => Game.Score(m.Shape, Game.DetermineYourShape(m.Shape, m.Outcome)))
                    .Sum();

                Console.WriteLine(score);
            }

            private record Move(Shape Shape, Outcome Outcome);

            private static Move ParseMove(string text)
            {
                var chars = text.Split(' ');
                return new Move(Game.ParseShape(chars[0][0]), Game.ParseOutcome(chars[1][0]));
            }
        }

        private enum Shape { Rock, Paper, Scissors }

        private enum Outcome { Draw, YouWin, YouLose }

        private static class Game
        {
            public static Shape ParseShape(char ch) =>
                ch switch
                {
                    'A' or 'X' => Shape.Rock,
                    'B' or 'Y' => Shape.Paper,
                    'C' or 'Z' or _ => Shape.Scissors,
                };

            public static Outcome ParseOutcome(char ch) =>
                ch switch
                {
                    'X' => Outcome.YouLose,
                    'Y' => Outcome.Draw,
                    'Z' or _ => Outcome.YouWin,
                };

            public static int Score(Shape opponents, Shape yours)
            {
                static int ScoreShape(Shape shape) =>
                    shape switch
                    {
                        Shape.Rock => 1,
                        Shape.Paper => 2,
                        Shape.Scissors or _ => 3
                    };

                static int ScoreOutcome(Outcome outcome) =>
                    outcome switch
                    {
                        Outcome.YouWin => 6,
                        Outcome.Draw => 3,
                        Outcome.YouLose or _ => 0,
                    };

                var outcome = DetermineOutcome(opponents, yours);

                return ScoreShape(yours) + ScoreOutcome(outcome);
            }

            public static Outcome DetermineOutcome(Shape opponents, Shape yours) =>
                (opponents, yours) switch
                {
                    (Shape.Scissors, Shape.Paper) or
                    (Shape.Paper, Shape.Rock) or
                    (Shape.Rock, Shape.Scissors) => Outcome.YouLose,

                    (var o, var y) when o == y => Outcome.Draw,

                    _ => Outcome.YouWin
                };

            public static Shape DetermineYourShape(Shape opponents, Outcome outcome) =>
                (opponents, outcome) switch
                {
                    (Shape.Scissors, Outcome.YouLose) => Shape.Paper,
                    (Shape.Scissors, Outcome.YouWin) => Shape.Rock,

                    (Shape.Paper, Outcome.YouLose) => Shape.Rock,
                    (Shape.Paper, Outcome.YouWin) => Shape.Scissors,

                    (Shape.Rock, Outcome.YouLose) => Shape.Scissors,
                    (Shape.Rock, Outcome.YouWin) => Shape.Paper,

                    (_, Outcome.Draw) or _ => opponents,
                };
        }
    }
}
