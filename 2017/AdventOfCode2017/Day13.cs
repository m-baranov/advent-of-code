using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day13
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "0: 3",
                    "1: 2",
                    "4: 4",
                    "6: 4"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/13/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var layers = input.Lines().Select(Layer.Parse).ToList();

                var severity = layers
                    .Where(l => l.ScannerPosition(l.Depth) == 0)
                    .Select(l => l.Depth * l.Range)
                    .Sum();

                Console.WriteLine(severity);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var layers = input.Lines().Select(Layer.Parse).ToList();

                var delay = 0;
                while (true)
                {
                    var caught = layers.Any(l => l.ScannerPosition(l.Depth + delay) == 0);
                    if (!caught)
                    {
                        break;
                    }
                    delay++;
                }

                Console.WriteLine(delay);
            }
        }

        private record Layer(int Depth, int Range)
        {
            public static Layer Parse(string text)
            {
                var parts = text.Split(": ");

                var depth = int.Parse(parts[0]);
                var range = int.Parse(parts[1]);

                return new Layer(depth, range);
            }

            public int ScannerPosition(int time)
            {
                var period = this.Range * 2 - 2;
                var p = time % period;
                return p < this.Range ? p : period - p;
            }
        }
    }
}
