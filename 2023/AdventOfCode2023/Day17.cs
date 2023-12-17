using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day17
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
2413432311323
3215453535623
3255245654254
3446585845452
4546657867536
1438598798454
4457876987766
3637877979653
4654967986887
4564679986453
1224686865563
2546548887735
4322674655533
"""""");

        public static readonly IInput Sample2 =
            Input.Literal(""""""
111111111111
999999999991
999999999991
999999999991
999999999991
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/17/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var cost = grid.Traverse(State.Initial);

            Console.WriteLine(cost);
        }

        private record State(Position Position, Trail Trail) : IState
        {
            public static readonly State Initial = new(
                Position: new Position(0, 0),
                Trail: Trail.Empty(depth: 3)
            );

            public IState Move(Direction direction)
            {
                var nextPosition = Position.Move(direction);
                var nextTrail = Trail.Push(direction);

                return new State(nextPosition, nextTrail);
            }

            public IEnumerable<(Direction direction, int times)> PossibleDirections()
            {
                var direction = Trail.Last();
                if (direction is null)
                {
                    yield return (Direction.Right, 1);
                    yield return (Direction.Down, 1);
                    yield break;
                }

                if (direction is Direction.Up or Direction.Down)
                {
                    yield return (Direction.Left, 1);
                    yield return (Direction.Right, 1);
                }
                else
                {
                    yield return (Direction.Up, 1);
                    yield return (Direction.Down, 1);
                }

                if (Trail.CanContinue(direction.Value))
                {
                    yield return (direction.Value, 1);
                }
            }
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var cost = grid.Traverse(State.Initial);

            Console.WriteLine(cost);
        }

        private record State(Position Position, Trail Trail) : IState
        {
            public static readonly State Initial = new(
                Position: new Position(0, 0),
                Trail: Trail.Empty(depth: 10)
            );

            public IState Move(Direction direction)
            {
                var nextPosition = Position.Move(direction);
                var nextTrail = Trail.Push(direction);

                return new State(nextPosition, nextTrail);
            }

            public IEnumerable<(Direction direction, int times)> PossibleDirections()
            {
                const int Times = 4;

                var direction = Trail.Last();
                if (direction is null)
                {
                    yield return (Direction.Right, Times);
                    yield return (Direction.Down, Times);
                    yield break;
                }

                if (direction is Direction.Up or Direction.Down)
                {
                    yield return (Direction.Left, Times);
                    yield return (Direction.Right, Times);
                }
                else
                {
                    yield return (Direction.Up, Times);
                    yield return (Direction.Down, Times);
                }

                if (Trail.CanContinue(direction.Value))
                {
                    yield return (direction.Value, 1);
                }
            }
        }
    }

    private enum Direction { Up, Down, Left, Right }

    private record Position(int Row, int Col)
    {
        public Position Up() => this with { Row = Row - 1 };
        public Position Down() => this with { Row = Row + 1 };
        public Position Left() => this with { Col = Col - 1 };
        public Position Right() => this with { Col = Col + 1 };

        public Position Move(Direction dir) =>
            dir switch
            {
                Direction.Up => Up(),
                Direction.Down => Down(),
                Direction.Left => Left(),
                Direction.Right => Right(),
                _ => this,
            };
    }

    private record Trail(Direction Direction, int Times, int Depth)
    {
        public static Trail Empty(int depth) => new(Direction.Down, Times: 0, Depth: depth);

        public Trail Push(Direction direction)
        {
            if (Times == 0)
            {
                return new Trail(direction, Times: 1, Depth: Depth);
            }

            if (Direction != direction)
            {
                return new Trail(direction, Times: 1, Depth: Depth);
            }

            return new Trail(direction, Times + 1, Depth: Depth);
        }

        public Direction? Last() =>
            Times == 0 ? null : Direction;

        public bool CanContinue(Direction direction)
        {
            if (Times == 0)
            {
                return true;
            }

            if (Direction != direction)
            {
                return true;
            }

            return Times < Depth;
        }
    }

    private interface IState
    {
        Position Position { get; }
        IState Move(Direction direction);
        IEnumerable<(Direction direction, int times)> PossibleDirections();
    }

    private sealed class Grid
    {
        public static Grid Parse(IEnumerable<string> lines)
        {
            var cells = lines
                .Select(line => line
                    .Select(ch => int.Parse(ch.ToString()))
                    .ToList()
                )
                .ToList();

            return new Grid(cells);
        }

        private readonly IReadOnlyList<IReadOnlyList<int>> cells;

        public Grid(IReadOnlyList<IReadOnlyList<int>> cells)
        {
            this.cells = cells;
        }

        public int Rows => this.cells.Count;
        public int Cols => this.cells[0].Count;

        public bool InBounds(Position p) =>
            0 <= p.Row && p.Row < Rows &&
            0 <= p.Col && p.Col < Cols;

        public int At(Position p) =>
            this.cells[p.Row][p.Col];

        public int Traverse(IState initialState)
        {
            var endPos = new Position(Rows - 1, Cols - 1);

            var costs = new Costs(initial: int.MaxValue);
            costs.Set(initialState, 0);

            var states = new PriorityQueue<IState, int>();
            states.Enqueue(initialState, 0);

            while (states.Count > 0)
            {
                var state = states.Dequeue();

                var currentCost = costs.Get(state);

                foreach (var (direction, times) in state.PossibleDirections())
                {
                    var (nextState, moveCost) = Move(state, direction, times);
                    if (!InBounds(nextState.Position))
                    {
                        continue;
                    }

                    var nextCost = currentCost + moveCost;
                    if (costs.Get(nextState) <= nextCost)
                    {
                        continue;
                    }

                    costs.Set(nextState, nextCost);
                    states.Enqueue(nextState, nextCost);
                }
            }

            return costs.Min(endPos);
        }

        private (IState state, int cost) Move(IState state, Direction direction, int times)
        {
            var nextState = state;
            var cost = 0;
            for (var t = 0; t < times; t++)
            {
                nextState = nextState.Move(direction);
                if (InBounds(nextState.Position))
                {
                    cost += At(nextState.Position);
                }
                else
                {
                    break;
                }
            }
            return (nextState, cost);
        }
    }

    private sealed class Costs
    {
        private readonly Dictionary<IState, int> costs;
        private readonly int initialCost;

        public Costs(int initial)
        {
            this.costs = new Dictionary<IState, int>();
            this.initialCost = initial;
        }

        public int Get(IState state) =>
            this.costs.TryGetValue(state, out var cost)
                ? cost
                : this.initialCost;

        public void Set(IState state, int cost) =>
            this.costs[state] = cost;

        public int Min(Position pos) =>
            this.costs
                .Where(p => p.Key.Position.Equals(pos))
                .Select(p => p.Value)
                .DefaultIfEmpty(this.initialCost)
                .Min();
    }
}
