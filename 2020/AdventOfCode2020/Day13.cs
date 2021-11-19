using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2020
{
    static class Day13
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "939",
                "7,13,x,x,59,x,31,19"
            );

        public static readonly IInput SampleInput2 =
            Input.Literal(
                "0",
                "17,x,13,19"
            );

        public static readonly IInput SampleInput3 =
            Input.Literal(
                "0",
                "67,7,59,61"
            );

        public static readonly IInput SampleInput4 =
            Input.Literal(
                "0",
                "67,x,7,59,61"
            );

        public static readonly IInput SampleInput5 =
            Input.Literal(
                "0",
                "67,7,x,59,61"
            );

        public static readonly IInput SampleInput6 =
            Input.Literal(
                "0",
                "1789,37,47,1889"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/13/input");

        public const long SampleInitialTime = 0;
        public const long TestInitialTime = 100000000000000;

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var lines = input.Lines().ToList();

                var time = int.Parse(lines[0]);
                var schedule = lines[1].Split(',').Where(p => p != "x").Select(int.Parse).ToList();

                var solution = schedule
                    .Select(t =>
                    {
                        var late = time - (time / t) * t;
                        var wait = t - late;
                        return new { bus = t, wait };
                    })
                    .OrderBy(p => p.wait)
                    .First();

                Console.WriteLine(solution.bus * solution.wait);
            }
        }

        public class Part2_Old1 : IProblem
        {
            private readonly long initialTime;

            public Part2_Old1(long initialTime)
            {
                this.initialTime = initialTime;
            }

            public void Run(TextReader input)
            {
                var lines = input.Lines().ToList();

                var schedule = lines[1].Split(',')
                    .Select(t => t == "x" ? (int?)null : int.Parse(t))
                    .ToList();

                var time = initialTime;

                var ring = new Ring<IReadOnlyList<bool>>(
                    Util.Range(time, count: schedule.Count)
                        .Select(time => DepartsAt(schedule, time))
                        .ToArray());

                while (true)
                {
                    if (Matches(ring.AsEnumerable(), schedule))
                    {
                        break;
                    }

                    time++;
                    ring.Add(DepartsAt(schedule, time));
                }

                Console.WriteLine(time - schedule.Count + 1);
            }

            private IReadOnlyList<bool> DepartsAt(IReadOnlyList<int?> schedule, long time)
            {
                return schedule.Select(b => b == null ? false : (time % b.Value) == 0).ToList();
            }

            private bool Matches(IEnumerable<IReadOnlyList<bool>> departures, IReadOnlyList<int?> schedule)
            {
                var index = 0;
                foreach (var row in departures)
                {
                    var stillMatches = schedule[index] != null ? row[index] == true : true;
                    if (!stillMatches)
                    {
                        return false;
                    }

                    index++;
                }

                return true;
            }

            public class Ring<T>
            {
                private readonly T[] _items;
                private int _start;

                public Ring(T[] initial)
                {
                    _items = new T[initial.Length];
                    initial.CopyTo(_items, 0);
                    _start = 0;
                }

                public void Add(T item)
                {
                    _items[_start] = item;
                    _start = (_start + 1) % _items.Length;
                }

                public IEnumerable<T> AsEnumerable()
                {
                    for (var i = 0; i < _items.Length; i++)
                    {
                        var index = (_start + i) % _items.Length;
                        yield return _items[index];
                    }
                }
            }

            public static class Util
            {
                public static IEnumerable<long> Range(long start, long count)
                {
                    for (var i = 0; i < count; i++)
                    {
                        yield return start + i;
                    }
                }
            }
        }

        public class Part2_Old2 : IProblem
        {
            private readonly long initialTime;

            public Part2_Old2(long initialTime)
            {
                this.initialTime = initialTime;
            }

            public void Run(TextReader input)
            {
                var lines = input.Lines().ToList();

                var schedule = lines[1].Split(',')
                    .Select((t, i) => (t, i))
                    .Where(p => p.t != "x")
                    .Select(p => (bus: int.Parse(p.t), index: p.i))
                    .OrderByDescending(p => p.bus)
                    .ToList();

                var constraints = schedule
                    .Select(p => (bus: p.bus, rem: (p.bus - p.index) % p.bus))
                    .ToList();

                var maxBus = schedule[0];
                var time = initialTime;

                while (true)
                {
                    if (Matches(constraints, time - maxBus.index))
                    {
                        break;
                    }

                    time += maxBus.bus; // schedule[0].bus;
                }

                Console.WriteLine(time - maxBus.index);
            }

            private bool Matches(IReadOnlyList<(int bus, int rem)> schedule, long time)
            {
                for (var i = 0; i < schedule.Count; i++)
                {
                    var (bus, rem) = schedule[i];
                    if (time % bus != rem)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var lines = input.Lines().ToList();

                var schedule = lines[1].Split(',')
                    .Select((t, i) => (t, i))
                    .Where(p => p.t != "x")
                    .Select(p => (bus: (long)int.Parse(p.t), index: (long)p.i))
                    .ToList();

                var time = 0L;

                var b1 = schedule[0];
                for (var i = 1; i < schedule.Count; i++)
                {
                    var b2 = schedule[i];

                    while (true) 
                    {
                        if (((time + b1.index) % b1.bus == 0) && 
                            ((time + b2.index) % b2.bus == 0))
                        {
                            break;
                        }

                        time += b1.bus;
                    }

                    b1 = (bus: b1.bus * b2.bus, index: -time);
                }

                Console.WriteLine(time);
            }
        }
    }
}
