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

            var problem = new Day15.Part1();
            await problem.Run(Day15.Inputs.Test);

            Console.ReadLine();
        }
    }
}
