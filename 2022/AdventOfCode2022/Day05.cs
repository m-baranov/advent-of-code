using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day05
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "    [D]    ",
                    "[N] [C]    ",
                    "[Z] [M] [P]",
                    " 1   2   3",
                    "",
                    "move 1 from 2 to 1",
                    "move 3 from 1 to 3",
                    "move 2 from 2 to 1",
                    "move 1 from 1 to 2"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/5/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var (stacks, moves) = Parse(input.Lines());

                foreach (var move in moves)
                {
                    for (var i = 0; i < move.Count; i++)
                    {
                        var ch = stacks[move.From - 1].Pop();
                        stacks[move.To - 1].Push(ch);
                    }
                }

                var tops = string.Join(string.Empty, stacks.Select(s => s.Pop()));
                
                Console.WriteLine(tops);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var (stacks, moves) = Parse(input.Lines());

                foreach (var move in moves)
                {
                    var buffer = new Stack<char>(capacity: move.Count);

                    for (var i = 0; i < move.Count; i++)
                    {
                        var ch = stacks[move.From - 1].Pop();
                        buffer.Push(ch);
                    }

                    for (var i = 0; i < move.Count; i++)
                    {
                        var ch = buffer.Pop();
                        stacks[move.To - 1].Push(ch);
                    }
                }

                var tops = string.Join(string.Empty, stacks.Select(s => s.Pop()));

                Console.WriteLine(tops);
            }
        }

        private static (IReadOnlyList<Stack<char>> stacks, IReadOnlyList<Move> moves) Parse(IEnumerable<string> lines)
        {
            var groups = lines.SplitByEmptyLine().ToList();

            var stacks = ParseStacks(groups[0]);
            var moves = ParseMoves(groups[1]);

            return (stacks, moves);
        }

        private static IReadOnlyList<Stack<char>> ParseStacks(IReadOnlyList<string> lines)
        {   
            // 01234567890
            // [Z] [M] [P]

            static IEnumerable<int> For(int start, int delta, int length)
            {
                for (var i = start; i < length; i += delta)
                {
                    yield return i;
                }
            }

            static IReadOnlyList<char> RowOf(string line) =>
                For(start: 1, delta: 4, length: line.Length).Select(i => line[i]).ToList();

            var rows = lines.SkipLast(1).Select(RowOf).ToList();

            static IEnumerable<char> ColumnOf(IReadOnlyList<IReadOnlyList<char>> rows, int c) =>
                Enumerable.Range(0, rows.Count).Select(r => rows[r][c]).Where(ch => ch != ' ');

            return Enumerable.Range(0, rows[0].Count)
                .Select(c => ColumnOf(rows, c))
                .Select(chs => new Stack<char>(chs.Reverse()))
                .ToList();
        }

        private static IReadOnlyList<Move> ParseMoves(IReadOnlyList<string> lines)
        {
            static Move Parse(string line)
            {
                var words = line.Split(' ');

                var count = int.Parse(words[1]);
                var from = int.Parse(words[3]);
                var to = int.Parse(words[5]);
                
                return new Move(count, from, to);
            }

            return lines.Select(Parse).ToList();
        }

        private record Move(int Count, int From, int To);
    }
}

