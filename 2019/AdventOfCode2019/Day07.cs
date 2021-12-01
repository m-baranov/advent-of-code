using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day07
    {
        public static readonly IInput Sample1Input =
            Input.Literal("3,15,3,16,1002,16,10,16,1,16,15,15,4,15,99,0,0");

        public static readonly IInput Sample2Input =
            Input.Literal("3,23,3,24,1002,24,10,24,1002,23,-1,23," +
                          "101,5,23,23,1,24,23,23,4,23,99,0,0");

        public static readonly IInput Sample3Input =
            Input.Literal("3,31,3,32,1002,32,10,32,1001,31,-2,31,1007,31,0,33," +
                          "1002,33,7,33,1,33,31,31,1,32,31,31,4,31,99,0,0,0");

        public static readonly IInput Sample4Input =
            Input.Literal("3,26,1001,26,-4,26,3,27,1002,27,2,27,1,27,26," +
                          "27,4,27,1001,28,-1,28,1005,28,6,99,0,0,5");

        public static readonly IInput Sample5Input =
            Input.Literal("3,52,1001,52,-5,52,3,53,1,52,56,54,1007,54,5,55,1005,55,26,1001,54," +
                          "-5,54,1105,1,12,1,53,54,53,1008,54,0,55,1001,55,1,55,2,53,55,53,4," +
                          "53,1001,56,-1,56,1005,56,6,99,0,0,0,0,10");

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/7/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();

                var answer = PossiblePhases()
                    .Select(phases => new { phases, signal = RunSequence(program, phases) })
                    .MaxBy(r => r.signal);

                Console.WriteLine($"phases = {string.Join(',', answer.phases)}; signal = {answer.signal}");
            }

            private int RunSequence(string program, IReadOnlyList<int> phases)
            {
                var signal = 0;
                foreach (var phase in phases)
                {
                    signal = RunOnce(program, phase, signal);
                }
                return signal;
            }

            private int RunOnce(string program, int phase, int signal)
            {
                var computer = Day05.Computer.Of(program, new[] { phase, signal });
                computer.Execute();
                return computer.Output.Values().First();
            }

            private IEnumerable<IReadOnlyList<int>> PossiblePhases() => 
                EnumerableExtensions.AllPossibleOrders(new[] { 0, 1, 2, 3, 4 });
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();

                var answer = PossiblePhases()
                    .Select(phases => new { phases, signal = RunSequence(program, phases) })
                    .MaxBy(r => r.signal);

                Console.WriteLine($"phases = {string.Join(',', answer.phases)}; signal = {answer.signal}");
            }

            private int RunSequence(string program, IReadOnlyList<int> phases)
            {
                var computers = phases
                    .Select(phase => Day07.Computer.Of(program, new[] { phase }))
                    .ToArray();

                var signal = 0;
                var index = 0;
                while (true)
                {
                    var computer = computers[index];
                    computer.Input.Enter(signal);

                    var result = computer.Execute();
                    signal = computer.Output.Values().Last();

                    if (index == computers.Length - 1 && result is Day07.Computer.Result.Halt)
                    {
                        return signal;
                    }

                    index++;
                    if (index >= computers.Length)
                    {
                        index = 0;
                    }
                }
            }

            private IEnumerable<IReadOnlyList<int>> PossiblePhases() =>
                EnumerableExtensions.AllPossibleOrders(new[] { 5, 6, 7, 8, 9 });
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
            private int ip;

            public Computer(Memory memory, InputDevice input, OutputDevice output)
            {
                this.memory = memory;
                this.input = input;
                this.output = output;
                this.ip = 0;
            }

            public InputDevice Input => input;
            public OutputDevice Output => output;

            public Result Execute()
            {
                while (true)
                {
                    var instruction = new Instruction(memory, this.ip);
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
                private readonly Queue<int> values;

                public InputDevice(IReadOnlyList<int> values)
                {
                    this.values = new Queue<int>(values);
                }

                public bool HasData() => values.Count > 0;

                public int Read()
                {
                    return this.values.Dequeue();
                }

                public void Enter(int value)
                {
                    this.values.Enqueue(value);
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
