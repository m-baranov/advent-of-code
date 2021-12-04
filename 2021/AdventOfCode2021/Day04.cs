using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day04
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "7, 4, 9, 5, 11, 17, 23, 2, 0, 14, 21, 24, 10, 16, 13, 6, 15, 25, 12, 22, 18, 20, 8, 19, 3, 26, 1",
                    "",
                    "22 13 17 11  0",
                    " 8  2 23  4 24",
                    "21  9 14 16  7",
                    " 6 10  3 18  5",
                    " 1 12 20 15 19",
                    "",
                    " 3 15  0  2 22",
                    " 9 18 13 17  5",
                    "19  8  7 25 23",
                    "20 11 10 24  4",
                    "14 21 16 12  6",
                    "",
                    "14 21 17 24  4",
                    "10 16 15  9 19",
                    "18  8 23 26 20",
                    "22 11 13  6  5",
                    " 2  0 12  3  7"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/4/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var setup = Setup.Parse(input.Lines());

                var drawn = new HashSet<int>();
                var score = 0L;
                foreach (var number in setup.RandomNumbers)
                {
                    drawn.Add(number);

                    var board = setup.Boards.Where(b => b.Wins(drawn)).FirstOrDefault();
                    if (board != null)
                    {
                        score = board.Unmarked(drawn).Sum() * number;
                        break;
                    }
                }

                Console.WriteLine(score);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var setup = Setup.Parse(input.Lines());

                var remainingBoards = setup.Boards.ToList();

                var drawn = new HashSet<int>();
                var score = 0L;
                foreach (var number in setup.RandomNumbers)
                {
                    drawn.Add(number);

                    var winnerBoards = remainingBoards.Where(b => b.Wins(drawn)).ToList();
                    foreach (var winnerBoard in winnerBoards)
                    {
                        remainingBoards.Remove(winnerBoard);
                        score = winnerBoard.Unmarked(drawn).Sum() * number;
                    }
                }

                Console.WriteLine(score);
            }
        }

        public class Setup
        {
            public static Setup Parse(IEnumerable<string> lines)
            {
                var groups = lines.SplitByEmptyLine().ToList();

                var randomNumbers = groups.First()[0].Split(',').Select(int.Parse).ToList();
                var boards = groups.Skip(1).Select(Board.Parse).ToList();

                return new Setup(randomNumbers, boards);
            }

            public Setup(IReadOnlyList<int> drawn, IReadOnlyList<Board> boards)
            {
                RandomNumbers = drawn;
                Boards = boards;
            }

            public IReadOnlyList<int> RandomNumbers { get; }
            public IReadOnlyList<Board> Boards { get; }
        }

        public class Board
        {
            public static Board Parse(IEnumerable<string> lines)
            {
                var cells = lines
                    .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList())
                    .ToList();

                return new Board(cells);
            }

            private readonly IReadOnlyList<IReadOnlyList<int>> cells;

            public Board(IReadOnlyList<IReadOnlyList<int>> cells)
            {
                this.cells = cells;
            }

            public int Size => this.cells.Count;

            public bool Wins(ISet<int> drawn)
            {
                static bool ContainsAll(ISet<int> set, IEnumerable<int> items) => items.All(set.Contains);

                return Rows().Concat(Cols()).Any(line => ContainsAll(drawn, line));
            }

            public IEnumerable<int> Unmarked(ISet<int> drawn) =>
                Rows().SelectMany(line => line).Where(n => !drawn.Contains(n));

            private IEnumerable<IEnumerable<int>> Rows() =>
                Enumerable.Range(0, Size).Select(Row);

            private IEnumerable<IEnumerable<int>> Cols() =>
                Enumerable.Range(0, Size).Select(Col);

            private IEnumerable<int> Row(int row) => this.cells[row];

            private IEnumerable<int> Col(int col)
            {
                for (var row = 0; row < Size; row++)
                {
                    yield return this.cells[row][col];
                }
            }
        }
    }
}
