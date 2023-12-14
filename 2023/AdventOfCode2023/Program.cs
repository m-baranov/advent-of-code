using System;
using System.Threading.Tasks;

namespace AdventOfCode2023
{
    // https://adventofcode.com/2023
    class Program
    {
        static async Task Main(string[] args)
        {
            Input.HttpSession = Environment.GetEnvironmentVariable("ADVENT_OF_CODE_SESSION", EnvironmentVariableTarget.User)!;

            var problem = new Day14.Part2();
            await problem.Run(Day14.Inputs.Test);

            Console.ReadLine();
        }
    }
}
