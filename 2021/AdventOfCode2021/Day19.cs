using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    // Slow, but works. Takes ~20-25 sec to complete on test input on my notebook.
    static class Day19
    {
        public static class Inputs
        {
            #region Sample
            public static readonly IInput Sample =
                Input.Literal(
                    "--- scanner 0 ---",
                    "404,-588,-901",
                    "528,-643,409",
                    "-838,591,734",
                    "390,-675,-793",
                    "-537,-823,-458",
                    "-485,-357,347",
                    "-345,-311,381",
                    "-661,-816,-575",
                    "-876,649,763",
                    "-618,-824,-621",
                    "553,345,-567",
                    "474,580,667",
                    "-447,-329,318",
                    "-584,868,-557",
                    "544,-627,-890",
                    "564,392,-477",
                    "455,729,728",
                    "-892,524,684",
                    "-689,845,-530",
                    "423,-701,434",
                    "7,-33,-71",
                    "630,319,-379",
                    "443,580,662",
                    "-789,900,-551",
                    "459,-707,401",
                    "",
                    "--- scanner 1 ---",
                    "686,422,578",
                    "605,423,415",
                    "515,917,-361",
                    "-336,658,858",
                    "95,138,22",
                    "-476,619,847",
                    "-340,-569,-846",
                    "567,-361,727",
                    "-460,603,-452",
                    "669,-402,600",
                    "729,430,532",
                    "-500,-761,534",
                    "-322,571,750",
                    "-466,-666,-811",
                    "-429,-592,574",
                    "-355,545,-477",
                    "703,-491,-529",
                    "-328,-685,520",
                    "413,935,-424",
                    "-391,539,-444",
                    "586,-435,557",
                    "-364,-763,-893",
                    "807,-499,-711",
                    "755,-354,-619",
                    "553,889,-390",
                    "",
                    "--- scanner 2 ---",
                    "649,640,665",
                    "682,-795,504",
                    "-784,533,-524",
                    "-644,584,-595",
                    "-588,-843,648",
                    "-30,6,44",
                    "-674,560,763",
                    "500,723,-460",
                    "609,671,-379",
                    "-555,-800,653",
                    "-675,-892,-343",
                    "697,-426,-610",
                    "578,704,681",
                    "493,664,-388",
                    "-671,-858,530",
                    "-667,343,800",
                    "571,-461,-707",
                    "-138,-166,112",
                    "-889,563,-600",
                    "646,-828,498",
                    "640,759,510",
                    "-630,509,768",
                    "-681,-892,-333",
                    "673,-379,-804",
                    "-742,-814,-386",
                    "577,-820,562",
                    "",
                    "--- scanner 3 ---",
                    "-589,542,597",
                    "605,-692,669",
                    "-500,565,-823",
                    "-660,373,557",
                    "-458,-679,-417",
                    "-488,449,543",
                    "-626,468,-788",
                    "338,-750,-386",
                    "528,-832,-391",
                    "562,-778,733",
                    "-938,-730,414",
                    "543,643,-506",
                    "-524,371,-870",
                    "407,773,750",
                    "-104,29,83",
                    "378,-903,-323",
                    "-778,-728,485",
                    "426,699,580",
                    "-438,-605,-362",
                    "-469,-447,-387",
                    "509,732,623",
                    "647,635,-688",
                    "-868,-804,481",
                    "614,-800,639",
                    "595,780,-596",
                    "",
                    "--- scanner 4 ---",
                    "727,592,562",
                    "-293,-554,779",
                    "441,611,-461",
                    "-714,465,-776",
                    "-743,427,-804",
                    "-660,-479,-426",
                    "832,-632,460",
                    "927,-485,-438",
                    "408,393,-506",
                    "466,436,-512",
                    "110,16,151",
                    "-258,-428,682",
                    "-393,719,612",
                    "-211,-452,876",
                    "808,-476,-593",
                    "-575,615,604",
                    "-485,667,467",
                    "-680,325,-822",
                    "-627,-443,-432",
                    "872,-547,-609",
                    "833,512,582",
                    "807,604,487",
                    "839,-516,451",
                    "891,-625,532",
                    "-652,-548,-490",
                    "30,-46,-14"
                );
            #endregion

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/19/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var scans = Scan.ParseMany(input.Lines());

                var conversions = ScanMatcher.FindConversionsToScanner0(scans);

                var pointCount = scans
                    .SelectMany(scan => scan.Points.Select(conversions[scan.Scanner].Convert))
                    .Distinct()
                    .Count();

                Console.WriteLine(pointCount);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var scans = Scan.ParseMany(input.Lines());

                var conversions = ScanMatcher.FindConversionsToScanner0(scans);

                var origins = scans
                    .Select(scan => conversions[scan.Scanner].Convert(Vector.Zero))
                    .ToList();

                var answer = origins.AllPossiblePairs()
                    .Select(p => Vector.ManhattanDistance(p.first, p.second))
                    .Max();

                Console.WriteLine(answer);
            }
        }

        private class Scan
        {
            public static IReadOnlyList<Scan> ParseMany(IEnumerable<string> lines)
            {
                return lines.SplitByEmptyLine().Select(Parse).ToList();
            }

            private static Scan Parse(IReadOnlyList<string> lines, int scanner)
            {
                var points = lines.Skip(1) // skip "--- scanner N ---"
                    .Select(Vector.Parse)
                    .ToList();

                return new Scan(scanner, points);
            }

            public Scan(int scanner, IReadOnlyList<Vector> points)
            {
                Scanner = scanner;
                Points = points;
            }

            public int Scanner { get; }
            public IReadOnlyList<Vector> Points { get; }

            public Scan RelativeTo(Vector origin)
            {
                var points = Points.Select(p => p.Sub(origin)).ToList();
                return new Scan(Scanner, points);
            }

            public IReadOnlyList<(int thisIndex, int otherIndex)> MatchingPointIndices(Scan other)
            {
                return Points
                    .Select((thisPoint, thisIndex) => (thisIndex, otherIndex: other.IndexOfPoint(thisPoint)))
                    .Where(p => p.otherIndex >= 0)
                    .ToList();
            }

            public int IndexOfPoint(Vector point) => Points.IndexOf(p => p.Equals(point));

            public IEnumerable<(Scan scan, Rotation rotation)> AllRotations() =>
                Rotation.All.Select(r => (Transform(r.Rotate), r));

            public Scan Transform(Func<Vector, Vector> tranfrom)
            {
                var points = Points.Select(tranfrom).ToList();
                return new Scan(Scanner, points);
            }
        }

        private class Vector
        {
            public static readonly Vector Zero = new Vector(0, 0, 0);

            public static Vector Parse(string text)
            {
                var parts = text.Split(',').Take(3).Select(int.Parse).ToList();
                return new Vector(parts[0], parts[1], parts[2]);
            }

            public static Vector Delta(Vector a, Vector b)
            {
                static int delta(int a, int b) => Math.Abs(a - b);
                return new Vector(delta(a.X, b.X), delta(a.Y, b.Y), delta(a.Z, b.Z));
            }

            public static int ManhattanDistance(Vector a, Vector b)
            {
                var d = Delta(a, b);
                return d.X + d.Y + d.Z;
            }

            public Vector(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public int X { get; }
            public int Y { get; }
            public int Z { get; }

            public override string ToString() => $"{X},{Y},{Z}";

            public override bool Equals(object obj) =>
                obj is Vector other ? Equals(other) : false;

            private bool Equals(Vector other) => X == other.X && Y == other.Y && Z == other.Z;

            public override int GetHashCode() => HashCode.Combine(X, Y, Z);

            public Vector Sub(Vector v) => new Vector(X - v.X, Y - v.Y, Z - v.Z);

            public Vector Negate() => new Vector(-X, -Y, -Z);

            public Vector FlipX() => new Vector(-X, Y, Z);
            public Vector FlipY() => new Vector(X, -Y, Z);
            public Vector FlipZ() => new Vector(X, Y, -Z);

            public Vector RotateX0() => RotateX(sin: 0, cos: 1);
            public Vector RotateX90() => RotateX(sin: 1, cos: 0);
            public Vector RotateX180() => RotateX(sin: 0, cos: -1);
            public Vector RotateX270() => RotateX(sin: -1, cos: 0);

            // Matrix: 
            // | 1       0        0 |
            // | 0  Cos(a)  -Sin(a) |
            // | 0  Sin(a)   Cos(a) |
            private Vector RotateX(int sin, int cos) =>
                new Vector(
                    X, 
                    cos * Y - sin * Z,
                    sin * Y + cos * Z
                );

            public Vector RotateY0() => RotateY(sin: 0, cos: 1);
            public Vector RotateY90() => RotateY(sin: 1, cos: 0);
            public Vector RotateY180() => RotateY(sin: 0, cos: -1);
            public Vector RotateY270() => RotateY(sin: -1, cos: 0);

            // Matrix
            // |  Cos(a)  0  Sin(a) |
            // |       0  1       0 |
            // | -Sin(a)  0  Cos(a) |
            private Vector RotateY(int sin, int cos) =>
                new Vector(
                    cos * X + sin * Z,
                    Y,
                    -sin * X + cos * Z
                );

            public Vector RotateZ0() => RotateZ(sin: 0, cos: 1);
            public Vector RotateZ90() => RotateZ(sin: 1, cos: 0);
            public Vector RotateZ180() => RotateZ(sin: 0, cos: -1);
            public Vector RotateZ270() => RotateZ(sin: -1, cos: 0);

            // Matrix
            // | Cos(a)  -Sin(a)  0 |
            // | Sin(a)   Cos(a)  0 |
            // |      0        0  1 |
            private Vector RotateZ(int sin, int cos) =>
                new Vector(
                    cos * X - sin * Y,
                    sin * X + cos * Y,
                    Z
                );
        }

        private class Rotation
        {
            public static readonly IReadOnlyList<Rotation> All =
                new[]
                {
                    new Rotation(v => v),
                 
                    // axis X
                    new Rotation(v => v.RotateX90(), v => v.RotateX270()),
                    new Rotation(v => v.RotateX180()),
                    new Rotation(v => v.RotateX270(), v => v.RotateX90()),

                    new Rotation(v => v.FlipX()),
                    new Rotation(v => v.FlipX().RotateX90(), v => v.RotateX270().FlipX()),
                    new Rotation(v => v.FlipX().RotateX180(), v => v.RotateX180().FlipX()),
                    new Rotation(v => v.FlipX().RotateX270(), v => v.RotateX90().FlipX()),

                    // axis Y
                    new Rotation(v => v.RotateY90(), v => v.RotateY270()),
                    new Rotation(v => v.RotateY180()),
                    new Rotation(v => v.RotateY270(), v => v.RotateY90()),

                    new Rotation(v => v.FlipY()),
                    new Rotation(v => v.FlipY().RotateY90(), v => v.RotateY270().FlipY()),
                    new Rotation(v => v.FlipY().RotateY180(), v => v.RotateY180().FlipY()),
                    new Rotation(v => v.FlipY().RotateY270(), v => v.RotateY90().FlipY()),

                    // axis Z
                    new Rotation(v => v.RotateZ90(), v => v.RotateZ270()),
                    new Rotation(v => v.RotateZ180()),
                    new Rotation(v => v.RotateZ270(), v => v.RotateZ90()),

                    new Rotation(v => v.FlipZ()),
                    new Rotation(v => v.FlipZ().RotateY90(), v => v.RotateZ270().FlipZ()),
                    new Rotation(v => v.FlipZ().RotateY180(), v => v.RotateZ180().FlipZ()),
                    new Rotation(v => v.FlipZ().RotateY270(), v => v.RotateZ90().FlipZ()),
                };

            public Rotation(Func<Vector, Vector> rotate)
                : this(rotate, unrotate: rotate)
            {
            }

            public Rotation(Func<Vector, Vector> rotate, Func<Vector, Vector> unrotate)
            {
                Rotate = rotate;
                Unrotate = unrotate;
            }

            public Func<Vector, Vector> Rotate { get; }
            public Func<Vector, Vector> Unrotate { get; }
        }

        private interface IConversion
        {
            Vector Convert(Vector point);
        }

        private class IdentityConversion : IConversion
        {
            public static readonly IdentityConversion Instance = new IdentityConversion();

            private IdentityConversion() {  }

            public Vector Convert(Vector point) => point;

            public override string ToString() => "id";
        }

        private class AggregateConversion : IConversion
        {
            private readonly IReadOnlyList<IConversion> conversions;

            public AggregateConversion(IReadOnlyList<IConversion> conversions)
            {
                this.conversions = conversions;
            }

            public Vector Convert(Vector point) => conversions.Aggregate(point, (p, c) => c.Convert(p));

            public override string ToString() => string.Join(",", conversions);
        }

        private class DirectConversion : IConversion
        {
            public DirectConversion(
                int scannerSrc, 
                Rotation rotationSrc, 
                int scannerDst, 
                Rotation rotationDst, 
                Vector originDstInSrc)
            {
                ScannerSrc = scannerSrc;
                RotationSrc = rotationSrc;
                ScannerDst = scannerDst;
                RotationDst = rotationDst;
                OriginDstInSrc = originDstInSrc;
            }

            public int ScannerSrc { get; }
            public Rotation RotationSrc { get; }
            public int ScannerDst { get; }
            public Rotation RotationDst { get; }
            public Vector OriginDstInSrc { get; }

            public override string ToString() => $"{ScannerSrc}->{ScannerDst}";

            public DirectConversion Invert() =>
                new DirectConversion(
                    scannerSrc: ScannerDst,
                    rotationSrc: RotationDst,
                    scannerDst: ScannerSrc,
                    rotationDst: RotationSrc,
                    originDstInSrc: OriginDstInSrc.Negate()
                );

            public Vector Convert(Vector src) => 
                RotationDst.Unrotate(RotationSrc.Rotate(src).Sub(OriginDstInSrc));
        }

        private class DirectConversionCollection
        {
            private readonly IReadOnlyList<DirectConversion> conversions;

            public DirectConversionCollection(IReadOnlyList<DirectConversion> conversions)
            {
                this.conversions = conversions;
            }

            public IEnumerable<DirectConversion> AsEnumerable() => conversions;

            public IEnumerable<int> ConversionDestinationsOf(int fromScanner)
            {
                var direct = conversions.Where(m => m.ScannerSrc == fromScanner).Select(m => m.ScannerDst);
                var inverse = conversions.Where(m => m.ScannerDst == fromScanner).Select(m => m.ScannerSrc);
                return direct.Concat(inverse);
            }

            public IConversion CreateTransitiveConversion(IReadOnlyList<int> scannerConversionPath)
            {
                if (scannerConversionPath.Count == 1)
                {
                    return IdentityConversion.Instance;
                }

                var conversions = scannerConversionPath.Pairs()
                    .Select(step => Get(step.first, step.second))
                    .ToList();

                if (conversions.Count == 1)
                {
                    return conversions[0];
                }

                return new AggregateConversion(conversions);
            }

            public DirectConversion Get(int fromScanner, int toScanner)
            {
                var direct = conversions.FirstOrDefault(m => m.ScannerSrc == fromScanner && m.ScannerDst == toScanner);
                if (direct != null)
                {
                    return direct;
                }

                var inverse = conversions.First(m => m.ScannerSrc == toScanner && m.ScannerDst == fromScanner);
                return inverse.Invert();
            }
        }

        private static class ScanMatcher
        {
            public static IReadOnlyDictionary<int, IConversion> FindConversionsToScanner0(IReadOnlyList<Scan> scans)
            {
                var watch = Stopwatch.StartNew();

                var directConversions = FindDirectConversions(scans);
                DisplayConversions(directConversions);

                var conversionsToScanner0 = FindTransitiveConversions(directConversions, toCommonScanner: 0);
                DisplayConversions(conversionsToScanner0);

                watch.Stop();
                Console.WriteLine($"Elapsed: {watch.ElapsedMilliseconds} ms");

                return conversionsToScanner0;
            }

            private static void DisplayConversions(DirectConversionCollection conversions)
            {
                foreach (var conversion in conversions.AsEnumerable())
                {
                    Console.WriteLine(conversion);
                }
                Console.WriteLine();
            }

            private static void DisplayConversions(IReadOnlyDictionary<int, IConversion> conversions)
            {
                foreach (var pair in conversions)
                {
                    Console.WriteLine($"{pair.Key}: {pair.Value}");
                }
            }

            private static DirectConversionCollection FindDirectConversions(IReadOnlyList<Scan> scans)
            {
                var conversions = scans
                    .AllPossiblePairs()
                    .Select(pair => TryFindDirectConversion(pair.first, pair.second))
                    .Where(c => c != null)
                    .ToList();

                return new DirectConversionCollection(conversions);
            }

            private static DirectConversion TryFindDirectConversion(Scan scanA, Scan scanB)
            {
                return scanA.AllRotations()
                    .SelectMany(a => scanB.AllRotations()
                        .Select(b =>
                        {
                            var indices = MatchingPointIndices(a.scan, b.scan);
                            if (indices == null)
                            {
                                return null;
                            }

                            var (indexA, indexB) = indices[0];
                            var originBInA = a.scan.Points[indexA].Sub(b.scan.Points[indexB]);

                            return new DirectConversion(
                                a.scan.Scanner,
                                a.rotation,
                                b.scan.Scanner,
                                b.rotation,
                                originBInA
                            );
                        })
                    )
                    .FirstOrDefault(c => c != null);
            }

            private static IReadOnlyList<(int indexA, int indexB)> MatchingPointIndices(Scan scanA, Scan scanB)
            {
                static IEnumerable<Vector> Deltas(Scan scan) =>
                    scan.Points.AllPossiblePairs().Select(p => Vector.Delta(p.first, p.second));

                var commonDeltas = Deltas(scanA).Intersect(Deltas(scanB)).ToHashSet();
                if (commonDeltas.Count < 11) // 12 points, but 11 pairs
                {
                    return null;
                }

                static IEnumerable<Vector> PossibleCommonPoints(Scan scan, ISet<Vector> commonDeltas) =>
                    scan.Points.AllPossiblePairs()
                        .Where(p => commonDeltas.Contains(Vector.Delta(p.first, p.second)))
                        .SelectMany(p => new[] { p.first, p.second })
                        .Distinct()
                        .ToList();

                var pointsA = PossibleCommonPoints(scanA, commonDeltas);
                var pointsB = PossibleCommonPoints(scanB, commonDeltas);

                return pointsA
                    .SelectMany(originA =>
                    {
                        var relativeScanA = scanA.RelativeTo(originA);

                        return pointsB
                            .Select(originB =>
                            {
                                var relativeScanB = scanB.RelativeTo(originB);
                                return relativeScanA.MatchingPointIndices(relativeScanB);
                            })
                            .Where(indices => indices.Count >= 12);
                    })
                    .FirstOrDefault();
            }

            private static IReadOnlyDictionary<int, IConversion> FindTransitiveConversions(
                DirectConversionCollection directConversions,
                int toCommonScanner)
            {
                var toVisit = new Queue<int>();
                toVisit.Enqueue(toCommonScanner);

                var visits = new Dictionary<int, Visit>();
                visits.Add(toCommonScanner, new Visit());

                while (toVisit.Count > 0)
                {
                    var srcScanner = toVisit.Dequeue();
                    var visit = visits[srcScanner];

                    var nextVisit = new Visit()
                    {
                        PreviousScanner = srcScanner,
                        Distance = visit.Distance + 1
                    };

                    foreach (var dstScanner in directConversions.ConversionDestinationsOf(srcScanner))
                    {
                        if (visits.TryGetValue(dstScanner, out var existingVisit) &&
                            existingVisit.Distance < nextVisit.Distance)
                        {
                            continue;
                        }

                        visits[dstScanner] = nextVisit;
                        toVisit.Enqueue(dstScanner);
                    }
                }

                return visits.Keys
                    .ToDictionary(s => s, s =>
                    {
                        var conversionPath = EnumerateScannerPath(visits, s).ToList();
                        return directConversions.CreateTransitiveConversion(conversionPath);
                    });
            }

            private static IEnumerable<int> EnumerateScannerPath(IReadOnlyDictionary<int, Visit> visits, int initialScanner)
            {
                int? scanner = initialScanner;
                while (scanner != null)
                {
                    yield return scanner.Value;
                    scanner = visits[scanner.Value].PreviousScanner;
                }
            }

            private class Visit
            {
                public int? PreviousScanner { get; set; }
                public int Distance { get; set; }
            }
        }
    }
}
