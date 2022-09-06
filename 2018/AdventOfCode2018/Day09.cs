using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day09
    {
        public static class Inputs
        {
            public static readonly IInput Sample1 =
                Input.Literal("10 players; last marble is worth 1618 points");

            public static readonly IInput Sample2 =
                Input.Literal("13 players; last marble is worth 7999 points");

            public static readonly IInput Sample3 =
                Input.Literal("17 players; last marble is worth 1104 points");

            public static readonly IInput Sample4 =
                Input.Literal("21 players; last marble is worth 6111 points");

            public static readonly IInput Sample5 =
                Input.Literal("30 players; last marble is worth 5807 points");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/9/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var highScore = Game.Simulate(input.Lines().First());
                Console.WriteLine(highScore);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var highScore = Game.Simulate(input.Lines().First(), moveCountMultiplier: 100);
                Console.WriteLine(highScore);
            }
        }

        private class Game
        {
            public static long Simulate(string input, int moveCountMultiplier = 1)
            {
                var (playerCount, moveCount) = Parse(input);

                var game = new Game(playerCount);
                game.MakeMoves(moveCount * moveCountMultiplier);

                return game.HighScore();
            }

            private static (int playerCount, int moveCount) Parse(string text)
            {
                // 0  1        2    3      4  5     6    7
                // 10 players; last marble is worth 1618 points

                var words = text.Split(' ');
                
                var playerCount = int.Parse(words[0]);
                var moveCount = int.Parse(words[6]);

                return (playerCount, moveCount);
            }

            private Ring ring;
            private readonly long[] playerScores;
            private int currentPlayer;

            public Game(int playerCount)
            {
                this.ring = new Ring(0);
                this.playerScores = new long[playerCount];
                this.currentPlayer = 0;
            }

            public long HighScore() => this.playerScores.Max();

            public void MakeMoves(long moveCount)
            {
                for (var i = 1; i <= moveCount; i++)
                {
                    this.MakeMove(i);
                }
            }

            private void MakeMove(int value)
            {
                var scoreIncrement = value % 23 == 0 ? Remove(value) : Add(value);

                this.playerScores[this.currentPlayer] += scoreIncrement;

                this.currentPlayer = (this.currentPlayer + 1) % this.playerScores.Length;
            }

            private long Add(int value)
            {
                var toInsertAfter = Ring.Next(this.ring, 1);
                this.ring = Ring.InsertAfter(toInsertAfter, value);
                return 0;
            }

            private long Remove(int value)
            {
                var toRemove = Ring.Prev(this.ring, 7);
                this.ring = Ring.Remove(toRemove);
                return value + toRemove.Value;
            }
        }

        private class Ring
        {
            private readonly long value;
            private Ring next;
            private Ring prev;

            public Ring(long value)
            {
                this.value = value;
                this.next = this;
                this.prev = this;
            }

            private Ring(long value, Ring next, Ring prev)
            {
                this.value = value;
                this.next = next;
                this.prev = prev;
            }

            public long Value => this.value;

            public static Ring Next(Ring current, long steps)
            {
                var r = current;
                for (var i = 0; i < steps; i++)
                {
                    r = r.next;
                }
                return r;
            }

            public static Ring Prev(Ring current, long steps)
            {
                var r = current;
                for (var i = 0; i < steps; i++)
                {
                    r = r.prev;
                }
                return r;
            }

            public static Ring InsertAfter(Ring current, long value)
            {
                //   curr      next
                //  [    ] -> [    ]
                //  [    ] <- [    ]

                //   curr      NEW       next
                //  [    ] -> [    ] -> [    ]
                //  [    ] <- [    ] <- [    ]

                var next = current.next;

                var @new = new Ring(value, next: next, prev: current);

                current.next = @new;
                next.prev = @new;

                return @new;
            }

            public static Ring Remove(Ring current)
            {
                //   prev      curr      next
                //  [    ] -> [    ] -> [    ]
                //  [    ] <- [    ] <- [    ]

                //   prev      next
                //  [    ] -> [    ]
                //  [    ] <- [    ]

                var prev = current.prev;
                var next = current.next;

                prev.next = next;
                next.prev = prev;

                return next;
            }

            public static void Dump(Ring current)
            {
                var r = current;
                do
                {
                    Console.Write(r.Value);
                    Console.Write(" ");

                    r = r.next;
                } 
                while (r != current);

                Console.WriteLine();
            }
        }
    }
}
