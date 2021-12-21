using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day21
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "Player 1 starting position: 4",
                    "Player 2 starting position: 8"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/21/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                const int MaxScore = 1000;

                var (player1, player2) = Util.Parse(input.Lines());

                var rolls = DeterministicDiceX3().GetEnumerator();
                rolls.MoveNext();

                var move = 0;
                while (player1.Score < MaxScore && player2.Score < MaxScore)
                {
                    if (move % 2 == 0)
                    {
                        player1 = player1.Move(rolls.Current);
                    }
                    else
                    {
                        player2 = player2.Move(rolls.Current);
                    }

                    rolls.MoveNext();
                    move += 1;
                }

                var loosingPlayerScore = Math.Min(player1.Score, player2.Score);
                Console.WriteLine(move * 3 * loosingPlayerScore);
            }

            private static IEnumerable<int> DeterministicDiceX3() =>
                DeterministicDice().Chunk(chunkSize: 3).Select(rolls => rolls.Sum());

            private static IEnumerable<int> DeterministicDice()
            {
                while (true)
                {
                    for (var i = 1; i <= 100; i++)
                    {
                        yield return i;
                    }
                }
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var oneRoll = Enumerable.Range(1, count: 3);

                // 3 roll sum -> possible roll combination count
                var threeRolls = 
                    (
                        from x in oneRoll
                        from y in oneRoll
                        from z in oneRoll
                        select x + y + z
                    )
                    .GroupBy(s => s)
                    .ToDictionary(g => g.Key, g => g.Count());

                var possiblePlayerRolls =
                    (
                        from player1Roll in threeRolls
                        from player2Roll in threeRolls
                        select (
                            player1: player1Roll.Key, 
                            player2: player2Roll.Key, 
                            combinationsCount: player1Roll.Value * player2Roll.Value
                        )
                    )
                    .ToList();

                const int MaxScore = 21;

                var (player1, player2) = Util.Parse(input.Lines());

                IReadOnlyList<Counter<Game>> counters = new[]
                {
                    new Counter<Game>(new Game(player1, player2), count: 1)
                };

                var player1wins = 0L;
                var player2wins = 0L;

                while (counters.Count > 0)
                {
                    var next = 
                        (
                            from counter in counters
                            from rolls in possiblePlayerRolls
                            select new Counter<Game>(
                                counter.Value.Move(rolls.player1, rolls.player2),
                                counter.Count * rolls.combinationsCount
                            )
                        )
                        .SumCounts()
                        .Select(counter => (counter, state: counter.Value.State(MaxScore)));

                    player1wins += next
                        .Where(p => p.state == GameState.Player1Won)
                        .Select(p => p.counter.Count)
                        .Sum();

                    player2wins += next
                        .Where(p => p.state == GameState.Player2Won)
                        .Select(p => p.counter.Count)
                        .Sum();

                    counters = next
                        .Where(p => p.state == GameState.InProgress)
                        .Select(p => p.counter)
                        .ToList();
                }

                // When player 1 wins, the loop above still counts all possible player 2 moves.
                // So player 1 win count is 27 times larger than it actually is.
                Console.WriteLine($"Player 1: {player1wins / threeRolls.Values.Sum()}");
                Console.WriteLine($"Player 2: {player2wins}");
            }
        }

        private class Game
        {
            public Game(Player player1, Player player2)
            {
                Player1 = player1;
                Player2 = player2;
            }

            public Player Player1 { get; }
            public Player Player2 { get; }
            public int MaxScore { get; }

            public override bool Equals(object obj) =>
                obj is Game g ? Player1.Equals(g.Player1) && Player2.Equals(g.Player2) : false;

            public override int GetHashCode() => HashCode.Combine(Player1, Player2);

            public Game Move(int dieRollSum1, int dieRollSum2) => 
                new Game(Player1.Move(dieRollSum1), Player2.Move(dieRollSum2));

            public GameState State(int maxScore)
            {
                if (Player1.Score >= maxScore)
                {
                    return GameState.Player1Won;
                }
                if (Player2.Score >= maxScore)
                {
                    return GameState.Player2Won;
                }
                return GameState.InProgress;
            }
        }

        private enum GameState { InProgress, Player1Won, Player2Won };

        private class Player
        {
            public const int PositionCount = 10;

            public Player(int position, int score)
            {
                Position = position;
                Score = score;
            }

            public int Position { get; }
            public int Score { get; }

            public override bool Equals(object obj) =>
                obj is Player p ? Position == p.Position && Score == p.Score : false;

            public override int GetHashCode() => HashCode.Combine(Position, Score);

            public Player Move(int dieRollSum)
            {
                var nextPosition = (Position - 1 + dieRollSum) % PositionCount + 1;
                var nextScore = Score + nextPosition;
                return new Player(nextPosition, nextScore);
            }
        }

        private static class Util
        {
            public static (Player player1, Player player2) Parse(IEnumerable<string> lines)
            {
                var players = lines
                    .Select(ParsePosition)
                    .Select(pos => new Player(pos, score: 0))
                    .Take(2)
                    .ToArray();

                return (players[0], players[1]);
            }

            public static int ParsePosition(string text)
            {
                const string prefix = "Player X starting position: ";
                return int.Parse(text.Substring(prefix.Length));
            }
        }
    }
}
