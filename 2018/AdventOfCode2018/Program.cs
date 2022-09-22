using System;
using System.Threading.Tasks;

namespace AdventOfCode2018
{
    // https://adventofcode.com/2018
    class Program
    {
        static async Task Main(string[] args)
        {
            Input.HttpSession = Environment.GetEnvironmentVariable("ADVENT_OF_CODE_SESSION", EnvironmentVariableTarget.User);

            var problem = new Day18.Part2();
            await problem.Run(Day18.Inputs.Test);

            Console.ReadLine();
        }
    }
}
