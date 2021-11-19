using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdventOfCode2020
{
    static class Day20
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "Tile 2311:",
                "..##.#..#.",
                "##..#.....",
                "#...##..#.",
                "####.#...#",
                "##.##.###.",
                "##...#.###",
                ".#.#.#..##",
                "..#....#..",
                "###...#.#.",
                "..###..###",
                "          ",
                "Tile 1951:",
                "#.##...##.",
                "#.####...#",
                ".....#..##",
                "#...######",
                ".##.#....#",
                ".###.#####",
                "###.##.##.",
                ".###....#.",
                "..#.#..#.#",
                "#...##.#..",
                "          ",
                "Tile 1171:",
                "####...##.",
                "#..##.#..#",
                "##.#..#.#.",
                ".###.####.",
                "..###.####",
                ".##....##.",
                ".#...####.",
                "#.##.####.",
                "####..#...",
                ".....##...",
                "          ",
                "Tile 1427:",
                "###.##.#..",
                ".#..#.##..",
                ".#.##.#..#",
                "#.#.#.##.#",
                "....#...##",
                "...##..##.",
                "...#.#####",
                ".#.####.#.",
                "..#..###.#",
                "..##.#..#.",
                "          ",
                "Tile 1489:",
                "##.#.#....",
                "..##...#..",
                ".##..##...",
                "..#...#...",
                "#####...#.",
                "#..#.#.#.#",
                "...#.#.#..",
                "##.#...##.",
                "..##.##.##",
                "###.##.#..",
                "          ",
                "Tile 2473:",
                "#....####.",
                "#..#.##...",
                "#.##..#...",
                "######.#.#",
                ".#...#.#.#",
                ".#########",
                ".###.#..#.",
                "########.#",
                "##...##.#.",
                "..###.#.#.",
                "          ",
                "Tile 2971:",
                "..#.#....#",
                "#...###...",
                "#.#.###...",
                "##.##..#..",
                ".#####..##",
                ".#..####.#",
                "#..#.#..#.",
                "..####.###",
                "..#.#.###.",
                "...#.#.#.#",
                "          ",
                "Tile 2729:",
                "...#.#.#.#",
                "####.#....",
                "..#.#.....",
                "....#..#.#",
                ".##..##.#.",
                ".#.####...",
                "####.#.#..",
                "##.####...",
                "##..#.##..",
                "#.##...##.",
                "          ",
                "Tile 3079:",
                "#.#.#####.",
                ".#..######",
                "..#.......",
                "######....",
                "####.#..#.",
                ".#...#.##.",
                "#.#####.##",
                "..#.###...",
                "..#.......",
                "..#.###..."
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/20/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var sourceTiles = LiteralTile.ParseAll(input.Lines());

                var cornerTiles = sourceTiles
                    .Where(tile => IsCornerTile(tile, sourceTiles.Where(t => t != tile)))
                    .ToList();

                Console.WriteLine("Found {0} corners", cornerTiles.Count);

                var result = cornerTiles.Select(t => t.Id).Aggregate((acc, n) => acc * n);
                Console.WriteLine(result);
            }

            private bool IsCornerTile(ITile tile, IEnumerable<ITile> otherTiles)
            {
                return tile.FlipTransformations()
                    .Any(t => t.Edges().Where(edge => CanConnectTo(edge, otherTiles)).Count() < 3);
            }

            private bool CanConnectTo(Edge edge, IEnumerable<ITile> otherTiles)
            {
                return otherTiles
                    .SelectMany(t => t.FlipTransformations())
                    .SelectMany(t => t.Edges())
                    .Any(e => e.Matches(edge));
            }
        }

        public class Part2 : IProblem
        {
            private static readonly IReadOnlyList<string> Pattern = 
                new[]
                {
                    "                  # ",
                    "#    ##    ##    ###",
                    " #  #  #  #  #  #   ",
                };

            public void Run(TextReader input)
            {
                var sourceTiles = LiteralTile.ParseAll(input.Lines());

                var size = (int)Math.Sqrt(sourceTiles.Count);

                var image = new ITile[size, size];
                var solved = Solve(image, 0, 0, sourceTiles.ToList());
                Console.WriteLine("Solution found: " + solved);
                
                if (!solved)
                {
                    return;
                }

                var combined = CombinedTile.OfImageWithoutFrames(image);

                var result = combined
                    .AllTransformations()
                    .Select(tile => new
                    {
                        tile,
                        patternCoordinates = tile.FindPatternIndexes(Pattern).ToList()
                    })
                    .Where(p => p.patternCoordinates.Count > 0)
                    .First();

                var totalCount = CountSymbols(result.tile.ToLines());
                var patternCount = result.patternCoordinates.Count * CountSymbols(Pattern);
                Console.WriteLine("Final result: " + (totalCount - patternCount));
            }

            private bool Solve(ITile[,] image, int row, int col, List<ITile> tiles)
            {
                var size = image.GetLength(0);

                if (row >= size)
                {
                    return true;
                }

                var rightEdge = col > 0 ? image[row, col - 1].RightEdge() : null;
                var bottomEdge = row > 0 ? image[row - 1, col].BottomEdge() : null;

                var candidates = FindCandidateTiles(tiles, rightEdge, bottomEdge);
                foreach (var (index, candidate) in candidates)
                {
                    var tile = tiles[index];

                    image[row, col] = candidate;
                    tiles[index] = null;

                    var (nextRow, nextCol) = col == size - 1 ? (row + 1, 0) : (row, col + 1);
                    if (Solve(image, nextRow, nextCol, tiles))
                    {
                        return true;
                    }

                    image[row, col] = null;
                    tiles[index] = tile;
                }

                return false;
            }

            private IReadOnlyList<(int, ITile)> FindCandidateTiles(List<ITile> tiles, Edge rightEdge, Edge bottomEdge)
            {
                var results = new List<(int, ITile)>();

                for (var i = 0; i < tiles.Count; i++)
                {
                    var tile = tiles[i];
                    if (tile == null)
                    {
                        continue;
                    }

                    foreach (var candidate in tile.AllTransformations())
                    {
                        var leftMatches = rightEdge == null 
                            ? IsImageLeftEdge(tiles.Where(t => t != null && t != tile), candidate.LeftEdge()) 
                            : candidate.LeftEdge().Matches(rightEdge);

                        var topMatches = bottomEdge == null 
                            ? IsImageTopEdge(tiles.Where(t => t != null && t != tile), candidate.TopEdge())
                            : candidate.TopEdge().Matches(bottomEdge);
                        
                        if (leftMatches && topMatches)
                        {
                            results.Add((i, candidate));
                        }
                    }
                }

                return results;
            }

            private bool IsImageLeftEdge(IEnumerable<ITile> tiles, Edge left)
            {
                return !tiles
                    .SelectMany(t => t.FlipTransformations())
                    .Where(t => t.RightEdge().Matches(left))
                    .Any();
            }

            private bool IsImageTopEdge(IEnumerable<ITile> tiles, Edge top)
            {
                return !tiles
                    .SelectMany(t => t.FlipTransformations())
                    .Where(t => t.BottomEdge().Matches(top))
                    .Any();
            }

            private int CountSymbols(IReadOnlyList<string> lines)
            {
                return lines.SelectMany(l => l).Where(c => c == '#').Count();
            }
        }
    }

    public interface ITile
    {
        long Id { get; }
        string Name { get; }
        int Size { get; }
        char At(int row, int col);
    }

    public class LiteralTile : ITile
    {
        public static IReadOnlyList<ITile> ParseAll(IEnumerable<string> lines)
        {
            var current = new List<string>();
            var tiles = new List<ITile>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    tiles.Add(ParseOne(current));
                    current = new List<string>();
                }
                else
                {
                    current.Add(line);
                }
            }

            tiles.Add(ParseOne(current));

            return tiles.Where(t => t != null).ToList();
        }

        private static ITile ParseOne(IReadOnlyList<string> lines)
        {
            if (lines.Count == 0)
            {
                return null;
            }

            var id = ParseId(lines[0]);
            var contentLines = lines.Skip(1).ToList();

            return new LiteralTile(id, contentLines);
        }

        private static long ParseId(string line)
        {
            // Tile 2311:

            const string start = "Tile ";
            const string end = ":";

            line = line.Substring(start.Length);
            
            var index = line.IndexOf(end);
            line = line.Substring(0, index);

            return long.Parse(line);
        }

        private readonly IReadOnlyList<string> lines;

        public LiteralTile(long id, IReadOnlyList<string> lines)
        {
            Id = id;
            this.lines = lines;
        }

        public long Id { get; }
        public string Name => $"Tile {this.Id}";
        public int Size => this.lines.Count;

        public char At(int row, int col) => lines[row][col];        
    }

    public class HorizontalFlipTile : ITile
    {
        private readonly ITile tile;

        public HorizontalFlipTile(ITile tile)
        {
            this.tile = tile;
        }

        public long Id => this.tile.Id;
        public string Name => $"{this.tile.Name}, FH";
        public int Size => this.tile.Size;

        public char At(int row, int col) => this.tile.At(row: Size - 1 - row, col: col);
    }

    public class VerticalFlipTile : ITile
    {
        private readonly ITile tile;

        public VerticalFlipTile(ITile tile)
        {
            this.tile = tile;
        }

        public long Id => this.tile.Id;
        public string Name => $"{this.tile.Name}, FV";
        public int Size => this.tile.Size;

        public char At(int row, int col) => this.tile.At(row: row, col: Size - 1 - col);
    }

    public class RotateCW90Tile : ITile
    {
        private readonly ITile tile;

        public RotateCW90Tile(ITile tile)
        {
            this.tile = tile;
        }

        public long Id => this.tile.Id;
        public string Name => $"{this.tile.Name}, CW90";
        public int Size => this.tile.Size;

        public char At(int row, int col) => this.tile.At(row: Size - 1 - col, col: row);
    }

    public class RotateCW180Tile : ITile
    {
        private readonly ITile tile;

        public RotateCW180Tile(ITile tile)
        {
            this.tile = tile;
        }

        public long Id => this.tile.Id;
        public string Name => $"{this.tile.Name}, CW180";
        public int Size => this.tile.Size;

        public char At(int row, int col) => this.tile.At(row: Size - 1 - row, col: Size - 1 - col);
    }

    public class RotateCW270Tile : ITile
    {
        private readonly ITile tile;

        public RotateCW270Tile(ITile tile)
        {
            this.tile = tile;
        }

        public long Id => this.tile.Id;
        public string Name => $"{this.tile.Name}, CW270";
        public int Size => this.tile.Size;

        public char At(int row, int col) => this.tile.At(row: col, col: Size - 1 - row);
    }

    public class RemoveFrameTile : ITile
    {
        private readonly ITile tile;

        public RemoveFrameTile(ITile tile)
        {
            this.tile = tile;
        }

        public long Id => tile.Id;
        public string Name => $"{tile.Name}, no frame";
        public int Size => tile.Size - 2;
        public char At(int row, int col) => tile.At(row + 1, col + 1);
    }

    public class CombinedTile : ITile
    {
        public static CombinedTile OfImageWithoutFrames(ITile[,] image)
        {
            var size = image.GetLength(0);
            var withoutFrames = new ITile[size, size];

            for (var row = 0; row < size; row++)
            {
                for (var col = 0; col < size; col++)
                {
                    withoutFrames[row, col] = new RemoveFrameTile(image[row, col]);
                }
            }

            return new CombinedTile(withoutFrames);
        }

        private readonly ITile[,] tiles;

        public CombinedTile(ITile[,] tiles)
        {
            this.tiles = tiles;
        }

        public long Id => -1;
        public string Name => "Combined";
        public int Size => tiles.GetLength(0) * tiles[0, 0].Size;

        public char At(int row, int col)
        {
            var tileSize = tiles[0, 0].Size;

            var rowDiv = Math.DivRem(row, tileSize, out var rowRem);
            var colDiv = Math.DivRem(col, tileSize, out var colRem);

            return tiles[rowDiv, colDiv].At(rowRem, colRem);
        }
    }

    public static class TileExtensions
    {
        public static IEnumerable<Edge> Edges(this ITile tile)
        {
            yield return tile.TopEdge();
            yield return tile.RightEdge();
            yield return tile.BottomEdge();
            yield return tile.LeftEdge();
        }

        public static Edge.Left LeftEdge(this ITile tile) => new Edge.Left(tile);
        public static Edge.Bottom BottomEdge(this ITile tile) => new Edge.Bottom(tile);
        public static Edge.Right RightEdge(this ITile tile) => new Edge.Right(tile);
        public static Edge.Top TopEdge(this ITile tile) => new Edge.Top(tile);

        public static IEnumerable<ITile> FlipTransformations(this ITile tile)
        {
            yield return tile;
            yield return tile.FlipHorizontally();
            yield return tile.FlipVertically();
        }

        public static IEnumerable<ITile> RotateTransformations(this ITile tile)
        {
            yield return tile;
            yield return tile.RotateClockwise90();
            yield return tile.RotateClockwise180();
            yield return tile.RotateClockwise270();
        }

        public static IEnumerable<ITile> AllTransformations(this ITile tile)
        {
            yield return tile;
            yield return tile.FlipHorizontally();
            yield return tile.FlipVertically();

            yield return tile.RotateClockwise90();
            yield return tile.RotateClockwise90().FlipHorizontally();
            yield return tile.RotateClockwise90().FlipVertically();

            yield return tile.RotateClockwise180();
            yield return tile.RotateClockwise270();
        }

        public static ITile FlipHorizontally(this ITile tile) => new HorizontalFlipTile(tile);
        public static ITile FlipVertically(this ITile tile) => new VerticalFlipTile(tile);
        public static ITile RotateClockwise90(this ITile tile) => new RotateCW90Tile(tile);
        public static ITile RotateClockwise180(this ITile tile) => new RotateCW180Tile(tile);
        public static ITile RotateClockwise270(this ITile tile) => new RotateCW270Tile(tile);

        public static IEnumerable<(int row, int col)> FindPatternIndexes(
            this ITile tile,
            IReadOnlyList<string> pattern)
        {
            var tileSize = tile.Size;

            var patternHeight = pattern.Count;
            var patternWidth = pattern[0].Length;

            for (var row = 0; row <= tileSize - patternHeight; row++)
            {
                for (var col = 0; col < tileSize - patternWidth; col++)
                {
                    if (tile.MatchesPattern(pattern, row, col))
                    {
                        yield return (row, col);
                    }
                }
            }
        }

        private static bool MatchesPattern(this ITile tile, IReadOnlyList<string> pattern, int startRow, int startCol)
        {
            var patternHeight = pattern.Count;
            var patternWidth = pattern[0].Length;

            for (var row = 0; row < patternHeight; row++)
            {
                for (var col = 0; col < patternWidth; col++)
                {
                    if (pattern[row][col] == '#' && tile.At(startRow + row, startCol + col) != '#')
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static void Dump(this ITile tile)
        {
            Console.WriteLine(tile.Name);

            for (var row = 0; row < tile.Size; row++)
            {
                for (var col = 0; col < tile.Size; col++)
                {
                    Console.Write(tile.At(row, col));
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public static IReadOnlyList<string> ToLines(this ITile tile)
        {
            var lines = new List<string>();

            for (var row = 0; row < tile.Size; row++)
            {
                var line = new StringBuilder();
                for (var col = 0; col < tile.Size; col++)
                {
                    line.Append(tile.At(row, col));
                }
                lines.Add(line.ToString());
            }

            return lines;
        }
    }

    public abstract class Edge
    {
        private readonly ITile tile;

        public Edge(ITile tile)
        {
            this.tile = tile;
        }

        public int Length => this.tile.Size;

        public abstract char At(int index);

        public bool Matches(Edge other)
        {
            if (this.Length != other.Length)
            {
                return false;
            }

            for (var i = 0; i < this.Length; i++)
            {
                if (this.At(i) != other.At(i))
                {
                    return false;
                }
            }

            return true;
        }

        public class Top : Edge
        {
            public Top(ITile tile) : base(tile) { }
            public override char At(int index) => this.tile.At(row: 0, col: index);
        }

        public class Bottom : Edge
        {
            public Bottom(ITile tile) : base(tile) { }
            public override char At(int index) => this.tile.At(row: this.Length - 1, col: index);
        }

        public class Left : Edge
        {
            public Left(ITile tile) : base(tile) { }
            public override char At(int index) => this.tile.At(row: index, col: 0);
        }

        public class Right : Edge
        {
            public Right(ITile tile) : base(tile) { }
            public override char At(int index) => this.tile.At(row: index, col: this.Length - 1);
        }
    }
}
