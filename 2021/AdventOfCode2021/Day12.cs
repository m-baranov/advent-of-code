using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day12
    {
        public static class Inputs
        {
            public static readonly IInput Sample1 =
                Input.Literal(
                    "start-A",
                    "start-b",
                    "A-c",
                    "A-b",
                    "b-d",
                    "A-end",
                    "b-end"
                );

            public static readonly IInput Sample2 =
                Input.Literal(
                    "dc-end",
                    "HN-start",
                    "start-kj",
                    "dc-start",
                    "dc-HN",
                    "LN-dc",
                    "HN-end",
                    "kj-sa",
                    "kj-HN",
                    "kj-dc"
                );

            public static readonly IInput Sample3 =
                Input.Literal(
                    "fs-end",
                    "he-DX",
                    "fs-he",
                    "start-DX",
                    "pj-DX",
                    "end-zg",
                    "zg-sl",
                    "zg-pj",
                    "pj-he",
                    "RW-he",
                    "fs-DX",
                    "pj-RW",
                    "zg-RW",
                    "start-pj",
                    "he-WI",
                    "zg-he",
                    "pj-fs",
                    "start-RW"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/12/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var connections = input.Lines().Select(Connection.Parse).ToList();

                var count = CountPaths(connections, new List<Cave>() { Cave.Start }, 0);

                Console.WriteLine(count);
            }

            private int CountPaths(IReadOnlyList<Connection> connections, List<Cave> path, int count)
            {
                var currentCave = path.Last();

                if (currentCave.Equals(Cave.End))
                {
                    return count + 1;
                }

                var visitedSmallCaves = path.Where(c => c.IsSmall());

                var nextCaves = Connection.CavesCanGoToFrom(connections, currentCave)
                    .Where(c => !visitedSmallCaves.Contains(c));

                foreach (var nextCave in nextCaves)
                {
                    path.Add(nextCave);
                    count = CountPaths(connections, path, count);
                    path.RemoveAt(path.Count - 1);
                }

                return count;
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var connections = input.Lines().Select(Connection.Parse).ToList();

                var count = CountPaths(connections, new List<Cave>() { Cave.Start }, 0);

                Console.WriteLine(count);
            }

            private int CountPaths(IReadOnlyList<Connection> connections, List<Cave> path, int count)
            {
                var currentCave = path.Last();

                if (currentCave.Equals(Cave.End))
                {
                    return count + 1;
                }

                var visitedSmallCaves = path.Where(c => c.IsSmall());

                var someSmallCaveVisitedTwice = visitedSmallCaves
                    .GroupBy(c => c)
                    .Any(g => g.Count() > 1);

                var nextCaves = Connection.CavesCanGoToFrom(connections, currentCave)
                    .Where(c => CanVisitCave(c, visitedSmallCaves, someSmallCaveVisitedTwice));

                foreach (var nextCave in nextCaves)
                {
                    path.Add(nextCave);
                    count = CountPaths(connections, path, count);
                    path.RemoveAt(path.Count - 1);
                }

                return count;
            }

            private static bool CanVisitCave(
                Cave cave, 
                IEnumerable<Cave> visitedSmallCaves, 
                bool someSmallCaveVisitedTwice)
            {
                if (!cave.IsSmall())
                {
                    return true;
                }

                var visited = visitedSmallCaves.Contains(cave);
                if (!visited)
                {
                    return true;
                }

                if (cave.IsStart())
                {
                    return false;
                }

                return !someSmallCaveVisitedTwice;
            }
        }

        private class Cave
        {
            public static readonly Cave Start = new Cave("start");
            public static readonly Cave End = new Cave("end");

            public Cave(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public bool IsStart() => Name == Start.Name;
            public bool IsEnd() => Name == End.Name;
            public bool IsSmall() => Name.All(char.IsLower);

            public override string ToString() => Name;

            public override bool Equals(object obj) =>
                obj is Cave other ? Name == other.Name : false;

            public override int GetHashCode() => Name.GetHashCode();
        }

        private class Connection
        {
            public static IEnumerable<Cave> CavesCanGoToFrom(IReadOnlyList<Connection> connections, Cave cave)
            {
                return connections
                    .Where(c => c.Start.Equals(cave)).Select(c => c.End)
                    .Concat(connections.Where(c => c.End.Equals(cave)).Select(c => c.Start));
            }

            public static Connection Parse(string line)
            {
                var parts = line.Split('-');
                return new Connection(new Cave(parts[0]), new Cave(parts[1]));
            }

            public Connection(Cave start, Cave end)
            {
                Start = start;
                End = end;
            }

            public Cave Start { get; }
            public Cave End { get; }
        }
    }
}
