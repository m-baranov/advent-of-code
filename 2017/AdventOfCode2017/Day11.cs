using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day11
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal("se,sw,se,sw,sw");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/11/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var directions = DirectionUtil.ParseMany(input.Lines().First());

                var from = Position.Origin;
                var to = from.Move(directions);

                var distance = Position.MinDistance(from, to);
                Console.WriteLine(distance);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var directions = DirectionUtil.ParseMany(input.Lines().First());

                var maxDistance = 0;
                var from = Position.Origin;
                var to = from;

                foreach (var direction in directions)
                {
                    to = to.Move(direction);

                    var distance = Position.MinDistance(from, to);
                    maxDistance = Math.Max(maxDistance, distance);
                }

                Console.WriteLine(maxDistance);
            }
        }

        private enum Direction { N, NE, SE, S, SW, NW }

        private static class DirectionUtil
        {
            public static IEnumerable<Direction> ParseMany(string text) =>
                text.Split(',').Select(Parse).ToList();

            public static Direction Parse(string text) =>
                text switch
                {
                    "n" => Direction.N,
                    "ne" => Direction.NE,
                    "se" => Direction.SE,
                    "s" => Direction.S,
                    "sw" => Direction.SW,
                    "nw" => Direction.NW,

                    _ => throw new Exception("unreachable"),
                };
        }

        private record Position(int X, int Y)
        {
            public static readonly Position Origin = new(0, 0);

            // https://www.redblobgames.com/grids/hexagons/#distances-doubled
            public static int MinDistance(Position a, Position b) 
            {
                static int X(Position p) => p.Y % 2 == 0 ? p.X * 2 : p.X * 2 + 1;

                var dx = Math.Abs(X(a) - X(b));
                var dy = Math.Abs(a.Y - b.Y);

                return dx + Math.Max(0, (dy - dx) / 2);
            }

            public Position Move(IEnumerable<Direction> directions) =>
                directions.Aggregate(this, (p, d) => p.Move(d));

            public Position Move(Direction direction) =>
                direction switch
                {
                    Direction.N => N(),
                    Direction.NE => NE(),
                    Direction.SE => SE(),
                    Direction.S => S(),
                    Direction.SW => SW(),
                    Direction.NW => NW(),

                    _ => throw new Exception("unreachable"),
                };

            public Position N() => this with { Y = Y + 2 };

            public Position S() => this with { Y = Y - 2 };

            public Position SW() =>
                Y % 2 == 0
                    ? this with { X = X - 1, Y = Y - 1 }
                    : this with { Y = Y - 1 };

            public Position SE() =>
                Y % 2 == 0
                    ? this with { Y = Y - 1 }
                    : this with { X = X + 1, Y = Y - 1 };

            public Position NW() =>
                Y % 2 == 0
                    ? this with { X = X - 1, Y = Y + 1 }
                    : this with { Y = Y + 1 };

            public Position NE() =>
                Y % 2 == 0
                    ? this with { Y = Y + 1 }
                    : this with { X = X + 1, Y = Y + 1 };
        }
    }
}
