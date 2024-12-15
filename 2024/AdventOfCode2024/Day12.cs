using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2024;

static class Day12
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
RRRRIICCFF
RRRRIICCCF
VVRRRCCFFF
VVRCCCJFFF
VVVVCJJCFE
VVIVCCJJEE
VVIIICJJEE
MIIIIIJJEE
MIIISIJEEE
MMMISSJEEE
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/12/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var regions = DiscoverRegions(grid);

            var sum = regions
                .Select(r => (long)r.Area() * Perimeter(r, grid))
                .Sum();

            Console.WriteLine(sum);
        }

        private static int Perimeter(Region region, Grid grid)
        {
            return region.Positions
                .Select(pos => pos
                    .Neighbours()
                    .Where(nb => !grid.Contains(nb) || grid.At(nb) != region.Kind)
                    .Count()
                )
                .Sum();
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var regions = DiscoverRegions(grid);

            var sum = regions
                .Select(r => (long)r.Area() * CountSides(r, grid))
                .Sum();

            Console.WriteLine(sum);
        }

        private static int CountSides(Region region, Grid grid)
        {
            bool IsInEdge(Position pos, Direction edgeDir)
            {
                var nb = pos.Add(DirectionUtil.ToPositionOffset(edgeDir));
                return !grid.Contains(nb) || grid.At(nb) != region.Kind;
            }

            bool IsOutEdge(Position pos, Direction outDir, Direction edgeDir)
            {
                var outPos = pos.Add(DirectionUtil.ToPositionOffset(outDir));
                if (!region.Positions.Contains(outPos))
                {
                    return false;
                }

                return IsInEdge(outPos, edgeDir);
            }

            int CountCorners(Position pos)
            {
                static int Int(bool f) => f ? 1 : 0;

                var l = IsInEdge(pos, Direction.Left);
                var r = IsInEdge(pos, Direction.Right);
                var u = IsInEdge(pos, Direction.Up);
                var d = IsInEdge(pos, Direction.Down);

                var ld = IsOutEdge(pos, Direction.Left, Direction.Down);
                var lu = IsOutEdge(pos, Direction.Left, Direction.Up);
                var rd = IsOutEdge(pos, Direction.Right, Direction.Down);
                var ru = IsOutEdge(pos, Direction.Right, Direction.Up);
                var dl = IsOutEdge(pos, Direction.Down, Direction.Left);
                var dr = IsOutEdge(pos, Direction.Down, Direction.Right);
                var ul = IsOutEdge(pos, Direction.Up, Direction.Left);
                var ur = IsOutEdge(pos, Direction.Up, Direction.Right);

                return Int(l && u)
                     + Int(u && r)
                     + Int(r && d)
                     + Int(d && l)
                     + Int(ld && dl)
                     + Int(rd && dr)
                     + Int(lu && ul)
                     + Int(ru && ur);
            }

            var sides = region.Positions
                .Select(CountCorners)
                .Sum();

            return sides;
        }
    }

    private static IReadOnlyList<Region> DiscoverRegions(Grid grid)
    {
        var regions = new List<Region>();
        var seen = new HashSet<Position>();

        while (true)
        {
            var start = grid.AllPositions()
                .Where(pos => !seen.Contains(pos))
                .FirstOrDefault();

            if (start is null)
            {
                break;
            }

            var region = DiscoverRegion(grid, start);
            regions.Add(region);

            seen.AddRange(region.Positions);
        }

        return regions;
    }

    private static Region DiscoverRegion(Grid grid, Position start)
    {
        var kind = grid.At(start);

        var visit = new Queue<Position>();
        visit.Enqueue(start);

        var seen = new HashSet<Position>();
        seen.Add(start);

        while (visit.Count > 0)
        {
            var pos = visit.Dequeue();

            foreach (var next in pos.Neighbours())
            {
                if (!grid.Contains(next))
                {
                    continue;
                }

                if (grid.At(next) != kind)
                {
                    continue;
                }

                if (seen.Contains(next))
                {
                    continue;
                }

                visit.Enqueue(next);
                seen.Add(next);
            }
        }

        return new Region(kind, seen);
    }

    private record Region(
        char Kind,
        ISet<Position> Positions)
    {
        public int Area() => Positions.Count;
    }

    private record Position(int Row, int Col)
    {
        public Position Add(Position p) =>
            new(p.Row + this.Row, p.Col + this.Col);

        public IEnumerable<Position> Neighbours() =>
            DirectionUtil.All
                .Select(DirectionUtil.ToPositionOffset)
                .Select(offset => this.Add(offset));
    }

    private enum Direction { Up, Down, Left, Right }

    private static class DirectionUtil
    {
        public static readonly IReadOnlyList<Direction> All =
            new[]
            {
                Direction.Up,
                Direction.Left,
                Direction.Down,
                Direction.Right
            };

        public static Position ToPositionOffset(Direction dir) =>
            dir switch
            {
                Direction.Up => new Position(Row: -1, Col: 0),
                Direction.Right => new Position(Row: 0, Col: 1),
                Direction.Down => new Position(Row: 1, Col: 0),
                Direction.Left => new Position(Row: 0, Col: -1),

                _ => throw new Exception("impossible")
            };
    }

    private class Grid
    {
        public static Grid Parse(IEnumerable<string> lines) =>
            new Grid(lines.ToArray());

        private readonly IReadOnlyList<string> cells;

        public Grid(IReadOnlyList<string> cells)
        {
            this.cells = cells;
        }

        public int Rows => this.cells.Count;
        public int Cols => this.cells[0].Length;

        public bool Contains(Position p) =>
            0 <= p.Row && p.Row < Rows &&
            0 <= p.Col && p.Col < Cols;

        public char At(Position p) =>
            this.cells[p.Row][p.Col];

        public IEnumerable<Position> AllPositions()
        {
            for (var row = 0; row < this.Rows; row++)
            {
                for (var col = 0; col < this.Cols; col++)
                {
                    yield return new Position(row, col);
                }
            }
        }
    }
}
