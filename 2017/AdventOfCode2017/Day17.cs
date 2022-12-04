using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day17
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal("3");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/17/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var steps = int.Parse(input.Lines().First());

                var ring = new Ring(0);
                for (var i = 1; i <= 2017; i++)
                {
                    ring = Ring.InsertAfter(Ring.Next(ring, steps), i);
                }

                var at2017 = Ring.Find(ring, value: 2017);
                var after2017 = Ring.Next(at2017, steps: 1);

                Console.WriteLine(after2017.Value);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var steps = int.Parse(input.Lines().First());

                var pos = 0;
                var after0 = 0;
                for (var i = 1; i <= 50_000_000; i++)
                {
                    pos = ((pos + steps) % i + 1) % i;
                    if (pos == 0)
                    {
                        after0 = i;
                    }
                }

                Console.WriteLine(after0);
            }
        }

        private class Ring
        {
            private readonly long value;
            private Ring next;

            public Ring(long value)
            {
                this.value = value;
                this.next = this;
            }

            private Ring(long value, Ring next)
            {
                this.value = value;
                this.next = next;
            }

            public long Value => this.value;

            public static Ring Find(Ring start, long value)
            {
                if (start.Value == value)
                {
                    return start;
                }

                var r = start.next;
                while (r != start)
                {
                    if (r.Value == value)
                    {
                        return r;
                    }
                    r = r.next;
                }

                return default;
            }

            public static Ring Next(Ring current, long steps)
            {
                var r = current;
                for (var i = 0; i < steps; i++)
                {
                    r = r.next;
                }
                return r;
            }

            public static Ring InsertAfter(Ring current, long value)
            {
                //   curr      next
                //  [    ] -> [    ]
                //  [    ] <- [    ]

                //   curr      NEW       next
                //  [    ] -> [    ] -> [    ]
                //  [    ] <- [    ] <- [    ]

                var next = current.next;

                var @new = new Ring(value, next);

                current.next = @new;

                return @new;
            }

            public static void Dump(Ring current)
            {
                var r = current;
                do
                {
                    Console.Write(r.Value);
                    Console.Write(" ");

                    r = r.next;
                }
                while (r != current);

                Console.WriteLine();
            }
        }
    }
}
