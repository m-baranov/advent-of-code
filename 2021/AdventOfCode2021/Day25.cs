using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day25
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "v...>>.vv>",
                    ".vv>>.vv..",
                    ">>.>v>...v",
                    ">>v>>.>.v.",
                    "v>v.vv.v..",
                    ">.>>..v...",
                    ".vv..>.>v.",
                    "v.v..>>v.v",
                    "....v..v.>"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/25/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = Grid.Parse(input.Lines());

                var steps = 0;
                var moved = true;
                while (moved)
                {
                    moved = grid.Next(out grid);
                    steps++;
                }

                grid.Draw();

                Console.WriteLine(steps);
            }
        }

        // There is no Part 2 on this day.

        //public class Part2 : IProblem
        //{
        //    public void Run(TextReader input)
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        private class Grid
        {
            public static Grid Parse(IEnumerable<string> lines)
            {
                var cells = lines.Select(l => l.ToList()).ToList();
                return new Grid(cells);
            }

            private readonly IReadOnlyList<IReadOnlyList<char>> cells;

            public Grid(IReadOnlyList<IReadOnlyList<char>> cells)
            {
                this.cells = cells;
            }

            public int Rows => cells.Count;
            public int Cols => cells[0].Count;

            public void Draw()
            {
                foreach (var row in cells)
                {
                    Console.WriteLine(string.Join("", row));
                }
                Console.WriteLine();
            }

            public bool Next(out Grid nextGrid)
            {
                var movedRight = this.Move('>', RightOf, out nextGrid);
                var movedDown = nextGrid.Move('v', DownOf, out nextGrid);
                return movedRight || movedDown;
            }

            private bool Move(char ch, Func<(int r, int c), (int r, int c)> nextPos, out Grid nextGrid)
            {
                var nextCells = cells.Select(r => r.ToList()).ToList();

                var moved = false;

                for (var r = 0; r < Rows; r++)
                {
                    for (var c = 0; c < Cols; c++)
                    {
                        var next = nextPos((r, c));

                        if (cells[r][c] == ch && cells[next.r][next.c] == '.')
                        {
                            nextCells[r][c] = '.';
                            nextCells[next.r][next.c] = ch;
                            moved = true;
                        }
                    }
                }

                nextGrid = new Grid(nextCells);
                return moved;
            }

            private (int r, int c) RightOf((int r, int c) pos) => 
                pos.c == Cols - 1 ? (pos.r, 0) : (pos.r, pos.c + 1);

            private (int r, int c) DownOf((int r, int c) pos) =>
                pos.r == Rows - 1 ? (0, pos.c) : (pos.r + 1, pos.c);
        }
    }
}
