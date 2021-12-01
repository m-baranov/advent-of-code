using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day06
    {
        public static readonly IInput Sample1Input =
            Input.Literal(
                "COM)B",
                "B)C",
                "C)D",
                "D)E",
                "E)F",
                "B)G",
                "G)H",
                "D)I",
                "E)J",
                "J)K",
                "K)L"
            );

        public static readonly IInput Sample2Input =
            Input.Literal(
                "COM)B",
                "B)C",
                "C)D",
                "D)E",
                "E)F",
                "B)G",
                "G)H",
                "D)I",
                "E)J",
                "J)K",
                "K)L",
                "K)YOU",
                "I)SAN"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/6/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var objects = ObjectOrbit.ParseMany(input.Lines()).ToList();

                var orbitLevels = new List<List<ObjectOrbit>>()
                {
                    new List<ObjectOrbit>() { new ObjectOrbit("COM", orbitsAround: null) }
                };

                bool madeProgress;
                do
                {
                    madeProgress = false;

                    var lastObjects = orbitLevels.Last();
                    var nextObjects = objects
                        .Where(obj => lastObjects.Any(orb => orb.ObjectName == obj.OrbitsAround))
                        .ToList();

                    if (nextObjects.Count > 0)
                    {
                        orbitLevels.Add(nextObjects);
                        madeProgress = true;
                    }
                } while (madeProgress);

                var checksum = orbitLevels
                    .Select((objects, index) => index * objects.Count)
                    .Sum();

                Console.WriteLine(checksum);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var objects = ObjectOrbit.ParseMany(input.Lines()).ToList();

                var pathYou = PathBetween(objects, "YOU", "COM");
                var pathSan = PathBetween(objects, "SAN", "COM");

                //                    *
                // indexYou :  0  1 2 3 4 5 6  7
                // pathYou  : YOU K J E D C B COM
                // pathSan  :     SAN I D C B COM
                // indexSan :      0  1 2 3 4  5
                //                    *

                var youIndex = pathYou.Count - 1;
                var sanIndex = pathSan.Count - 1;
                while (pathYou[youIndex] == pathSan[sanIndex])
                {
                    youIndex--;
                    sanIndex--;
                }

                Console.WriteLine(youIndex + sanIndex);
            }

            private IReadOnlyList<string> PathBetween(IReadOnlyList<ObjectOrbit> objects, string from, string to)
            {
                var path = new List<string>();

                var currentName = from;
                while (true)
                {
                    path.Add(currentName);

                    var obj = objects.FirstOrDefault(o => o.ObjectName == currentName);
                    if (obj == null)
                    {
                        break;
                    }

                    currentName = obj.OrbitsAround;
                }

                return path;
            }
        }

        private class ObjectOrbit
        {
            public static IEnumerable<ObjectOrbit> ParseMany(IEnumerable<string> lines)
            {
                return lines.Select(Parse);
            }

            public static ObjectOrbit Parse(string text)
            {
                var index = text.IndexOf(')');
                var left = text.Substring(0, index);
                var right = text.Substring(index + 1);

                return new ObjectOrbit(objectName: right, orbitsAround: left);
            }

            public ObjectOrbit(string objectName, string orbitsAround)
            {
                ObjectName = objectName;
                OrbitsAround = orbitsAround;
            }

            public string ObjectName { get; }
            public string OrbitsAround { get; }
        }
    }
}
