using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2020
{
    static class Day14
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "mask = XXXXXXXXXXXXXXXXXXXXXXXXXXXXX1XXXX0X",
                "mem[8] = 11",
                "mem[7] = 101",
                "mem[8] = 0"
            );

        public static readonly IInput SampleInput2 =
            Input.Literal(
                "mask = 000000000000000000000000000000X1001X",
                "mem[42] = 100",
                "mask = 00000000000000000000000000000000X0XX",
                "mem[26] = 1"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/14/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var instructions = input.Lines().Select(Instruction.Parse).ToList();

                var mem = new Dictionary<long, long>();
                var mask = Mask.None;

                foreach (var instruction in instructions)
                {
                    if (instruction is Instruction.SetMask setMask) 
                    {
                        mask = setMask.Mask;
                    }
                    else if (instruction is Instruction.SetValue setValue)
                    {
                        mem[setValue.Address] = mask.Apply(setValue.Value);
                    }
                }

                var sum = mem.Values.Where(v => v != 0).Sum();
                Console.WriteLine(sum);
            }

            class Mask
            {
                public static Mask Parse(string text)
                {
                    var andMask = ParseAndMask(text);
                    var orMask = ParseOrMask(text);
                    return new Mask(andMask, orMask);
                }

                private static long ParseAndMask(string text)
                {
                    // "XXXXXXXXXXXXXXXXXXXXXXXXXXXXX1XXXX0X"
                    //  111111111111111111111111111111111101

                    var mask = long.MaxValue;

                    for (var i = 0; i < text.Length; i++)
                    {
                        var ch = text[text.Length - 1 - i];
                        if (ch == '0')
                        {
                            mask = mask & ~(1L << i);
                        }
                    }

                    return mask;
                }

                private static long ParseOrMask(string text)
                {
                    // "XXXXXXXXXXXXXXXXXXXXXXXXXXXXX1XXXX0X"
                    //  000000000000000000000000000001000000

                    var mask = 0L;

                    for (var i = 0; i < text.Length; i++)
                    {
                        var ch = text[text.Length - 1 - i];
                        if (ch == '1')
                        {
                            mask = mask | (1L << i);
                        }
                    }

                    return mask;
                }

                public static readonly Mask None = new Mask(andMask: long.MaxValue, orMask: 0L);

                public Mask(long andMask, long orMask)
                {
                    AndMask = andMask;
                    OrMask = orMask;
                }

                public long AndMask { get; }
                public long OrMask { get; }

                public long Apply(long value) => (value & AndMask) | OrMask;
            }

            abstract class Instruction 
            {
                public static Instruction Parse(string text)
                {
                    var (name, value) = SplitAssignment(text);
                    if (name == "mask")
                    {
                        return new SetMask(Mask.Parse(value));
                    }

                    var address = ParseAddress(name);
                    return new SetValue(address, long.Parse(value));
                }

                private static long ParseAddress(string name)
                {
                    var start = "mem[";
                    var end = "]";

                    var numberText = name.Substring(start.Length, name.Length - start.Length - end.Length);
                    return long.Parse(numberText);
                }

                private static (string, string) SplitAssignment(string text)
                {
                    var sep = " = ";
                    var index = text.IndexOf(sep);
                    return (text.Substring(0, index), text.Substring(index + sep.Length));
                }

                public class SetMask : Instruction
                {
                    public SetMask(Mask mask)
                    {
                        Mask = mask;
                    }

                    public Mask Mask { get; }
                }

                public class SetValue : Instruction
                {
                    public SetValue(long address, long value)
                    {
                        Address = address;
                        Value = value;
                    }

                    public long Address { get; }
                    public long Value { get; }
                }
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var instructions = input.Lines().Select(Instruction.Parse).ToList();

                var mem = new Dictionary<long, long>();
                var mask = Mask.None;

                foreach (var instruction in instructions)
                {
                    if (instruction is Instruction.SetMask setMask)
                    {
                        mask = setMask.Mask;
                    }
                    else if (instruction is Instruction.SetValue setValue)
                    {
                        foreach (var address in mask.Apply(setValue.Address))
                        {
                            mem[address] = setValue.Value;
                        }
                    }
                }

                var sum = mem.Values.Where(v => v != 0).Sum();
                Console.WriteLine(sum);
            }

            class Mask
            {
                public static Mask Parse(string text)
                {
                    return new Mask(text.PadLeft(64, '0'));
                }

                public static readonly Mask None = Parse(string.Empty);

                public Mask(string maskText)
                {
                    MaskText = maskText;
                }

                public string MaskText { get; }

                public IReadOnlyList<long> Apply(long value) 
                {
                    var list = new List<long>();
                    Collect(value, MaskText, 0, list);
                    return list;
                }

                private void Collect(long value, string mask, int bit, List<long> results)
                {
                    if (bit >= 64)
                    {
                        results.Add(value);
                        return;
                    }

                    var maskCh = mask[mask.Length - 1 - bit];

                    if (maskCh == '0')
                    {
                        Collect(value, mask, bit + 1, results);
                    }
                    else if (maskCh == '1')
                    {
                        Collect(SetBitTo1(value, bit), mask, bit + 1, results);
                    }
                    else
                    {
                        Collect(SetBitTo0(value, bit), mask, bit + 1, results);
                        Collect(SetBitTo1(value, bit), mask, bit + 1, results);
                    }
                }

                private long SetBitTo0(long value, int bit) => value & ~(1L << bit);

                private long SetBitTo1(long value, int bit) => value | (1L << bit);
            }

            abstract class Instruction
            {
                public static Instruction Parse(string text)
                {
                    var (name, value) = SplitAssignment(text);
                    if (name == "mask")
                    {
                        return new SetMask(Mask.Parse(value));
                    }

                    var address = ParseAddress(name);
                    return new SetValue(address, long.Parse(value));
                }

                private static long ParseAddress(string name)
                {
                    var start = "mem[";
                    var end = "]";

                    var numberText = name.Substring(start.Length, name.Length - start.Length - end.Length);
                    return long.Parse(numberText);
                }

                private static (string, string) SplitAssignment(string text)
                {
                    var sep = " = ";
                    var index = text.IndexOf(sep);
                    return (text.Substring(0, index), text.Substring(index + sep.Length));
                }

                public class SetMask : Instruction
                {
                    public SetMask(Mask mask)
                    {
                        Mask = mask;
                    }

                    public Mask Mask { get; }
                }

                public class SetValue : Instruction
                {
                    public SetValue(long address, long value)
                    {
                        Address = address;
                        Value = value;
                    }

                    public long Address { get; }
                    public long Value { get; }
                }
            }
        }

    }
}
