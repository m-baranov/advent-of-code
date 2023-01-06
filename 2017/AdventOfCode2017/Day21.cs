using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day21
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "../.# => ##./#../...",
                    ".#./..#/### => #..#/..../..../#..#"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/21/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                const int IterationCount = 5;

                var rules = RuleBook.Parse(input.Lines());

                ITile tile = LiteralTile.Initial();
                tile = Enhance(tile, rules, IterationCount);

                var count = Tile.Count(tile, cell: true);
                Console.WriteLine(count);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                const int IterationCount = 18;

                var rules = RuleBook.Parse(input.Lines());

                ITile tile = LiteralTile.Initial();
                tile = Enhance(tile, rules, IterationCount);

                var count = Tile.Count(tile, cell: true);
                Console.WriteLine(count);
            }
        }

        private static ITile Enhance(ITile tile, RuleBook rules, int iterations)
        {
            for (var i = 0; i < iterations; i++)
            {
                tile = Enhance(tile, rules);
            }
            return tile;
        }

        private static ITile Enhance(ITile tile, RuleBook rules)
        {
            var subTiles = tile.Size % 2 == 0
                ? Tile.Split(tile, size: 2)
                : Tile.Split(tile, size: 3);

            var count = subTiles.GetLength(0);

            for (var row = 0; row < count; row++)
            {
                for (var col = 0; col < count; col++)
                {
                    var subTile = subTiles[row, col];
                    var rule = rules.Find(subTile);
                    subTiles[row, col] = rule.Output;
                }
            }

            return Tile.Materialize(new TileSet(subTiles));
        }

        private static class Tile
        {
            public static ITile[,] Split(ITile tile, int size)
            {
                var count = tile.Size / size;

                var tiles = new ITile[count, count];

                for (var row = 0; row < count; row++)
                {
                    for (var col = 0; col < count; col++)
                    {
                        tiles[row, col] = Slice(tile, (row * size, col * size), size);
                    }
                }

                return tiles;
            }

            public static ITile Slice(ITile tile, (int row, int col) offset, int size) => new SliceTile(tile, offset, size);

            public static bool Equal(ITile x, ITile y)
            {
                if (x.Size != y.Size)
                {
                    return false;
                }

                for (var row = 0; row < x.Size; row++)
                {
                    for (var col = 0; col < x.Size; col++)
                    {
                        if (x.At(row, col) != y.At(row, col))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            public static ITile Materialize(ITile tile)
            {
                var cells = new bool[tile.Size, tile.Size];

                for (var row = 0; row < tile.Size; row++)
                {
                    for (var col = 0; col < tile.Size; col++)
                    {
                        cells[row, col] = tile.At(row, col);
                    }
                }

                return new LiteralTile(cells);
            }

            public static int Count(ITile tile, bool cell)
            {
                var count = 0;

                for (var row = 0; row < tile.Size; row++)
                {
                    for (var col = 0; col < tile.Size; col++)
                    {
                        if (tile.At(row, col) == cell)
                        {
                            count++;
                        }
                    }
                }

                return count;
            }

            public static void Draw(string title, ITile tile)
            {
                Console.WriteLine(title);

                for (var row = 0; row < tile.Size; row++)
                {
                    for (var col = 0; col < tile.Size; col++)
                    {
                        var ch = tile.At(row, col) ? '#' : '.';
                        Console.Write(ch);
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }

            public static IEnumerable<ITile> AllTransforms(ITile tile)
            {
                yield return tile;
                yield return FlipH(tile);
                yield return FlipV(tile);

                yield return Rot90(tile);
                yield return FlipH(Rot90(tile));
                yield return FlipV(Rot90(tile));

                yield return Rot180(tile);
                yield return Rot270(tile);
            }

            public static ITile FlipH(ITile tile) => new HorizontalFlipTile(tile);
            public static ITile FlipV(ITile tile) => new VerticalFlipTile(tile);
            public static ITile Rot90(ITile tile) => new RotateCW90Tile(tile);
            public static ITile Rot180(ITile tile) => new RotateCW180Tile(tile);
            public static ITile Rot270(ITile tile) => new RotateCW270Tile(tile);
        }

        private interface ITile
        {
            public int Size { get; }
            public bool At(int row, int col);
        }

        private sealed class LiteralTile : ITile
        {
            public static LiteralTile Parse(string text)
            {
                var rows = text.Split('/');
                var size = rows.Length;

                var cells = new bool[size, size];

                for (var row = 0; row < size; row++)
                {
                    for (var col = 0; col < size; col++)
                    {
                        var ch = rows[row][col];
                        cells[row, col] = ch == '#';
                    }
                }

                return new LiteralTile(cells);
            }

            public static LiteralTile Initial() => Parse(".#./..#/###");

            private readonly bool[,] cells;

            public LiteralTile(bool[,] cells)
            {
                this.cells = cells;
                this.Size = this.cells.GetLength(0);
            }

            public int Size { get; }
            public bool At(int row, int col) => this.cells[row, col];
        }

        private sealed class HorizontalFlipTile : ITile
        {
            private readonly ITile tile;
            public HorizontalFlipTile(ITile tile) => this.tile = tile;
            public int Size => this.tile.Size;
            public bool At(int row, int col) => this.tile.At(row: Size - 1 - row, col: col);
        }

        private sealed class VerticalFlipTile : ITile
        {
            private readonly ITile tile;
            public VerticalFlipTile(ITile tile) => this.tile = tile;
            public int Size => this.tile.Size;
            public bool At(int row, int col) => this.tile.At(row: row, col: Size - 1 - col);
        }

        private sealed class RotateCW90Tile : ITile
        {
            private readonly ITile tile;
            public RotateCW90Tile(ITile tile) => this.tile = tile;
            public int Size => this.tile.Size;
            public bool At(int row, int col) => this.tile.At(row: Size - 1 - col, col: row);
        }

        private sealed class RotateCW180Tile : ITile
        {
            private readonly ITile tile;
            public RotateCW180Tile(ITile tile) => this.tile = tile;
            public int Size => this.tile.Size;
            public bool At(int row, int col) => this.tile.At(row: Size - 1 - row, col: Size - 1 - col);
        }

        private sealed class RotateCW270Tile : ITile
        {
            private readonly ITile tile;
            public RotateCW270Tile(ITile tile) => this.tile = tile;
            public int Size => this.tile.Size;
            public bool At(int row, int col) => this.tile.At(row: col, col: Size - 1 - row);
        }

        private sealed class SliceTile : ITile
        {
            private readonly ITile tile;
            private readonly (int row, int col) offset;

            public SliceTile(ITile tile, (int row, int col) offset, int size)
            {
                this.tile = tile;
                this.offset = offset;
                this.Size = size;
            }
            
            public int Size { get; }

            public bool At(int row, int col) => this.tile.At(this.offset.row + row, this.offset.col + col);
        }

        private sealed class TileSet : ITile
        {
            private readonly ITile[,] tiles;

            public TileSet(ITile[,] tiles) => this.tiles = tiles;

            public int Size => this.tiles.GetLength(0) * this.TileSize;
            public int TileSize => this.tiles[0, 0].Size;

            public bool At(int row, int col)
            {
                static (int div, int rem) DivRem(int x, int y)
                {
                    var div = Math.DivRem(x, y, out var rem);
                    return (div, rem);
                }

                var (tileRow, cellRow) = DivRem(row, TileSize);
                var (tileCol, cellCol) = DivRem(col, TileSize);

                var tile = this.tiles[tileRow, tileCol];
                return tile.At(cellRow, cellCol);
            }
        }

        private sealed record Rule(ITile Input, ITile Output)
        {
            public static Rule Parse(string text)
            {
                var parts = text.Split(" => ");

                var input = LiteralTile.Parse(parts[0]);
                var output = LiteralTile.Parse(parts[1]);

                return new Rule(input, output);
            }
        }

        private sealed record RuleBook(IReadOnlyList<Rule> Rules)
        {
            public static RuleBook Parse(IEnumerable<string> lines)
            {
                var rules = lines.Select(Rule.Parse).ToList();
                return new RuleBook(rules);
            }

            public Rule Find(ITile input) =>
                Tile.AllTransforms(input)
                    .Select(@in => Rules.FirstOrDefault(r => Tile.Equal(r.Input, @in)))
                    .Where(t => t != null)
                    .First();
        }
    }
}
