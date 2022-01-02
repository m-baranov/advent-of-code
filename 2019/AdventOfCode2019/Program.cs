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

            var problem = new Day20.Part2();
            await problem.Run(Day20.TestInput);

            Console.ReadLine();
        }
    }
}
