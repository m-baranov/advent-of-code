using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/18/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                // This one took me A LOT of time to solve.

                var grid = Grid.Parse(input.Lines());

                var keyCount = grid.CountKeys();

                var entrancePosition = grid.PositionOfEntrance();
                var map = MapGenerator.Discover(grid, entrancePosition);
                var entranceArea = map.AreaById(1);

                var initialPath = new Path(entranceArea, entrancePosition, distance: 0, steps: ImmutableStack<Step>.Empty);
                var bestPath = Search(keyCount, map, initialPath, bestPath: null);
                Console.WriteLine(bestPath);
            }

            private Path Search(int keyCount, Map map, Path currentPath, Path bestPath)
            {
                if (keyCount == currentPath.Keys.Count())
                {
                    return currentPath.IsShorterThan(bestPath) ? currentPath : bestPath;
                }

                var allowBacktracking = currentPath.KeysPickedUp;

                IEnumerable<Exit> exitsToConsider = currentPath.CurrentArea.Exits;
                if (!allowBacktracking && !currentPath.Steps.IsEmpty)
                {
                    var cameFromAreaId = currentPath.Steps.Peek().From.Id;
                    exitsToConsider = exitsToConsider.Where(e => e.ToAreaId != cameFromAreaId);
                }

                var exitOptions = exitsToConsider
                    .Select(exit => (exit: exit, availableKeys: Lookahead(map, currentPath, exit)))
                    .ToList();

                var exitWithAllKeys = exitOptions
                    .Where(o => o.availableKeys == AvailableKeys.All)
                    .Select(o => o.exit)
                    .FirstOrDefault();
                if (exitWithAllKeys != null)
                {
                    var nextPath = currentPath.Go(exitWithAllKeys, map);
                    return Search(keyCount, map, nextPath, bestPath);
                }

                var exitsWithSomeKeys = exitOptions
                   .Where(o => o.availableKeys == AvailableKeys.Some)
                   .Select(o => o.exit);
                foreach (var exitWithSomeKeys in exitsWithSomeKeys)
                {
                    var nextPath = currentPath.Go(exitWithSomeKeys, map);
                    bestPath = Search(keyCount, map, nextPath, bestPath);
                }

                return bestPath;
            }

            private AvailableKeys Lookahead(Map map, Path path, Exit exit)
            {
                var collectedKeys = path.Keys;
                var keysBehindExit = map.Keys(path.CurrentArea, exit);

                var notCollected = keysBehindExit.Where(d => !collectedKeys.Contains(d.Key)).ToList();
                if (notCollected.Count == 0)
                {
                    return AvailableKeys.None;
                }

                var canCollectCount = notCollected.Count(d => d.RequiresKeys.All(rk => collectedKeys.Contains(rk)));
                if (canCollectCount == 0)
                {
                    return AvailableKeys.None;
                }
                else if (canCollectCount == notCollected.Count)
                {
                    return AvailableKeys.All;
                }
                else
                {
                    return AvailableKeys.Some;
                }
            }

            private enum AvailableKeys { None, Some, All };
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                throw new NotImplementedException();
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

        private abstract class Cell
        {
            public static Cell Parse(char ch) =>
                ch switch
                {
                    '@' => Entrance.Instance,
                    '#' => Wall.Instance,
                    var k when 'a' <= k && k <= 'z' => new Key(k),
                    var d when 'A' <= d && d <= 'Z' => new Door(char.ToLower(d)),
                    _ => Passage.Instance
                };

            public override string ToString()
            {
                if (this is Entrance)
                {
                    return "@";
                }
                else if (this is Wall)
                {
                    return "#";
                }
                else if (this is Key key)
                {
                    return key.Symbol.ToString();
                }
                else if (this is Door door)
                {
                    return char.ToUpper(door.Symbol).ToString();
                }
                else
                {
                    return ".";
                }
            }

            public override bool Equals(object obj) =>
                obj is Cell other ? ToString() == other.ToString() : false;

            public override int GetHashCode() => ToString().GetHashCode();

            public class Entrance : Cell
            {
                public static readonly Entrance Instance = new Entrance();
                private Entrance() { }
            }

            public class Wall : Cell
            {
                public static readonly Wall Instance = new Wall();
                private Wall() { }
            }

            public class Passage : Cell
            {
                public static readonly Passage Instance = new Passage();
                private Passage() { }
            }

            public class Door : Cell
            {
                public Door(char symbol)
                {
                    Symbol = symbol;
                }

                public char Symbol { get; }
            }

            public class Key : Cell
            {
                public Key(char symbol)
                {
                    Symbol = symbol;
                }

                public char Symbol { get; }
            }
        }

        private class Grid
        {
            public static Grid Parse(IEnumerable<string> lines)
            {
                var cells = lines
                    .Select(line => line.Select(Cell.Parse).ToArray())
                    .ToArray();

                return new Grid(cells);
            }

            private readonly IReadOnlyList<IReadOnlyList<Cell>> cells;

            public Grid(IReadOnlyList<IReadOnlyList<Cell>> cells)
            {
                this.cells = cells;
            }

            public int Rows => this.cells.Count;
            public int Cols => this.cells[0].Count;

            public bool IsInBounds(Position pos) => 
                0 <= pos.Row && pos.Row < Rows &&
                0 <= pos.Col && pos.Col < Cols;

            public Cell At(Position pos)
            {
                if (IsInBounds(pos))
                {
                    return this.cells[pos.Row][pos.Col];
                }
                return Cell.Wall.Instance;
            }

            public Position PositionOfEntrance() => PositionOf(Cell.Entrance.Instance);

            public Position PositionOf(Cell cellToFind)
            {
                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Cols; col++)
                    {
                        var cell = cells[row][col];
                        if (cell.Equals(cellToFind))
                        {
                            return new Position(row, col);
                        }
                    }
                }

                return null;
            }

            public int CountKeys() => Count(c => c is Cell.Key);

            public int Count(Func<Cell, bool> predicate)
            {
                var count = 0;

                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Cols; col++)
                    {
                        var cell = cells[row][col];
                        if (predicate(cell))
                        {
                            count++;
                        }
                    }
                }

                return count;
            }

            public IReadOnlyList<char> RequiresKeysAt(IEnumerable<Position> positions) =>
                positions.Select(At).OfType<Cell.Door>().Select(d => d.Symbol).ToList();

            public IReadOnlyList<char> ContainsKeysAt(IEnumerable<Position> positions) =>
                positions.Select(At).OfType<Cell.Key>().Select(d => d.Symbol).ToList();

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
                        if (At(neighbourPosition) is Cell.Wall)
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
        }

        private static class MapGenerator
        {
            public static Map Discover(Grid grid, Position entrance)
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

                return new Map(areas.Select(area => CreateArea(area, grid, entrance)).ToList());
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

                        if (cellType != CellType.Key && neighbourCellType == cellType)
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
                if (cell is Cell.Wall)
                {
                    return CellType.Wall;
                }
                if (cell is Cell.Key)
                {
                    return CellType.Key;
                }

                var isRoom = pos.Corners().Any(cps => cps.Select(grid.At).All(c => c is not Cell.Wall));
                if (isRoom)
                {
                    return CellType.Room;
                }

                var exits = pos.CardinalNeighbours().Select(grid.At).Count(c => c is not Cell.Wall);
                return exits > 2 ? CellType.Crossroads : CellType.Passage;
            }

            private enum CellType { Wall, Key, Passage, Crossroads, Room }

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
            }

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
            public Path(
                Area currentArea,
                Position currentPosition,
                int distance, 
                ImmutableStack<Step> steps,
                IReadOnlyList<char> keys = null,
                bool keysPickedUp = false)
            {
                CurrentArea = currentArea;
                CurrentPosition = currentPosition;
                Distance = distance;
                Steps = steps;
                Keys = keys ?? Array.Empty<char>();
                KeysPickedUp = keysPickedUp;
            }

            public Area CurrentArea { get; }
            public Position CurrentPosition { get; }
            public int Distance { get; }
            public ImmutableStack<Step> Steps { get; }
            public IReadOnlyList<char> Keys { get; }
            public bool KeysPickedUp { get; }

            public override string ToString() => $"{Distance}, keys={string.Join("", Keys)}";

            public bool IsShorterThan(Path other) => Distance < (other?.Distance ?? int.MaxValue);

            public Path Go(Exit throughExit, Map map) => Go(throughExit, map.AreaById(throughExit.ToAreaId));

            public Path Go(Exit throughExit, Area toArea)
            {
                var nextSteps = Steps.Push(new Step(from: CurrentArea, exit: throughExit));

                // +1 accounts for going throuh exit
                var nextDistance = Distance + 1 + 
                    CurrentArea.Distances.Between(CurrentPosition, throughExit.FromPosition);

                var nextKeys = Keys.Concat(toArea.ContainsKeys).Distinct().ToList();
                var keysPickedUp = nextKeys.Count > Keys.Count;

                return new Path(toArea, throughExit.ToPosition, nextDistance, nextSteps, nextKeys, keysPickedUp);
            }
        }

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
                var lookup = new Dictionary<AreaExitPair, IReadOnlyList<KeyRequirements>>();

                var allPairs = map.Areas().SelectMany(area => area.Exits.Select(exit => new AreaExitPair(area, exit)));
                foreach (var pair in allPairs)
                {
                    SearchKeys(map, pair, lookup);
                }

                return new KeyRequirementsLookup(lookup);
            }

            private static IReadOnlyList<KeyRequirements> SearchKeys(
                Map map, 
                AreaExitPair from, 
                Dictionary<AreaExitPair, IReadOnlyList<KeyRequirements>> lookup)
            {
                if (lookup.TryGetValue(from, out var existingRequirements))
                {
                    return existingRequirements;
                }

                var area = map.AreaById(from.Exit.ToAreaId);

                var areaRequirements = area.ContainsKeys
                    .Select(key => new KeyRequirements(key, area.RequiresKeys));

                var childRequirements = area.Exits.Where(e => e.ToAreaId != from.Area.Id)
                    .Select(exit => new AreaExitPair(area, exit))
                    .SelectMany(p => SearchKeys(map, p, lookup))
                    .Select(r => new KeyRequirements(r.Key, r.RequiresKeys.Concat(area.RequiresKeys).ToList()));

                var requirements = areaRequirements.Concat(childRequirements).ToList();
                lookup[from] = requirements;
                return requirements;
            }

            private readonly IReadOnlyDictionary<AreaExitPair, IReadOnlyList<KeyRequirements>> lookup;

            public KeyRequirementsLookup(
                IReadOnlyDictionary<AreaExitPair, IReadOnlyList<KeyRequirements>> lookup)
            {
                this.lookup = lookup;
            }

            public IReadOnlyList<KeyRequirements> Keys(Area fromArea, Exit throughExit) => 
                lookup[new AreaExitPair(fromArea, throughExit)];
        }

        private class AreaExitPair
        {
            public AreaExitPair(Area area, Exit exit)
            {
                this.Area = area;
                this.Exit = exit;
            }

            public Area Area { get; }
            public Exit Exit { get; }

            public override bool Equals(object obj) =>
                obj is AreaExitPair other ? Equals(other) : false;
            
            private bool Equals(AreaExitPair other) =>
                Area.Id == other.Area.Id && Exit.ToAreaId == other.Exit.ToAreaId;

            public override int GetHashCode() => HashCode.Combine(Area.Id, Exit.ToAreaId);
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
    }
}
