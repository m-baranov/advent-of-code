using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day05
    {
        public static readonly IInput SampleInput =
            Input.Literal();

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/5/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();

                var computer = Computer.Of(program, inputs: new[] { 1 });
                computer.Execute();

                Console.WriteLine(computer.Output.Text());
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();

                var computer = Computer.Of(program, inputs: new[] { 5 });
                computer.Execute();

                Console.WriteLine(computer.Output.Text());
            }
        }

        public class Memory
        {
            private readonly int[] values;

            public Memory(int[] values)
            {
                this.values = values;
            }

            public int Read(int address)
            {
                if (address >= this.values.Length)
                {
                    Console.WriteLine($"ERROR: attempt to read outside of bounds at '{address}'.");
                }

                return this.values[address];
            }

            public void Write(int address, int value)
            {
                if (address >= this.values.Length)
                {
                    Console.WriteLine($"ERROR: attempt to write outside of bounds at '{address}'.");
                }

                this.values[address] = value;
            }
        }

        public class InputDevice
        {
            private readonly IReadOnlyList<int> values;
            private int nextIndex;

            public InputDevice(IReadOnlyList<int> values)
            {
                this.values = values;
                this.nextIndex = 0;
            }

            public int Read()
            {
                var value = this.values[this.nextIndex];
                this.nextIndex++;
                return value;
            }
        }

        public class OutputDevice
        {
            private readonly List<int> values;

            public OutputDevice()
            {
                this.values = new List<int>();
            }

            public void Write(int value)
            {
                this.values.Add(value);
            }

            public IReadOnlyList<int> Values() => this.values;

            public string Text() => string.Join(',', Values());
        }

        public class Computer
        {
            public static Computer Of(string programText, IReadOnlyList<int> inputs)
            {
                var program = programText.Split(',').Select(int.Parse).ToArray();

                var memory = new Memory(program);
                var inputDevice = new InputDevice(inputs);
                var outputDevice = new OutputDevice();

                return new Computer(memory, inputDevice, outputDevice);
            }

            private readonly Memory memory;
            private readonly InputDevice input;
            private readonly OutputDevice output;

            public Computer(Memory memory, InputDevice input, OutputDevice output)
            {
                this.memory = memory;
                this.input = input;
                this.output = output;
            }

            public OutputDevice Output => output;

            public void Execute()
            {
                var ip = 0;
                while (true)
                {
                    var instruction = new Instruction(memory, ip);
                    var result = ExecuteInstruction(instruction);
                    
                    if (result is Result.Advance advance)
                    {
                        ip += advance.By;
                    }
                    else if (result is Result.Jump jump)
                    {
                        ip = jump.Address;
                    }
                    else if (result is Result.Error error)
                    {
                        Console.WriteLine($"ERROR: {error.Message}.");
                        break;
                    }
                    else /* if (result is Result.Halt) */
                    {
                        break;
                    }
                }
            }

            private Result ExecuteInstruction(Instruction instr)
            {
                if (instr.Opcode == 1)
                {
                    return ExecuteAdd(instr);
                }
                else if (instr.Opcode == 2)
                {
                    return ExecuteMul(instr);
                }
                else if (instr.Opcode == 3)
                {
                    return ExecuteRead(instr);
                }
                else if (instr.Opcode == 4)
                {
                    return ExecuteWrite(instr);
                }
                else if (instr.Opcode == 5)
                {
                    return ExecuteJumpIfTrue(instr);
                }
                else if (instr.Opcode == 6)
                {
                    return ExecuteJumpIfFalse(instr);
                }
                else if (instr.Opcode == 7)
                {
                    return ExecuteCompareLessThan(instr);
                }
                else if (instr.Opcode == 8)
                {
                    return ExecuteCompareEquals(instr);
                }
                else if (instr.Opcode == 99)
                {
                    return new Result.Halt();
                }
                else
                {
                    return new Result.Error($"unknown opcode '{instr.Opcode}'");
                }
            }

            private Result ExecuteAdd(Instruction instr) =>
                ExecuteBinaryOperation(instr, (a, b) => a + b);

            private Result ExecuteMul(Instruction instr) =>
                ExecuteBinaryOperation(instr, (a, b) => a * b);

            private Result ExecuteCompareLessThan(Instruction instr) =>
                ExecuteBinaryOperation(instr, (a, b) => a < b ? 1 : 0);

            private Result ExecuteCompareEquals(Instruction instr) =>
                ExecuteBinaryOperation(instr, (a, b) => a == b ? 1 : 0);

            private Result ExecuteBinaryOperation(Instruction instr, Func<int, int, int> operation)
            {
                var val1 = memory.Read(instr.ParameterAddress(0));
                var val2 = memory.Read(instr.ParameterAddress(1));

                memory.Write(instr.ParameterAddress(2), operation(val1, val2));

                return new Result.Advance(by: 4);
            }

            private Result ExecuteRead(Instruction instr)
            {
                var val = input.Read();

                memory.Write(instr.ParameterAddress(0), val);

                return new Result.Advance(by: 2);
            }

            private Result ExecuteWrite(Instruction instr)
            {
                var val = memory.Read(instr.ParameterAddress(0));

                output.Write(val);

                return new Result.Advance(by: 2);
            }

            private Result ExecuteJumpIfTrue(Instruction instr) => 
                ExecuteConditionalJump(instr, a => a != 0);

            private Result ExecuteJumpIfFalse(Instruction instr) => 
                ExecuteConditionalJump(instr, a => a == 0);

            private Result ExecuteConditionalJump(Instruction instr, Func<int, bool> shouldJump)
            {
                var val = memory.Read(instr.ParameterAddress(0));
                var addr = memory.Read(instr.ParameterAddress(1));

                if (shouldJump(val))
                {
                    return new Result.Jump(addr);
                }
                else
                {
                    return new Result.Advance(by: 3);
                }
            }

            private class Instruction
            {
                private readonly Memory memory;
                private readonly int ip;
                private readonly IReadOnlyList<ParameterMode> modes;

                public Instruction(Memory memory, int ip)
                {
                    this.memory = memory;
                    this.ip = ip;

                    var (opcode, modes) = ParseInstruction(memory.Read(ip));
                    this.Opcode = opcode;
                    this.modes = modes;
                }

                public int Opcode { get; }

                private static (int opcode, IReadOnlyList<ParameterMode>) ParseInstruction(int value)
                {
                    var modesValue = Math.DivRem(value, 100, out var opcode);
                    return (opcode, ParseParameterModes(modesValue));
                }

                private static IReadOnlyList<ParameterMode> ParseParameterModes(int value)
                {
                    var modes = new List<ParameterMode>();

                    while (value != 0)
                    {
                        value = Math.DivRem(value, 10, out var rem);
                        modes.Add(rem > 0 ? ParameterMode.Immediate : ParameterMode.Position);
                    }

                    return modes;
                }

                public int ParameterAddress(int parameterIndex)
                {
                    var mode = ParameterModeAt(parameterIndex);
                    var address = ip + 1 + parameterIndex;

                    if (mode == ParameterMode.Immediate)
                    {
                        return address;
                    }
                    else
                    {
                        return memory.Read(address);
                    }
                }

                private ParameterMode ParameterModeAt(int parameterIndex)
                {
                    if (parameterIndex < this.modes.Count)
                    {
                        return this.modes[parameterIndex];
                    }
                    else
                    {
                        return ParameterMode.Position;
                    }
                }

                private enum ParameterMode { Position, Immediate }
            }

            private abstract class Result
            {
                public class Advance : Result
                {
                    public Advance(int by)
                    {
                        By = by;
                    }

                    public int By { get; }
                }

                public class Jump : Result
                {
                    public Jump(int address)
                    {
                        Address = address;
                    }

                    public int Address { get; }
                }

                public class Halt : Result { }

                public class Error : Result
                {
                    public Error(string message)
                    {
                        Message = message;
                    }

                    public string Message { get; }
                }
            }
        }
    }
}
