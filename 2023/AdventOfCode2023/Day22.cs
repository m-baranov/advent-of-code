using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day22
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
1,0,1~1,2,1
0,0,2~2,0,2
0,2,3~2,2,3
0,0,4~0,2,4
2,0,5~2,2,5
0,1,6~2,1,6
1,1,8~1,1,9
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/22/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var bricks = Brick.ParseMany(input.Lines());

            var space = Space.CreateLargeEnough(bricks);
            var supports = space.DropMany(bricks);

            var count = supports
                .Select(s => s.BrickId)
                .Where(brickId =>
                {
                    var bricksAbove = supports
                        .Where(s => s.SupportedByBrickIds.Contains(brickId))
                        .ToList();

                    return bricksAbove.Count == 0
                        || bricksAbove.All(s => s.SupportedByBrickIds.Count > 1);
                })
                .Count();

            Console.WriteLine(count);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var bricks = Brick.ParseMany(input.Lines());

            var space = Space.CreateLargeEnough(bricks);
            var supports = space.DropMany(bricks);
            
            var aboveByBrickId = CreateAboveLookup(supports);
            var sum = supports
                .Select(s => Count(s, supports, aboveByBrickId))
                .Sum();

            Console.WriteLine(sum);
        }

        private static IReadOnlyDictionary<int, IReadOnlyList<SupportRelation>> CreateAboveLookup(
            IReadOnlyList<SupportRelation> supports)
        {
            return supports
                .Select(brick =>
                {
                    var above = supports
                        .Where(s => s.SupportedByBrickIds.Contains(brick.BrickId))
                        .ToList();

                    return (
                        brickId: brick.BrickId, 
                        above: (IReadOnlyList<SupportRelation>)above
                    );
                })
                .ToDictionary(p => p.brickId, p => p.above);
        }

        private static int Count(
            SupportRelation brick,
            IReadOnlyList<SupportRelation> supports,
            IReadOnlyDictionary<int, IReadOnlyList<SupportRelation>> aboveByBrickId)
        {
            static void Wipe(
                SupportRelation brick,
                IReadOnlyList<SupportRelation> supports,
                IReadOnlyDictionary<int, IReadOnlyList<SupportRelation>> aboveByBrickId,
                HashSet<int> removedBrickIds)
            {
                var supportedByAny = brick.SupportedByBrickIds
                    .Where(id => !removedBrickIds.Contains(id))
                    .Any();

                if (!supportedByAny)
                {
                    removedBrickIds.Add(brick.BrickId);
                }

                foreach (var above in aboveByBrickId[brick.BrickId])
                {
                    Wipe(above, supports, aboveByBrickId, removedBrickIds);
                }
            }

            var removedBrickIds = new HashSet<int>() { brick.BrickId };
            Wipe(brick, supports, aboveByBrickId, removedBrickIds);

            return removedBrickIds.Count - 1;
        }
    }

    private record Point(int X, int Y, int Z)
    {
        public static Point Parse(string text)
        {
            var parts = text.Split(',');

            var x = int.Parse(parts[0]);
            var y = int.Parse(parts[1]);
            var z = int.Parse(parts[2]);

            return new Point(x, y, z);
        }
    }

    private record Brick(int Id, Point Start, Point End)
    {
        public static IReadOnlyList<Brick> ParseMany(IEnumerable<string> lines) =>
            lines.Select((line, index) => Parse(line, index)).ToList();

        public static Brick Parse(string text, int id)
        {
            var parts = text.Split('~');

            var start = Point.Parse(parts[0]);
            var end = Point.Parse(parts[1]);

            return new Brick(id, start, end);
        }
    }

    private record SupportRelation(int BrickId, IReadOnlyList<int> SupportedByBrickIds);

    private sealed class Space
    {
        public static Space CreateLargeEnough(IReadOnlyList<Brick> bricks)
        {
            var (minX, maxX) = MinMax(bricks.SelectMany(b => new[] { b.Start.X, b.End.X }));
            var (minY, maxY) = MinMax(bricks.SelectMany(b => new[] { b.Start.Y, b.End.Y }));

            return new Space(minX, maxX, minY, maxY);
        }

        private readonly Cell[,] cells;
        private readonly int minX;
        private readonly int minY;

        public Space(int minX, int maxX, int minY, int maxY)
        {
            this.minX = minX;
            this.minY = minY;

            var cols = maxX - minX + 1;
            var rows = maxY - minY + 1;

            this.cells = new Cell[rows, cols];

            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    this.cells[row, col] = new Cell(Z: 0, BrickId: null);
                }
            }
        }

        public IReadOnlyList<SupportRelation> DropMany(IReadOnlyList<Brick> bricks)
        {
            var orderedBricks = bricks.OrderBy(b => Math.Min(b.Start.Z, b.End.Z));

            var supports = new List<SupportRelation>();

            foreach (var brick in orderedBricks)
            {
                supports.Add(Drop(brick));
            }

            return supports;
        }

        private SupportRelation Drop(Brick brick)
        {
            var (minX, maxX) = MinMax(brick.Start.X, brick.End.X);
            var (minY, maxY) = MinMax(brick.Start.Y, brick.End.Y);
            var (minZ, maxZ) = MinMax(brick.Start.Z, brick.End.Z);

            var z = 0;
            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var (row, col) = Convert(x, y);
                    z = Math.Max(z, this.cells[row, col].Z);
                }
            }

            var h = maxZ - minZ + 1;

            var supporingBrickIds = new HashSet<int>();

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var (row, col) = Convert(x, y);
                    
                    var cell = this.cells[row, col];
                    if (cell.Z == z && cell.BrickId is not null)
                    {
                        supporingBrickIds.Add(cell.BrickId.Value);
                    }

                    this.cells[row, col] = new Cell(z + h, brick.Id);
                }
            }

            return new SupportRelation(brick.Id, supporingBrickIds.ToList());
        }

        private (int row, int col) Convert(int x, int y) =>
            (x + this.minX, y + this.minY);

        private record Cell(int Z, int? BrickId);
    }

    private static (int min, int max) MinMax(IEnumerable<int> vs) =>
        vs.Aggregate(
            (min: int.MaxValue, max: int.MinValue),
            (acc, v) => (min: Math.Min(acc.min, v), max: Math.Max(acc.max, v))
        );

    private static (int min, int max) MinMax(int a, int b) => 
        (Math.Min(a, b), Math.Max(a, b));
}
