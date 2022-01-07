using System;
using System.Threading.Tasks;

namespace AdventOfCode2019
{
    // https://adventofcode.com/2019
    class Program
    {
        static async Task Main(string[] args)
        {
            Input.HttpSession = Environment.GetEnvironmentVariable("ADVENT_OF_CODE_SESSION");

            var problem = new Day25.Part1();
            await problem.Run(Day25.TestInput);

            Console.ReadLine();
        }
    }
}
