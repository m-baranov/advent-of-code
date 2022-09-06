using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day16
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "Before: [3, 2, 1, 1]",
                    "9 2 1 2",
                    "After:  [3, 2, 2, 1]"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/16/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var (samples, program) = Parser.Parse(input.Lines());

                var answer = samples.Where(s => s.PossibleInstructions().Count > 2).Count();
                
                Console.WriteLine(answer);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var (samples, program) = Parser.Parse(input.Lines());

                var opcodeToInstruction = DeduceInstructionOpcodes(samples);

                foreach (var pair in opcodeToInstruction)
                {
                    Console.WriteLine($"{pair.Key}: {pair.Value.Name}");
                }
                Console.WriteLine();

                var registers = RunProgram(program, opcodeToInstruction);                
                Console.WriteLine($"reg0: {registers[0]}");
            }

            private static IReadOnlyDictionary<int, Instruction> DeduceInstructionOpcodes(
                IReadOnlyList<Sample> samples)
            {
                var opcodeToPossibility = new Dictionary<int, HashSet<Instruction>>();

                foreach (var sample in samples)
                {
                    var possibleInstructions = sample.PossibleInstructions();

                    if (opcodeToPossibility.TryGetValue(sample.Code.Opcode, out var existingInstructions))
                    {
                        existingInstructions.IntersectWith(possibleInstructions);
                    }
                    else
                    {
                        opcodeToPossibility[sample.Code.Opcode] = possibleInstructions.ToHashSet();
                    }
                }

                var opcodeToInstruction = new Dictionary<int, Instruction>();

                while (opcodeToPossibility.Count > 0)
                {
                    var definitivePairs = opcodeToPossibility.Where(p => p.Value.Count == 1).ToList();

                    foreach (var pair in definitivePairs)
                    {
                        opcodeToInstruction.Add(pair.Key, pair.Value.First());
                        opcodeToPossibility.Remove(pair.Key);
                    }

                    var definitiveInstructions = definitivePairs.Select(p => p.Value.First());

                    foreach (var pair in opcodeToPossibility)
                    {
                        pair.Value.RemoveRange(definitiveInstructions);
                    }
                }

                return opcodeToInstruction;
            }

            private IReadOnlyList<int> RunProgram(
                IReadOnlyList<InstructionCode> program,
                IReadOnlyDictionary<int, Instruction> opcodeToInstruction)
            {
                var registers = new Registers();

                foreach (var code in program)
                {
                    var instruction = opcodeToInstruction[code.Opcode];
                    instruction.Run(registers, code.InA, code.InB, code.Out);
                }

                return registers.Values();
            }
        }

        private class Registers
        {
            private readonly int[] values;

            public Registers() : this(new int[4]) { }

            public Registers(IReadOnlyList<int> values)
            {
                this.values = values.ToArray();
            }

            public int Get(int index) => this.values[index];

            public void Set(int index, int value) => this.values[index] = value;

            public bool ValuesEqual(IReadOnlyList<int> after) => this.values.SequenceEqual(after);

            public IReadOnlyList<int> Values() => this.values;
        }

        private enum Mode { Value, Register }

        private class Instruction
        {
            private static readonly Func<int, int, int> add = (x, y) => x + y;
            
            public static readonly Instruction Addr = new(nameof(Addr), Mode.Register, Mode.Register, add);
            public static readonly Instruction Addi = new(nameof(Addi), Mode.Register, Mode.Value, add);


            private static readonly Func<int, int, int> mul = (x, y) => x * y;
           
            public static readonly Instruction Mulr = new(nameof(Mulr), Mode.Register, Mode.Register, mul);
            public static readonly Instruction Muli = new(nameof(Muli), Mode.Register, Mode.Value, mul);


            private static readonly Func<int, int, int> and = (x, y) => x & y;

            public static readonly Instruction Banr = new(nameof(Banr), Mode.Register, Mode.Register, and);
            public static readonly Instruction Bani = new(nameof(Bani), Mode.Register, Mode.Value, and);


            private static readonly Func<int, int, int> or = (x, y) => x | y;

            public static readonly Instruction Borr = new(nameof(Borr), Mode.Register, Mode.Register, or);
            public static readonly Instruction Bori = new(nameof(Bori), Mode.Register, Mode.Value, or);

            private static readonly Func<int, int, int> set = (x, _) => x;

            public static readonly Instruction Setr = new(nameof(Setr), Mode.Register, default, set);
            public static readonly Instruction Seti = new(nameof(Seti), Mode.Value, default, set);


            private static readonly Func<int, int, int> gt = (x, y) => x > y ? 1 : 0;

            public static readonly Instruction Gtir = new(nameof(Gtir), Mode.Value, Mode.Register, gt);
            public static readonly Instruction Gtri = new(nameof(Gtri), Mode.Register, Mode.Value, gt);
            public static readonly Instruction Gtrr = new(nameof(Gtrr), Mode.Register, Mode.Register, gt);


            private static readonly Func<int, int, int> eq = (x, y) => x == y ? 1 : 0;

            public static readonly Instruction Eqir = new(nameof(Eqir), Mode.Value, Mode.Register, eq);
            public static readonly Instruction Eqri = new(nameof(Eqri), Mode.Register, Mode.Value, eq);
            public static readonly Instruction Eqrr = new(nameof(Eqrr), Mode.Register, Mode.Register, eq);


            public static readonly IReadOnlyList<Instruction> All = new[]
            {
                Addr, Addi,
                Mulr, Muli,
                Banr, Bani,
                Borr, Bori,
                Setr, Seti,
                Gtir, Gtri, Gtrr,
                Eqir, Eqri, Eqrr,
            };


            private readonly Mode modeA;
            private readonly Mode modeB;
            private readonly Func<int, int, int> apply;

            private Instruction(string name, Mode modeA, Mode modeB, Func<int, int, int> apply)
            {
                this.Name = name;
                this.modeA = modeA;
                this.modeB = modeB;
                this.apply = apply;
            }

            public string Name { get; }

            public void Run(Registers registers, int inA, int inB, int @out)
            {
                static int Value(Mode mode, int @in, Registers registers) =>
                    mode == Mode.Value ? @in : registers.Get(@in);

                var valA = Value(this.modeA, inA, registers);
                var valB = Value(this.modeB, inB, registers);

                var result = this.apply(valA, valB);
                registers.Set(@out, result);
            }
        }
        
        private record InstructionCode(int Opcode, int InA, int InB, int Out);

        private record Sample(IReadOnlyList<int> Before, InstructionCode Code, IReadOnlyList<int> After)
        {
            public IReadOnlyList<Instruction> PossibleInstructions() =>
                Instruction.All.Where(this.BehavesLike).ToList();

            public bool BehavesLike(Instruction instruction)
            {
                var registers = new Registers(this.Before);

                instruction.Run(registers, this.Code.InA, this.Code.InB, this.Code.Out);

                return registers.ValuesEqual(this.After);
            }
        }

        private static class Parser
        {
            private const string BeforeStart = "Before: [";
            private const string BeforeEnd = "]";

            private const string AfterStart = "After:  [";
            private const string AfterEnd = "]";

            private const string SampleSeparator = ", ";
            private const string CodeSeparator = " ";

            public static (IReadOnlyList<Sample>, IReadOnlyList<InstructionCode>) Parse(IEnumerable<string> lines)
            {
                var samples = new List<Sample>();
                var program = new List<InstructionCode>();

                var parsingSamples = true;

                var enumerator = lines.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (parsingSamples)
                    {
                        if (!TryParseOne(enumerator, out var sample, out var code))
                        {
                            continue;
                        }

                        if (sample != null)
                        {
                            samples.Add(sample);
                        }
                        else
                        {
                            program.Add(code);
                            parsingSamples = false;
                        }
                    }
                    else
                    {
                        var code = ParseCode(enumerator);
                        program.Add(code);
                    }
                }

                return (samples, program);
            }

            private static bool TryParseOne(
                IEnumerator<string> enumerator, 
                out Sample sample, 
                out InstructionCode code)
            {
                sample = default;
                code = default;

                var line = enumerator.Current;
                
                if (string.IsNullOrWhiteSpace(line))
                {
                    return false;
                }

                if (line.StartsWith(BeforeStart))
                {
                    var before = line;

                    enumerator.MoveNext();
                    var sampleCode = enumerator.Current;

                    enumerator.MoveNext();
                    var after = enumerator.Current;

                    sample = ParseSample(before, sampleCode, after);
                    return true;
                }
                else
                {
                    code = ParseCode(line);
                    return true;
                }
            }

            private static InstructionCode ParseCode(IEnumerator<string> enumerator)
            {
                return ParseCode(enumerator.Current);
            }

            private static Sample ParseSample(string before, string code, string after) =>
                new Sample(
                    Before: ParseRegisters(before, BeforeStart, BeforeEnd),
                    Code: ParseCode(code),
                    After: ParseRegisters(after, AfterStart, AfterEnd)
                );

            private static InstructionCode ParseCode(string line)
            {
                var values = ParseValues(line, CodeSeparator);
                return new InstructionCode(values[0], values[1], values[2], values[3]);
            }
            
            private static IReadOnlyList<int> ParseRegisters(string line, string start, string end)
            {
                var valuesText = line.Substring(start.Length, line.Length - start.Length - end.Length);
                return ParseValues(valuesText, SampleSeparator);
            }

            private static IReadOnlyList<int> ParseValues(string line, string sep) =>
                line.Split(sep).Select(int.Parse).ToList();
        }
    }
}
