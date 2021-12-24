using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day24
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal();

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/24/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                for (var n = 9_999_999; n >= 1_111_111; n--)
                {
                    var number = Generator.TryGenerate(n);
                    if (number != null)
                    {
                        Console.WriteLine(number);
                        break;
                    }
                }
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                for (var n = 1_111_111; n <= 9_999_999; n++)
                {
                    var number = Generator.TryGenerate(n);
                    if (number != null)
                    {
                        Console.WriteLine(number);
                        break;
                    }
                }
            }
        }

        private static class Analysis
        {
            public static void Run(TextReader input)
            {
                var program = input.Lines().ToList();

                // Let's look at the program from the puzzle input.
                // It must have 14 "inp w" instructions.
                var programParts = program.SplitBy(ln => ln == "inp w").ToList();

                // Is processing of each digit different?
                PrintCommonCharacters(programParts);

                // Ouput:
                //  0: mul x 0
                //  1: add x z
                //  2: mod x 26
                //  3: div z ??    -- A
                //  4: add x ???   -- B
                //  5: eql x w
                //  6: eql x 0
                //  7: mul y 0
                //  8: add y 25
                //  9: mul y x
                // 10: add y 1
                // 11: mul z y
                // 12: mul y 0
                // 13: add y w
                // 14: add y ??    -- C
                // 15: mul y x
                // 16: add z y

                // So, the processing of each digit is mostly the same.

                // What's the difference?
                var argAs = SecondArgumentsAtLine(programParts, line: 3);
                var argBs = SecondArgumentsAtLine(programParts, line: 4);
                var argCs = SecondArgumentsAtLine(programParts, line: 14);

                PrintItems("A", argAs);
                PrintItems("B", argBs);
                PrintItems("C", argCs);

                // Output: 
                // A:    1   1   1  26  26   1  26  26   1   1  26   1  26  26
                // B:   12  13  13  -2 -10  13 -14  -5  15  15 -14  10 -14  -5
                // C:    7   8  10   4   4   6  11  13   1   8   4  13   4  14

                // All numbers.

                // What does logic do?

                //     inp w
                //  0: mul x 0     x = 0
                //  1: add x z     x = z
                //  2: mod x 26    x = z % 26
                //  
                //  3: div z Ai    z = z / Ai
                //
                //  4: add x Bi    x = z % 26 + Bi
                //  5: eql x w     x = x == w |
                //  6: eql x 0     x = x == 0 | computes to x = x != w
                //                              so x = (z % 26 + Bi) != w
                // 
                //  7: mul y 0     y = 0
                //  8: add y 25    y = 25
                //  9: mul y x     y = 25 * x
                // 10: add y 1     y = 25 * x + 1
                //
                // 11: mul z y     z = (z / Ai) * (25 * x + 1)
                //
                // 12: mul y 0     y = 0
                // 13: add y w     y = w
                // 14: add y Ci    y = w + Ci
                // 15: mul y x     y = (w + Ci) * x
                // 
                // 16: add z y     z = (z / Ai) * (25 * x + 1) + (w + Ci) * x 

                // This boils down to:
                // 
                // if (z % 26 + Bi == w) // x = 0
                //    z = z / Ai
                // else                  // x = 1
                //    z = z / Ai * 26 + w + Ci

                // w is a digit between 1 and 9.
                // So if B is >= 10 the condition will never be true.

                //       1   2   3   4   5   6   7   8   9  10  11  12  13  14
                //   -----------------------------------------------------------
                // A:    1   1   1  26  26   1  26  26   1   1  26   1  26  26
                // B:   12  13  13  -2 -10  13 -14  -5  15  15 -14  10 -14  -5
                // C:    7   8  10   4   4   6  11  13   1   8   4  13   4  14

                // B is either >= 10, or negative.
                // A is 1 when B is >= 10, A is 26 when B is negative.

                // if (z % 26 + Bi == w)
                //    z = z / 26         // Ai == 1, Bi < 0
                // else
                //    z = z * 26 + w + Ci

                // z should be 0 by the end.
                // It'll be zero only if we visit then-branch at least as many time as the else-branch.
                // We'll visit the else-branch every time Ai == 1, i.e. 7 times.
                // So we have to hit the then-branch every time Ai != 1.

                // Let's trace execution:
                //    |      |                         z = 0
                //  1 | else |                         z = w1 + c1
                //  2 | else |                         z = (w1 + c1) * 26 + w2 + c2
                //  3 | else |                         z = (w1 + c1) * 26^2 + (w2 + c2) * 26 + (w3 + c3)
                //  4 | then | w3 + c3 + b4 == w4,     z = (w1 + c1) * 26 + w2 + c2
                //  5 | then | w2 + c2 + b5 == w5,     z = w1 + c1
                //  6 | else |                         z = (w1 + c1) * 26 + w6 + c6
                //  7 | then | w6 + c6 + b7 == w7,     z = w1 + c1
                //  8 | then | w1 + c1 + b8 == w8,     z = 0
                //  9 | else |                         z = w9 + c9
                // 10 | else |                         z = (w9 + c9) * 26 + w10 + c10
                // 11 | then | w10 + c10 + b11 == w11, z = w9 + c9
                // 12 | else |                         z = (w9 + c9) * 26 + w12 + c12
                // 13 | then | w12 + c12 + b13 == w13, z = w9 + c9
                // 14 | else | w9 + c9 + b14 == w14,   z = 0 

                // So here are the conditions that must hold for valid number:

                var Bs = argBs.Select(int.Parse).ToList();
                var Cs = argCs.Select(int.Parse).ToList();

                Console.WriteLine("w[3 ] + " + (Cs[3 -1] + Bs[4 -1]) + " == w[4 ]");
                Console.WriteLine("w[2 ] + " + (Cs[2 -1] + Bs[5 -1]) + " == w[5 ]");
                Console.WriteLine("w[6 ] + " + (Cs[6 -1] + Bs[7 -1]) + " == w[7 ]");
                Console.WriteLine("w[1 ] + " + (Cs[1 -1] + Bs[8 -1]) + " == w[8 ]");
                Console.WriteLine("w[10] + " + (Cs[10-1] + Bs[11-1]) + " == w[11]");
                Console.WriteLine("w[12] + " + (Cs[12-1] + Bs[13-1]) + " == w[13]");
                Console.WriteLine("w[9 ] + " + (Cs[9 -1] + Bs[14-1]) + " == w[14]");

                // Output: 
                // w[3]  +  8 == w[4]
                // w[2]  + -2 == w[5]
                // w[6]  + -8 == w[7]
                // w[1]  +  2 == w[8]
                // w[10] + -6 == w[11]
                // w[12] + -1 == w[13]
                // w[9]  + -4 == w[14]

                // All w have to be between 1 and 9.

                // To generate a valid number:
                // 1. generate 7 digits on the left
                // 2. compute the remaing digits
                // 3. check if all between 1 and 9 
                // 
                // See Generator.TryGenerate.
            }

            private static void PrintCommonCharacters(
                IReadOnlyList<IReadOnlyList<string>> programParts)
            {
                var maxLineCount = programParts.Max(p => p.Count);

                for (var line = 0; line < maxLineCount; line++)
                {
                    var lines = programParts
                        .Select(part => line < part.Count ? part[line] : string.Empty)
                        .ToList();

                    var maxLength = lines.Max(l => l.Length);
                    var commonLine = string.Join("", Enumerable.Range(0, maxLength)
                        .Select(i =>
                        {
                            var chars = lines.Select(l => i < l.Length ? l[i] : ' ').ToList();
                            return chars.All(ch => ch == chars[0]) ? chars[0] : '?';
                        }));

                    Console.WriteLine($"{line,3}: {commonLine}");
                }
            }

            private static IReadOnlyList<string> SecondArgumentsAtLine(
                List<IReadOnlyList<string>> programParts, 
                int line)
            {
                return programParts
                    .Select(part => part[line])
                    .Select(line => line.Split(' ')[2])
                    .ToList();
            }

            private static void PrintItems(string name, IReadOnlyList<string> items)
            {
                Console.Write($"{name}: ");
                foreach (var item in items)
                {
                    Console.Write($"{item,4}");
                }
                Console.WriteLine();
            }

        }

        private static class Generator
        {
            public static string TryGenerate(int n)
            {
                var w = new int[14];

                var d = Digits(n);
                
                w[0]  = d[0];
                w[2]  = d[1];
                w[4]  = d[2];
                w[6]  = d[3];
                w[10] = d[4];
                w[12] = d[5];
                w[13] = d[6];

                w[7]  = w[0]  + 2;
                w[3]  = w[2]  + 8;
                w[1]  = w[4]  + 2;
                w[5]  = w[6]  + 8;
                w[9]  = w[10] + 6;
                w[11] = w[12] + 1;
                w[8]  = w[13] + 4;

                if (!w.All(d => 1 <= d && d <= 9))
                {
                    return null;
                }

                return string.Join("", w);
            }

            private static IReadOnlyList<int> Digits(int n)
            {
                var digits = new List<int>();

                while (n != 0)
                {
                    n = Math.DivRem(n, 10, out var d);
                    digits.Add(d);
                }

                digits.Reverse();
                return digits;
            }
        }

        // Funny how this all is not needed :)

        //private class Memory
        //{
        //    public static readonly IReadOnlyList<char> VariableNames = new[] { 'w', 'x', 'y', 'z' };

        //    private readonly Dictionary<char, int> values;

        //    public Memory()
        //    {
        //        values = VariableNames.ToDictionary(name => name, name => 0);
        //    }

        //    public int Get(char name) => values[name];

        //    public void Set(char name, int value) => values[name] = value;
        //}

        //private abstract class Argument
        //{
        //    public static Argument Parse(string text)
        //    {
        //        return Memory.VariableNames.Contains(text[0]) 
        //            ? new Variable(text[0]) 
        //            : new Number(int.Parse(text));
        //    }

        //    public class Number : Argument
        //    {
        //        public Number(int value)
        //        {
        //            Value = value;
        //        }

        //        public int Value { get; }
        //    }

        //    public class Variable : Argument
        //    {
        //        public Variable(char name)
        //        {
        //            Name = name;
        //        }

        //        public char Name { get; }
        //    }
        //}

        //private enum Command { Inp, Add, Mul, Div, Mod, Eql };

        //private class Instruction
        //{
        //    public static IReadOnlyList<Instruction> ParseMany(IEnumerable<string> lines) =>
        //        lines.Select(Parse).ToList();

        //    public static Instruction Parse(string text)
        //    {
        //        var parts = text.Split(' ');

        //        var command = ParseCommand(parts[0]);
        //        var arguments = parts.Skip(1).Select(Argument.Parse).ToList();

        //        return new Instruction(command, arguments);
        //    }

        //    private static Command ParseCommand(string text) =>
        //        text switch
        //        {
        //            "inp" => Command.Inp,
        //            "add" => Command.Add,
        //            "mul" => Command.Mul,
        //            "div" => Command.Div,
        //            "mod" => Command.Mod,
        //            "eql" => Command.Eql,
        //            _ => throw new Exception($"Unknown command '{text}'.")
        //        };

        //    public Instruction(Command command, IReadOnlyList<Argument> arguments)
        //    {
        //        Command = command;
        //        Arguments = arguments;
        //    }

        //    public Command Command { get; }
        //    public IReadOnlyList<Argument> Arguments { get; }
        //}

        //private class InputSource 
        //{
        //    private readonly IReadOnlyList<int> values;
        //    private int index;

        //    public InputSource(IReadOnlyList<int> values)
        //    {
        //        this.values = values;
        //        this.index = 0;
        //    }

        //    public int Read()
        //    {
        //        var value = values[index];
        //        index++;
        //        return value;
        //    }
        //}

        //private class Computer
        //{
        //    public Computer(IReadOnlyList<int> input)
        //    {
        //        Memory = new Memory();
        //        Input = new InputSource(input);
        //    }

        //    public Memory Memory { get; }
        //    public InputSource Input { get; }

        //    public void Run(IReadOnlyList<Instruction> instructions)
        //    {
        //        foreach (var instruction in instructions)
        //        {
        //            Run(instruction);
        //        }
        //    }

        //    private void Run(Instruction instruction)
        //    {
        //        switch (instruction.Command)
        //        {
        //            case Command.Inp:
        //                RunInp(instruction);
        //                break;

        //            case Command.Add:
        //                RunAdd(instruction);
        //                break;

        //            case Command.Mul:
        //                RunMul(instruction);
        //                break;

        //            case Command.Div:
        //                RunDiv(instruction);
        //                break;

        //            case Command.Mod:
        //                RunMod(instruction);
        //                break;

        //            case Command.Eql:
        //                RunEql(instruction);
        //                break;
        //        }
        //    }

        //    private void RunInp(Instruction instruction)
        //    {
        //        var variable = VariableNameOf(instruction.Arguments[0]);
        //        Memory.Set(variable, Input.Read());
        //    }

        //    private void RunAdd(Instruction instruction) => 
        //        RunBinary(instruction, (a, b) => a + b);

        //    private void RunMul(Instruction instruction) =>
        //        RunBinary(instruction, (a, b) => a * b);

        //    private void RunDiv(Instruction instruction) => 
        //        RunBinary(instruction, (a, b) => a / b);

        //    private void RunMod(Instruction instruction) =>
        //        RunBinary(instruction, (a, b) => a % b);

        //    private void RunEql(Instruction instruction) =>
        //        RunBinary(instruction, (a, b) => a == b ? 1 : 0);

        //    private void RunBinary(Instruction instruction, Func<int, int, int> operation)
        //    {
        //        var variable = VariableNameOf(instruction.Arguments[0]);

        //        var valueA = Memory.Get(variable);
        //        var valueB = ValueOf(instruction.Arguments[1]);
        //        var result = operation(valueA, valueB);

        //        Memory.Set(variable, result);
        //    }

        //    private char VariableNameOf(Argument argument)
        //    {
        //        var variable = (Argument.Variable)argument;
        //        return variable.Name;
        //    }

        //    private int ValueOf(Argument argument)
        //    {
        //        if (argument is Argument.Number number)
        //        {
        //            return number.Value;
        //        }

        //        var variable = VariableNameOf(argument);
        //        return Memory.Get(variable);
        //    }
        //}
    }
}
