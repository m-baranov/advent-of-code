using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdventOfCode2022
{
    static class Day22
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "        ...#    ",
                    "        .#..    ",
                    "        #...    ",
                    "        ....    ",
                    "...#.......#    ",
                    "........#...    ",
                    "..#....#....    ",
                    "..........#.    ",
                    "        ...#....",
                    "        .....#..",
                    "        .#......",
                    "        ......#.",
                    "",
                    "10R5L5R10L4R5L5"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/22/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var (grid, instructions) = Parse(input.Lines());

                var actor = new Actor(grid.InitialPosition(), Direction.Right);

                foreach (var instruction in instructions)
                {
                    if (instruction is Instruction.Rotate rotate)
                    {
                        actor = actor.Rotate(rotate.Rotation);
                    }
                    else if (instruction is Instruction.Walk walk)
                    {
                        for (var i = 0; i < walk.Times; i++)
                        {
                            if (!TryStep(grid, actor, out actor))
                            {
                                break;
                            }
                        }
                    }
                }

                Console.WriteLine(actor);
                Console.WriteLine(actor.FinalPassword());
            }

            private static bool TryStep(Grid grid, Actor actor, out Actor nextActor)
            {
                var nextPosition = NextPosition(grid, actor.Position, actor.Direction);

                if (grid.At(nextPosition) == '.')
                {
                    nextActor = actor with { Position = nextPosition };
                    return true;
                }

                nextActor = actor;
                return false;
            }

            private static Position NextPosition(Grid grid, Position position, Direction direction)
            {
                Position Wrap(Position position)
                {
                    if (position.Col < 0)
                    {
                        return position with { Col = grid.Cols - 1 };
                    }
                    else if (grid.Cols <= position.Col)
                    {
                        return position with { Col = 0 };
                    }
                    else if (position.Row < 0)
                    {
                        return position with { Row = grid.Rows - 1 };
                    }
                    else if (grid.Rows <= position.Row)
                    {
                        return position with { Row = 0 };
                    }
                    else
                    {
                        return position;
                    }
                }

                Position SkipBlanks(Position position, Direction direction)
                {
                    while (grid.InBounds(position) && grid.At(position) == ' ')
                    {
                        position = position.Move(direction);
                    }

                    return position;
                }

                var next = position.Move(direction);
                next = SkipBlanks(next, direction);

                if (!grid.InBounds(next))
                {
                    next = Wrap(next);
                    next = SkipBlanks(next, direction);
                    return next;
                }

                return next;
            }
        }

        public class Part2 : IProblem
        {
            // Only works for my specific test input.
            // 
            // Apparently there is a limited number of possible cube folds:
            // https://das.org.sg/news-events/blogs/182-maths-teaching-and-learning/809-understanding-cubes-and-nets.html
            // 
            // But, I could not be bothered to implement it for a general case.

            public void Run(TextReader input)
            {
                const int SideSize = 50;

                var (grid, instructions) = Parse(input.Lines());
                var actor = new Actor(grid.InitialPosition(), Direction.Right);

                foreach (var instruction in instructions)
                {
                    if (instruction is Instruction.Rotate rotate)
                    {
                        actor = actor.Rotate(rotate.Rotation);
                    }
                    else if (instruction is Instruction.Walk walk)
                    {
                        for (var i = 0; i < walk.Times; i++)
                        {
                            if (!TryStep(grid, actor, SideSize, out actor))
                            {
                                break;
                            }
                        }
                    }
                }

                Console.WriteLine(actor);
                Console.WriteLine(actor.FinalPassword());
            }

            private static bool TryStep(Grid grid, Actor actor, int sideSize, out Actor nextActor)
            {
                var (nextPosition, nextDirection) = Move(actor.Position, actor.Direction, sideSize);

                if (grid.At(nextPosition) == '.')
                {
                    nextActor = new Actor(nextPosition, nextDirection);
                    return true;
                }

                nextActor = actor;
                return false;
            }

            private static (Position, Direction) Move(Position pos, Direction dir, int sideSize)
            {
                var sp = SidePosition.FromAbsolute(pos, sideSize);
                var (nextSp, nextDir) = Move(sp, dir, sideSize);
                return (SidePosition.ToAbsolute(nextSp, sideSize), nextDir);
            }

            private static (SidePosition, Direction) Move(SidePosition pos, Direction dir, int sideSize)
            {
                static bool InBounds(Position p, int sideSize) =>
                    0 <= p.Row && p.Row < sideSize &&
                    0 <= p.Col && p.Col < sideSize;

                var next = pos.Relative.Move(dir);
                if (InBounds(next, sideSize))
                {
                    return (pos with { Relative = next }, dir);
                }

                return (pos.Side, dir.ToString()) switch
                {
                    ({ Row: 0, Col: 1 }, "D") => 
                        (new SidePosition(Side: new(1, 1), Relative: new(0, next.Col)), Direction.Down),
                    ({ Row: 0, Col: 1 }, "R") =>
                        (new SidePosition(Side: new(0, 2), Relative: new(next.Row, 0)), Direction.Right),
                    ({ Row: 0, Col: 1 }, "L") =>
                        (new SidePosition(Side: new(2, 0), Relative: new(sideSize - 1 - next.Row, 0)), Direction.Right),
                    ({ Row: 0, Col: 1 }, "U") =>
                        (new SidePosition(Side: new(3, 0), Relative: new(next.Col, 0)), Direction.Right),

                    ({ Row: 1, Col: 1 }, "D") =>
                        (new SidePosition(Side: new(2, 1), Relative: new(0, next.Col)), Direction.Down),
                    ({ Row: 1, Col: 1 }, "R") =>
                        (new SidePosition(Side: new(0, 2), Relative: new(sideSize - 1, next.Row)), Direction.Up),
                    ({ Row: 1, Col: 1 }, "L") =>
                        (new SidePosition(Side: new(2, 0), Relative: new(0, next.Row)), Direction.Down),
                    ({ Row: 1, Col: 1 }, "U") =>
                        (new SidePosition(Side: new(0, 1), Relative: new(sideSize - 1, next.Col)), Direction.Up),

                    ({ Row: 2, Col: 1 }, "D") =>
                        (new SidePosition(Side: new(3, 0), Relative: new(next.Col, sideSize - 1)), Direction.Left),
                    ({ Row: 2, Col: 1 }, "R") =>
                        (new SidePosition(Side: new(0, 2), Relative: new(sideSize - 1 - next.Row, sideSize - 1)), Direction.Left),
                    ({ Row: 2, Col: 1 }, "L") =>
                        (new SidePosition(Side: new(2, 0), Relative: new(next.Row, sideSize - 1)), Direction.Left),
                    ({ Row: 2, Col: 1 }, "U") =>
                        (new SidePosition(Side: new(1, 1), Relative: new(sideSize - 1, next.Col)), Direction.Up),

                    ({ Row: 2, Col: 0 }, "D") =>
                        (new SidePosition(Side: new(3, 0), Relative: new(0, next.Col)), Direction.Down),
                    ({ Row: 2, Col: 0 }, "R") =>
                        (new SidePosition(Side: new(2, 1), Relative: new(next.Row, 0)), Direction.Right),
                    ({ Row: 2, Col: 0 }, "L") =>
                        (new SidePosition(Side: new(0, 1), Relative: new(sideSize - 1 - next.Row, 0)), Direction.Right),
                    ({ Row: 2, Col: 0 }, "U") =>
                        (new SidePosition(Side: new(1, 1), Relative: new(next.Col, 0)), Direction.Right),

                    ({ Row: 3, Col: 0 }, "D") =>
                        (new SidePosition(Side: new(0, 2), Relative: new(0, next.Col)), Direction.Down),
                    ({ Row: 3, Col: 0 }, "R") =>
                        (new SidePosition(Side: new(2, 1), Relative: new(sideSize - 1, next.Row)), Direction.Up),
                    ({ Row: 3, Col: 0 }, "L") =>
                        (new SidePosition(Side: new(0, 1), Relative: new(0, next.Row)), Direction.Down),
                    ({ Row: 3, Col: 0 }, "U") =>
                        (new SidePosition(Side: new(2, 0), Relative: new(sideSize - 1, next.Col)), Direction.Up),

                    ({ Row: 0, Col: 2 }, "D") =>
                        (new SidePosition(Side: new(1, 1), Relative: new(next.Col, sideSize - 1)), Direction.Left),
                    ({ Row: 0, Col: 2 }, "R") =>
                        (new SidePosition(Side: new(2, 1), Relative: new(sideSize - 1 - next.Row, sideSize - 1)), Direction.Left),
                    ({ Row: 0, Col: 2 }, "L") =>
                        (new SidePosition(Side: new(0, 1), Relative: new(next.Row, sideSize - 1)), Direction.Left),
                    ({ Row: 0, Col: 2 }, "U") =>
                        (new SidePosition(Side: new(3, 0), Relative: new(sideSize - 1, next.Col)), Direction.Up),
                    
                    _ => throw new Exception("Should not be here."),
                };
            }
            
            private record SidePosition(Position Side, Position Relative)
            {
                public static SidePosition FromAbsolute(Position position, int sideSize)
                {
                    static (int div, int rem) DivRem(int x, int y)
                    {
                        var div = Math.DivRem(x, y, out var rem);
                        return (div, rem);
                    }

                    var (sideRow, row) = DivRem(position.Row, sideSize);
                    var (sideCol, col) = DivRem(position.Col, sideSize);
                    return new SidePosition(new Position(sideRow, sideCol), new Position(row, col));
                }

                public static Position ToAbsolute(SidePosition position, int sideSize)
                {
                    var row = position.Side.Row * sideSize + position.Relative.Row;
                    var col = position.Side.Col * sideSize + position.Relative.Col;
                    return new Position(row, col);
                }
            }
        }

        private static (Grid, IReadOnlyList<Instruction>) Parse(IEnumerable<string> lines)
        {
            var groups = lines.SplitByEmptyLine().Take(2).ToList();

            var grid = Grid.Parse(groups[0]);
            var instructions = Instruction.ParseAll(groups[1][0]);

            return (grid, instructions);
        }

        private enum Rotation { CW, CCW }

        private sealed class Direction
        {
            public static readonly Direction Up = new(-1, 0);
            public static readonly Direction Down = new(1, 0);
            public static readonly Direction Left = new(0, -1);
            public static readonly Direction Right = new(0, 1);

            private Direction(int DeltaRow, int DeltaCol)
            {
                this.DeltaRow = DeltaRow;
                this.DeltaCol = DeltaCol;
            }

            public int DeltaRow { get; }
            public int DeltaCol { get; }

            public Direction Rotate(Rotation rotation) =>
                rotation == Rotation.CW ? RotateCW() : RotateCCW();

            public Direction RotateCW()
            {
                if (this == Direction.Up) return Direction.Right;
                if (this == Direction.Right) return Direction.Down;
                if (this == Direction.Down) return Direction.Left;
                /* if (this == Direction.Left) */ return Direction.Up;
            }

            public Direction RotateCCW()
            {
                if (this == Direction.Up) return Direction.Left;
                if (this == Direction.Left) return Direction.Down;
                if (this == Direction.Down) return Direction.Right;
                /* if (this == Direction.Right) */ return Direction.Up;
            }

            public int Score()
            {
                if (this == Direction.Right) return 0;
                if (this == Direction.Down) return 1;
                if (this == Direction.Left) return 2;
                /* if (this == Direction.Up) */ return 3;
            }

            public override string ToString()
            {
                if (this == Direction.Right) return "R";
                if (this == Direction.Down) return "D";
                if (this == Direction.Left) return "L";
                /* if (this == Direction.Up) */ return "U";
            }
        }

        private abstract class Instruction
        {
            public static IReadOnlyList<Instruction> ParseAll(string text)
            {
                static IEnumerable<string> Tokenize(string text)
                {
                    var buffer = new StringBuilder();

                    foreach (var ch in text)
                    {
                        if (ch == 'L' || ch == 'R')
                        {
                            if (buffer.Length > 0)
                            {
                                yield return buffer.ToString();
                                buffer.Clear();
                            }

                            yield return ch.ToString();
                        }
                        else
                        {
                            buffer.Append(ch);
                        }
                    }

                    if (buffer.Length > 0)
                    {
                        yield return buffer.ToString();
                    }
                }

                static Instruction ParseOne(string token) =>
                    token switch
                    {
                        "L" => new Rotate(Rotation.CCW),
                        "R" => new Rotate(Rotation.CW),
                        var t => new Walk(int.Parse(t))
                    };

                return Tokenize(text).Select(ParseOne).ToList();
            }

            public sealed class Walk : Instruction
            {
                public Walk(int times)
                {
                    Times = times;
                }

                public int Times { get; }
            }

            public sealed class Rotate : Instruction
            {
                public Rotate(Rotation rotation)
                {
                    Rotation = rotation;
                }

                public Rotation Rotation { get; }
            }
        }

        private record Position(int Row, int Col)
        {
            public Position Move(Direction direction) =>
                new Position(Row + direction.DeltaRow, Col + direction.DeltaCol);
        }

        private record Actor(Position Position, Direction Direction)
        {
            public Actor Rotate(Rotation rotation) => 
                this with { Direction = Direction.Rotate(rotation) };
            
            public int FinalPassword() =>
                (Position.Row + 1) * 1000 + (Position.Col + 1) * 4 + Direction.Score();
        }

        private sealed class Grid
        {
            public static Grid Parse(IReadOnlyList<string> lines)
            {
                var cols = lines.Max(l => l.Length);
                var rows = lines.Count;

                var cells = new char[rows, cols];

                for (var r = 0; r < rows; r++)
                {
                    var line = lines[r];
                    line = line.Length == cols ? line : line.PadRight(cols);

                    for (var c = 0; c < cols; c++)
                    {
                        cells[r, c] = line[c];
                    }
                }

                return new Grid(cells);
            }

            private readonly char[,] cells;

            public Grid(char[,] cells)
            {
                this.cells = cells;
            }

            public int Rows => this.cells.GetLength(0);
            public int Cols => this.cells.GetLength(1);

            public char At(Position p) => this.cells[p.Row, p.Col];

            public bool InBounds(Position p) =>
                0 <= p.Row && p.Row < Rows &&
                0 <= p.Col && p.Col < Cols;

            public Position InitialPosition()
            {
                var row = 0;

                for (var col = 0; col < this.Cols; col++)
                {
                    if (this.cells[row, col] == '.')
                    {
                        return new Position(row, col);
                    }
                }

                return null;
            }

            public void Draw()
            {
                for (var r = 0; r < this.Rows; r++)
                {
                    for (var c = 0; c < this.Cols; c++)
                    {
                        Console.Write(this.cells[r, c]);
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
        }
    }
}
