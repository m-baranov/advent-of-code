using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day03
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "#1 @ 1,3: 4x4",
                    "#2 @ 3,1: 4x4",
                    "#3 @ 5,5: 2x2"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/3/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var claims = input.Lines().Select(Claim.Parse).ToList();

                var fabric = new int[1000, 1000];

                foreach (var claim in claims)
                {
                    Inc(fabric, claim.Rect);
                }

                var answer = Count(fabric, claimCount => claimCount > 1);
                Console.WriteLine(answer);
            }

            private void Inc(int[,] fabric, Rect rect)
            {
                for (var row = rect.Top; row < rect.Top + rect.Height; row++)
                {
                    for (var col = rect.Left; col < rect.Left + rect.Width; col++)
                    {
                        fabric[row, col]++; 
                    }
                }
            }

            private int Count(int[,] fabric, Func<int, bool> predicate)
            {
                var rows = fabric.GetLength(0);
                var cols = fabric.GetLength(1);

                var count = 0;
                for (var row = 0; row < rows; row++)
                {
                    for (var col = 0; col < cols; col++)
                    {
                        if (predicate(fabric[row, col]))
                        {
                            count++;
                        }
                    }
                }
                return count;
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var claims = input.Lines().Select(Claim.Parse).ToList();

                var claim = claims
                    .Single(claim => claims
                        .Where(other => other != claim)
                        .All(other => !claim.IntersectsWith(other))
                    );

                Console.WriteLine(claim.Id);
            }
        }

        private class Claim
        {
            public static Claim Parse(string text)
            {
                // #1 @ 1,3: 4x4

                var values = text
                    .Split(new[] { '#', ' ', '@', ',', ':', 'x' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                var rect = new Rect(values[1], values[2], values[3], values[4]);
                return new Claim(values[0], rect);
            }

            public Claim(int id, Rect rect)
            {
                Id = id;
                Rect = rect;
            }

            public int Id { get; }
            public Rect Rect { get; }

            public bool IntersectsWith(Claim other) => Rect.IntersectsWith(other.Rect);
        }

        private class Rect
        {
            public Rect(int left, int top, int width, int height)
            {
                Left = left;
                Top = top;
                Width = width;
                Height = height;
            }

            public int Left { get; }
            public int Top { get; }
            public int Width { get; }
            public int Height { get; }

            public bool IntersectsWith(Rect other)
            {
                static bool DimensionIntesects(int start1, int size1, int start2, int size2)
                {
                    var end1 = start1 + size1;
                    var end2 = start2 + size2;

                    return !(end2 <= start1 || end1 <= start2);
                }

                return DimensionIntesects(Left, Width, other.Left, other.Width)
                    && DimensionIntesects(Top, Height, other.Top, other.Height);
            }
        }
    }
}
