using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day15
    {
        public static class Inputs
        {
            public static readonly IInput Sample1 =
                Input.Literal(
                    "#######",   
                    "#.G...#",
                    "#...EG#",
                    "#.#.#G#",
                    "#..G#E#",
                    "#.....#",   
                    "#######"
                );

            public static readonly IInput Sample2 =
                Input.Literal(
                    "#######",
                    "#G..#E#",
                    "#E#E.E#",
                    "#G.##.#",
                    "#...#E#",
                    "#...E.#",
                    "#######"
                );

            public static readonly IInput Sample3 =
                Input.Literal(
                    "#######",
                    "#E..EG#",
                    "#.#G.E#",
                    "#E.##E#",
                    "#G..#.#",
                    "#..E#.#",
                    "#######"
                );

            public static readonly IInput Sample4 =
                Input.Literal(
                    "#######",
                    "#E.G#.#",
                    "#.#G..#",
                    "#G.#.G#",
                    "#G..#.#",
                    "#...E.#",
                    "#######"
                );

            public static readonly IInput Sample5 =
                Input.Literal(
                    "#######",
                    "#.E...#",
                    "#.#..G#",
                    "#.###.#",
                    "#E#G#G#",
                    "#...#G#",
                    "#######"
                );

            public static readonly IInput Sample6 =
                 Input.Literal(
                    "#########",
                    "#G......#",
                    "#.E.#...#",
                    "#..##..G#",
                    "#...##..#",
                    "#...#...#",
                    "#.G...G.#",
                    "#.....G.#",
                    "#########"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/15/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var (grid, units) = Grid.Parse(input.Lines());

                var (winners, round) = units.SimulateBattle(grid);
                var totalHp = units.TotalHitPoints();

                units.Draw(grid);

                var winnerType = winners == UnitType.Elf ? "Elfs" : "Goblins";

                Console.WriteLine($"Winners: {winnerType}");
                Console.WriteLine($"Rounds: {round}");
                Console.WriteLine($"Total hit points: {totalHp}");
                Console.WriteLine($"Outcome: {round * totalHp}");
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var (grid, initialUnits) = Grid.Parse(input.Lines());

                var elfCount = initialUnits.RemainingUnits(UnitType.Elf);
                var elfAttackPower = 4;

                while (true)
                {
                    var units = initialUnits.Clone(elfAttackPower);
                    var (winners, round) = units.SimulateBattle(grid);

                    var elfsWinWithoutLoses = 
                        winners == UnitType.Elf && 
                        units.RemainingUnits(UnitType.Elf) == elfCount;

                    if (elfsWinWithoutLoses)
                    {
                        units.Draw(grid);

                        var totalHp = units.TotalHitPoints();
                        Console.WriteLine($"Elf attack power: {elfAttackPower}");
                        Console.WriteLine($"Rounds: {round}");
                        Console.WriteLine($"Total hit points: {totalHp}");
                        Console.WriteLine($"Outcome: {round * totalHp}");

                        break;
                    }

                    elfAttackPower++;
                }
            }
        }

        private record Point(int Row, int Col)
        {
            public IEnumerable<Point> Adjacent()
            {
                yield return Up();
                yield return Right();
                yield return Down();
                yield return Left();
            }

            public Point Left() => new Point(Row, Col - 1);
            public Point Right() => new Point(Row, Col + 1);
            public Point Up() => new Point(Row - 1, Col);
            public Point Down() => new Point(Row + 1, Col);
        }

        private class ReadOrderComparer : IComparer<Point>
        {
            public static readonly IComparer<Point> Instance = new ReadOrderComparer();

            private ReadOrderComparer() { }

            public int Compare(Point x, Point y)
            {
                var crow = x.Row.CompareTo(y.Row);
                return crow == 0 ? x.Col.CompareTo(y.Col) : crow;
            }
        }

        private enum Cell { Empty, Wall }

        private enum UnitType { Goblin, Elf }

        private record Unit(UnitType Type, Point Position, int HitPoints, int AttackPower)
        {
            public const int InitialHitPoints = 200;
            public const int DefaultAttackPower = 3;

            public static Unit Goblin(Point position) =>
                new Unit(UnitType.Goblin, position, InitialHitPoints, DefaultAttackPower);

            public static Unit Elf(Point position) =>
                new Unit(UnitType.Elf, position, InitialHitPoints, DefaultAttackPower);

            public bool IsAlive() => this.HitPoints > 0;

            public Unit Damage(int by) => this with { HitPoints = HitPoints - by };

            public Unit Move(Point newPosition) => this with { Position = newPosition };

            public Unit RaiseAttackPower(int newAttackPower) => this with { AttackPower = newAttackPower };
        }

        private class Grid
        {
            public static (Grid, UnitList) Parse(IEnumerable<string> lines)
            {
                var grid = new List<IReadOnlyList<Cell>>();
                var units = new List<Unit>();

                var row = 0;
                foreach (var line in lines)
                {
                    var cells = new List<Cell>();
                    grid.Add(cells);

                    var col = 0;
                    foreach (var ch in line)
                    {
                        if (ch == 'G')
                        {
                            units.Add(Unit.Goblin(new Point(row, col)));
                            cells.Add(Cell.Empty);
                        }
                        else if (ch == 'E')
                        {
                            units.Add(Unit.Elf(new Point(row, col)));
                            cells.Add(Cell.Empty);
                        }
                        else if (ch == '.')
                        {
                            cells.Add(Cell.Empty);
                        }
                        else
                        {
                            cells.Add(Cell.Wall);
                        }

                        col++;
                    }

                    row++;
                }

                return (new Grid(grid), new UnitList(units));
            }

            private readonly IReadOnlyList<IReadOnlyList<Cell>> grid;

            public Grid(IReadOnlyList<IReadOnlyList<Cell>> grid)
            {
                this.grid = grid;
            }

            public int Rows => grid.Count;
            public int Cols => grid[0].Count;

            public bool InBounds(Point pos) =>
                0 <= pos.Row && pos.Row < Rows &&
                0 <= pos.Col && pos.Col < Cols;

            public bool IsEmpty(Point pos) =>
                InBounds(pos) && grid[pos.Row][pos.Col] == Cell.Empty;

            public IReadOnlyList<Path> ShortestPathsFrom(
                Point start,
                ISet<Point> occupied,
                IReadOnlyList<Point> ends)
            {
                var toVisit = new PriorityQueue<Point, int>();
                toVisit.Enqueue(start, 0);

                var visited = new Dictionary<Point, Visit>();
                visited.Add(start, new Visit());

                while (toVisit.Count > 0)
                {
                    var pos = toVisit.Dequeue();
                    var nextDistance = visited[pos].Distance + 1;

                    foreach (var adjacent in pos.Adjacent())
                    {
                        if (!IsEmpty(adjacent) || occupied.Contains(adjacent))
                        {
                            continue;
                        }

                        if (visited.TryGetValue(adjacent, out var visit))
                        {
                            if (visit.Distance < nextDistance)
                            {
                                continue;
                            }
                            else if (visit.Distance == nextDistance)
                            {
                                visit.Previous.Add(pos);
                                continue;
                            }
                        }

                        visited[adjacent] = new Visit(nextDistance, pos);
                        toVisit.Enqueue(adjacent, nextDistance);
                    }
                }

                return ends
                    .Select(end => PathBetween(start, end, visited))
                    .Where(p => p != null)
                    .ToList();
            }

            private static Path PathBetween(
                Point start,
                Point end,
                IReadOnlyDictionary<Point, Visit> visited)
            {
                if (!visited.TryGetValue(end, out var visit))
                {
                    return null;
                }

                return new Path(start, end, visit.Distance, visited);
            }

            public class Visit
            {
                public Visit()
                {
                    this.Distance = 0;
                    this.Previous = new List<Point>();
                }

                public Visit(int distance, Point previous)
                {
                    this.Distance = distance;
                    this.Previous = new List<Point>() { previous };
                }

                public int Distance { get; }
                public List<Point> Previous { get; }
            }

            public class Path 
            {
                private readonly IReadOnlyDictionary<Point, Visit> visited;

                public Path(Point start, Point end, int distance, IReadOnlyDictionary<Point, Visit> visited)
                {
                    Start = start;
                    End = end;
                    Distance = distance;
                    this.visited = visited;
                }

                public Point Start { get; }
                public Point End { get; }
                public int Distance { get; }

                public IReadOnlyList<Step> FirstSteps()
                {
                    var finished = new List<Step>();

                    var wip = new Queue<Step>();
                    wip.Enqueue(new Step(this.End, Next: null));

                    while (wip.Count > 0)
                    {
                        var step = wip.Dequeue();

                        if (step.Point.Equals(this.Start))
                        {
                            finished.Add(step);
                            continue;
                        }

                        var visit = this.visited[step.Point];
                        foreach (var point in visit.Previous)
                        {
                            wip.Enqueue(new Step(point, step));
                        }
                    }

                    return finished;
                }
            }

            public record Step(Point Point, Step Next);
        }

        private class UnitList
        {
            private List<Unit> units;
            private readonly HashSet<Point> positions;

            public UnitList(IEnumerable<Unit> units)
            {
                this.units = SortByMoveOrder(units);

                this.positions = units
                    .Where(static u => u.IsAlive())
                    .Select(static u => u.Position)
                    .ToHashSet();
            }

            private static List<Unit> SortByMoveOrder(IEnumerable<Unit> units) =>
                units
                    .OrderBy(static u => u.Position, ReadOrderComparer.Instance)
                    .ToList();
            
            public UnitList Clone(int elfAttackPower)
            {
                var newUnits = this.units
                    .Select(u => u.Type == UnitType.Elf ? u.RaiseAttackPower(elfAttackPower) : u)
                    .ToList();

                return new UnitList(newUnits);
            }

            public int TotalHitPoints() => 
                this.units
                    .Where(u => u.IsAlive())
                    .Select(u => u.HitPoints)
                    .Sum();

            public int RemainingUnits(UnitType type) =>
                this.units
                    .Where(u => u.IsAlive() && u.Type == type)
                    .Count();

            private Unit Move(int unitIndex, Point newPosition)
            {
                var unit = this.units[unitIndex];
                this.positions.Remove(unit.Position);

                var newUnit = this.units[unitIndex].Move(newPosition);
                
                this.units[unitIndex] = newUnit;
                this.positions.Add(newUnit.Position);

                return newUnit;
            }

            private Unit Damage(Unit unit, int damage)
            {
                var unitIndex = this.units.IndexOf(unit);

                var newUnit = unit.Damage(damage);
                
                this.units[unitIndex] = newUnit;

                if (!newUnit.IsAlive())
                {
                    this.positions.Remove(unit.Position);
                }

                return newUnit;
            }

            private bool IsEmptyCell(Grid grid, Point pos) =>
                grid.IsEmpty(pos) && !this.positions.Contains(pos);

            public (UnitType winners, int round) SimulateBattle(Grid grid)
            {
                var round = 0;
                UnitType winners;

                while (true)
                {
                    var result = SimulateRound(grid);
                    if (result == RoundResult.ElfsWin)
                    {
                        winners = UnitType.Elf;
                        break;
                    }
                    if (result == RoundResult.GoblinsWin)
                    {
                        winners = UnitType.Goblin;
                        break;
                    }

                    round++;
                }

                return (winners, round);
            }

            private RoundResult SimulateRound(Grid grid)
            {
                static IReadOnlyList<Unit> TargetsInRange(Unit currentUnit, IReadOnlyList<Unit> targets) =>
                    targets
                        .Where(t => currentUnit.Position.Adjacent().Contains(t.Position))
                        .ToList();

                var i = 0;
                while (i < this.units.Count)
                {
                    var currentUnit = this.units[i];
                    if (!currentUnit.IsAlive())
                    {
                        i++;
                        continue;
                    }

                    var aliveTargets = this.units
                        .Where(u => u.IsAlive() && u.Type != currentUnit.Type)
                        .ToList();

                    if (aliveTargets.Count == 0)
                    {
                        return currentUnit.Type == UnitType.Elf ? RoundResult.ElfsWin : RoundResult.GoblinsWin;
                    }

                    // move
                    var targetsInRange = TargetsInRange(currentUnit, aliveTargets);
                    if (targetsInRange.Count == 0)
                    {
                        var destinations = aliveTargets
                            .SelectMany(static t => t.Position.Adjacent())
                            .Distinct()
                            .Where(p => IsEmptyCell(grid, p))
                            .ToList();

                        if (TryFindPositionToMove(grid, currentUnit, destinations, out var destination))
                        {
                            currentUnit = Move(i, destination);
                            targetsInRange = TargetsInRange(currentUnit, aliveTargets);
                        }
                    }

                    // attack
                    if (targetsInRange.Count > 0)
                    {
                        var targetToAttack = targetsInRange
                            .OrderBy(static t => t.HitPoints)
                            .ThenBy(static t => t.Position, ReadOrderComparer.Instance)
                            .First();

                        Damage(targetToAttack, currentUnit.AttackPower);
                    }

                    i++;
                }

                this.units = SortByMoveOrder(units);

                return RoundResult.BattleContinues;
            }

            private enum RoundResult { BattleContinues, ElfsWin, GoblinsWin }

            private bool TryFindPositionToMove(Grid grid, Unit unit, IReadOnlyList<Point> candidates, out Point position)
            {
                if (candidates.Count == 0)
                {
                    position = default;
                    return false;
                }

                var paths = grid.ShortestPathsFrom(unit.Position, this.positions, candidates);

                if (paths.Count == 0)
                {
                    position = default;
                    return false;
                }

                var minDistance = paths.Min(p => p.Distance);

                var path = paths
                    .Where(p => p.Distance == minDistance)
                    .OrderBy(p => p.End, ReadOrderComparer.Instance)
                    .First();

                position = path.FirstSteps()
                    .Select(s => s.Next.Point)
                    .OrderBy(p => p, ReadOrderComparer.Instance)
                    .First();
                return true;
            }

            public void Draw(Grid grid)
            {
                static char UnitChar(Unit unit) => unit.Type == UnitType.Elf ? 'E' : 'G';

                static char CellChar(bool isEmptyCell) => isEmptyCell ? '.' : '#';

                static char CharOf(Unit unit, bool isEmptyCell) => unit != null ? UnitChar(unit) : CellChar(isEmptyCell);

                static string UnitToString(Unit unit) => $"{UnitChar(unit)}({unit.HitPoints})";

                static string UnitsToString(IEnumerable<Unit> units) => string.Join(", ", units.Select(UnitToString));

                for (var row = 0; row < grid.Rows; row++)
                {
                    var unitsInRow = new List<Unit>();

                    for (var col = 0; col < grid.Cols; col++)
                    {
                        var pos = new Point(row, col);
                        var unit = this.units.FirstOrDefault(u => u.IsAlive() && u.Position.Equals(pos));
                        var isEmptyCell = grid.IsEmpty(pos);

                        Console.Write(CharOf(unit, isEmptyCell));

                        if (unit != null)
                        {
                            unitsInRow.Add(unit);
                        }
                    }

                    if (unitsInRow.Count > 0)
                    {
                        Console.Write($"   {UnitsToString(unitsInRow)}");
                    }

                    Console.WriteLine();
                }

                Console.WriteLine();
            }
        }
    }
}
