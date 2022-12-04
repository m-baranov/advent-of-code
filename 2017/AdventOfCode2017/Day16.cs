using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day16
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal("s1,x3/4,pe/b");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/16/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var moves = Move.ParseMany(input.Lines().First());

                var programs = new Programs(count: 16);
                programs.ApplyMany(moves);

                Console.WriteLine(programs);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                const int N = 1_000_000_000;

                var moves = Move.ParseMany(input.Lines().First());

                var programs = new Programs(count: 16);

                var seen = new List<string>();

                while (true)
                {
                    programs.ApplyMany(moves);

                    var program = programs.ToString();
                    if (seen.Contains(program))
                    {
                        break;
                    }

                    seen.Add(program);
                }

                var result = seen[N % seen.Count - 1];
                Console.WriteLine(result);
            }
        }

        private abstract class Move
        {
            public static IReadOnlyList<Move> ParseMany(string text) =>
                text.Split(',').Select(Parse).ToList();

            public static Move Parse(string text)
            {
                static (char first, string rest) Head(string text) =>
                    (text[0], text.Substring(1));

                static (string left, string right) Split(string text, char by) 
                {
                    var index = text.IndexOf(by);
                    return (text.Substring(0, index), text.Substring(index + 1));
                }

                var (move, args) = Head(text);

                if (move == 's')
                {
                    var value = int.Parse(args);
                    return new Move.Spin(value);
                }
                else
                {
                    var (left, right) = Split(args, '/');
                    if (move == 'x')
                    {
                        var from = int.Parse(left);
                        var to = int.Parse(right);
                        return new Move.Exchange(from, to);
                    }
                    else
                    {
                        Debug.Assert(move == 'p');
                        return new Move.Partner(left[0], right[0]);
                    }
                }
            }

            private Move() { }

            public sealed class Spin : Move
            {
                public Spin(int value)
                {
                    Value = value;
                }

                public int Value { get; }
            }

            public sealed class Exchange : Move
            {
                public Exchange(int from, int to)
                {
                    From = from;
                    To = to;
                }

                public int From { get; }
                public int To { get; }
            }

            public sealed class Partner : Move
            {
                public Partner(char from, char to)
                {
                    From = from;
                    To = to;
                }

                public char From { get; }
                public char To { get; }
            }
        }

        private class Programs
        {
            private readonly char[] values;
            private int offset;

            public Programs(int count)
            {
                this.values = Enumerable.Range(0, count).Select(i => (char)('a' + i)).ToArray();
                this.offset = 0;
            }

            public int Count => this.values.Length;

            public override string ToString()
            {
                var chars = Enumerable.Range(0, this.Count)
                    .Select(VirtualIndexToActual)
                    .Select(i => this.values[i]);

                return string.Join("", chars);
            }

            public void ApplyMany(IReadOnlyList<Move> moves)
            {
                foreach (var move in moves)
                {
                    Apply(move);
                }
            }

            public void Apply(Move move)
            {
                switch (move)
                {
                    case Move.Spin spin:
                        ApplySpin(spin);
                        break;

                    case Move.Exchange exchange:
                        ApplyExchange(exchange);
                        break;

                    case Move.Partner partner:
                        ApplyPartner(partner);
                        break;
                }
            }

            private void ApplySpin(Move.Spin spin)
            {
                this.offset = (this.offset + spin.Value) % this.Count;
            }

            private void ApplyExchange(Move.Exchange exchange)
            {
                var from = VirtualIndexToActual(exchange.From);
                var to = VirtualIndexToActual(exchange.To);
                
                Swap(from, to);
            }

            private void ApplyPartner(Move.Partner partner)
            {
                var from = this.values.IndexOf(ch => ch == partner.From);
                var to = this.values.IndexOf(ch => ch == partner.To);

                Swap(from, to);
            }

            private void Swap(int from, int to)
            {
                (this.values[to], this.values[from]) = (this.values[from], this.values[to]);
            }

            private int VirtualIndexToActual(int index)
            {
                var actual = index - this.offset;
                return actual < 0 ? actual + this.Count : actual;
            }
        }
    }
}
