using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day11
    {
        public static class Inputs
        {
            public static readonly IInput Sample1 =
                Input.Literal("18");

            public static readonly IInput Sample2 =
                Input.Literal("42");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/11/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var serial = int.Parse(input.Lines().First());

                var grid = new Grid(serial);
                var (x, y, p) = grid.LargestPowerSquareCoordinates(side: 3);

                Console.WriteLine($"{x},{y} = {p}");
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var serial = int.Parse(input.Lines().First());

                var grid = new Grid(serial);
                var (x, y, s, p) = grid.LargestPowerSquareCoordinates();

                Console.WriteLine($"{x},{y},{s} = {p}");
            }
        }

        private class Grid
        {
            private const int SIZE = 300;
            
            private readonly int[,] cells;

            public Grid(int serial)
            {
                this.cells = Precalc(serial);
            }

            private static int[,] Precalc(int serial)
            {
                static int PowerLevel(int x, int y, int serial)
                {
                    var powerLevel = ((x + 10) * y + serial) * (x + 10);
                    return powerLevel / 100 % 10 - 5;
                }

                var cells = new int[SIZE - 1, SIZE - 1];

                for (var x = 0; x < SIZE - 1; x++)
                {
                    for (var y = 0; y < SIZE - 1; y++)
                    {
                        cells[x, y] = PowerLevel(x + 1, y + 1, serial);
                    }
                }

                return cells;
            }

            public int CellPowerLevel(int x, int y) => this.cells[x - 1, y - 1];

            public int SquarePowerLevel(int x, int y, int side)
            {
                var sum = 0;

                for (var dx = 0; dx < side; dx++)
                {
                    for (var dy = 0; dy < side; dy++)
                    {
                        sum += CellPowerLevel(x + dx, y + dy);
                    }
                }

                return sum;
            }

            public (int x, int y, int power) LargestPowerSquareCoordinates(int side)
            {
                var maxPower = 0;
                var result = (x: 0, y: 0, power: 0);

                for (var x = 1; x < SIZE - side + 1; x++)
                {
                    for (var y = 1; y < SIZE - side + 1; y++)
                    {
                        var power = SquarePowerLevel(x, y, side);
                        if (power > maxPower)
                        {
                            maxPower = power;
                            result = (x, y, power);
                        }
                    }
                }

                return result;
            }

            public (int x, int y, int side, int power) LargestPowerSquareCoordinates()
            {
                var maxPower = 0;
                var result = (x: 0, y: 0, side: 0, power: 0);

                var sums = new int[SIZE - 1, SIZE - 1];

                for (var side = 1; side <= 300; side++)
                {
                    for (var x = 0; x < SIZE - side; x++)
                    {
                        for (var y = 0; y < SIZE - side; y++)
                        {
                            var power = sums[x, y];

                            for (var i = 0; i < side - 1; i++)
                            {
                                power += this.cells[x + side - 1, y + i] + this.cells[x + i, y + side - 1];
                            }

                            power += this.cells[x + side - 1, y + side - 1];

                            sums[x, y] = power;

                            if (power > maxPower)
                            {
                                maxPower = power;
                                result = (x + 1, y + 1, side, power);
                            }
                        }
                    }
                }

                return result;
            }
        }
    }
}
