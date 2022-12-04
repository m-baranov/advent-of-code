using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day03
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal("1024");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/3/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var n = int.Parse(input.Lines().First());

                Console.WriteLine(ManhattanDistance(n));
            }

            private static int ManhattanDistance(int n)
            {
                static int Pow2(int x) => x * x;

                var size = (int)Math.Ceiling(Math.Sqrt(n));
                
                var startsAt = Pow2(size - 2) + 1;
                
                var pos = (n - startsAt) % (size - 1);

                return size / 2 + Math.Abs(pos - size / 2 + 1);                    
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var n = int.Parse(input.Lines().First());

                var answer = EnumerateValues().First(v => v > n);
                
                Console.WriteLine(answer);
            }

            private static IEnumerable<int> EnumerateValues()
            {
                var previous = new Square(size: 1);
                previous[Position.Origin] = 1;
                yield return 1;

                while (true)
                {
                    var current = new Square(size: previous.Size + 2);

                    foreach (var pos in PositionsOnSquare(current.Size))
                    {
                        var sum = pos.Neighbors()
                            .Select(npos => current.GetOrDefault(npos) + previous.GetOrDefault(npos))
                            .Sum();

                        current[pos] = sum;
                        yield return sum;
                    }

                    previous = current;
                }
            }

            private static IEnumerable<Position> PositionsOnSquare(int size)
            {
                var half = size / 2;

                int row, col;

                // left side
                col = half;
                for (row = -half + 1; row <= half; row++)
                {
                    yield return new Position(row, col);
                }

                // top side
                row = half;
                for (col = half - 1; col >= -half; col--)
                {
                    yield return new Position(row, col);
                }

                // right side
                col = -half;
                for (row = half - 1; row >= -half; row--)
                {
                    yield return new Position(row, col);
                }

                // top side
                row = -half;
                for (col = -half + 1; col <= half; col++)
                {
                    yield return new Position(row, col);
                }
            }

            private record Position(int Row, int Col)
            {
                public static readonly Position Origin = new(0, 0);

                public IEnumerable<Position> Neighbors()
                {
                    var deltas = new[]
                    {
                        (r: 0, c: 1),
                        (r: 1, c: 1),
                        (r: 1, c: 0),
                        (r: 1, c: -1),
                        (r: 0, c: -1),
                        (r: -1, c: -1),
                        (r: -1, c: 0),
                        (r: -1, c: 1),
                    };

                    return deltas.Select(d => new Position(this.Row + d.r, this.Col + d.c));
                }
            }

            private class Square
            {
                private readonly int size;
                private readonly int[] values;

                public Square(int size)
                {
                    this.size = size;
                    this.values = new int[size * size];
                }

                public int Size => this.size;

                public int this[Position pos]
                {
                    get => this.values[Index(pos)];
                    set => this.values[Index(pos)] = value;
                }

                public int GetOrDefault(Position pos) =>
                    Contains(pos) ? this[pos] : 0;

                public bool Contains(Position pos)
                {
                    var half = this.size / 2;
                    return (Math.Abs(pos.Row) == half && Math.Abs(pos.Col) <= half) 
                        || (Math.Abs(pos.Col) == half && Math.Abs(pos.Row) <= half);
                }

                private int Index(Position pos)
                {
                    var half = this.size / 2;
                    if (pos.Col == half)
                    {
                        // left side
                        return pos.Row + half;
                    }
                    if (pos.Col == -half)
                    {
                        // right side
                        return pos.Row + half + this.size;
                    }
                    if (pos.Row == half)
                    {
                        // top side
                        return this.size * 2 + pos.Col + half - 1;
                    }
                    //if (pos.Row == -half)
                    {
                        // bottom side
                        return (this.size * 3 - 2) + pos.Col + half - 1;
                    }
                }
            }
        }
    }
}
