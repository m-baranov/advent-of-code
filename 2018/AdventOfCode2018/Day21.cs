using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day21
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal();

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/21/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var (declaration, program) = Parser.ParseProgram(input.Lines().ToList());
                Decompiler.Print(declaration, program);

                // Output (jump locations):
                // 
                //  0: R3 = 123
                // 
                //  1: R3 = R3 & 456
                //  2: R3 = bool(R3 = 72)   # if R3=72, jmp 5, else jmp 1
                //  3: ip = R3 + 3          # 
                //  4: ip = 0               #
                //
                //  5: R3 = 0
                // 
                //  6: R1 = R3 | 65536
                //  7: R3 = 10373714
                // 
                //  8: R5 = R1 & 255
                //  9: R3 = R3 + R5
                // 10: R3 = R3 & 16777215
                // 11: R3 = R3 * 65899
                // 12: R3 = R3 & 16777215
                // 13: R5 = bool(256 > R1)  # if 256>R1, jmp 28, else jmp 17
                // 14: ip = R5 + 14         #
                // 15: ip = 16              #
                // 16: ip = 27              #
                //
                // 17: R5 = 0
                //
                // 18: R4 = R5 + 1
                // 19: R4 = R4 * 256
                // 20: R4 = bool(R4 > R1)   # if R4>R1, jmp 26, else jmp 24
                // 21: ip = R4 + 21         #
                // 22: ip = 23              #
                // 23: ip = 25              #
                //
                // 24: R5 = R5 + 1
                // 25: ip = 17              # jmp 18
                //
                // 26: R1 = R5
                // 27: ip = 7               # jmp 8

                // 28: R5 = bool(R3 = R0)   # if R3=R0, halt, else jmp 6
                // 29: ip = R5 + 29
                // 30: ip = 5

                // ------------------------------------------------------------

                // Output (annotated):

                //  0: R3 = 123             # R3 = 123
                //                          # while (true) {
                //  1: R3 = R3 & 456        #    R3 = R3 & 456
                //  2: R3 = bool(R3 = 72)   #    if (R3=72) { break; }
                //  3: ip = R3 + 3          # }
                //  4: ip = 0               #
                // 
                //  5: R3 = 0               # R3 = 0
                //                          # while (true) {
                //  6: R1 = R3 | 65536      #  R1 = R3 | 65536;
                //  7: R3 = 10373714        #  R3 = 10373714;
                //                          #  while (true) {
                //  8: R5 = R1 & 255        #    
                //  9: R3 = R3 + R5         #    R3 = (((R3 + (R1 & 255)) & 16777215) * 65899) & 16777215;
                // 10: R3 = R3 & 16777215
                // 11: R3 = R3 * 65899
                // 12: R3 = R3 & 16777215
                // 13: R5 = bool(256 > R1)  #    if (256>R1) { break; }
                // 14: ip = R5 + 14         #
                // 15: ip = 16              #
                // 16: ip = 27              #
                // 
                // 17: R5 = 0               #    R5 = 0;
                //                          #    while (true) {
                // 18: R4 = R5 + 1          #      R4 = (R5 + 1) * 256;
                // 19: R4 = R4 * 256        #  
                // 20: R4 = bool(R4 > R1)   #      if (R4>R1) { break; } 
                // 21: ip = R4 + 21         #
                // 22: ip = 23              #
                // 23: ip = 25              #
                // 24: R5 = R5 + 1          #      R5++;
                // 25: ip = 17              #    }
                // 26: R1 = R5              #    R1 = R5;
                // 27: ip = 7               #  }
                // 
                // 28: R5 = bool(R3 = R0)   #  if (R3=R0) { break; }
                // 29: ip = R5 + 29         #
                // 30: ip = 5               # }

                // ------------------------------------------------------------

                // Pseudocode:
                // 
                // R3 = 123;
                // do {
                //    R3 = R3 & 456;                    # 123 & 456 == 72, always
                // } while (R3 != 72);                  
                // 
                // R3 = 0;
                // do {
                //   R1 = R3 | 65536;
                //   R3 = 10373714;
                //   while (true) {
                //     R3 = (((R3 + (R1 & 255)) & 16777215) * 65899) & 16777215;
                //     if (256>R1) { break; }
                // 
                //     R5 = 0;                          #
                //     while ((R5 + 1) * 256 <= R1) {   #
                //       R5++;                          #
                //     }                                #
                //     R1 = R5;                         # R1 = R1 / 256
                //   }
                // } while (R3 != R0);

                var R0 = 0;
                var R1 = 0;
                var R3 = 0;

                R3 = 0;
                do 
                {
                    R1 = R3 | 65536;
                    R3 = 10373714;
                    while (true) 
                    {
                        R3 = (((R3 + (R1 & 255)) & 16777215) * 65899) & 16777215;

                        if (256>R1) { break; }
                
                        R1 = R1 / 256;
                    }

                    R0 = R3; // to halt after one pass
                } while (R3 != R0);

                Console.WriteLine(R0);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                //var seen = new HashSet<int>();
                var i = 0;

                var R0 = 0;
                var R1 = 0;
                var R3 = 0;

                R3 = 0;
                do
                {
                    R1 = R3 | 65536;
                    R3 = 10373714;
                    while (true)
                    {
                        R3 = (((R3 + (R1 & 255)) & 16777215) * 65899) & 16777215;

                        if (256 > R1) { break; }

                        R1 = R1 / 256;
                    }

                    //if (seen.Contains(R3))
                    //{
                    //    Console.WriteLine($"{i}: {R3}");
                    //    Console.ReadLine();
                    //}
                    //else
                    //{
                    //    seen.Add(R3);
                    //}

                    // R3 values start repeating at 10497-th iteration, so stop just before that
                    if (i == 10497 - 1)
                    {
                        R0 = R3; // to halt after one pass
                    }

                    i++;
                } while (R3 != R0);

                Console.WriteLine(R0);
            }
        }

        private enum Mode { Value, Register }

        private class InstructionHandler
        {
            private static readonly Func<int, int, int> add = (x, y) => x + y;

            public static readonly InstructionHandler Addr = new(nameof(Addr), Mode.Register, Mode.Register, add);
            public static readonly InstructionHandler Addi = new(nameof(Addi), Mode.Register, Mode.Value, add);


            private static readonly Func<int, int, int> mul = (x, y) => x * y;

            public static readonly InstructionHandler Mulr = new(nameof(Mulr), Mode.Register, Mode.Register, mul);
            public static readonly InstructionHandler Muli = new(nameof(Muli), Mode.Register, Mode.Value, mul);


            private static readonly Func<int, int, int> and = (x, y) => x & y;

            public static readonly InstructionHandler Banr = new(nameof(Banr), Mode.Register, Mode.Register, and);
            public static readonly InstructionHandler Bani = new(nameof(Bani), Mode.Register, Mode.Value, and);


            private static readonly Func<int, int, int> or = (x, y) => x | y;

            public static readonly InstructionHandler Borr = new(nameof(Borr), Mode.Register, Mode.Register, or);
            public static readonly InstructionHandler Bori = new(nameof(Bori), Mode.Register, Mode.Value, or);

            private static readonly Func<int, int, int> set = (x, _) => x;

            public static readonly InstructionHandler Setr = new(nameof(Setr), Mode.Register, default, set);
            public static readonly InstructionHandler Seti = new(nameof(Seti), Mode.Value, default, set);


            private static readonly Func<int, int, int> gt = (x, y) => x > y ? 1 : 0;

            public static readonly InstructionHandler Gtir = new(nameof(Gtir), Mode.Value, Mode.Register, gt);
            public static readonly InstructionHandler Gtri = new(nameof(Gtri), Mode.Register, Mode.Value, gt);
            public static readonly InstructionHandler Gtrr = new(nameof(Gtrr), Mode.Register, Mode.Register, gt);


            private static readonly Func<int, int, int> eq = (x, y) => x == y ? 1 : 0;

            public static readonly InstructionHandler Eqir = new(nameof(Eqir), Mode.Value, Mode.Register, eq);
            public static readonly InstructionHandler Eqri = new(nameof(Eqri), Mode.Register, Mode.Value, eq);
            public static readonly InstructionHandler Eqrr = new(nameof(Eqrr), Mode.Register, Mode.Register, eq);


            public static readonly IReadOnlyList<InstructionHandler> All = new[]
            {
                Addr, Addi,
                Mulr, Muli,
                Banr, Bani,
                Borr, Bori,
                Setr, Seti,
                Gtir, Gtri, Gtrr,
                Eqir, Eqri, Eqrr,
            };

            private readonly Func<int, int, int> apply;

            private InstructionHandler(string name, Mode modeA, Mode modeB, Func<int, int, int> apply)
            {
                this.Name = name.ToLower();
                this.ModeA = modeA;
                this.ModeB = modeB;
                this.apply = apply;
            }

            public string Name { get; }
            public Mode ModeA { get; }
            public Mode ModeB { get; }
        }

        private sealed class Instruction
        {
            public Instruction(InstructionHandler handler, int inA, int inB, int @out)
            {
                Handler = handler;
                InA = inA;
                InB = inB;
                Out = @out;
            }

            public InstructionHandler Handler { get; }
            public int InA { get; }
            public int InB { get; }
            public int Out { get; }
        }

        private class IpBindDeclaration
        {
            public IpBindDeclaration(int boundRegister)
            {
                BoundRegister = boundRegister;
            }

            public int BoundRegister { get; }
        }

        private static class Parser
        {
            public static (IpBindDeclaration, IReadOnlyList<Instruction>) ParseProgram(IReadOnlyList<string> lines)
            {
                var declaration = ParseDeclaration(lines[0]);
                var instructions = lines.Skip(1).Select(ParseInstruction).ToList();
                return (declaration, instructions);
            }

            private static IpBindDeclaration ParseDeclaration(string line)
            {
                const string prefix = "#ip ";

                var register = int.Parse(line.Substring(prefix.Length));
                return new IpBindDeclaration(register);
            }

            private static Instruction ParseInstruction(string line)
            {
                var parts = line.Split(' ');

                var handler = InstructionHandler.All.First(h => h.Name == parts[0]);
                var args = parts.Skip(1).Take(3).Select(int.Parse).ToList();

                return new Instruction(handler, args[0], args[1], args[2]);
            }
        }

        private static class Decompiler
        {
            public static void Print(IpBindDeclaration declaration, IReadOnlyList<Instruction> program)
            {
                static Op ClassifyOp(InstructionHandler handler) =>
                    handler.Name switch
                    {
                        "addr" or "addi" => Op.Add,
                        "mulr" or "muli" => Op.Mul,
                        "banr" or "bani" => Op.And,
                        "borr" or "bori" => Op.Or,
                        "setr" or "seti" => Op.Set,
                        "gtir" or "gtri" or "gtrr" => Op.Gt,
                        "eqir" or "eqri" or "eqrr" => Op.Eq,

                        _ => throw new Exception($"unknown instruction handler {handler.Name}")
                    };

                static Arg ClassifyArg(Mode mode, int value, IpBindDeclaration declaration) =>
                    mode switch
                    {
                        Mode.Value => new Arg.Literal(value),
                        Mode.Register when value == declaration.BoundRegister => Arg.Ip.Instance,
                        Mode.Register or _ => new Arg.Register(value)
                    };

                var instructions = program
                    .Select((instr, index) => new Instr(
                        index,
                        ClassifyOp(instr.Handler),
                        ClassifyArg(instr.Handler.ModeA, instr.InA, declaration),
                        ClassifyArg(instr.Handler.ModeB, instr.InB, declaration),
                        ClassifyArg(Mode.Register, instr.Out, declaration)
                    ))
                    .Select(instr => instr.Simplify())
                    .ToList();

                foreach (var instr in instructions)
                {
                    Console.WriteLine(instr);
                }
            }

            private abstract class Arg
            {
                private Arg() { }

                public sealed class Literal : Arg
                {
                    public Literal(int value)
                    {
                        Value = value;
                    }

                    public int Value { get; }

                    public override string ToString() => this.Value.ToString();
                }

                public sealed class Register : Arg
                {
                    public Register(int index)
                    {
                        Index = index;
                    }

                    public int Index { get; }

                    public override string ToString() => $"R{this.Index}";
                }

                public sealed class Ip : Arg
                {
                    public static readonly Arg Instance = new Ip();

                    private Ip() { }

                    public override string ToString() => "ip";
                }
            }

            private enum Op { Add, Mul, And, Or, Set, Gt, Eq };

            private record Instr(int Index, Op Op, Arg InA, Arg InB, Arg Out)
            {
                public Instr Simplify()
                {
                    static Instr ReplaceIpArgs(Instr instr)
                    {
                        static Arg TryReplace(Arg arg, int index) =>
                            arg is Arg.Ip ? new Arg.Literal(index) : arg;

                        return instr with
                        {
                            InA = TryReplace(instr.InA, instr.Index),
                            InB = TryReplace(instr.InB, instr.Index)
                        };
                    }

                    static Instr PreCalc(Instr instr)
                    {
                        static Instr TryReplace(Instr instr, Func<int, int, int> calc)
                        {
                            if (instr.InA is not Arg.Literal litA)
                            {
                                return instr;
                            }

                            if (instr.InB is not Arg.Literal litB)
                            {
                                return instr;
                            }

                            return instr with
                            {
                                Op = Op.Set,
                                InA = new Arg.Literal(calc(litA.Value, litB.Value)),
                                InB = new Arg.Literal(0) // ignored in Set 
                            };
                        }

                        return instr.Op switch
                        {
                            Op.Add => TryReplace(instr, static (x, y) => x + y),
                            Op.Mul => TryReplace(instr, static (x, y) => x * y),
                            Op.And => TryReplace(instr, static (x, y) => x & y),
                            Op.Or => TryReplace(instr, static (x, y) => x | y),
                            Op.Gt => TryReplace(instr, static (x, y) => x > y ? 1 : 0),
                            Op.Eq => TryReplace(instr, static (x, y) => x == y ? 1 : 0),

                            // No need to convert Set
                            _ => instr
                        };
                    }

                    return PreCalc(ReplaceIpArgs(this));
                }

                public override string ToString()
                {
                    static string Text(Op op, Arg inA, Arg inB, Arg @out) =>
                        op switch
                        {
                            Op.Add => $"{@out} = {inA} + {inB}",
                            Op.Mul => $"{@out} = {inA} * {inB}",
                            Op.And => $"{@out} = {inA} & {inB}",
                            Op.Or => $"{@out} = {inA} | {inB}",
                            Op.Set => $"{@out} = {inA}",
                            Op.Gt => $"{@out} = bool({inA} > {inB})",
                            Op.Eq => $"{@out} = bool({inA} = {inB})",
                            
                            _ => "???"
                        };

                    return $"{Index,2}: {Text(Op, InA, InB, Out)}";
                }
            }
        }
    }
}
