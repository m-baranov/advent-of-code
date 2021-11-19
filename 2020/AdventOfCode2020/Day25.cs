using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2020
{
    static class Day25
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "5764801",
                "17807724"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/25/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var keys = input.Lines().Select(long.Parse).ToList();

                var cardKey = keys[0];
                var doorKey = keys[1];

                var cardLoopSize = BruteforceLoopSize(cardKey);
                var doorLookSize = BruteforceLoopSize(doorKey);

                var result = Transform(doorKey, cardLoopSize);
                Console.WriteLine(result);
            }

            private long BruteforceLoopSize(long target)
            {
                long result = 1;
                long i = 0;
                while (result != target)
                {
                    result = (result * 7) % 20201227;
                    i++;
                }
                return i;
            } 

            private long Transform(long subject, long loopSize)
            {
                long result = 1;
                for (var i = 0; i < loopSize; i++)
                {
                    result = (result * subject) % 20201227;
                }
                return result;
            }
        }

        // There is no Part2 on the final day :)
    }
}
