using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AdventOfCode2024;

static class Day14
{
    public static class Inputs
    {
        public static readonly IInput Debug =
            Input.Literal(""""""
p=2,4 v=2,-3
"""""");

        public static readonly IInput Sample =
            Input.Literal(""""""
p=0,4 v=3,-3
p=6,3 v=-1,-3
p=10,3 v=-1,2
p=2,0 v=2,-1
p=0,0 v=1,3
p=3,0 v=-2,-2
p=7,6 v=-1,-3
p=3,0 v=-1,-2
p=9,3 v=2,3
p=7,3 v=-1,2
p=2,4 v=2,-3
p=9,5 v=-3,-3
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/14/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var robots = input.Lines()
                .Select(Robot.Parse)
                .ToArray();

            var size = new Vector(101, 103);
            var time = 100;

            var positions = robots
                .Select(r => r.PositionAt(time, size))
                .ToArray();

            var mul = Mul(positions
                .Select(p => Quadrant(p, size))
                .Where(q => q != 0)
                .GroupBy(q => q)
                .Select(q => (long)q.Count()));

            Console.WriteLine(mul);
        }

        private static int Quadrant(Vector p, Vector size)
        {
            var mx = size.X / 2;
            var my = size.Y / 2;

            var l = 0 <= p.X && p.X < mx;
            var r = mx < p.X;

            var u = 0 <= p.Y && p.Y < my;
            var d = my < p.Y;

            if (l && u) return 1;
            if (r && u) return 2;
            if (r && d) return 3;
            if (l && d) return 4;
            return 0;
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var robots = input.Lines()
                .Select(Robot.Parse)
                .ToArray();

            var size = new Vector(101, 103);

            var time = 0;
            while (true)
            {
                var positions = robots
                    .Select(r => r.PositionAt(time, size))
                    .ToArray();

                // After drawing robot positions at every time increment,
                // one can observe that occasionally most of the robots
                // cluster between lines 38 and 70. Lets see if this is
                // the easter egg appears sometime when time happens.
                var count = positions.Count(p => 38 <= p.Y && p.Y <= 70);
                if (count >= robots.Length / 2)
                {
                    var lines = Draw(size, positions);
                    foreach (var line in lines)
                    {
                        Console.WriteLine(line);
                    }
                    Console.WriteLine($"time={time}");

                    // No automatic easter egg detection here. Abort the
                    // program when the christmas tree appears on the
                    // screen.
                    Console.ReadLine();
                }

                time++;
            }
        }

// lines 38-70
// ..................#............................###############################.......................
// ...............................................#.............................#.......................
// ...............................................#.............................#.....#....#..#.........
// .................................#.............#.............................#.........#.............
// ........................#......................#.............................#.......................
// ...............................................#..............#..............#.......................
// ...............................................#.............###.............#..........#............
// ..#............................................#............#####............#.......................
// ..............................#................#...........#######...........#.......................
// ...............................................#..........#########..........#.......................
// ...............................................#............#####............#.......................
// ...............................................#...........#######...........#.......................
// ...................................#...........#..........#########..........#.....#.................
// ............#..........................#.......#.........###########.........#.#.....................
// ...............................................#........#############........#.......................
// ...............................................#..........#########..........#.......................
// ...............................................#.........###########.........#.................#.....
// ...............................................#........#############........#.......................
// .........................#......#..............#.......###############.......#.......................
// ...............................................#......#################......#.......#............#..
// .....#.........................................#........#############........#...........#...........
// ..................#............#...............#.......###############.......#................#......
// ...............................................#......#################......#.......................
// ...............................................#.....###################.....#.......................
// ...............................................#....#####################....#.......................
// ........#.................................#....#.............###.............#.......................
// ...............................................#.............###.............#.......................
// ..#............................................#.............###.............#.......................
// ........#.............#........................#.............................#.......................
// ..........................#....................#.............................#........#..............
// ........................................#......#.............................#.......................
// ...............................................#.............................#.......................
// .............#.................................###############################.......................

        private static IReadOnlyList<string> Draw(Vector size, IReadOnlyList<Vector> positions)
        {
            var lines = new List<string>(capacity: (int)size.Y);

            for (var y = 0; y < size.Y; y++)
            {
                var sb = new StringBuilder();
                for (var x = 0; x < size.X; x++)
                {
                    var pos = new Vector(x, y);
                    var has = positions.Contains(pos);
                    sb.Append(has ? '#' : '.');
                }
                lines.Add(sb.ToString());
            }

            return lines;
        }
    }

    private static long Mul(IEnumerable<long> nums) =>
        nums.Aggregate(1L, (acc, num) => acc * num);

    private record Vector(long X, long Y)
    {
        public static Vector Parse(string text)
        {
            var parts = text.Split(',');

            var x = long.Parse(parts[0]);
            var y = long.Parse(parts[1]);

            return new Vector(x, y);
        }

        public Vector Add(Vector v) =>
            new(this.X + v.X, this.Y + v.Y);

        public Vector Mul(long f) =>
            new(this.X * f, this.Y * f);

        public Vector Mod(Vector v)
        {
            static long Mod(long v, long u) =>
                v >= 0 ? v % u : u - 1 + (v + 1) % u;

            return new(Mod(this.X, v.X), Mod(this.Y, v.Y));
        }
    }

    private record Robot(Vector P, Vector V)
    {
        public static Robot Parse(string text)
        {
            var parts = text.Split(' ');

            var p = TrimStart(parts[0], "p=");
            var v = TrimStart(parts[1], "v=");

            return new Robot(Vector.Parse(p), Vector.Parse(v));
        }

        public Vector PositionAt(int time, Vector size) =>
            this.P.Add(this.V.Mul(time)).Mod(size);
    }

    private static string TrimStart(string text, string prefix) =>
        text.Substring(prefix.Length);
}
