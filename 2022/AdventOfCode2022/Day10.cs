using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day10
    {
        public static class Inputs
        {
            public static readonly IInput Sample0 =
                Input.Literal(
                    "noop",
                    "addx 3",
                    "addx -5"
                );

            public static readonly IInput Sample =
                Input.Literal(
                    "addx 15",
                    "addx -11",
                    "addx 6",
                    "addx -3",
                    "addx 5",
                    "addx -1",
                    "addx -8",
                    "addx 13",
                    "addx 4",
                    "noop",
                    "addx -1",
                    "addx 5",
                    "addx -1",
                    "addx 5",
                    "addx -1",
                    "addx 5",
                    "addx -1",
                    "addx 5",
                    "addx -1",
                    "addx -35",
                    "addx 1",
                    "addx 24",
                    "addx -19",
                    "addx 1",
                    "addx 16",
                    "addx -11",
                    "noop",
                    "noop",
                    "addx 21",
                    "addx -15",
                    "noop",
                    "noop",
                    "addx -3",
                    "addx 9",
                    "addx 1",
                    "addx -3",
                    "addx 8",
                    "addx 1",
                    "addx 5",
                    "noop",
                    "noop",
                    "noop",
                    "noop",
                    "noop",
                    "addx -36",
                    "noop",
                    "addx 1",
                    "addx 7",
                    "noop",
                    "noop",
                    "noop",
                    "addx 2",
                    "addx 6",
                    "noop",
                    "noop",
                    "noop",
                    "noop",
                    "noop",
                    "addx 1",
                    "noop",
                    "noop",
                    "addx 7",
                    "addx 1",
                    "noop",
                    "addx -13",
                    "addx 13",
                    "addx 7",
                    "noop",
                    "addx 1",
                    "addx -33",
                    "noop",
                    "noop",
                    "noop",
                    "addx 2",
                    "noop",
                    "noop",
                    "noop",
                    "addx 8",
                    "noop",
                    "addx -1",
                    "addx 2",
                    "addx 1",
                    "noop",
                    "addx 17",
                    "addx -9",
                    "addx 1",
                    "addx 1",
                    "addx -3",
                    "addx 11",
                    "noop",
                    "noop",
                    "addx 1",
                    "noop",
                    "addx 1",
                    "noop",
                    "noop",
                    "addx -13",
                    "addx -19",
                    "addx 1",
                    "addx 3",
                    "addx 26",
                    "addx -30",
                    "addx 12",
                    "addx -1",
                    "addx 3",
                    "addx 1",
                    "noop",
                    "noop",
                    "noop",
                    "addx -9",
                    "addx 18",
                    "addx 1",
                    "addx 2",
                    "noop",
                    "noop",
                    "addx 9",
                    "noop",
                    "noop",
                    "noop",
                    "addx -1",
                    "addx 2",
                    "addx -37",
                    "addx 1",
                    "addx 3",
                    "noop",
                    "addx 15",
                    "addx -21",
                    "addx 22",
                    "addx -6",
                    "addx 1",
                    "noop",
                    "addx 2",
                    "addx 1",
                    "noop",
                    "addx -10",
                    "noop",
                    "noop",
                    "addx 20",
                    "addx 1",
                    "addx 2",
                    "addx 2",
                    "addx -6",
                    "addx -11",
                    "noop",
                    "noop",
                    "noop"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/10/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var instructions = Instruction.Parse(input.Lines());

                var sum = Instruction.Evaluate(instructions)
                    .Where(p => p.time % 40 == 20)
                    .Take(6)
                    .Select(p => (long)p.time * p.regX)
                    .Sum();

                Console.WriteLine(sum);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var instructions = Instruction.Parse(input.Lines());

                var pixels = Instruction.Evaluate(instructions)
                    .Zip(ScanLine(), (ip, sp) =>
                    {
                        var (_, regX) = ip;
                        var (_, x, eol) = sp;
                        return (regX, x, eol);
                    })
                    .Select(p => (lit: p.regX - 1 == p.x || p.regX == p.x || p.regX + 1 == p.x, p.eol));

                foreach (var (lit, eol) in pixels)
                {
                    Console.Write(lit ? '#' : '.');

                    if (eol)
                    {
                        Console.WriteLine();
                    }
                }
            }

            private static IEnumerable<(int time, int x, bool eol)> ScanLine()
            {
                const int H = 6;
                const int W = 40;

                var time = 1;
                for (var y = 0; y < H; y++)
                {
                    for (var x = 0; x < W; x++)
                    {
                        yield return (time, x, eol: x == W - 1);
                        time++;
                    }
                }
            }
        }

        private abstract class Instruction
        {
            public static IEnumerable<Instruction> Parse(IEnumerable<string> lines) =>
                lines.Select(Parse);

            public static Instruction Parse(string line)
            {
                if (line.StartsWith("noop"))
                {
                    return Instruction.Noop.Instance;
                }

                var parts = line.Split(' ');
                Debug.Assert(parts[0] == "addx");

                var value = int.Parse(parts[1]);
                return new Instruction.AddX(value);
            }

            public static IEnumerable<(int time, int regX)> Evaluate(IEnumerable<Instruction> instructions)
            {
                var regX = 1;
                var time = 1;

                foreach (var instruction in instructions)
                {
                    if (instruction is Instruction.Noop)
                    {
                        yield return (time, regX);
                        time++;
                    }
                    else if (instruction is Instruction.AddX addX)
                    {
                        yield return (time, regX);
                        time++;

                        yield return (time, regX);
                        time++;
                        regX += addX.Value;
                    }
                }

                yield return (time, regX);
            }

            public class AddX : Instruction
            {
                public AddX(int value)
                {
                    Value = value;
                }

                public int Value { get; }
            }

            public class Noop : Instruction
            {
                public static readonly Noop Instance = new();

                private Noop() { }
            }
        }
    }
}
