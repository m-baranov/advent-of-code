using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day21
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "root: pppw + sjmn",
                    "dbpl: 5",
                    "cczh: sllz + lgvd",
                    "zczc: 2",
                    "ptdq: humn - dvpt",
                    "dvpt: 3",
                    "lfqf: 4",
                    "humn: 5",
                    "ljgn: 2",
                    "sjmn: drzm * dbpl",
                    "sllz: 4",
                    "pppw: cczh / lfqf",
                    "lgvd: ljgn * ptdq",
                    "drzm: hmdt - zczc",
                    "hmdt: 32"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/21/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var jobs = Jobs.Parse(input.Lines());

                var result = TryEval(jobs.Get("root"), jobs);
                Console.WriteLine(result);
            }

            private static long Eval(string monkey, Jobs jobs)
            {
                var memo = new Dictionary<string, long>();

                long EvalBinary(Expression.BinaryOperation binary)
                {
                    var left = (binary.Left as Expression.Monkey).Name;
                    var right = (binary.Right as Expression.Monkey).Name; 

                    return binary.Operator switch
                    {
                        '+' => EvalMemo(left) + EvalMemo(right),
                        '-' => EvalMemo(left) - EvalMemo(right),
                        '*' => EvalMemo(left) * EvalMemo(right),
                        '/' => EvalMemo(left) / EvalMemo(right),

                        _ => throw new Exception($"Unknown operator '{binary.Operator}'.")
                    };
                }

                long Eval(string monkey) =>
                    jobs.Get(monkey) switch
                    {
                        Expression.Literal literal => literal.Value,

                        Expression.BinaryOperation binary => EvalBinary(binary),

                        var expr => throw new Exception($"Unknown expression type '{expr.GetType().Name}'.")
                    };

                long EvalMemo(string monkey)
                {
                    if (memo.TryGetValue(monkey, out var cached))
                    {
                        return cached;
                    }

                    var value = Eval(monkey);
                    
                    memo[monkey] = value;
                    return value;
                }

                return EvalMemo(monkey);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var jobs = Jobs.Parse(input.Lines(), withHuman: true);

                var root = jobs.Get("root") as Expression.BinaryOperation;

                var left = TryEval(root.Left, jobs);
                var right = TryEval(root.Right, jobs);

                var answer = left == null 
                    ? Solve(root.Left, right.Value, jobs) 
                    : Solve(root.Right, left.Value, jobs);

                Console.WriteLine(answer);
            }

            private long Solve(Expression expression, long equalsTo, Jobs jobs)
            {
                long SolveBinary(Expression.BinaryOperation binary, long equalsTo)
                {
                    var left = TryEval(binary.Left, jobs);
                    var right = TryEval(binary.Right, jobs);

                    return (left, binary.Operator, right) switch
                    {
                        (null, '+', var val) => Solve(binary.Left, equalsTo - val.Value),
                        (var val, '+', null) => Solve(binary.Right, equalsTo - val.Value),

                        (null, '-', var val) => Solve(binary.Left, equalsTo + val.Value),
                        (var val, '-', null) => Solve(binary.Right, val.Value - equalsTo),

                        (null, '*', var val) => Solve(binary.Left, equalsTo / val.Value),
                        (var val, '*', null) => Solve(binary.Right, equalsTo / val.Value),

                        (null, '/', var val) => Solve(binary.Left, equalsTo * val.Value),
                        (var val, '/', null) => Solve(binary.Right, val.Value / equalsTo),

                        _ => throw new Exception("Should not be here"),
                    };
                }

                long Solve(Expression expression, long equalsTo) =>
                    expression switch
                    {
                        Expression.Human => equalsTo,
                        Expression.BinaryOperation binary => SolveBinary(binary, equalsTo),
                        Expression.Monkey monkey => Solve(jobs.Get(monkey.Name), equalsTo),

                        Expression.Literal => throw new Exception("Should not be here"),
                        var expr => throw new Exception($"Unknown expression type '{expr.GetType().Name}'.")
                    };

                return Solve(expression, equalsTo);
            }
        }

        private static long? TryEval(Expression expression, Jobs jobs)
        {
            var memo = new Dictionary<string, long?>();

            static long? Apply(long? x, long? y, Func<long, long, long> op) =>
                x == null || y == null ? null : op(x.Value, y.Value);

            long? EvalBinary(Expression.BinaryOperation binary) =>
                binary.Operator switch
                {
                    '+' => Apply(Eval(binary.Left), Eval(binary.Right), static (x, y) => x + y),
                    '-' => Apply(Eval(binary.Left), Eval(binary.Right), static (x, y) => x - y),
                    '*' => Apply(Eval(binary.Left), Eval(binary.Right), static (x, y) => x * y),
                    '/' => Apply(Eval(binary.Left), Eval(binary.Right), static (x, y) => x / y),

                    _ => throw new Exception($"Unknown operator '{binary.Operator}'.")
                };

            long? EvalMonkey(Expression.Monkey monkey)
            {
                if (memo.TryGetValue(monkey.Name, out var cached))
                {
                    return cached;
                }

                var expr = jobs.Get(monkey.Name);
                var value = Eval(expr);

                memo[monkey.Name] = value;
                return value;
            }

            long? Eval(Expression expression) =>
                expression switch
                {
                    Expression.Literal literal => literal.Value,
                    Expression.Human => null,
                    Expression.Monkey monkey => EvalMonkey(monkey),
                    Expression.BinaryOperation binary => EvalBinary(binary),

                    var expr => throw new Exception($"Unknown expression type '{expr.GetType().Name}'.")
                };

            return Eval(expression);
        }

        private sealed class Jobs
        {
            public static Jobs Parse(IEnumerable<string> lines, bool withHuman = false)
            {
                static (string monkey, Expression job) ParseOne(string text, bool withHuman)
                {
                    var parts = text.Split(": ");
                    
                    var monkey = parts[0];
                    var job = withHuman && monkey == "humn" 
                        ? Expression.Human.Instance
                        : Expression.Parse(parts[1]);

                    return (monkey, job);
                }

                var jobByMonkey = lines
                    .Select(l => ParseOne(l, withHuman))
                    .ToDictionary(p => p.monkey, p => p.job);

                return new Jobs(jobByMonkey);
            }

            private readonly IReadOnlyDictionary<string, Expression> jobByMonkey;

            public Jobs(IReadOnlyDictionary<string, Expression> jobByMonkey)
            {
                this.jobByMonkey = jobByMonkey;
            }

            public Expression Get(string monkey) => this.jobByMonkey[monkey];
        }

        private abstract class Expression
        {
            public static Expression Parse(string text)
            {
                if (long.TryParse(text, out var number))
                {
                    return new Literal(number);
                }
                else
                {
                    var parts = text.Split(' ');

                    var left = new Monkey(parts[0]);
                    var op = parts[1][0];
                    var right = new Monkey(parts[2]);
                    
                    return new BinaryOperation(left, right, op);
                }
            }

            public sealed class Literal : Expression
            {
                public Literal(long value)
                {
                    Value = value;
                }

                public long Value { get; }
            }

            public sealed class Monkey : Expression
            {
                public Monkey(string name)
                {
                    Name = name;
                }

                public string Name { get; }
            }

            public sealed class Human : Expression
            {
                public static readonly Expression Instance = new Human();
                private Human() { }
            }

            public sealed class BinaryOperation : Expression
            {
                public BinaryOperation(Expression left, Expression right, char @operator)
                {
                    Left = left;
                    Right = right;
                    Operator = @operator;
                }

                public Expression Left { get; }
                public Expression Right { get; }
                public char Operator { get; }
            }
        }
    }
}
