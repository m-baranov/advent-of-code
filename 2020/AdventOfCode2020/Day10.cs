using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2020
{
    static class Day10
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "16",
                "10",
                "15",
                "5",
                "1",
                "11",
                "7",
                "19",
                "6",
                "12",
                "4"
            );

        public static readonly IInput SampleInput2 =
            Input.Literal(
                "28",
                "33",
                "18",
                "42",
                "31",
                "14",
                "46",
                "20",
                "48",
                "47",
                "24",
                "23",
                "49",
                "45",
                "19",
                "38",
                "39",
                "11",
                "1",
                "32",
                "25",
                "35",
                "8",
                "17",
                "7",
                "9",
                "4",
                "2",
                "34",
                "10",
                "3"
            );

        public static readonly IInput TestInput =
           Input.Http("https://adventofcode.com/2020/day/10/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var numbers = input.Lines().Select(int.Parse).OrderBy(n => n).ToArray();

                var solution = Solve(numbers, new Stack<int>(capacity: numbers.Length), 0);

                var previous = 0;
                var ones = 0;
                var threes = 1;
                for (var i = 0; i < solution.Count; i++)
                {
                    var diff = solution[i] - previous;
                    if (diff == 3) threes++;
                    if (diff == 1) ones++;

                    previous = solution[i];
                }

                Console.WriteLine(ones * threes);
            }

            private IReadOnlyList<int> Solve(int[] numbers, Stack<int> seq, int current)
            {
                for (var i = 0; i < numbers.Length; i++)
                {
                    var number = numbers[i];
                    if (number < 0)
                    {
                        continue;
                    }

                    if (number > current + 3) 
                    {
                        break;
                    }
                    
                    if (current + 1 <= number && number <= current + 3)
                    {
                        numbers[i] = -1;
                        seq.Push(number);

                        if (seq.Count == numbers.Length)
                        {
                            return seq.Reverse().ToList();
                        }

                        var result = Solve(numbers, seq, number);
                        if (result != null)
                        {
                            return result;
                        }

                        numbers[i] = number;
                        seq.Pop();
                    }
                }

                return null;
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var numbers = input.Lines().Select(int.Parse).OrderByDescending(n => n).ToArray();

                var start = numbers.Max() + 3;

                var solution = Solve(numbers, start, new Dictionary<int, long>());
                Console.WriteLine(solution);
            }

            private long Solve(int[] numbers, int start, Dictionary<int, long> memo)
            {
                if (start <= 0)
                {
                    return 0;
                }
                
                if (memo.TryGetValue(start, out var m))
                {
                    return m;
                }

                var canConnect = numbers.Where(n => start - 3 <= n && n <= start - 1);

                var solution = canConnect.Select(n => Solve(numbers, n, memo)).Sum();

                if (1 <= start && start <= 3)
                {
                    solution += 1;
                }

                memo.Add(start, solution);

                return solution;
            }
        }
    }
}
