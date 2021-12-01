using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day12
    {
        public static readonly IInput Sample1Input =
            Input.Literal(
                "<x=-1, y=0, z=2>",
                "<x=2, y=-10, z=-7>",
                "<x=4, y=-8, z=8>",
                "<x=3, y=5, z=-1>"
            );

        public static readonly IInput Sample2Input =
            Input.Literal(
                "<x=-8, y=-10, z=0>",
                "<x=5, y=5, z=10>",
                "<x=2, y=-7, z=3>",
                "<x=9, y=-8, z=-3>"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/12/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var moons = input.Lines()
                    .Select(Vector.Parse)
                    .Select(position => new Moon(position, velocity: Vector.Zero))
                    .ToArray();

                var steps = 1000;

                for (var i = 0; i < steps; i++)
                {
                    //DumpMoons(i, moons);
                    moons = SimulateTimeStep(moons);
                }

                //DumpMoons(steps, moons);

                var answer = moons
                    .Select(moon => moon.Position.SumOfAbsolueValues() * moon.Velocity.SumOfAbsolueValues())
                    .Sum();

                Console.WriteLine(answer);
            }

            private Moon[] SimulateTimeStep(Moon[] moons)
            {
                var gravity = Simulation.CalculateGravity(moons);
                //DumpGravity(gravity);
                return Simulation.ApplyGravity(moons, gravity);
            }

            private static void DumpGravity(Vector[] gravity)
            {
                Console.WriteLine("Gravity");
                foreach (var velocity in gravity)
                {
                    Console.WriteLine(velocity);
                }
                Console.WriteLine();
            }

            private void DumpMoons(int step, Moon[] moons)
            {
                Console.WriteLine($"Ater {step} steps");
                foreach (var moon in moons)
                {
                    Console.WriteLine(moon);
                }
                Console.WriteLine();
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var initialMoons = input.Lines()
                    .Select(Vector.Parse)
                    .Select(position => new Moon(position, velocity: Vector.Zero))
                    .ToArray();

                var step = 0;
                var moons = initialMoons;
                var repeatStepPerCoordinate = new[] { -1, -1, -1 }; 
                do
                {
                    moons = SimulateTimeStep(moons);
                    step++;

                    for (var i = 0; i < 3; i++)
                    {
                        if (repeatStepPerCoordinate[i] < 0 && AreSame(moons, initialMoons, coord: i))
                        {
                            repeatStepPerCoordinate[i] = step;
                        }
                    }
                } while (repeatStepPerCoordinate.Any(s => s < 0));

                var answer = repeatStepPerCoordinate.Select(n => (long)n).Aggregate(MathExtensions.Lcm);
                Console.WriteLine(answer);
            }

            private bool AreSame(Moon[] moons, Moon[] initialMoons, int coord)
            {
                var currentVs = moons.Select(m => m.Velocity.Coordinates[coord]);
                if (currentVs.Any(v => v != 0))
                {
                    return false;
                }

                var currentPs = moons.Select(m => m.Position.Coordinates[coord]);
                var initialPs = initialMoons.Select(m => m.Position.Coordinates[coord]);
                return currentPs.SequenceEqual(initialPs);
            }

            private Moon[] SimulateTimeStep(Moon[] moons)
            {
                var gravity = Simulation.CalculateGravity(moons);
                return Simulation.ApplyGravity(moons, gravity);
            }
        }

        private class Vector
        {
            public static Vector Parse(string text)
            {
                var coords = text
                    .TrimStart('<').TrimEnd('>')
                    .Split(", ")
                    .Select(p => p.Substring(2)) // skip x=
                    .Select(long.Parse)
                    .ToArray();

                return new Vector(coords);
            }

            public static readonly Vector Zero = new Vector(new[] { 0L, 0L, 0L });
            
            private readonly long[] coordinates;

            public Vector(long[] coordinates)
            {
                this.coordinates = coordinates;
            }

            public IReadOnlyList<long> Coordinates => coordinates;

            public override string ToString() => 
                $"<x={coordinates[0],3}, y={coordinates[1],3}, z={coordinates[2],3}>";

            public long SumOfAbsolueValues() => 
                Math.Abs(coordinates[0]) + Math.Abs(coordinates[1]) + Math.Abs(coordinates[2]);

            public Vector Add(Vector v) => 
                new Vector(new[] 
                { 
                    coordinates[0] + v.coordinates[0], 
                    coordinates[1] + v.coordinates[1], 
                    coordinates[2] + v.coordinates[2] 
                });
        }

        private class Moon
        {
            public Moon(Vector position, Vector velocity)
            {
                Position = position;
                Velocity = velocity;
            }

            public Vector Position { get; }
            public Vector Velocity { get; }

            public override string ToString() => $"pos={Position}, vel={Velocity}";
        }
        
        private static class Simulation
        {
            public static Vector[] CalculateGravity(Moon[] moons)
            {
                static long delta(long a, long b)
                {
                    if (a < b) return 1;
                    if (a > b) return -1;
                    return 0;
                }

                static Vector deltaVelocity(Vector a, Vector b)
                {
                    return new Vector(a.Coordinates.Zip(b.Coordinates, delta).ToArray());
                }

                return moons
                    .Select(moon => moons
                        .Where(m => m != moon)
                        .Select(other => deltaVelocity(moon.Position, other.Position))
                        .Aggregate(Vector.Zero, (acc, v) => acc.Add(v))
                    )
                    .ToArray();
            }

            public static Moon[] ApplyGravity(Moon[] moons, Vector[] gravity)
            {
                return moons
                    .Zip(gravity, (moon, gravity) =>
                    {
                        var velocity = moon.Velocity.Add(gravity);
                        return new Moon(moon.Position.Add(velocity), velocity);
                    })
                    .ToArray();
            }
        }
    }
}
