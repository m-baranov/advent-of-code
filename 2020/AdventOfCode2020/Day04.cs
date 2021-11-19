using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode2020
{
    static class Day04
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "ecl:gry pid:860033327 eyr:2020 hcl:#fffffd",
                "byr:1937 iyr:2017 cid:147 hgt:183cm",
                "",
                "iyr:2013 ecl:amb cid:350 eyr:2023 pid:028048884",
                "hcl:#cfa07d byr:1929",
                "",
                "hcl:#ae17e1 iyr:2013",
                "eyr:2024",
                "ecl:brn pid:760753108 byr:1931",
                "hgt:179cm",
                "",
                "hcl:#cfa07d eyr:2025 pid:166559648",
                "iyr:2011 ecl:brn hgt:59in"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2020/day/4/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var count = 0;

                foreach (var lines in Util.Partition(input.Lines()))
                {
                    var dict = Util.ToDictionary(lines);
                    if (IsValid(dict))
                    {
                        count++;
                    }
                }

                Console.WriteLine(count);
            }

            private bool IsValid(IReadOnlyDictionary<string, string> dict)
            {
                var requiredKeys = new[] { "byr", "iyr", "eyr", "hgt", "hcl", "ecl", "pid" };
                return requiredKeys.All(dict.ContainsKey);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var count = 0;

                foreach (var lines in Util.Partition(input.Lines()))
                {
                    var dict = Util.ToDictionary(lines);
                    if (IsValid(dict))
                    {
                        count++;
                    }
                }

                Console.WriteLine(count);
            }

            private bool IsValid(IReadOnlyDictionary<string, string> dict)
            {
                return IsValidByr(dict) && IsValidIyr(dict) && IsValidEyr(dict)
                    && IsValidHgt(dict) && IsValidHcl(dict) && IsValidEcl(dict) 
                    && IsValidPid(dict);
            }

            private bool IsValidByr(IReadOnlyDictionary<string, string> dict)
            {
                if (!dict.TryGetValue("byr", out var byr))
                {
                    return false;
                }
                return IsValidNumber(byr, 1920, 2002);
            }

            private bool IsValidIyr(IReadOnlyDictionary<string, string> dict)
            {
                if (!dict.TryGetValue("iyr", out var iyr))
                {
                    return false;
                }
                return IsValidNumber(iyr, 2010, 2020);
            }

            private bool IsValidEyr(IReadOnlyDictionary<string, string> dict)
            {
                if (!dict.TryGetValue("eyr", out var eyr))
                {
                    return false;
                }
                return IsValidNumber(eyr, 2020, 2030);
            }

            private bool IsValidHgt(IReadOnlyDictionary<string, string> dict)
            {
                if (!dict.TryGetValue("hgt", out var hgt))
                {
                    return false;
                }

                if (hgt.EndsWith("cm"))
                {
                    return IsValidNumber(hgt.Substring(0, hgt.Length - 2), 150, 193);
                }

                if (hgt.EndsWith("in"))
                {
                    return IsValidNumber(hgt.Substring(0, hgt.Length - 2), 59, 76);
                }

                return false;
            }

            private bool IsValidHcl(IReadOnlyDictionary<string, string> dict)
            {
                if (!dict.TryGetValue("hcl", out var hcl))
                {
                    return false;
                }

                var regex = new Regex("^#[0-9a-f]{6}$");
                return regex.IsMatch(hcl);
            }

            private bool IsValidEcl(IReadOnlyDictionary<string, string> dict)
            {
                if (!dict.TryGetValue("ecl", out var ecl))
                {
                    return false;
                }

                var valid = new[] { "amb", "blu", "brn", "gry", "grn", "hzl", "oth" };
                return valid.Contains(ecl);
            }

            private bool IsValidPid(IReadOnlyDictionary<string, string> dict)
            {
                if (!dict.TryGetValue("pid", out var pid))
                {
                    return false;
                }

                var regex = new Regex("^[0-9]{9}$");
                return regex.IsMatch(pid);
            }

            private bool IsValidNumber(string text, int min, int max)
            {
                if (!int.TryParse(text, out var num))
                {
                    return false;
                }

                return min <= num && num <= max;
            }
        }

        private static class Util
        {
            public static IEnumerable<IReadOnlyList<string>> Partition(IEnumerable<string> lines)
            {
                var partition = new List<string>();

                foreach (var line in lines)
                {
                    if (line == string.Empty)
                    {
                        if (partition.Count > 0)
                        {
                            yield return partition;
                        }

                        partition = new List<string>();
                    }
                    else
                    {
                        partition.Add(line);
                    }
                }

                if (partition.Count > 0)
                {
                    yield return partition;
                }
            }

            public static IReadOnlyDictionary<string, string> ToDictionary(IReadOnlyList<string> lines)
            {
                return lines.SelectMany(ParseKeyValues).ToDictionary(p => p.key, p => p.value);
            }

            public static IReadOnlyList<(string key, string value)> ParseKeyValues(string line)
            {
                return line.Split(' ')
                    .Select(pair => 
                    {
                        var index = pair.IndexOf(':');
                        if (index < 0)
                        {
                            return (null, null);
                        }

                        return (pair.Substring(0, index), pair.Substring(index + 1));
                    })
                    .Where(pair => pair.Item1 != null)
                    .ToList();
            }
        }
    }
}
