using System;
using System.Threading.Tasks;

namespace AdventOfCode2020
{
    // https://adventofcode.com/2020
    class Program
    {
        static async Task Main(string[] args)
        {
            var problem = new Day25.Part1();
            await problem.Run(Day25.TestInput);

            Console.ReadLine();
        }
    }
}
