using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day10
{
    public static class Inputs
    {
        public static readonly IInput Sample1 =
            Input.Literal(""""""
.....
.S-7.
.|.|.
.L-J.
.....
"""""");

        public static readonly IInput Sample2 =
            Input.Literal(""""""
7-F7-
.FJ|7
SJLL7
|F--J
LJ.LJ
"""""");

        public static readonly IInput Sample3 =
            Input.Literal(""""""
...........
.S-------7.
.|F-----7|.
.||.....||.
.||.....||.
.|L-7.F-J|.
.|..|.|..|.
.L--J.L--J.
...........
"""""");

        public static readonly IInput Sample4 =
            Input.Literal(""""""
.F----7F7F7F7F-7....
.|F--7||||||||FJ....
.||.FJ||||||||L7....
FJL7L7LJLJ||LJ.L-7..
L--J.L7...LJS7F-7L7.
....F-J..F7FJ|L7L7L7
....L7.F7||L7|.L7L7|
.....|FJLJ|FJ|F7|.LJ
....FJL-7.||.||||...
....L---J.LJ.LJLJ...
"""""");

        public static readonly IInput Sample5 =
            Input.Literal(""""""
FF7FSF7F7F7F7F7F---7
L|LJ||||||||||||F--J
FL-7LJLJ||||||LJL-77
F--JF--7||LJLJ7F7FJ-
L---JF-JLJ.||-FJLJJ7
|F|F-JF---7F7-L7L|7|
|FFJF7L7F-JF7|JL---7
7-L-JL7||F7|L7F-7F7|
L.L7LFJ|||||FJL7||LJ
L7JLJL-JLJLJL--JLJ.L
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/10/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var loop = grid.FindLoop();

            Console.WriteLine(loop.Count / 2);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var loop = grid.FindLoop();

            var squares = new FloodFillGrid(grid.Rows, grid.Cols);

            foreach (var pos in loop)
            {
                squares.AddWalls(pos, grid.At(pos));
            }

            var inside = squares.FloodFill(grid.StartPos, grid.StartCell);

            Console.WriteLine(inside);
        }
    }

    private record struct Position(int Row, int Col)
    {
        public static readonly Position Up = new(Row: -1, Col: 0);
        public static readonly Position Down = new(Row: 1, Col: 0);
        public static readonly Position Left = new(Row: 0, Col: -1);
        public static readonly Position Right = new(Row: 0, Col: 1);

        public Position Add(Position pos) => new(Row + pos.Row, Col + pos.Col);
    }

    private enum Cell
    {
        Start,
        Ground,
        NS, // |
        EW, // -
        NE, // L
        NW, // J
        SW, // 7
        SE, // F
    }

    private static class CellUtil
    {
        public static Cell Parse(char ch) =>
           ch switch
           {
               'S' => Cell.Start,
               '|' => Cell.NS,
               '-' => Cell.EW,
               'L' => Cell.NE,
               'J' => Cell.NW,
               '7' => Cell.SW,
               'F' => Cell.SE,
               '.' or _ => Cell.Ground,
           };

        public static IReadOnlyList<Position> ConnectedDirections(Cell cell) =>
            cell switch
            {
                Cell.NS => new[] { Position.Up, Position.Down },
                Cell.EW => new[] { Position.Left, Position.Right },
                Cell.NE => new[] { Position.Up, Position.Right },
                Cell.NW => new[] { Position.Up, Position.Left },
                Cell.SW => new[] { Position.Down, Position.Left },
                Cell.SE => new[] { Position.Down, Position.Right },

                Cell.Start or Cell.Ground or _ => Array.Empty<Position>(),
            };
    }

    private sealed class Grid
    {
        public static Grid Parse(IEnumerable<string> lines)
        {
            var cells = lines
                .Select(text => text.Select(CellUtil.Parse).ToList())
                .ToList();

            return new Grid(cells);
        }

        private readonly IReadOnlyList<IReadOnlyList<Cell>> cells;
        private readonly Position startPos;
        private readonly Cell startCell;
        
        public Grid(IReadOnlyList<IReadOnlyList<Cell>> cells)
        {
            this.cells = cells;

            this.startPos = FindPosition(cell => cell == Cell.Start)!.Value;
            this.startCell = DetermineCellType(this.startPos);
        }

        public int Rows => this.cells.Count;
        public int Cols => this.cells[0].Count;

        public Position StartPos => this.startPos;
        public Cell StartCell => this.startCell;

        public bool InBounds(Position pos) =>
            0 <= pos.Row && pos.Row < Rows &&
            0 <= pos.Col && pos.Col < Cols;

        public Cell At(Position pos) =>
            pos.Equals(this.startPos) ? this.startCell : this.cells[pos.Row][pos.Col];

        private Position? FindPosition(Func<Cell, bool> predicate)
        {
            for (var r = 0; r < Rows; r++)
            {
                for (var c = 0; c < Cols; c++)
                {
                    if (predicate(this.cells[r][c]))
                    {
                        return new Position(r, c);
                    }
                }
            }

            return null;
        }

        private Cell DetermineCellType(Position pos)
        {
            static bool IsConnected(Grid grid, Position pos, Position neigb)
            {
                if (!grid.InBounds(neigb))
                {
                    return false;
                }


                return CellUtil.ConnectedDirections(grid.At(neigb))
                    .Select(neigb.Add)
                    .Contains(pos);
            }

            static bool AreSame(IReadOnlyList<Position> xs, IReadOnlyList<Position> ys) =>
                xs.Count == ys.Count && !xs.Except(ys).Any();

            var directions = new[] 
            { 
                Position.Up, 
                Position.Down, 
                Position.Left, 
                Position.Right 
            };

            var possibleCells = new[]
            {
                Cell.NS,
                Cell.EW,
                Cell.NE,
                Cell.NW,
                Cell.SW,
                Cell.SE,
            };

            var connectedDirections = directions
                .Where(dir => IsConnected(this, pos, pos.Add(dir)))
                .ToList();

            return possibleCells
                .First(cell => AreSame(CellUtil.ConnectedDirections(cell), connectedDirections));
        }

        public IReadOnlyList<Position> FindLoop()
        {
            var loop = new List<Position>();

            var startDirs = CellUtil.ConnectedDirections(this.startCell);
            loop.Add(this.startPos);

            var prevPos = startPos;
            var pos = startPos.Add(startDirs[0]);
            while (!pos.Equals(startPos))
            {
                loop.Add(pos);

                var dirs = CellUtil.ConnectedDirections(At(pos));
                var dir = dirs.First(dir => !pos.Add(dir).Equals(prevPos));

                prevPos = pos;
                pos = pos.Add(dir);
            }

            return loop;
        }
    }

    private sealed class FloodFillGrid
    {
        private readonly Square[,] squares;

        public FloodFillGrid(int gridRows, int gridCols)
        {
            this.squares = CreateSquares(gridRows, gridCols);
        }

        private int Rows => this.squares.GetLength(0);
        private int Cols => this.squares.GetLength(1);

        private Square[,] CreateSquares(int gridRows, int gridCols)
        {
            var rows = gridRows * 2;
            var cols = gridCols * 2;

            var squares = new Square[rows, cols];

            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    squares[r, c] = new Square();
                }
            }

            return squares;
        }

        public void AddWalls(Position gridPos, Cell gridCell)
        {
            var row = gridPos.Row * 2;
            var col = gridPos.Col * 2;

            if (gridCell == Cell.SE)
            {
                squares[row + 1, col + 1].WallLeft = true;
                squares[row + 1, col + 1].WallUp = true;
            }
            else if (gridCell == Cell.SW)
            {
                squares[row + 1, col + 1].WallLeft = true;
                squares[row + 1, col].WallUp = true;
            }
            else if (gridCell == Cell.NE)
            {
                squares[row, col + 1].WallLeft = true;
                squares[row + 1, col + 1].WallUp = true;
            }
            else if (gridCell == Cell.NW)
            {
                squares[row, col + 1].WallLeft = true;
                squares[row + 1, col].WallUp = true;
            }
            else if (gridCell == Cell.EW)
            {
                squares[row + 1, col].WallUp = true;
                squares[row + 1, col + 1].WallUp = true;
            }
            else if (gridCell == Cell.NS)
            {
                squares[row, col + 1].WallLeft = true;
                squares[row + 1, col + 1].WallLeft = true;
            }
        }

        public int FloodFill(Position startGridPos, Cell startCell)
        {
            static (Position one, Position two) AlternativePositions(Position gridPos, Cell cell)
            {
                var row = gridPos.Row * 2;
                var col = gridPos.Col * 2;
                
                return cell switch
                {
                    Cell.SE => (new Position(row + 1, col + 1), new Position(row, col)),
                    Cell.SW => (new Position(row + 1, col), new Position(row, col + 1)),
                    Cell.NE => (new Position(row, col + 1), new(row + 1, col)),
                    Cell.NW => (new Position(row, col), new Position(row + 1, col + 1)),
                    Cell.EW => (new Position(row, col), new Position(row + 1, col)),
                    Cell.NS => (new Position(row, col), new Position(row, col + 1)),

                    _ => throw new Exception("impossible"),
                };
            }

            var (start1, start2) = AlternativePositions(startGridPos, startCell);

            return TryFloodFill(start1) ?? TryFloodFill(start2) ?? throw new Exception("impossible");
        }

        private int? TryFloodFill(Position start)
        {
            var filled = new bool[Rows, Cols];

            var visit = new Queue<Position>();
            visit.Enqueue(start);

            while (visit.Count > 0)
            {
                var pos = visit.Dequeue();

                if (pos.Row == 0 || pos.Row == Rows - 1 || 
                    pos.Col == 0 || pos.Col == Cols - 1)
                {
                    return null;
                }

                if (filled[pos.Row, pos.Col])
                {
                    continue;
                }

                filled[pos.Row, pos.Col] = true;

                var nexts = new List<Position>();
                if (pos.Row > 0 && !squares[pos.Row, pos.Col].WallUp)
                {
                    nexts.Add(pos.Add(Position.Up));
                }
                if (pos.Row < Rows - 1 && !squares[pos.Row + 1, pos.Col].WallUp)
                {
                    nexts.Add(pos.Add(Position.Down));
                }
                if (pos.Col > 0 && !squares[pos.Row, pos.Col].WallLeft)
                {
                    nexts.Add(pos.Add(Position.Left));
                }
                if (pos.Col < Cols - 1 && !squares[pos.Row, pos.Col + 1].WallLeft)
                {
                    nexts.Add(pos.Add(Position.Right));
                }

                foreach (var next in nexts)
                {
                    if (filled[next.Row, next.Col])
                    {
                        continue;
                    }

                    visit.Enqueue(next);
                }
            }

            return CountFilled(filled);
        }

        private static int CountFilled(bool[,] filled)
        {
            var count = 0;

            var rows = filled.GetLength(0) / 2;
            var cols = filled.GetLength(1) / 2;

            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    if (filled[row * 2, col * 2] &&
                        filled[row * 2 + 1, col * 2] &&
                        filled[row * 2, col * 2 + 1] &&
                        filled[row * 2 + 1, col * 2 + 1])
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private class Square
        {
            public bool WallUp { get; set; }
            public bool WallLeft { get; set; }
        }
    }
}
