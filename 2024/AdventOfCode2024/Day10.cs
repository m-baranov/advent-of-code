using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2024;

static class Day10
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
89010123
78121874
87430965
96549874
45678903
32019012
01329801
10456732
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/10/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var sum = grid.AllPositions()
                .Where(pos => grid.At(pos) == 0)
                .Select(pos => Score(grid, pos))
                .Sum();

            Console.WriteLine(sum);
        }

        private static int Score(Grid grid, Position start)
        {
            var visit = new Queue<Position>();
            visit.Enqueue(start);

            var seen = new HashSet<Position>();
            seen.Add(start);

            var score = 0;

            while (visit.Count > 0)
            {
                var pos = visit.Dequeue();
                var height = grid.At(pos);

                var neigbours = DirectionUtil.All
                    .Select(DirectionUtil.ToPositionOffset)
                    .Select(offset => pos.Add(offset));

                foreach (var neighbour in neigbours)
                {
                    if (!grid.Contains(neighbour))
                    {
                        continue;
                    }
                    if (seen.Contains(neighbour))
                    {
                        continue;
                    }

                    var neighbourHeight = grid.At(neighbour);
                    if (neighbourHeight - height != 1)
                    {
                        continue;
                    }

                    seen.Add(neighbour);

                    if (neighbourHeight == 9)
                    {
                        score++;
                    }
                    else
                    {
                        visit.Enqueue(neighbour);
                    }
                }
            }

            return score;
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var sum = grid.AllPositions()
                .Where(pos => grid.At(pos) == 0)
                .Select(pos => Rate(grid, pos))
                .Sum();

            Console.WriteLine(sum);
        }

        private static int Rate(Grid grid, Position start)
        {
            var visit = new Queue<Position>();
            visit.Enqueue(start);

            var score = 0;

            while (visit.Count > 0)
            {
                var pos = visit.Dequeue();
                var height = grid.At(pos);

                var neigbours = DirectionUtil.All
                    .Select(DirectionUtil.ToPositionOffset)
                    .Select(offset => pos.Add(offset));

                foreach (var neighbour in neigbours)
                {
                    if (!grid.Contains(neighbour))
                    {
                        continue;
                    }

                    var neighbourHeight = grid.At(neighbour);
                    if (neighbourHeight - height != 1)
                    {
                        continue;
                    }

                    if (neighbourHeight == 9)
                    {
                        score++;
                    }
                    else
                    {
                        visit.Enqueue(neighbour);
                    }
                }
            }

            return score;
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
        public static readonly IReadOnlyList<Direction> All =
            new[]
            {
                Direction.Up,
                Direction.Left,
                Direction.Down,
                Direction.Right
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
        public static Grid Parse(IEnumerable<string> lines)
        {
            var cells = lines
                .Select(line => line
                    .Select(ch => ch - '0')
                    .ToArray()
                )
                .ToArray();

            return new Grid(cells);
        }

        private readonly IReadOnlyList<IReadOnlyList<int>> cells;

        public Grid(IReadOnlyList<IReadOnlyList<int>> cells)
        {
            this.cells = cells;
        }

        public int Rows => this.cells.Count;
        public int Cols => this.cells[0].Count;

        public bool Contains(Position p) =>
            0 <= p.Row && p.Row < Rows &&
            0 <= p.Col && p.Col < Cols;

        public int At(Position p) =>
            this.cells[p.Row][p.Col];

        public IEnumerable<Position> AllPositions()
        {
            for (var row = 0; row < this.Rows; row++)
            {
                for (var col = 0; col < this.Cols; col++)
                {
                    yield return new Position(row, col);
                }
            }
        }
    }
}
