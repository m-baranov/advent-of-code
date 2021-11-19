using System;
using System.IO;
using System.Linq;

namespace AdventOfCode2020
{
    static class Day12
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "F10",
                "N3",
                "F7",
                "R90",
                "F11"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/12/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var instructions = input.Lines().Select(Instruction.Parse).ToList();

                var ship = new Ship(0, 0, Direction.East);
                var final = instructions.Aggregate(ship, (ship, instr) => ship.Apply(instr));

                var dist = Math.Abs(final.X) + Math.Abs(final.Y);
                Console.WriteLine(dist);
            }

            public class Ship
            {
                public Ship(int x, int y, Direction dir)
                {
                    X = x;
                    Y = y;
                    Dir = dir;
                }

                public int X { get; }
                public int Y { get; }
                public Direction Dir { get; }

                public Ship With(int? newX = null, int? newY = null, Direction? newDir = null)
                {
                    return new Ship(newX ?? X, newY ?? Y, newDir ?? Dir);
                }

                public Ship Apply(Instruction instruction)
                {
                    if (instruction.Operation == Operation.North)
                    {
                        return With(newY: Y - instruction.Value);
                    }
                    if (instruction.Operation == Operation.South)
                    {
                        return With(newY: Y + instruction.Value);
                    }
                    if (instruction.Operation == Operation.East)
                    {
                        return With(newX: X + instruction.Value);
                    }
                    if (instruction.Operation == Operation.West)
                    {
                        return With(newX: X - instruction.Value);
                    }
                    if (instruction.Operation == Operation.Right)
                    {
                        return With(newDir: Util.Rotate(Dir, instruction.Value));
                    }
                    if (instruction.Operation == Operation.Left)
                    {
                        return With(newDir: Util.Rotate(Dir, -instruction.Value));
                    }
                    if (instruction.Operation == Operation.Forward)
                    {
                        var operation = Util.DirectionToOperation(Dir);
                        var newInstruction = new Instruction(operation, instruction.Value);
                        return Apply(newInstruction);
                    }
                    return this;
                }
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var instructions = input.Lines().Select(Instruction.Parse).ToList();

                var ship = new Ship(x: 0, y: 0, wx: 10, wy: 1);
                var final = instructions.Aggregate(ship, (ship, instr) =>
                {
                    var next = ship.Apply(instr);
                    return next;
                });

                var dist = Math.Abs(final.X) + Math.Abs(final.Y);
                Console.WriteLine(dist);
            }

            public class Ship
            {
                public Ship(int x, int y, int wx, int wy)
                {
                    X = x;
                    Y = y;
                    Wx = wx;
                    Wy = wy;
                }

                public int X { get; }
                public int Y { get; }
                public int Wx { get; }
                public int Wy { get; }

                public Ship With(int? newX = null, int? newY = null, int? newWx = null, int? newWy = null)
                {
                    return new Ship(newX ?? X, newY ?? Y, newWx ?? Wx, newWy ?? Wy);
                }

                public Ship Apply(Instruction instruction)
                {
                    if (instruction.Operation == Operation.North)
                    {
                        return With(newWy: Wy + instruction.Value);
                    }
                    if (instruction.Operation == Operation.South)
                    {
                        return With(newWy: Wy - instruction.Value);
                    }
                    if (instruction.Operation == Operation.East)
                    {
                        return With(newWx: Wx + instruction.Value);
                    }
                    if (instruction.Operation == Operation.West)
                    {
                        return With(newWx: Wx - instruction.Value);
                    }
                    if (instruction.Operation == Operation.Right)
                    {
                        var (newWx, newWy) = Rotate(Wx, Wy, instruction.Value);
                        return With(newWx: newWx, newWy: newWy);
                    }
                    if (instruction.Operation == Operation.Left)
                    {
                        var (newWx, newWy) = Rotate(Wx, Wy, -instruction.Value);
                        return With(newWx: newWx, newWy: newWy);
                    }
                    if (instruction.Operation == Operation.Forward)
                    {
                        var dx = Wx * instruction.Value;
                        var dy = Wy * instruction.Value;
                        return With(newX: X + dx, newY: Y + dy);
                    }
                    return this;
                }

                private (int x, int y) Rotate(int x, int y, int angle)
                {
                    var cw = angle > 0; 
                    var rot = Math.Abs(angle) / 90;
                    var mul = cw ? 1 : -1;

                    for (var i = 0; i < rot; i++)
                    {
                        var nx = mul * y;
                        var ny = mul * -x;
                        x = nx;
                        y = ny;
                    }

                    return (x, y);
                }
            }
        }

        public enum Operation
        {
            North,
            South,
            East,
            West,
            Left,
            Right, 
            Forward
        }

        public class Instruction
        {
            public static Instruction Parse(string text)
            {
                var operationText = text.Substring(0, 1);
                var valueText = text.Substring(1);

                var operation = ParseOperation(operationText);
                var value = ParseValue(valueText);

                return new Instruction(operation, value);
            }

            private static Operation ParseOperation(string text)
            {
                if (text == "N") return Operation.North;
                if (text == "S") return Operation.South;
                if (text == "E") return Operation.East;
                if (text == "W") return Operation.West;
                if (text == "L") return Operation.Left;
                if (text == "R") return Operation.Right;
                /* if (text == "F") */ return Operation.Forward;
            }

            private static int ParseValue(string text)
            {
                return int.Parse(text);
            }

            public Instruction(Operation operation, int value)
            {
                Operation = operation;
                Value = value;
            }

            public Operation Operation { get; }
            public int Value { get; }
        }

        public enum Direction 
        {
            North,
            East,
            South,
            West
        }

        public static class Util
        {
            public static Direction Rotate(Direction dir, int angleClockwise)
            {
                var rot = angleClockwise / 90;
                var next = ((int)dir + rot) % 4;

                if (next < 0)
                {
                    next = 4 + next;
                }

                return (Direction)next;
            }

            public static Operation DirectionToOperation(Direction dir)
            {
                if (dir == Direction.North) return Operation.North;
                if (dir == Direction.East) return Operation.East;
                if (dir == Direction.South) return Operation.South;
                /*if (dir == Direction.West)*/ return Operation.West;
            }
        }
    }
}
