using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day18
    {
        public static readonly IInput Sample1Input =
            Input.Literal(
                "########################",
                "#...............b.C.D.f#",
                "#.######################",
                "#.....@.a.B.c.d.A.e.F.g#",
                "########################"
            );

        public static readonly IInput Sample2Input =
            Input.Literal(
                "#################",
                "#i.G..c...e..H.p#",
                "########.########",
                "#j.A..b...f..D.o#",
                "########@########",
                "#k.E..a...g..B.n#",
                "########.########",
                "#l.F..d...h..C.m#",
                "#################"
            );

        public static readonly IInput Sample3Input =
            Input.Literal(
                "########################",
                "#@..............ac.GI.b#",
                "###d#e#f################",
                "###A#B#C################",
                "###g#h#i################",
                "########################"
            );

        public static readonly IInput Sample4Input =
            Input.Literal(
                "#######",
                "#a.#Cd#",
                "##...##",
                "##.@.##",
                "##...##",
                "#cB#Ab#",
                "#######"
            );

        public static readonly IInput Sample5Input =
            Input.Literal(
                "###############",
                "#d.ABC.#.....a#",
                "######...######",
                "######.@.######",
                "######...######",
                "#b.....#.....c#",
                "###############"
            );

        public static readonly IInput Sample6Input =
            Input.Literal(
                "#############",
                "#DcBa.#.GhKl#",
                "#.###...#I###",
                "#e#d#.@.#j#k#",
                "###C#...###J#",
                "#fEbA.#.FgHi#",
                "#############"
            );

        public static readonly IInput Sample7Input =
            Input.Literal(
                "#############",
                "#g#f.D#..h#l#",
                "#F###e#E###.#",
                "#dCba...BcIJ#",
                "#####.@.#####",
                "#nK.L...G...#",
                "#M###N#H###.#",
                "#o#m..#i#jk.#",
                "#############"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/18/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                // This one took me A LOT of time to solve.

                var grid = Grid.Parse(input.Lines());

                var watch = Stopwatch.StartNew();

                var entrancePosition = grid.FindEntrances().Single();
                var (map, entranceArea) = MapGenerator.Discover(grid, entrancePosition);

                var initialPath = Path.Initial(map, entranceArea, entrancePosition);
                var bestSolution = Search(initialPath, Solution.Worst);

                watch.Stop();
                Console.WriteLine(bestSolution);
                Console.WriteLine($"Elapsed: {watch.ElapsedMilliseconds} ms");
            }

            private Solution Search(Path path, Solution solution)
            {
                if (path.Map.ContainsKeys.Count == path.CollectedKeys.Count)
                {
                    return path.Distance < solution.Distance ? Solution.Of(path) : solution;
                }

                var possibilities = path.PossibleNextPaths();
                foreach (var nextPath in possibilities)
                {
                    if (nextPath.Distance < solution.Distance)
                    {
                        solution = Search(nextPath, solution);
                    }
                }
                return solution;
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                // And this one took A LOT MORE!

                var grid = Grid.Parse(input.Lines()).ToMultiEntrance();

                var watch = Stopwatch.StartNew();

                var initialPaths = grid.FindEntrances()
                    .Select(entrancePosition =>
                    {
                        var (map, entranceArea) = MapGenerator.Discover(grid, entrancePosition);
                        return Path.Initial(map, entranceArea, entrancePosition);
                    })
                    .ToList();

                var bestSolution = Search(MultiPath.Of(initialPaths), Solution.Worst);

                watch.Stop();
                Console.WriteLine(bestSolution);
                Console.WriteLine($"Elapsed: {watch.ElapsedMilliseconds} ms");
            }

            private Solution Search(MultiPath paths, Solution solution)
            {
                if (paths.TotalKeys == paths.CollectedKeys.Count)
                {
                    return paths.Distance < solution.Distance ? Solution.Of(paths) : solution;
                }

                var possibilities = paths.PossibleNextPaths();
                foreach (var nextPaths in possibilities)
                {
                    if (nextPaths.Distance < solution.Distance)
                    {
                        solution = Search(nextPaths, solution);
                    }
                }
                return solution;
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

            public override string ToString() => $"({Row + 1},{Col + 1})";

            public override bool Equals(object obj) =>
                obj is Position other ? Row == other.Row && Col == other.Col : false;

            public override int GetHashCode() => HashCode.Combine(Row, Col);

            public IEnumerable<Position> CardinalNeighbours()
            {
                yield return Top();
                yield return Right();
                yield return Bottom();
                yield return Left();
            }

            public IEnumerable<Position> DiagonalNeighbours()
            {
                yield return TopLeft();
                yield return TopRight();
                yield return BottomRight();
                yield return BottomLeft();
            }

            public IEnumerable<IEnumerable<Position>> Corners()
            {
                yield return TopRightCorner();
                yield return BottomRightCorner();
                yield return BottomLeftCorner();
                yield return TopLeftCorner();
            }

            public IEnumerable<Position> TopRightCorner()
            {
                yield return Top();
                yield return TopRight();
                yield return Right();
            }

            public IEnumerable<Position> BottomRightCorner()
            {
                yield return Right();
                yield return BottomRight();
                yield return Bottom();
            }

            public IEnumerable<Position> BottomLeftCorner()
            {
                yield return Bottom();
                yield return BottomLeft();
                yield return Left();
            }

            public IEnumerable<Position> TopLeftCorner()
            {
                yield return Left();
                yield return TopLeft();
                yield return Top();
            }

            private Position Top() => new Position(Row - 1, Col);
            private Position TopRight() => new Position(Row - 1, Col + 1);
            private Position Right() => new Position(Row, Col + 1);
            private Position BottomRight() => new Position(Row + 1, Col + 1);
            private Position Bottom() => new Position(Row + 1, Col);
            private Position BottomLeft() => new Position(Row + 1, Col - 1);
            private Position Left() => new Position(Row, Col - 1);
            private Position TopLeft() => new Position(Row - 1, Col - 1);
        }

        private class DistanceLookup
        {
            private readonly IReadOnlyList<(Position x, Position y, int distance)> distances;

            public DistanceLookup(
                IReadOnlyList<(Position x, Position y, int distance)> distances)
            {
                this.distances = distances;
            }

            public int Between(Position x, Position y)
            {
                if (x.Equals(y))
                {
                    return 0;
                }

                var item = distances
                    .Where(d => (d.x.Equals(x) && d.y.Equals(y)) ||
                                (d.y.Equals(x) && d.x.Equals(y)))
                    .First();

                return item.distance;
            }
        }

        private static class Symbols
        {
            public const char Wall = '#';
            public const char Passage = '.';
            public const char Entrance = '@';

            public static bool IsKey(char ch) => 'a' <= ch && ch <= 'z';
            public static bool IsDoor(char ch) => 'A' <= ch && ch <= 'Z';
            public static char KeyForDoor(char door) => char.ToLower(door);
        }

        private class Grid
        {
            public static Grid Parse(IEnumerable<string> lines)
            {
                var cells = lines.Select(line => line.ToArray()).ToArray();
                return new Grid(cells);
            }

            private readonly IReadOnlyList<IReadOnlyList<char>> cells;

            public Grid(IReadOnlyList<IReadOnlyList<char>> cells)
            {
                this.cells = cells;
            }

            public int Rows => this.cells.Count;
            public int Cols => this.cells[0].Count;

            public bool IsInBounds(Position pos) => 
                0 <= pos.Row && pos.Row < Rows &&
                0 <= pos.Col && pos.Col < Cols;

            public char At(Position pos) => 
                IsInBounds(pos) ? this.cells[pos.Row][pos.Col] : Symbols.Wall;

            public IReadOnlyList<Position> FindEntrances()
            {
                var positions = new List<Position>();

                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Cols; col++)
                    {
                        var pos = new Position(row, col);
                        if (At(pos) == Symbols.Entrance)
                        {
                            positions.Add(pos);
                        }
                    }
                }

                return positions;
            }

            public IReadOnlyList<char> RequiresKeysAt(IEnumerable<Position> positions) =>
                positions.Select(At).Where(Symbols.IsDoor).Select(Symbols.KeyForDoor).ToList();

            public IReadOnlyList<char> ContainsKeysAt(IEnumerable<Position> positions) =>
                positions.Select(At).Where(Symbols.IsKey).ToList();

            public DistanceLookup FindShortestDistances(
                ISet<Position> area,
                IReadOnlyList<Position> points)
            {
                var allDistances = new List<(Position x, Position y, int distance)>();

                for (var i = 0; i < points.Count - 1; i++)
                {
                    var from = points[i];
                    var distances = FindShortestDistances(area, from);
                    allDistances.AddRange(points.Skip(i + 1).Select(to => (from, to, distances[to])));
                }

                return new DistanceLookup(allDistances);
            }

            private IReadOnlyDictionary<Position, int> FindShortestDistances(
                ISet<Position> area, 
                Position start)
            {
                var toVisit = new Queue<Position>();
                toVisit.Enqueue(start);

                var distances = new Dictionary<Position, int>();
                distances.Add(start, 0);

                while (toVisit.Count > 0)
                {
                    var position = toVisit.Dequeue();
                    var nextDistance = distances[position] + 1;

                    var neighbourPositions = position.CardinalNeighbours().Where(area.Contains);
                    foreach (var neighbourPosition in neighbourPositions)
                    {
                        if (At(neighbourPosition) == Symbols.Wall)
                        {
                            continue;
                        }

                        if (distances.TryGetValue(neighbourPosition, out var neighbourDistance) &&
                            neighbourDistance < nextDistance)
                        {
                            continue;
                        }

                        distances[neighbourPosition] = nextDistance;
                        toVisit.Enqueue(neighbourPosition);
                    }
                }

                return distances;
            }

            public Grid ToMultiEntrance()
            {
                var entrance = FindEntrances().Single();

                var rows = new List<List<char>>();

                for (var r = 0; r < Rows; r++)
                {
                    var row = new List<char>();

                    for (var c = 0; c < Cols; c++)
                    {
                        var pos = new Position(r, c);

                        var dr = Math.Abs(pos.Row - entrance.Row);
                        var dc = Math.Abs(pos.Col - entrance.Col);

                        if (dr == 1 && dc == 1)
                        {
                            row.Add(Symbols.Entrance);
                        }
                        else if (dr <= 1 && dc <= 1)
                        {
                            row.Add(Symbols.Wall);
                        }
                        else
                        {
                            row.Add(At(pos));
                        }
                    }

                    rows.Add(row);
                }

                return new Grid(rows);
            }
        }

        private static class MapGenerator
        {
            public static (Map map, Area entranceArea) Discover(Grid grid, Position entrance)
            {
                var areas = new List<WorkArea>();
                var lastAreaId = 0;

                var toVisit = new Queue<(Position position, int areaId)>();
                toVisit.Enqueue((entrance, ++lastAreaId));

                while (toVisit.Count > 0)
                {
                    var (position, areaId) = toVisit.Dequeue();

                    var discovered = DiscoverArea(grid, position);

                    var exits = new List<Exit>();
                    foreach (var exit in discovered.Exits)
                    {
                        int toAreaId;

                        var existingArea = areas.FirstOrDefault(a => a.Positions.Contains(exit.to));
                        if (existingArea != null)
                        {
                            toAreaId = existingArea.Id;
                        }
                        else
                        {
                            toAreaId = ++lastAreaId;
                            toVisit.Enqueue((exit.to, toAreaId));
                        }

                        exits.Add(new Exit(exit.from, exit.to, toAreaId));
                    }

                    areas.Add(new WorkArea(areaId, discovered.Positions, exits));
                }

                var map = new Map(areas.Select(area => CreateArea(area, grid, entrance)).ToList());
                return (map, map.AreaById(1));
            }

            private class WorkArea
            {
                public WorkArea(int id, ISet<Position> positions, IReadOnlyList<Exit> exits)
                {
                    Id = id;
                    Positions = positions;
                    Exits = exits;
                }

                public int Id { get; }
                public ISet<Position> Positions { get; }
                public IReadOnlyList<Exit> Exits { get; }
            }

            private static DiscoveryResult DiscoverArea(Grid grid, Position start)
            {
                var positions = new HashSet<Position>() { start };
                var exits = new List<(Position from, Position to)>();
                var cellType = ClassifyCell(grid, start);

                var toVisit = new Queue<Position>();
                toVisit.Enqueue(start);

                while (toVisit.Count > 0)
                {
                    var position = toVisit.Dequeue();

                    var neighbourPositions = position.CardinalNeighbours().Where(np => !positions.Contains(np));
                    foreach (var neighbourPosition in neighbourPositions)
                    {
                        var neighbourCellType = ClassifyCell(grid, neighbourPosition);
                        if (neighbourCellType == CellType.Wall)
                        {
                            continue;
                        }

                        // make keys and doors always be an area of its own
                        var sameArea = cellType != CellType.Key 
                            && cellType != CellType.Door
                            && neighbourCellType == cellType;

                        if (sameArea)
                        {
                            positions.Add(neighbourPosition);
                            toVisit.Enqueue(neighbourPosition);
                        }
                        else
                        {
                            exits.Add((from: position, to: neighbourPosition));
                        }
                    }
                }

                return new DiscoveryResult(positions, exits);
            }

            private class DiscoveryResult
            {
                public DiscoveryResult(
                    ISet<Position> positions,
                    IReadOnlyList<(Position from, Position to)> exits)
                {
                    Positions = positions;
                    Exits = exits;
                }

                public ISet<Position> Positions { get; }
                public IReadOnlyList<(Position from, Position to)> Exits { get; }
            }

            private static CellType ClassifyCell(Grid grid, Position pos)
            {
                var cell = grid.At(pos);
                if (cell == Symbols.Wall)
                {
                    return CellType.Wall;
                }
                if (Symbols.IsKey(cell))
                {
                    return CellType.Key;
                }
                if (Symbols.IsDoor(cell))
                {
                    return CellType.Door;
                }

                var isRoom = pos.Corners().Any(cps => cps.Select(grid.At).All(c => c != Symbols.Wall));
                if (isRoom)
                {
                    return CellType.Room;
                }

                var exits = pos.CardinalNeighbours().Select(grid.At).Count(c => c != Symbols.Wall);
                return exits > 2 ? CellType.Crossroads : CellType.Passage;
            }

            private enum CellType { Wall, Key, Door, Passage, Crossroads, Room }

            private static Area CreateArea(WorkArea area, Grid grid, Position entrance)
            {
                var containedKeys = grid.ContainsKeysAt(area.Positions);
                var requiredKeys = grid.RequiresKeysAt(area.Positions);

                var pointsOfInterest = area.Exits.Select(e => e.FromPosition);
                if (area.Positions.Contains(entrance))
                {
                    pointsOfInterest = pointsOfInterest.Append(entrance);
                }
                var distances = grid.FindShortestDistances(area.Positions, pointsOfInterest.ToList());

                return new Area(area.Id, area.Exits, distances, containedKeys, requiredKeys);
            }
        }

        private class Map
        {
            private readonly IReadOnlyDictionary<int, Area> _areaById;
            private readonly KeyRequirementsLookup _keyRequirementsLookup;

            public Map(IReadOnlyList<Area> areas)
            {
                _areaById = areas.ToDictionary(a => a.Id);
                _keyRequirementsLookup = KeyRequirementsLookup.Create(this);
                ContainsKeys = areas.SelectMany(a => a.ContainsKeys).ToList();
            }

            public IReadOnlyList<char> ContainsKeys { get; }

            public IEnumerable<Area> Areas() => _areaById.Values;

            public Area AreaById(int id) => _areaById[id];

            public IReadOnlyList<KeyRequirements> Keys(Area fromArea, Exit throughExit) =>
                _keyRequirementsLookup.Keys(fromArea, throughExit);
        }

        private class Area
        {
            public Area(
                int id, 
                IReadOnlyList<Exit> exits,
                DistanceLookup distances,
                IReadOnlyList<char> containsKeys,
                IReadOnlyList<char> requiresKeys)
            {
                Id = id;
                Exits = exits;
                Distances = distances;
                ContainsKeys = containsKeys;
                RequiresKeys = requiresKeys;
            }

            public int Id { get; } 
            public IReadOnlyList<Exit> Exits { get; }
            public DistanceLookup Distances { get; }
            public IReadOnlyList<char> ContainsKeys { get; }
            public IReadOnlyList<char> RequiresKeys { get; }
        }

        private class Exit
        {
            public Exit(Position fromPosition, Position toPosition, int toAreaId)
            {
                FromPosition = fromPosition;
                ToPosition = toPosition;
                ToAreaId = toAreaId;
            }

            public Position FromPosition { get; }
            public Position ToPosition { get; }
            public int ToAreaId { get; }
        }

        private class Path
        {
            public static Path Initial(Map map, Area area, Position position) =>
                new Path(map, area, position, distance: 0, steps: ImmutableStack<Step>.Empty);

            public Path(
                Map map,
                Area currentArea,
                Position currentPosition,
                int distance, 
                ImmutableStack<Step> steps,
                IReadOnlyList<char> keys = null,
                bool keysPickedUp = false)
            {
                Map = map;
                CurrentArea = currentArea;
                CurrentPosition = currentPosition;
                Distance = distance;
                Steps = steps;
                CollectedKeys = keys ?? Array.Empty<char>();
                KeysPickedUp = keysPickedUp;
            }

            public Map Map { get; }
            public Area CurrentArea { get; }
            public Position CurrentPosition { get; }
            public int Distance { get; }
            public ImmutableStack<Step> Steps { get; }
            public IReadOnlyList<char> CollectedKeys { get; }
            public bool KeysPickedUp { get; }

            public override string ToString() => $"{Distance}, keys={string.Join("", CollectedKeys)}";

            public Path Go(Exit throughExit)
            {
                var toArea = Map.AreaById(throughExit.ToAreaId);

                var nextSteps = Steps.Push(new Step(from: CurrentArea, exit: throughExit));

                // +1 accounts for going throuh exit
                var nextDistance = Distance + 1 + 
                    CurrentArea.Distances.Between(CurrentPosition, throughExit.FromPosition);

                var nextKeys = CollectedKeys.Concat(toArea.ContainsKeys).Distinct().ToList();
                var keysPickedUp = nextKeys.Count > CollectedKeys.Count;

                return new Path(Map, toArea, throughExit.ToPosition, nextDistance, nextSteps, nextKeys, keysPickedUp);
            }

            public IReadOnlyList<Path> PossibleNextPaths()
            {
                return Lookahead(CollectedKeys).Select(r => Go(r.exit)).ToList();
            }

            // Allow supplying collected keys externally and return more details to account
            // for the Part 2 requirements.
            public IReadOnlyList<(Exit exit, AvailableKeys availableKeys)> Lookahead(IReadOnlyList<char> collectedKeys)
            {
                var allowBacktracking = KeysPickedUp;

                var possibleExits = new List<(Exit exit, AvailableKeys availableKeys)>();

                foreach (var exit in CurrentArea.Exits)
                {
                    // Only consider going back to where we came from if a key is picked
                    // up on the last step. This ensures that we are always making progress,
                    // and removes infinite loops of going back and forth between two areas.
                    if (!allowBacktracking)
                    {
                        if (!Steps.IsEmpty && exit.ToAreaId == Steps.Peek().From.Id)
                        {
                            continue;
                        }
                    }

                    var availableKeys = Lookahead(exit, collectedKeys);
                    if (availableKeys == AvailableKeys.All)
                    {
                        return new[] { (exit, AvailableKeys.All) };
                    }

                    if (availableKeys == AvailableKeys.Some)
                    {
                        possibleExits.Add((exit, AvailableKeys.Some));
                    }
                }

                return possibleExits;
            }

            private AvailableKeys Lookahead(Exit exit, IReadOnlyList<char> collectedKeys)
            {
                var keysBehindExit = Map.Keys(CurrentArea, exit);

                var uncollectedCount = 0;
                var canCollectCount = 0;
                foreach (var keyReq in keysBehindExit)
                {
                    if (collectedKeys.Contains(keyReq.Key))
                    {
                        continue;
                    }

                    uncollectedCount++;
                    if (keyReq.RequiresKeys.All(rk => collectedKeys.Contains(rk)))
                    {
                        canCollectCount++;
                    }
                }

                if (uncollectedCount == 0 || canCollectCount == 0)
                {
                    return AvailableKeys.None;
                }
                if (canCollectCount == uncollectedCount)
                {
                    return AvailableKeys.All;
                }
                return AvailableKeys.Some;
            }
        }

        private enum AvailableKeys { None, Some, All };

        private class Step
        {
            public Step(Area from, Exit exit)
            {
                From = from;
                Exit = exit;
            }

            public Area From { get; }
            public Exit Exit { get; }
        }

        private class KeyRequirementsLookup
        {
            public static KeyRequirementsLookup Create(Map map)
            {
                var lookup = new Dictionary<(Area, Exit), IReadOnlyList<KeyRequirements>>();

                var allPairs = map.Areas().SelectMany(area => area.Exits.Select(exit => (area, exit)));
                foreach (var pair in allPairs)
                {
                    SearchKeys(map, pair, lookup);
                }

                return new KeyRequirementsLookup(lookup);
            }

            private static IReadOnlyList<KeyRequirements> SearchKeys(
                Map map,
                (Area area, Exit exit) from, 
                Dictionary<(Area, Exit), IReadOnlyList<KeyRequirements>> lookup)
            {
                if (lookup.TryGetValue(from, out var existingRequirements))
                {
                    return existingRequirements;
                }

                var area = map.AreaById(from.exit.ToAreaId);

                var areaRequirements = area.ContainsKeys
                    .Select(key => new KeyRequirements(key, area.RequiresKeys));

                var childRequirements = area.Exits.Where(e => e.ToAreaId != from.area.Id)
                    .Select(exit => (area, exit))
                    .SelectMany(p => SearchKeys(map, p, lookup))
                    .Select(r => new KeyRequirements(r.Key, r.RequiresKeys.Concat(area.RequiresKeys).ToList()));

                var requirements = areaRequirements.Concat(childRequirements).ToList();
                lookup[from] = requirements;
                return requirements;
            }

            private readonly IReadOnlyDictionary<(Area, Exit), IReadOnlyList<KeyRequirements>> lookup;

            public KeyRequirementsLookup(
                IReadOnlyDictionary<(Area, Exit), IReadOnlyList<KeyRequirements>> lookup)
            {
                this.lookup = lookup;
            }

            public IReadOnlyList<KeyRequirements> Keys(Area fromArea, Exit throughExit) => 
                lookup[(fromArea, throughExit)];
        }

        private class KeyRequirements
        {
            public KeyRequirements(char key, IReadOnlyList<char> requiresKeys)
            {
                Key = key;
                RequiresKeys = requiresKeys;
            }

            public char Key { get; }
            public IReadOnlyList<char> RequiresKeys { get; }
        }

        private class MultiPath
        {
            public static MultiPath Of(IReadOnlyList<Path> paths)
            {
                var totalKeys = paths.Select(p => p.Map.ContainsKeys.Count).Sum();
                return new MultiPath(paths, totalKeys);
            }

            public MultiPath(IReadOnlyList<Path> paths, int totalKeys)
            {
                Paths = paths;
                TotalKeys = totalKeys;

                CollectedKeys = paths.SelectMany(p => p.CollectedKeys).ToList();
                Distance = paths.Select(p => p.Distance).Sum();
            }

            public IReadOnlyList<Path> Paths { get; }
            public int TotalKeys { get; }
            public IReadOnlyList<char> CollectedKeys { get; }
            public int Distance { get; }

            public IReadOnlyList<MultiPath> PossibleNextPaths()
            {
                // Using LINQ in this method adds ~10 seconds to execution time.

                var possiblePaths = new List<(int pathIndex, IReadOnlyList<(Exit exit, AvailableKeys availableKeys)> exits)>();
                var index = 0;
                foreach (var path in Paths)
                {
                    var exits = path.Lookahead(CollectedKeys);
                    if (exits.Count == 0)
                    {
                        index++;
                        continue;
                    }

                    if (exits.Count == 1 && exits[0].availableKeys == AvailableKeys.All)
                    {
                        return new[] { Go(index, exits[0].exit) };
                    }

                    possiblePaths.Add((index, exits));
                    index++;
                }

                // If key was not picked up on a path, and next exit does not have a key 
                // to collect, move there without considering any other paths/exits. This
                // move would need to happen any way, so might as well take it. This cuts
                // down on a lot of unnecessary branching when bruteforcing all move
                // combinations.
                foreach (var (pathIndex, exits) in possiblePaths)
                {
                    var path = Paths[pathIndex];
                    if (path.KeysPickedUp)
                    {
                        continue;
                    }

                    foreach (var (exit, _) in exits)
                    {
                        var keys = path.Map.AreaById(exit.ToAreaId).ContainsKeys;
                        if (keys.Count == 0 || keys.All(k => CollectedKeys.Contains(k)))
                        {
                            return new[] { Go(pathIndex, exit) };
                        }
                    }
                }

                var nextPaths = new List<MultiPath>();
                foreach (var (pathIndex, exits) in possiblePaths)
                {
                    foreach (var (exit, _) in exits)
                    {
                        nextPaths.Add(Go(pathIndex, exit));
                    }
                }
                return nextPaths;
            }

            public MultiPath Go(int pathIndex, Exit throughExit)
            {
                var newPaths = Paths.ToList();
                newPaths[pathIndex] = Paths[pathIndex].Go(throughExit);
                return new MultiPath(newPaths, TotalKeys);
            }
        }

        private class Solution
        {
            public static readonly Solution Worst = 
                new Solution(int.MaxValue, Array.Empty<char>());

            public static Solution Of(Path path) => 
                new Solution(path.Distance, path.CollectedKeys);

            public static Solution Of(MultiPath path) =>
                new Solution(path.Distance, path.CollectedKeys);

            public Solution(int distance, IReadOnlyList<char> keys)
            {
                Distance = distance;
                Keys = keys;
            }

            public int Distance { get; }
            public IReadOnlyList<char> Keys { get; }

            public override string ToString() => $"{Distance}, keys={string.Join("", Keys)}";
        }        
    }
}
