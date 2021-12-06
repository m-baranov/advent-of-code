using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day06
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal("3,4,3,1,2");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/6/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var offsets = input.Lines().First().Split(',').Select(int.Parse).ToList();
                
                var sum = Simulation.Run(offsets, totalDays: 80);
                Console.WriteLine(sum);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var offsets = input.Lines().First().Split(',').Select(int.Parse).ToList();

                var sum = Simulation.Run(offsets, totalDays: 256);
                Console.WriteLine(sum);
            }
        }

        public static class Simulation
        {
            public static long Run(IReadOnlyList<int> initialOffsets, int totalDays)
            {
                var totalFish = (long)initialOffsets.Count;

                var fishClasses = SumByInstance(initialOffsets
                    .Select(offset => new FishClass(new FishInstance(offset, birthDay: 0), count: 1)));

                while (fishClasses.Count > 0)
                {
                    totalFish += fishClasses
                        .Select(fc => fc.Count * fc.Instance.ChildCountAfter(totalDays))
                        .Sum();

                    fishClasses = SumByInstance(fishClasses
                        .SelectMany(fc => fc.Instance.Children()
                            // -7 to take only those that will have at least one child in remaining time
                            .TakeWhile(child => child.BirthDay <= totalDays - 7)
                            .Select(child => new FishClass(child, fc.Count))
                        ));
                }

                return totalFish;
            }

            private static IReadOnlyList<FishClass> SumByInstance(IEnumerable<FishClass> fishClasses)
            {
                return fishClasses
                    .GroupBy(f => f.Instance)
                    .Select(g => new FishClass(g.Key, g.Select(g => g.Count).Sum()))
                    .ToList();
            }
        }

        public class FishInstance
        {
            public FishInstance(int offset, int birthDay)
            {
                Offset = offset;
                BirthDay = birthDay;
            }

            public int Offset { get; }
            public int BirthDay { get; }

            public int FirstChildBirthDay => BirthDay + Offset + 1;

            public int ChildCountAfter(int daysPassed) => (daysPassed - FirstChildBirthDay) / 7 + 1;

            public IEnumerable<FishInstance> Children()
            {
                // + 2 -- to account for first cycle delay
                return EnumerableExtensions
                    .Sequence(start: FirstChildBirthDay + 2, delta: 7)
                    .Select(birthDay => new FishInstance(offset: 6, birthDay: birthDay));
            }

            public override bool Equals(object obj) =>
                obj is FishInstance other ? Offset == other.Offset && BirthDay == other.BirthDay : false;

            public override int GetHashCode() => HashCode.Combine(Offset, BirthDay);
        }

        public class FishClass
        {
            public FishClass(FishInstance instance, long count)
            {
                Instance = instance;
                Count = count;
            }

            public FishInstance Instance { get; }
            public long Count { get; }
        }
    }
}
