using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace AdventOfCode2024;

static class Day15
{
    public static class Inputs
    {
        public static readonly IInput Debug =
            Input.Literal(""""""
#######
#...#.#
#.....#
#..OO@#
#..O..#
#.....#
#######

<vv<<^^<<^^
"""""");

        public static readonly IInput Sample =
            Input.Literal(""""""
##########
#..O..O.O#
#......O.#
#.OO..O.O#
#..O@..O.#
#O#..O...#
#O..O..O.#
#.OO.O.OO#
#....O...#
##########

<vv>^<v^>v>^vv^v>v<>v^v<v<^vv<<<^><<><>>v<vvv<>^v^>^<<<><<v<<<v^vv^v>^
vvv<<^>^v^^><<>>><>^<<><^vv^^<>vvv<>><^^v>^>vv<>v<<<<v<^v>^<^^>>>^<v<v
><>vv>v^v^<>><>>>><^^>vv>v<^^^>>v^v^<^^>v^^>v^<^v>v<>>v^v^<v>v^^<^^vv<
<<v<^>>^^^^>>>v^<>vvv^><v<<<>^^^vv^<vvv>^>v<^^^^v<>^>vvvv><>>v^<<^^^^^
^><^><>>><>^^<<^^v>>><^<v>^<vv>>v>>>^v><>^v><<<<v>>v<v<v>vvv>^<><<>^><
^>><>^v<><^vvv<^^<><v<<<<<><^v<<<><<<^^<v<^^^><^>>^<v^><<<^>>^v<v^v<v^
>^>>^v>vv>^<<^v<>><<><<v<<v><>v<^vv<<<>^^v^>^^>>><<^v>>v^v><^^>>^<>vv^
<><^^>^^^<><vvvvv^v<v<<>^v<v>v<<^><<><<><<<^^<<<^<<>><<><^^^>^^<>^>v<>
^^>vv<^v^v<vv>^<><v<^v>^^^>>>^^vvv^>vvv<>>>^<^>>>>>^<<^v>^vvv<>^<><<v>
v^^>>><<^^<>>^v^<v^vv<>v^<<>^<^v^v><^<<<><<^<v><v<>vv>>v><v^<vv<>v^<<^
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/15/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var (grid, robot, boxes, directions) = State.Parse(input.Lines());

            foreach (var direction in directions)
            {
                var (nextRobot, nextBoxes) = Simulation.TryMove(grid, robot, boxes, direction);
                robot = nextRobot;
                boxes = nextBoxes;
            }

            var sum = Box.SumCoordinates(boxes);
            Console.WriteLine(sum);
        }

        private record State(
            Grid Grid,
            Position Robot,
            IReadOnlyList<Box> Boxes,
            IReadOnlyList<Direction> Directions)
        {
            public static State Parse(IEnumerable<string> lines)
            {
                var groups = lines.SplitByEmptyLine().ToArray();

                var (grid, robot, boxes) = ParseGrid(groups[0]);
                var directions = DirectionUtil.ParseAll(groups[1]);

                return new State(grid, robot, boxes, directions);
            }

            private static (Grid, Position, IReadOnlyList<Box>) ParseGrid(IEnumerable<string> lines)
            {
                var grid = new List<IReadOnlyList<char>>();
                var robot = new Position(Row: 0, Col: 0);
                var boxes = new List<Box>();

                var row = 0;
                foreach (var line in lines)
                {
                    var cells = new List<char>();

                    var col = 0;
                    foreach (var ch in line)
                    {
                        if (ch == '@')
                        {
                            robot = new Position(row, col);
                            cells.Add('.');
                        }
                        else if (ch == 'O')
                        {
                            boxes.Add(new Box(new Position(row, col), Width: 1));
                            cells.Add('.');
                        }
                        else
                        {
                            cells.Add(ch);
                        }

                        col++;
                    }

                    grid.Add(cells);
                    row++;
                }

                return (new Grid(grid), robot, boxes);
            }
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var (grid, robot, boxes, directions) = State.Parse(input.Lines());

            foreach (var direction in directions)
            {
                var (nextRobot, nextBoxes) = Simulation.TryMove(grid, robot, boxes, direction);
                robot = nextRobot;
                boxes = nextBoxes;
            }

            var sum = Box.SumCoordinates(boxes);
            Console.WriteLine(sum);
        }

        // private static void Draw(
        //     Grid grid,
        //     Position robot,
        //     IReadOnlyList<Box> boxes)
        // {
        //     for (var row = 0; row < grid.Rows; row++)
        //     {
        //         for (var col = 0; col < grid.Cols; col++)
        //         {
        //             var pos = new Position(row, col);

        //             if (pos.Equals(robot))
        //             {
        //                 Console.Write('@');
        //             }
        //             else if (boxes.Any(b => b.Position.Equals(pos)))
        //             {
        //                 Console.Write("[]");
        //                 col++;
        //             }
        //             else
        //             {
        //                 Console.Write(grid.At(pos));
        //             }
        //         }
        //         Console.WriteLine();
        //     }
        //     Console.WriteLine();
        // }

        private record State(
            Grid Grid,
            Position Robot,
            IReadOnlyList<Box> Boxes,
            IReadOnlyList<Direction> Directions)
        {
            public static State Parse(IEnumerable<string> lines)
            {
                var groups = lines.SplitByEmptyLine().ToArray();

                var (grid, robot, boxes) = ParseGrid(groups[0].Select(PatchGridLine));
                var directions = DirectionUtil.ParseAll(groups[1]);

                return new State(grid, robot, boxes, directions);
            }

            private static string PatchGridLine(string line)
            {
                var sb = new StringBuilder();

                foreach (var ch in line)
                {
                    if (ch == '#')
                    {
                        sb.Append("##");
                    }
                    else if (ch == '.')
                    {
                        sb.Append("..");
                    }
                    else if (ch == '@')
                    {
                        sb.Append("@.");
                    }
                    else if (ch == 'O')
                    {
                        sb.Append("[]");
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }

                return sb.ToString();
            }

            private static (Grid, Position, IReadOnlyList<Box>) ParseGrid(IEnumerable<string> lines)
            {
                var grid = new List<IReadOnlyList<char>>();
                var robot = new Position(Row: 0, Col: 0);
                var boxes = new List<Box>();

                var row = 0;
                foreach (var line in lines)
                {
                    var cells = new List<char>();

                    var col = 0;
                    foreach (var ch in line)
                    {
                        if (ch == '@')
                        {
                            robot = new Position(row, col);
                            cells.Add('.');
                        }
                        else if (ch == '[')
                        {
                            boxes.Add(new Box(new Position(row, col), Width: 2));
                            cells.Add('.');
                        }
                        else if (ch == ']')
                        {
                            cells.Add('.');
                        }
                        else
                        {
                            cells.Add(ch);
                        }

                        col++;
                    }

                    grid.Add(cells);
                    row++;
                }

                return (new Grid(grid), robot, boxes);
            }
        }
    }

    private static class Simulation
    {
        public static (Position, IReadOnlyList<Box>) TryMove(
            Grid grid,
            Position robot,
            IReadOnlyList<Box> boxes,
            Direction direction)
        {
            var offset = DirectionUtil.ToPositionOffset(direction);
            var nextRobot = robot.Add(offset);

            var affectedBoxes = AffectedBoxes(robot, boxes, direction);
            if (affectedBoxes.Count > 0)
            {
                if (CanMoveAllBoxes(grid, affectedBoxes, direction))
                {
                    var nextBoxes = MoveAllBoxes(affectedBoxes, direction);
                    var stillBoxes = boxes.Except(affectedBoxes);
                    return (nextRobot, nextBoxes.Concat(stillBoxes).ToArray());
                }

                return (robot, boxes);
            }

            if (IsFreeAt(grid, nextRobot))
            {
                return (nextRobot, boxes);
            }

            return (robot, boxes);
        }

        private static IReadOnlyList<Box> AffectedBoxes(
            Position start,
            IReadOnlyList<Box> boxes,
            Direction direction)
        {
            var offset = DirectionUtil.ToPositionOffset(direction);

            var visit = new Queue<Position>();
            visit.Enqueue(start);

            var boxesHit = new HashSet<Box>();

            while (visit.Count > 0)
            {
                var pos = visit.Dequeue();

                var nextPos = pos.Add(offset);
                var boxHit = boxes
                    .Where(b => !boxesHit.Contains(b))
                    .FirstOrDefault(b => b.Contains(nextPos));

                if (boxHit is not null)
                {
                    visit.EnqueueRange(boxHit.Positions());
                    boxesHit.Add(boxHit);
                }
            }

            return boxesHit.ToArray();
        }

        private static bool CanMoveAllBoxes(
            Grid grid,
            IReadOnlyList<Box> boxes,
            Direction direction)
        {
            var offset = DirectionUtil.ToPositionOffset(direction);

            return boxes.All(b => b.Positions().All(p => IsFreeAt(grid, p.Add(offset))));
        }

        private static IReadOnlyList<Box> MoveAllBoxes(
            IReadOnlyList<Box> boxes,
            Direction direction)
        {
            var offset = DirectionUtil.ToPositionOffset(direction);

            return boxes
                .Select(b => b with { Position = b.Position.Add(offset) })
                .ToArray();
        }

        private static bool IsFreeAt(Grid grid, Position pos) =>
            grid.At(pos) == '.';
    }

    private record Box(Position Position, int Width = 1)
    {
        public static int SumCoordinates(IReadOnlyList<Box> boxes)
        {
            return boxes
                .Select(box => box.Position)
                .Select(pos => 100 * pos.Row + pos.Col)
                .Sum();
        }

        public bool Contains(Position pos) =>
            this.Positions().Any(p => p.Equals(pos));

        public IEnumerable<Position> Positions()
        {
            var offset = DirectionUtil.ToPositionOffset(Direction.Right);

            var pos = this.Position;
            for (var i = 0; i < this.Width; i++)
            {
                yield return pos;
                pos = pos.Add(offset);
            }
        }
    }

    private record Position(int Row, int Col)
    {
        public Position Add(Position p) =>
            new(p.Row + this.Row, p.Col + this.Col);
    }

    private enum Direction { Up, Down, Left, Right }

    private static class DirectionUtil
    {
        public static IReadOnlyList<Direction> ParseAll(IEnumerable<string> lines)
        {
            return lines
                .SelectMany(line => line
                    .Select(TryParse)
                    .Where(d => d is not null)
                )
                .Select(d => d!.Value)
                .ToArray();
        }

        public static Direction? TryParse(char ch) =>
            ch switch
            {
                '^' => Direction.Up,
                'v' => Direction.Down,
                '<' => Direction.Left,
                '>' => Direction.Right,

                _ => null
            };

        public static Position ToPositionOffset(Direction dir) =>
            dir switch
            {
                Direction.Up => new Position(Row: -1, Col: 0),
                Direction.Right => new Position(Row: 0, Col: 1),
                Direction.Down => new Position(Row: 1, Col: 0),
                Direction.Left => new Position(Row: 0, Col: -1),

                _ => throw new Exception("impossible")
            };
    }

    private class Grid
    {
        private readonly IReadOnlyList<IReadOnlyList<char>> cells;

        public Grid(IReadOnlyList<IReadOnlyList<char>> cells)
        {
            this.cells = cells;
        }

        public int Rows => this.cells.Count;
        public int Cols => this.cells[0].Count;

        public bool Contains(Position p) =>
            0 <= p.Row && p.Row < Rows &&
            0 <= p.Col && p.Col < Cols;

        public char At(Position p) =>
            Contains(p) ? this.cells[p.Row][p.Col] : '#';
    }
}
