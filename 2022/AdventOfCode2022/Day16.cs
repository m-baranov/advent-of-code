using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day16
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "Valve AA has flow rate=0; tunnels lead to valves DD, II, BB",
                    "Valve BB has flow rate=13; tunnels lead to valves CC, AA",
                    "Valve CC has flow rate=2; tunnels lead to valves DD, BB",
                    "Valve DD has flow rate=20; tunnels lead to valves CC, AA, EE",
                    "Valve EE has flow rate=3; tunnels lead to valves FF, DD",
                    "Valve FF has flow rate=0; tunnels lead to valves EE, GG",
                    "Valve GG has flow rate=0; tunnels lead to valves FF, HH",
                    "Valve HH has flow rate=22; tunnel leads to valve GG",
                    "Valve II has flow rate=0; tunnels lead to valves AA, JJ",
                    "Valve JJ has flow rate=21; tunnel leads to valve II"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/16/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var map = Map.Parse(input.Lines());

                var maxPressure = new Solution().Solve(map, time: 30);
                Console.WriteLine(maxPressure);
            }
        }

        public class Part2 : IProblem
        {
            // Slow :(
            public void Run(TextReader input)
            {
                const int Time = 26;

                var map = Map.Parse(input.Lines());

                var openableValves = map.OpenableValves().ToList();

                // warm cache
                foreach (var valve in openableValves)
                {
                    map.WalkTimesFrom(valve);
                }

                // Not sure how correct this estimation is, but cuts down on search time from
                // ~4.5 min to ~1.5 min on my notebook.
                var minWork = openableValves.Count / 2 - 1;
                var maxWork = openableValves.Count / 2 + 1;
                
                var splits = Combinatorics.AllPossibleSplits(openableValves)
                    .Where(p => (minWork <= p.left.Count && p.left.Count <= maxWork) ||
                                (minWork <= p.right.Count && p.right.Count <= maxWork))
                    .ToList();

                var maxPressure = splits.AsParallel()
                    .Select(split =>
                    {
                        var leftPressure = new Solution().Solve(map.WithClosedValves(split.left), Time);
                        var rightPressure = new Solution().Solve(map.WithClosedValves(split.right), Time);
                        return leftPressure + rightPressure;
                    })
                    .Aggregate(-1, Math.Max);

                Console.WriteLine(maxPressure);
            }
        }

        private record Valve(string Name, int Rate, IReadOnlyList<string> ConnectsTo)
        {
            public static Valve Parse(string text)
            {
                // Valve DD has flow rate=20; tunnels lead to valves CC, AA, EE

                const string NamePrefix = "Valve ";
                const string FlowRatePrefix = " has flow rate=";
                const string ConnectionPrefix = "; tunnel leads to valve ";
                const string ConnectionsPrefix = "; tunnels lead to valves ";

                var parts = text.Substring(NamePrefix.Length).Split(FlowRatePrefix);

                var name = parts[0];

                parts = parts[1].Split(parts[1].Contains(ConnectionsPrefix) ? ConnectionsPrefix : ConnectionPrefix);

                var flowRate = int.Parse(parts[0]);
                var connectsTo = parts[1].Split(", ").ToList();

                return new Valve(name, flowRate, connectsTo);
            }

            public bool IsOpenable => this.Rate > 0;
        }

        private class Map
        {
            public const string StartName = "AA";

            public static Map Parse(IEnumerable<string> lines)
            {
                var valves = lines.Select(Valve.Parse).ToList();
                return new Map(valves);
            }

            private readonly IReadOnlyDictionary<string, Valve> valvesByName;
            private readonly Dictionary<string, IReadOnlyDictionary<string, int>> timesCache;

            public Map(IReadOnlyList<Valve> valves)
                : this(valves, timesCache: new Dictionary<string, IReadOnlyDictionary<string, int>>())
            {
            }

            private Map(IReadOnlyList<Valve> valves, Dictionary<string, IReadOnlyDictionary<string, int>> timesCache)
            {
                this.valvesByName = valves.ToDictionary(v => v.Name);
                this.timesCache = timesCache;
                this.OpenableValveCount = valves.Count(v => v.IsOpenable);
            }

            public int OpenableValveCount { get; }

            public IEnumerable<string> OpenableValves() => 
                valvesByName.Values.Where(v => v.IsOpenable).Select(v => v.Name);

            public Valve Get(string name) => this.valvesByName[name];

            public IReadOnlyDictionary<string, int> WalkTimesFrom(string fromValve)
            {
                IReadOnlyDictionary<string, int> Calculate(string fromValue)
                {
                    var queue = new Queue<string>();
                    queue.Enqueue(fromValve);

                    var visits = new Dictionary<string, int>();
                    visits.Add(fromValve, 0);

                    var openables = new HashSet<string>();

                    while (queue.Count > 0)
                    {
                        var valve = queue.Dequeue();
                        var time = visits[valve];

                        foreach (var connectedValve in Get(valve).ConnectsTo)
                        {
                            if (visits.TryGetValue(connectedValve, out var previosTime) &&
                                previosTime <= time + 1)
                            {
                                continue;
                            }

                            visits[connectedValve] = time + 1;

                            queue.Enqueue(connectedValve);
                        }
                    }

                    return visits;
                }

                if (this.timesCache.TryGetValue(fromValve, out var cached))
                {
                    return cached;
                }

                var times = Calculate(fromValve);
                this.timesCache[fromValve] = times;
                return times;
            }

            public Map WithClosedValves(IReadOnlyList<string> closedValves)
            {
                var valves = this.valvesByName.Values
                    .Select(v => closedValves.Contains(v.Name) ? v with { Rate = 0 } : v)
                    .ToList();
                return new Map(valves, this.timesCache);
            }
        }

        private class Solution
        {
            private int MaxPressure;

            public int Solve(Map map, int time)
            {
                var state = State.Initial(map, time);

                this.MaxPressure = -1;
                Recurse(state);
                
                return this.MaxPressure;
            }

            private void Recurse(State state)
            {
                if (state.RemainingTime < 0)
                {
                    return;
                }

                this.MaxPressure = Math.Max(this.MaxPressure, state.Pressure());

                if (state.RemainingTime == 0 || state.IsAllValvesOpened())
                {
                    return;
                }

                if (state.IsValveOpenable(state.CurrentValve))
                {
                    Recurse(state.WithOpenedValve(state.CurrentValve));
                    return;
                }

                var destinations = state.DestinationsFromCurrentValve();
                foreach (var (nextValve, time) in destinations)
                {
                    Recurse(state.WithCurrentValve(nextValve, time));
                }
            }

            private record State(
                Map Map,
                int InitialTime,
                int RemainingTime,
                string CurrentValve,
                IReadOnlyList<(string valve, int time)> OpenValves)
            {
                public static State Initial(Map map, int time) =>
                    new(
                        Map: map,
                        InitialTime: time,
                        RemainingTime: time,
                        CurrentValve: Map.StartName,
                        OpenValves: Array.Empty<(string, int)>()
                    );

                public bool IsAllValvesOpened() => this.OpenValves.Count == this.Map.OpenableValveCount;

                public bool IsValveOpenable(string valve) =>
                    !this.IsValveOpen(valve) && this.Map.Get(valve).IsOpenable;

                public bool IsValveOpen(string valve) => OpenValves.Any(p => p.valve == valve);

                public IReadOnlyList<(string valve, int time)> DestinationsFromCurrentValve()
                {
                    var times = this.Map.WalkTimesFrom(this.CurrentValve);

                    return this.Map.OpenableValves()
                        .Where(v => !this.IsValveOpen(v))
                        .Select(valve => (valve, time: times[valve]))
                        .Where(p => p.time > 0)
                        .ToList();
                }

                public State WithOpenedValve(string valve)
                {
                    return this with
                    {
                        RemainingTime = RemainingTime - 1,
                        OpenValves = OpenValves.Append((valve, RemainingTime - 1)).ToList()
                    };
                }

                public State WithCurrentValve(string valve, int timeCost)
                {
                    return this with
                    {
                        RemainingTime = RemainingTime - timeCost,
                        CurrentValve = valve
                    };
                }

                public int Pressure() =>
                    this.OpenValves
                        .Select(p =>
                        {
                            var rate = this.Map.Get(p.valve).Rate;
                            return rate * p.time;
                        })
                        .Sum();
            }
        }

        private static class Combinatorics
        {
            public static void PrintAllPossibleSplits(IReadOnlyList<string> items)
            {
                foreach (var (left, right) in AllPossibleSplits(items))
                {
                    var l = string.Join(',', left);
                    var r = string.Join(',', right);
                    Console.WriteLine($"l={l}, r={r}");
                }
            }
            
            public static IReadOnlyList<(IReadOnlyList<string> left, IReadOnlyList<string> right)> AllPossibleSplits(IReadOnlyList<string> items)
            {
                static IEnumerable<(IReadOnlyList<string>, IReadOnlyList<string>)> EnumerateSplits(IReadOnlyList<string> items)
                {
                    var seen = new HashSet<IReadOnlyList<string>>(new ListComparer());

                    foreach (var left in AllPossibleSelections(items))
                    {
                        var right = items.Except(left).ToList();

                        if (seen.Contains(left) || seen.Contains(right))
                        {
                            continue;
                        }

                        seen.Add(left);
                        yield return (left, right);
                    }
                }

                return EnumerateSplits(items).ToList();
            }

            private sealed class ListComparer : IEqualityComparer<IReadOnlyList<string>>
            {
                public bool Equals(IReadOnlyList<string> x, IReadOnlyList<string> y) => x.SequenceEqual(y);

                public int GetHashCode([DisallowNull] IReadOnlyList<string> xs)
                {
                    if (xs.Count == 0)
                    {
                        return 0;
                    }
                    
                    return xs.Select(x => x.GetHashCode()).Aggregate(HashCode.Combine);
                }
            }

            public static IEnumerable<IReadOnlyList<string>> AllPossibleSelections(IReadOnlyList<string> items)
            {
                var list = new List<IReadOnlyList<string>>();

                void Recurse(IReadOnlyList<string> items, int index, List<string> result)
                {
                    if (index == items.Count)
                    {
                        list.Add(result.ToArray());
                        return;
                    }

                    Recurse(items, index + 1, result);

                    result.Add(items[index]);
                    Recurse(items, index + 1, result);
                    result.RemoveAt(result.Count - 1);
                }

                Recurse(items, index: 0, result: new List<string>());
                
                return list;
            }
        }
    }
}
