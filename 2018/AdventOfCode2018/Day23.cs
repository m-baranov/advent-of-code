using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day23
    {
        public static class Inputs
        {
            public static readonly IInput Sample1 =
                Input.Literal(
                    "pos=<0,0,0>, r=4",
                    "pos=<1,0,0>, r=1",
                    "pos=<4,0,0>, r=3",
                    "pos=<0,2,0>, r=1",
                    "pos=<0,5,0>, r=3",
                    "pos=<0,0,3>, r=1",
                    "pos=<1,1,1>, r=1",
                    "pos=<1,1,2>, r=1",
                    "pos=<1,3,1>, r=1"
                );

            public static readonly IInput Sample2 =
                Input.Literal(
                    "pos=<10,12,12>, r=2",
                    "pos=<12,14,12>, r=2",
                    "pos=<16,12,12>, r=4",
                    "pos=<14,14,14>, r=6",
                    "pos=<50,50,50>, r=200",
                    "pos=<10,10,10>, r=5"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/23/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var bots = input.Lines().Select(Bot.Parse).ToList();

                var strongestBot = bots.MaxBy(b => b.Range);

                var botsInRange = bots
                    .Where(b => Point.ManhattanDistance(b.Position, strongestBot.Position) <= strongestBot.Range)
                    .Count();

                Console.WriteLine($"bots in range = {botsInRange}");
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var bots = input.Lines().Select(Bot.Parse).ToList();

                // For each two bots check in how many points their edges intersect.
                //
                // 1) Turns out, they can intersect in 0, 1, 2 or more points. If there is
                // more than 2 intersection points, all of them are on the same plane. So
                // in these cases bots touch by at most one side.
                //
                // 2) In cases when there's only 2 points, i.e. when there is a common 
                // segment of intersection, these segments belong to just few planes. And
                // these planes seem to intersect in a single point (at least on my test
                // data).
                //
                // So the point that is in range of most bots, is either the intersection
                // between planes from 1) and from 2), or one of the single points where
                // two bots touch. 
                
                var points = new List<Point>();
                var segments = new List<Segment>();
                var planes = new List<Plane>();

                for (var ai = 0; ai < bots.Count; ai++)
                {
                    for (var bi = ai + 1; bi < bots.Count; bi++)
                    {
                        var intersections = Bot.IntersectEdges(bots[ai], bots[bi]);

                        if (intersections.Count == 1)
                        {
                            points.Add(intersections[0]);
                        }
                        else if (intersections.Count == 2)
                        {
                            segments.Add(Segment.Of(intersections[0], intersections[1]));
                        }
                        else if (intersections.Count > 2)
                        {
                            var found = Bot.FindCoincidentSides(bots[ai], bots[bi], out var plane);
                            Debug.Assert(found);                            
                            planes.Add(plane);
                        }
                    }
                }

                planes.AddRange(Plane.AllOf(segments));
                points.AddRange(Plane.Intersect(Plane.NonCoincident(planes)));

                var counts = points
                    .Distinct()
                    .Select(p => new { point = p, count = bots.Count(b => Bot.Contains(b, p)) })
                    .ToList();

                var max = counts.Max(c => c.count);

                foreach (var c in counts.Where(c => c.count == max))
                {
                    Console.WriteLine($"c={c.count}, p={c.point}, d={Point.ManhattanDistance(c.point)}");
                }
            }
        }

        private record Bot(Point Position, long Range)
        {
            public static Bot Parse(string text)
            {
                const string PosPrefix = "pos=<";
                const string Separator = ">, ";
                const string RangePrefix = "r=";

                var parts = text.Split(Separator);

                var posText = parts[0].Substring(PosPrefix.Length);
                var rangeText = parts[1].Substring(RangePrefix.Length);

                var pos = Point.Parse(posText);
                var range = long.Parse(rangeText);

                return new Bot(pos, range);
            }

            public static bool Contains(Bot bot, Point pos) =>
                Point.ManhattanDistance(bot.Position, pos) <= bot.Range;

            public static IReadOnlyList<Plane> Sides(Bot bot)
            {
                var n = VertexN(bot);
                var s = VertexS(bot);
                var e = VertexE(bot);
                var w = VertexW(bot);
                var f = VertexF(bot);
                var b = VertexB(bot);

                var nfe = Plane.Of(n, f, e);
                var neb = Plane.Of(n, e, b);
                var nbw = Plane.Of(n, b, w);
                var nwf = Plane.Of(n, w, f);
                var sfe = Plane.Of(s, f, e);
                var seb = Plane.Of(s, e, b);
                var sbw = Plane.Of(s, b, w);
                var swf = Plane.Of(s, w, f);

                return new[] { nfe, neb, nbw, nwf, sfe, seb, sbw, swf };
            }

            public static IReadOnlyList<Segment> Edges(Bot bot)
            {
                var n = VertexN(bot);
                var s = VertexS(bot);
                var e = VertexE(bot);
                var w = VertexW(bot);
                var f = VertexF(bot);
                var b = VertexB(bot);

                var nf = Segment.Of(n, f);
                var ne = Segment.Of(n, e);
                var nb = Segment.Of(n, b);
                var nw = Segment.Of(n, w);

                var sf = Segment.Of(s, f);
                var se = Segment.Of(s, e);
                var sb = Segment.Of(s, b);
                var sw = Segment.Of(s, w);

                var fe = Segment.Of(f, e);
                var eb = Segment.Of(e, b);
                var bw = Segment.Of(b, w);
                var wf = Segment.Of(w, f);

                return new[] { nf, ne, nb, nw, sf, se, sb, sw, fe, eb, bw, wf };
            }

            private static Point VertexB(Bot bot) => bot.Position with { Z = bot.Position.Z - bot.Range };
            private static Point VertexF(Bot bot) => bot.Position with { Z = bot.Position.Z + bot.Range };
            private static Point VertexW(Bot bot) => bot.Position with { X = bot.Position.X - bot.Range };
            private static Point VertexE(Bot bot) => bot.Position with { X = bot.Position.X + bot.Range };
            private static Point VertexS(Bot bot) => bot.Position with { Y = bot.Position.Y - bot.Range };
            private static Point VertexN(Bot bot) => bot.Position with { Y = bot.Position.Y + bot.Range };

            public static bool FindCoincidentSides(Bot a, Bot b, out Plane plane)
            {
                var aps = Bot.Sides(a);
                var bps = Bot.Sides(b);

                var pairs =
                    (
                        from ap in aps
                        from bp in bps
                        where Plane.Coincident(ap, bp)
                        select (a: ap, b: bp)
                    )
                    .ToList();

                if (pairs.Count > 0)
                {
                    plane = pairs[0].a;
                    return true;
                }

                plane = default;
                return false;
            }

            public static IReadOnlyList<Point> IntersectEdges(Bot a, Bot b)
            {
                var eas = Bot.Edges(a);
                var ebs = Bot.Edges(b);

                var points = new List<Point>();

                foreach (var ea in eas)
                {
                    foreach (var eb in ebs)
                    {
                        var ps = Segment.Intersect(ea, eb);
                        points.AddRange(ps);
                    }
                }

                return points.Distinct().ToList();
            }

            public override string ToString() => $"p={Position}, r={Range}";
        }

        private static class Num
        {
            public static bool Close(double x, double y) => Math.Abs(x - y) < 0.0000001;

            // https://en.wikipedia.org/wiki/Cramer%27s_rule#Explicit_formulas_for_small_systems
            //
            // a1*x + b1*y = c1
            // a2*x + b2*y = c2
            public static bool TrySolve(
                double a1, double b1, double c1,
                double a2, double b2, double c2,
                out (double x, double y) solution)
            {
                var div = a1 * b2 - b1 * a2;
                if (Num.Close(div, 0))
                {
                    solution = default;
                    return false;
                }

                var x = (c1 * b2 - b1 * c2) / div;
                var y = (a1 * c2 - c1 * a2) / div;

                solution = (x, y);
                return true;
            }
        }

        private record Point(long X, long Y, long Z)
        {
            public static readonly Point Origin = new(0, 0, 0);

            public static Point Parse(string text)
            {
                var parts = text.Split(',').Select(p => p.Trim()).Select(long.Parse).ToList();
                return new Point(parts[0], parts[1], parts[2]);
            }

            public static double ManhattanDistance(Point a) => ManhattanDistance(a, Origin);

            public static double ManhattanDistance(Point a, Point b) =>
                Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z);
        }

        // ax + by + cz + d = 0
        private record Plane(double A, double B, double C, double D)
        {
            public static Plane Of(Point p1, Point p2, Point p3)
            {
                // Need to widen the data type here to avoid overflow on test data
                var a1 = (decimal)p2.X - p1.X;
                var b1 = (decimal)p2.Y - p1.Y;
                var c1 = (decimal)p2.Z - p1.Z;
                var a2 = (decimal)p3.X - p1.X;
                var b2 = (decimal)p3.Y - p1.Y;
                var c2 = (decimal)p3.Z - p1.Z;
                var a = b1 * c2 - b2 * c1;
                var b = a2 * c1 - a1 * c2;
                var c = a1 * b2 - b1 * a2;
                var d = -a * p1.X - b * p1.Y - c * p1.Z;

                return new Plane(1, (double)(b/a), (double)(c/a), (double)(d/a));
            }

            public static bool Of(Segment s1, Segment s2, out Plane p)
            {
                if (!Line.Intersect(s1.Line, s2.Line, out var _))
                {
                    p = default;
                    return false;
                }

                p = Line.Contains(s1.Line, s2.A) ? Of(s1.A, s1.B, s2.B) : Of(s1.A, s1.B, s2.A);
                return true;
            }

            public static IReadOnlyList<Plane> AllOf(IReadOnlyList<Segment> segments)
            {
                var planes = new List<Plane>();

                for (var ai = 0; ai < segments.Count; ai++)
                {
                    for (var bi = ai + 1; bi < segments.Count; bi++)
                    {
                        if (Plane.Of(segments[ai], segments[bi], out var plane))
                        {
                            planes.Add(plane);
                        }
                    }
                }

                return planes;
            }

            public static IReadOnlyList<Point> Intersect(IReadOnlyList<Plane> planes)
            {
                var points = new List<Point>();

                for (var ai = 0; ai < planes.Count; ai++)
                {
                    for (var bi = ai + 1; bi < planes.Count; bi++)
                    {
                        for (var ci = bi + 1; ci < planes.Count; ci++)
                        {
                            var a = planes[ai];
                            var b = planes[bi];
                            var c = planes[ci];

                            var l1 = Line.Of(a, b);
                            var l2 = Line.Of(b, c);

                            if (Line.Intersect(l1, l2, out var p))
                            {
                                points.Add(p);
                            }
                        }
                    }
                }

                return points;
            }

            public static IReadOnlyList<Plane> NonCoincident(IReadOnlyList<Plane> planes)
            {
                var result = new List<Plane>();
                
                foreach (var plane in planes)
                {
                    if (!result.Any(p => Plane.Coincident(p, plane)))
                    {
                        result.Add(plane);
                    }
                }

                return result;
            }

            public static bool Coincident(Plane x, Plane y) =>
                Num.Close(x.A * y.B, x.B * y.A) && Num.Close(x.B * y.C, x.C * y.B) && Num.Close(x.C * y.D, x.D * y.C);
        }

        // x = At + B
        // y = Ct + D
        // z = Et + F
        private record Line(double A, double B, double C, double D, double E, double F)
        {
            public static Line Of(Point p1, Point p2)
            {
                var a = p2.X - p1.X;
                var b = p1.X;
                var c = p2.Y - p1.Y;
                var d = p1.Y;
                var e = p2.Z - p1.Z;
                var f = p1.Z;

                return new Line(a, b, c, d, e, f);
            }

            public static Line Of(Plane p1, Plane p2)
            {
                static (double a, double b, double c) NormalVector(Plane p1, Plane p2)
                {
                    // | i  j  k  |
                    // | a1 b1 c1 | = i | b1 c1 | - j | a1 c1 | + k | a1 b1 | 
                    // | a2 b2 c2 |     | b2 c2 |     | a2 c2 |     | a2 b2 |

                    var a = p1.B * p2.C - p1.C * p2.B;
                    var b = -(p1.A * p2.C - p1.C * p2.A);
                    var c = p1.A * p2.B - p1.B * p2.A;

                    return (a, b, c);
                }

                static (double x, double y, double z) CommonPoint(Plane p1, Plane p2)
                {
                    var solved = Num.TrySolve(
                        p1.A, p1.B, -p1.D,
                        p2.A, p2.B, -p2.D,
                        out var solution
                    );

                    if (solved) 
                    {
                        return (solution.x, solution.y, 0);
                    }

                    Num.TrySolve(
                        p1.A, p1.C, -p1.D,
                        p2.A, p2.C, -p2.D,
                        out solution
                    );
                    return (solution.x, 0, solution.y);
                }

                var v = NormalVector(p1, p2);
                var p = CommonPoint(p1, p2);

                return new Line(v.a, p.x, v.b, p.y, v.c, p.z);
            }

            public static bool Contains(Line l, Point p) =>
                Num.Close((p.X - l.B) * l.C, (p.Y - l.D) * l.A) && 
                Num.Close((p.Y - l.D) * l.E, (p.Z - l.F) * l.C);

            public static bool Intersect(Line l1, Line l2, out Point p)
            {
                // x = A1t + B1 = A2q + B2
                // y = C1t + D1 = C2q + D2
                // z = E1t + F1 = E2q + F2

                // A1t + (-A2)q = (B2 - B1)
                // C1t + (-C2)q = (D2 - D1)
                // E1t + (-E2)q = (F2 - F1)

                var solved = Num.TrySolve(
                    l1.A, -l2.A, l2.B - l1.B,
                    l1.C, -l2.C, l2.D - l1.D,
                    out var solution
                );
                if (!solved)
                {
                    p = default;
                    return false;
                }

                var (t, q) = solution;

                var check = Num.Close(l1.E * t - l2.E * q, l2.F - l1.F);
                if (!check)
                {
                    p = default;
                    return false;
                }

                p = new Point((long)(l1.A * t + l1.B), (long)(l1.C * t + l1.D), (long)(l1.E * t + l1.F));
                return true;
            }
        }

        private record Segment(Line Line, Point A, Point B)
        {
            public static Segment Of(Point a, Point b) =>
                new Segment(Line.Of(a, b), a, b);

            public static bool Contains(Segment s, Point p)
            {
                static bool Between(double n, double a, double b)
                {
                    var (l, r) = (Math.Min(a, b), Math.Max(a, b));
                    return (l < n || Num.Close(l, n)) && (n < r || Num.Close(n, r));
                }

                if (!Line.Contains(s.Line, p))
                {
                    return false;
                }

                return Between(p.X, s.A.X, s.B.X)
                    && Between(p.Y, s.A.Y, s.B.Y)
                    && Between(p.Z, s.A.Z, s.B.Z);
            }

            public static IReadOnlyList<Point> Intersect(Segment s1, Segment s2)
            {
                var points = new List<Point>();

                if (Segment.Contains(s1, s2.A))
                {
                    points.Add(s2.A);
                }
                if (Segment.Contains(s1, s2.B))
                {
                    points.Add(s2.B);
                }
                if (Segment.Contains(s2, s1.A))
                {
                    points.Add(s1.A);
                }
                if (Segment.Contains(s2, s1.B))
                {
                    points.Add(s1.B);
                }

                if (points.Any())
                {
                    return points.Distinct().ToList();
                }

                if (!Line.Intersect(s1.Line, s2.Line, out var p))
                {
                    return points;
                }

                if (Segment.Contains(s1, p) && Segment.Contains(s2, p))
                {
                    points.Add(p);
                }
                return points;
            }
        }
    }
}
