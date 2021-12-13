using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Computer = AdventOfCode2019.Day09.Computer;

namespace AdventOfCode2019
{
    static class Day17
    {
        public static readonly IInput SampleInput =
            Input.Literal();

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/17/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();

                var computer = Computer.Of(program);
                computer.Execute();

                var lines = computer.Output.AsciiLines()
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();

                var grid = new Grid(lines);
                grid.Draw();

                var answer = Intersections(grid).Select(p => p.row * p.col).Sum();
                Console.WriteLine(answer);
            }

            private IReadOnlyList<(int row, int col)> Intersections(Grid grid)
            {
                var deltas = new[]
                {
                    (dr:  0, dc:  0),
                    (dr: -1, dc:  0),
                    (dr:  0, dc:  1),
                    (dr:  1, dc:  0),
                    (dr:  0, dc: -1)
                };

                var intersections = new List<(int row, int col)>();

                for (var row = 1; row < grid.Rows - 1; row++)
                {
                    for (var col = 1; col < grid.Cols - 1; col++)
                    {
                        var isIntersection = deltas.Select(d => grid.At(row + d.dr, col + d.dc)).All(IsScaffold);
                        if (isIntersection)
                        {
                            intersections.Add((row, col));
                        }
                    }
                }

                return intersections;
            }

            private bool IsScaffold(char ch) => ch == '#' || IsRobot(ch);

            private static bool IsRobot(char ch) => ch == '^' || ch == 'v' || ch == '<' || ch == '>';
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();

                var grid = InspectScaffolding(program);
                var robot = FindRobot(grid);
                var commands = FindPath(grid, robot);
                var commandLines = CompressCommands(commands);
                
                if (commandLines.Count == 0)
                {
                    Console.WriteLine("ERROR: can't compress commands.");
                }

                var dust = SendCommands(program, commandLines);
                Console.WriteLine(dust);
            }

            private Grid InspectScaffolding(string program)
            {
                var computer = Computer.Of(program);
                computer.Execute();

                var lines = computer.Output.AsciiLines()
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();

                return new Grid(lines);
            }

            private Robot FindRobot(Grid grid)
            {
                for (var row = 0; row < grid.Rows; row++)
                {
                    for (var col = 0; col < grid.Cols; col++)
                    {
                        var ch = grid.At(row, col);
                        if (ch == '^')
                        {
                            return new Robot(Direction.Up, new Position(row, col));
                        }
                        else if (ch == '<')
                        {
                            return new Robot(Direction.Left, new Position(row, col));
                        }
                        else if (ch == '>')
                        {
                            return new Robot(Direction.Right, new Position(row, col));
                        }
                        else if (ch == 'v')
                        {
                            return new Robot(Direction.Down, new Position(row, col));
                        }
                    }
                }

                return null;
            }

            private IReadOnlyList<string> FindPath(Grid grid, Robot robot)
            {
                var commands = new List<int>();

                bool deadend;
                do
                {
                    deadend = false;

                    if (grid.At(robot.PositionAhead()) == '#')
                    {
                        robot = robot.MoveForward();

                        if (commands[commands.Count - 1] > 0)
                        {
                            commands[commands.Count - 1]++;
                        }
                        else
                        {
                            commands.Add(1);
                        }
                    }
                    else if (grid.At(robot.PositionToLeft()) == '#')
                    {
                        robot = robot.RotateLeft();
                        commands.Add(-1);
                    }
                    else if (grid.At(robot.PositionToRight()) == '#')
                    {
                        robot = robot.RotateRight();
                        commands.Add(-2);
                    }
                    else
                    {
                        deadend = true;
                    }
                } while (!deadend);

                return commands
                    .Select(c => c switch
                    {
                        -1 => "L",
                        -2 => "R",
                        var i => i.ToString()
                    })
                    .ToList();
            }

            private IReadOnlyList<string> CompressCommands(IReadOnlyList<string> commands)
            {
                var success = Compress(commands, out var patterns, out var patternIndices);
                if (!success)
                {
                    return Array.Empty<string>();
                }

                var patternCodes = new[] { "A", "B", "C" };
                var patternCodesLine = string.Join(",", patternIndices.Select(i => patternCodes[i]));
                
                var patternLines = patterns.Select(pattern => string.Join(",", pattern));

                return patternLines.Prepend(patternCodesLine).ToList();
            }

            private bool Compress(
                IReadOnlyList<string> commands, 
                out IReadOnlyList<IReadOnlyList<string>> finalPatterns,
                out IReadOnlyList<int> finalPatternIndices)
            {
                var minItems = 2;
                var maxItems = 10;

                for (var a = maxItems; a >= minItems; a--)
                for (var b = maxItems; b >= minItems; b--)
                for (var c = maxItems; c >= minItems; c--)
                {
                    if (TryCompress(new[] { a, b, c }, commands, out finalPatterns, out finalPatternIndices) &&
                        finalPatternIndices.Count <= maxItems)
                    {
                        return true;
                    }
                }

                finalPatterns = null;
                finalPatternIndices = null;
                return false;
            }

            private bool TryCompress(
                IReadOnlyList<int> patternLengths, 
                IReadOnlyList<string> commands,
                out IReadOnlyList<IReadOnlyList<string>> finalPatterns,
                out IReadOnlyList<int> finalPatternIndices)
            {
                var remaining = new Slice<string>(commands, start: 0, length: commands.Count);

                var patterns = new List<Slice<string>>();
                var patternIndices = new List<int>();

                while (remaining.Length > 0)
                {
                    var index = patterns.FindIndex(pattern => remaining.StartsWith(pattern));
                    if (index >= 0)
                    {
                        patternIndices.Add(index);
                        remaining = remaining.Skip(patterns[index].Length);
                    }
                    else if (patterns.Count < patternLengths.Count)
                    {
                        var pattern = remaining.Take(patternLengths[patterns.Count]);
                        if (pattern.Length > 0)
                        {
                            patterns.Add(pattern);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (remaining.Length > 0)
                {
                    finalPatternIndices = null;
                    finalPatterns = null;
                    return false;
                }

                finalPatternIndices = patternIndices;
                finalPatterns = patterns.Select(p => p.ToList()).ToList();
                return true;
            }

            private long SendCommands(string program, IReadOnlyList<string> commandLines)
            {
                var computer = Computer.Of(program);
                computer.Mem.Write(address: 0, value: 2);
                computer.Input.EnterAsciiLines(commandLines.Append("n"));

                computer.Execute();

                return computer.Output.Values().Last();
            }

            private enum Direction { Up, Left, Down, Right }

            private class Robot
            {
                public Robot(Direction direction, Position position)
                {
                    Direction = direction;
                    Position = position;
                }

                public Direction Direction { get; }
                public Position Position { get; }

                public Position PositionAhead()
                {
                    if (Direction == Direction.Up) return new Position(Position.Row - 1, Position.Col);
                    if (Direction == Direction.Down) return new Position(Position.Row + 1, Position.Col);
                    if (Direction == Direction.Left) return new Position(Position.Row, Position.Col - 1);
                    return new Position(Position.Row, Position.Col + 1);
                }

                public Position PositionToLeft()
                {
                    if (Direction == Direction.Up) return new Position(Position.Row, Position.Col - 1);
                    if (Direction == Direction.Down) return new Position(Position.Row, Position.Col + 1);
                    if (Direction == Direction.Left) return new Position(Position.Row + 1, Position.Col);
                    return new Position(Position.Row - 1, Position.Col);
                }

                public Position PositionToRight()
                {
                    if (Direction == Direction.Up) return new Position(Position.Row, Position.Col + 1);
                    if (Direction == Direction.Down) return new Position(Position.Row, Position.Col - 1);
                    if (Direction == Direction.Left) return new Position(Position.Row - 1, Position.Col);
                    return new Position(Position.Row + 1, Position.Col);
                }

                public Robot MoveForward() => new Robot(Direction, PositionAhead());

                public Robot RotateLeft()
                {
                    if (Direction == Direction.Up) return new Robot(Direction.Left, Position);
                    if (Direction == Direction.Left) return new Robot(Direction.Down, Position);
                    if (Direction == Direction.Down) return new Robot(Direction.Right, Position);
                    return new Robot(Direction.Up, Position);
                }

                public Robot RotateRight()
                {
                    if (Direction == Direction.Up) return new Robot(Direction.Right, Position);
                    if (Direction == Direction.Right) return new Robot(Direction.Down, Position);
                    if (Direction == Direction.Down) return new Robot(Direction.Left, Position);
                    return new Robot(Direction.Up, Position);
                }
            }

            private class Slice<T>
            {
                private readonly IReadOnlyList<T> items;
                private readonly int start;

                public Slice(IReadOnlyList<T> items, int start, int length)
                {
                    this.items = items;
                    this.start = start;
                    this.Length = length;
                }

                public int Length;

                public T At(int index)
                {
                    if (index < Length)
                    {
                        return items[start + index];
                    }
                    throw new IndexOutOfRangeException();
                }

                public IReadOnlyList<T> ToList() => items.Skip(start).Take(Length).ToList();

                public override string ToString() => string.Join(", ", ToList());

                public bool StartsWith(Slice<T> other)
                {
                    if (Length < other.Length)
                    {
                        return false;
                    }

                    for (var i = 0; i < other.Length; i++)
                    {
                        if (!At(i).Equals(other.At(i)))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                public Slice<T> Skip(int count)
                {
                    if (count >= Length)
                    {
                        return new Slice<T>(items, 0, length: 0);
                    }
                    else
                    {
                        return new Slice<T>(items, start + count, Length - count);
                    }
                }

                public Slice<T> Take(int count)
                {
                    if (count <= Length)
                    {
                        return new Slice<T>(items, start, count);
                    }
                    else
                    {
                        return this;
                    }
                }
            }
        }

        private class Position
        {
            public Position(int row, int col)
            {
                Row = row;
                Col = col;
            }

            public int Row { get; }
            public int Col { get; }
        }

        private class Grid
        {
            private readonly IReadOnlyList<string> lines;

            public Grid(IReadOnlyList<string> lines)
            {
                this.lines = lines;
            }

            public int Rows => lines.Count;
            public int Cols => lines[0].Length;

            public void Draw()
            {
                foreach (var line in lines)
                {
                    Console.WriteLine(line);
                }
            }

            public char At(Position pos) => At(pos.Row, pos.Col);

            public char At(int row, int col)
            {
                if (0 <= row && row < Rows && 
                    0 <= col && col < Cols)
                {
                    return lines[row][col];
                }
                else
                {
                    return '?';
                }
            }
        }
    }
}
