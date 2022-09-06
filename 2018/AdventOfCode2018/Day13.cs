using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day13
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    @"/->-\        ",
                    @"|   |  /----\",
                    @"| /-+--+-\  |",
                    @"| | |  | v  |",
                    @"\-+-/  \-+--/",
                    @"  \------/   "
                );

            public static readonly IInput Sample2 =
                Input.Literal(
                    @"/>-<\  ",
                    @"|   |  ",
                    @"| /<+-\",
                    @"| | | v",
                    @"\>+</ |",
                    @"  |   ^",
                    @"  \<->/"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/13/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var (map, carts) = Map.Parse(input.Lines().ToList());

                //map.Draw(carts);
                //Console.ReadLine();

                while (true)
                {
                    var (nextCarts, isCrash, crashPos) = map.MovePart1(carts);

                    if (isCrash)
                    {
                        Console.WriteLine(crashPos);
                        break;
                    }
                    //else
                    //{
                    //    map.Draw(nextCarts);
                    //    Console.ReadLine();
                    //}

                    carts = nextCarts;
                }
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var (map, carts) = Map.Parse(input.Lines().ToList());

                while (carts.Count > 1)
                {
                    carts = map.MovePart2(carts);
                }

                Console.WriteLine(carts[0].Pos);
            }
        }

        private record struct Point(int Row, int Col)
        {
            public static readonly Point Origin = new Point(0, 0);

            public override string ToString() => $"{Col},{Row}";

            public Point Move(Direction dir)
            {
                if (dir == Direction.Left) return Left();
                if (dir == Direction.Right) return Right();
                if (dir == Direction.Up) return Up();
                return Down();
            }

            public Point Left() => new Point(Row, Col - 1);
            public Point Right() => new Point(Row, Col + 1);
            public Point Up() => new Point(Row - 1, Col);
            public Point Down() => new Point(Row + 1, Col);
        }

        private enum Turn { Left, Straight, Right }

        private abstract class Direction
        {
            public static readonly Direction Up = new UpDirection();
            public static readonly Direction Down = new DownDirection();
            public static readonly Direction Left = new LeftDirection();
            public static readonly Direction Right = new RightDirection();

            public abstract Direction Rotate(Turn turn);

            private class UpDirection : Direction 
            {
                public override string ToString() => "^";

                public override Direction Rotate(Turn turn)
                {
                    if (turn == Turn.Left) return Direction.Left;
                    if (turn == Turn.Right) return Direction.Right;
                    return Direction.Up;
                }
            }

            private class DownDirection : Direction
            {
                public override string ToString() => "v";

                public override Direction Rotate(Turn turn)
                {
                    if (turn == Turn.Left) return Direction.Right;
                    if (turn == Turn.Right) return Direction.Left;
                    return Direction.Down;
                }
            }

            private class LeftDirection : Direction 
            {
                public override string ToString() => "<";

                public override Direction Rotate(Turn turn)
                {
                    if (turn == Turn.Left) return Direction.Down;
                    if (turn == Turn.Right) return Direction.Up;
                    return Direction.Left;
                }
            }

            private class RightDirection : Direction 
            {
                public override string ToString() => ">";

                public override Direction Rotate(Turn turn)
                {
                    if (turn == Turn.Left) return Direction.Up;
                    if (turn == Turn.Right) return Direction.Down;
                    return Direction.Right;
                }
            }
        }

        private class Cart
        {
            private static readonly IReadOnlyList<Turn> TurnOrder =
                new[] { Turn.Left, Turn.Straight, Turn.Right };
            
            private readonly Point pos;
            private readonly Direction dir;
            private readonly int nextTurnIndex;

            public Cart(Point pos, Direction dir) : this(pos, dir, nextTurnIndex: 0) { }

            private Cart(Point pos, Direction dir, int nextTurnIndex)
            {
                this.pos = pos;
                this.dir = dir;
                this.nextTurnIndex = nextTurnIndex;
            }

            public Point Pos => this.pos;

            public Direction Dir => this.dir;

            public Cart Move(Map map)
            {
                var newPos = this.pos.Move(this.dir);

                var cell = map.At(newPos);

                if (cell == Cell.UpLeftTurn)
                {
                    //  |
                    // -/
                    var newDir = this.dir == Direction.Down ? Direction.Left : Direction.Up;
                    return new Cart(newPos, newDir, this.nextTurnIndex);
                }
                else if (cell == Cell.UpRightTurn)
                {
                    // |
                    // \-
                    var newDir = this.dir == Direction.Down ? Direction.Right : Direction.Up;
                    return new Cart(newPos, newDir, this.nextTurnIndex);
                }
                else if (cell == Cell.DownLeftTurn)
                {
                    // -\
                    //  |
                    var newDir = this.dir == Direction.Up ? Direction.Left : Direction.Down;
                    return new Cart(newPos, newDir, this.nextTurnIndex);
                }
                else if (cell == Cell.DownRightTurn)
                {
                    // /-
                    // |
                    var newDir = this.dir == Direction.Up ? Direction.Right : Direction.Down;
                    return new Cart(newPos, newDir, this.nextTurnIndex);
                }
                else if (cell == Cell.Intersection)
                {
                    var newDir = this.dir.Rotate(TurnOrder[this.nextTurnIndex]);
                    var newNextTurnIndex = (this.nextTurnIndex + 1) % TurnOrder.Count;
                    return new Cart(newPos, newDir, newNextTurnIndex);
                }
                else
                {
                    return new Cart(newPos, this.dir, this.nextTurnIndex);
                }
            }
        }
        
        private abstract class Cell
        {
            public static readonly Cell Empty = new EmptyCell();
            public static readonly Cell Horizontal = new HorizontalCell();
            public static readonly Cell Vertical = new VerticalCell();
            public static readonly Cell DownRightTurn = new DownRightTurnCell();
            public static readonly Cell DownLeftTurn = new DownLeftTurnCell();
            public static readonly Cell UpRightTurn = new UpRightTurnCell();
            public static readonly Cell UpLeftTurn = new UpLeftTurnCell();
            public static readonly Cell Intersection = new IntersectionCell();

            private class EmptyCell : Cell 
            {
                public override string ToString() => " ";
            }

            private class HorizontalCell : Cell 
            {
                public override string ToString() => "-";
            }

            private class VerticalCell : Cell 
            {
                public override string ToString() => "|";
            }

            private class DownRightTurnCell : Cell 
            {
                public override string ToString() => "/";
            }

            private class DownLeftTurnCell : Cell 
            {
                public override string ToString() => "\\";
            }

            private class UpRightTurnCell : Cell 
            {
                public override string ToString() => "\\";
            }

            private class UpLeftTurnCell : Cell 
            {
                public override string ToString() => "/";
            }

            private class IntersectionCell : Cell 
            {
                public override string ToString() => "+";
            }
        }

        private class CharMap
        {
            private readonly IReadOnlyList<string> lines;

            public CharMap(IReadOnlyList<string> lines)
            {
                this.lines = lines;
            }

            public int Rows => this.lines.Count;

            public int Cols => this.lines[0].Length;

            public bool InBounds(Point p) =>
                0 <= p.Col && p.Col < Cols &&
                0 <= p.Row && p.Row < Rows;

            public char At(Point p) =>
                InBounds(p) ? this.lines[p.Row][p.Col] : ' ';
        }

        private class Map
        {
            public static (Map, IReadOnlyList<Cart>) Parse(IReadOnlyList<string> lines) =>
                Parse(new CharMap(lines));

            public static (Map, IReadOnlyList<Cart>) Parse(CharMap charMap)
            {
                static (Cell, Cart) Classify(CharMap map, Point pos)
                {
                    bool HasPathToRight(CharMap map, Point pos)
                    {
                        var cell = map.At(pos.Right());
                        return cell == '-' || cell == '+' || cell == '<' || cell == '>';
                    }

                    return map.At(pos) switch
                    {
                        '-' => (Cell.Horizontal, null),
                        '<' => (Cell.Horizontal, new Cart(pos, Direction.Left)),
                        '>' => (Cell.Horizontal, new Cart(pos, Direction.Right)),

                        '|' => (Cell.Vertical, null),
                        '^' => (Cell.Vertical, new Cart(pos, Direction.Up)),
                        'v' => (Cell.Vertical, new Cart(pos, Direction.Down)),

                        '/' when HasPathToRight(map, pos) => (Cell.DownRightTurn, null),
                        '/' => (Cell.UpLeftTurn, null),

                        '\\' when HasPathToRight(map, pos) => (Cell.UpRightTurn, null),
                        '\\' => (Cell.DownLeftTurn, null),

                        '+' => (Cell.Intersection, null),
                        _ => (Cell.Empty, null)
                    };
                }

                var grid = new List<IReadOnlyList<Cell>>();
                var carts = new List<Cart>();

                for (var row = 0; row < charMap.Rows; row++)
                {
                    var cells = new List<Cell>();

                    for (var col = 0; col < charMap.Cols; col++)
                    {
                        var (cell, cart) = Classify(charMap, new Point(row, col));
                        cells.Add(cell);

                        if (cart != null)
                        {
                            carts.Add(cart);
                        }
                    }

                    grid.Add(cells);
                }

                return (new Map(grid), carts);
            }

            private readonly IReadOnlyList<IReadOnlyList<Cell>> cells;

            public Map(IReadOnlyList<IReadOnlyList<Cell>> cells)
            {
                this.cells = cells;
            }

            public int Rows => this.cells.Count;

            public int Cols => this.cells[0].Count;

            public Cell At(Point p) => cells[p.Row][p.Col];

            public void Draw(IReadOnlyList<Cart> carts)
            {
                for (var row = 0; row < Rows; row++)
                {
                    for (var col = 0; col < Cols; col++)
                    {
                        var pos = new Point(row, col);

                        var cart = carts.FirstOrDefault(c => c.Pos.Equals(pos));

                        var chr = cart != null
                            ? cart.Dir.ToString()
                            : At(pos).ToString();

                        Console.Write(chr);
                    }
                    Console.WriteLine();
                }
            }

            public (IReadOnlyList<Cart> nextCarts, bool isCrashed, Point crashPos) MovePart1(
                IReadOnlyList<Cart> carts)
            {
                var orderedCarts = carts.OrderBy(c => c.Pos.Row).ThenBy(c => c.Pos.Col).ToList();

                var occupiedPositions = orderedCarts.Select(c => c.Pos).ToHashSet();

                var newCarts = new List<Cart>();

                foreach (var cart in orderedCarts)
                {
                    occupiedPositions.Remove(cart.Pos);

                    var newCart = cart.Move(map: this);

                    if (occupiedPositions.Contains(newCart.Pos))
                    {
                        return (carts, true, newCart.Pos);
                    }

                    newCarts.Add(newCart);
                    occupiedPositions.Add(newCart.Pos);
                }

                return (newCarts, false, Point.Origin);
            }

            public IReadOnlyList<Cart> MovePart2(IReadOnlyList<Cart> carts)
            {
                var orderedCarts = carts.OrderBy(c => c.Pos.Row).ThenBy(c => c.Pos.Col).ToList();
                var newCarts = new List<Cart>();

                var i = 0;
                while (i < orderedCarts.Count)
                {
                    var newCart = orderedCarts[i].Move(map: this);

                    var crashed = false;

                    var index = newCarts.IndexOf(c => c.Pos.Equals(newCart.Pos));
                    if (index >= 0)
                    {
                        newCarts.RemoveAt(index);
                        crashed = true;
                    }
                    else
                    {
                        index = orderedCarts.IndexOf(c => c.Pos.Equals(newCart.Pos), startIndex: i + 1);
                        if (index >= 0)
                        {
                            orderedCarts.RemoveAt(index);
                            crashed = true;
                        }
                    }

                    if (!crashed)
                    {
                        newCarts.Add(newCart);
                    }

                    i++;
                }

                return newCarts;
            }
        }
    }
}
