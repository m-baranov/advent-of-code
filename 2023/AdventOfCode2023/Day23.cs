using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day23
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
#.#####################
#.......#########...###
#######.#########.#.###
###.....#.>.>.###.#.###
###v#####.#v#.###.#.###
###.>...#.#.#.....#...#
###v###.#.#.#########.#
###...#.#.#.......#...#
#####.#.#.#######.#.###
#.....#.#.#.......#...#
#.#####.#.#.#########v#
#.#...#...#...###...>.#
#.#.#v#######v###.###v#
#...#.>.#...>.>.#.###.#
#####v#.#.###v#.#.###.#
#.....#...#...#.#.#...#
#.#########.###.#.#.###
#...###...#...#...#.###
###.###.#.###v#####v###
#...#...#.#.>.>.#.>.###
#.###.###.#.###.#.#v###
#.....###...###...#...#
#####################.#
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/23/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var start = new Position(Row: 0, Col: 1);
            var end = new Position(Row: grid.Rows - 1, Col: grid.Cols - 2);

            var max = MaxDistance(new Map(grid, start, end));
            Console.WriteLine(max);
        }

        private record Map(Grid Grid, Position Start, Position End);

        private static int MaxDistance(Map map)
        {
            static IReadOnlyList<Position> Next(Map map, Position pos)
            {
                if (!map.Grid.InBounds(pos))
                {
                    return Array.Empty<Position>();
                }

                var cell = map.Grid.At(pos);
                if (cell == '#')
                {
                    return Array.Empty<Position>();
                }

                return cell switch
                {
                    '>' => new[] { pos, pos.Right() },
                    '<' => new[] { pos, pos.Left() },
                    '^' => new[] { pos, pos.Up() },
                    'v' => new[] { pos, pos.Down() },
                    _ => new[] { pos },
                };
            }

            static int Recurse(Map map, Position pos, HashSet<Position> path)
            {
                if (pos.Equals(map.End))
                {
                    return path.Count;
                }

                var maxDistance = 0;

                var neighbours = new[] { pos.Up(), pos.Down(), pos.Left(), pos.Right() };
                foreach (var neighbour in neighbours)
                {
                    var next = Next(map, neighbour);
                    if (next.Count == 0)
                    {
                        continue;
                    }

                    if (next.Any(path.Contains))
                    {
                        continue;
                    }

                    path.AddRange(next);

                    var distance = Recurse(map, next[^1], path);
                    maxDistance = Math.Max(distance, maxDistance);

                    path.RemoveRange(next);
                }

                return maxDistance;
            }

            return Recurse(map, map.Start, new HashSet<Position>());
        }
    }

    public class Part2 : IProblem
    {
        // Not too fast (~20 sec), but it works.
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var start = new Position(Row: 0, Col: 1);
            var end = new Position(Row: grid.Rows - 1, Col: grid.Cols - 2);

            var graph = DiscoverGraph(grid, start, end);

            var max = MaxDistance(graph, start, end);

            Console.WriteLine(max);
        }

        private record Edge(int Length, Position End);

        private record Node(Position Start, IReadOnlyList<Edge> Edges);

        private static IReadOnlyDictionary<Position, Node> DiscoverGraph(Grid grid, Position start, Position end)
        {
            static IEnumerable<Position> Neighbours(Grid grid, Position pos) =>
                new[] { pos.Up(), pos.Down(), pos.Left(), pos.Right() }
                    .Where(grid.InBounds)
                    .Where(n => grid.At(n) != '#');

            static IReadOnlyList<Position> Nodes(Grid grid)
            {
                var nodes = new List<Position>();

                for (var row = 0; row < grid.Rows; row++)
                {
                    for (var col = 0; col < grid.Cols; col++)
                    {
                        var pos = new Position(row, col);
                        if (grid.At(pos) == '#')
                        {
                            continue;
                        }

                        var neighbours = Neighbours(grid, pos).Count();
                        if (neighbours > 2)
                        {
                            nodes.Add(pos);
                        }
                    }
                }

                return nodes;
            }

            static Edge Edge(Grid grid, Position node, Position start)
            {
                var length = 0;
                var prev = node;
                var end = start;
                while (true)
                {
                    var neighbours = Neighbours(grid, end).Where(n => !n.Equals(prev)).ToList();

                    if (neighbours.Count != 1)
                    {
                        break;
                    }

                    length++;
                    prev = end;
                    end = neighbours[0];
                }

                return new Edge(length, end);
            }

            static IReadOnlyList<Edge> Edges(Grid grid, Position node) =>
                Neighbours(grid, node).Select(n => Edge(grid, node, n)).ToList();
           
            return Nodes(grid).Prepend(start).Append(end)
                .Select(node => new Node(node, Edges(grid, node)))
                .ToDictionary(n => n.Start);
        }

        private static int MaxDistance(
            IReadOnlyDictionary<Position, Node> graph,
            Position start,
            Position end)
        {
            static int Recurse(
                IReadOnlyDictionary<Position, Node> graph,
                Position end,
                Position pos,
                int distance,
                HashSet<Position> seen)
            {
                if (pos.Equals(end))
                {
                    return distance;
                }

                if (seen.Contains(pos))
                {
                    return 0;
                }
                seen.Add(pos);

                var max = 0;

                var edges = graph[pos].Edges;
                foreach (var edge in edges)
                {
                    var found = Recurse(graph, end, edge.End, distance + edge.Length + 1, seen);
                    max = Math.Max(max, found);
                }

                seen.Remove(pos);
                return max;
            }

            return Recurse(graph, end, start, 0, new HashSet<Position>());
        }
    }

    private record Position(int Row, int Col)
    {
        public Position Up() => this with { Row = Row - 1 };
        public Position Down() => this with { Row = Row + 1 };
        public Position Left() => this with { Col = Col - 1 };
        public Position Right() => this with { Col = Col + 1 };

        public override string ToString() => $"r:{Row},c:{Col}";

    }

    private sealed class Grid
    {
        public static Grid Parse(IEnumerable<string> lines)
        {
            var cells = lines.Select(l => l.ToList()).ToList();
            return new Grid(cells);
        }

        private readonly IReadOnlyList<IReadOnlyList<char>> cells;

        public Grid(IReadOnlyList<IReadOnlyList<char>> cells)
        {
            this.cells = cells;
        }

        public int Rows => this.cells.Count;
        public int Cols => this.cells[0].Count;

        public bool InBounds(Position p) =>
            0 <= p.Row && p.Row < Rows &&
            0 <= p.Col && p.Col < Cols;

        public char At(Position p) => this.cells[p.Row][p.Col];
    }
}
