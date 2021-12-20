using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day20
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "..#.#..#####.#.#.#.###.##.....###.##.#..###.####..#####..#....#..#..##..###..######.###...####..#..#####..##..#.#####...##.#.#..#.##..#.#......#.###.######.###.####...#.##.##..#..#..#####.....#.#....###..#.##......#.....#..#..#..##..#...##.######.####.####.#.#...#.......#..#.#.#...####.##.#......#..#...##.#.##..#...##.#.##..###.#......#.#.......#.#.#.####.###.##...#.....####.#..#..#.##.#....##..#.####....##...##..#...#......#.#.......#.......##..####..#...#.#.#...##..#.#..###..#####........#..####......#..#",
                    "",
                    "#..#.",
                    "#....",
                    "##..#",
                    "..#..",
                    "..###"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/20/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var setup = Setup.Parse(input.Lines());

                var enhancedImage = Image.Enhance(setup.Image, setup.Enhancement, times: 2);

                Console.WriteLine(Image.CountLitBits(enhancedImage));
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var setup = Setup.Parse(input.Lines());

                var enhancedImage = Image.Enhance(setup.Image, setup.Enhancement, times: 50);

                Console.WriteLine(Image.CountLitBits(enhancedImage));
            }
        }

        private class Enhancement
        {
            public static Enhancement Parse(string text)
            {
                var pixels = text.Select(c => c == '#').ToList();
                return new Enhancement(pixels);
            }

            private readonly IReadOnlyList<bool> pixels;

            public Enhancement(IReadOnlyList<bool> pixels)
            {
                this.pixels = pixels;
            }

            public bool At(IEnumerable<bool> bits) => pixels[ToInt(bits)];

            private static int ToInt(IEnumerable<bool> bits)
            {
                var result = 0;
                foreach (var bit in bits)
                {
                    result = (result << 1) + (bit ? 1 : 0);
                }
                return result;
            }
        }

        private interface IImage
        {
            int RowOffset { get; }
            int ColOffset { get; }
            int Rows { get; }
            int Cols { get; }
            bool At(int row, int col);
        }

        private static class Image
        {
            public static IImage Enhance(IImage image, Enhancement enhancement, int times)
            {
                IImage enhancedImage = image;
                for (var i = 0; i < times; i++)
                {
                    enhancedImage = new EnhancedImage(enhancedImage, enhancement).Materialize();
                }
                return enhancedImage;
            }

            public static void Draw(IImage image)
            {
                for (var row = 0; row < image.Rows; row++)
                {
                    for (var col = 0; col < image.Cols; col++)
                    {
                        var bit = image.At(row + image.RowOffset, col + image.ColOffset);
                        Console.Write(bit ? "██" : "  ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }

            public static int CountLitBits(IImage image) => Count(image, bit => bit == true);

            public static int Count(IImage image, Func<bool, bool> predicate)
            {
                var count = 0;
                for (var row = 0; row < image.Rows; row++)
                {
                    for (var col = 0; col < image.Cols; col++)
                    {
                        var bit = image.At(row + image.RowOffset, col + image.ColOffset);
                        if (predicate(bit))
                        {
                            count++;
                        }
                    }
                }

                return count;
            }
        }

        private class InitialImage : IImage
        {
            public static InitialImage Parse(IEnumerable<string> lines)
            {
                var pixels = lines
                    .Select(l => l.Select(c => c == '#').ToList())
                    .ToList();

                return new InitialImage(pixels);
            }

            private readonly IReadOnlyList<IReadOnlyList<bool>> pixels;
            private readonly bool defaultPixel;

            public InitialImage(
                IReadOnlyList<IReadOnlyList<bool>> pixels,
                int rowOffset = 0,
                int colOffset = 0,
                bool defaultPixel = false)
            {
                this.pixels = pixels;
                RowOffset = rowOffset;
                ColOffset = colOffset;
                this.defaultPixel = defaultPixel;
            }

            public int RowOffset { get; }
            public int ColOffset { get; }
            public int Rows => pixels.Count;
            public int Cols => pixels[0].Count;

            public bool At(int row, int col) => 
                InRange(row - RowOffset, col - ColOffset) 
                    ? pixels[row - RowOffset][col - ColOffset] 
                    : defaultPixel;

            private bool InRange(int row, int col) =>
                0 <= row && row < Rows &&
                0 <= col && col < Cols;
        }

        private class EnhancedImage : IImage
        {
            private readonly IImage image;
            private readonly Enhancement enhancement;

            public EnhancedImage(IImage image, Enhancement enhancement)
            {
                this.image = image;
                this.enhancement = enhancement;
            }

            public int RowOffset => image.RowOffset - 1;
            public int ColOffset => image.ColOffset - 1;
            public int Rows => image.Rows + 2;
            public int Cols => image.Cols + 2;

            public bool At(int row, int col)
            {
                var deltas = new[]
                {
                    (r: -1, c: -1),
                    (r: -1, c: 0),
                    (r: -1, c: 1),
                    (r: 0, c: -1),
                    (r: 0, c: 0),
                    (r: 0, c: 1),
                    (r: 1, c: -1),
                    (r: 1, c: 0),
                    (r: 1, c: 1),
                };

                var bits = deltas.Select(d => image.At(row + d.r, col + d.c));
                return enhancement.At(bits);
            }

            public InitialImage Materialize()
            {
                var pixels = new List<IReadOnlyList<bool>>();

                for (var row = 0; row < Rows; row++)
                {
                    var line = new List<bool>();
                    for (var col = 0; col < Cols; col++)
                    {
                        line.Add(At(row + RowOffset, col + ColOffset));
                    }
                    pixels.Add(line);
                }

                var defaultPixel = At(RowOffset - 1, ColOffset - 1);
                return new InitialImage(pixels, RowOffset, ColOffset, defaultPixel);
            }
        }

        private class Setup
        {
            public static Setup Parse(IEnumerable<string> lines)
            {
                var groups = lines.SplitByEmptyLine().Take(2).ToList();

                var enchancement = Enhancement.Parse(groups[0][0]);
                var image = InitialImage.Parse(groups[1]);

                return new Setup(enchancement, image);
            }

            public Setup(Enhancement enhancement, InitialImage image)
            {
                Enhancement = enhancement;
                Image = image;
            }

            public Enhancement Enhancement { get; }
            public InitialImage Image { get; }
        }
    }
}
