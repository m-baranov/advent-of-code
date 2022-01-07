using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Computer = AdventOfCode2019.Day09.Computer;

namespace AdventOfCode2019
{
    static class Day25
    {
        public static readonly IInput SampleInput =
            Input.Literal();

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/25/input");

        public class Part1 : IProblem
        {
            // 1. Implement REPL
            // 2. Start the game
            // 3. Draw map of all locations and items
            // 4. Try to pickup every item.

            private static readonly IReadOnlyList<string> CollectAllAndGoToCheckpointCommands = 
                new[]
                {
                    "west",
                    "take fixed point",
                    "north",
                    "take sand",
                    "south",
                    "east",
                    "east",
                    "take asterisk",
                    "north",
                    "north",
                    "take hypercube",
                    "north",
                    "take coin",
                    "north",
                    "take easter egg",
                    "south",
                    "south",
                    "south",
                    "west",
                    "north",
                    "take spool of cat6",
                    "north",
                    "take shell",
                    "west"
                };

            private static readonly IReadOnlyList<string> CollectibleItems =
                new[]
                {
                    "easter egg",
                    "sand",
                    "fixed point",
                    "coin",
                    "spool of cat6",
                    "shell",
                    "hypercube",
                    "asterisk"
                };

            public void Run(TextReader input)
            {
                var program = input.Lines().First();

                var computer = Computer.Of(program);
                computer.Execute(); // show initial prompt

                RunCommands(computer, CollectAllAndGoToCheckpointCommands);

                var items = BruteforceWeight(computer, CollectibleItems);
                RunCommands(computer, CollectibleItems.Select(item => $"drop {item}"));
                RunCommands(computer, items.Select(item => $"take {item}"));

                // Only thing left is to go "north"
                RunRepl(computer);
            }

            private void RunCommands(Computer computer, IEnumerable<string> commands)
            {
                var queue = new Queue<string>(commands);

                while (queue.Count > 0)
                {
                    var command = queue.Dequeue();
                    computer.Input.EnterAsciiLines(new[] { command });

                    computer.Execute();
                }
            }

            private static void RunRepl(Computer computer)
            {
                while (true)
                {
                    var command = Console.ReadLine();
                    computer.Output.Clear();
                    computer.Input.EnterAsciiLines(new[] { command });

                    computer.Execute();

                    foreach (var line in computer.Output.AsciiLines())
                    {
                        Console.WriteLine(line);
                    }
                }
            }

            private IReadOnlyList<string> BruteforceWeight(Computer computer, IReadOnlyList<string> allItems)
            {
                RunCommands(computer, allItems.Select(item => $"drop {item}"));

                foreach (var itemsToPickup in PossibleItemCombinations(allItems))
                {
                    var clone = computer.Clone();

                    RunCommands(clone, itemsToPickup.Select(item => $"take {item}"));
                    RunCommands(clone, new[] { "north" });

                    var result = clone.Output.AsciiText();
                    if (!result.Contains("you are ejected back to the checkpoint."))
                    {
                        return itemsToPickup;
                    }
                }

                return Array.Empty<string>();
            }

            private IEnumerable<IReadOnlyList<string>> PossibleItemCombinations(IReadOnlyList<string> allItems)
            {
                IReadOnlyList<string> Choose(IReadOnlyList<string> items, IReadOnlyList<bool> takes) =>
                    items.Zip(takes, (item, take) => take ? item : null).Where(i => i != null).ToList();

                var choices = Enumerable.Range(0, allItems.Count)
                    .Select(_ => new[] { true, false })
                    .ToList();

                return EnumerableExtensions.AllPossibleCombinations(choices)
                    .Select(choice => Choose(allItems, choice));
            }
        }

        // There is no Part 2 on Day 25.
    }
}
