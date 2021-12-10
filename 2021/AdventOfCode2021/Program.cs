using System;
using System.Threading.Tasks;

namespace AdventOfCode2021
{
    // https://adventofcode.com/2021
    class Program
    {
        static async Task Main(string[] args)
        {
            Input.HttpSession = Environment.GetEnvironmentVariable("ADVENT_OF_CODE_SESSION");

            var problem = new Day10.Part2();
            await problem.Run(Day10.Inputs.Test);

            Console.ReadLine();
        }
    }
}
