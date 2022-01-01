using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Computer = AdventOfCode2019.Day09.Computer;

namespace AdventOfCode2019
{
    static class Day19
    {
        public static readonly IInput SampleInput =
            Input.Literal();

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/19/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var probe = new Probe(input.Lines().First());

                var count = Coordinates().Where(probe.IsAffected).Count();

                Console.WriteLine(count);
            }

            private static IEnumerable<(int x, int y)> Coordinates()
            {
                var values = Enumerable.Range(0, 50);

                return from x in values
                       from y in values
                       select (x, y);
            }
        }

        public class Part2 : IProblem
        {
            private const int SquareLength = 100;

            public void Run(TextReader input)
            {
                // Slow, but works. Without approximation takes twice as long.

                var probe = new Probe(input.Lines().First());

                var approxY = EstimateApproxY(probe);
                var square = FindExactXY(probe, startY: approxY - SquareLength);

                Console.WriteLine(square.x * 10000 + square.y);
            }

            private static int EstimateApproxY(Probe probe)
            {
                var sample = AffectedLines(probe, startY: 0)
                    .Where(p => p.xs != null)
                    .Take(50) // from part 1
                    .ToList();

                var avgLengthIncrease = sample.Pairwise()
                    .Select(pair => pair.second.xs.Length() - pair.first.xs.Length())
                    .Average();

                var avgStartIncrease = sample.Pairwise()
                    .Select(pair => pair.second.xs.Start - pair.first.xs.Start)
                    .Average();

                return (int)((SquareLength * avgStartIncrease + SquareLength) / avgLengthIncrease);
            }

            private static (int x, int y) FindExactXY(Probe probe, int startY)
            {
                var squareLines = new Ring<(int y, Range xs)>(capacity: SquareLength);

                var square = (x: -1, y: -1);

                foreach (var (y, beamLine) in AffectedLines(probe, startY))
                {
                    if (beamLine == null)
                    {
                        continue;
                    }

                    if (beamLine.Length() < SquareLength)
                    {
                        continue;
                    }

                    squareLines.Add((y, beamLine));

                    if (squareLines.Count == SquareLength)
                    {
                        var (topY, topLine) = squareLines.At(0);
                        var (bottomY, bottomLine) = squareLines.At(squareLines.Count - 1);

                        var squareSide = new Range(topLine.End - SquareLength, topLine.End);
                        if (bottomLine.Contains(squareSide))
                        {
                            square = (squareSide.Start, topY);
                            break;
                        }

                        squareSide = new Range(bottomLine.Start, bottomLine.Start + SquareLength);
                        if (topLine.Contains(squareSide))
                        {
                            square = (squareSide.Start, topY);
                            break;
                        }
                    }
                }

                return square;
            }

            private static IEnumerable<(int y, Range xs)> AffectedLines(Probe probe, int startY)
            {
                for (var y = startY; y < int.MaxValue; y++)
                {
                    // Some arbitrary value to limit search in cases when there is no
                    // affected points on a given line.
                    var maxX = y * 100;

                    var x = 0;
                    while (x < maxX && !probe.IsAffected((x, y)))
                    {
                        x++;
                    }

                    var startX = x < maxX ? x : -1;

                    while (x < maxX && probe.IsAffected((x, y)))
                    {
                        x++;
                    }

                    var endX = x < maxX ? x : -1;

                    if (startX == -1 || endX == -1)
                    {
                        yield return (y, null);
                    }
                    else
                    {
                        yield return (y, new Range(startX, endX));
                    }
                }
            }
        }

        private class Probe
        {
            private readonly string program;

            public Probe(string program)
            {
                this.program = program;
            }

            public bool IsAffected((int x, int y) coordinates)
            {
                var computer = Computer.Of(program, new[] { (long)coordinates.x, coordinates.y });
                computer.Execute();
                return computer.Output.Values().Last() == 1;
            }
        }

        private class Range
        {
            public Range(int start, int end)
            {
                Start = start;
                End = end;
            }

            public int Start { get; }
            public int End { get; }

            public int Length() => End - Start;

            public bool Contains(Range other) => Start <= other.Start && other.End <= End;
        }

        private class Ring<T>
        {
            private readonly int capacity;
            private readonly List<T> items;
            private int offset;

            public Ring(int capacity)
            {
                this.capacity = capacity;
                this.items = new List<T>(capacity: capacity);
                this.offset = 0;
            }

            public int Count => this.items.Count;

            public T At(int index)
            {
                if (index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                var actual = (index + offset) % capacity;
                return items[actual];
            }

            public void Add(T item)
            {
                if (items.Count < capacity)
                {
                    items.Add(item);
                    return;
                }

                items[offset] = item;
                    
                offset++;
                if (offset >= capacity)
                {
                    offset = 0;
                }
            }

            public IEnumerable<T> AsEnumerable()
            {
                for (var i = 0; i < Count; i++)
                {
                    yield return At(i);
                }
            }
        }
    }
}
