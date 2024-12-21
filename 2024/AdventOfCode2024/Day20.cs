using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace AdventOfCode2024;

static class Day20
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
###############
#...#...#.....#
#.#.#.#.#.###.#
#S#...#.#.#...#
#######.#.#.###
#######.#.#...#
#######.#.###.#
###..E#...#...#
###.#######.###
#...###...#...#
#.#####.#.###.#
#.#...#.#.#...#
#.#.#.#.#.#.###
#...#...#...###
###############
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/20/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var (grid, start, end) = Grid.Parse(input.Lines());

            var path = FindPath(grid, start, end);

            var cheats = PossibleCheats(grid, path, allowedTime: 2);

            var count = cheats
                .Where(c => c.TimeSaved >= 100)
                .Count();

            Console.WriteLine(count);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var (grid, start, end) = Grid.Parse(input.Lines());

            var path = FindPath(grid, start, end);

            var cheats = PossibleCheats(grid, path, allowedTime: 20);

            var count = cheats
                .Where(c => c.TimeSaved >= 100)
                .Count();

            Console.WriteLine(count);

        }
    }

    private static IReadOnlyList<Position> FindPath(Grid grid, Position start, Position end)
    {
        var path = new List<Position>();
        path.Add(start);

        var previous = start;
        var current = start;
        while (!current.Equals(end))
        {
            var next = DirectionUtil.All
                .Select(dir => current.Shift(dir))
                .Where(pos => grid.IsFreeAt(pos) && !pos.Equals(previous))
                .First();

            previous = current;
            current = next;
            path.Add(current);
        }

        return path;
    }

    private static IReadOnlyList<Cheat> PossibleCheats(Grid grid, IReadOnlyList<Position> path, int allowedTime)
    {
        var pathIndex = path
            .Select((pos, index) => (pos, index))
            .ToDictionary(p => p.pos, p => p.index);

        int TimeSaved(Position start, Position end)
        {
            var startIndex = pathIndex[start];
            var endIndex = pathIndex[end];
            return endIndex - startIndex;
        }

        var cheats = new List<Cheat>();

        foreach (var current in path)
        {
            var exits = FreePositionsAround(grid, current, allowedTime);

            foreach (var exit in exits)
            {
                var timeSaved = TimeSaved(current, exit.pos) - exit.dist;
                if (timeSaved <= 0)
                {
                    continue;
                }

                cheats.Add(new Cheat(current, exit.pos, timeSaved));
            }
        }

        return cheats;
    }

    private record Cheat(Position Current, Position Exit, int TimeSaved);

    private static IEnumerable<(Position pos, int dist)> FreePositionsAround(Grid grid, Position pos, int distance)
    {
        static int ManhattanDistance(Position x, Position y) =>
            Math.Abs(x.Row - y.Row) + Math.Abs(x.Col - y.Col);

        bool Acceptable(Position p) =>
            grid.IsFreeAt(p);

        var minRow = pos.Row - distance;
        var maxRow = pos.Row + distance;

        var minCol = pos.Col;
        var maxCol = pos.Col;

        var row = minRow;
        while (row <= pos.Row)
        {
            for (var col = minCol; col <= maxCol; col++)
            {
                var p = new Position(row, col);
                if (Acceptable(p))
                {
                    var d = ManhattanDistance(p, pos);
                    yield return (p, d);
                }
            }

            minCol--;
            maxCol++;
            row++;
        }

        minCol += 2;
        maxCol -= 2;

        while (row <= maxRow)
        {
            for (var col = minCol; col <= maxCol; col++)
            {
                var p = new Position(row, col);
                if (Acceptable(p))
                {
                    var d = ManhattanDistance(p, pos);
                    yield return (p, d);
                }
            }

            minCol++;
            maxCol--;
            row++;
        }
    }

    private record Position(int Row, int Col)
    {
        public Position Add(Position p) =>
            new(p.Row + this.Row, p.Col + this.Col);

        public Position Shift(Direction dir) =>
            this.Add(DirectionUtil.ToPositionOffset(dir));
    }

    private enum Direction { Up, Down, Left, Right }

    private static class DirectionUtil
    {
        public static readonly IReadOnlyList<Direction> All =
            new[]
            {
                Direction.Up,
                Direction.Right,
                Direction.Down,
                Direction.Left,
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

    private class Grid
    {
        public static (Grid, Position start, Position end) Parse(IEnumerable<string> lines)
        {
            var grid = new List<IReadOnlyList<char>>();
            var start = new Position(Row: 0, Col: 0);
            var end = new Position(Row: 0, Col: 0);

            var row = 0;
            foreach (var line in lines)
            {
                var cells = new List<char>();

                var col = 0;
                foreach (var ch in line)
                {
                    if (ch == 'S')
                    {
                        start = new Position(row, col);
                        cells.Add('.');
                    }
                    else if (ch == 'E')
                    {
                        end = new Position(row, col);
                        cells.Add('.');
                    }
                    else
                    {
                        cells.Add(ch);
                    }

                    col++;
                }

                grid.Add(cells);
                row++;
            }

            return (new Grid(grid), start, end);
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

        public bool IsFreeAt(Position p) =>
            Contains(p) && At(p) == '.';
    }
}
