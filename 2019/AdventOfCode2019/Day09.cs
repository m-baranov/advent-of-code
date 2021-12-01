using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day09
    {
        public static readonly IInput Sample1Input =
            Input.Literal("109,1,204,-1,1001,100,1,100,1008,100,16,101,1006,101,0,99");

        public static readonly IInput Sample2Input =
            Input.Literal("1102,34915192,34915192,7,4,7,99,0");

        public static readonly IInput Sample3Input =
            Input.Literal("104,1125899906842624,99");

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/9/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();

                var computer = Computer.Of(program, new[] { 1L });
                computer.Execute();

                Console.WriteLine(computer.Output.Text());
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();

                var computer = Computer.Of(program, new[] { 2L });
                computer.Execute();

                Console.WriteLine(computer.Output.Text());
            }
        }

        public class Computer
        {
            public static Computer Of(string programText, IReadOnlyList<long> inputs = null)
            {
                var program = programText.Split(',').Select(long.Parse).ToArray();

                var memory = new Memory(program);
                var inputDevice = new InputDevice(inputs ?? Array.Empty<long>());
                var outputDevice = new OutputDevice();

                return new Computer(memory, inputDevice, outputDevice);
            }

            private readonly Memory memory;
            private readonly InputDevice input;
            private readonly OutputDevice output;
            private int ip;
            private long relativeBase;

            public Computer(Memory memory, InputDevice input, OutputDevice output)
            {
                this.memory = memory;
                this.input = input;
                this.output = output;
                this.ip = 0;
                this.relativeBase = 0;
            }

            public InputDevice Input => input;
            public OutputDevice Output => output;
            public Memory Mem => memory;

            public Result Execute()
            {
                while (true)
                {
                    var instruction = new Instruction(memory, this.ip, this.relativeBase);
                    var result = ExecuteInstruction(instruction);

                    if (result is Result.Advance advance)
                    {
                        this.ip += advance.By;
                    }
                    else if (result is Result.Jump jump)
                    {
                        this.ip = jump.Address;
                    }
                    else
                    {
                        if (result is Result.Error error)
                        {
                            Console.WriteLine($"ERROR: {error.Message}.");
                        }
                        return result;
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
                else if (instr.Opcode == 9)
                {
                    return ExecuteIncrRelativeBase(instr);
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

            private Result ExecuteBinaryOperation(Instruction instr, Func<long, long, long> operation)
            {
                var val1 = memory.Read(instr.ParameterAddress(0));
                var val2 = memory.Read(instr.ParameterAddress(1));

                memory.Write(instr.ParameterAddress(2), operation(val1, val2));

                return new Result.Advance(by: 4);
            }

            private Result ExecuteRead(Instruction instr)
            {
                if (!input.HasData())
                {
                    return new Result.WaitingForInput();
                }

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

            private Result ExecuteConditionalJump(Instruction instr, Func<long, bool> shouldJump)
            {
                var val = memory.Read(instr.ParameterAddress(0));
                var addr = memory.Read(instr.ParameterAddress(1));

                if (shouldJump(val))
                {
                    return new Result.Jump((int)addr);
                }
                else
                {
                    return new Result.Advance(by: 3);
                }
            }

            private Result ExecuteIncrRelativeBase(Instruction instr)
            {
                var val = memory.Read(instr.ParameterAddress(0));

                this.relativeBase += val;

                return new Result.Advance(by: 2);
            }

            public class Memory
            {
                private long[] values;

                public Memory(long[] values)
                {
                    this.values = values;
                }

                public long Read(int address)
                {
                    EnsureAddressExists(address);

                    if (address >= this.values.Length)
                    {
                        Console.WriteLine($"ERROR: attempt to read outside of bounds at '{address}'.");
                    }

                    return this.values[address];
                }

                public void Write(int address, long value)
                {
                    EnsureAddressExists(address);

                    if (address >= this.values.Length)
                    {
                        Console.WriteLine($"ERROR: attempt to write outside of bounds at '{address}'.");
                    }

                    this.values[(int)address] = value;
                }

                private void EnsureAddressExists(int address)
                {
                    if (address < this.values.Length)
                    {
                        return;
                    }

                    var length = this.values.Length;
                    while (length <= address)
                    {
                        length *= 2;
                    }

                    var newValues = new long[length];
                    this.values.CopyTo(newValues, 0);

                    this.values = newValues;
                }
            }

            public class InputDevice
            {
                private readonly Queue<long> values;

                public InputDevice(IReadOnlyList<long> values)
                {
                    this.values = new Queue<long>(values);
                }

                public bool HasData() => values.Count > 0;

                public long Read()
                {
                    return this.values.Dequeue();
                }

                public void Enter(long value)
                {
                    this.values.Enqueue(value);
                }
            }

            public class OutputDevice
            {
                private readonly List<long> values;

                public OutputDevice()
                {
                    this.values = new List<long>();
                }

                public void Write(long value)
                {
                    this.values.Add(value);
                }

                public IReadOnlyList<long> Values() => this.values;

                public string Text() => string.Join(',', Values());
            }

            private class Instruction
            {
                private readonly Memory memory;
                private readonly int ip;
                private readonly long relativeBase;
                private readonly IReadOnlyList<ParameterMode> modes;

                public Instruction(Memory memory, int ip, long relativeBase)
                {
                    this.memory = memory;
                    this.ip = ip;
                    this.relativeBase = relativeBase;

                    var (opcode, modes) = ParseInstruction(memory.Read(ip));
                    this.Opcode = opcode;
                    this.modes = modes;
                }

                public int Opcode { get; }

                private static (int opcode, IReadOnlyList<ParameterMode>) ParseInstruction(long value)
                {
                    var modesValue = Math.DivRem(value, 100, out var opcode);
                    return ((int)opcode, ParseParameterModes(modesValue));
                }

                private static IReadOnlyList<ParameterMode> ParseParameterModes(long value)
                {
                    var modes = new List<ParameterMode>();

                    while (value != 0)
                    {
                        value = Math.DivRem(value, 10, out var rem);
                        modes.Add(ParseParameterMode(rem));
                    }

                    return modes;
                }

                private static ParameterMode ParseParameterMode(long value)
                {
                    if (value == 1) return ParameterMode.Immediate;
                    if (value == 2) return ParameterMode.Relative;
                    return ParameterMode.Position;
                }

                public int ParameterAddress(int parameterIndex)
                {
                    var mode = ParameterModeAt(parameterIndex);
                    var address = ip + 1 + parameterIndex;

                    if (mode == ParameterMode.Position)
                    {
                        return (int)memory.Read(address);
                    }
                    else if (mode == ParameterMode.Relative)
                    {
                        return (int)(this.relativeBase + memory.Read(address));
                    }
                    else /* if (mode == ParameterMode.Immediate) */
                    {
                        return address;
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

                private enum ParameterMode { Position, Immediate, Relative }
            }

            public abstract class Result
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

                public class WaitingForInput : Result { }

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
