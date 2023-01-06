using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day20
    {
        public static class Inputs
        {
            public static readonly IInput Sample1 =
                Input.Literal(
                    "p=<3,0,0>, v=<2,0,0>, a=<-1,0,0>",
                    "p=<4,0,0>, v=<0,0,0>, a=<-2,0,0>"
                );

            public static readonly IInput Sample2 =
                Input.Literal(
                    "p=<-6,0,0>, v=< 3,0,0>, a=< 0,0,0>",
                    "p=<-4,0,0>, v=< 2,0,0>, a=< 0,0,0>",
                    "p=<-2,0,0>, v=< 1,0,0>, a=< 0,0,0>",
                    "p=< 3,0,0>, v=<-1,0,0>, a=< 0,0,0>"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/20/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var particles = input.Lines().Select(Particle.Parse).ToList();

                var target = Vector.Zero;

                var states = particles
                    .Select(p => (current: p, previous: p))
                    .ToList();

                do
                {
                    AdvanceAll(states);
                } 
                while (!AreAllMovingAwayFast(target, states));

                do
                {
                    AdvanceAll(states);
                }
                while (!AreAllAtTopSpeed(states));

                var (_, index) = states
                    .Select((state, index) => (particle: state.current, index))
                    .MinBy(p => Vector.Length(p.particle.Velocity));
                
                Console.WriteLine(index);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var particles = input.Lines().Select(Particle.Parse).ToList();

                var target = Vector.Zero;

                var states = particles
                    .Select(p => (current: p, previous: p))
                    .ToList();

                do
                {
                    AdvanceAll(states);
                    states = RemoveCollisions(states);
                }
                while (!AreAllMovingAwayFast(target, states));

                do
                {
                    AdvanceAll(states);
                    states = RemoveCollisions(states);
                }
                while (!AreAllAtTopSpeed(states));

                Console.WriteLine(states.Count);
            }
        }

        private static void AdvanceAll(List<(Particle current, Particle prev)> states)
        {
            for (var i = 0; i < states.Count; i++)
            {
                var (current, _) = states[i];
                var next = Particle.Advance(current);
                states[i] = (next, current);
            }
        }

        private static List<(Particle current, Particle prev)> RemoveCollisions(List<(Particle current, Particle prev)> states)
        {
            var collistionPositions = states
                .Select(s => s.current.Position)
                .GroupBy(p => p)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();

            return states
                .Where(s => !collistionPositions.Contains(s.current.Position))
                .ToList();
        }

        private static bool AreAllMovingAwayFast(Vector target, IReadOnlyList<(Particle current, Particle prev)> states) =>
            states
                .All(state =>
                {
                    var (current, prev) = state;

                    var movingAway =
                        Vector.ManhattanDistance(target, current.Position) >
                        Vector.ManhattanDistance(target, prev.Position);

                    var notAccelerating = current.Acceleration == Vector.Zero;

                    var movingFaster =
                        Vector.Length(current.Velocity) >
                        Vector.Length(prev.Velocity);

                    return movingAway && (notAccelerating || movingFaster);
                });

        private static bool AreAllAtTopSpeed(IReadOnlyList<(Particle current, Particle previous)> states)
        {
            var particles = states.Select(s => s.current);

            var nonAcceleratedParticles = particles
                .Where(p => p.Acceleration == Vector.Zero)
                .ToList();

            if (nonAcceleratedParticles.Count == 0)
            {
                return true;
            }

            var maxVelocityLength = nonAcceleratedParticles
                .Select(p => Vector.Length(p.Velocity))
                .Max();

            return particles
                .Where(p => p.Acceleration != Vector.Zero)
                .All(p => Vector.Length(p.Velocity) > maxVelocityLength);
        }

        private record Vector(int X, int Y, int Z)
        {
            public static readonly Vector Zero = new(0, 0, 0);

            public static Vector Parse(string text)
            {
                // 0123456789
                // a=<-1,0,0>

                var start = text.IndexOf('<');
                var end = text.IndexOf('>');

                var parts = text
                    .Substring(start + 1, end - start - 1)
                    .Split(',')
                    .Select(int.Parse)
                    .ToList();

                return new Vector(parts[0], parts[1], parts[2]);
            }

            public static Vector Add(Vector a, Vector b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

            public static int ManhattanDistance(Vector a, Vector b) =>
                Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z);

            public static double Length(Vector v) => 
                Math.Sqrt((long)v.X * v.X + (long)v.Y * v.Y + (long)v.Z * v.Z);
        }

        private record Particle(Vector Position, Vector Velocity, Vector Acceleration)
        {
            public static Particle Parse(string text)
            {
                var parts = text
                    .Split(", ")
                    .Select(Vector.Parse)
                    .ToList();

                return new Particle(parts[0], parts[1], parts[2]);
            }

            public static Particle Advance(Particle particle)
            {
                var v = Vector.Add(particle.Velocity, particle.Acceleration);
                var p = Vector.Add(particle.Position, v);
                return particle with { Velocity = v, Position = p };
            }
        }
    }
}
