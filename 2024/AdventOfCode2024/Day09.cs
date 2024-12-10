using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;

namespace AdventOfCode2024;

static class Day09
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
2333133121414131402
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/9/input");
    }

    public class Part1 : IProblem
    {
        // 6359829300160 - low
        public void Run(TextReader input)
        {
            var diskMap = new DiskMap(input.Lines().First());

            var forward = diskMap.EnumerateFileIdsForward().GetEnumerator();
            var backward = diskMap.EnumerateFileIdsBackward().GetEnumerator();

            var fi = -1;
            var bi = diskMap.Size();
            var sum = 0L;

            while (true)
            {
                forward.MoveNext();
                fi++;

                if (fi >= bi)
                {
                    break;
                }

                if (forward.Current is not null)
                {
                    sum += fi * forward.Current.Value;
                }
                else
                {
                    do
                    {
                        backward.MoveNext();
                        bi--;
                    } while (backward.Current is null);

                    if (fi >= bi)
                    {
                        break;
                    }

                    sum += fi * backward.Current.Value;
                }
            }
            Console.WriteLine(sum);
        }

        private class DiskMap
        {
            private readonly string map;

            public DiskMap(string map)
            {
                this.map = map;
            }

            public int Size()
            {
                return this.map.Select(CharToInt).Sum();
            }

            public IEnumerable<int?> EnumerateFileIdsForward()
            {
                var fileId = 0;
                var isFile = true;

                for (var i = 0; i < this.map.Length; i++)
                {
                    var length = CharToInt(this.map[i]);

                    if (isFile)
                    {
                        for (var j = 0; j < length; j++)
                        {
                            yield return fileId;
                        }
                    }
                    else
                    {
                        for (var j = 0; j < length; j++)
                        {
                            yield return null;
                        }
                    }

                    if (isFile)
                    {
                        fileId++;
                    }
                    isFile = !isFile;
                }
            }

            public IEnumerable<int?> EnumerateFileIdsBackward()
            {
                var (div, rem) = Math.DivRem(this.map.Length, 2);

                var fileId = div + rem - 1;
                var isFile = rem == 1;

                for (var i = this.map.Length - 1; i >= 0; i--)
                {
                    var length = CharToInt(this.map[i]);

                    if (isFile)
                    {
                        for (var j = 0; j < length; j++)
                        {
                            yield return fileId;
                        }
                    }
                    else
                    {
                        for (var j = 0; j < length; j++)
                        {
                            yield return null;
                        }
                    }

                    if (isFile)
                    {
                        fileId--;
                    }
                    isFile = !isFile;
                }
            }

            private static int CharToInt(char ch) => ch - '0';
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var diskMap = DiskMap.Parse(input.Lines().First());

            Defragment(diskMap);

            var sum = CalculateCheckSum(diskMap);
            Console.WriteLine(sum);
        }

        private static void Defragment(DiskMap diskMap)
        {
            var fileId = diskMap.LastFileId();
            while (fileId > 0)
            {
                var fileBlock = diskMap.FindFileBlock(fileId);
                var spaceBlock = diskMap.FindFirstSpaceFor(fileBlock);

                if (spaceBlock is not null)
                {
                    diskMap.Move(fileBlock, spaceBlock);
                }

                fileId--;
            }
        }

        private long CalculateCheckSum(DiskMap diskMap)
        {
            var sum = 0L;
            var index = 0;
            foreach (var fileId in diskMap.EnumerateFileIds())
            {
                if (fileId is not null)
                {
                    sum += fileId.Value * index;
                }

                index++;
            }
            return sum;
        }

        private class DiskMap
        {
            public static DiskMap Parse(string text)
            {
                static int CharToInt(char ch) => ch - '0';

                var fileId = 0;
                var isFile = true;

                Block? start = null;
                Block? end = null;

                for (var i = 0; i < text.Length; i++)
                {
                    var block = new Block()
                    {
                        length = CharToInt(text[i]),
                        fileId = isFile ? fileId : null,
                        next = null,
                        prev = null,
                    };

                    if (start is null)
                    {
                        start = block;
                        end = block;
                    }
                    else
                    {
                        block.prev = end;
                        end!.next = block;
                        end = block;
                    }

                    if (isFile)
                    {
                        fileId++;
                    }
                    isFile = !isFile;
                }

                return new DiskMap(start!, end!);
            }

            private Block start;
            private Block end;

            private DiskMap(Block start, Block end)
            {
                this.start = start;
                this.end = end;
            }

            public int LastFileId()
            {
                var block = this.end;
                while (block!.fileId is null)
                {
                    block = block.prev;
                }

                return block.fileId.Value;
            }

            public Block FindFileBlock(int fileId)
            {
                var block = this.end;
                while (block!.fileId != fileId)
                {
                    block = block.prev;
                }

                return block;
            }

            public Block? FindFirstSpaceFor(Block fileBlock)
            {
                var block = this.start;
                while (block is not null)
                {
                    if (block == fileBlock)
                    {
                        return null;
                    }

                    if (block.fileId is null && block.length >= fileBlock.length)
                    {
                        return block;
                    }
                    block = block.next;
                }

                return null;
            }

            public void Move(Block file, Block space)
            {
                Cut(file);
                Paste(file, space);
            }

            private void Paste(Block file, Block space)
            {
                if (file.length > space.length)
                {
                    throw new InvalidOperationException();
                }

                InsertBefore(file, space);

                if (file.length == space.length)
                {
                    Remove(space);
                }
                else
                {
                    space.length -= file.length;
                }
            }

            private void Cut(Block file)
            {
                var prevFree = file.prev is not null && file.prev.fileId is null;
                var nextFree = file.next is not null && file.next.fileId is null;

                if (prevFree && nextFree)
                {
                    file.prev!.length += file.next!.length + file.length;

                    Remove(file.next!);
                    Remove(file);
                }
                else if (prevFree && !nextFree)
                {
                    file.prev!.length += file.length;

                    Remove(file);
                }
                else if (!prevFree && nextFree)
                {
                    var space = new Block()
                    {
                        length = file.length + file.next!.length,
                        fileId = null,
                    };
                    InsertBefore(space, file);
                    Remove(file.next!);
                    Remove(file);
                }
                else if (!prevFree && !nextFree)
                {
                    var space = new Block()
                    {
                        length = file.length,
                        fileId = null,
                    };
                    InsertBefore(space, file);
                    Remove(file);
                }
            }

            private void InsertBefore(Block block, Block before)
            {
                var prev = before.prev;

                before.prev = block;
                block.next = before;

                if (prev is not null)
                {
                    prev.next = block;
                }
                block.prev = prev;

                if (before == this.start)
                {
                    this.start = block;
                }
            }

            private void Remove(Block block)
            {
                if (block.prev is not null)
                {
                    block.prev.next = block.next;
                }
                if (block.next is not null)
                {
                    block.next.prev = block.prev;
                }

                if (block == this.start)
                {
                    this.start = this.start!.next!;
                }
                if (block == this.end)
                {
                    this.end = this.end!.prev!;
                }
            }

            public IEnumerable<int?> EnumerateFileIds()
            {
                var block = this.start;

                while (block is not null)
                {
                    for (var i = 0; i < block.length; i++)
                    {
                        yield return block.fileId;
                    }
                    block = block.next;
                }
            }

            public class Block
            {
                public int length;
                public int? fileId;

                public Block? next;
                public Block? prev;
            }
        }
    }
}
