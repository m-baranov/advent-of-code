using System;
using System.Threading.Tasks;

namespace AdventOfCode2019
{
    // https://adventofcode.com/2019
    class Program
    {
        static async Task Main(string[] args)
        {
            var problem = new Day16.Part2();
            await problem.Run(Day16.TestInput);

            Console.ReadLine();
        }
    }
}
