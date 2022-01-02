using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day20
    {
        public static readonly IInput Sample1Input =
            Input.Literal(
                "                   A               ",
                "                   A               ",
                "  #################.#############  ",
                "  #.#...#...................#.#.#  ",
                "  #.#.#.###.###.###.#########.#.#  ",
                "  #.#.#.......#...#.....#.#.#...#  ",
                "  #.#########.###.#####.#.#.###.#  ",
                "  #.............#.#.....#.......#  ",
                "  ###.###########.###.#####.#.#.#  ",
                "  #.....#        A   C    #.#.#.#  ",
                "  #######        S   P    #####.#  ",
                "  #.#...#                 #......VT",
                "  #.#.#.#                 #.#####  ",
                "  #...#.#               YN....#.#  ",
                "  #.###.#                 #####.#  ",
                "DI....#.#                 #.....#  ",
                "  #####.#                 #.###.#  ",
                "ZZ......#               QG....#..AS",
                "  ###.###                 #######  ",
                "JO..#.#.#                 #.....#  ",
                "  #.#.#.#                 ###.#.#  ",
                "  #...#..DI             BU....#..LF",
                "  #####.#                 #.#####  ",
                "YN......#               VT..#....QG",
                "  #.###.#                 #.###.#  ",
                "  #.#...#                 #.....#  ",
                "  ###.###    J L     J    #.#.###  ",
                "  #.....#    O F     P    #.#...#  ",
                "  #.###.#####.#.#####.#####.###.#  ",
                "  #...#.#.#...#.....#.....#.#...#  ",
                "  #.#####.###.###.#.#.#########.#  ",
                "  #...#.#.....#...#.#.#.#.....#.#  ",
                "  #.###.#####.###.###.#.#.#######  ",
                "  #.#.........#...#.............#  ",
                "  #########.###.###.#############  ",
                "           B   J   C               ",
                "           U   P   P               "
            );

        public static readonly IInput Sample2Input =
            Input.Literal(
                "             Z L X W       C                 ",
                "             Z P Q B       K                 ",
                "  ###########.#.#.#.#######.###############  ",
                "  #...#.......#.#.......#.#.......#.#.#...#  ",
                "  ###.#.#.#.#.#.#.#.###.#.#.#######.#.#.###  ",
                "  #.#...#.#.#...#.#.#...#...#...#.#.......#  ",
                "  #.###.#######.###.###.#.###.###.#.#######  ",
                "  #...#.......#.#...#...#.............#...#  ",
                "  #.#########.#######.#.#######.#######.###  ",
                "  #...#.#    F       R I       Z    #.#.#.#  ",
                "  #.###.#    D       E C       H    #.#.#.#  ",
                "  #.#...#                           #...#.#  ",
                "  #.###.#                           #.###.#  ",
                "  #.#....OA                       WB..#.#..ZH",
                "  #.###.#                           #.#.#.#  ",
                "CJ......#                           #.....#  ",
                "  #######                           #######  ",
                "  #.#....CK                         #......IC",
                "  #.###.#                           #.###.#  ",
                "  #.....#                           #...#.#  ",
                "  ###.###                           #.#.#.#  ",
                "XF....#.#                         RF..#.#.#  ",
                "  #####.#                           #######  ",
                "  #......CJ                       NM..#...#  ",
                "  ###.#.#                           #.###.#  ",
                "RE....#.#                           #......RF",
                "  ###.###        X   X       L      #.#.#.#  ",
                "  #.....#        F   Q       P      #.#.#.#  ",
                "  ###.###########.###.#######.#########.###  ",
                "  #.....#...#.....#.......#...#.....#.#...#  ",
                "  #####.#.###.#######.#######.###.###.#.#.#  ",
                "  #.......#.......#.#.#.#.#...#...#...#.#.#  ",
                "  #####.###.#####.#.#.#.#.###.###.#.###.###  ",
                "  #.......#.....#.#...#...............#...#  ",
                "  #############.#.#.###.###################  ",
                "               A O F   N                     ",
                "               A A D   M                     "
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/20/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = Grid.Parse(input.Lines());

                var distance = grid.ShortestDistance(grid.Start, grid.End);
                Console.WriteLine(distance);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = Grid.Parse(input.Lines());

                var distance = grid.ShortestDistance(
                    new RecursivePosition(grid.Start, level: 0), 
                    new RecursivePosition(grid.End, level: 0));

                Console.WriteLine(distance);
            }
        }

        private class Position
        {
            public Position(int row, int col)
            {
                Row = row;
                Col = col;
            }

            public int Row { get; }
            public int Col { get; }

            public override string ToString() => $"({Row}, {Col})";

            public override bool Equals(object obj) =>
                obj is Position other ? Row == other.Row && Col == other.Col : false;

            public override int GetHashCode() => HashCode.Combine(Row, Col);

            public IEnumerable<Position> Neighbors()
            {
                yield return Up();
                yield return Right();
                yield return Down();
                yield return Left();
            }

            public Position Left() => new Position(Row, Col - 1);
            public Position Right() => new Position(Row, Col + 1);
            public Position Up() => new Position(Row - 1, Col);
            public Position Down() => new Position(Row + 1, Col);
        }

        private class RecursivePosition
        {
            public RecursivePosition(Position position, int level)
            {
                Position = position;
                Level = level;
            }

            public Position Position { get; }
            public int Level { get; }

            public override string ToString() => $"{Position}@{Level}";

            public override bool Equals(object obj) =>
               obj is RecursivePosition other ? Position.Equals(other.Position) && Level == other.Level : false;

            public override int GetHashCode() => HashCode.Combine(Position, Level);
        }

        private class Exit
        {
            public Exit(string label, Position position)
            {
                Label = label;
                Position = position;
            }

            public string Label { get; }
            public Position Position { get; }
        }

        private static class Symbols
        {
            public const char Wall = '#';
            public const char Passage = '.';

            public static bool IsLabel(char ch) => 'A' <= ch && ch <= 'Z';
        }

        private class Grid
        {
            public static Grid Parse(IEnumerable<string> lines)
            {
                var cells = lines.Select(l => l.ToArray()).ToArray();
                return new Grid(cells);
            }

            private readonly IReadOnlyList<IReadOnlyList<char>> cells;
            private readonly IReadOnlyDictionary<Position, Exit> portals;
            
            public Grid(IReadOnlyList<IReadOnlyList<char>> cells)
            {
                this.cells = cells;

                var exits = FindExits();

                Start = exits.First(e => e.Label == "AA").Position;
                End = exits.First(e => e.Label == "ZZ").Position;

                this.portals = MatchPortalExits(exits);
            }

            public int Rows => cells.Count;
            public int Cols => cells[0].Count;

            public Position Start { get; }
            public Position End { get; }

            public bool IsInBounds(Position pos) =>
                0 <= pos.Row && pos.Row < Rows &&
                0 <= pos.Col && pos.Col < Cols;

            public char At(Position pos) =>
                IsInBounds(pos) ? cells[pos.Row][pos.Col] : Symbols.Wall;

            private IReadOnlyDictionary<Position, Exit> MatchPortalExits(IEnumerable<Exit> exits)
            {
                return exits
                    .GroupBy(e => e.Label)
                    .Where(g => g.Count() > 1)
                    .SelectMany(g =>
                    {
                        var ex = g.ElementAt(0);
                        var ey = g.ElementAt(1);
                        return new[] { (ex.Position, ey), (ey.Position, ex) };
                    })
                    .ToDictionary(p => p.Item1, p => p.Item2);
            }

            private IReadOnlyList<Exit> FindExits()
            {
                var exits = new List<Exit>();

                for (var row = 0; row < cells.Count - 1; row++)
                {
                    for (var col = 0; col < cells[0].Count - 1; col++)
                    {
                        var exit = IsExit(new Position(row, col));
                        if (exit != null)
                        {
                            exits.Add(exit);
                        }
                    }
                }

                return exits;
            }

            private Exit IsExit(Position startPos) =>
                IsExit(startPos, p => p.Right(), p => p.Left()) ??
                IsExit(startPos, p => p.Down(), p => p.Up());

            private Exit IsExit(
                Position startPos,
                Func<Position, Position> inc,
                Func<Position, Position> dec) 
            {
                var label1 = At(startPos);
                if (!Symbols.IsLabel(label1))
                {
                    return null;
                }

                var endPos = inc(startPos);

                var label2 = At(endPos);
                if (!Symbols.IsLabel(label2))
                {
                    return null;
                }

                var name = $"{label1}{label2}";

                var exitPos = dec(startPos);
                if (At(exitPos) == Symbols.Passage)
                {
                    return new Exit(name, exitPos);
                }

                exitPos = inc(endPos);
                if (At(exitPos) == Symbols.Passage)
                {
                    return new Exit(name, exitPos);
                }

                return null;
            }

            // Dijkstra algorithm using a priority queue.
            public long ShortestDistance(Position start, Position end)
            {
                IEnumerable<Position> NeighboursOf(Position pos)
                {
                    var neighbours = pos.Neighbors().Where(p => At(p) == Symbols.Passage);
                    if (portals.TryGetValue(pos, out var portalExit))
                    {
                        neighbours = neighbours.Append(portalExit.Position);
                    }

                    return neighbours;
                }

                var toVisit = new PriorityQueue<Position>();
                toVisit.EnqueueOrUpdate(start, 0);

                var distances = new Dictionary<Position, long>();
                distances.Add(start, 0);

                while (!toVisit.IsEmpty())
                {
                    var position = toVisit.DequeueMinPriority();
                    var distance = distances[position];

                    foreach (var neighbour in NeighboursOf(position))
                    {
                        var newDistance = distance + 1;

                        if (distances.TryGetValue(neighbour, out var currentDistance) &&
                            currentDistance < newDistance)
                        {
                            continue;
                        }

                        distances[neighbour] = newDistance;
                        toVisit.EnqueueOrUpdate(neighbour, newDistance);
                    }
                }

                return distances[end];
            }

            public long ShortestDistance(RecursivePosition start, RecursivePosition end)
            {
                bool isOuterExit(Position pos) =>
                    pos.Col == 2 || pos.Col == Cols - 3 ||
                    pos.Row == 2 || pos.Row == Rows - 3;

                RecursivePosition tryGetPortalExit(RecursivePosition rp)
                {
                    if (!portals.TryGetValue(rp.Position, out var portalExit))
                    {
                        return null;
                    }

                    var isOuter = isOuterExit(rp.Position);

                    if (rp.Level == 0 && isOuter)
                    {
                        return null;
                    }

                    return new RecursivePosition(
                        portalExit.Position, 
                        isOuter ? rp.Level - 1 : rp.Level + 1);
                }

                char at(Position pos, int level)
                {
                    var ch = At(pos);
                    if (ch != Symbols.Passage)
                    {
                        return ch;
                    }

                    if (level == 0 && portals.ContainsKey(pos) && isOuterExit(pos))
                    {
                        return Symbols.Wall;
                    }
                    if (level > 0 && (pos == Start || pos == End))
                    {
                        return Symbols.Wall;
                    }
                    return Symbols.Passage;
                }

                IEnumerable<RecursivePosition> NeighboursOf(RecursivePosition rp)
                {
                    var neighbours = rp.Position.Neighbors()
                        .Where(p => at(p, rp.Level) == Symbols.Passage)
                        .Select(p => new RecursivePosition(p, rp.Level));

                    var portalExit = tryGetPortalExit(rp);
                    if (portalExit != null)
                    {
                        neighbours = neighbours.Append(portalExit);
                    }

                    return neighbours;
                }

                var maxLevel = 0;

                var toVisit = new PriorityQueue<RecursivePosition>();
                toVisit.EnqueueOrUpdate(start, 0);

                var distances = new Dictionary<RecursivePosition, long>();
                distances.Add(start, 0);

                do
                {
                    while (!toVisit.IsEmpty())
                    {
                        var position = toVisit.DequeueMinPriority();
                        var distance = distances[position];

                        foreach (var neighbour in NeighboursOf(position))
                        {
                            if (neighbour.Level > maxLevel)
                            {
                                continue;
                            }

                            var newDistance = distance + 1;

                            if (distances.TryGetValue(neighbour, out var currentDistance) &&
                                currentDistance < newDistance)
                            {
                                continue;
                            }

                            distances[neighbour] = newDistance;
                            toVisit.EnqueueOrUpdate(neighbour, newDistance);
                        }
                    }

                    maxLevel++;
                    toVisit.EnqueueOrUpdate(start, 0);
                } while (!distances.ContainsKey(end));

                return distances[end];
            }
        }

        // Poor man's "min-priority queue".
        private class PriorityQueue<T>
        {
            private readonly Dictionary<T, long> _priorities;

            public PriorityQueue()
            {
                _priorities = new Dictionary<T, long>();
            }

            public bool IsEmpty() => _priorities.Count == 0;

            public void EnqueueOrUpdate(T item, long priority)
            {
                _priorities[item] = priority;
            }

            public T DequeueMinPriority()
            {
                var pair = _priorities.MinBy(p => p.Value);
                _priorities.Remove(pair.Key);
                return pair.Key;
            }
        }
    }
}
