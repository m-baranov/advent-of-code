using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdventOfCode2024;

static class Day19
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
r, wr, b, g, bwu, rb, gb, br

brwrr
bggr
gbbr
rrbgbr
ubwu
bwurrg
brgr
bbrgwb
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/19/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var (patterns, designs) = State.Parse(input.Lines());

            var remainingPatterns = RemoveRedundant(patterns);

            var count = 0;
            foreach (var design in designs)
            {
                if (IsPossible(design, remainingPatterns))
                {
                    count++;
                }
            }

            Console.WriteLine(count);
        }

        private static IReadOnlyList<string> RemoveRedundant(IReadOnlyList<string> patterns)
        {
            var remainingPatterns = patterns
                .Where(p => p.Length == 1)
                .ToList();

            foreach (var pattern in patterns.OrderBy(p => p.Length))
            {
                if (IsPossible(pattern, remainingPatterns))
                {
                    continue;
                }
                remainingPatterns.Add(pattern);
            }

            return remainingPatterns;
        }

        private static bool IsPossible(string design, IReadOnlyList<string> patterns)
        {
            bool Recurse(string design, int index)
            {
                if (index >= design.Length)
                {
                    return true;
                }

                foreach (var pattern in patterns)
                {
                    if (!ContainsAt(design, index, pattern))
                    {
                        continue;
                    }

                    if (Recurse(design, index + pattern.Length))
                    {
                        return true;
                    }
                }

                return false;
            }

            return Recurse(design, 0);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var (patterns, designs) = State.Parse(input.Lines());

            var sum = 0L;
            foreach (var design in designs)
            {
                sum += CountArrangements(design, patterns);
            }

            Console.WriteLine(sum);
        }

        private static long CountArrangements(string design, IReadOnlyList<string> patterns)
        {
            var memo = new Dictionary<int, long>();

            long RecurseMemoed(int index)
            {
                if (memo.TryGetValue(index, out var stored))
                {
                    return stored;
                }

                var count = Recurse(index);
                memo[index] = count;
                return count;
            }


            long Recurse(int index)
            {
                if (index >= design.Length)
                {
                    return 1;
                }

                var sum = 0L;
                foreach (var pattern in patterns)
                {
                    if (!ContainsAt(design, index, pattern))
                    {
                        continue;
                    }

                    sum += RecurseMemoed(index + pattern.Length);
                }
                return sum;
            }

            return RecurseMemoed(0);
        }
    }

    private static bool ContainsAt(string text, int index, string sub)
    {
        if (sub.Length > text.Length - index)
        {
            return false;
        }

        return text.AsSpan().Slice(index).StartsWith(sub);
    }

    private record State(IReadOnlyList<string> Patterns, IReadOnlyList<string> Designs)
    {
        public static State Parse(IEnumerable<string> lines)
        {
            var groups = lines.SplitByEmptyLine().ToArray();

            var patterns = groups[0][0].Split(", ");
            var designs = groups[1];

            return new State(patterns, designs);
        }
    }
}
