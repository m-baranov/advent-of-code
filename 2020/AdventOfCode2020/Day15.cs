using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2020
{
    static class Day15
    {
        public static readonly IInput SampleInput =
            Input.Literal("0,3,6");

        public static readonly IInput SampleInput2 =
            Input.Literal("1,3,2");

        public static readonly IInput SampleInput3 =
            Input.Literal("2,1,3");

        public static readonly IInput SampleInput4 =
            Input.Literal("1,2,3");

        public static readonly IInput SampleInput5 =
            Input.Literal("2,3,1");

        public static readonly IInput SampleInput6 =
            Input.Literal("3,2,1");

        public static readonly IInput SampleInput7 =
            Input.Literal("3,1,2");

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/15/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var line = input.Lines().First();

                var init = line.Split(',').Select(int.Parse).ToList();

                var numbers = new int[2020];
                for (var i = 0; i < init.Count; i++)
                {
                    numbers[i] = init[i];
                }

                for (var i = init.Count; i < numbers.Length; i++)
                {
                    var number = numbers[i - 1];

                    var j = i - 2;
                    var age = 0;
                    while (j >= 0)
                    {
                        if (numbers[j] == number)
                        {
                            age = i - 1 - j;
                            break;
                        }
                        j--;
                    }

                    numbers[i] = age;
                }

                Console.WriteLine(numbers[numbers.Length - 1]);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var line = input.Lines().First();

                var init = line.Split(',').Select(int.Parse).ToList();

                var lastAt = new Dictionary<int, (int, int)>();

                for (var i = 0; i < init.Count;i++)
                {
                    lastAt[init[i]] = (i, -1);
                }

                var lastNumber = init[init.Count - 1];
                for (var i = init.Count; i < 30000000; i++)
                {
                    var age = 0;
                    if (lastAt.TryGetValue(lastNumber, out var lastIndices))
                    {
                        var (oneBack, twoBack) = lastIndices;
                        if (twoBack >= 0)
                        {
                            age = oneBack - twoBack;
                        }
                    }
                    
                    if (!lastAt.TryGetValue(age, out var nextIndices))
                    {
                        nextIndices = (-1, -1);
                    }

                    lastAt[age] = (i, nextIndices.Item1);
                    lastNumber = age;
                }

                Console.WriteLine(lastNumber);
            }
        }
    }
}
