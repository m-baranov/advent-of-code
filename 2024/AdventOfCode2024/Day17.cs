using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2024;

static class Day17
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
Register A: 729
Register B: 0
Register C: 0

Program: 0,1,5,4,3,0
"""""");

        public static readonly IInput Sample2 =
            Input.Literal(""""""
Register A: 2024
Register B: 0
Register C: 0

Program: 0,3,5,4,3,0
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/17/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var (registers, instructions) = State.Parse(input.Lines().ToArray());

            var output = Cpu.Eval(registers, instructions);

            Console.WriteLine(string.Join(',', output));
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var (_, instructions) = State.Parse(input.Lines().ToArray());

            Console.WriteLine(string.Join(',', instructions));

            var possibleAs = Reverse(instructions);

            var minA = possibleAs
                .OrderBy(a => a)
                .Where(a => IsSelfReplicates(instructions, a))
                .First();

            Console.WriteLine(minA);
        }

        // My particular program translates roughly to the code below.
        //
        // Given that, one can work backwards from the last to the first
        // instruction and try to guess what A must be so that when B is
        // computed, b % 8 equals a given instruction.

        // private static void Program(long a)
        // {
        //     while (a > 0)
        //     {
        //         var (div, rem) = Math.DivRem(a, 8);

        //         var b = rem ^ 3 ^ 5 ^ (a / Pow2(rem ^ 3));
        //         var next = b % 8;

        //         Console.Write(next + ",");

        //         a = div;
        //     }
        //     Console.WriteLine();
        // }

        private static IReadOnlyList<long> Reverse(IReadOnlyList<int> expected)
        {
            static long Pow2(long x) => (long)Math.Pow(2, x);

            static int Eval(long a)
            {
                var rem = a % 8;
                var b = rem ^ 3 ^ 5 ^ (a / Pow2(rem ^ 3));
                return (int)(b % 8);
            }

            var results = new List<long>();

            void Recurse(long a, int index)
            {
                if (index < 0)
                {
                    results.Add(a);
                    return;
                }

                var mod = expected[index];

                var nextRem = 0;
                while (nextRem < 8)
                {
                    var nextA = a * 8 + nextRem;
                    if (Eval(nextA) == mod)
                    {
                        Recurse(nextA, index - 1);
                    }

                    nextRem++;
                }
            }

            Recurse(0, expected.Count - 1);

            return results;
        }

        private static bool IsSelfReplicates(IReadOnlyList<int> instructions, long a)
        {
            var output = Cpu.Eval(new Registers(a, 0, 0, 0), instructions);
            return output.Select(o => (int)o).SequenceEqual(instructions);
        }
    }

    private static class Cpu
    {
        public static IReadOnlyList<long> Eval(Registers registers, IReadOnlyList<int> instructions)
        {
            var output = new List<long>();
            while (true)
            {
                if (registers.Ip < 0 || registers.Ip >= instructions.Count)
                {
                    break;
                }

                var opcode = instructions[registers.Ip];
                var operand = instructions[registers.Ip + 1];

                var (nextRegisters, additionalOutput) = Eval(registers, opcode, operand);
                registers = nextRegisters;
                output.AddRange(additionalOutput);
            }

            return output;
        }

        private static (Registers next, IReadOnlyList<long> output) Eval(Registers registers, int opcode, int operand)
        {
            if (opcode == 0 /* adv */)
            {
                return EvalAdv(registers, operand);
            }
            if (opcode == 1 /* bxl */)
            {
                return EvalBxl(registers, operand);
            }
            if (opcode == 2 /* bst */)
            {
                return EvalBst(registers, operand);
            }
            if (opcode == 3 /* jnz */)
            {
                return EvalJnz(registers, operand);
            }
            if (opcode == 4 /* bxc */)
            {
                return EvalBxc(registers, operand);
            }
            if (opcode == 5 /* out */)
            {
                return EvalOut(registers, operand);
            }
            if (opcode == 6 /* bdv */)
            {
                return EvalBdv(registers, operand);
            }
            if (opcode == 7 /* cdv */)
            {
                return EvalCdv(registers, operand);
            }

            throw new Exception("impossible");
        }

        private static (Registers next, IReadOnlyList<long> output) EvalAdv(Registers registers, int operand)
        {
            var result = EvalDiv(registers, operand);

            return (registers with { A = result, Ip = registers.Ip + 2 }, Array.Empty<long>());
        }

        private static (Registers next, IReadOnlyList<long> output) EvalBdv(Registers registers, int operand)
        {
            var result = EvalDiv(registers, operand);

            return (registers with { B = result, Ip = registers.Ip + 2 }, Array.Empty<long>());
        }

        private static (Registers next, IReadOnlyList<long> output) EvalCdv(Registers registers, int operand)
        {
            var result = EvalDiv(registers, operand);

            return (registers with { C = result, Ip = registers.Ip + 2 }, Array.Empty<long>());
        }

        private static long EvalDiv(Registers registers, int operand)
        {
            var x = (double)registers.A;
            var y = Math.Pow(2, EvalComboOperand(registers, operand));
            var result = (long)Math.Truncate(x / y);
            return result;
        }

        private static (Registers next, IReadOnlyList<long> output) EvalBxl(Registers registers, int operand)
        {
            var x = registers.B;
            var y = (long)operand;
            var result = x ^ y;

            return (registers with { B = result, Ip = registers.Ip + 2 }, Array.Empty<long>());
        }

        private static (Registers next, IReadOnlyList<long> output) EvalBst(Registers registers, int operand)
        {
            var x = EvalComboOperand(registers, operand);
            var result = x % 8;

             return (registers with { B = result, Ip = registers.Ip + 2 }, Array.Empty<long>());
        }

        private static (Registers next, IReadOnlyList<long> output) EvalJnz(Registers registers, int operand)
        {
            if (registers.A == 0)
            {
                return (registers with { Ip = registers.Ip + 2 }, Array.Empty<long>());
            }

            return (registers with { Ip = operand }, Array.Empty<long>());
        }

        private static (Registers next, IReadOnlyList<long> output) EvalBxc(Registers registers, int operand)
        {
            var x = registers.B;
            var y = registers.C;
            var result = x ^ y;

            return (registers with { B = result, Ip = registers.Ip + 2 }, Array.Empty<long>());
        }

        private static (Registers next, IReadOnlyList<long> output) EvalOut(Registers registers, int operand)
        {
            var x = EvalComboOperand(registers, operand);
            var result = x % 8;

            return (registers with { B = result, Ip = registers.Ip + 2 }, new[] { result });
        }

        private static long EvalComboOperand(Registers registers, int operand)
        {
            if (0 <= operand && operand <= 3)
            {
                return operand;
            }

            if (operand == 4)
            {
                return registers.A;
            }
            if (operand == 5)
            {
                return registers.B;
            }
            if (operand == 6)
            {
                return registers.C;
            }

            throw new Exception("impossible");
        }
    }

    private record Registers(long A, long B, long C, int Ip);

    private record State(Registers Registers, IReadOnlyList<int> Instructions)
    {
        public static State Parse(IReadOnlyList<string> lines)
        {
            var a = TrimStart(lines[0], "Register A: ");
            var b = TrimStart(lines[1], "Register B: ");
            var c = TrimStart(lines[2], "Register C: ");

            var program = TrimStart(lines[4], "Program: ");

            var registers = new Registers(
                long.Parse(a),
                long.Parse(b),
                long.Parse(c),
                Ip: 0
            );

            var instructions = program
                .Split(',')
                .Select(int.Parse)
                .ToList();

            return new State(registers, instructions);
        }
    }

    private static string TrimStart(string text, string prefix) =>
        text.Substring(prefix.Length);
}
