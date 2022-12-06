using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day06
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "mjqjpqmgbljsphdztnvjfqwrcgsmlb",
                    "bvwbjplbgvbhsrlpgdmjqwftvncz",
                    "nppdvjthqldpwncqszvftbrmjlhg",
                    "nznrnfrfntjfmvfwmzdfjlvtqnbhcprsg",
                    "zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/6/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                foreach (var line in input.Lines())
                {
                    var count = Solve(line, targetCount: 4);
                    Console.WriteLine(count);
                }
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                foreach (var line in input.Lines())
                {
                    var count = Solve(line, targetCount: 14);
                    Console.WriteLine(count);
                }
            }
        }

        private static int Solve(string chars, int targetCount)
        {
            var buffer = new Buffer<char>(capacity: targetCount);

            for (var i = 0; i < chars.Length; i++)
            {
                buffer.Add(chars[i]);

                var uniqueCount = buffer.AsEnumerable().Distinct().Count();
                if (uniqueCount == targetCount)
                {
                    return i + 1;
                }
            }

            return -1;
        }

        private class Buffer<T>
        {
            private readonly T[] items;
            private int start;
            private int count;

            public Buffer(int capacity)
            {
                this.items = new T[capacity];
                this.start = 0;
                this.count = 0;
            }

            private int Capacity => this.items.Length;

            public void Add(T item)
            {
                if (this.count < this.Capacity)
                {
                    var index = (this.start + this.count) % this.Capacity;
                    this.items[index] = item;
                    this.count++;
                }
                else
                {
                    this.items[this.start] = item;
                    this.start = (this.start + 1) % this.Capacity;
                }
            }

            public IEnumerable<T> AsEnumerable()
            {
                for (var i = 0; i < this.count; i++)
                {
                    var index = (this.start + i) % this.Capacity;
                    yield return this.items[index];
                }
            }
        }
    }
}
