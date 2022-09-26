using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day20
    {
        public static class Inputs
        {
            public static readonly IInput Sample1 = 
                Input.Literal("^WNE$");

            public static readonly IInput Sample2 =
                Input.Literal("^ENWWW(NEEE|SSE(EE|N))$");

            public static readonly IInput Sample3 =
                Input.Literal("^ENNWSWW(NEWS|)SSSEEN(WNSE|)EE(SWEN|)NNN$");

            public static readonly IInput Sample4 =
                Input.Literal("^ESSWWN(E|NNENN(EESS(WNSE|)SSS|WWWSSSSE(SW|NNNE)))$");

            public static readonly IInput Sample5 =
                Input.Literal("^WSSEESWWWNW(S|NENNEEEENN(ESSSSW(NWSW|SSEN)|WSWWN(E|WWS(E|SS))))$");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/20/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var regex = input.Lines().First();

                var expr = Expr.Parse(regex);
                var node = Node.Of(expr);
                var map = Map.Of(node, Position.Origin);

                var distances = map.ShortestDistances(Position.Origin);
                Console.WriteLine($"max doors = {distances.Values.Max()}");
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var regex = input.Lines().First();

                var expr = Expr.Parse(regex);
                var node = Node.Of(expr);
                var map = Map.Of(node, Position.Origin);

                var distances = map.ShortestDistances(Position.Origin);
                Console.WriteLine($"doors = {distances.Values.Count(v => v >= 1000)}");
            }
        }

        private enum Direction { N, E, S, W };

        private abstract class Expr
        {
            public static Sequence Parse(string regex)
            {
                static bool TryParseDirection(char ch, out Direction dir)
                {
                    switch (ch)
                    {
                        case 'N':
                            dir = Direction.N;
                            return true;

                        case 'E':
                            dir = Direction.E;
                            return true;

                        case 'S':
                            dir = Direction.S;
                            return true;

                        case 'W':
                            dir = Direction.W;
                            return true;

                        default:
                            dir = default;
                            return false;
                    }
                }

                static (Steps, int) ParseSteps(string text, int start)
                {
                    var directions = new List<Direction>();

                    var i = start;
                    while (i < text.Length && TryParseDirection(text[i], out var dir))
                    {
                        directions.Add(dir);
                        i++;
                    }

                    return (new Steps(directions), i);
                }

                static (Alternative, int) ParseAlternative(string text, int start)
                {
                    var alts = new List<Expr>();

                    var i = start;
                    if (!(i < text.Length && text[i] == '('))
                    {
                        return (new Alternative(alts), i);
                    }

                    i++;

                    while (i < text.Length)
                    {
                        var (expr, next) = ParseSequence(text, i);
                        alts.Add(expr);
                        i = next;

                        var ch = text[i];
                        if (ch == ')')
                        {
                            i++;
                            break;
                        }

                        if (ch == '|')
                        {
                            i++;
                            continue;
                        }
                    }

                    return (new Alternative(alts), i);
                }

                static (Sequence, int) ParseSequence(string text, int start)
                {
                    var exprs = new List<Expr>();

                    var i = start;
                    while (i < text.Length)
                    {
                        var (alt, afterAlt) = ParseAlternative(text, i);
                        if (afterAlt != i)
                        {
                            exprs.Add(alt);
                            i = afterAlt;
                            continue;
                        }

                        var (steps, afterSteps) = ParseSteps(text, i);
                        if (afterSteps != i)
                        {
                            exprs.Add(steps);
                            i = afterSteps;
                            continue;
                        }

                        break;
                    }

                    return (new Sequence(exprs), i);
                }

                var (expr, _) = ParseSequence(regex, start: 1); // skip ^ at the beginning
                return expr;
            }

            private Expr() { }

            public sealed class Sequence : Expr
            {
                public Sequence(IReadOnlyList<Expr> expressions)
                {
                    Expressions = expressions;
                }

                public IReadOnlyList<Expr> Expressions { get; }

                public override string ToString() => string.Join(string.Empty, Expressions);
            }

            public sealed class Alternative : Expr
            {
                public Alternative(IReadOnlyList<Expr> expressions)
                {
                    Expressions = expressions;
                }

                public IReadOnlyList<Expr> Expressions { get; }

                public override string ToString() => $"({string.Join('|', Expressions)})";
            }

            public sealed class Steps : Expr
            {
                public Steps(IReadOnlyList<Direction> directions)
                {
                    Directions = directions;
                }

                public IReadOnlyList<Direction> Directions { get; }

                public override string ToString()
                {
                    static char DirectionChar(Direction dir) =>
                        dir switch
                        {
                            Direction.N => 'N',
                            Direction.E => 'E',
                            Direction.S => 'S',
                            Direction.W or _ => 'W'
                        };

                    return string.Join(string.Empty, Directions.Select(DirectionChar));
                }
            }
        }

        private sealed class Node
        {
            public static Node Of(Expr expr)
            {
                static IReadOnlyList<Node> CreateMany(IEnumerable<Expr> exprs) =>
                    exprs.Select(Of).ToList();

                static IReadOnlyList<Node> ConnectSequentially(IReadOnlyList<Node> paths)
                {
                    for (var i = 0; i < paths.Count - 1; i++)
                    {
                        var path = paths[i];
                        var next = new[] { paths[i + 1] };

                        foreach (var leaf in LeafsOf(path))
                        {
                            leaf.Paths = next;
                        }
                    }

                    return paths;
                }

                if (expr is Expr.Steps steps)
                {
                    return new Node(steps.Directions);
                }

                if (expr is Expr.Alternative alt)
                {
                    return new Node(CreateMany(alt.Expressions));
                }

                if (expr is Expr.Sequence seq)
                {
                    var paths = ConnectSequentially(CreateMany(seq.Expressions));
                    return paths.Count > 0 ? paths[0] : new Node();
                }

                throw new Exception("impossible");
            }

            private static IReadOnlyList<Node> LeafsOf(Node node)
            {
                static void Visit(Node node, List<Node> leafs)
                {
                    if (node.Paths.Count == 0)
                    {
                        leafs.Add(node);
                    }
                    else
                    {
                        foreach (var path in node.Paths)
                        {
                            Visit(path, leafs);
                        }
                    }
                }

                var leafs = new List<Node>();
                Visit(node, leafs);
                return leafs;
            }

            public Node() : this(Array.Empty<Direction>(), Array.Empty<Node>()) { }

            public Node(IReadOnlyList<Direction> directions) : this(directions, Array.Empty<Node>()) { }

            public Node(IReadOnlyList<Node> paths) : this(Array.Empty<Direction>(), paths) { }

            public static int count;

            public Node(IReadOnlyList<Direction> directions, IReadOnlyList<Node> paths)
            {
                if (paths.Count > 1) { count++; }

                Directions = directions;
                Paths = paths;
            }

            public IReadOnlyList<Direction> Directions { get; }
            public IReadOnlyList<Node> Paths { get; private set; }
        }

        private record Position(int X, int Y)
        {
            public static readonly Position Origin = new(0, 0);

            public Position Next(Direction dir) =>
                dir switch
                {
                    Direction.N => this with { Y = Y - 1 },
                    Direction.S => this with { Y = Y + 1 },
                    Direction.W => this with { X = X - 1 },
                    Direction.E => this with { X = X + 1 },
                    _ => this
                };
        }

        private sealed class Map
        {
            public static Map Of(Node node, Position origin)
            {
                static void Traverse(Map map, Node node, Position current, HashSet<(Node, Position)> seen)
                {
                    var position = current;
                    foreach (var direction in node.Directions)
                    {
                        var next = position.Next(direction);
                        map.AddPath(position, next);
                        position = next;
                    }

                    foreach (var path in node.Paths)
                    {
                        if (seen.Contains((path, position)))
                        {
                            continue;
                        }
                        seen.Add((path, position));

                        Traverse(map, path, position, seen);
                    }
                }

                var map = new Map();
                var seen = new HashSet<(Node, Position)>();
                Traverse(map, node, origin, seen);
                return map;
            }

            private readonly Dictionary<Position, HashSet<Position>> paths;

            public Map()
            {
                this.paths = new Dictionary<Position, HashSet<Position>>();
            }

            public void AddPath(Position x, Position y)
            {
                void AddDirection(Position from, Position to)
                {
                    if (this.paths.TryGetValue(from, out var ends))
                    {
                        ends.Add(to);
                    }
                    else
                    {
                        this.paths[from] = new HashSet<Position>() { to };
                    }
                }

                AddDirection(from: x, to: y);
                AddDirection(from: y, to: x);
            }

            private IEnumerable<Position> CanGoFrom(Position from) =>
                this.paths.TryGetValue(from, out var ends) ? ends : Enumerable.Empty<Position>();

            public IReadOnlyDictionary<Position, int> ShortestDistances(Position start)
            {
                var toVisit = new Queue<Position>();
                toVisit.Enqueue(start);

                var visited = new Dictionary<Position, int>();
                visited.Add(start, 0);

                while (toVisit.Count > 0)
                {
                    var pos = toVisit.Dequeue();
                    var nextDistance = visited[pos] + 1;

                    foreach (var nextPos in CanGoFrom(pos))
                    {
                        if (visited.TryGetValue(nextPos, out var distance) &&
                            distance <= nextDistance)
                        {
                            continue;
                        }

                        visited[nextPos] = nextDistance;
                        toVisit.Enqueue(nextPos);
                    }
                }

                return visited;
            }
        }
    }
}
