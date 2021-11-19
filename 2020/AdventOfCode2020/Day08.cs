using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020
{
    static class Day08
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "nop +0",
                "acc +1",
                "jmp +4",
                "acc +3",
                "jmp -3",
                "acc -99",
                "acc +1",
                "jmp -4",
                "acc +6"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/8/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var instructions = input.Lines().Select(Instruction.Parse).ToList();

                var (_, acc) = Interpreter.Run(instructions);

                Console.WriteLine(acc);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var instructions = input.Lines().Select(Instruction.Parse).ToList();

                for (var i = 0; i < instructions.Count; i++)
                {
                    var instruction = instructions[i];
                    if (instruction.Operation == Operation.Jmp || instruction.Operation == Operation.Nop)
                    {
                        var replacement = instruction.Operation == Operation.Jmp
                            ? new Instruction(Operation.Nop, instruction.Argument)
                            : new Instruction(Operation.Jmp, instruction.Argument);

                        instructions[i] = replacement;

                        var (terminated, acc) = Interpreter.Run(instructions);
                        if (terminated)
                        {
                            Console.WriteLine(acc);
                            break;
                        }

                        instructions[i] = instruction;
                    }
                }
            }
        }

        static class Interpreter
        {
            public static (bool terminated, int acc) Run(IReadOnlyList<Instruction> instructions)
            {
                var seen = new bool[instructions.Count];

                var ip = 0;
                var acc = 0;

                while (true)
                {
                    if (ip < 0 || ip >= instructions.Count)
                    {
                        return (true, acc);
                    }

                    if (seen[ip])
                    {
                        return (false, acc);
                    }

                    seen[ip] = true;
                    var instruction = instructions[ip];

                    if (instruction.Operation == Operation.Acc)
                    {
                        acc += instruction.Argument;
                        ip++;
                    }
                    else if (instruction.Operation == Operation.Jmp)
                    {
                        ip += instruction.Argument;
                    }
                    else if (instruction.Operation == Operation.Nop)
                    {
                        ip++;
                    }
                }
            }
        }

        enum Operation { Nop, Acc, Jmp }

        class Instruction
        {
            public static Instruction Parse(string text)
            {
                var (operationText, argumentText) = SplitOperationAndArgument(text);
                
                var operation = ParseOperation(operationText);
                var argument = ParseArgument(argumentText);

                return new Instruction(operation, argument);
            }

            private static (string, string) SplitOperationAndArgument(string text)
            {
                var index = text.IndexOf(' ');
                return (text.Substring(0, index), text.Substring(index + 1));
            }

            private static Operation ParseOperation(string operationText)
            {
                if (operationText == "acc")
                {
                    return Operation.Acc;
                }
                if (operationText == "jmp")
                {
                    return Operation.Jmp;
                }
                return Operation.Nop;
            }

            private static int ParseArgument(string argumentText)
            {
                return int.Parse(argumentText);
            }

            public Instruction(Operation operation, int argument)
            {
                Operation = operation;
                Argument = argument;
            }

            public Operation Operation { get; }
            public int Argument { get; }
        }
    }
}
