using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day05
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
seeds: 79 14 55 13

seed-to-soil map:
50 98 2
52 50 48

soil-to-fertilizer map:
0 15 37
37 52 2
39 0 15

fertilizer-to-water map:
49 53 8
0 11 42
42 0 7
57 7 4

water-to-light map:
88 18 7
18 25 70

light-to-temperature map:
45 77 23
81 45 19
68 64 13

temperature-to-humidity map:
0 69 1
1 0 69

humidity-to-location map:
60 56 37
56 93 4
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/5/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var almanac = Almanac.Parse(input.Lines().ToList());

            var maps = almanac.BuildMapOrder(startTag: "seed", endTag: "location");

            var lowest = almanac.Seeds
                .Select(seed => ApplyInOrder(seed, maps))
                .Min();

            Console.WriteLine(lowest);
        }

        private static long ApplyInOrder(long source, IReadOnlyList<Map> maps) =>
            maps.Aggregate(source, Apply);

        private static long Apply(long source, Map map)
        {
            static bool InSourceRange(MapRange range, long value) =>
                range.SourceStart <= value && value < range.SourceStart + range.Length;

            var range = map.Ranges.FirstOrDefault(range => InSourceRange(range, source));
            if (range is null)
            {
                return source;
            }

            return range.MapToDestination(source);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var almanac = Almanac.Parse(input.Lines().ToList());

            var maps = almanac.BuildMapOrder(startTag: "seed", endTag: "location");

            var seedRanges = RangesOf(almanac.Seeds).ToList();

            var lowest = ApplyInOrder(seedRanges, maps)
                .Select(range => range.Start)
                .Min();

            Console.WriteLine(lowest);
        }

        private static IEnumerable<Range> RangesOf(IReadOnlyList<long> items)
        {
            for (var i = 0; i < items.Count / 2; i++)
            {
                yield return new Range(items[i * 2], items[i * 2 + 1]);
            }
        }

        private static IReadOnlyList<Range> ApplyInOrder(IReadOnlyList<Range> source, IReadOnlyList<Map> maps) =>
            maps.Aggregate(source, ApplyOnce);

        private static IReadOnlyList<Range> ApplyOnce(IReadOnlyList<Range> ranges, Map map) =>
            ranges.SelectMany(range => Apply(range, map)).ToList();

        private static IReadOnlyList<Range> Apply(Range range, Map map)
        {
            var remaining = range;
            var applied = new List<Range>();

            foreach (var mapRange in map.Ranges.OrderBy(r => r.SourceStart))
            {
                var source = new Range(mapRange.SourceStart, mapRange.Length);

                var intersection = Range.Intersect(remaining, source);
                if (intersection is null)
                {
                    continue;
                }

                if (remaining.Start < source.Start)
                {
                    applied.Add(new Range(
                        Start: remaining.Start, 
                        Length: source.Start - remaining.Start
                    ));
                }

                applied.Add(new Range(
                    Start: mapRange.MapToDestination(intersection.Start),
                    Length: intersection.Length
                ));

                if (remaining.End <= source.End)
                {
                    break;
                }
                else
                {
                    remaining = new Range(
                        Start: source.End + 1, 
                        Length: remaining.End - source.End
                    );
                }
            }

            if (applied.Count == 0)
            {
                applied.Add(range);
            }

            return applied;
        }

        private record Range(long Start, long Length)
        {
            public static bool HitTest(Range a, Range b) =>
                !(a.End < b.Start || b.End < a.Start);
            
            public static Range? Intersect(Range a, Range b)
            {
                if (!HitTest(a, b))
                {
                    return null;
                }

                var start = Math.Max(a.Start, b.Start);
                var end = Math.Min(a.End, b.End);

                return new Range(start, Length: end - start + 1);
            }

            public long End => Start + Length - 1;
        }
    }

    private record MapRange(long DestinationStart, long SourceStart, long Length)
    {
        public static MapRange Parse(string text)
        {
            var parts = text.Split(' ');
            return new(
                DestinationStart: long.Parse(parts[0]),
                SourceStart: long.Parse(parts[1]),
                Length: long.Parse(parts[2])
            );
        }

        public long MapToDestination(long value) =>
            value - SourceStart + DestinationStart;
    }

    private record Map(string SourceTag, string DestinationTag, IReadOnlyList<MapRange> Ranges)
    {
        public static Map Parse(IReadOnlyList<string> lines)
        {
            static (string sourceTag, string destinationTag) ParseTitle(string text) => 
                SplitBy(TrimSuffix(text, " map:"), "-to-");

            var (sourceTag, destinationTag) = ParseTitle(lines[0]);
            var ranges = lines.Skip(1).Select(MapRange.Parse).ToList();

            return new Map(sourceTag, destinationTag, ranges);
        }
    }

    private record Almanac(IReadOnlyList<long> Seeds, IReadOnlyList<Map> Maps)
    {
        public static Almanac Parse(IReadOnlyList<string> lines)
        {
            static IReadOnlyList<long> ParseSeeds(string text) =>
                TrimPrefix(text, "seeds: ").Split(' ').Select(long.Parse).ToList();

            var seeds = ParseSeeds(lines[0]);
            var maps = lines.Skip(2).SplitByEmptyLine().Select(Map.Parse).ToList();

            return new Almanac(seeds, maps);
        }

        public IReadOnlyList<Map> BuildMapOrder(string startTag, string endTag)
        {
            var maps = new List<Map>();
            var tag = endTag;

            while (tag != startTag)
            {
                var map = Maps.First(m => m.DestinationTag == tag);
                maps.Add(map);
                tag = map.SourceTag;
            }

            maps.Reverse();
            return maps;
        }
    }

    private static (string left, string right) SplitBy(string text, string sep)
    {
        var index = text.IndexOf(sep);
        return (text.Substring(0, index), text.Substring(index + sep.Length));
    }

    private static string TrimPrefix(string text, string prefix) =>
        text.Substring(prefix.Length);

    private static string TrimSuffix(string text, string suffix) =>
        text.Substring(0, text.Length - suffix.Length);
}
