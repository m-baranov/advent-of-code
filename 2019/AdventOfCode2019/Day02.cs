using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day02
    {
        public static readonly IInput SampleInput =
            Input.Literal();

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/2/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var memory = input.Lines().First().Split(',').Select(long.Parse).ToArray();

                memory[1] = 12;
                memory[2] = 2;

                Computer.Run(memory);

                Console.WriteLine(memory[0]);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var memory = input.Lines().First().Split(',').Select(long.Parse).ToArray();

                for (long noun = 0; noun <= 99; noun++)
                {
                    for (long verb = 0; verb <= 99; verb++)
                    {
                        var result = RunOnce(memory, noun, verb);
                        if (result == 19690720)
                        {
                            Console.WriteLine(100 * noun + verb);
                            return;
                        }
                    }
                }
            }

            private long RunOnce(long[] initialMemory, long noun, long verb)
            {
                var memory = new long[initialMemory.Length];
                initialMemory.CopyTo(memory, index: 0);

                memory[1] = noun;
                memory[2] = verb;

                Computer.Run(memory);

                return memory[0];
            }
        }

        private static class Computer
        {
            public static void Run(long[] memory)
            {
                var ip = 0;
                while (true)
                {
                    var opcode = memory[ip];

                    if (opcode == 1 || opcode == 2)
                    {
                        var arg1Addr = memory[ip + 1];
                        var arg2Addr = memory[ip + 2];
                        var resultAddr = memory[ip + 3];

                        var arg1 = memory[arg1Addr];
                        var arg2 = memory[arg2Addr];

                        memory[resultAddr] = opcode == 1 ? arg1 + arg2 : arg1 * arg2;

                        ip += 4;
                    }
                    else if (opcode == 99)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"ERROR: unknown op '{opcode}'.");
                        break;
                    }
                }
            }
        }
    }
}
