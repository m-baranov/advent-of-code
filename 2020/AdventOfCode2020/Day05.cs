using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2020
{
    static class Day05
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "BFFFBBFRRR",
                "FFFBBBFRRR",
                "BBFFBBFRLL"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/5/input");


        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var highestSeatId = input.Lines().Select(Util.Parse).Select(p => p.seatId).Max();
                Console.WriteLine(highestSeatId);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var seatIds = input.Lines().Select(Util.Parse).Select(p => p.seatId);

                var (min, max, sum) = seatIds.Aggregate(
                    (min: long.MaxValue, max: long.MinValue, sum: 0),
                    (acc, seat) => (Math.Min(acc.min, seat), Math.Max(acc.max, seat), acc.sum + seat)
                );

                var total = (max + min) * (max - min + 1) / 2;
                var missing = total - sum;

                Console.WriteLine(missing);
            }
        }

        private static class Util
        {
            public static (int row, int column, int seatId) Parse(string line)
            {
                var rowText = line.Substring(0, 7);
                var colText = line.Substring(7, 3);

                var row = Util.Find(128, rowText, 'F', 'B');
                var col = Util.Find(8, colText, 'L', 'R');

                return (row, col, row * 8 + col);
            }

            public static int Find(int number, string text, char left, char right)
            {
                var min = 0;
                var max = number - 1;

                foreach (var ch in text)
                {
                    var mid = (max + min + 1) / 2;

                    if (ch == left)
                    {
                        max = mid - 1;
                    }
                    else if (ch == right)
                    {
                        min = mid;
                    }
                }

                return min;
            }
        }
    }
}
