using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace AdventOfCode2019
{
    static class Day22
    {
        public static readonly IInput Sample1Input =
            Input.Literal(
                "deal with increment 7",
                "deal into new stack",
                "deal into new stack"
            );

        public static readonly IInput Sample2Input =
            Input.Literal(
                "cut 6",
                "deal with increment 7",
                "deal into new stack"
            );

        public static readonly IInput Sample3Input =
            Input.Literal(
                "deal with increment 7",
                "deal with increment 9",
                "cut -2"
            );

        public static readonly IInput Sample4Input =
            Input.Literal(
                "deal into new stack",
                "cut -2",
                "deal with increment 7",
                "cut 8",
                "cut -4",
                "deal with increment 7",
                "cut 3",
                "deal with increment 9",
                "deal with increment 3",
                "cut -1"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/22/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                const long N = 10007;

                var techniques = Technique.ParseMany(input.Lines());

                var stack = new Stack(N, techniques);

                var i = 0;
                while (i < stack.Count && stack.At(i) != 2019)
                {
                    i++;
                }

                Console.WriteLine(i);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                // That was fun!

                const long N = 119_315_717_514_047;
                const long R = 101_741_582_076_661;

                var techniques = Technique.ParseMany(input.Lines());

                var simplifiedTechniques = Technique.Simplify(techniques, N);
                Display("Simplified: ", simplifiedTechniques);

                var inc = (IncrementTechnique)simplifiedTechniques[0];
                var cut = (CutLeftTechnique)simplifiedTechniques[1];
                var repeatedTechniques = Technique.Repeat(inc, cut, N, R);
                Display("Repeated: ", repeatedTechniques);

                var stack = new Stack(N, repeatedTechniques);
                Console.WriteLine(stack.At(2020));
            }

            private static void Display(string title, IReadOnlyList<ITechnique> simplified)
            {
                Console.WriteLine(title);

                var i = 0;
                foreach (var t in simplified)
                {
                    Console.WriteLine($"{i,3}: {t}");
                    i++;
                }

                Console.WriteLine();
            }
        }

        private class Stack
        {
            private readonly IReadOnlyList<ITechnique> techniques;

            public Stack(long count, IReadOnlyList<ITechnique> techniques)
            {
                Count = count;
                this.techniques = techniques;
            }

            public long Count { get; }

            public long At(long index)
            {
                for (var i = techniques.Count - 1; i >= 0; i--)
                {
                    index = techniques[i].IndexAt(Count, index);
                }
                return index;
            }

            public override string ToString() =>
                string.Join(",", Enumerable.Range(0, Math.Min(100, (int)Count)).Select(i => At(i)));
        }

        private interface ITechnique
        {
            long IndexAt(long count, long index);
        }

        private static class Technique
        {
            public static IReadOnlyList<ITechnique> ParseMany(IEnumerable<string> lines) =>
               lines.Select(Parse).ToList();

            public static ITechnique Parse(string text)
            {
                if (text == "deal into new stack")
                {
                    return ReverseTechnique.Instance;
                }

                var index = text.LastIndexOf(' ');

                var valueText = text.Substring(index + 1);
                var value = int.Parse(valueText);

                var instructionText = text.Substring(0, index);

                if (instructionText == "deal with increment")
                {
                    return new IncrementTechnique(value);
                }

                if (value >= 0)
                {
                    return new CutLeftTechnique(value);
                }
                else
                {
                    return new CutRightTechnique(-value);
                }
            }

            public static IReadOnlyList<ITechnique> Repeat(IncrementTechnique inc, CutLeftTechnique cut, long N, long R)
            {
                // Assuming R = 4:
                //
                // inc X + cut Y + inc X + cut Y + inc X + cut Y + inc X + cut Y
                // 
                // under cut a + inc b = inc b + cut((a * b) % N)
                //
                // inc X + inc X + cut (X * Y % N) + inc X + cut (X * Y % N) + inc X + cut (X * Y % N) + cut Y
                //
                // inc X + inc X + inc X + cut (X^2 * Y % N) + inc X + cut (X^2 * Y % N) + cut (X * Y % N) + cut Y
                //
                // inc X + inc X + inc X + inc X + cut (X^3 * Y % N) + cut (X^2 * Y % N) + cut (X * Y % N) + cut Y

                // So: 
                // inc(X^R % N) + cut( Sum [Y * X^i % N], i = 0..R-1 )

                // Then: 
                // Y * X^i, i = 0..R-1  is a https://en.wikipedia.org/wiki/Geometric_series
                // 
                // Sum [Y * X^i % N], i = 0..R-1 = Y * (X^N - 1) * (X - 1)

                // Finally:
                // inc(X^R % N) + cut(Y * (X^N - 1) * (X - 1) % N)

                var incRep = BigInteger.ModPow(inc.Value, R, N);
                var cutRep = (cut.Value % N * (incRep - 1) % N * ModMath.Inverse(inc.Value - 1, N)) % N;

                return new ITechnique[]
                {
                    new IncrementTechnique((long)incRep),
                    new CutLeftTechnique((long)cutRep)
                };
            }

            public static IReadOnlyList<ITechnique> Simplify(IReadOnlyList<ITechnique> techniques, long N)
            {
                return Combine(Normalize(techniques, N), N);
            }

            private static IReadOnlyList<ITechnique> Normalize(IReadOnlyList<ITechnique> techniques, long N)
            {
                var normalize = new List<ITechnique>();

                foreach (var technique in techniques)
                {
                    var (n1, n2) = Normalize(technique, N);

                    normalize.Add(n1);
                    if (n2 != null)
                    {
                        normalize.Add(n2);
                    }
                }

                return normalize;
            }

            private static (ITechnique, ITechnique) Normalize(ITechnique technique, long N)
            {
                // 1) rev = inc(N-1) + cut 1
                // 2) cut(-a) = cut(N-a)

                return technique switch
                {
                    ReverseTechnique =>
                        (new IncrementTechnique(N - 1), new CutLeftTechnique(1)),

                    CutRightTechnique cr =>
                        (new CutLeftTechnique(N - cr.Value), null),

                    _ => 
                        (technique, null)
                };
            }

            private static IReadOnlyList<ITechnique> Combine(IReadOnlyList<ITechnique> techniques, long N)
            {
                var combined = techniques.ToList();

                var madeProgress = true;

                while (madeProgress)
                {
                    madeProgress = false;

                    var i = 0;
                    while (i < combined.Count - 1)
                    {
                        var t1 = combined[i];
                        var t2 = combined[i + 1];

                        var (s1, s2) = Combine(t1, t2, N);
                        if (s1 != null)
                        {
                            combined[i] = s1;

                            if (s2 != null)
                            {
                                combined[i + 1] = s2;
                            }
                            else
                            {
                                combined.RemoveAt(i + 1);
                            }
                            madeProgress = true;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }

                return combined;
            }

            private static (ITechnique, ITechnique) Combine(ITechnique t1, ITechnique t2, long N)
            {
                // 1) inc a + inc b = inc((a * b) % N)
                // 2) cut a + cut b = cut((a + b) % N)
                // 3) cut a + inc b = inc b + cut((a * b) % N)

                return (t1, t2) switch
                {
                    (IncrementTechnique i1, IncrementTechnique i2) =>
                        (new IncrementTechnique(ModMath.Mul(i1.Value, i2.Value, N)), null),

                    (CutLeftTechnique c1, CutLeftTechnique c2) =>
                        (new CutLeftTechnique(ModMath.Add(c1.Value, c2.Value, N)), null),

                    (CutLeftTechnique c, IncrementTechnique i) =>
                        (i, new CutLeftTechnique(i.InveseIndexAt(N, c.Value))),

                    _ => 
                        (null, null)
                };
            }
        }

        private class ReverseTechnique : ITechnique
        {
            public static readonly ReverseTechnique Instance = new ReverseTechnique();

            private ReverseTechnique() { }

            public override string ToString() => $"rev";

            public long IndexAt(long count, long index) => count - index - 1;
        }

        private class CutLeftTechnique : ITechnique
        {
            public CutLeftTechnique(long value)
            {
                this.Value = value;
            }

            public long Value { get; }

            public override string ToString() => $"cut {Value}";

            public long IndexAt(long count, long index) =>
                index < count - Value ? index + Value : index - count + Value;
        }

        private class CutRightTechnique : ITechnique
        {
            public CutRightTechnique(long value)
            {
                this.Value = value;
            }

            public long Value { get; }

            public override string ToString() => $"cut -{Value}";

            public long IndexAt(long count, long index) =>
                index < Value ? index + count - Value : index - Value;
        }

        private class IncrementTechnique : ITechnique
        {
            public IncrementTechnique(long value)
            {
                this.Value = value;
            }
            
            public long Value { get; }

            public override string ToString() => $"inc {Value}";

            public long IndexAt(long count, long index) => 
                ModMath.Mul(ModMath.Inverse(Value, count), index, count);

            public long InveseIndexAt(long count, long index) => 
                ModMath.Mul(Value, index, count);
        }

        private static class ModMath
        {
            // (a * b) mod n
            // NOTE: convert to big integer here, because a * b overflows on test input
            public static long Mul(long a, long b, long n) => 
                (long)(((BigInteger)a * b) % n);

            // (a + b) mod n
            public static long Add(long a, long b, long n) =>
                ((a % n) + (b % n)) % n;

            // a^-1 = t, such that (a * t) mod n = 1
            // https://en.wikipedia.org/wiki/Extended_Euclidean_algorithm
            public static long Inverse(long a, long n)
            {
                long s = 0;
                long old_s = 1;
                long r = n;
                long old_r = a;

                while (r != 0)
                {
                    long q = old_r / r;
                    (old_r, r) = (r, old_r - q * r);
                    (old_s, s) = (s, old_s - q * s);
                }

                return old_s < 0 ? old_s + n : old_s;
            }
        }
    }
}
