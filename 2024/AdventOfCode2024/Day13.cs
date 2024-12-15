using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2024;

static class Day13
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
Button A: X+94, Y+34
Button B: X+22, Y+67
Prize: X=8400, Y=5400

Button A: X+26, Y+66
Button B: X+67, Y+21
Prize: X=12748, Y=12176

Button A: X+17, Y+86
Button B: X+84, Y+37
Prize: X=7870, Y=6450

Button A: X+69, Y+23
Button B: X+27, Y+71
Prize: X=18641, Y=10279
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/13/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var cases = Case.ParseMany(input.Lines());

            var sum = SolveAll(cases);
            Console.WriteLine(sum);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var cases = Case.ParseMany(input.Lines())
                .Select(c => c with
                {
                    P = new Point(c.P.X + 10_000_000_000_000, c.P.Y + 10_000_000_000_000)
                })
                .ToList();

            var sum = SolveAll(cases);
            Console.WriteLine(sum);
        }
    }

    private static long SolveAll(IReadOnlyList<Case> cases)
    {
        var sum = 0L;
        foreach (var @case in cases)
        {
            var solved = Solve(@case, out var solution);
            if (solved)
            {
                sum += solution.X * 3 + solution.Y;
            }
        }
        return sum;
    }

    private static bool Solve(Case @case, out Point solution)
    {
        var (a, b, p) = @case;

        var (y, remY) = Math.DivRem(p.X * a.Y - p.Y * a.X, b.X * a.Y - b.Y * a.X);
        if (remY != 0)
        {
            solution = new Point(0, 0);
            return false;
        }

        var (x, remX) = Math.DivRem(p.X - y * b.X, a.X);
        if (remX != 0)
        {
            solution = new Point(0, 0);
            return false;
        }

        solution = new Point(x, y);
        return true;
    }

    private record Point(long X, long Y);

    private record Case(Point A, Point B, Point P)
    {
        public static IReadOnlyList<Case> ParseMany(IEnumerable<string> lines)
        {
            return lines.Chunk(4)
                .Select(c => Parse(c.Take(3).ToArray()))
                .ToArray();
        }

        public static Case Parse(IReadOnlyList<string> lines)
        {
            static Point ParsePoint(string text, string prefix1, string prefix2)
            {
                var parts = text.Split(", ");

                var x = TrimStart(parts[0], prefix1);
                var y = TrimStart(parts[1], prefix2);

                return new Point(long.Parse(x), long.Parse(y));
            }

            var a = ParsePoint(lines[0], "Button A: X+", "Y+");
            var b = ParsePoint(lines[1], "Button B: X+", "Y+");
            var p = ParsePoint(lines[2], "Prize: X=", "Y=");

            return new Case(a, b, p);
        }
    }

    private static string TrimStart(string text, string prefix) =>
        text.Substring(prefix.Length);
}
