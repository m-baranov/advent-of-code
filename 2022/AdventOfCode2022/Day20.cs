using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day20
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "1",
                    "2",
                    "-3",
                    "3",
                    "-2",
                    "0",
                    "4"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/20/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var values = Parse(input, mul: 1);

                var ring = Ring.Of(values);

                ring = Mix(ring, values);

                var sum = Sum(ring, values.Count);
                Console.WriteLine(sum);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var values = Parse(input, mul: 811589153L);

                var ring = Ring.Of(values);

                for (var i = 0; i < 10; i++)
                {
                    ring = Mix(ring, values);
                }

                var sum = Sum(ring, values.Count);
                Console.WriteLine(sum);
            }
        }

        private static IReadOnlyList<(long number, int index)> Parse(TextReader input, long mul)
        {
            return input.Lines()
                .Select(long.Parse)
                .Select((number, index) => (number: number * mul, index))
                .ToList();
        }

        private static Ring<(long number, int index)> Mix(
            Ring<(long number, int index)> ring,
            IReadOnlyList<(long number, int index)> values)
        {
            foreach (var value in values)
            {
                if (value.number == 0)
                {
                    continue;
                }

                var current = Ring.Find(ring, v => v.Equals(value));
                Debug.Assert(current != null);

                var after = Ring.Remove(current);
                if (value.number > 0)
                {
                    after = Ring.Next(Ring.Prev(after, 1), (int)(value.number % (values.Count - 1)));
                }
                else
                {
                    after = Ring.Prev(Ring.Prev(after, 1), (int)((-value.number) % (values.Count - 1)));
                }

                ring = Ring.InsertAfter(after, current.Value);

            }

            return ring;
        }

        private static long Sum(Ring<(long number, int index)> ring, int count)
        {
            ring = Ring.Find(ring, v => v.number == 0);

            var indexes = new[] { 1000, 2000, 3000 };

            return indexes
                .Select(i => i % count)
                .Select(i => Ring.Next(ring, i).Value.number)
                .Sum();
        }

        private static class Ring
        {
            public static Ring<T> Of<T>(IReadOnlyList<T> values)
            {
                var root = new Ring<T>(values[0]);

                var current = root;
                for (var i = 1; i < values.Count; i++)
                {
                    current = Ring.InsertAfter(current, values[i]);
                }

                return root;
            }

            public static Ring<T> Next<T>(Ring<T> current, int steps)
            {
                var r = current;
                for (var i = 0; i < steps; i++)
                {
                    r = r.Next;
                }
                return r;
            }

            public static Ring<T> Prev<T>(Ring<T> current, int steps)
            {
                var r = current;
                for (var i = 0; i < steps; i++)
                {
                    r = r.Prev;
                }
                return r;
            }

            public static Ring<T> InsertAfter<T>(Ring<T> current, T value)
            {
                //   curr      next
                //  [    ] -> [    ]
                //  [    ] <- [    ]

                //   curr      NEW       next
                //  [    ] -> [    ] -> [    ]
                //  [    ] <- [    ] <- [    ]

                var next = current.Next;

                var @new = new Ring<T>(value, next: next, prev: current);

                current.Next = @new;
                next.Prev = @new;

                return @new;
            }

            public static Ring<T> Remove<T>(Ring<T> current)
            {
                //   prev      curr      next
                //  [    ] -> [    ] -> [    ]
                //  [    ] <- [    ] <- [    ]

                //   prev      next
                //  [    ] -> [    ]
                //  [    ] <- [    ]

                var prev = current.Prev;
                var next = current.Next;

                prev.Next = next;
                next.Prev = prev;

                return next;
            }

            public static Ring<T> Find<T>(Ring<T> start, Func<T, bool> predicate)
            {
                var r = start;
                do
                {
                    if (predicate(r.Value))
                    {
                        return r;
                    }

                    r = r.Next;
                }
                while (r != start);

                return null;
            }

            public static void Dump<T>(Ring<T> current)
            {
                var r = current;
                do
                {
                    Console.Write(r.Value);
                    Console.Write(" ");

                    r = r.Next;
                }
                while (r != current);

                Console.WriteLine();
            }
        }
        
        private class Ring<T>
        {
            public Ring(T value)
            {
                this.Value = value;
                this.Next = this;
                this.Prev = this;
            }

            public Ring(T value, Ring<T> next, Ring<T> prev)
            {
                this.Value = value;
                this.Next = next;
                this.Prev = prev;
            }

            public T Value { get; }
            public Ring<T> Next { get; set; }
            public Ring<T> Prev { get; set; }
        }
    }
}
