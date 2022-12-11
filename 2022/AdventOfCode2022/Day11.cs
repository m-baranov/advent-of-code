using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day11
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "Monkey 0:",
                    "  Starting items: 79, 98",
                    "  Operation: new = old * 19",
                    "  Test: divisible by 23",
                    "    If true: throw to monkey 2",
                    "    If false: throw to monkey 3",
                    "",
                    "Monkey 1:",
                    "  Starting items: 54, 65, 75, 74",
                    "  Operation: new = old + 6",
                    "  Test: divisible by 19",
                    "    If true: throw to monkey 2",
                    "    If false: throw to monkey 0",
                    "",
                    "Monkey 2:",
                    "  Starting items: 79, 60, 97",
                    "  Operation: new = old * old",
                    "  Test: divisible by 13",
                    "    If true: throw to monkey 1",
                    "    If false: throw to monkey 3",
                    "",
                    "Monkey 3:",
                    "  Starting items: 74",
                    "  Operation: new = old + 3",
                    "  Test: divisible by 17",
                    "    If true: throw to monkey 0",
                    "    If false: throw to monkey 1"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/11/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var monkeys = Monkey.ParseAll(input.Lines());

                var level = Monkey.Evaluate(monkeys, rounds: 20, relief: v => v / 3);
                Console.WriteLine(level);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var monkeys = Monkey.ParseAll(input.Lines());

                var div = monkeys.Select(m => m.Test.DivisibleBy).Aggregate((x, y) => x * y);

                var level = Monkey.Evaluate(monkeys, rounds: 10_000, relief: v => v % div);
                Console.WriteLine(level);
            }
        }

        private abstract class Operand
        {
            public static long Evaluate(Operand operand, long variable) =>
                operand switch
                {
                    Variable => variable,
                    Literal lit => lit.Value,
                    _ => throw new NotSupportedException(nameof(operand)),
                };

            public class Variable : Operand 
            {
                public static readonly Variable Instance = new();
                private Variable() { }
            }

            public class Literal : Operand
            {
                public Literal(long value)
                {
                    Value = value;
                }

                public long Value { get; }
            }
        }

        private enum Operator { Add, Mul }

        private record Operation(Operator Operator, Operand Left, Operand Right)
        {
            public static long Evaluate(Operation operation, long variable)
            {
                var left = Operand.Evaluate(operation.Left, variable);
                var right = Operand.Evaluate(operation.Right, variable);

                checked
                {
                    return operation.Operator switch
                    {
                        Operator.Add => left + right,
                        Operator.Mul => left * right,
                        _ => throw new NotSupportedException(nameof(operation.Operator)),
                    };
                }
            }
        }

        private record Test(int DivisibleBy, int IfTrue, int IfFalse)
        {
            public static int Evaluate(Test test, long variable) =>
                variable % test.DivisibleBy == 0 ? test.IfTrue : test.IfFalse;
        }

        private record Monkey(IReadOnlyList<long> Items, Operation Operation, Test Test)
        {
            public static IReadOnlyList<Monkey> ParseAll(IEnumerable<string> lines) =>
                lines.SplitByEmptyLine().Select(Parse).ToList();

            public static Monkey Parse(IReadOnlyList<string> lines)
            {
                // ignore like 0, with monkey number

                var items = ParseItems(lines[1]);
                var operation = ParseOperation(lines[2]);
                var test = ParseTest(lines[3], lines[4], lines[5]);

                return new Monkey(items, operation, test);
            }

            private static IReadOnlyList<long> ParseItems(string text)
            {
                const string Prefix = "  Starting items: ";
                return text.Substring(Prefix.Length).Split(", ").Select(long.Parse).ToList();
            }

            private static Operation ParseOperation(string text)
            {
                static Operand ParseOperand(string text) => 
                    text == "old" 
                        ? Operand.Variable.Instance 
                        : new Operand.Literal(long.Parse(text));

                static Operator ParseOperator(string text) =>
                    text == "+"
                        ? Operator.Add
                        : Operator.Mul;

                const string Prefix = "  Operation: new = ";

                var tokens = text.Substring(Prefix.Length).Split(' ');

                var op = ParseOperator(tokens[1]);
                var left = ParseOperand(tokens[0]);
                var right = ParseOperand(tokens[2]);

                return new Operation(op, left, right);
            }

            private static Test ParseTest(string conditionText, string ifTrueText, string ifFalseText)
            {
                static int ParseNumber(string prefix, string text) => int.Parse(text.Substring(prefix.Length));

                const string DivisibleByPrefix = "  Test: divisible by ";
                const string IfTruePrefix = "    If true: throw to monkey ";
                const string IfFalsePrefix = "    If false: throw to monkey ";

                var divisibleBy = ParseNumber(DivisibleByPrefix, conditionText);
                var ifTrueMonkey = ParseNumber(IfTruePrefix, ifTrueText);
                var ifFalseMonkey = ParseNumber(IfFalsePrefix, ifFalseText);

                return new Test(divisibleBy, ifTrueMonkey, ifFalseMonkey);
            }

            public static IReadOnlyList<(int monkey, long item)> Evaluate(Monkey monkey, Func<long, long> relief)
            {
                var throws = new List<(int monkey, long item)>();

                foreach (var item in monkey.Items)
                {
                    var worryLevel = item;
                    worryLevel = Operation.Evaluate(monkey.Operation, worryLevel);
                    worryLevel = relief(worryLevel);

                    var throwTo = Test.Evaluate(monkey.Test, worryLevel);
                    throws.Add((throwTo, worryLevel));
                }

                return throws;
            }

            public static long Evaluate(IReadOnlyList<Monkey> monkeys, int rounds, Func<long, long> relief)
            {
                var inspections = new int[monkeys.Count];

                for (var round = 0; round < rounds; round++)
                {
                    for (var m = 0; m < monkeys.Count; m++)
                    {
                        var monkey = monkeys[m];
                        var throws = Monkey.Evaluate(monkey, relief);

                        inspections[m] += monkey.Items.Count;

                        monkeys = monkeys
                            .Select((monkey, i) => i == m
                                ? monkey.WithoutItems()
                                : monkey.WithAdditionalItems(throws.Where(t => t.monkey == i).Select(t => t.item))
                            )
                            .ToList();
                    }
                }

                var topCounts = inspections
                    .Select((count, monkey) => (count, monkey))
                    .OrderByDescending(p => p.count)
                    .Take(2)
                    .ToList();

                return (long)topCounts[0].count * topCounts[1].count;
            }

            public Monkey WithoutItems() => 
                this with { Items = Array.Empty<long>() };

            public Monkey WithAdditionalItems(IEnumerable<long> items) => 
                this with { Items = Items.Concat(items).ToList() };
        }
    }
}
