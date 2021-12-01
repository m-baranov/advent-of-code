using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day10
    {
        public static readonly IInput Sample1Input =
            Input.Literal(
                "......#.#.",
                "#..#.#....",
                "..#######.",
                ".#.#.###..",
                ".#..#.....",
                "..#....#.#",
                "#..#....#.",
                ".##.#..###",
                "##...#..#.",
                ".#....####"
            );

        public static readonly IInput Sample2Input =
            Input.Literal(
                "#.#...#.#.",
                ".###....#.",
                ".#....#...",
                "##.#.#.#.#",
                "....#.#.#.",
                ".##..###.#",
                "..#...##..",
                "..##....##",
                "......#...",
                ".####.###."
            );

        public static readonly IInput Sample3Input =
            Input.Literal(
                ".#..#..###",
                "####.###.#",
                "....###.#.",
                "..###.##.#",
                "##.##.#.#.",
                "....###..#",
                "..#.#..#.#",
                "#..#.#.###",
                ".##...##.#",
                ".....#.#.."
            );

        public static readonly IInput Sample4Input =
            Input.Literal(
                ".#..##.###...#######",
                "##.############..##.",
                ".#.######.########.#",
                ".###.#######.####.#.",
                "#####.##.#.##.###.##",
                "..#####..#.#########",
                "####################",
                "#.####....###.#.#.##",
                "##.#################",
                "#####.##.###..####..",
                "..######..##.#######",
                "####.##.####...##..#",
                ".#####..#.######.###",
                "##...#.##########...",
                "#.##########.#######",
                ".####.#.###.###.#.##",
                "....##.##.###..#####",
                ".#.#.###########.###",
                "#.#.#.#####.####.###",
                "###.##.####.##.#..##"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/10/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var map = Map.Parse(input.Lines());

                var directions = Map.AllDirections(map.Size);

                var result = map.CoordinatesOf('#')
                    .Select(coordinate => new
                    {
                        coordinate,
                        count = directions
                            .Where(dir => map.Line(coordinate, dir).Skip(1).Any(ch => ch == '#'))
                            .Count()
                    })
                    .MaxBy(c => c.count);

                Console.WriteLine($"{result.count} asteroids at ({result.coordinate.col},{result.coordinate.row})");
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var map = Map.Parse(input.Lines());

                var directions = Map.AllDirections(map.Size)
                    .OrderBy(d => Angle(d))
                    // swap x and y, because map uses (row, col) coordinates, i.e. y goes first there
                    .Select(d => (dx: d.dy, dy: d.dx)) 
                    .ToList();

                var asteroidPos = map.CoordinatesOf('#')
                    .Select(coordinate => new
                    {
                        coordinate,
                        count = directions
                            .Where(dir => map.Line(coordinate, dir).Skip(1).Any(ch => ch == '#'))
                            .Count()
                    })
                    .MaxBy(c => c.count)
                    .coordinate;

                var allBlasted = new List<(int row, int col)>();
                var directionIndex = 0;
                while (allBlasted.Count < 200)
                {
                    var dir = directions[directionIndex];

                    directionIndex++;
                    if (directionIndex >= directions.Count)
                    {
                        directionIndex = 0;
                    }

                    var blastPos = map
                        .LineCoordinates(asteroidPos, dir).Skip(1)
                        .Where(c => map.Get(c) == '#')
                        .Take(1)
                        .ToList();

                    if (blastPos.Any())
                    {
                        allBlasted.Add(blastPos.First());
                        map.Set(blastPos.First(), '.');
                    }
                }

                var answer = allBlasted.Last();
                Console.WriteLine(answer.col * 100 + answer.row);
            }

            private double Angle((int dx, int dy) dir)
            {
                var andgleRad = NormalizeRad(Math.Atan2(dir.dy, dir.dx) + Math.PI / 2);
                return andgleRad * 180 / Math.PI; // convert to degrees to make it easier to debug
            }

            private double NormalizeRad(double value)
            {
                var twoPI = Math.PI * 2;
                return value + Math.Ceiling(-value / twoPI) * twoPI;
            }
        }

        private class Map
        {
            public static IReadOnlyList<(int dx, int dy)> AllDirections(int size)
            {
                var cardinal = new[]
                {
                    (dx: 1, dy: 0),
                    (dx: 1, dy: 1),
                    (dx: 0, dy: 1),
                    (dx: -1, dy: 1),
                    (dx: -1, dy: 0),
                    (dx: -1, dy: -1),
                    (dx: 0, dy: -1),
                    (dx: 1, dy: -1),
                };

                var dirs = Enumerable.Range(2, count: size - 2)
                    .SelectMany(dx => Enumerable.Range(1, count: dx - 1).Select(dy => (dx, dy)))
                    .Where(d => MathExtensions.Gcd(d.dx, d.dy) == 1)
                    .ToList();

                var quad = dirs
                    .Concat(dirs.Select(d => (dx: d.dy, dy: d.dx)));

                return cardinal
                    .Concat(quad)
                    .Concat(quad.Select(d => (dx: d.dx, dy: -d.dy)))
                    .Concat(quad.Select(d => (dx: -d.dx, dy: d.dy)))
                    .Concat(quad.Select(d => (dx: -d.dx, dy: -d.dy)))
                    .ToList();
            }

            public static Map Parse(IEnumerable<string> lines)
            {
                char[,] map = null;
                var row = 0;

                foreach (var line in lines)
                {
                    if (map == null)
                    {
                        map = new char[line.Length, line.Length];
                    }

                    var col = 0;
                    foreach (var ch in line)
                    {
                        map[row, col] = ch;
                        col++;
                    }
                    row++;
                }

                return new Map(map);
            }

            private readonly char[,] map;

            public Map(char[,] map)
            {
                this.map = map;
            }

            public int Size => map.GetLength(0);

            public char Get((int row, int col) pos) => map[pos.row, pos.col];

            public void Set((int row, int col) pos, char ch)
            {
                map[pos.row, pos.col] = ch;
            }

            public IEnumerable<(int row, int col)> CoordinatesOf(char ch)
            {
                var size = Size;

                for (var row = 0; row < size; row++)
                {
                    for (var col = 0; col < size; col++)
                    {
                        if (map[row, col] == ch)
                        {
                            yield return (row, col);
                        }
                    }
                }
            }

            public IEnumerable<char> Line((int row, int col) start, (int dr, int dc) direction)
            {
                return LineCoordinates(start, direction).Select(Get);
            }

            public IEnumerable<(int row, int col)> LineCoordinates((int row, int col) start, (int dr, int dc) direction)
            {
                var size = Size;

                var pos = start;
                while (0 <= pos.row && pos.row < size &&
                       0 <= pos.col && pos.col < size)
                {
                    yield return pos;
                    pos = (pos.row + direction.dr, pos.col + direction.dc);
                }
            }
        }
    }
}
