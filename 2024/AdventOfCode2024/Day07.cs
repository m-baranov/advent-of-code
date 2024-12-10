using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2024;

static class Day07
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
190: 10 19
3267: 81 40 27
83: 17 5
156: 15 6
7290: 6 8 6 15
161011: 16 10 13
192: 17 8 14
21037: 9 7 18 13
292: 11 6 16 20
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/7/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var equations = input.Lines()
                .Select(Equation.Parse)
                .ToArray();

            var sum = equations
                .Where(TryEval)
                .Select(e => e.Result)
                .Sum();

            Console.WriteLine(sum);
        }

        private static bool TryEval(Equation equation)
        {
            static bool Recurse(Equation equation, long result, int index)
            {
                if (index >= equation.Operands.Count)
                {
                    return equation.Result == result;
                }

                if (equation.Result < result)
                {
                    return false;
                }

                var sumResult = result + equation.Operands[index];
                if (Recurse(equation, sumResult, index + 1))
                {
                    return true;
                }

                var mulResult = result * equation.Operands[index];
                if (Recurse(equation, mulResult, index + 1))
                {
                    return true;
                }

                return false;
            }

            return Recurse(equation, equation.Operands[0], index: 1);
        }
    }

    public class Part2 : IProblem
    {
         public void Run(TextReader input)
        {
            var equations = input.Lines()
                .Select(Equation.Parse)
                .ToArray();

            var sum = equations
                .Where(TryEval)
                .Select(e => e.Result)
                .Sum();

            Console.WriteLine(sum);
        }

        private static bool TryEval(Equation equation)
        {
            static long Concat(long x, long y)
            {
                var mul = 1;

                var r = y;
                while (r > 0)
                {
                    r /= 10;
                    mul *= 10;
                }

                return x * mul + y;
            }

            static bool Recurse(Equation equation, long result, int index)
            {
                if (index >= equation.Operands.Count)
                {
                    return equation.Result == result;
                }

                if (equation.Result < result)
                {
                    return false;
                }

                var sumResult = result + equation.Operands[index];
                if (Recurse(equation, sumResult, index + 1))
                {
                    return true;
                }

                var mulResult = result * equation.Operands[index];
                if (Recurse(equation, mulResult, index + 1))
                {
                    return true;
                }

                var concatResult = Concat(result, equation.Operands[index]);
                if (Recurse(equation, concatResult, index + 1))
                {
                    return true;
                }

                return false;
            }

            return Recurse(equation, equation.Operands[0], index: 1);
        }
    }

    private record Equation(long Result, IReadOnlyList<long> Operands)
    {
        public static Equation Parse(string text)
        {
            var parts = text.Split(": ");

            var result = long.Parse(parts[0]);
            var operands = parts[1].Split(' ').Select(long.Parse).ToArray();

            return new Equation(result, operands);
        }
    }
}
