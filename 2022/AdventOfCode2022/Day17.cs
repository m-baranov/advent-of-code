using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day17
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(">>><<><>><<<>><>>><<<>>><<<><<<>><>><<>>");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/17/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var jetPattern = JetPattern.Parse(input.Lines().First());
                var chamber = Simulation.Run(jetPattern, count: 2022);

                Console.WriteLine(chamber.Box.Height);
            }
        }

        public class Part2 : IProblem
        {
            // Slow :(
            public void Run(TextReader input)
            {
                var jetPattern = JetPattern.Parse(input.Lines().First());

                var i = 0;
                var repeat = (index: -1, length: 0);

                var chamber = Simulation.Run(jetPattern, chamber =>
                {
                    i++;

                    if (i > jetPattern.Directions.Count)
                    {
                        repeat = FindRepeatingSection(chamber.Instances);
                        if (repeat.length > 0)
                        {
                            Console.WriteLine($"i={repeat.index}, l={repeat.length}");
                            return false;
                        }
                    }

                    return true;
                });

                i = 0;
                var heights = new List<int>();

                Simulation.Run(jetPattern, chamber =>
                {
                    i++;
                    if (repeat.index <= i)
                    {
                        heights.Add(chamber.Box.Height);
                    }

                    return i <= repeat.index + repeat.length;
                });

                var count = 1000000000000L;
                var div = Math.DivRem(count - repeat.index, repeat.length, out var rem);
                var h = heights[0] + div * (heights[heights.Count - 2] - heights[0]) + (heights[(int)rem] - heights[0]);
                Console.WriteLine(h);
            }

            private static (int index, int length) FindRepeatingSection(IReadOnlyList<Instance> instances)
            {
                static bool Equal(Instance a, Instance b)
                {
                    return a.Point.X == b.Point.X && a.Shape == b.Shape;
                }

                static bool IsRepeatingSection(IReadOnlyList<Instance> instances, int index1, int index2)
                {
                    var len = index2 - index1;

                    var index3 = index2 + len;
                    if (index3 > instances.Count)
                    {
                        return false;
                    }

                    for (var i = 0; i < len; i++)
                    {
                        if (!Equal(instances[index1 + i], instances[index2 + i]))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                var index1 = 0;
                while (index1 < instances.Count)
                {
                    var index2 = index1 + 1;
                    while (index2 < instances.Count)
                    {
                        if (IsRepeatingSection(instances, index1, index2) &&
                            IsRepeatingSection(instances, index2, index2 + index2 - index1))
                        {
                            return (index1, index2 - index1);
                        }

                        index2++;
                    }

                    index1++;
                }

                return (-1, 0);
            }
        }

        private record struct Point(int X, int Y)
        {
            public Point Left() => this with { X = X - 1 };
            public Point Right() => this with { X = X + 1 };
            public Point Down() => this with { Y = Y - 1 };
        }

        private record struct Range(int Start, int End)
        {
            public static bool HitTest(Range a, Range b) =>
                !(a.End < b.Start || b.End < a.Start);

            public static Range Intersect(Range a, Range b) =>
                new Range(Math.Max(a.Start, b.Start), Math.Min(a.End, b.End));

            public int Length => End - Start + 1;
        }

        private record struct Rect(int Left, int Top, int Width, int Height)
        {
            public static bool HitTest(Rect a, Rect b)
            {
                if (!Range.HitTest(a.VRange(), b.VRange()))
                {
                    return false;
                }
                if (!Range.HitTest(a.HRange(), b.HRange()))
                {
                    return false;
                }

                return true;
            }

            public static Rect Intersect(Rect a, Rect b)
            {
                var hr = Range.Intersect(a.HRange(), b.HRange());
                var vr = Range.Intersect(a.VRange(), b.VRange());
                return new Rect(hr.Start, vr.Start, hr.Length, vr.Length);
            }

            public int Right => Left + Width - 1;
            public int Bottom => Top + Height - 1;

            public Range HRange() => new Range(Left, Right);

            public Range VRange() => new Range(Top, Bottom);
        }

        private sealed class Shape
        {
            public static readonly Shape HLine = new Shape(width: 4, height: 1);

            public static readonly Shape VLine = new Shape(width: 1, height: 4);

            public static readonly Shape Square = new Shape(width: 2, height: 2);

            public static readonly Shape Plus = new Shape(width: 3, height: 3,
                holes: new[] { new Point(0, 0), new Point(2, 0), new Point(0, 2), new Point(2, 2) });

            public static readonly Shape Corner = new Shape(width: 3, height: 3,
                holes: new[] { new Point(0, 1), new Point(1, 1), new Point(0, 2), new Point(1, 2) });

            public static readonly IReadOnlyList<Shape> AllOrdered = new[] { HLine, Plus, Corner, VLine, Square };

            private readonly IReadOnlyList<Point> holes;

            private Shape(int width, int height)
                : this(width, height, holes: Array.Empty<Point>())
            {
            }

            private Shape(int width, int height, IReadOnlyList<Point> holes)
            {
                this.Width = width;
                this.Height = height;
                this.holes = holes;
            }

            public int Width { get; }
            public int Height { get; }

            public bool Contains(Point p)
            {
                var inBounds =
                    0 <= p.X && p.X < Width &&
                    0 <= p.Y && p.Y < Height;

                if (!inBounds)
                {
                    return false;
                }

                return !holes.Contains(p);
            }
        }

        private record struct Instance(Shape Shape, Point Point)
        {
            public static bool HitTest(Instance a, Instance b)
            {
                var boxA = a.Box();
                var boxB = b.Box();

                if (!Rect.HitTest(boxA, boxB))
                {
                    return false;
                }

                var box = Rect.Intersect(boxA, boxB);
                for (var y = box.Top; y <= box.Bottom; y++)
                {
                    for (var x = box.Left; x <= box.Right; x++)
                    {
                        var p = new Point(x, y);
                        if (Contains(a, p) && Contains(b, p))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            public static bool Contains(Instance i, Point p) => 
                i.Shape.Contains(new Point(p.X - i.Point.X, p.Y - i.Point.Y));

            public Rect Box() => new Rect(Point.X, Point.Y, Shape.Width, Shape.Height);

            public Instance Move(Direction direction) =>
                direction == Direction.Left ? 
                    this with { Point = Point.Left() } : 
                    this with { Point = Point.Right() };

            public Instance MoveDown() =>
                this with { Point = Point.Down() };
        }

        private sealed class Chamber
        {
            private readonly List<Instance> instances;

            public Chamber(int width)
            {
                Box = new Rect(0, 0, width, 0);
                this.instances = new List<Instance>();
            }

            public Rect Box { get; private set; }
            public IReadOnlyList<Instance> Instances => this.instances;

            public bool HitTest(Instance instance)
            {
                var ibox = instance.Box();
                if (ibox.Left < Box.Left || Box.Right < ibox.Right)
                {
                    return true;
                }
                if (ibox.Top < Box.Top)
                {
                    return true;
                }

                return this.instances.Any(i => Instance.HitTest(i, instance));
            }

            public void Add(Instance instance)
            {
                this.Box = Box with { Height = Math.Max(Box.Bottom, instance.Box().Bottom) + 1 };
                this.instances.Add(instance);
            }
        }

        private enum Direction { Left, Right }

        private record JetPattern(IReadOnlyList<Direction> Directions)
        {
            public static JetPattern Parse(string text)
            {
                static Direction ParseDirection(char ch) => 
                    ch == '<' ? Direction.Left : Direction.Right;

                var directions = text.Select(ParseDirection).ToList();
                return new JetPattern(directions);
            }
        }

        private static class Simulation
        {
            public static Chamber Run(JetPattern jetPattern, int count)
            {
                var i = 0;
                return Simulation.Run(jetPattern, _ => ++i < count);
            }

            public static Chamber Run(JetPattern jetPattern, Func<Chamber, bool> shouldContinue)
            {
                var shapes = Shape.AllOrdered;

                var chamber = new Chamber(width: 7);

                var jetIndex = 0;
                var shapeIndex = 0;

                while (true)
                {
                    var shape = shapes[shapeIndex];
                    shapeIndex = (shapeIndex + 1) % shapes.Count;

                    var point = new Point(X: 2, Y: chamber.Box.Bottom + 1 + 3);
                    var instance = new Instance(shape, point);
                    //Draw(chamber, instance);

                    while (true)
                    {
                        var direction = jetPattern.Directions[jetIndex];
                        jetIndex = (jetIndex + 1) % jetPattern.Directions.Count;

                        var sideInstance = instance.Move(direction);
                        if (!chamber.HitTest(sideInstance))
                        {
                            instance = sideInstance;
                        }

                        var downInstance = instance.MoveDown();
                        if (!chamber.HitTest(downInstance))
                        {
                            instance = downInstance;
                        }
                        else
                        {
                            chamber.Add(instance);
                            //Draw(chamber, instance);
                            //Console.ReadLine();
                            break;
                        }
                    }

                    if (!shouldContinue(chamber))
                    {
                        break;
                    }
                }

                return chamber;
            }

            public static void Draw(Chamber chamber, Instance? instance = null)
            {
                var lines = new List<string>();

                for (var y = 0; y <= chamber.Box.Bottom + 8; y++)
                {
                    var line = new List<char>();
                    for (var x = 0; x <= chamber.Box.Right; x++)
                    {
                        var point = new Point(x, y);
                        var ch = chamber.Instances.Any(i => Instance.Contains(i, point)) ? '#' : '.';

                        if (instance != null && Instance.Contains(instance.Value, point))
                        {
                            ch = '@';
                        }
                        line.Add(ch);
                    }

                    lines.Add(string.Join("", line));
                }

                lines.Reverse();
                foreach (var line in lines)
                {
                    Console.WriteLine(line);
                }
                Console.WriteLine($"H={chamber.Box.Height}");
                Console.WriteLine();
            }
        }
    }
}
