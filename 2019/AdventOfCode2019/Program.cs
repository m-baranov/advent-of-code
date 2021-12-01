using System;
using System.Threading.Tasks;

namespace AdventOfCode2019
{
    // https://adventofcode.com/2019
    class Program
    {
        static async Task Main(string[] args)
        {
            var problem = new Day14.Part1();
            await problem.Run(Day14.Sample3Input);

            Console.ReadLine();
        }
    }
}
