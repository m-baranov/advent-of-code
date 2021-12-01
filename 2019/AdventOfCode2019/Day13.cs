using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day13
    {
        public static readonly IInput SampleInput =
            Input.Literal();

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/13/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();
                var computer = Day09.Computer.Of(program);
                computer.Execute();

                var board = new Board();
                foreach (var chunk in computer.Output.Values().Chunk(3))
                {
                    var pos = new Position((int)chunk[1], (int)chunk[0]);
                    board.Set(pos, (int)chunk[2]);
                }

                board.Draw();

                Console.WriteLine(board.Count(Tile.Block));
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();
                var computer = Day09.Computer.Of(program);
                computer.Mem.Write(address: 0, value: 2L);

                long score = 0;
                Board board;
                while (true)
                { 
                    var result = computer.Execute();

                    board = new Board();
                    foreach (var chunk in computer.Output.Values().Chunk(3))
                    {
                        if (chunk[0] == -1 && chunk[1] == 0)
                        {
                            score = chunk[2];
                        }
                        else
                        {
                            var pos = new Position((int)chunk[1], (int)chunk[0]);
                            board.Set(pos, (int)chunk[2]);
                        }
                    }

                    if (result is Day09.Computer.Result.WaitingForInput)
                    {
                        var paddlePos = board.PositionOf(Tile.Paddle);
                        var ballPos = board.PositionOf(Tile.Ball);

                        computer.Input.Enter(Math.Sign(ballPos.Col - paddlePos.Col));
                    }
                    else
                    {
                        break;
                    }
                }

                board.Draw();
                Console.WriteLine($"SCORE: {score}");
                Console.WriteLine();
            }
        }

        private enum Tile { Empty, Wall, Block, Paddle, Ball }

        private class Position
        {
            public static readonly Position Origin = new Position(0, 0);

            public Position(int row, int col)
            {
                Row = row;
                Col = col;
            }

            public int Row { get; }
            public int Col { get; }

            public override bool Equals(object obj) =>
                obj is Position pos ? Row == pos.Row && Col == pos.Col : false;

            public override int GetHashCode() =>
                HashCode.Combine(Row, Col);
        }

        private class Board
        {
            private Dictionary<Position, Tile> cells;

            public Board()
            {
                cells = new Dictionary<Position, Tile>();
            }

            public int Count(Tile tile) => cells.Values.Where(t => t == tile).Count();

            public Position PositionOf(Tile tile)
            {
                var pairs = cells.Where(t => t.Value == tile).Take(1).ToArray();
                return pairs.Any() ? pairs.First().Key : null;
            }

            public Tile Get(Position pos)
            {
                if (cells.TryGetValue(pos, out var tile))
                {
                    return tile;
                }
                else
                {
                    return Tile.Empty;
                }
            }

            public void Set(Position pos, int tile)
            {
                Set(pos, IntToTile(tile));
            }

            private Tile IntToTile(int value)
            {
                if (value == 0) return Tile.Empty;
                if (value == 1) return Tile.Wall;
                if (value == 2) return Tile.Block;
                if (value == 3) return Tile.Paddle;
                return Tile.Ball;
            }

            public void Set(Position pos, Tile tile)
            {
                cells[pos] = tile;
            }

            public void Draw()
            {
                var minRow = cells.Keys.Select(pos => pos.Row).Min();
                var maxRow = cells.Keys.Select(pos => pos.Row).Max();

                var minCol = cells.Keys.Select(pos => pos.Col).Min();
                var maxCol = cells.Keys.Select(pos => pos.Col).Max();

                var lines = new List<IReadOnlyList<Tile>>();

                for (var row = minRow; row <= maxRow; row++)
                {
                    for (var col = minCol; col <= maxCol; col++)
                    {
                        var tile = Get(new Position(row, col));
                        Console.Write(TileToString(tile));
                    }
                    Console.WriteLine();
                }

                Console.WriteLine();
            }

            private string TileToString(Tile tile)
            {
                if (tile == Tile.Empty) return "  ";
                if (tile == Tile.Wall) return "██";
                if (tile == Tile.Block) return "[]";
                if (tile == Tile.Paddle) return "==";
                return "<>";
            }
        }
    }
}
