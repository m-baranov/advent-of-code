using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdventOfCode2020
{
    static class Day18
    {
        // part 1: 71 + 26 + 437 + 12240 + 13632 = 26406
        // part 2: 231 + 46 + 1445 + 669060 + 23340 = 694122
        public static readonly IInput SampleInput =
            Input.Literal(
                "1 + 2 * 3 + 4 * 5 + 6",
                "2 * 3 + (4 * 5)",
                "5 + (8 * 3 + 9 + 3 * 4 * 3)",
                "5 * 9 * (7 * 3 * 3 + 9 * 3 + (8 + 6 * 4))",
                "((2 + 4 * 9) * (6 + 9 * 8 + 6) + 6) + 2 + 4 * 2"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/18/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var expressions = input.Lines().Select(Expression.Parse).ToList();

                var solution = expressions.Select(Eval).Sum();

                Console.WriteLine(solution);
            }

            private long Eval(Expression expr)
            {
                var result = Eval(expr.Operands[0]);

                for (var i = 0; i < expr.Operations.Count; i++)
                {
                    var op = Eval(expr.Operands[i + 1]);

                    if (expr.Operations[i] == Operation.Add)
                    {
                        result = result + op;
                    }
                    else
                    {
                        result = result * op;
                    }
                }

                return result;
            }

            private long Eval(Operand operand)
            {
                if (operand is Operand.Literal literal)
                {
                    return literal.Value;
                }

                if (operand is Operand.Subexpression subexpr)
                {
                    return Eval(subexpr.Expression);
                }

                throw new NotSupportedException();
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var expressions = input.Lines().Select(Expression.Parse).ToList();

                var solution = expressions.Select(Eval).Sum();

                Console.WriteLine(solution);
            }

            private long Eval(Expression expr)
            {
                var sum = Eval(expr.Operands[0]);
                var muls = new List<long>();

                for (var i = 0; i < expr.Operations.Count; i++)
                {
                    var op = Eval(expr.Operands[i + 1]);

                    if (expr.Operations[i] == Operation.Add)
                    {
                        sum += op;
                    }
                    else
                    {
                        muls.Add(sum);
                        sum = op;
                    }
                }
                muls.Add(sum);

                return muls.Aggregate((acc, n) => acc * n);
            }

            private long Eval(Operand operand)
            {
                if (operand is Operand.Literal literal)
                {
                    return literal.Value;
                }

                if (operand is Operand.Subexpression subexpr)
                {
                    return Eval(subexpr.Expression);
                }

                throw new NotSupportedException();
            }
        }

        abstract class Operand
        {
            public class Literal : Operand
            {
                public Literal(int value)
                {
                    Value = value;
                }

                public int Value { get; }

                public override string ToString()
                {
                    return Value.ToString();
                }
            }

            public class Subexpression : Operand
            {
                public Subexpression(Expression expression)
                {
                    Expression = expression;
                }

                public Expression Expression { get; }

                public override string ToString()
                {
                    return $"({Expression})";
                }
            }
        }

        enum Operation { Add, Mul }

        class Expression
        {
            public static Expression Parse(string text)
            {
                return Parse(text, 0, text.Length);
            }

            private static Expression Parse(string text, int start, int length)
            {
                var operands = new List<Operand>();
                var operations = new List<Operation>();

                var i = 0;
                while (i < length)
                {
                    var index = start + i;
                    var ch = text[index];

                    if (ch == '(')
                    {
                        var end = SkipSubexpression(text, index + 1);
                        var exprLength = end - index; 

                        var expression = Parse(text, index + 1, exprLength - 2); // +1/-2 to remove brackets

                        var operand = new Operand.Subexpression(expression);
                        operands.Add(operand);

                        i += exprLength;
                    }
                    else if (ch == ' ')
                    {
                        var operation = text[index + 1] == '+' ? Operation.Add : Operation.Mul;
                        operations.Add(operation);
                        
                        i += 3;
                    }
                    else
                    {
                        var end = SkipDigits(text, index);
                        var numberText = text.Substring(index, end - index);
                        var number = int.Parse(numberText);
                        
                        var operand = new Operand.Literal(number);
                        operands.Add(operand);

                        i += numberText.Length;
                    }
                }

                return new Expression(operands, operations);
            }

            private static int SkipSubexpression(string text, int index)
            {
                var bracketCount = 1;
                while (index < text.Length && bracketCount > 0)
                {
                    if (text[index] == '(') 
                    { 
                        bracketCount++; 
                    } 
                    else if (text[index] == ')')
                    {
                        bracketCount--;
                    }

                    index++;
                }

                return index;
            }

            private static int SkipDigits(string text, int index)
            {
                while (index < text.Length && char.IsDigit(text[index]))
                {
                    index++;
                }

                return index;
            }

            public Expression(
                IReadOnlyList<Operand> operands,
                IReadOnlyList<Operation> operations)
            {
                Operands = operands;
                Operations = operations;
            }

            public IReadOnlyList<Operand> Operands { get; }
            public IReadOnlyList<Operation> Operations { get; }

            public override string ToString()
            {
                var builder = new StringBuilder();

                for (var i = 0; i < Operations.Count; i++)
                {
                    builder.Append(Operands[i]);
                    builder.Append(Operations[i] == Operation.Add ? " + " : " * ");
                }

                builder.Append(Operands[Operations.Count]);

                return builder.ToString();
            }
        }
    }
}
