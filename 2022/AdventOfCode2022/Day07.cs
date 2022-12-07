using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2022
{
    static class Day07
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "$ cd /",
                    "$ ls",
                    "dir a",
                    "14848514 b.txt",
                    "8504156 c.dat",
                    "dir d",
                    "$ cd a",
                    "$ ls",
                    "dir e",
                    "29116 f",
                    "2557 g",
                    "62596 h.lst",
                    "$ cd e",
                    "$ ls",
                    "584 i",
                    "$ cd ..",
                    "$ cd ..",
                    "$ cd d",
                    "$ ls",
                    "4060174 j",
                    "8033020 d.log",
                    "5626152 d.ext",
                    "7214296 k"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2022/day/7/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var commands = Command.ParseAll(input.Lines().ToList());

                var root = Command.Evaluate(commands);
                var sizes = Entry.Directory.CalculateSizes(root);

                var sum = sizes
                    .Where(p => p.Value <= 100000)
                    .Select(p => p.Value)
                    .Sum();

                Console.WriteLine(sum);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var commands = Command.ParseAll(input.Lines().ToList());

                var root = Command.Evaluate(commands);
                var sizes = Entry.Directory.CalculateSizes(root);

                const long TotalSpace = 70_000_000;
                const long RequiredSpace = 30_000_000;

                var usedSpace = sizes[root];
                var freeSpace = TotalSpace - usedSpace;
                var missingSpace = RequiredSpace - freeSpace;

                var minSize = sizes.Values
                    .Where(s => s >= missingSpace)
                    .Min();

                Console.WriteLine(minSize);
            }
        }

        private abstract class Command
        {
            public static IReadOnlyList<Command> ParseAll(IReadOnlyList<string> lines)
            {
                var commands = new List<Command>();

                var index = 0; 
                while (index < lines.Count)
                {
                    var (command, nextIndex) = Consume(lines, index);
                    commands.Add(command);
                    index = nextIndex;
                }

                return commands;
            }

            private static (Command command, int nextIndex) Consume(IReadOnlyList<string> lines, int index)
            {
                var line = lines[index];
                if (line.StartsWith("$ ls"))
                {
                    return ConsumeListCommand(lines, index + 1);
                }
                else
                {
                    return ConsumeChangeCommand(lines, index);
                }
            }

            private static (Command command, int nextIndex) ConsumeListCommand(IReadOnlyList<string> lines, int index)
            {
                var entries = new List<Entry>();

                while (index < lines.Count)
                {
                    var line = lines[index];
                    if (line.StartsWith("$"))
                    {
                        break;
                    }

                    entries.Add(ParseEntry(line));
                    index++;
                }

                var command = new Command.List(entries);
                return (command, index);
            }

            private static Entry ParseEntry(string line)
            {
                var parts = line.Split(' ');
                if (parts[0] == "dir")
                {
                    return new Entry.Directory(parts[1]);
                }
                else
                {
                    var size = long.Parse(parts[0]);
                    return new Entry.File(parts[1], size);
                }
            }

            private static (Command command, int nextIndex) ConsumeChangeCommand(IReadOnlyList<string> lines, int index)
            {
                var prefix = "$ cd ";
                var target = lines[index].Substring(prefix.Length);

                Command command = target switch
                {
                    "/" => new Command.ChangeRoot(),
                    ".." => new Command.ChangeUp(),
                    var dir => new Command.Change(dir)
                };

                return (command, index + 1);
            }

            public static Entry.Directory Evaluate(IReadOnlyList<Command> commands)
            {
                var root = new Entry.Directory("");

                var path = new Stack<Entry.Directory>();
                path.Push(root);

                foreach (var command in commands)
                {
                    if (command is Command.ChangeRoot)
                    {
                        path.Clear();
                        path.Push(root);
                    }
                    else if (command is Command.ChangeUp)
                    {
                        path.Pop();
                    }
                    else if (command is Command.Change cd)
                    {
                        var current = path.Peek();
                        path.Push(current.FindDir(cd.Dir));
                    }
                    else if (command is Command.List ls)
                    {
                        var current = path.Peek();
                        current.AddRange(ls.Entries);
                    }
                }

                return root;
            }

            public class ChangeUp : Command { }

            public class ChangeRoot : Command { }

            public class Change : Command
            {
                public Change(string dir)
                {
                    Dir = dir;
                }

                public string Dir { get; }
            }

            public class List : Command
            {
                public List(IReadOnlyList<Entry> entries)
                {
                    Entries = entries;
                }

                public IReadOnlyList<Entry> Entries { get; }
            }
        }

        private abstract class Entry
        {
            public class File : Entry
            {
                public File(string name, long size)
                {
                    Name = name;
                    Size = size;
                }

                public string Name { get; }
                public long Size { get; }
            }

            public class Directory : Entry
            {
                public static IReadOnlyDictionary<Entry.Directory, long> CalculateSizes(Entry.Directory root)
                {
                    static void Recurse(Entry.Directory dir, Dictionary<Entry.Directory, long> sizes)
                    {
                        foreach (var subdir in dir.Directories)
                        {
                            Recurse(subdir, sizes);
                        }

                        var fileSize = dir.Files.Select(f => f.Size).Sum();
                        var dirSize = dir.Directories.Select(d => sizes[d]).Sum();
                        sizes[dir] = fileSize + dirSize;
                    }

                    var sizes = new Dictionary<Entry.Directory, long>();
                    Recurse(root, sizes);
                    return sizes;
                }

                private readonly Dictionary<string, Entry.File> files;
                private readonly Dictionary<string, Entry.Directory> directories;

                public Directory(string name)
                {
                    Name = name;

                    this.files = new Dictionary<string, File>();
                    this.directories = new Dictionary<string, Directory>();
                }

                public string Name { get; }

                public IReadOnlyCollection<File> Files => this.files.Values;

                public IReadOnlyCollection<Directory> Directories => this.directories.Values;

                public void AddRange(IReadOnlyList<Entry> entries)
                {
                    foreach (var entry in entries)
                    {
                        if (entry is Entry.Directory dir)
                        {
                            this.directories[dir.Name] = dir;
                        }
                        else if (entry is Entry.File file)
                        {
                            this.files[file.Name] = file;
                        }
                    }
                }

                public Directory FindDir(string dir) => this.directories[dir];
            }
        }
    }
}
