using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day08
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "b inc 5 if a > 1",
                    "a inc 1 if b < 5",
                    "c dec -10 if a >= 1",
                    "c inc -20 if c == 10"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/8/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var instructions = input.Lines().Select(Instruction.Parse).ToList();

                var registers = new Registers();

                foreach (var instruction in instructions)
                {
                    Cpu.Run(instruction, registers);
                }

                var maxValue = registers.MaxValue();
                Console.WriteLine(maxValue);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var instructions = input.Lines().Select(Instruction.Parse).ToList();

                var registers = new Registers();
                var maxValue = 0;

                foreach (var instruction in instructions)
                {
                    Cpu.Run(instruction, registers);
                    maxValue = Math.Max(maxValue, registers.MaxValue());
                }

                Console.WriteLine(maxValue);
            }
        }

        private enum IntOperator { Inc, Dec }

        private enum BoolOperator { Lt, Gt, LtEq, GtEq, Eq, Neq }

        private record Condition(string Register, BoolOperator Operator, int Value);

        private record Operation(string Register, IntOperator Operator, int Value);

        private record Instruction(Operation Operation, Condition Condition)
        {
            public static Instruction Parse(string text)
            {
                static IntOperator ParseIntOperator(string text) =>
                    text switch
                    {
                        "inc" => IntOperator.Inc,
                        "dec" => IntOperator.Dec,
                        
                        _ => throw new Exception("Unknown int operator"),
                    };

                static BoolOperator ParseBoolOperator(string text) =>
                    text switch
                    {
                        "<" => BoolOperator.Lt,
                        "<=" => BoolOperator.LtEq,

                        ">" => BoolOperator.Gt,
                        ">=" => BoolOperator.GtEq,

                        "==" => BoolOperator.Eq,
                        "!=" => BoolOperator.Neq,

                        _ => throw new Exception("Unknown bool operator"),
                    };

                static Operation ParseOperation(string regText, string opText, string valText)
                {
                    var op = ParseIntOperator(opText);
                    var val = int.Parse(valText);
                    return new Operation(regText, op, val);
                }

                static Condition ParseCondition(string regText, string opText, string valText)
                {
                    var op = ParseBoolOperator(opText);
                    var val = int.Parse(valText);
                    return new Condition(regText, op, val);
                }

                // 0 1   2 3  4 5 6
                // b inc 5 if a > 1

                var words = text.Split(' ');

                var operation = ParseOperation(words[0], words[1], words[2]);
                var condition = ParseCondition(words[4], words[5], words[6]);

                return new Instruction(operation, condition);
            }
        }

        private class Registers
        {
            private readonly Dictionary<string, int> values;

            public Registers()
            {
                this.values = new Dictionary<string, int>();
            }

            public int Get(string register) => 
                this.values.TryGetValue(register, out var value) ? value : 0;

            public void Set(string register, int value) =>
                this.values[register] = value;

            public int MaxValue() => 
                this.values.Count > 0 
                    ? this.values.Select(p => p.Value).Max()
                    : 0;
        }

        private static class Cpu
        {
            public static void Run(Instruction instruction, Registers registers)
            {
                if (EvalCondition(instruction.Condition, registers))
                {
                    ApplyOperation(instruction.Operation, registers);
                }
            }

            private static void ApplyOperation(Operation operation, Registers registers)
            {
                var val = EvalOperation(operation, registers);
                registers.Set(operation.Register, val);
            }

            private static int EvalOperation(Operation operation, Registers registers)
            {
                var reg = registers.Get(operation.Register);
                var val = operation.Value;

                return operation.Operator switch
                {
                    IntOperator.Inc => reg + val,
                    IntOperator.Dec => reg - val,

                    _ => reg,
                };
            }

            private static bool EvalCondition(Condition condition, Registers registers)
            {
                var reg = registers.Get(condition.Register);
                var val = condition.Value;

                return condition.Operator switch
                {
                    BoolOperator.Lt => reg < val,
                    BoolOperator.LtEq => reg <= val,

                    BoolOperator.Gt => reg > val,
                    BoolOperator.GtEq => reg >= val,

                    BoolOperator.Eq => reg == val,
                    BoolOperator.Neq => reg != val,

                    _ => false,
                };
            }
        }
    }
}
