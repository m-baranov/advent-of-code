using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2024;

static class Day16
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
###############
#.......#....E#
#.#.###.#.###.#
#.....#.#...#.#
#.###.#####.#.#
#.#.#.......#.#
#.#.#####.###.#
#...........#.#
###.#.#####.#.#
#...#.....#.#.#
#.#.#.###.#.#.#
#.....#...#.#.#
#.###.#.#.#.#.#
#S..#.....#...#
###############
"""""");

        public static readonly IInput Sample2 =
            Input.Literal(""""""
#################
#...#...#...#..E#
#.#.#.#.#.#.#.#.#
#.#.#.#...#...#.#
#.#.#.#.###.#.#.#
#...#.#.#.....#.#
#.#.#.#.#.#####.#
#.#...#.#.#.....#
#.#.#####.#.###.#
#.#.#.......#...#
#.#.###.#####.###
#.#.#...#.....#.#
#.#.#.#####.###.#
#.#.#.........#.#
#.#.#.#########.#
#S#.............#
#################
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/16/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var (grid, start, end) = Grid.Parse(input.Lines());

            var reindeer = new Reindeer(start, Direction.Right);

            var (min, _) = Dijkstra.MinCostPath(grid, reindeer, end);

            Console.WriteLine(min);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var (grid, start, end) = Grid.Parse(input.Lines());

            var reindeer = new Reindeer(start, Direction.Right);

            var (min, path) = Dijkstra.MinCostPath(grid, reindeer, end);

            var count = Solve(grid, reindeer, end, min, path);

            Console.WriteLine(count);
        }

        private static int Solve(
            Grid grid,
            Reindeer start,
            Position end,
            long minCost,
            IReadOnlyList<Reindeer> minPath)
        {
            var paths = new List<IReadOnlyList<Reindeer>>();
            var triedBlocking = new HashSet<Position>();

            void Recurse(IReadOnlyList<Reindeer> path, HashSet<Position> blocked)
            {
                for (var i = 1; i < path.Count; i++)
                {
                    var prev = path[i - 1];
                    var curr = path[i];

                    if (!prev.Position.Equals(curr.Position))
                    {
                        continue;
                    }

                    var posToBlock = curr.Position.Shift(curr.Direction);
                    if (triedBlocking.Contains(posToBlock))
                    {
                        continue;
                    }
                    triedBlocking.Add(posToBlock);

                    blocked.Add(posToBlock);
                    var (min, minPath) = Dijkstra.MinCostPath(grid, start, end, blocked, minCost);

                    if (min == minCost)
                    {
                        paths.Add(minPath);
                        Recurse(minPath, blocked);
                    }

                    blocked.Remove(posToBlock);
                }
            }

            paths.Add(minPath);

            Recurse(minPath, new HashSet<Position>());

            return paths
                .SelectMany(p => p)
                .Select(p => p.Position)
                .Distinct()
                .Count();
        }
    }

    private static class Dijkstra
    {
        public static (long, IReadOnlyList<Reindeer>) MinCostPath(
            Grid grid,
            Reindeer initial,
            Position end,
            HashSet<Position>? blocked = null,
            long? knownMinCost = null)
        {
            var states = RunDijkstra(
                grid,
                initial,
                end,
                blocked ?? new HashSet<Position>(),
                knownMinCost);

            if (!states.Any(p => p.Key.Position.Equals(end)))
            {
                return (-1, Array.Empty<Reindeer>());
            }

            var min = states
                .Where(p => p.Key.Position.Equals(end))
                .Select(p => p.Value.Cost)
                .Min();

            var endReindeer = states
                .Where(p => p.Key.Position.Equals(end) &&
                            p.Value.Cost == min)
                .First().Key;

            var path = TracePath(states, endReindeer);

            return (min, path);
        }

        private static IReadOnlyList<Reindeer> TracePath(
            IReadOnlyDictionary<Reindeer, State> states,
            Reindeer end)
        {
            var path = new List<Reindeer>();

            var reindeer = end;
            while (true)
            {
                path.Add(reindeer);

                var state = states[reindeer];
                if (state.Previous == reindeer)
                {
                    break;
                }
                reindeer = state.Previous;
            }

            path.Reverse();
            return path;
        }

        private static IReadOnlyDictionary<Reindeer, State> RunDijkstra(
            Grid grid,
            Reindeer initial,
            Position end,
            HashSet<Position> blocked,
            long? knownMinCost)
        {
            var visit = new PriorityQueue<Reindeer, long>();
            visit.Enqueue(initial, 0);

            var states = new Dictionary<Reindeer, State>();
            states.Add(initial, new(0, initial));

            while (visit.Count > 0)
            {
                var reindeer = visit.Dequeue();

                if (reindeer.Position.Equals(end))
                {
                    continue;
                }

                var state = states[reindeer];

                foreach (var (nextReindeer, costIncrease) in NextPossibleTransitions(grid, reindeer, blocked))
                {
                    var nextCost = state.Cost + costIncrease;
                    if (knownMinCost is not null && nextCost > knownMinCost)
                    {
                        continue;
                    }

                    if (states.TryGetValue(nextReindeer, out var existingState) &&
                        existingState.Cost <= nextCost)
                    {
                        continue;
                    }

                    states[nextReindeer] = new State(nextCost, reindeer);
                    visit.Enqueue(nextReindeer, nextCost);
                }
            }

            return states;
        }

        private record State(long Cost, Reindeer Previous);

        public static IEnumerable<(Reindeer, int)> NextPossibleTransitions(
            Grid grid,
            Reindeer reindeer,
            HashSet<Position> blocked)
        {
            const int ROTATION_COST = 1000;
            const int MOVEMENT_COST = 1;

            if (ShouldConsiderTurns(grid, reindeer.Position))
            {
                yield return (reindeer with
                {
                    Direction = DirectionUtil.RotateClockwise(reindeer.Direction)
                }, ROTATION_COST);
                yield return (reindeer with
                {
                    Direction = DirectionUtil.RotateCounterClockwise(reindeer.Direction)
                }, ROTATION_COST);
            }

            var next = reindeer.Position.Shift(reindeer.Direction);
            if (grid.IsFreeAt(next) && !blocked.Contains(next))
            {
                yield return (reindeer with
                {
                    Position = next
                }, MOVEMENT_COST);
            }
        }

        private static bool ShouldConsiderTurns(Grid grid, Position pos)
        {
            var l = grid.IsFreeAt(pos.Shift(Direction.Left));
            var r = grid.IsFreeAt(pos.Shift(Direction.Right));
            var u = grid.IsFreeAt(pos.Shift(Direction.Up));
            var d = grid.IsFreeAt(pos.Shift(Direction.Down));

            var inCorridor =
                (l && r && !u && !d) ||
                (!l && !r && u && d);

            return !inCorridor;
        }
    }

    private record Reindeer(Position Position, Direction Direction);

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
        public static Direction? TryParse(char ch) =>
            ch switch
            {
                '^' => Direction.Up,
                'v' => Direction.Down,
                '<' => Direction.Left,
                '>' => Direction.Right,

                _ => null
            };

        public static Direction RotateClockwise(Direction dir) =>
            dir switch
            {
                Direction.Up => Direction.Left,
                Direction.Left => Direction.Down,
                Direction.Down => Direction.Right,
                Direction.Right => Direction.Up,

                _ => throw new Exception("impossible"),
            };

        public static Direction RotateCounterClockwise(Direction dir) =>
            dir switch
            {
                Direction.Up => Direction.Right,
                Direction.Right => Direction.Down,
                Direction.Down => Direction.Left,
                Direction.Left => Direction.Up,

                _ => throw new Exception("impossible"),
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
            Contains(p) ? this.cells[p.Row][p.Col] : '#';

        public bool IsFreeAt(Position p) =>
            At(p) == '.';
    }
}
