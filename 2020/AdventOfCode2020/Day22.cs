using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2020
{
    static class Day22
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "Player 1:",
                "9",
                "2",
                "6",
                "3",
                "1",
                "",
                "Player 2:",
                "5",
                "8",
                "4",
                "7",
                "10"
            );

        public static readonly IInput TestInput =
           Input.Http("https://adventofcode.com/2020/day/22/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var (player1, player2) = Utils.ParseGame(input.Lines());

                var hand1 = new Hand(player1);
                var hand2 = new Hand(player2);

                while (!hand1.Empty() && !hand2.Empty())
                {
                    var card1 = hand1.Draw();
                    var card2 = hand2.Draw();

                    if (card1 > card2)
                    {
                        hand1.Return(card1, card2);
                    }
                    else
                    {
                        hand2.Return(card2, card1);
                    }
                }

                var winner = !hand1.Empty() ? hand1 : hand2;

                var score = winner.Score();

                Console.WriteLine(score);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var (player1, player2) = Utils.ParseGame(input.Lines());

                var (_, score) = Play(player1, player2);

                Console.WriteLine(score);
            }

            private (Player winner, int score) Play(IEnumerable<int> player1, IEnumerable<int> player2)
            {
                var pastRounds = new HashSet<RoundSnapshot>();

                var hand1 = new Hand(player1);
                var hand2 = new Hand(player2);

                while (!hand1.Empty() && !hand2.Empty())
                {
                    var round = new RoundSnapshot(hand1, hand2);
                    if (pastRounds.Contains(round))
                    {
                        return (Player.One, hand1.Score());
                    }
                    else
                    {
                        pastRounds.Add(round);
                    }

                    var card1 = hand1.Draw();
                    var card2 = hand2.Draw();

                    Player roundWinner;
                    if (hand1.Size() >= card1 && hand2.Size() >= card2)
                    {
                        var subHand1 = hand1.AsEnumerable().Take(card1);
                        var subHand2 = hand2.AsEnumerable().Take(card2);

                        var (subgameWinner, _) = Play(subHand1, subHand2);
                        roundWinner = subgameWinner;
                    }
                    else
                    {
                        roundWinner = card1 > card2 ? Player.One : Player.Two;
                    }

                    if (roundWinner == Player.One)
                    {
                        hand1.Return(card1, card2);
                    }
                    else
                    {
                        hand2.Return(card2, card1);
                    }
                }

                return !hand1.Empty() 
                    ? (Player.One, hand1.Score()) 
                    : (Player.Two, hand2.Score());
            }
        }

        public enum Player { One, Two }

        public class RoundSnapshot
        {
            private readonly IReadOnlyList<int> hand1;
            private readonly IReadOnlyList<int> hand2;

            public RoundSnapshot(Hand hand1, Hand hand2)
            {
                this.hand1 = hand1.AsEnumerable().ToList();
                this.hand2 = hand2.AsEnumerable().ToList();
            }

            public override bool Equals(object obj)
            {
                if (obj is not RoundSnapshot other)
                {
                    return false;
                }

                return hand1.Count == other.hand1.Count
                    && hand2.Count == other.hand2.Count
                    && hand1.SequenceEqual(other.hand1)
                    && hand2.SequenceEqual(other.hand2);
            }

            public override int GetHashCode()
            {
                return hand1.Concat(hand2).Aggregate((acc, card) => acc ^ card);
            }
        }

        public class Hand
        {
            private readonly Queue<int> cards;

            public Hand(IEnumerable<int> initial)
            {
                cards = new Queue<int>(initial);
            }

            public bool Empty() => Size() == 0;

            public int Size() => cards.Count;

            public int Draw() => cards.Dequeue();

            public void Return(int card1, int card2)
            {
                cards.Enqueue(card1);
                cards.Enqueue(card2);
            }

            public int Score() => cards.Select((card, index) => card * (cards.Count - index)).Sum();

            public IEnumerable<int> AsEnumerable() => cards.AsEnumerable();

            public override string ToString() => string.Join(",", cards);
        }

        public static class Utils
        {
            public static (IReadOnlyList<int>, IReadOnlyList<int>) ParseGame(IEnumerable<string> lines)
            {
                var groups = SplitByEmptyLine(lines).ToList();

                var first = ParseHand(groups[0]);
                var second = ParseHand(groups[1]);

                return (first, second);
            }

            private static IReadOnlyList<int> ParseHand(IReadOnlyList<string> lines)
            {
                return lines
                    .Skip(1)  // Player N:
                    .Select(int.Parse)
                    .ToList();
            }

            private static IEnumerable<IReadOnlyList<string>> SplitByEmptyLine(IEnumerable<string> lines)
            {
                var group = new List<string>();

                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        if (group.Count > 0)
                        {
                            yield return group;
                            group = new List<string>();
                        }
                    }
                    else
                    {
                        group.Add(line);
                    }
                }

                if (group.Count > 0)
                {
                    yield return group;
                }
            }
        }
    }
}
