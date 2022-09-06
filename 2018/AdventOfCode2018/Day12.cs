using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day12
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "initial state: #..#.#..##......###...###",
                    "",
                    "...## => #",
                    "..#.. => #",
                    ".#... => #",
                    ".#.#. => #",
                    ".#.## => #",
                    ".##.. => #",
                    ".#### => #",
                    "#.#.# => #",
                    "#.### => #",
                    "##.#. => #",
                    "##.## => #",
                    "###.. => #",
                    "###.# => #",
                    "####. => #"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/12/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var (row, rules) = Util.ParseInput(input.Lines());

                for (var i = 0; i < 20; i++)
                {
                    row = row.Advance(rules);
                }

                var answer = row.PlantIndices().Sum();
                Console.WriteLine(answer);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var (row, rules) = Util.ParseInput(input.Lines());

                var i = 0;
                Row prevRow;
                while (true)
                {
                    prevRow = row;
                    row = row.Advance(rules);
                    i++;

                    if (prevRow.CellsToString() == row.CellsToString())
                    {
                        break;
                    }
                }

                // After this many generations, the pattern stops changing. It now just shifts
                // by 1 to the right with each next generation.
                var prevSum = prevRow.PlantIndices().Sum();
                Console.WriteLine(prevRow);
                Console.WriteLine("Prev gen: " + (i - 1));
                Console.WriteLine("Prev sum: " + prevSum);

                // Each next generation increases the sum by this much.
                var currentGen = i;
                var currentSum = row.PlantIndices().Sum();
                Console.WriteLine(row);
                Console.WriteLine("Curr gen: " + currentGen);
                Console.WriteLine("Curr sum: " + currentSum);

                var nextRow = row.Advance(rules);
                var nextSum = nextRow.PlantIndices().Sum();
                Console.WriteLine(nextRow);
                Console.WriteLine("Next gen: " + (currentGen + 1));
                Console.WriteLine("Next sum: " + nextSum);

                var delta = nextSum - currentSum;
                Console.WriteLine("Delta: " + delta);

                var targetGen = 50_000_000_000L;
                var finalSum = (targetGen - (i - 1)) * delta + prevSum;
                Console.WriteLine("Final sum: " + finalSum);
            }
        }

        private enum Cell { Empty, Plant }

        private static class Util
        {
            public static Cell ParseCell(char ch) =>
                ch == '#' ? Cell.Plant : Cell.Empty;

            public static IReadOnlyList<Cell> ParseCells(string line) =>
                line.Select(ParseCell).ToList();

            public static string ToString(Cell cell) =>
                cell == Cell.Plant ? "#" : ".";

            public static string ToString(IEnumerable<Cell> cells) =>
                string.Join("", cells.Select(ToString));

            public static (Row, RuleSet) ParseInput(IEnumerable<string> lines)
            {
                Row row = null;
                var rules = new List<Rule>();

                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    if (row == null)
                    {
                        row = Row.Parse(line);
                    }
                    else
                    {
                        rules.Add(Rule.Parse(line));
                    }
                }

                return (row, new RuleSet(rules));
            }
        }

        private class Rule
        {
            public static Rule Parse(string text)
            {
                var parts = text.Split(" => ");

                var pattern = Util.ParseCells(parts[0]);
                var result = Util.ParseCell(parts[1][0]);

                return new Rule(pattern, result);
            }

            private readonly IReadOnlyList<Cell> pattern;
            private readonly Cell result;

            public Rule(IReadOnlyList<Cell> pattern, Cell result)
            {
                this.pattern = pattern;
                this.result = result;
            }

            public Cell Result => this.result;

            public bool Matches(RowSlice slice)
            {
                for (var i = 0; i < slice.Length; i++)
                {
                    if (this.pattern[i] != slice[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private class RuleSet
        {
            private readonly IReadOnlyList<Rule> rules;

            public RuleSet(IReadOnlyList<Rule> rules)
            {
                this.rules = rules;
            }

            public Cell Match(RowSlice slice)
            {
                var rule = rules.FirstOrDefault(r => r.Matches(slice));
                return rule?.Result ?? Cell.Empty;
            }
        }

        private class RowSlice
        {
            private readonly Row row;
            private readonly int start;
            private readonly int length;

            public RowSlice(Row row, int start, int length)
            {
                this.row = row;
                this.start = start;
                this.length = length;
            }

            public int Length => this.length;

            public Cell this[int index] => this.row.At(this.start + index);

            public override string ToString() => 
                Util.ToString(Enumerable.Range(0, this.length).Select(i => this[i]));
        }

        private class Row
        {
            public static Row Parse(string text)
            {
                const string PREFIX = "initial state: ";

                if (text.StartsWith(PREFIX))
                {
                    text = text.Substring(PREFIX.Length);
                }

                var cells = Util.ParseCells(text);
                return new Row(cells).Compact();
            }

            private readonly IReadOnlyList<Cell> cells;
            private readonly int start;

            public Row(IReadOnlyList<Cell> cells) : this(cells, start: 0) {}

            private Row(IReadOnlyList<Cell> cells, int start)
            {
                this.cells = cells;
                this.start = start;
            }

            public IEnumerable<int> PlantIndices()
            {
                for (var actual = 0; actual < this.cells.Count; actual++)
                {
                    if (this.cells[actual] == Cell.Plant)
                    {
                        var index = actual + this.start;
                        yield return index;
                    }
                }
            }

            public Cell At(int index)
            {
                //  0  1  2  3  4 <- actual
                //
                // -2 -1  0  1  2 <- index
                //  .  .  .  #  .
                //  ^
                //  start

                var actual = index - this.start;
                return (0 <= actual && actual < this.cells.Count) ? this.cells[actual] : Cell.Empty;
            }

            public Row Advance(RuleSet rules)
            {
                const int AROUND = 2;

                var newCells = new List<Cell>();
                var newStart = this.start - AROUND;
                var newEnd = newStart + this.cells.Count + AROUND * 2 - 1;

                for (var i = newStart; i <= newEnd; i++)
                {
                    var slice = new RowSlice(this, start: i - AROUND, length: AROUND * 2 + 1);
                    var newCell = rules.Match(slice);
                    newCells.Add(newCell);
                }

                return new Row(newCells, newStart).Compact();
            }

            private Row Compact()
            {
                static int LeadingEmpty(IReadOnlyList<Cell> cells)
                {
                    var i = 0;
                    while (i < cells.Count && cells[i] == Cell.Empty)
                    {
                        i++;
                    }
                    return i;
                }

                static int TrailingEmpty(IReadOnlyList<Cell> cells)
                {
                    var i = cells.Count - 1;
                    while (i >= 0 && cells[i] == Cell.Empty)
                    {
                        i--;
                    }

                    return cells.Count - 1 - i;
                }

                var leading = LeadingEmpty(this.cells);
                if (leading == this.cells.Count)
                {
                    return new Row(Array.Empty<Cell>(), start: 0);
                }

                var trailing = TrailingEmpty(this.cells);

                var compacted = this.cells.Skip(leading).Take(this.cells.Count - leading - trailing).ToList();
                return new Row(compacted, this.start + leading);
            }

            public override string ToString() => $"start={this.start}: {CellsToString()}";

            public string CellsToString() => Util.ToString(this.cells);

            public bool SameAs(Row other)
            {
                if (this.start != other.start)
                {
                    return false;
                }

                if (this.cells.Count != other.cells.Count)
                {
                    return false;
                }

                for (var i = 0; i < this.cells.Count; i++) 
                {
                    if (this.cells[i] != other.cells[i])
                    {
                        return false;
                    }    
                }

                return true;
            }
        }
    }
}
