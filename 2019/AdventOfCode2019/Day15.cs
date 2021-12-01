using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Computer = AdventOfCode2019.Day09.Computer;

namespace AdventOfCode2019
{
    static class Day15
    {
        public static readonly IInput SampleInput =
            Input.Literal();

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/15/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();

                var board = Simulation.Run(program, true /* stopWhenOxygenSystemFound */, out var oxygenSystemPos);

                var oxygenSystemState = board[oxygenSystemPos];
                Console.WriteLine(oxygenSystemState.Distance);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();
                var directions = Enum.GetValues<Direction>();

                var board = Simulation.Run(program, false /* stopWhenOxygenSystemFound */, out var oxygenSystemPos);
                Draw(board, oxygenSystemPos);

                var toVisit = new HashSet<Position>();
                toVisit.Add(oxygenSystemPos);

                var visited = new HashSet<Position>();

                var time = 0;
                while (toVisit.Count > 0)
                {
                    foreach (var pos in toVisit)
                    {
                        visited.Add(pos);
                    }

                    toVisit = toVisit
                        .SelectMany(pos => directions.Select(d => pos.Neighbour(d)))
                        .Distinct()
                        .Where(pos => !visited.Contains(pos))
                        .Where(pos => !board[pos].IsWall)
                        .ToHashSet();

                    //Draw(board, oxygenSystemPos, visited);
                    //Console.ReadLine();

                    time++;
                }

                Console.WriteLine(time - 1);
            }

            private void Draw(Dictionary<Position, State> board, Position startPos, HashSet<Position> visited = null)
            {
                visited = visited ?? new HashSet<Position>();

                var minY = board.Keys.Select(pos => pos.Y).Min();
                var maxY = board.Keys.Select(pos => pos.Y).Max();

                var minX = board.Keys.Select(pos => pos.X).Min();
                var maxX = board.Keys.Select(pos => pos.X).Max();

                for (var y = minY; y <= maxY; y++)
                {
                    for (var x = minX; x <= maxX; x++)
                    {
                        var pos = new Position(x, y);

                        if (pos.Equals(startPos))
                        {
                            Console.Write("<>");
                        } 
                        else if (visited.Contains(pos))
                        {
                            Console.Write("..");
                        }
                        else if (board.TryGetValue(pos, out var state))
                        {
                            Console.Write(state.IsWall ? "██" : "  ");
                        }
                        else
                        {
                            Console.Write("??");
                        }
                    }
                    Console.WriteLine();
                }

                Console.WriteLine();
            }
        }

        private enum Direction { North = 1, South = 2, West = 3, East = 4 }

        private class State
        {
            public Computer Computer { get; set; }
            public int Distance { get; set; }
            public bool IsWall { get; set; }
        }

        private class Position
        {
            public Position(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }

            public override bool Equals(object obj) =>
                obj is Position other ? X == other.X && Y == other.Y : false;

            public override int GetHashCode() => HashCode.Combine(X, Y);

            public Position Neighbour(Direction dir)
            {
                if (dir == Direction.North) return new Position(X, Y - 1);
                if (dir == Direction.South) return new Position(X, Y + 1);
                if (dir == Direction.West) return new Position(X - 1, Y);
                return new Position(X + 1, Y);
            }
        }

        private static class Simulation
        {
            public static Dictionary<Position, State> Run(
                string program, 
                bool stopWhenOxygenSystemFound, 
                out Position oxygenSystemPos)
            {
                var visitQueue = new Queue<Position>();
                visitQueue.Enqueue(new Position(0, 0));

                var board = new Dictionary<Position, State>();
                board.Add(new Position(0, 0), new State()
                {
                    Computer = Computer.Of(program),
                    Distance = 0,
                    IsWall = false
                });

                var directions = Enum.GetValues<Direction>();
                oxygenSystemPos = null;

                while (visitQueue.Count > 0)
                {
                    var pos = visitQueue.Dequeue();
                    var state = board[pos];

                    foreach (var direction in directions)
                    {
                        var nextPos = pos.Neighbour(direction);
                        var nextDistance = state.Distance + 1;

                        if (board.TryGetValue(nextPos, out var nextState) &&
                            (nextState.IsWall || nextState.Distance <= nextDistance))
                        {
                            continue;
                        }

                        var nextComputer = state.Computer.Clone();

                        nextComputer.Input.Enter((int)direction);
                        nextComputer.Execute();
                        var result = nextComputer.Output.Values().Last();

                        board[nextPos] = new State()
                        {
                            Computer = nextComputer,
                            Distance = nextDistance,
                            IsWall = result == 0
                        };

                        if (result == 1)
                        {
                            visitQueue.Enqueue(nextPos);
                        }
                        else if (result == 2)
                        {
                            oxygenSystemPos = nextPos;

                            if (stopWhenOxygenSystemFound) break;
                        }
                    }

                    if (stopWhenOxygenSystemFound && oxygenSystemPos != null) break;
                }

                return board;
            }
        }
    }
}
