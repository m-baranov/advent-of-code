using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day06
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal("0 2 7 0");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/6/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var memory = Memory.Parse(input.Lines().First());

                var cycles = 0;
                var seen = new HashSet<Memory>();

                while (true)
                {
                    var nextMemory = memory.Redistribute();
                    cycles++;
                    
                    if (seen.Contains(nextMemory))
                    {
                        break;
                    }

                    seen.Add(nextMemory);

                    memory = nextMemory;
                }

                Console.WriteLine(cycles);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var memory = Memory.Parse(input.Lines().First());

                var seen = new HashSet<Memory>();
                while (true)
                {
                    var nextMemory = memory.Redistribute();

                    if (seen.Contains(nextMemory))
                    {
                        break;
                    }

                    seen.Add(nextMemory);

                    memory = nextMemory;
                }

                var memoryToFind = memory;

                var cycles = 0;
                while (true)
                {
                    var nextMemory = memory.Redistribute();
                    cycles++;

                    if (nextMemory.Equals(memoryToFind))
                    {
                        break;
                    }

                    memory = nextMemory;
                }

                Console.WriteLine(cycles);
            }
        }

        private class Memory
        {
            public static Memory Parse(string text)
            {
                var blocks = text
                    .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                return new Memory(blocks);
            }

            private readonly IReadOnlyList<int> blocks;

            public Memory(IReadOnlyList<int> blocks)
            {
                this.blocks = blocks;
            }

            public override bool Equals(object obj) =>
                obj is Memory other && Equals(other);

            public bool Equals(Memory other)
            {
                if (other.blocks.Count != this.blocks.Count)
                {
                    return false;
                }

                for (var i = 0; i < this.blocks.Count; i++)
                {
                    if (other.blocks[i] != this.blocks[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            public override int GetHashCode()
            {
                if (blocks.Count == 0)
                {
                    return 0;
                }

                var hash = HashCode.Combine(this.blocks[0]);
                for (var i = 1; i < this.blocks.Count; i++)
                {
                    hash = HashCode.Combine(hash, this.blocks[i]);
                }

                return hash;
            }

            public Memory Redistribute()
            {
                static int MaxIndex(IReadOnlyList<int> blocks)
                {
                    var imax = 0;
                    for (var i = 1; i < blocks.Count; i++)
                    {
                        if (blocks[imax] < blocks[i])
                        {
                            imax = i;
                        }
                    }
                    return imax;
                }

                static void Redistribute(int[] blocks, int index)
                {
                    var blocksToRedistribute = blocks[index];
                    blocks[index] = 0;

                    while (blocksToRedistribute > 0)
                    {
                        var nextIndex = (index + 1) % blocks.Length;
                        blocks[nextIndex]++;

                        blocksToRedistribute--;

                        index = nextIndex;
                    }
                }

                var blocks = this.blocks.ToArray();

                var maxIndex = MaxIndex(blocks);
                Redistribute(blocks, maxIndex);

                return new Memory(blocks);
            }
        }
    }
}
