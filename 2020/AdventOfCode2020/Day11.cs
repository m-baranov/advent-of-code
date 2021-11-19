using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2020
{
    static class Day11
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "L.LL.LL.LL",
                "LLLLLLL.LL",
                "L.L.L..L..",
                "LLLL.LL.LL",
                "L.LL.LL.LL",
                "L.LLLLL.LL",
                "..L.L.....",
                "LLLLLLLLLL",
                "L.LLLLLL.L",
                "L.LLLLL.LL"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/11/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var lines = input.Lines().ToList();
                var seats = Seats.Parse(lines);

                var current = Seats.Simulate(seats, NextSeat);

                var occupied = seats.Count(s => s == Seat.Occupied);
                Console.WriteLine(occupied);
            }

            private static Seat NextSeat(Seat[,] current, int r, int c)
            {
                var seat = current[r, c];

                if (seat == Seat.Empty)
                {
                    if (!current.AdjacentSeats(r, c).Any(s => s == Seat.Occupied))
                    {
                        return Seat.Occupied;
                    }
                }
                else if (seat == Seat.Occupied)
                {
                    if (current.AdjacentSeats(r, c).Where(s => s == Seat.Occupied).Count() >= 4)
                    {
                        return Seat.Empty;
                    }
                }

                return seat;
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var lines = input.Lines().ToList();
                var seats = Seats.Parse(lines);

                var current = Seats.Simulate(seats, NextSeat);

                var occupied = seats.Count(s => s == Seat.Occupied);
                Console.WriteLine(occupied);
            }

            private static Seat NextSeat(Seat[,] current, int r, int c)
            {
                var seat = current[r, c];

                if (seat == Seat.Empty)
                {
                    if (!current.AdjacentVisibleSeats(r, c).Any(s => s == Seat.Occupied))
                    {
                        return Seat.Occupied;
                    }
                }
                else if (seat == Seat.Occupied)
                {
                    if (current.AdjacentVisibleSeats(r, c).Where(s => s == Seat.Occupied).Count() >= 5)
                    {
                        return Seat.Empty;
                    }
                }

                return seat;
            }
        }
    }

    public enum Seat
    {
        Floor,
        Empty,
        Occupied
    }

    public static class Seats
    {
        public static Seat[,] Parse(IReadOnlyList<string> lines)
        {
            var rows = lines.Count;
            var cols = lines.First().Length;

            var seats = new Seat[rows, cols];

            var row = 0;
            foreach (var line in lines)
            {
                var col = 0;
                foreach (var ch in line)
                {
                    seats[row, col] = Parse(ch);
                    col++;
                }
                row++;
            }

            return seats;
        }

        private static Seat Parse(char ch)
        {
            if (ch == 'L') return Seat.Empty;
            if (ch == '#') return Seat.Occupied;
            return Seat.Floor;
        }

        private static IReadOnlyList<(int dr, int dc)> AdjacentIndices =
            new[]
            {
                    (-1, -1),
                    (0, -1),
                    (1, -1),
                    (-1, 0),
                    (1, 0),
                    (-1, 1),
                    (0, 1),
                    (1, 1)
            };

        public static int Rows(this Seat[,] seats) => seats.GetLength(0);

        public static int Cols(this Seat[,] seats) => seats.GetLength(1);

        public static IEnumerable<Seat> AdjacentSeats(this Seat[,] seats, int row, int col)
        {
            var rows = seats.Rows();
            var cols = seats.Cols();

            foreach (var (dr, dc) in AdjacentIndices)
            {
                var r = row + dr;
                var c = col + dc;

                if (r < 0 || r >= rows || c < 0 || c >= cols)
                {
                    continue;
                }

                yield return seats[r, c];
            }
        }

        public static IEnumerable<Seat> AdjacentVisibleSeats(this Seat[,] seats, int row, int col)
        {
            foreach (var (dr, dc) in AdjacentIndices)
            {
                var seatsInDirection = seats.SeatsInDirection(row, col, dr, dc);
                
                var firstNotFloor = seatsInDirection.Where(s => s != Seat.Floor).FirstOrDefault();
                yield return firstNotFloor;
            }
        }

        private static IEnumerable<Seat> SeatsInDirection(this Seat[,] seats, int row, int col, int dr, int dc)
        {
            var rows = seats.Rows();
            var cols = seats.Cols();

            var r = row + dr;
            var c = col + dc;
            while (0 <= r && r < rows && 0 <= c && c < cols)
            {
                yield return seats[r, c];

                r += dr;
                c += dc;
            }
        }

        public static int Count(this Seat[,] seats, Func<Seat, bool> predicate)
        {
            var count = 0;
            for (var r = 0; r < seats.Rows(); r++)
            {
                for (var c = 0; c < seats.Cols(); c++)
                {
                    if (predicate(seats[r, c]))
                        count++;
                }
            }

            return count;
        }

        public static Seat[,] Simulate(Seat[,] seats, Func<Seat[,], int, int, Seat> nextSeat)
        {
            var current = seats;
            var next = new Seat[current.Rows(), current.Cols()];
            while (true)
            {
                var advanced = Advance(current, next, nextSeat);
                if (!advanced)
                {
                    break;
                }

                var temp = next;
                next = current;
                current = temp;
            }

            return current;
        }

        private static bool Advance(Seat[,] current, Seat[,] next, Func<Seat[,], int, int, Seat> nextSeat)
        {
            var advanced = false;

            for (var r = 0; r < current.Rows(); r++)
            {
                for (var c = 0; c < current.Cols(); c++)
                {
                    next[r, c] = nextSeat(current, r, c);

                    if (current[r, c] != next[r, c])
                    {
                        advanced = true;
                    }
                }
            }

            return advanced;
        }
    }
}
