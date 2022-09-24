using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day19
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "#ip 0",
                    "seti 5 0 1",
                    "seti 6 0 2",
                    "addi 0 1 0",
                    "addr 1 2 3",
                    "setr 1 0 0",
                    "seti 8 0 4",
                    "seti 9 0 5"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/19/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var (declaration, program) = Parser.ParseProgram(input.Lines().ToList());

                var registers = Cpu.Execute(declaration, program);

                Console.WriteLine($"R0: {registers.GetRegister(0)}");
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var (declaration, program) = Parser.ParseProgram(input.Lines().ToList());
                Decompiler.Print(declaration, program);

                // Output (annotated):
                // 
                //           0: ip = sub_1 (16)
                //
                // sub_3:    1: R4 = 1
                //           2: R1 = 1
                //           
                //           3: R3 = R4 * R1         # if R4*R1=R5 { R0 = R4 + R0 }
                //           4: R3 = bool(R3 = R5)   #
                //           5: ip = R3 + 5          #
                //           6: ip = 7               #
                //           7: R0 = R4 + R0         # 
                //           
                //           8: R1 = R1 + 1
                //           
                //           9: R3 = bool(R1 > R5)   # if R1>R5 jmp 12, else jmp 3
                //          10: ip = 10 + R3         #
                //          11: ip = 2               #
                //          
                //          12: R4 = R4 + 1
                //          13: R3 = bool(R4 > R5)   # if R4>R5 exit, else jmp 2
                //          14: ip = R3 + 14
                //          15: ip = 1
                //          16: ip = 256
                //          
                // sub_1:   17: R5 = R5 + 2
                //          18: R5 = R5 * R5
                //          19: R5 = 19 * R5
                //          20: R5 = R5 * 11         # R5 = (0+2)*(0+2)*19*11=836
                //          21: R3 = R3 + 8      
                //          22: R3 = R3 * 22
                //          23: R3 = R3 + 5          # R3 = (0+8)*22+5= 181
                //          24: R5 = R5 + R3         # R5 = 836+181 = 1017
                //          25: ip = 25 + R0         # if R0=1, jmp part2
                //          26: ip = sub_3 (0)
                //          
                // sub_2:   27: R3 = 27
                //          28: R3 = R3 * 28
                //          29: R3 = 29 + R3
                //          30: R3 = 30 * R3
                //          31: R3 = R3 * 14
                //          32: R3 = R3 * 32         # R3 = (27 * 28 + 29) * 30 * 14 * 32 = 10550400
                //          33: R5 = R5 + R3         # R5 = 1017 + 10550400 = 10551417
                //          34: R0 = 0
                //          35: ip = sub_3 (0)


                // Pseudo-code:
                //
                // R5 = R0 == 0 ? 1017 /* part1 */ : 10_551_417 /* part2 */;
                // 
                // R4=1;
                // do {
                //   R1=1;
                //   
                //   do {
                //     if (R4 * R1 = R5) { R0 = R0 + R4; }
                //     R1++;
                //   } while (R1 <= R5);
                //   
                //   R4++;
                // while (R4 <= R5);


                // Answer: find sum of all divisors of R5 

                var n = 10_551_417;

                var sum = 0;
                for (var i = 1; i <= n; i++)
                {
                    if (n % i == 0)
                    {
                        sum += i;
                    }
                }

                Console.WriteLine($"Answer: {sum}");
            }
        }

        private class Registers
        {
            public const int Count = 6;

            private int ip;
            private readonly int registerIpIsBoundTo;
            private readonly int[] registers;

            public Registers(int registerIpIsBoundTo)
            {
                this.ip = 0;
                this.registers = new int[Count];
                this.registerIpIsBoundTo = registerIpIsBoundTo;
            }

            public int GetIp() => this.ip;

            public int GetRegister(int index) => this.registers[index];

            public void SetRegister(int index, int value) => this.registers[index] = value;
             
            public void BeforeInstruction()
            {
                this.registers[this.registerIpIsBoundTo] = this.ip;
            }

            public void AfterInstruction()
            {
                this.ip = this.registers[this.registerIpIsBoundTo] + 1;
            }

            public override string ToString() => $"ip={this.ip}, regs={string.Join(',', this.registers)}";
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

            public void Run(Registers registers, int inA, int inB, int @out)
            {
                static int Value(Mode mode, int @in, Registers registers) =>
                    mode == Mode.Value ? @in : registers.GetRegister(@in);

                var valA = Value(this.ModeA, inA, registers);
                var valB = Value(this.ModeB, inB, registers);

                var result = this.apply(valA, valB);
                registers.SetRegister(@out, result);
            }
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

            public void Run(Registers registers) => Handler.Run(registers, InA, InB, Out);
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

        private static class Cpu
        {
            public static Registers Execute(
                IpBindDeclaration declaration, 
                IReadOnlyList<Instruction> program,
                int r0 = 0)
            {
                var registers = new Registers(declaration.BoundRegister);
                registers.SetRegister(0, r0);

                while (true)
                {
                    var ip = registers.GetIp();
                    if (ip < 0 || ip >= program.Count)
                    {
                        break;
                    }

                    registers.BeforeInstruction();

                    var instruction = program[ip];
                    instruction.Run(registers);

                    registers.AfterInstruction();
                }

                return registers;
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
                            Op.Gt => TryReplace(instr, static (x, y) => x > y ? 1 : 0),
                            Op.Eq => TryReplace(instr, static (x, y) => x == y ? 1 : 0),

                            // Don't bother with And or Or, there is none in the sample or test input
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
                            Op.Set => $"{@out} = {inA}",
                            Op.Gt => $"{@out} = bool({inA} > {inB})",
                            Op.Eq => $"{@out} = bool({inA} = {inB})",

                            // Don't bother with And or Or, there is none in the sample or test input
                            _ => "?"
                        };

                    return $"{Index,2}: {Text(Op, InA, InB, Out)}";
                }
            }
        }
    }
}
