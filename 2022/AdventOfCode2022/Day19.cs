using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day19
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "Blueprint 1: Each ore robot costs 4 ore. Each clay robot costs 2 ore. Each obsidian robot costs 3 ore and 14 clay. Each geode robot costs 2 ore and 7 obsidian.",
                    "Blueprint 2: Each ore robot costs 2 ore. Each clay robot costs 3 ore. Each obsidian robot costs 3 ore and 8 clay. Each geode robot costs 3 ore and 12 obsidian."
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/19/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                const int time = 24;

                var blueprints = input.Lines().Select(Blueprint.Parse).ToList();
                
                var sum = blueprints
                    .Select(b => (id: b.Id, geodes: MaxGeodesProduced(time, b)))
                    .Sum(p => p.id * p.geodes);

                Console.WriteLine(sum);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                const int time = 32;

                var blueprints = input.Lines().Select(Blueprint.Parse).ToList();

                var mul = blueprints
                    .Take(3)
                    .Select(b => MaxGeodesProduced(time, b))
                    .Aggregate(1, (acc, geodes) => acc * geodes);

                Console.WriteLine(mul);
            }
        }

        private static int MaxGeodesProduced(int time, Blueprint blueprint)
        {
            var geodeRobot = blueprint.Robots.First(r => r.Produces == Resource.Geode);

            var maxOreRobots = blueprint.Robots.Select(robot => robot.Cost.Get(Resource.Ore)).Max();

            int Recurse(State state, Resource minResource)
            {
                if (state.Time == 0)
                {
                    return state.Resources.Get(Resource.Geode);
                }

                if (state.Resources.GreaterOrEqualTo(geodeRobot.Cost))
                {
                    return Recurse(state.Tick(geodeRobot), default);
                }

                var resources = new List<Resource>();

                if (state.Robots.Get(Resource.Ore) < maxOreRobots)
                {
                    resources.Add(Resource.Ore);
                }
                resources.Add(Resource.Clay);
                resources.Add(Resource.Obsidian);
                resources.Add(Resource.Geode);

                resources = resources
                    .Where(r => (int)r >= (int)minResource)
                    .ToList();

                var robotsCanBuild = blueprint.Robots
                    .Where(robot => resources.Contains(robot.Produces))
                    .Where(robot => state.Resources.GreaterOrEqualTo(robot.Cost))
                    .ToList();

                var max = 0;

                foreach (var robot in robotsCanBuild)
                {
                    max = Math.Max(max, Recurse(state.Tick(robot), default));
                }

                var firstNotBuilt = resources.FirstOrDefault(r => !robotsCanBuild.Any(rb => rb.Produces == r));

                max = Math.Max(max, Recurse(state.Tick(), firstNotBuilt));
                return max;
            }

            var state = State.Initial(time);
            return Recurse(state, default);
        }

        private record State(int Time, ResourceVector Robots, ResourceVector Resources)
        {
            public static State Initial(int time)
            {
                var robots = ResourceVector.Zero.Inc(Resource.Ore);
                var resources = ResourceVector.Zero;
                return new State(Time: time, Robots: robots, Resources: resources);
            }

            public State Tick()
            {
                var nextResources = Resources.Add(Robots);
                return new State(Time: Time - 1, Robots: Robots, Resources: nextResources);
            }

            public State Tick(RobotSpec robotToProduce)
            {
                var nextResources = Resources.Sub(robotToProduce.Cost).Add(Robots);
                var nextRobots = Robots.Inc(robotToProduce.Produces);
                return new State(Time: Time - 1, Robots: nextRobots, Resources: nextResources);
            }
        }

        private record Blueprint(int Id, IReadOnlyList<RobotSpec> Robots)
        {
            public static Blueprint Parse(string text)
            {
                static int ParseBlueprintId(string text)
                {
                    const string Prefix = "Blueprint ";
                    return int.Parse(text.Substring(Prefix.Length));
                }

                static Resource ParseResource(string text) =>
                    text switch
                    {
                        "ore" => Resource.Ore,
                        "clay" => Resource.Clay,
                        "obsidian" => Resource.Obsidian,
                        "geode" => Resource.Geode,
                        _ => throw new Exception($"Unknown resource '{text}'.")
                    };

                static (Resource resource, int amount) ParseCost(string text)
                {
                    var parts = text.Split(' ');

                    var amount = int.Parse(parts[0]);
                    var resource = ParseResource(parts[1]);

                    return (resource, amount);
                }

                static RobotSpec ParseRobotSpec(string text)
                {
                    const string Prefix = "Each ";
                    const string Separator = " robot costs ";

                    var parts = text.Substring(Prefix.Length).Split(Separator);

                    var produces = ParseResource(parts[0]);
                    var costs = parts[1].Split(" and ").Select(ParseCost).ToList();

                    return new RobotSpec(produces, ResourceVector.OfCosts(costs));
                }
                
                var parts = text.Split(':');

                var id = ParseBlueprintId(parts[0]);
                var specs = parts[1].Split('.', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Select(ParseRobotSpec)
                    .ToList();

                return new Blueprint(id, specs);
            }
        }

        private record RobotSpec(Resource Produces, ResourceVector Cost); 

        private enum Resource { Ore, Clay, Obsidian, Geode };

        private sealed class ResourceVector
        {
            private const int ResourceCount = 4;

            public static readonly ResourceVector Zero = new(new int[ResourceCount]);

            public static ResourceVector OfCosts(IReadOnlyList<(Resource resource, int amount)> costs)
            {
                var values = new int[ResourceCount];

                foreach (var cost in costs)
                {
                    values[(int)cost.resource] += cost.amount;
                }

                return new ResourceVector(values);
            }

            private readonly IReadOnlyList<int> values;

            private ResourceVector(IReadOnlyList<int> values)
            {
                this.values = values;
            }

            public int Get(Resource resource) => this.values[(int)resource];

            public ResourceVector Inc(Resource resource, int by = 1)
            {
                var values = this.values.ToArray();
                values[(int)resource] += by;
                return new ResourceVector(values);
            }

            public ResourceVector Add(ResourceVector other)
            {
                var values = new int[ResourceCount];
                for (var i = 0; i < ResourceCount; i++)
                {
                    values[i] = this.values[i] + other.values[i];
                }
                return new ResourceVector(values);
            }

            public ResourceVector Sub(ResourceVector other)
            {
                var values = new int[ResourceCount];
                for (var i = 0; i < ResourceCount; i++)
                {
                    values[i] = this.values[i] - other.values[i];
                }
                return new ResourceVector(values);
            }

            public ResourceVector Mul(int times)
            {
                var values = new int[ResourceCount];
                for (var i = 0; i < ResourceCount; i++)
                {
                    values[i] = this.values[i] * times;
                }
                return new ResourceVector(values);
            }

            public bool GreaterOrEqualTo(ResourceVector other)
            {
                for (var i = 0; i < ResourceCount; i++)
                {
                    if (this.values[i] < other.values[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            public override bool Equals(object obj) => obj is ResourceVector other && Equals(other);

            public bool Equals(ResourceVector other)
            {
                for (var i = 0; i < ResourceCount; i++)
                {
                    if (this.values[i] != other.values[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode() => this.values.Aggregate(HashCode.Combine);

            public override string ToString() => string.Join(',', this.values);
        }
    }
}
