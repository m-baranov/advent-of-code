using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day22
    {
        public static class Inputs
        {
            #region Inputs

            public static readonly IInput Sample0 =
                Input.Literal(
                    "on x=-5..47,y=-31..22,z=-19..33",
                    "on x=-44..5,y=-27..21,z=-14..35",
                    "on x=-49..-1,y=-11..42,z=-10..38",
                    "on x=-20..34,y=-40..6,z=-44..1",
                    "off x=26..39,y=40..50,z=-2..11",
                    "on x=-41..5,y=-41..6,z=-36..8",
                    "off x=-43..-33,y=-45..-28,z=7..25",
                    "on x=-33..15,y=-32..19,z=-34..11",
                    "off x=35..47,y=-46..-34,z=-11..5",
                    "on x=-14..36,y=-6..44,z=-16..29"
                );

            public static readonly IInput Sample1 =
                Input.Literal(
                    "on x=-20..26,y=-36..17,z=-47..7",
                    "on x=-20..33,y=-21..23,z=-26..28",
                    "on x=-22..28,y=-29..23,z=-38..16",
                    "on x=-46..7,y=-6..46,z=-50..-1",
                    "on x=-49..1,y=-3..46,z=-24..28",
                    "on x=2..47,y=-22..22,z=-23..27",
                    "on x=-27..23,y=-28..26,z=-21..29",
                    "on x=-39..5,y=-6..47,z=-3..44",
                    "on x=-30..21,y=-8..43,z=-13..34",
                    "on x=-22..26,y=-27..20,z=-29..19",
                    "off x=-48..-32,y=26..41,z=-47..-37",
                    "on x=-12..35,y=6..50,z=-50..-2",
                    "off x=-48..-32,y=-32..-16,z=-15..-5",
                    "on x=-18..26,y=-33..15,z=-7..46",
                    "off x=-40..-22,y=-38..-28,z=23..41",
                    "on x=-16..35,y=-41..10,z=-47..6",
                    "off x=-32..-23,y=11..30,z=-14..3",
                    "on x=-49..-5,y=-3..45,z=-29..18",
                    "off x=18..30,y=-20..-8,z=-3..13",
                    "on x=-41..9,y=-7..43,z=-33..15",
                    "on x=-54112..-39298,y=-85059..-49293,z=-27449..7877",
                    "on x=967..23432,y=45373..81175,z=27513..53682"
                );

            public static readonly IInput Sample2 =
                Input.Literal(new[] {
                    "on x=-5..47,y=-31..22,z=-19..33",
                    "on x=-44..5,y=-27..21,z=-14..35",
                    "on x=-49..-1,y=-11..42,z=-10..38",
                    "on x=-20..34,y=-40..6,z=-44..1",
                    "off x=26..39,y=40..50,z=-2..11",
                    "on x=-41..5,y=-41..6,z=-36..8",
                    "off x=-43..-33,y=-45..-28,z=7..25",
                    "on x=-33..15,y=-32..19,z=-34..11",
                    "off x=35..47,y=-46..-34,z=-11..5",
                    "on x=-14..36,y=-6..44,z=-16..29",
                    "on x=-57795..-6158,y=29564..72030,z=20435..90618",
                    "on x=36731..105352,y=-21140..28532,z=16094..90401",
                    "on x=30999..107136,y=-53464..15513,z=8553..71215",
                    "on x=13528..83982,y=-99403..-27377,z=-24141..23996",
                    "on x=-72682..-12347,y=18159..111354,z=7391..80950",
                    "on x=-1060..80757,y=-65301..-20884,z=-103788..-16709",
                    "on x=-83015..-9461,y=-72160..-8347,z=-81239..-26856",
                    "on x=-52752..22273,y=-49450..9096,z=54442..119054",
                    "on x=-29982..40483,y=-108474..-28371,z=-24328..38471",
                    "on x=-4958..62750,y=40422..118853,z=-7672..65583",
                    "on x=55694..108686,y=-43367..46958,z=-26781..48729",
                    "on x=-98497..-18186,y=-63569..3412,z=1232..88485",
                    "on x=-726..56291,y=-62629..13224,z=18033..85226",
                    "on x=-110886..-34664,y=-81338..-8658,z=8914..63723",
                    "on x=-55829..24974,y=-16897..54165,z=-121762..-28058",
                    "on x=-65152..-11147,y=22489..91432,z=-58782..1780",
                    "on x=-120100..-32970,y=-46592..27473,z=-11695..61039",
                    "on x=-18631..37533,y=-124565..-50804,z=-35667..28308",
                    "on x=-57817..18248,y=49321..117703,z=5745..55881",
                    "on x=14781..98692,y=-1341..70827,z=15753..70151",
                    "on x=-34419..55919,y=-19626..40991,z=39015..114138",
                    "on x=-60785..11593,y=-56135..2999,z=-95368..-26915",
                    "on x=-32178..58085,y=17647..101866,z=-91405..-8878",
                    "on x=-53655..12091,y=50097..105568,z=-75335..-4862",
                    "on x=-111166..-40997,y=-71714..2688,z=5609..50954",
                    "on x=-16602..70118,y=-98693..-44401,z=5197..76897",
                    "on x=16383..101554,y=4615..83635,z=-44907..18747",
                    "off x=-95822..-15171,y=-19987..48940,z=10804..104439",
                    "on x=-89813..-14614,y=16069..88491,z=-3297..45228",
                    "on x=41075..99376,y=-20427..49978,z=-52012..13762",
                    "on x=-21330..50085,y=-17944..62733,z=-112280..-30197",
                    "on x=-16478..35915,y=36008..118594,z=-7885..47086",
                    "off x=-98156..-27851,y=-49952..43171,z=-99005..-8456",
                    "off x=2032..69770,y=-71013..4824,z=7471..94418",
                    "on x=43670..120875,y=-42068..12382,z=-24787..38892",
                    "off x=37514..111226,y=-45862..25743,z=-16714..54663",
                    "off x=25699..97951,y=-30668..59918,z=-15349..69697",
                    "off x=-44271..17935,y=-9516..60759,z=49131..112598",
                    "on x=-61695..-5813,y=40978..94975,z=8655..80240",
                    "off x=-101086..-9439,y=-7088..67543,z=33935..83858",
                    "off x=18020..114017,y=-48931..32606,z=21474..89843",
                    "off x=-77139..10506,y=-89994..-18797,z=-80..59318",
                    "off x=8476..79288,y=-75520..11602,z=-96624..-24783",
                    "on x=-47488..-1262,y=24338..100707,z=16292..72967",
                    "off x=-84341..13987,y=2429..92914,z=-90671..-1318",
                    "off x=-37810..49457,y=-71013..-7894,z=-105357..-13188",
                    "off x=-27365..46395,y=31009..98017,z=15428..76570",
                    "off x=-70369..-16548,y=22648..78696,z=-1892..86821",
                    "on x=-53470..21291,y=-120233..-33476,z=-44150..38147",
                    "off x=-93533..-4276,y=-16170..68771,z=-104985..-24507"
                });

            #endregion

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/22/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var cube = new Cube(
                    new Range(-50, 50 + 1),
                    new Range(-50, 50 + 1),
                    new Range(-50, 50 + 1)
                );

                var instructions = Instruction.ParseMany(input.Lines());

                var count = PointsOf(cube)
                    .Where(point => IsTurnedOn(instructions, point))
                    .Count();

                Console.WriteLine(count);
            }

            private static IEnumerable<Point> PointsOf(Cube cube) =>
                from x in ValuesOf(cube.X)
                from y in ValuesOf(cube.Y)
                from z in ValuesOf(cube.Z)
                select new Point(x, y, z);

            private static IEnumerable<int> ValuesOf(Range range)
            {
                for (var i = range.Start; i < range.End; i++)
                {
                    yield return i;
                }
            }

            private bool IsTurnedOn(IReadOnlyList<Instruction> instructions, Point point)
            {
                var last = instructions.Where(i => CubeContains(i.Cube, point)).LastOrDefault();
                return last?.TurnOn ?? false;
            }

            private static bool CubeContains(Cube cube, Point point) =>
                RangeContains(cube.X, point.X) &&
                RangeContains(cube.Y, point.Y) &&
                RangeContains(cube.Z, point.Z);

            private static bool RangeContains(Range range, int value) =>
                range.Start <= value && value < range.End;

            private class Point
            {
                public Point(int x, int y, int z)
                {
                    X = x;
                    Y = y;
                    Z = z;
                }

                public int X { get; }
                public int Y { get; }
                public int Z { get; }
            }
        }

        // Relatively simple.
        // Consumes a lot of memory.
        // Takes too much time on test input of Part 2.
        public class Part2Slow : IProblem
        {
            public void Run(TextReader input)
            {
                var instructions = Instruction.ParseMany(input.Lines());

                var space = instructions.Aggregate(Space.Off, (s, i) => s.Set(i.Cube, i.TurnOn));

                //var space =  Space.Off;
                //foreach (var instr in instructions)
                //{
                //    space = space.Set(instr.Cube, instr.TurnOn);
                //}

                var count = space.CubesOn().Select(c => c.PointCount()).Sum();
                Console.WriteLine(count);
            }

            private class Space
            {
                public static readonly Space Off =
                    new Space(
                        xs: new List<int>(),
                        ys: new List<int>(),
                        zs: new List<int>(),
                        isOn: new bool[0, 0, 0]
                    );

                private Space(
                    List<int> xs,
                    List<int> ys,
                    List<int> zs,
                    bool[,,] isOn)
                {
                    Xs = xs;
                    Ys = ys;
                    Zs = zs;
                    IsOn = isOn;
                }

                public List<int> Xs { get; }
                public List<int> Ys { get; }
                public List<int> Zs { get; }
                public bool[,,] IsOn { get; }

                public IEnumerable<Cube> CubesOn()
                {
                    for (var ix = 0; ix < Xs.Count - 1; ix++)
                    {
                        for (var iy = 0; iy < Ys.Count - 1; iy++)
                        {
                            for (var iz = 0; iz < Zs.Count - 1; iz++)
                            {
                                if (IsOn[ix, iy, iz])
                                {
                                    yield return new Cube(
                                        new Range(Xs[ix], Xs[ix + 1]),
                                        new Range(Ys[iy], Ys[iy + 1]),
                                        new Range(Zs[iz], Zs[iz + 1])
                                    );
                                }
                            }
                        }
                    }
                }

                public Space Set(Cube cube, bool isOn)
                {
                    var (newXs, mappingX) = InsertBounds(Xs, cube.X);
                    var (newYs, mappingY) = InsertBounds(Ys, cube.Y);
                    var (newZs, mappingZ) = InsertBounds(Zs, cube.Z);

                    var newIsOn = new bool[newXs.Count, newYs.Count, newZs.Count];
                    for (var ix = 0; ix < newXs.Count; ix++)
                    {
                        var oldIx = mappingX.OldIndex(ix);
                        for (var iy = 0; iy < newYs.Count; iy++)
                        {
                            var oldIy = mappingY.OldIndex(iy);
                            for (var iz = 0; iz < newZs.Count; iz++)
                            {
                                var oldIz = mappingZ.OldIndex(iz);


                                if (mappingX.IsInserted(ix) && mappingY.IsInserted(iy) && mappingZ.IsInserted(iz))
                                {
                                    if (isOn)
                                    {
                                        newIsOn[ix, iy, iz] = true;
                                    }
                                }
                                else
                                {
                                    if (oldIx >= 0 && oldIy >= 0 && oldIz >= 0 && IsOn[oldIx, oldIy, oldIz])
                                    {
                                        newIsOn[ix, iy, iz] = true;
                                    }
                                }
                            }
                        }
                    }

                    return new Space(newXs, newYs, newZs, newIsOn);
                }

                private (List<int>, Mapping) InsertBounds(List<int> coordinates, Range range)
                {
                    var newCoordinates = coordinates.ToList();

                    var (startInserted, startIndex) = Insert(newCoordinates, range.Start);
                    var (endInserted, endIndex) = Insert(newCoordinates, range.End);

                    var mapping = new Mapping(startInserted, startIndex, endInserted, endIndex);

                    return (newCoordinates, mapping);
                }

                private class Mapping
                {
                    private readonly bool startInserted;
                    private readonly int startIndex;
                    private readonly bool endInserted;
                    private readonly int endIndex;

                    public Mapping(bool startInserted, int startIndex, bool endInserted, int endIndex)
                    {
                        this.startInserted = startInserted;
                        this.startIndex = startIndex;
                        this.endInserted = endInserted;
                        this.endIndex = endIndex;
                    }

                    public bool IsInserted(int newIndex) =>
                        startIndex <= newIndex && newIndex < endIndex;

                    public int OldIndex(int newIndex)
                    {
                        var deltaS = startInserted ? -1 : 0;
                        var deltaE = endInserted ? -1 : 0;

                        if (newIndex < startIndex)
                        {
                            return newIndex;
                        }
                        else if (newIndex < endIndex)
                        {
                            return newIndex + deltaS;
                        }
                        else
                        {
                            return newIndex + deltaS + deltaE;
                        }
                    }
                }

                private (bool, int) Insert(List<int> coordinates, int coordinate)
                {
                    var index = coordinates.BinarySearch(coordinate);
                    if (index < 0)
                    {
                        var insertAt = ~index;
                        coordinates.Insert(insertAt, coordinate);
                        return (true, insertAt);
                    }
                    return (false, index);
                }
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var instructions = Instruction.ParseMany(input.Lines());

                var watch = Stopwatch.StartNew();

                IReadOnlyList<Cube> onCubes = Array.Empty<Cube>();
                foreach (var instr in instructions)
                {
                    if (instr.TurnOn)
                    {
                        onCubes = Cube.Add(onCubes, instr.Cube);
                    }
                    else
                    {
                        onCubes = Cube.Sub(onCubes, instr.Cube);
                    }
                }

                var count = onCubes.Select(c => c.PointCount()).Sum();
                Console.WriteLine(count);

                watch.Stop();
                Console.WriteLine($"{watch.ElapsedMilliseconds} ms");
            }
        }

        private class Range
        {
            public static Range Parse(string text)
            {
                var parts = text.Split("..").Select(int.Parse).Take(2).ToList();
                return new Range(parts[0], parts[1] + 1);
            }

            public static IReadOnlyList<Subrange> Split(Range a, Range b)
            {
                if (a.Start == b.Start && a.End == b.End)
                {
                    return new[] { Subrange.AB(a.Start, a.End) };
                }

                if (b.End <= a.Start || a.End <= b.Start)
                {
                    return Array.Empty<Subrange>();
                }

                if (b.Start < a.Start && b.End == a.End)
                {
                    return new[]
                    {
                        Subrange.B(b.Start, a.Start),
                        Subrange.AB(a.Start, a.End)
                    };
                }

                if (a.Start < b.Start && b.End == a.End)
                {
                    return new[]
                    {
                        Subrange.A(a.Start, b.Start),
                        Subrange.AB(b.Start, b.End)
                    };
                }

                if (b.Start < a.Start && a.End < b.End)
                {
                    return new[]
                    {
                        Subrange.B(b.Start, a.Start),
                        Subrange.AB(a.Start, a.End),
                        Subrange.B(a.End, b.End)
                    };
                }

                if (a.Start < b.Start && b.End < a.End)
                {
                    return new[]
                    {
                        Subrange.A(a.Start, b.Start),
                        Subrange.AB(b.Start, b.End),
                        Subrange.A(b.End, a.End)
                    };
                }

                if (a.End < b.End && a.Start == b.Start)
                {
                    return new[]
                    {
                        Subrange.AB(a.Start, a.End),
                        Subrange.B(a.End, b.End)
                    };
                }

                if (b.End < a.End && a.Start == b.Start)
                {
                    return new[]
                    {
                        Subrange.AB(b.Start, b.End),
                        Subrange.A(b.End, a.End)
                    };
                }

                if (b.Start < a.Start && a.Start < b.End && b.End < a.End)
                {
                    return new[]
                    {
                        Subrange.B(b.Start, a.Start),
                        Subrange.AB(a.Start, b.End),
                        Subrange.A(b.End, a.End)
                    };
                }

                if (a.Start < b.Start && b.Start < a.End && a.End < b.End)
                {
                    return new[]
                    {
                        Subrange.A(a.Start, b.Start),
                        Subrange.AB(b.Start, a.End),
                        Subrange.B(a.End, b.End)
                    };
                }

                return Array.Empty<Subrange>();
            }

            public static bool TryMerge(Range a, Range b, out Range merged)
            {
                if (a.End == b.Start)
                {
                    merged = new Range(a.Start, b.End);
                    return true;
                }
                if (b.End == a.Start)
                {
                    merged = new Range(b.Start, a.End);
                    return true;
                }

                merged = default;
                return false;
            }

            public Range(int start, int end)
            {
                Start = start;
                End = end;
            }

            public int Start { get; }
            public int End { get; }

            public override string ToString() => $"[{Start}..{End - 1}]";

            public bool IntersectsWith(Range other) =>
                !(other.End <= Start || End <= other.Start);

            public long ValueCount() => End - Start;
        }

        private class Subrange
        {
            public static Subrange A(int start, int end) => Of(start, end, hasA: true);
            public static Subrange B(int start, int end) => Of(start, end, hasB: true);
            public static Subrange AB(int start, int end) => Of(start, end, hasA: true, hasB: true);

            public static Subrange Of(int start, int end, bool hasA = false, bool hasB = false) =>
                new Subrange(new Range(start, end), hasA, hasB);

            public Subrange(Range range, bool hasA, bool hasB)
            {
                Range = range;
                HasA = hasA;
                HasB = hasB;
            }

            public Range Range { get; }
            public bool HasA { get; }
            public bool HasB { get; }

            public override string ToString()
            {
                var sideA = HasA ? "A" : string.Empty;
                var sideB = HasB ? "B" : string.Empty;

                return $"{Range}({sideA}{sideB})";
            }
        }

        private class Cube
        {
            public static Cube Parse(string text)
            {
                var ranges = text
                    .Split(',')
                    .Select(t => t.Substring(2)) // skip x=, y= or z=
                    .Select(Range.Parse)
                    .ToList();

                return new Cube(ranges[0], ranges[1], ranges[2]);
            }

            public static IReadOnlyList<Cube> Add(IReadOnlyList<Cube> cubes, Cube newCube)
            {
                var resultCubes = new List<Cube>();
                var intersectingCubes = new List<Cube>();

                foreach (var cube in cubes)
                {
                    if (cube.IntersectsWith(newCube))
                    {
                        intersectingCubes.Add(cube);
                    }
                    else
                    {
                        resultCubes.Add(cube);
                    }
                }

                if (intersectingCubes.Count == 0)
                {
                    resultCubes.Add(newCube);
                    return resultCubes;
                }

                var pendingCubes = new Queue<Cube>();
                pendingCubes.Enqueue(newCube);

                while (pendingCubes.Count > 0)
                {
                    var pendingCube = pendingCubes.Dequeue();

                    var index = 0;
                    IReadOnlyList<Cube> combinedCubes = Array.Empty<Cube>();
                    while (index < intersectingCubes.Count)
                    {
                        var intersectingCube = intersectingCubes[index];
                        if (intersectingCube == null)
                        {
                            index++;
                            continue;
                        }

                        combinedCubes = Add(intersectingCube, pendingCube);
                        if (combinedCubes.Count > 0)
                        {
                            break;
                        }

                        index++;
                    }

                    if (index < intersectingCubes.Count)
                    {
                        intersectingCubes[index] = null;
                        pendingCubes.EnqueueRange(combinedCubes);
                    }
                    else
                    {
                        intersectingCubes.Add(pendingCube);
                    }
                }

                resultCubes.AddRange(intersectingCubes.Where(c => c != null));
                return resultCubes;
            }

            public static IReadOnlyList<Cube> Sub(IReadOnlyList<Cube> cubes, Cube newCube)
            {
                // LINQ is short, but slower than explicit loops below

                //return cubes
                //    .SelectMany(cube => cube.IntersectsWith(newCube) 
                //        ? Sub(cube, newCube) 
                //        : new[] { cube }
                //    )
                //    .ToList();

                var resultCubes = new List<Cube>();
                
                foreach (var cube in cubes)
                {
                    if (cube.IntersectsWith(newCube))
                    {
                        resultCubes.AddRange(Sub(cube, newCube));
                    }
                    else
                    {
                        resultCubes.Add(cube);
                    }
                }

                return resultCubes;
            }

            public static IReadOnlyList<Cube> Add(Cube a, Cube b)
            {
                // LINQ is short, but slower than explicit loops below

                //IEnumerable<Cube> add(Cube a, Cube b) =>
                //    from xs in Range.Split(a.X, b.X)
                //    from ys in Range.Split(a.Y, b.Y)
                //    from zs in Range.Split(a.Z, b.Z)
                //    let subranges = new[] { xs, ys, zs }
                //    where subranges.All(sr => sr.HasA) || subranges.All(sr => sr.HasB)
                //    select new Cube(xs.Range, ys.Range, zs.Range);
                // 
                // return Merge(sub(a, b));

                var result = new List<Cube>();

                foreach (var xs in Range.Split(a.X, b.X))
                {
                    foreach (var ys in Range.Split(a.Y, b.Y))
                    {
                        foreach (var zs in Range.Split(a.Z, b.Z))
                        {
                            if ((xs.HasA && ys.HasA && zs.HasA) || 
                                (xs.HasB && ys.HasB && zs.HasB))
                            {
                                result.Add(new Cube(xs.Range, ys.Range, zs.Range));
                            }
                        }
                    }
                }

                return Merge(result);
            }

            public static IReadOnlyList<Cube> Sub(Cube a, Cube b)
            {
                // LINQ is short, but slower than explicit loops below

                //IEnumerable<Cube> sub(Cube a, Cube b) =>
                //    from xs in Range.Split(a.X, b.X)
                //    from ys in Range.Split(a.Y, b.Y)
                //    from zs in Range.Split(a.Z, b.Z)
                //    let subranges = new[] { xs, ys, zs }
                //    where subranges.All(sr => sr.HasA) && !subranges.All(sr => sr.HasA && sr.HasB)
                //    select new Cube(xs.Range, ys.Range, zs.Range);
                //
                // return Merge(sub(a, b));

                var result = new List<Cube>();

                foreach (var xs in Range.Split(a.X, b.X))
                {
                    foreach (var ys in Range.Split(a.Y, b.Y))
                    {
                        foreach (var zs in Range.Split(a.Z, b.Z))
                        {
                            if (xs.HasA && ys.HasA && zs.HasA && 
                                !(xs.HasA && ys.HasA && zs.HasA && xs.HasB && ys.HasB && zs.HasB))
                            {
                                result.Add(new Cube(xs.Range, ys.Range, zs.Range));
                            }
                        }
                    }
                }

                return Merge(result);
            }

            public static IReadOnlyList<Cube> Merge(IEnumerable<Cube> cubes)
            {
                var mergedCubes = cubes.ToArray();

                var i = 0;
                while (i < mergedCubes.Length)
                {
                    var cube = mergedCubes[i];
                    if (cube == null)
                    {
                        i++;
                        continue;
                    }

                    var j = i + 1;
                    Cube merged = null;
                    while (j < mergedCubes.Length)
                    {
                        var other = mergedCubes[j];
                        if (other == null)
                        {
                            j++;
                            continue;
                        }

                        if (TryMerge(cube, other, out merged))
                        {
                            break;
                        }

                        j++;
                    }

                    if (j < mergedCubes.Length)
                    {
                        mergedCubes[i] = merged;
                        mergedCubes[j] = null;
                    }
                    else
                    {
                        i++;
                    }
                }

                return mergedCubes.Where(c => c != null).ToList();
            }

            public static bool TryMerge(Cube a, Cube b, out Cube merged)
            {
                var matchesX = a.X.Equals(b.X);
                var matchesY = a.Y.Equals(b.Y);
                var matchesZ = a.Z.Equals(b.Z);

                if (matchesX && matchesY && Range.TryMerge(a.Z, b.Z, out var mergedZ))
                {
                    merged = new Cube(a.X, a.Y, mergedZ);
                    return true;
                }
                
                if (matchesX && matchesZ && Range.TryMerge(a.Y, b.Y, out var mergedY))
                {
                    merged = new Cube(a.X, mergedY, a.Z);
                    return true;
                }

                if (matchesY && matchesZ && Range.TryMerge(a.X, b.X, out var mergedX))
                {
                    merged = new Cube(mergedX, a.Y, a.Z);
                    return true;
                }

                merged = default;
                return false;
            }

            public Cube(Range x, Range y, Range z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public Range X { get; }
            public Range Y { get; }
            public Range Z { get; }

            public override string ToString() => $"x={X},y={Y},z={Z}";

            public bool IntersectsWith(Cube other) =>
                X.IntersectsWith(other.X) && Y.IntersectsWith(other.Y) && Z.IntersectsWith(other.Z);

            public long PointCount() => X.ValueCount() * Y.ValueCount() * Z.ValueCount();
        }

        private class Instruction
        {
            public static IReadOnlyList<Instruction> ParseMany(IEnumerable<string> lines) =>
                lines.Select(Parse).ToList();

            public static Instruction Parse(string text)
            {
                var index = text.IndexOf(' ');

                var onOffText = text.Substring(0, index);
                var cubeText = text.Substring(index + 1);

                var turnOn = onOffText == "on";
                var cube = Cube.Parse(cubeText);

                return new Instruction(turnOn, cube);
            }

            public Instruction(bool turnOn, Cube cube)
            {
                TurnOn = turnOn;
                Cube = cube;
            }

            public bool TurnOn { get; }
            public Cube Cube { get; }
        }
    }
}
