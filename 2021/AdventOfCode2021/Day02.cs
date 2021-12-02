using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day02
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "forward 5",
                    "down 5",
                    "forward 8",
                    "up 3",
                    "down 8",
                    "forward 2"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/2/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var commands = input.Lines().Select(Command.Parse);

                var location = commands.Aggregate(Location.Initial, (loc, cmd) => loc.Apply(cmd));

                Console.WriteLine(location.HPosition * location.Depth);
            }

            public class Location
            {
                public static readonly Location Initial = new Location(hPosition: 0, depth: 0);

                public Location(long hPosition, long depth)
                {
                    HPosition = hPosition;
                    Depth = depth;
                }

                public long HPosition { get; }
                public long Depth { get; }

                public Location Apply(Command cmd)
                {
                    if (cmd.Direction == Direction.Forward)
                    {
                        return new Location(HPosition + cmd.Units, Depth);
                    }
                    else if (cmd.Direction == Direction.Down)
                    {
                        return new Location(HPosition, Depth + cmd.Units);
                    }
                    else
                    {
                        return new Location(HPosition, Depth - cmd.Units);
                    }
                }
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var commands = input.Lines().Select(Command.Parse);

                var location = commands.Aggregate(Location.Initial, (loc, cmd) => loc.Apply(cmd));

                Console.WriteLine(location.HPosition * location.Depth);
            }

            public class Location
            {
                public static readonly Location Initial = new Location(hPosition: 0, depth: 0, aim: 0);

                public Location(long hPosition, long depth, long aim)
                {
                    HPosition = hPosition;
                    Depth = depth;
                    Aim = aim;
                }

                public long HPosition { get; }
                public long Depth { get; }
                public long Aim { get; }

                public Location Apply(Command cmd)
                {
                    if (cmd.Direction == Direction.Forward)
                    {
                        return new Location(HPosition + cmd.Units, Depth + Aim * cmd.Units, Aim);
                    }
                    else if (cmd.Direction == Direction.Down)
                    {
                        return new Location(HPosition, Depth, Aim + cmd.Units);
                    }
                    else
                    {
                        return new Location(HPosition, Depth, Aim - cmd.Units);
                    }
                }
            }
        }

        public enum Direction { Forward, Up, Down }

        public class Command
        {
            public static Command Parse(string text)
            {
                var parts = text.Split(' ');

                var direction = ParseDirection(parts[0]);
                var units = int.Parse(parts[1]);

                return new Command(direction, units);
            }

            private static Direction ParseDirection(string text) =>
                text switch
                {
                    "forward" => Direction.Forward,
                    "down" => Direction.Down,
                    _ => Direction.Up
                };

            public Command(Direction direction, int units)
            {
                Direction = direction;
                Units = units;
            }

            public Direction Direction { get; }
            public int Units { get; }
        }
    }
}
