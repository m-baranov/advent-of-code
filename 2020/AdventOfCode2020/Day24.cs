using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Math;

namespace AdventOfCode2020
{
    static class Day24
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "sesenwnenenewseeswwswswwnenewsewsw",
                "neeenesenwnwwswnenewnwwsewnenwseswesw",
                "seswneswswsenwwnwse",
                "nwnwneseeswswnenewneswwnewseswneseene",
                "swweswneswnenwsewnwneneseenw",
                "eesenwseswswnenwswnwnwsewwnwsene",
                "sewnenenenesenwsewnenwwwse",
                "wenwwweseeeweswwwnwwe",
                "wsweesenenewnwwnwsenewsenwwsesesenwne",
                "neeswseenwwswnwswswnw",
                "nenwswwsewswnenenewsenwsenwnesesenew",
                "enewnwewneswsewnwswenweswnenwsenwsw",
                "sweneswneswneneenwnewenewwneswswnese",
                "swwesenesewenwneswnwwneseswwne",
                "enesenwswwswneneswsenwnewswseenwsese",
                "wnwnesenesenenwwnenwsewesewsesesew",
                "nenewswnwewswnenesenwnesewesw",
                "eneswnwswnwsenenwnwnwwseeswneewsenese",
                "neswnwewnwnwseenwseesewsenwsweewe",
                "wseweeenwnesenwwwswnew"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/24/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = new Grid();

                foreach (var line in input.Lines())
                {
                    var coordinate = Coordinate.OfPath(Util.Parse(line));
                    grid.Flip(coordinate);
                }

                var answer = grid.Tiles().Where(t => t.Color == Color.Black).Count();
                Console.WriteLine(answer);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var grid = new Grid();

                foreach (var line in input.Lines())
                {
                    var coordinate = Coordinate.OfPath(Util.Parse(line));
                    grid.Flip(coordinate);
                }

                var current = grid;
                for (var i = 0; i < 100; i++)
                {
                    current = OneDayFlip(current);
                }

                var answer = current.Tiles().Where(t => t.Color == Color.Black).Count();
                Console.WriteLine(answer);
            }

            private static Grid OneDayFlip(Grid current)
            {
                var coordinatesToInspect = current.Tiles()
                    .Where(t => t.Color == Color.Black)
                    .SelectMany(t => t.Coordinate.Neighbours().Append(t.Coordinate))
                    .ToHashSet();

                return Grid.OfBlackTileCoordinates(coordinatesToInspect
                    .Where(c =>
                    {
                        var tile = current.At(c);
                        var count = c.Neighbours().Select(n => current.At(n)).Where(t => t.Color == Color.Black).Count();

                        if (tile.Color == Color.Black)
                        {
                            return !(count == 0 || count > 2);
                        }
                        else
                        {
                            return count == 2;
                        }
                    }));
            }
        }

        public enum Direction { E, SE, SW, W, NW, NE }

        public enum Color { White, Black }

        public class Coordinate
        {
            public static readonly Coordinate Origin = new Coordinate(row: 0, col: 0);

            public static Coordinate OfPath(IEnumerable<Direction> directions) => 
                directions.Aggregate(Origin, (coord, dir) => coord.Neighbour(dir));

            public Coordinate(int row, int col)
            {
                Row = row;
                Col = col;
            }

            public int Row { get; }
            public int Col { get; }

            public override bool Equals(object obj) => 
                obj is Coordinate c ? Row == c.Row && Col == c.Col : false;

            public override int GetHashCode() => 
                HashCode.Combine(Row, Col);

            public Coordinate Neighbour(Direction direction)
            {
                if (direction == Direction.W) return W();
                if (direction == Direction.E) return E();
                if (direction == Direction.NW) return NW();
                if (direction == Direction.NE) return NE();
                if (direction == Direction.SW) return SW();
                return SE();
            }

            public IEnumerable<Coordinate> Neighbours()
            {
                yield return W();
                yield return E();
                yield return NW();
                yield return NE();
                yield return SW();
                yield return SE();
            }

            public Coordinate W() => new Coordinate(Row, Col - 1);

            public Coordinate E() => new Coordinate(Row, Col + 1);

            public Coordinate NE() => new Coordinate(Row + 1, Col + Abs(Row % 2));
            
            public Coordinate NW() => new Coordinate(Row + 1, Col + Abs(Row % 2) - 1);

            public Coordinate SE() => new Coordinate(Row - 1, Col + Abs(Row % 2));

            public Coordinate SW() => new Coordinate(Row - 1, Col + Abs(Row % 2) - 1);
        }

        public class Tile
        {
            public Tile(Coordinate coordinate)
            {
                Coordinate = coordinate;
                Color = Color.White;
            }

            public Coordinate Coordinate { get; }
            public Color Color { get; private set; }

            public void Flip()
            {
                Color = Color == Color.Black ? Color.White : Color.Black;
            }
        }

        public class Grid
        {
            public static Grid OfBlackTileCoordinates(IEnumerable<Coordinate> blackTileCoordinates)
            {
                var grid = new Grid();
                foreach (var coordinate in blackTileCoordinates)
                {
                    grid.Flip(coordinate);
                }
                return grid;
            }

            private readonly Dictionary<Coordinate, Tile> tiles;

            public Grid()
            {
                tiles = new Dictionary<Coordinate, Tile>();
            }

            public Tile At(Coordinate coordinate)
            {
                if (tiles.TryGetValue(coordinate, out var tile))
                {
                    return tile;
                }
                else
                {
                    return new Tile(coordinate);
                }
            }

            public IEnumerable<Tile> Tiles() => tiles.Values;

            public void Flip(Coordinate coordinate)
            {
                Tile tile;
                if (!tiles.TryGetValue(coordinate, out tile))
                {
                    tile = CreateTile(coordinate);
                }

                tile.Flip();
            }

            private Tile CreateTile(Coordinate coordinate)
            {
                var tile = new Tile(coordinate);
                tiles.Add(coordinate, tile);
                return tile;
            }
        }

        public static class Util
        {
            public static IEnumerable<Direction> Parse(string line)
            {
                while (line.Length > 0)
                {
                    int consumed;

                    if (line.StartsWith('e'))
                    {
                        yield return Direction.E;
                        consumed = 1;
                    }
                    else if (line.StartsWith('w'))
                    {
                        yield return Direction.W;
                        consumed = 1;
                    }
                    else if (line.StartsWith("se"))
                    {
                        yield return Direction.SE;
                        consumed = 2;
                    }
                    else if (line.StartsWith("sw"))
                    {
                        yield return Direction.SW;
                        consumed = 2;
                    }
                    else if (line.StartsWith("ne"))
                    {
                        yield return Direction.NE;
                        consumed = 2;
                    }
                    else /* if (line.StartsWith("nw")) */
                    {
                        yield return Direction.NW;
                        consumed = 2;
                    }

                    line = line.Substring(consumed);
                }
            }
        }
    }
}
