using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day16
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
.|...\....
|.-.\.....
.....|-...
........|.
..........
.........\
..../.\\..
.-.-/..|..
.|....-|.\
..//.|....
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/16/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());
            
            var initialBeam = new Beam(new Position(Row: 0, Col: -1), Direction.Right);

            var energized = Simulation.Run(grid, initialBeam);
            Console.WriteLine(energized);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var initialBeams = InitialBeams(grid);

            var energized = initialBeams
                .Select(initialBeam => Simulation.Run(grid, initialBeam))
                .Max();
            Console.WriteLine(energized);
        }

        private static IEnumerable<Beam> InitialBeams(Grid grid)
        {
            static IEnumerable<Beam> BeamsFromTop(Grid grid) =>
                Enumerable.Range(start: 0, count: grid.Cols)
                    .Select(col => new Position(Row: -1, Col: col))
                    .Select(pos => new Beam(pos, Direction.Down));

            static IEnumerable<Beam> BeamsFromBottom(Grid grid) =>
                Enumerable.Range(start: 0, count: grid.Cols)
                    .Select(col => new Position(Row: grid.Rows, Col: col))
                    .Select(pos => new Beam(pos, Direction.Up));

            static IEnumerable<Beam> BeamsFromLeft(Grid grid) =>
                Enumerable.Range(start: 0, count: grid.Rows)
                    .Select(row => new Position(Row: row, Col: -1))
                    .Select(pos => new Beam(pos, Direction.Right));

            static IEnumerable<Beam> BeamsFromRight(Grid grid) =>
                Enumerable.Range(start: 0, count: grid.Rows)
                    .Select(row => new Position(Row: row, Col: grid.Cols))
                    .Select(pos => new Beam(pos, Direction.Left));

            return BeamsFromTop(grid)
                .Concat(BeamsFromBottom(grid))
                .Concat(BeamsFromLeft(grid))
                .Concat(BeamsFromRight(grid));
        }
    }

    private enum Direction { Up, Down, Left, Right }

    private record Position(int Row, int Col)
    {
        public Position Up() => this with { Row = Row - 1 };
        public Position Down() => this with { Row = Row + 1 };
        public Position Left() => this with { Col = Col - 1 };
        public Position Right() => this with { Col = Col + 1 };

        public Position Move(Direction dir) =>
            dir switch
            {
                Direction.Up => Up(),
                Direction.Down => Down(),
                Direction.Left => Left(),
                Direction.Right => Right(),
                _ => this,
            };
    }

    private record Beam(Position Position, Direction Direction)
    {
        public Beam Move() => this with { Position = Position.Move(Direction) };
    }

    private sealed class Grid
    {
        public static Grid Parse(IEnumerable<string> lines) =>
            new(lines.ToList());

        private readonly IReadOnlyList<string> cells;

        public Grid(IReadOnlyList<string> cells)
        {
            this.cells = cells;
        }

        public int Rows => this.cells.Count;
        public int Cols => this.cells[0].Length;

        public bool InBounds(Position p) =>
            0 <= p.Row && p.Row < Rows &&
            0 <= p.Col && p.Col < Cols;

        public char At(Position p) =>
            this.cells[p.Row][p.Col];
    }

    private static class Simulation
    {
        public static int Run(Grid grid, Beam initialBeam)
        {
            var beams = new List<Beam>() { initialBeam };
            var seen = new HashSet<Beam>();

            while (beams.Count > 0)
            {
                beams = beams
                    .SelectMany(beam => Move(grid, beam))
                    .Where(beam => !seen.Contains(beam))
                    .ToList();

                seen.AddRange(beams);
            }

            var energized = seen
                .Select(beam => beam.Position)
                .Distinct()
                .Count();
            return energized;
        }

        private static IReadOnlyList<Beam> Move(Grid grid, Beam beam)
        {
            var nextBeam = beam.Move();

            if (!grid.InBounds(nextBeam.Position))
            {
                return Array.Empty<Beam>();
            }

            var cell = grid.At(nextBeam.Position);
            return (cell, beam.Direction) switch
            {
                ('|', Direction.Left) or ('|', Direction.Right) => new[]
                    {
                        nextBeam with { Direction = Direction.Up },
                        nextBeam with { Direction = Direction.Down },
                    },

                ('-', Direction.Up) or ('-', Direction.Down) => new[]
                    {
                        nextBeam with { Direction = Direction.Left },
                        nextBeam with { Direction = Direction.Right },
                    },

                ('\\', Direction.Left) =>
                    new[] { nextBeam with { Direction = Direction.Up } },
                ('\\', Direction.Right) =>
                    new[] { nextBeam with { Direction = Direction.Down } },
                ('\\', Direction.Down) =>
                    new[] { nextBeam with { Direction = Direction.Right } },
                ('\\', Direction.Up) =>
                    new[] { nextBeam with { Direction = Direction.Left } },

                ('/', Direction.Left) =>
                    new[] { nextBeam with { Direction = Direction.Down } },
                ('/', Direction.Right) =>
                    new[] { nextBeam with { Direction = Direction.Up } },
                ('/', Direction.Down) =>
                    new[] { nextBeam with { Direction = Direction.Left } },
                ('/', Direction.Up) =>
                    new[] { nextBeam with { Direction = Direction.Right } },

                _ => new[] { nextBeam },
            };
        }
    }
}
