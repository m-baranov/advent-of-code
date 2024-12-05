using System;
using System.Threading.Tasks;

namespace AdventOfCode2024
{
    // https://adventofcode.com/2024
    class Program
    {
        static async Task Main(string[] args)
        {
            Input.HttpSession = Environment.GetEnvironmentVariable("ADVENT_OF_CODE_SESSION", EnvironmentVariableTarget.User)!;

            var problem = new Day05.Part2();
            await problem.Run(Day05.Inputs.Test);

            Console.ReadLine();
        }
    }
}
