using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2024;

static class Day06
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
....#.....
.........#
..........
..#.......
.......#..
..........
.#..^.....
........#.
#.........
......#...
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/6/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var (grid, guard) = Grid.Parse(input.Lines());

            var visited = new HashSet<Position>()
            {
                guard.Position
            };

            while (true)
            {
                var ahead = guard.PositionAhead();

                if (!grid.Contains(ahead))
                {
                    break;
                }

                if (grid.At(ahead) == '#')
                {
                    guard = guard.RotateRight();
                }
                else
                {
                    guard = guard.MoveForward();

                    visited.Add(guard.Position);
                }
            }

            Console.WriteLine(visited.Count);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var (grid, guard) = Grid.Parse(input.Lines());

            var _ = IsOnLoopedPath(grid, guard, out var visitedPositions);
            var candidatePositions = visitedPositions.Except([guard.Position]);

            var count = 0;

            foreach (var pos in candidatePositions)
            {
                var obstructedGrid = new ObstructedGrid(grid, pos);
                if (IsOnLoopedPath(obstructedGrid, guard, out var _))
                {
                    count++;
                }
            }

            Console.WriteLine(count);
        }

        private static bool IsOnLoopedPath(IGrid grid, Guard guard, out IReadOnlyList<Position> positions)
        {
            var visited = new HashSet<Guard>()
            {
                guard
            };

            while (true)
            {
                var ahead = guard.PositionAhead();

                if (!grid.Contains(ahead))
                {
                    break;
                }

                if (grid.At(ahead) == '#')
                {
                    guard = guard.RotateRight();
                }
                else
                {
                    guard = guard.MoveForward();
                }

                if (visited.Contains(guard))
                {
                    positions = Array.Empty<Position>();
                    return true;
                }

                visited.Add(guard);
            }

            positions = visited.Select(v => v.Position).Distinct().ToArray();
            return false;
        }
    }

    private record Position(int Row, int Col)
    {
        public Position Add(Position p) =>
            new(p.Row + this.Row, p.Col + this.Col);
    }

    private enum Direction { Up, Down, Left, Right }

    private static class DirectionUtil
    {
        public static Direction? TryParse(char ch) =>
            ch switch
            {
                '^' => Direction.Up,
                'v' => Direction.Down,
                '<' => Direction.Left,
                '>' => Direction.Right,

                _ => null
            };

        public static Direction TurnRight(Direction dir) =>
            dir switch
            {
                Direction.Up => Direction.Right,
                Direction.Right => Direction.Down,
                Direction.Down => Direction.Left,
                Direction.Left => Direction.Up,

                _ => throw new Exception("impossible")
            };

        public static Position ToPositionOffset(Direction dir) =>
            dir switch
            {
                Direction.Up => new Position(Row: -1, Col: 0),
                Direction.Right => new Position(Row: 0, Col: 1),
                Direction.Down => new Position(Row: 1, Col: 0),
                Direction.Left => new Position(Row: 0, Col: -1),

                _ => throw new Exception("impossible")
            };
    }

    private record Guard(Direction Direction, Position Position)
    {
        public Position PositionAhead() =>
            this.Position.Add(DirectionUtil.ToPositionOffset(this.Direction));

        public Guard RotateRight() =>
            this with
            {
                Direction = DirectionUtil.TurnRight(this.Direction)
            };

        public Guard MoveForward() =>
            this with
            {
                Position = this.PositionAhead()
            };
    }

    private interface IGrid
    {
        bool Contains(Position p);
        char At(Position p);
    }

    private class Grid : IGrid
    {
        public static (Grid, Guard) Parse(IEnumerable<string> lines)
        {
            var grid = new List<IReadOnlyList<char>>();
            var guard = new Guard(Direction.Up, new Position(Row: 0, Col: 0));

            var row = 0;
            foreach (var line in lines)
            {
                var cells = new List<char>();

                var col = 0;
                foreach (var ch in line)
                {
                    if (ch == '#' || ch == '.')
                    {
                        cells.Add(ch);
                    }
                    else
                    {
                        var dir = DirectionUtil.TryParse(ch);
                        if (dir is not null)
                        {
                            guard = new Guard(dir.Value, new Position(row, col));
                        }

                        cells.Add('.');
                    }

                    col++;
                }

                grid.Add(cells);
                row++;
            }

            return (new Grid(grid), guard);
        }

        private readonly IReadOnlyList<IReadOnlyList<char>> cells;

        public Grid(IReadOnlyList<IReadOnlyList<char>> cells)
        {
            this.cells = cells;
        }

        public int Rows => this.cells.Count;
        public int Cols => this.cells[0].Count;

        public bool Contains(Position p) =>
            0 <= p.Row && p.Row < Rows &&
            0 <= p.Col && p.Col < Cols;

        public char At(Position p) =>
            this.cells[p.Row][p.Col];
    }

    private class ObstructedGrid : IGrid
    {
        private readonly Grid grid;
        private readonly Position obstruction;

        public ObstructedGrid(Grid grid, Position obstruction)
        {
            this.grid = grid;
            this.obstruction = obstruction;
        }

        public bool Contains(Position p) =>
            this.grid.Contains(p);

        public char At(Position p) =>
            p.Equals(this.obstruction) ? '#' : this.grid.At(p);
    }
}
