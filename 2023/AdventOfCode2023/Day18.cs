using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdventOfCode2023;

static class Day18
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
R 6 (#70c710)
D 5 (#0dc571)
L 2 (#5713f0)
D 2 (#d2c081)
R 2 (#59c680)
D 2 (#411b91)
L 5 (#8ceee2)
U 2 (#caa173)
L 1 (#1b58a2)
U 2 (#caa171)
R 2 (#7807d2)
U 3 (#a77fa3)
L 2 (#015232)
U 2 (#7a21e3)
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/18/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var instructions = input.Lines().Select(Instruction.Parse).ToList();

            var (grid, start) = Grid.Create(instructions);
            var fillSize = FloodFill.FillSize(grid, start);

            Console.WriteLine(fillSize);
        }

        private record Position(int Row, int Col)
        {
            public Position Up(int times = 1) => this with { Row = Row - times };
            public Position Down(int times = 1) => this with { Row = Row + times };
            public Position Left(int times = 1) => this with { Col = Col - times };
            public Position Right(int times = 1) => this with { Col = Col + times };

            public Position Move(Direction dir, int times = 1) =>
                dir switch
                {
                    Direction.Up => Up(times),
                    Direction.Down => Down(times),
                    Direction.Left => Left(times),
                    Direction.Right => Right(times),
                    _ => this,
                };
        }

        private record Instruction(Direction Direction, int Length, string Color)
        {
            public static Instruction Parse(string text)
            {
                static Direction ParseDirection(string text) =>
                    text switch
                    {
                        "U" => Direction.Up,
                        "D" => Direction.Down,
                        "L" => Direction.Left,
                        "R" => Direction.Right,

                        _ => throw new Exception("impossible"),
                    };

                static string ParseColor(string text) =>
                    TrimSuffix(TrimPrefix(text, "("), ")");

                var parts = text.Split(' ');

                var direction = ParseDirection(parts[0]);
                var length = int.Parse(parts[1]);
                var color = ParseColor(parts[2]);

                return new Instruction(direction, length, color);
            }
        }

        private sealed class Grid
        {
            public static (Grid grid, Position start) Create(IReadOnlyList<Instruction> instructions)
            {
                var (start, size) = DetermineSize(instructions);

                var grid = new Grid(size.Row, size.Col);
                Apply(grid, instructions, start);

                return (grid, start);
            }

            private static (Position start, Position size) DetermineSize(IReadOnlyList<Instruction> instructions)
            {
                static IEnumerable<Position> Positions(IReadOnlyList<Instruction> instructions)
                {
                    var pos = new Position(Row: 0, Col: 0);
                    yield return pos;

                    foreach (var instruction in instructions)
                    {
                        pos = pos.Move(instruction.Direction, instruction.Length);
                        yield return pos;
                    }
                }

                static (Position topLeft, Position bottomRight) BoundingBox(IReadOnlyList<Instruction> instructions) =>
                    Positions(instructions).Aggregate(
                        seed: (
                            topLeft: new Position(
                                Row: int.MaxValue,
                                Col: int.MaxValue
                            ),
                            bottomRight: new Position(
                                Row: int.MinValue,
                                Col: int.MinValue
                            )
                        ),
                        func: (acc, pos) => (
                            topLeft: new Position(
                                Row: Math.Min(acc.topLeft.Row, pos.Row),
                                Col: Math.Min(acc.topLeft.Col, pos.Col)
                            ),
                            bottomRight: new Position(
                                Row: Math.Max(acc.bottomRight.Row, pos.Row),
                                Col: Math.Max(acc.bottomRight.Col, pos.Col)
                            )
                        )
                    );

                var (topLeft, bottomRight) = BoundingBox(instructions);

                var size = new Position(
                    Row: bottomRight.Row - topLeft.Row + 1,
                    Col: bottomRight.Col - topLeft.Col + 1
                );

                var start = new Position(
                    Row: -topLeft.Row,
                    Col: -topLeft.Col
                );

                return (start, size);
            }

            private static void Apply(Grid grid, IReadOnlyList<Instruction> instructions, Position start)
            {
                var pos = start;
                grid.Set(pos, '#');

                foreach (var instruction in instructions)
                {
                    for (var i = 0; i < instruction.Length; i++)
                    {
                        pos = pos.Move(instruction.Direction);
                        grid.Set(pos, '#');
                    }
                }
            }

            private readonly char[,] cells;

            public Grid(int rows, int cols)
            {
                this.cells = new char[rows, cols];

                for (var row = 0; row < rows; row++)
                {
                    for (var col = 0; col < cols; col++)
                    {
                        this.cells[row, col] = '.';
                    }
                }
            }

            private Grid(char[,] cells)
            {
                this.cells = cells;
            }

            public int Rows => this.cells.GetLength(0);
            public int Cols => this.cells.GetLength(1);

            public Grid Clone()
            {
                var cells = new char[Rows, Cols];

                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Cols; col++)
                    {
                        cells[row, col] = this.cells[row, col];
                    }
                }

                return new Grid(cells);
            }

            public bool InBounds(Position pos) =>
                0 <= pos.Row && pos.Row < Rows &&
                0 <= pos.Col && pos.Col < Cols;

            public char At(Position pos) =>
                this.cells[pos.Row, pos.Col];

            public void Set(Position pos, char value)
            {
                this.cells[pos.Row, pos.Col] = value;
            }

            public int Count(char ch)
            {
                var count = 0;

                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Cols; col++)
                    {
                        var pos = new Position(row, col);
                        if (At(pos) == ch)
                        {
                            count++;
                        }
                    }
                }

                return count;
            }
        }

        private static class FloodFill
        {
            public static int FillSize(Grid grid, Position start)
            {
                var fillStarts = DetermineFillStartPositions(grid, start);
                return Fill(grid, fillStarts);
            }

            private static IReadOnlyList<Position> DetermineFillStartPositions(Grid grid, Position start)
            {
                static bool IsAt(Grid grid, Position pos, char ch) =>
                    grid.InBounds(pos) && grid.At(pos) == ch;

                var type = (
                    u: IsAt(grid, start.Move(Direction.Up), '#'),
                    d: IsAt(grid, start.Move(Direction.Down), '#'),
                    l: IsAt(grid, start.Move(Direction.Left), '#'),
                    r: IsAt(grid, start.Move(Direction.Right), '#')
                );

                var possibilities = type switch
                {
                    // .#.
                    // .#.
                    // .#.
                    (u: true, d: true, l: false, r: false) => new[]
                    {
                    new[]
                    {
                        start.Move(Direction.Left),
                        start.Move(Direction.Left).Move(Direction.Up),
                        start.Move(Direction.Left).Move(Direction.Down),
                    },
                    new[]
                    {
                        start.Move(Direction.Right),
                        start.Move(Direction.Right).Move(Direction.Up),
                        start.Move(Direction.Right).Move(Direction.Down),
                    }
                },

                    // ...
                    // ###
                    // ...
                    (u: false, d: false, l: true, r: true) => new[]
                    {
                    new[]
                    {
                        start.Move(Direction.Up),
                        start.Move(Direction.Up).Move(Direction.Left),
                        start.Move(Direction.Up).Move(Direction.Right),
                    },
                    new[]
                    {
                        start.Move(Direction.Down),
                        start.Move(Direction.Down).Move(Direction.Left),
                        start.Move(Direction.Down).Move(Direction.Right),
                    }
                },

                    // ...
                    // .##
                    // .#.
                    (u: false, d: true, l: false, r: true) => new[]
                    {
                    new[]
                    {
                        start.Move(Direction.Up),
                        start.Move(Direction.Up).Move(Direction.Left),
                        start.Move(Direction.Up).Move(Direction.Right),
                        start.Move(Direction.Left),
                        start.Move(Direction.Down).Move(Direction.Left)
                    },
                    new[]
                    {
                        start.Move(Direction.Down).Move(Direction.Right),
                    }
                },

                    // ...
                    // ##.
                    // .#.
                    (u: false, d: true, l: true, r: false) => new[]
                    {
                    new[]
                    {
                        start.Move(Direction.Up),
                        start.Move(Direction.Up).Move(Direction.Left),
                        start.Move(Direction.Up).Move(Direction.Right),
                        start.Move(Direction.Right),
                        start.Move(Direction.Up).Move(Direction.Right)
                    },
                    new[]
                    {
                        start.Move(Direction.Down).Move(Direction.Left),
                    }
                },

                    // .#.
                    // ##.
                    // ...
                    (u: true, d: false, l: true, r: false) => new[]
                    {
                    new[]
                    {
                        start.Move(Direction.Down),
                        start.Move(Direction.Down).Move(Direction.Left),
                        start.Move(Direction.Down).Move(Direction.Right),
                        start.Move(Direction.Right),
                        start.Move(Direction.Up).Move(Direction.Right)
                    },
                    new[]
                    {
                        start.Move(Direction.Up).Move(Direction.Left),
                    }
                },

                    // .#.
                    // .##
                    // ...
                    (u: true, d: false, l: false, r: true) => new[]
                    {
                    new[]
                    {
                        start.Move(Direction.Down),
                        start.Move(Direction.Down).Move(Direction.Left),
                        start.Move(Direction.Down).Move(Direction.Right),
                        start.Move(Direction.Left),
                        start.Move(Direction.Up).Move(Direction.Left)
                    },
                    new[]
                    {
                        start.Move(Direction.Up).Move(Direction.Right),
                    }
                },

                    _ => Array.Empty<Position[]>()
                };

                return possibilities
                    .SelectMany(positions =>
                    {
                        var inBounds = positions.FirstOrDefault(grid.InBounds);
                        return inBounds is not null ? new[] { inBounds } : Array.Empty<Position>();
                    })
                    .ToList();
            }

            private static int Fill(Grid grid, IReadOnlyList<Position> starts)
            {
                foreach (var start in starts)
                {
                    var count = TryFill(grid.Clone(), start);
                    if (count >= 0)
                    {
                        return count;
                    }
                }

                return -1;
            }

            private static int TryFill(Grid grid, Position start)
            {
                var visit = new Queue<Position>();
                visit.Enqueue(start);

                while (visit.Count > 0)
                {
                    var pos = visit.Dequeue();

                    if (pos.Row == 0 || pos.Row == grid.Rows - 1 ||
                        pos.Col == 0 || pos.Col == grid.Cols - 1)
                    {
                        return -1;
                    }

                    if (grid.At(pos) == '#')
                    {
                        continue;
                    }

                    grid.Set(pos, '#');

                    var nexts = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right }
                        .Select(dir => pos.Move(dir))
                        .Where(pos => grid.InBounds(pos) && grid.At(pos) != '#');

                    foreach (var next in nexts)
                    {
                        visit.Enqueue(next);
                    }
                }

                return grid.Count('#');
            }
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var instructions = input.Lines().Select(Instruction.Parse).ToList();

            var lines = ConvertToLines(instructions);
            var boundingBox = BoundingBox(lines);
            var boundingBoxArea = boundingBox.Area();

            var boxes = SplitIntoBoxes(lines);
            boxes = AssignBoxEdges(boxes, lines);

            var tainted = Taint(boxes);
            var taintedArea = CalculateTaintedArea(boxes, tainted);

            var result = boundingBoxArea - taintedArea;
            Console.WriteLine(result);
        }

        private static IReadOnlyList<Line> ConvertToLines(IReadOnlyList<Instruction> instructions)
        {
            static Point EndPoint(Instruction instruction, Point start) =>
                instruction.Direction switch
                {
                    Direction.Right => start with { X = start.X + instruction.Length },
                    Direction.Left => start with { X = start.X - instruction.Length },
                    Direction.Up => start with { Y = start.Y - instruction.Length },
                    Direction.Down => start with { Y = start.Y + instruction.Length },
                    
                    _ => throw new Exception("impossible"),
                };

            var start = new Point(0, 0);
            
            var lines = new List<Line>();
            foreach (var instruction in instructions)
            {
                var end = EndPoint(instruction, start);
                lines.Add(Line.Of(start, end));
                start = end;
            }

            return lines;
        }

        private static Box BoundingBox(IReadOnlyList<Line> lines)
        {
            static (long min, long max) MinMax(IEnumerable<long> items) =>
                items.Aggregate(
                    (min: long.MaxValue, max: long.MinValue),
                    (acc, item) => (min: Math.Min(acc.min, item), max: Math.Max(acc.max, item))
                );

            var (minX, maxX) = MinMax(lines.SelectMany(l => new[] { l.Start.X, l.End.X }));
            var (minY, maxY) = MinMax(lines.SelectMany(l => new[] { l.Start.Y, l.End.Y }));

            return new Box(
                Origin: new Point(minX, minY),
                Width: maxX - minX + 1,
                Height: maxY - minY + 1,
                Edges: BoxEdges.None
            );
        }

        private static IReadOnlyList<IReadOnlyList<Box>> SplitIntoBoxes(IReadOnlyList<Line> lines)
        {
            var xs = lines
                .Where(line => line.IsVertical)
                .Select(l => l.Start.X)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var ys = lines
                .Where(line => line.IsHorizontal)
                .Select(l => l.Start.Y)
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            var boxes = new List<IReadOnlyList<Box>>();

            for (var yi = 0; yi < ys.Count; yi++)
            {
                var row = new List<Box>();

                var y1 = ys[yi];
                var y2 = yi == ys.Count - 1 ? y1 + 1 : ys[yi + 1];

                for (var xi = 0; xi < xs.Count; xi++)
                {
                    var x1 = xs[xi];
                    var x2 = xi == xs.Count - 1 ? x1 + 1 : xs[xi + 1];

                    var box = new Box(
                        Origin: new Point(x1, y1),
                        Width: x2 - x1,
                        Height: y2 - y1,
                        Edges: BoxEdges.None
                    );

                    row.Add(box);
                }

                boxes.Add(row);
            }

            return boxes;
        }

        private static IReadOnlyList<IReadOnlyList<Box>> AssignBoxEdges(
            IReadOnlyList<IReadOnlyList<Box>> boxes,
            IReadOnlyList<Line> lines)
        {
            var verticalLinesByX = lines
                .Where(l => l.IsVertical)
                .GroupBy(l => l.Start.X)
                .ToDictionary(g => g.Key, g => g.ToList());

            var horizontalLinesByY = lines
                .Where(l => l.IsHorizontal)
                .GroupBy(l => l.Start.Y)
                .ToDictionary(g => g.Key, g => g.ToList());

            bool AnyFullyContains(Line edge)
            {
                var vertical = false;
                var horizontal = false;

                if (edge.IsVertical)
                {
                    if (verticalLinesByX.TryGetValue(edge.Start.X, out var verticalLines))
                    {
                        vertical = verticalLines.Any(line => line.FullyContains(edge));
                    }
                }
                if (edge.IsHorizontal)
                {
                    if (horizontalLinesByY.TryGetValue(edge.Start.Y, out var horizontalLines))
                    {
                        horizontal = horizontalLines.Any(line => line.FullyContains(edge));
                    }
                }

                return vertical || horizontal;
            }

            return boxes
                .Select(row => row
                    .Select(box => box with
                    {
                        Edges = new BoxEdges(
                            Top: AnyFullyContains(box.TopEdgeLine()),
                            Bottom: AnyFullyContains(box.BottomEdgeLine()),
                            Left: AnyFullyContains(box.LeftEdgeLine()),
                            Right: AnyFullyContains(box.RightEdgeLine()),
                            TopLeftCorner: false
                        )
                    })
                    .Select(box =>
                    {
                        // With the way the space is split into boxes, a box may
                        // * have no # characters
                        // * have # characters along any edges
                        // * have a single # character in the top-left corner 
                        //
                        // The first two cases are covered above, the last one is
                        // checked here. Such top-left corner boxes are a special
                        // case: they behave like boxes without any edges in all 
                        // cases except for the area calculation, in which case the 
                        // corner is subtracted from the covered area.

                        var noEdges = box.Edges.Equals(BoxEdges.None);
                        var topLeftCorner = new Line(box.Origin, box.Origin);
                        
                        return noEdges && AnyFullyContains(topLeftCorner)
                            ? box with { Edges = box.Edges with { TopLeftCorner = true } }
                            : box;
                    })
                    .ToList()
                )
                .ToList();
        }

        private static bool[,] Taint(IReadOnlyList<IReadOnlyList<Box>> boxes)
        {
            static void EnqueueBoundaries(Queue<(int row, int col)> visit, int rows, int cols)
            {
                visit.EnqueueRange(
                    Enumerable.Range(start: 0, count: cols)
                        .Select(col => (row: 0, col))
                );
                visit.EnqueueRange(
                    Enumerable.Range(start: 0, count: cols)
                        .Select(col => (row: rows - 1, col))
                );
                visit.EnqueueRange(
                    Enumerable.Range(start: 0, count: rows)
                        .Skip(1).SkipLast(1)
                        .Select(row => (row, col: 0))
                );
                visit.EnqueueRange(
                    Enumerable.Range(start: 0, count: rows)
                        .Skip(1).SkipLast(1)
                        .Select(row => (row, col: cols - 1))
                );
            }

            static bool IsTainted(
                IReadOnlyList<IReadOnlyList<Box>> boxes,
                bool[,] tainted, 
                int row, 
                int col)
            {
                var box = boxes[row][col];

                var rows = tainted.GetLength(0);
                var cols = tainted.GetLength(1);

                var upTainted = row == 0 || tainted[row - 1, col];
                var upHasBottom = row == 0 ? false : boxes[row - 1][col].Edges.Bottom;
                if (upTainted && !(upHasBottom || box.Edges.Top))
                {
                    return true;
                }

                var downTainted = row == rows - 1 || tainted[row + 1, col];
                var downHasTop = row == rows - 1 ? false : boxes[row + 1][col].Edges.Top;
                if (downTainted && !(downHasTop || box.Edges.Bottom))
                {
                    return true;
                }

                var leftTainted = col == 0 || tainted[row, col - 1];
                var leftHasRight = col == 0 ? false : boxes[row][col - 1].Edges.Right;
                if (leftTainted && !(leftHasRight || box.Edges.Left))
                {
                    return true;
                }

                var rightTainted = col == cols - 1 || tainted[row, col + 1];
                var rightHasLeft = col == cols - 1 ? false : boxes[row][col + 1].Edges.Left;
                if (rightTainted && !(rightHasLeft || box.Edges.Right))
                {
                    return true;
                }

                return false;
            }

            var rows = boxes.Count;
            var cols = boxes[0].Count;

            var tainted = new bool[rows, cols];

            var visit = new Queue<(int row, int col)>();
            EnqueueBoundaries(visit, rows, cols);

            while (visit.Count > 0)
            {
                var (row, col) = visit.Dequeue();

                var inBounds = 0 <= row && row < rows &&
                               0 <= col && col < cols;
                if (!inBounds)
                {
                    continue;
                }

                if (tainted[row, col])
                {
                    continue;
                }

                if (!IsTainted(boxes, tainted, row, col))
                {
                    continue;
                }

                tainted[row, col] = true;

                visit.Enqueue((row - 1, col));
                visit.Enqueue((row + 1, col));
                visit.Enqueue((row, col - 1));
                visit.Enqueue((row, col + 1));
            }

            return tainted;
        }

        private static long CalculateTaintedArea(IReadOnlyList<IReadOnlyList<Box>> boxes, bool[,] tainted)
        {
            var rows = boxes.Count;
            var cols = boxes[0].Count;

            var sum = 0L;
            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    if (!tainted[row, col])
                    {
                        continue;
                    }

                    sum += boxes[row][col].AreaExcludingEdges();
                }
            }

            return sum;
        }

        private record Instruction(Direction Direction, long Length)
        {
            public static Instruction Parse(string text)
            {
                static Direction ParseDirection(char ch) =>
                    ch switch
                    {
                        '0' => Direction.Right,
                        '1' => Direction.Down,
                        '2' => Direction.Left,
                        '3' => Direction.Up,

                        _ => throw new Exception("impossible"),
                    };

                var parts = text.Split(' ');
                var color = TrimSuffix(TrimPrefix(parts[2], "(#"), ")");

                var length = Convert.ToInt64(color[..5], fromBase: 16);
                var direction = ParseDirection(color[5]);

                return new Instruction(direction, length);
            }

            public static Instruction ParsePart1(string text)
            {
                static Direction ParseDirection(string text) =>
                    text switch
                    {
                        "U" => Direction.Up,
                        "D" => Direction.Down,
                        "L" => Direction.Left,
                        "R" => Direction.Right,

                        _ => throw new Exception("impossible"),
                    };

                var parts = text.Split(' ');

                var direction = ParseDirection(parts[0]);
                var length = long.Parse(parts[1]);
                
                return new Instruction(direction, length);
            }
        }

        private record Point(long X, long Y)
        {
            public override string ToString() => $"({X},{Y})";
        }

        private record Line(Point Start, Point End)
        {
            public static Line Of(Point a, Point b)
            {
                var start = new Point(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
                var end = new Point(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
                return new Line(start, end);
            }

            public bool IsHorizontal => Start.Y == End.Y;
            public bool IsVertical => Start.X == End.X;

            public override string ToString() => $"{Start}-{End}";

            public long Length()
            {
                if (IsHorizontal)
                {
                    return End.X - Start.X + 1;
                }
                if (IsVertical)
                {
                    return End.Y - Start.Y + 1;
                }
                return 0;
            }

            public bool FullyContains(Line other)
            {
                if (IsHorizontal && other.IsHorizontal)
                {
                    return Start.Y == other.Start.Y 
                        && Start.X <= other.Start.X && other.End.X <= End.X;
                }
                if (IsVertical && other.IsVertical)
                {
                    return Start.X == other.Start.X 
                        && Start.Y <= other.Start.Y && other.End.Y <= End.Y;
                }
                return false;
            }
        }

        private record Box(
            Point Origin, 
            long Width, 
            long Height,
            BoxEdges Edges)
        {
            public override string ToString() =>
                $"{Origin}, {Width} x {Height}, [{Edges}]";

            public Line TopEdgeLine()
            {
                var right = Origin.X + Width - 1;
                return new Line(Origin, new Point(right, Origin.Y));
            }

            public Line BottomEdgeLine()
            {
                var right = Origin.X + Width - 1;
                var bottom = Origin.Y + Height - 1;
                return new Line(new Point(Origin.X, bottom), new Point(right, bottom));
            }

            public Line LeftEdgeLine()
            {
                var bottom = Origin.Y + Height - 1;
                return new Line(Origin, new Point(Origin.X, bottom));
            }

            public Line RightEdgeLine()
            {
                var right = Origin.X + Width - 1;
                var bottom = Origin.Y + Height - 1;
                return new Line(new Point(right, Origin.Y), new Point(right, bottom));
            }

            public long Area() => Width * Height;

            public long AreaExcludingEdges()
            {
                var area = Area();

                if (Edges.TopLeftCorner)
                {
                    return area - 1;
                }

                if (Edges.Top)
                {
                    area -= TopEdgeLine().Length();
                }
                if (Edges.Bottom)
                {
                    area -= BottomEdgeLine().Length();
                }
                if (Edges.Left)
                {
                    area -= LeftEdgeLine().Length();
                }
                if (Edges.Right)
                {
                    area -= RightEdgeLine().Length();
                }

                if (Edges.Top && Edges.Left)
                {
                    area += 1;
                }
                if (Edges.Top && Edges.Right)
                {
                    area += 1;
                }
                if (Edges.Bottom && Edges.Left)
                {
                    area += 1;
                }
                if (Edges.Bottom && Edges.Right)
                {
                    area += 1;
                }

                return area;
            }
        }

        private record BoxEdges(
            bool Top,
            bool Bottom,
            bool Left,
            bool Right,
            bool TopLeftCorner)
        {
            public static readonly BoxEdges None = new(false, false, false, false, false);

            public override string ToString()
            {
                var sb = new StringBuilder();
                
                if (Top) sb.Append('t');
                if (Bottom) sb.Append('b');
                if (Left) sb.Append('l');
                if (Right) sb.Append('r');
                if (TopLeftCorner) sb.Append('*');

                return sb.ToString();
            }
        }
    }

    private enum Direction { Up, Down, Left, Right }

    private static string TrimPrefix(string text, string prefix) =>
        text.Substring(prefix.Length);

    private static string TrimSuffix(string text, string suffix) =>
        text.Substring(0, text.Length - suffix.Length);
}
