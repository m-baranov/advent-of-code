﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode2024
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T first, T second)> Pairs<T>(this IEnumerable<T> items)
        {
            return items.Zip(items.Skip(1), (first, second) => (first, second));
        }

        public static IEnumerable<(T first, T second, T third)> Threes<T>(this IEnumerable<T> items)
        {
            return items
                .Zip(items.Skip(1), (a, b) => (a, b))
                .Zip(items.Skip(2), (ab, c) => (ab.a, ab.b, c));
        }

        public static int IndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            var index = 0;
            foreach (var item in items)
            {
                if (predicate(item))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public static IReadOnlyDictionary<TKey, int> CountBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> selector)
            where TKey : notnull
        {
            var counts = new Dictionary<TKey, int>();

            foreach (var item in items)
            {
                var key = selector(item);
                if (counts.TryGetValue(key, out var count))
                {
                    counts[key] = count + 1;
                }
                else
                {
                    counts.Add(key, 1);
                }
            }

            return counts;
        }

        public static IEnumerable<IReadOnlyList<int>> AllPossibleOrders(IReadOnlyList<int> numbers)
        {
            IEnumerable<IReadOnlyList<int>> AppendUnique(IEnumerable<IReadOnlyList<int>> options, IEnumerable<int> append)
            {
                return options.SelectMany(option => append.Except(option).Select(n => option.Append(n).ToList()));
            }

            var options = numbers.Select(n => new[] { n }).Cast<IReadOnlyList<int>>();
            for (var i = 0; i < numbers.Count - 1; i++)
            {
                options = AppendUnique(options, numbers);
            }

            return options;
        }

        public static IEnumerable<(T first, T second)> AllPossiblePairs<T>(this IEnumerable<T> items)
        {
            return items.SelectMany((first, index) => items.Skip(index + 1).Select(second => (first, second)));
        }

        public static IEnumerable<IReadOnlyList<string>> SplitByEmptyLine(this IEnumerable<string> lines) =>
            lines.SplitBy(string.IsNullOrEmpty);

        public static IEnumerable<IReadOnlyList<string>> SplitBy(
            this IEnumerable<string> lines,
            Func<string, bool> predicate)
        {
            var partition = new List<string>();

            foreach (var line in lines)
            {
                if (predicate(line))
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

        public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                queue.Enqueue(item);
            }
        }

        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                set.Add(item);
            }
        }

        public static void RemoveRange<T>(this HashSet<T> set, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                set.Remove(item);
            }
        }

        public static IEnumerable<int> Sequence(int start, int delta)
        {
            var i = start;
            while (true)
            {
                yield return i;
                i += delta;
            }
        }

        public static IEnumerable<int> Between(int start, int end)
        {
            IEnumerable<int> between(int start, int end)
            {
                for (var i = start; i <= end; i++)
                {
                    yield return i;
                }
            }

            return start <= end
                ? between(start, end)
                : between(end, start);
        }
    }

    public static class MathExtensions
    {
        public static long Gcd(long a, long b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a | b;
        }

        public static long Lcm(long a, long b) => Math.Abs(a * b) / Gcd(a, b);

        public static long Lcm(IReadOnlyList<long> ns) => ns.Aggregate(Lcm);
    }
}
