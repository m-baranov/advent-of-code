using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Computer = AdventOfCode2019.Day09.Computer;

namespace AdventOfCode2019
{
    static class Day21
    {
        public static readonly IInput SampleInput =
            Input.Literal();

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/21/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();

                var instructions = new[]
                {
                    // Jump if landing point (D) is hull, but before it (C) is an empty space.
                    // @ 
                    // #__.#
                    
                    "NOT C J",
                    "AND D J",

                    // Also jump  if the next point (A) is an empty space.
                    // @
                    // #.___

                    "NOT A T",
                    "OR T J",
                    
                    "WALK"
                };

                var damage = Simulation.Run(program, instructions, out var _);

                Console.WriteLine(damage);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();

                var instructions = new[]
                {
                    // DO NOT jump in the following scenarios:
                    //
                    // @
                    // #   #.  .
                    //  abcdefghi
                    // 
                    // @
                    // #   ##. ..
                    //  abcdefghi
                    //
                    // In both cases there is no way out.
                    //
                    // !(!e & !h) | !(e & !h & !i & !f)
                    // = (e | h) & (!e | h | i | f)

                    "NOT E J",
                    "NOT J T",
                    "OR H J",
                    "OR I J",
                    "OR F J",
                    "OR H T",
                    "AND T J",

                    // But, DO jump in the following case:
                    //
                    // @
                    // #???#       <- if a or b or c is space
                    //  abcdefghi
                    //
                    // (!a | !b | !c) & d
                    // = !(a & b & c) & d

                    "NOT A T",
                    "NOT T T",
                    "AND B T",
                    "AND C T",
                    "NOT T T",
                    "AND D T",
                    "AND T J",

                    "RUN"
                };

                var damage = Simulation.Run(program, instructions, out var _);

                Console.WriteLine(damage);
            }
        }

        private static class Simulation
        {
            public static void Repl(string program)
            {
                do
                {
                    var instructions = new List<string>();
                    var index = 1;

                    while (true)
                    {
                        Console.Write($"{index,2}: ");

                        var newInstructions = Console.ReadLine()
                            .Split(Environment.NewLine)
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToList();

                        instructions.AddRange(newInstructions);

                        if (newInstructions.Contains("EXIT"))
                        {
                            return;
                        }
                        if (newInstructions.Contains("WALK") || newInstructions.Contains("RUN"))
                        {
                            break;
                        }

                        index++;
                    }

                    Run(program, instructions, out var output);
                    foreach (var line in output)
                    {
                        Console.WriteLine(line);
                    }

                    Console.WriteLine();
                    Console.WriteLine("---------------------------------");
                    Console.WriteLine();
                } while (true);
            }

            public static long? Run(
                string program, 
                IReadOnlyList<string> instructions, 
                out IReadOnlyList<string> output)
            {
                var cpu = Computer.Of(program);
                cpu.Input.EnterAsciiLines(instructions);
                cpu.Execute();

                var damage = cpu.Output.Values().Last();

                if (damage < 255)
                {
                    output = cpu.Output.AsciiLines();
                    return null;
                }

                output = cpu.Output.AsciiLines()
                    .SkipLast(1)
                    .Append($"SUCCESS. Damage = {damage}.")
                    .ToArray();

                return damage;
            }
        }
    }
}
