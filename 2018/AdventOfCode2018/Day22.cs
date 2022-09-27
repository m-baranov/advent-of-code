using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day22
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "depth: 510",
                    "target: 10,10"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/22/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var info = CaveInfo.Parse(input.Lines().ToList());

                var cave = Cave.Create(info);
                var riskLevel = cave.RiskLevel();

                Console.WriteLine($"risk level = {riskLevel}");
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var info = CaveInfo.Parse(input.Lines().ToList());

                var approxCave = Cave.Create(info);
                var approxDistance = approxCave.ShortestDistanceToTarget();

                var maxCaveWidth = approxCave.Width + approxDistance / 2 + 1;
                var maxCaveHeight = approxCave.Height + approxDistance / 2 + 1;

                var cave = Cave.Create(info, maxCaveWidth, maxCaveHeight);
                var distance = cave.ShortestDistanceToTarget(maxDistance: approxDistance);

                Console.WriteLine($"distance = {distance}");
            }
        }

        private readonly record struct Position(int X, int Y)
        {
            public static readonly Position Origin = new(0, 0);

            public static Position Parse(string text)
            {
                var parts = text.Split(',');

                var x = int.Parse(parts[0].Trim());
                var y = int.Parse(parts[1].Trim());

                return new Position(x, y);
            }

            public IEnumerable<Position> Adjacent()
            {
                yield return Up();
                yield return Right();
                yield return Down();
                yield return Left();
            }

            private Position Up() => this with { Y = Y - 1 };
            private Position Right() => this with { X = X + 1 };
            private Position Down() => this with { Y = Y + 1 };
            private Position Left() => this with { X = X - 1 };

            public override string ToString() => $"({X},{Y})";
        }

        private record CaveInfo(int Depth, Position Target)
        {
            public static CaveInfo Parse(IReadOnlyList<string> lines)
            {
                const string depthPrefix = "depth: ";
                const string targetPrefix = "target: ";

                var depth = int.Parse(lines[0].Substring(depthPrefix.Length));
                var target = Position.Parse(lines[1].Substring(targetPrefix.Length));

                return new CaveInfo(depth, target);
            }
        }

        private enum Cell { Rocky, Wet, Narrow }

        private enum Tool { None, ClimbingGear, Torch }

        private class Cave
        {
            public static Cave Create(CaveInfo info, int? width = null, int? height = null)
            {
                static int GeologicIndex(Position target, int[,] erosion, Position pos) =>
                    pos switch
                    {
                        var p when p == Position.Origin || p == target => 0,
                        { X: var x, Y: 0 } => x * 16807,
                        { X: 0, Y: var y } => y * 48271,
                        { X: var x, Y: var y } => erosion[y, x - 1] * erosion[y - 1, x]
                    };

                static int ErosionLevel(CaveInfo info, int[,] erosion, Position pos) =>
                    (GeologicIndex(info.Target, erosion, pos) + info.Depth) % 20183;

                var effectiveWidth = width ?? info.Target.X + 1;
                var effectiveHeight = height ?? info.Target.Y + 1;
                var erosion = new int[effectiveHeight, effectiveWidth];

                for (var y = 0; y < effectiveHeight; y++)
                {
                    for (var x = 0; x < effectiveWidth; x++)
                    {
                        erosion[y, x] = ErosionLevel(info, erosion, new Position(x, y));
                    }
                }

                return new Cave(info, erosion);
            }

            private readonly CaveInfo info;
            private readonly int[,] erosion;

            public Cave(CaveInfo info, int[,] erosion)
            {
                this.info = info;
                this.erosion = erosion;
            }

            public int Height => this.erosion.GetLength(0);
            public int Width => this.erosion.GetLength(1);

            public bool InBounds(Position p) =>
                0 <= p.Y && p.Y < this.Height &&
                0 <= p.X && p.X < this.Width;

            public Cell At(Position p) => At(p.X, p.Y);
            public Cell At(int x, int y) => (Cell)(this.erosion[y, x] % 3);

            public int RiskLevel()
            {
                var riskLevel = 0;
                for (var y = 0; y < this.Height; y++)
                {
                    for (var x = 0; x < this.Width; x++)
                    {
                        riskLevel += (int)this.At(x, y);
                    }
                }

                return riskLevel;
            }

            public void Display()
            {
                static char CharOf(Cell cell) =>
                    cell switch
                    {
                        Cell.Rocky => '.',
                        Cell.Wet => '=',
                        Cell.Narrow => '|',
                        _ => '?'
                    };

                for (var y = 0; y < this.Height; y++)
                {
                    for (var x = 0; x < this.Width; x++)
                    {
                        Console.Write(CharOf(this.At(x, y)));
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }

            public int ShortestDistanceToTarget(int? maxDistance = null)
            {
                const int MoveCost = 1;
                const int ToolSwitchCost = 7;

                static IReadOnlyList<Tool> PossibleNextTools(Cell current, Cell next) =>
                    (current, next) switch
                    {
                        (Cell.Rocky, Cell.Wet) or (Cell.Wet, Cell.Rocky) => Tools.ClimbingGear,
                        (Cell.Rocky, Cell.Narrow) or (Cell.Narrow, Cell.Rocky) => Tools.Torch,
                        (Cell.Wet, Cell.Narrow) or (Cell.Narrow, Cell.Wet) => Tools.None,
                        (Cell.Rocky, Cell.Rocky) => Tools.ClimbingGearAndTorch,
                        (Cell.Wet, Cell.Wet) => Tools.ClimbingGearAndNone,
                        (Cell.Narrow, Cell.Narrow) or _ => Tools.TorchAndNone,
                    };

                static int Cost(Tool currentTool, Tool nextTool) =>
                    nextTool == currentTool ? MoveCost : MoveCost + ToolSwitchCost;

                // using PriorityQueue over plain Queue makes huge difference in perf here (~ 35 sec vs 2 sec)

                var toVisit = new PriorityQueue<(Position position, Tool tool), int>();
                toVisit.Enqueue((Position.Origin, Tool.Torch), 0);

                var distances = new Dictionary<(Position position, Tool tool), int>();
                distances.Add((Position.Origin, Tool.Torch), 0);

                while (toVisit.Count > 0)
                {
                    var (currentPos, currentTool) = toVisit.Dequeue();
                    var currentDistance = distances[(currentPos, currentTool)];

                    foreach (var nextPos in currentPos.Adjacent().Where(InBounds))
                    {
                        foreach (var nextTool in PossibleNextTools(At(currentPos), At(nextPos)))
                        {
                            var nextDistance = currentDistance + Cost(currentTool, nextTool);

                            if (maxDistance != null && nextDistance > maxDistance)
                            {
                                continue;
                            }

                            if (distances.TryGetValue((nextPos, nextTool), out var distance) &&
                                distance <= nextDistance)
                            {
                                continue;
                            }

                            distances[(nextPos, nextTool)] = nextDistance;
                            toVisit.Enqueue((nextPos, nextTool), nextDistance);
                        }
                    }
                }

                return distances
                    .Where(p => p.Key.position == this.info.Target)
                    .Select(p => p.Key.tool == Tool.Torch ? p.Value : p.Value + ToolSwitchCost)
                    .Min();
            }

            private static class Tools
            {
                public static readonly IReadOnlyList<Tool> ClimbingGear = new[] { Tool.ClimbingGear };
                public static readonly IReadOnlyList<Tool> Torch = new[] { Tool.Torch };
                public static readonly IReadOnlyList<Tool> None = new[] { Tool.None };
                public static readonly IReadOnlyList<Tool> ClimbingGearAndTorch = new[] { Tool.ClimbingGear, Tool.Torch };
                public static readonly IReadOnlyList<Tool> ClimbingGearAndNone = new[] { Tool.ClimbingGear, Tool.None };
                public static readonly IReadOnlyList<Tool> TorchAndNone = new[] { Tool.Torch, Tool.None };
            }
        }
    }
}
