using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day22
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "..#",
                    "#..",
                    "..."
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/22/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                static Cell NextCell(Cell cell) => 
                    cell == Cell.Infected ? Cell.Clean : Cell.Infected;

                static Direction NextDirection(Cell cell, Direction direction) => 
                    cell == Cell.Infected ? direction.RotateCW() : direction.RotateCCW();

                var (grid, start) = Grid.Parse(input.Lines().ToList());

                var infections = Simulate(grid, start, 10_000, NextCell, NextDirection);

                Console.WriteLine(infections);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                static Cell NextCell(Cell cell) =>
                    cell switch
                    {
                        Cell.Clean => Cell.Weakened,
                        Cell.Weakened => Cell.Infected,
                        Cell.Infected => Cell.Flagged,
                        Cell.Flagged or _ => Cell.Clean,
                    };

                static Direction NextDirection(Cell cell, Direction direction) =>
                    cell switch
                    {
                        Cell.Clean => direction.RotateCCW(),
                        Cell.Weakened => direction,
                        Cell.Infected => direction.RotateCW(),
                        Cell.Flagged or _ => direction.Reverse(),
                    };

                var (grid, start) = Grid.Parse(input.Lines().ToList());

                var infections = Simulate(grid, start, 10_000_000, NextCell, NextDirection);

                Console.WriteLine(infections);
            }
        }

        private static int Simulate(
            Grid grid, 
            Position start, 
            int rounds, 
            Func<Cell, Cell> nextCellFn, 
            Func<Cell, Direction, Direction> nextDirectionFn)
        {
            var virus = new Virus(start, Direction.Up);
            var infections = 0;

            for (var i = 0; i < rounds; i++)
            {
                var cell = grid.At(virus.Position);

                var nextCell = nextCellFn(cell);
                grid.Set(virus.Position, nextCell);

                var newDirection = nextDirectionFn(cell, virus.Direction);
                var newPosition = virus.Position.Move(newDirection);
                virus = new Virus(newPosition, newDirection);

                if (nextCell == Cell.Infected)
                {
                    infections++;
                }

                if (grid.IsAtEdge(virus.Position))
                {
                    grid = grid.Extend();
                }
            }

            return infections;
        }

        private record Virus(Position Position, Direction Direction);

        private record class Direction(int DeltaRow, int DeltaCol)
        {
            public static readonly Direction Up = new(-1, 0);
            public static readonly Direction Down = new(1, 0);
            public static readonly Direction Left = new(0, -1);
            public static readonly Direction Right = new(0, 1);

            public Direction RotateCW()
            {
                if (this == Direction.Up) return Direction.Right;
                if (this == Direction.Right) return Direction.Down;
                if (this == Direction.Down) return Direction.Left;
                /* if (this == Direction.Left) */ return Direction.Up;
            }

            public Direction RotateCCW()
            {
                if (this == Direction.Up) return Direction.Left;
                if (this == Direction.Left) return Direction.Down;
                if (this == Direction.Down) return Direction.Right;
                /* if (this == Direction.Right) */ return Direction.Up;
            }

            public Direction Reverse()
            {
                if (this == Direction.Up) return Direction.Down;
                if (this == Direction.Left) return Direction.Right;
                if (this == Direction.Down) return Direction.Up;
                /* if (this == Direction.Right) */ return Direction.Left;
            }

            public override string ToString()
            {
                if (this == Direction.Up) return "^";
                if (this == Direction.Right) return ">";
                if (this == Direction.Down) return "v";
                /* if (this == Direction.Left) */ return "<";
            }
        }

        private record Position(int Row, int Col)
        {
            public Position Move(Direction direction) =>
                new(Row + direction.DeltaRow, Col + direction.DeltaCol);
        }

        private enum Cell { Clean, Weakened, Infected, Flagged }

        private sealed class Grid
        {
            public static (Grid, Position) Parse(IReadOnlyList<string> lines)
            {
                var rows = lines.Count;
                var cols = lines[0].Length;

                var grid = CreatedExtendedGrid(rows, cols);

                for (var r = 0; r < rows; r++)
                {
                    var line = lines[r];
                    for (var c = 0; c < cols; c++)
                    {
                        var ch = line[c];
                        grid.Set(new Position(r, c), ch == '#' ? Cell.Infected : Cell.Clean);
                    }
                }

                var start = new Position(rows / 2, cols / 2);

                return (grid, start);
            }

            private static Grid CreatedExtendedGrid(int rows, int cols)
            {
                var minRow = -rows;
                var maxRow = 2 * rows - 1;

                var minCol = -cols;
                var maxCol = 2 * cols - 1;

                return new Grid(minRow, minCol, maxRow, maxCol);
            }

            private readonly Cell[,] cells;
            private readonly int minRow;
            private readonly int maxRow;
            private readonly int minCol;
            private readonly int maxCol;
            private readonly int rows;
            private readonly int cols;

            public Grid(int minRow, int minCol, int maxRow, int maxCol)
            {
                this.minRow = minRow;
                this.maxRow = maxRow;
                this.rows = this.maxRow - this.minRow + 1;

                this.minCol = minCol;
                this.maxCol = maxCol;
                this.cols = this.maxCol - this.minCol + 1;

                this.cells = new Cell[this.rows, this.cols];
            }

            public bool IsAtEdge(Position position) =>
                this.minRow == position.Row || (this.minRow + 1) == position.Row ||
                this.maxRow == position.Row || (this.maxRow - 1) == position.Row ||
                this.minCol == position.Col || (this.minCol + 1) == position.Col ||
                this.maxCol == position.Col || (this.maxCol - 1) == position.Col;

            public Cell At(Position p)
            {
                var row = p.Row - this.minRow;
                var col = p.Col - this.minCol;
                return this.cells[row, col];
            }

            public void Set(Position p, Cell value)
            {
                var row = p.Row - this.minRow;
                var col = p.Col - this.minCol;
                this.cells[row, col] = value;
            }

            public Grid Extend()
            {
                var newGrid = CreatedExtendedGrid(this.rows, this.cols);

                for (var r = this.minRow; r <= this.maxRow; r++)
                {
                    for (var c = this.minCol; c <= this.maxCol; c++)
                    {
                        var p = new Position(r, c);
                        newGrid.Set(p, this.At(p));
                    }
                }

                return newGrid;
            }

            public void Draw()
            {
                for (var r = this.minRow; r <= this.maxRow; r++)
                {
                    for (var c = this.minCol; c <= this.maxCol; c++)
                    {
                        var p = new Position(r, c);
                        var ch = this.At(p);
                        Console.Write(ch == Cell.Clean ? '.' : '#');
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
        }
    }
}
