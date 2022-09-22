using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdventOfCode2018
{
    static class Day17
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "x=495, y=2..7",
                    "y=7, x=495..501",
                    "x=501, y=3..7",
                    "x=498, y=2..4",
                    "x=506, y=1..2",
                    "x=498, y=10..13",
                    "x=504, y=10..13",
                    "y=13, x=498..504"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/17/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var lines = input.Lines().Select(Line.Parse).ToList();
                
                var grid = Grid.Of(lines);
                grid.Draw("initial");

                Grid.Simulate(grid, (500, 0));

                grid.Draw("final", true);

                var count = grid.CountWaterCells();
                Console.WriteLine($"Total water: {count}");
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var lines = input.Lines().Select(Line.Parse).ToList();

                var grid = Grid.Of(lines);
                grid.Draw("initial");

                Grid.Simulate(grid, (500, 0));

                grid.Draw("final", true);

                var count = grid.CountSettledWaterCells();
                Console.WriteLine($"Total settled water: {count}");
            }
        }

        private abstract class Line
        {
            public static Line Parse(string text)
            {
                // "y=b..c" -> "b..c"
                static string RemoveLabel(string text)
                {
                    var index = text.IndexOf('=');
                    return text.Substring(index + 1);
                }

                // "x=a" -> a
                static int ParseOne(string text) =>
                    int.Parse(RemoveLabel(text));

                // "y=b..c" -> (b, c)
                static (int, int) ParseTwo(string text)
                {
                    var parts = RemoveLabel(text).Split("..");
                    return (int.Parse(parts[0]), int.Parse(parts[1]));
                }

                // "x=a, y=b..c" -> (a, b, c)
                static (int, int, int) ParseThree(string text)
                {
                    var parts = text.Split(',');
                    var a = ParseOne(parts[0]);
                    var (b, c) = ParseTwo(parts[1]);
                    return (a, b, c);
                }

                var isVertical = text.StartsWith('x');
                var (a, b, c) = ParseThree(text);

                return isVertical
                    ? new Vertical(a, b, c)
                    : new Horizontal(a, b, c);
            }

            public static IEnumerable<int> Xs(Line line)
            {
                if (line is Line.Horizontal h)
                {
                    yield return h.X1;
                    yield return h.X2;
                }
                else if (line is Line.Vertical v)
                {
                    yield return v.X;
                }
            }

            public static IEnumerable<int> Ys(Line line)
            {
                if (line is Line.Horizontal h)
                {
                    yield return h.Y;
                }
                else if (line is Line.Vertical v)
                {
                    yield return v.Y1;
                    yield return v.Y2;
                }
            }

            private Line() { }

            public sealed class Horizontal : Line
            {
                public Horizontal(int y, int x1, int x2)
                {
                    Y = y;
                    X1 = x1;
                    X2 = x2;
                }

                public int Y { get; }
                public int X1 { get; }
                public int X2 { get; }
            }

            public sealed class Vertical : Line
            {
                public Vertical(int x, int y1, int y2)
                {
                    X = x;
                    Y1 = y1;
                    Y2 = y2;
                }

                public int X { get; }
                public int Y1 { get; }
                public int Y2 { get; }
            }
        }

        private static class Cell
        {
            public const char Empty = '.';
            public const char Wall = '#';
            public const char WaterSettled = '~';
            public const char WaterInMotion = '|';
            public const char WaterInfinite = '*';
        }

        private class Grid
        {
            public static Grid Of(IReadOnlyList<Line> lines)
            {
                var xs = lines.SelectMany(Line.Xs);
                var ys = lines.SelectMany(Line.Ys);

                var (minX, maxX) = (xs.Min(), xs.Max());
                var (minY, maxY) = (ys.Min(), ys.Max());

                var grid = new Grid(minX - 1, minY, maxX + 1, maxY);
                grid.SetLines(lines, Cell.Wall);
                return grid;
            }

            public static bool Simulate(Grid grid, (int x, int y) source)
            {
                var (fallLine, isInfinite) = grid.WaterFallLineFrom(source.x, source.y);
                grid.SetLine(fallLine, isInfinite ? Cell.WaterInfinite : Cell.WaterInMotion);

                grid.Draw("fall");

                if (isInfinite)
                {
                    return true;
                }

                var y = fallLine.Y2;
                while (fallLine.Y1 < y)
                {
                    var (leftLine, leftResult) = grid.WaterSpreadLineLeftFrom(fallLine.X, y);
                    var (rightLine, rightResult) = grid.WaterSpreadLineRightFrom(fallLine.X, y);

                    if (leftResult == SpreadResult.Walled && rightResult == SpreadResult.Walled)
                    {
                        grid.SetLine(leftLine, Cell.WaterSettled);
                        grid.SetLine(rightLine, Cell.WaterSettled);

                        grid.Draw("spread settled");

                        y--;
                    }
                    else
                    {
                        var done = true;

                        if (leftResult == SpreadResult.Infinite)
                        {
                            grid.SetLine(new Line.Horizontal(leftLine.Y, leftLine.X1, leftLine.X2 - 1), Cell.WaterInfinite);
                        }
                        else
                        {
                            grid.SetLine(leftLine, Cell.WaterInMotion);

                            if (leftResult == SpreadResult.FallsOff)
                            {
                                done &= Simulate(grid, (leftLine.X1 - 1, y));
                            }
                        }

                        if (rightResult == SpreadResult.Infinite)
                        {
                            grid.SetLine(new Line.Horizontal(rightLine.Y, rightLine.X1 + 1, rightLine.X2), Cell.WaterInfinite);
                        }
                        else
                        {
                            grid.SetLine(rightLine, Cell.WaterInMotion);

                            if (rightResult == SpreadResult.FallsOff)
                            {
                                done &= Simulate(grid, (rightLine.X2 + 1, y));
                            }
                        }

                        if (done)
                        {
                            grid.SetLine(new Line.Vertical(fallLine.X, fallLine.Y1, y), Cell.WaterInfinite);
                            grid.Draw("spread done");

                            return true;
                        }

                        grid.Draw("spread in motion");
                    }
                }

                return false;
            }

            private readonly int minX;
            private readonly int minY;
            private readonly int maxX;
            private readonly int maxY;
            private readonly char[,] cells;

            public Grid(int minX, int minY, int maxX, int maxY)
            {
                this.minX = minX;
                this.minY = minY;
                this.maxX = maxX;
                this.maxY = maxY;

                var width = maxX - minX + 1;
                var height = maxY - minY + 1;
                this.cells = new char[height, width];

                Fill(Cell.Empty);
            }

            public int CountWaterCells()
            {
                var count = 0;

                for (var y = this.minY; y <= this.maxY; y++)
                {
                    for (var x = this.minX; x <= this.maxX; x++)
                    {
                        var ch = this.GetCell(x, y);
                        if (ch == Cell.WaterSettled || ch == Cell.WaterInMotion || ch == Cell.WaterInfinite)
                        {
                            count++;
                        }
                    }
                }

                return count;
            }

            public int CountSettledWaterCells()
            {
                var count = 0;

                for (var y = this.minY; y <= this.maxY; y++)
                {
                    for (var x = this.minX; x <= this.maxX; x++)
                    {
                        var ch = this.GetCell(x, y);
                        if (ch == Cell.WaterSettled)
                        {
                            count++;
                        }
                    }
                }

                return count;
            }

            private static int drawCount = 0; 

            public void Draw(string title, bool force = false)
            {
                if (force || drawCount % 1000 == 0)
                {
                    //DrawToHtmlFile(title);
                    //DrawToConsole(title);
                }

                drawCount++;
            }

            private void DrawToConsole(string title)
            {
                Console.WriteLine($"--- {title} ---");

                for (var y = this.minY; y <= this.maxY; y++)
                {
                    for (var x = this.minX; x <= this.maxX; x++)
                    {
                        Console.Write(this.GetCell(x, y));
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.ReadLine();
            }

            private void DrawToHtmlFile(string title)
            {
                static string ClassOf(char ch)
                {
                    if (ch == Cell.Wall) return "w";
                    if (ch == Cell.WaterInMotion) return "u";
                    if (ch == Cell.WaterSettled) return "s";
                    if (ch == Cell.WaterInfinite) return "i";
                    return string.Empty;
                }

                var sb = new StringBuilder();

                sb.AppendLine("<html><head><style>");
                sb.AppendLine("div { display: flex; }");
                sb.AppendLine("i { width: 4px; height: 4px; }");
                sb.AppendLine(".w { background-color: gray; }");
                sb.AppendLine(".s { background-color: blue; }");
                sb.AppendLine(".u { background-color: lightblue; }");
                sb.AppendLine(".i { background-color: darkblue; }");
                sb.AppendLine("</style></head><body>");

                for (var y = this.minY; y <= this.maxY; y++)
                {
                    sb.AppendLine("<div>");
                    for (var x = this.minX; x <= this.maxX; x++)
                    {
                        var ch = this.GetCell(x, y);
                        sb.Append($"<i class=\"{ClassOf(ch)}\"></i>");

                    }
                    sb.AppendLine("</div>");
                }
                sb.AppendLine("</body></html>");

                File.WriteAllText("dump.html", sb.ToString());

                Console.WriteLine($"--- {title} ---");
                Console.ReadLine();
            }

            private void Fill(char ch)
            {
                for (var y = this.minY; y <= this.maxY; y++)
                {
                    for (var x = this.minX; x <= this.maxX; x++)
                    {
                        this.SetCell(x, y, ch);
                    }
                }
            }

            public void SetLines(IEnumerable<Line> lines, char ch)
            {
                foreach (var line in lines)
                {
                    this.SetLine(line, ch);
                }
            }

            public void SetLine(Line line, char ch)
            {
                if (line is Line.Horizontal horizontal)
                {
                    this.SetHorizontalLine(horizontal, ch);
                }
                else if (line is Line.Vertical vertical)
                {
                    this.SetVerticalLine(vertical, ch);
                }
            }

            public void SetVerticalLine(Line.Vertical line, char ch)
            {
                for (var y = line.Y1; y <= line.Y2; y++)
                {
                    this.SetCell(line.X, y, ch);
                }
            }

            public void SetHorizontalLine(Line.Horizontal line, char ch)
            {
                for (var x = line.X1; x <= line.X2; x++)
                {
                    this.SetCell(x, line.Y, ch);
                }
            }

            public char GetCell(int x, int y)
            {
                if (this.IsInBounds(x, y))
                {
                    return this.cells[y - this.minY, x - this.minX];
                }
                else
                {
                    return Cell.Empty;
                }
            }

            public void SetCell(int x, int y, char ch)
            {
                if (this.IsInBounds(x, y))
                {
                    this.cells[y - this.minY, x - this.minX] = ch;
                }
            }

            private bool IsInBounds(int x, int y) =>
                this.minX <= x && x <= this.maxX &&
                this.minY <= y && y <= this.maxY;

            public (Line.Vertical line, bool isInfinite) WaterFallLineFrom(int x, int y)
            {
                var y1 = y;
                var y2 = y;

                char ch = Cell.Empty;
                while (y2 <= this.maxY)
                {
                    ch = this.GetCell(x, y2);
                    if (ch == Cell.Wall || ch == Cell.WaterSettled || ch == Cell.WaterInfinite)
                    {
                        break;
                    }
                    y2++;
                }

                var isInfinite = ch == Cell.WaterInfinite || y2 > this.maxY;
                return (new Line.Vertical(x, y1, y2 - 1), isInfinite);
            }

            public (Line.Horizontal line, SpreadResult result) WaterSpreadLineLeftFrom(int x, int y)
            {
                var x1 = x;
                var x2 = x;
                SpreadResult result;

                while (true)
                {
                    var ch = this.GetCell(x1, y);
                    var canSpreadHere = ch == Cell.Empty || ch == Cell.WaterInMotion;

                    var below = this.GetCell(x1, y + 1);
                    var isSolidBelow = below == Cell.Wall || below == Cell.WaterSettled;

                    if (!canSpreadHere || !isSolidBelow) 
                    {
                        if (ch == Cell.WaterInfinite)
                        {
                            result = SpreadResult.Infinite;
                        }
                        else
                        {
                            result = canSpreadHere && !isSolidBelow ? SpreadResult.FallsOff : SpreadResult.Walled;
                        }

                        break;
                    }
                    x1--;
                }

                return (new Line.Horizontal(y, x1 + 1, x2), result);
            }

            public (Line.Horizontal line, SpreadResult result) WaterSpreadLineRightFrom(int x, int y)
            {
                var x1 = x;
                var x2 = x;
                SpreadResult result;

                while (true)
                {
                    var ch = this.GetCell(x2, y);
                    var canSpreadHere = ch == Cell.Empty || ch == Cell.WaterInMotion;

                    var below = this.GetCell(x2, y + 1);
                    var isSolidBelow = below == Cell.Wall || below == Cell.WaterSettled;

                    if (!canSpreadHere || !isSolidBelow)
                    {
                        if (ch == Cell.WaterInfinite)
                        {
                            result = SpreadResult.Infinite;
                        }
                        else
                        {
                            result = canSpreadHere && !isSolidBelow ? SpreadResult.FallsOff : SpreadResult.Walled;
                        }

                        break;
                    }
                    x2++;
                }

                return (new Line.Horizontal(y, x1, x2 - 1), result);
            }
        }

        private enum SpreadResult
        {
            Walled,
            FallsOff,
            Infinite
        }
    }
}
