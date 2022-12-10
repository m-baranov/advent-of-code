using System;
using System.Threading.Tasks;

namespace AdventOfCode2022
{
    // https://adventofcode.com/2022
    class Program
    {
        static async Task Main(string[] args)
        {
            Input.HttpSession = Environment.GetEnvironmentVariable("ADVENT_OF_CODE_SESSION", EnvironmentVariableTarget.User);

            var problem = new Day10.Part2();
            await problem.Run(Day10.Inputs.Test);

            Console.ReadLine();
        }
    }
}
