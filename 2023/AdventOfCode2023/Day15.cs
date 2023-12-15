using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day15
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
rn=1,cm-,qp=3,cm=2,qp-,pc=4,ot=9,ab=5,pc-,pc=6,ot=7
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/15/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var line = input.Lines().First();

            var sum = line.Split(',').Select(Hash).Sum();

            Console.WriteLine(sum);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var operations = Operation.ParseMany(input.Lines().First());

            var boxes = new Boxes();
            
            foreach (var operation in operations)
            {
                if (operation is Operation.Add add)
                {
                    boxes.Add(new Lens(add.Label, add.FocalLength));
                }
                else if (operation is Operation.Remove remove)
                {
                    boxes.Remove(remove.Label);
                }
            }

            var sum = boxes
                .AsEnumerable()
                .SelectMany((lenses, boxIndex) =>
                    lenses.Select((lens, lensIndex) => (boxIndex, lensIndex, lens)))
                .Select(p => (p.boxIndex + 1) * (p.lensIndex + 1) * p.lens.FocalLength)
                .Sum();

            Console.WriteLine(sum);
        }
    }

    private static int Hash(string text) =>
        text.Aggregate(0, (hash, ch) => (hash + (byte)ch) * 17 % 256);

    private abstract record Operation
    {
        public static IReadOnlyList<Operation> ParseMany(string text) =>
            text.Split(',').Select(Parse).ToList();

        public static Operation Parse(string text)
        {
            var index = text.IndexOf('-');
            if (index >= 0)
            {
                var label = text.Substring(0, index);

                return new Operation.Remove(label);
            }

            index = text.IndexOf('=');
            if (index >= 0)
            {
                var label = text.Substring(0, index);
                var length = int.Parse(text.Substring(index + 1));
                
                return new Operation.Add(label, length);
            }

            throw new Exception("impossible");
        }

        public sealed record Remove(string Label) : Operation;

        public sealed record Add(string Label, int FocalLength) : Operation;
    }

    public record Lens(string Label, int FocalLength);

    private sealed class Boxes
    {
        private readonly Box[] boxes;

        public Boxes()
        {
            this.boxes = new Box[256];

            for (var i = 0; i < this.boxes.Length; i++)
            {
                this.boxes[i] = new Box();
            }
        }

        public IEnumerable<IEnumerable<Lens>> AsEnumerable() =>
            this.boxes.Select(box => box.AsEnumerable());

        public void Add(Lens lens)
        {
            var box = Hash(lens.Label);
            this.boxes[box].Add(lens);
        }

        public void Remove(string label)
        {
            var box = Hash(label);
            this.boxes[box].Remove(label);
        }

        private sealed class Box
        {
            private readonly List<Lens> lenses = new();

            public IEnumerable<Lens> AsEnumerable() => this.lenses;

            public void Add(Lens lens)
            {
                var index = this.lenses.IndexOf(l => l.Label == lens.Label);
                if (index >= 0)
                {
                    this.lenses[index] = lens;
                }
                else
                {
                    this.lenses.Add(lens);
                }
            }

            public void Remove(string label)
            {
                var index = this.lenses.IndexOf(l => l.Label == label);
                if (index >= 0)
                {
                    this.lenses.RemoveAt(index);
                }
            }
        }
    }
}
