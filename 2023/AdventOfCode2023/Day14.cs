using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day14
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
O....#....
O.OO#....#
.....##...
OO.#O....O
.O.....O#.
O.#..O.#.#
..O..#O..O
.......O..
#....###..
#OO..#....
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/14/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var tiltedGrid = grid.TiltNorth();
            var load = tiltedGrid.CalculateNorthBeamLoad();

            Console.WriteLine(load);
        }       
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var grid = Grid.Parse(input.Lines());

            var tiltedGrid = grid;
            var seenGrids = new List<Grid>();
            var repeatCycle = -1;
            while (true)
            {
                tiltedGrid = tiltedGrid.TiltNorth().TiltWest().TiltSouth().TiltEast();
                repeatCycle = seenGrids.IndexOf(seenGrid => seenGrid.SameAs(tiltedGrid));
                if (repeatCycle > 0)
                {
                    break;
                }

                seenGrids.Add(tiltedGrid);
            }

            const long CYCLES = 1_000_000_000L;

            var cycle = repeatCycle + (CYCLES - repeatCycle) % (seenGrids.Count - repeatCycle) - 1;
            var load = seenGrids[(int)cycle].CalculateNorthBeamLoad();
            Console.WriteLine(load);
        }
    }

    private record Span(int Index, int Start, int End);

    private record VerticalSpan(int RowStart, int RowEnd, int Col);

    private record HorizontalSpan(int Row, int ColStart, int ColEnd);

    private record GridSpans(
        IReadOnlyList<HorizontalSpan> Horizontal, 
        IReadOnlyList<VerticalSpan> Vertical
    );

    private sealed class Grid
    {
        public static Grid Parse(IEnumerable<string> lines) =>
            new Grid(lines.Select(line => line.ToList()).ToList());

        private readonly IReadOnlyList<IReadOnlyList<char>> cells;
        private readonly GridSpans spans;

        public Grid(IReadOnlyList<IReadOnlyList<char>> cells)
        {
            this.cells = cells;
            this.spans = this.DetectSpans();
        }

        private Grid(
            IReadOnlyList<IReadOnlyList<char>> cells,
            GridSpans spans)
        {
            this.cells = cells;
            this.spans = spans;
        }

        public int Rows => this.cells.Count;
        public int Cols => this.cells[0].Count;

        public char At(int row, int col) => 
            this.cells[row][col];

        private GridSpans DetectSpans() =>
            new(DetectHorizontalSpans(), DetectVerticalSpans());

        private IReadOnlyList<VerticalSpan> DetectVerticalSpans() =>
            Enumerable.Range(start: 0, count: Cols)
                .SelectMany(DetectVerticalSpans)
                .ToList();

        private IReadOnlyList<HorizontalSpan> DetectHorizontalSpans() =>
            Enumerable.Range(start: 0, count: Rows)
                .SelectMany(DetectHorizontalSpans)
                .ToList();

        private IEnumerable<VerticalSpan> DetectVerticalSpans(int col)
        {
            var row = 0;
            while (row < Rows)
            {
                while (row < Rows)
                {
                    var cell = At(row, col);
                    if (cell != '#')
                    {
                        break;
                    }
                    row++;
                }

                var start = row;
                while (row < Rows)
                {
                    var cell = At(row, col);
                    if (cell == '#')
                    {
                        break;
                    }
                    row++;
                }

                var end = row;
                if (start < end)
                {
                    yield return new VerticalSpan(RowStart: start, RowEnd: end, Col: col);
                }
            }
        }

        private IEnumerable<HorizontalSpan> DetectHorizontalSpans(int row)
        {
            var col = 0;
            while (col < Cols)
            {
                while (col < Cols)
                {
                    var cell = At(row, col);
                    if (cell != '#')
                    {
                        break;
                    }
                    col++;
                }

                var start = col;
                while (col < Cols)
                {
                    var cell = At(row, col);
                    if (cell == '#')
                    {
                        break;
                    }
                    col++;
                }

                var end = col;
                if (start < end)
                {
                    yield return new HorizontalSpan(Row: row, ColStart: start, ColEnd: end);
                }
            }
        }

        public Grid TiltNorth()
        {
            var newCells = CreateCells();

            foreach (var vspan in this.spans.Vertical)
            {
                var rocks = CountRocks(vspan);

                var row = vspan.RowStart;
                for (var i = 0; i < rocks; i++)
                {
                    newCells[row][vspan.Col] = 'O';
                    row++;
                }

                while (row < vspan.RowEnd)
                {
                    newCells[row][vspan.Col] = '.';
                    row++;
                }
            }

            return new Grid(newCells, this.spans);
        }

        public Grid TiltSouth()
        {
            var newCells = CreateCells();

            foreach (var vspan in this.spans.Vertical)
            {
                var rocks = CountRocks(vspan);
                var blanks = (vspan.RowEnd - vspan.RowStart) - rocks;

                var row = vspan.RowStart;
                for (var i = 0; i < blanks; i++)
                {
                    newCells[row][vspan.Col] = '.';
                    row++;
                }

                while (row < vspan.RowEnd)
                {
                    newCells[row][vspan.Col] = 'O';
                    row++;
                }
            }

            return new Grid(newCells, this.spans);
        }

        public Grid TiltWest()
        {
            var newCells = CreateCells();

            foreach (var hspan in this.spans.Horizontal)
            {
                var rocks = CountRocks(hspan);

                var col = hspan.ColStart;
                for (var i = 0; i < rocks; i++)
                {
                    newCells[hspan.Row][col] = 'O';
                    col++;
                }

                while (col < hspan.ColEnd)
                {
                    newCells[hspan.Row][col] = '.';
                    col++;
                }
            }

            return new Grid(newCells, this.spans);
        }

        public Grid TiltEast()
        {
            var newCells = CreateCells();

            foreach (var hspan in this.spans.Horizontal)
            {
                var rocks = CountRocks(hspan);
                var blanks = (hspan.ColEnd - hspan.ColStart) - rocks;

                var col = hspan.ColStart;
                for (var i = 0; i < blanks; i++)
                {
                    newCells[hspan.Row][col] = '.';
                    col++;
                }

                while (col < hspan.ColEnd)
                {
                    newCells[hspan.Row][col] = 'O';
                    col++;
                }
            }

            return new Grid(newCells, this.spans);
        }

        private List<List<char>> CreateCells()
        {
            var rows = new List<List<char>>();

            for (var row = 0; row < Rows; row++)
            {
                var cells = new List<char>();
                for (var col = 0; col < Cols; col++)
                {
                    cells.Add('#');
                }

                rows.Add(cells);
            }

            return rows;
        }

        private int CountRocks(VerticalSpan span) =>
           Between(span.RowStart, span.RowEnd)
               .Select(row => At(row, span.Col))
               .Count(cell => cell == 'O');

        private int CountRocks(HorizontalSpan span) =>
           Between(span.ColStart, span.ColEnd)
               .Select(col => At(span.Row, col))
               .Count(cell => cell == 'O');

        public int CalculateNorthBeamLoad()
        {
            var sum = 0;
            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Cols; col++)
                {
                    if (At(row, col) == 'O')
                    {
                        sum += Rows - row;
                    }
                }
            }
            return sum;
        }

        public bool SameAs(Grid other)
        {
            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Cols; col++)
                {
                    if (At(row, col) != other.At(row, col))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void Draw()
        {
            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Cols; col++)
                {
                    Console.Write(At(row, col));
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }

    private static IEnumerable<int> Between(int start, int end)
    {
        for (var i = start; i < end; i++)
        {
            yield return i;
        }
    }
}
