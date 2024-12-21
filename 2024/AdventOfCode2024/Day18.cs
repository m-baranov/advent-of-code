using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;

namespace AdventOfCode2024;

static class Day18
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
5,4
4,2
4,5
3,0
2,1
6,3
2,4
1,5
0,6
3,3
2,6
5,1
1,2
5,5
2,5
6,5
1,4
0,4
6,4
1,1
6,1
1,0
0,5
1,6
2,0
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/18/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var GRID_SIZE = new Position(71, 71);
            var COUNT = 1024;

            var positions = input.Lines()
                .Select(Position.Parse)
                .ToList();

            var grid = new Grid(GRID_SIZE.Row, GRID_SIZE.Col);
            foreach (var position in positions.Take(COUNT))
            {
                grid.Set(position, '#');
            }

            var start = new Position(0, 0);
            var end = new Position(GRID_SIZE.Row - 1, GRID_SIZE.Col - 1);
            var cost = Dijkstra.MinCost(grid, start, end);

            Console.WriteLine(cost);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var GRID_SIZE = new Position(71, 71);

            var positions = input.Lines()
                .Select(Position.Parse)
                .ToList();

            Position? cutOffPosition = null;

            var grid = new Grid(GRID_SIZE.Row, GRID_SIZE.Col);
            foreach (var position in positions)
            {
                grid.Set(position, '#');

                var start = new Position(0, 0);
                var end = new Position(GRID_SIZE.Row - 1, GRID_SIZE.Col - 1);
                var isReachable = IsReachable(grid, start, end);
                if (!isReachable)
                {
                    cutOffPosition = position;
                    break;
                }
            }

            if (cutOffPosition is null)
            {
                Console.WriteLine("(none)");
            }
            else
            {
                Console.WriteLine($"{cutOffPosition.Col},{cutOffPosition.Row}");
            }
        }

        private static bool IsReachable(Grid grid, Position start, Position end)
        {
            var visit = new Queue<Position>();
            visit.Enqueue(start);

            var seen = new HashSet<Position>();
            seen.Add(start);

            while (visit.Count > 0)
            {
                var pos = visit.Dequeue();
                if (pos.Equals(end))
                {
                    return true;
                }

                foreach (var nextPos in Dijkstra.NextPossibleTransitions(grid, pos))
                {
                    if (seen.Contains(nextPos))
                    {
                        continue;
                    }

                    visit.Enqueue(nextPos);
                    seen.Add(nextPos);
                }
            }

            return false;
        }
    }

    private static class Dijkstra
    {
        public static int? MinCost(
            Grid grid,
            Position start,
            Position end)
        {
            var states = RunDijkstra(grid, start, end);

            if (states.TryGetValue(end, out var state))
            {
                return state.Cost;
            }
            return null;
        }

        private static IReadOnlyDictionary<Position, State> RunDijkstra(
            Grid grid,
            Position start,
            Position end)
        {
            var visit = new PriorityQueue<Position, int>();
            visit.Enqueue(start, 0);

            var states = new Dictionary<Position, State>();
            states.Add(start, new(0, start));

            while (visit.Count > 0)
            {
                var position = visit.Dequeue();

                if (position.Equals(end))
                {
                    continue;
                }

                var state = states[position];

                foreach (var nextPosition in NextPossibleTransitions(grid, position))
                {
                    var nextCost = state.Cost + 1;

                    if (states.TryGetValue(nextPosition, out var existingState) &&
                        existingState.Cost <= nextCost)
                    {
                        continue;
                    }

                    states[nextPosition] = new State(nextCost, position);
                    visit.Enqueue(nextPosition, nextCost);
                }
            }

            return states;
        }

        private record State(int Cost, Position Previous);

        public static IEnumerable<Position> NextPossibleTransitions(Grid grid, Position position)
        {
            return DirectionUtil.All
                .Select(dir => position.Shift(dir))
                .Where(pos => grid.IsFreeAt(pos));
        }
    }

    private record Position(int Row, int Col)
    {
        public static Position Parse(string text)
        {
            var parts = text.Split(',');

            var col = int.Parse(parts[0]);
            var row = int.Parse(parts[1]);

            return new Position(row, col);
        }

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
        private readonly List<List<char>> cells;

        public Grid(int rows, int cols)
        {
            var cells = new List<List<char>>();

            for (var row = 0; row < rows; row++)
            {
                var cs = new List<char>();
                for (var col = 0; col < cols; col++)
                {
                    cs.Add('.');
                }
                cells.Add(cs);
            }

            this.cells = cells;
        }

        public int Rows => this.cells.Count;
        public int Cols => this.cells[0].Count;

        public bool Contains(Position p) =>
            0 <= p.Row && p.Row < Rows &&
            0 <= p.Col && p.Col < Cols;

        public char At(Position p) =>
            Contains(p) ? this.cells[p.Row][p.Col] : '#';

        public void Set(Position p, char ch) =>
            this.cells[p.Row][p.Col] = '#';

        public bool IsFreeAt(Position p) =>
            At(p) == '.';
    }
}
