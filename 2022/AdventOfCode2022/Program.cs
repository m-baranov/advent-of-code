﻿using System;
using System.Threading.Tasks;

namespace AdventOfCode2022
{
    // https://adventofcode.com/2022
    class Program
    {
        static async Task Main(string[] args)
        {
            Input.HttpSession = Environment.GetEnvironmentVariable("ADVENT_OF_CODE_SESSION", EnvironmentVariableTarget.User);

            var problem = new Day25.Part1();
            await problem.Run(Day25.Inputs.Test);

            Console.ReadLine();
        }
    }
}
