using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode2021
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

        public static T MaxBy<T, V>(this IEnumerable<T> items, Func<T, V> selector, IComparer<V> comparer = null)
        {
            comparer = comparer ?? Comparer<V>.Default;

            var maxItemSet = false;
            var maxItem = default(T);
            var maxItemValue = default(V);
            foreach (var item in items)
            {
                if (!maxItemSet)
                {
                    maxItem = item;
                    maxItemValue = selector(item);
                    maxItemSet = true;
                    continue;
                }

                var itemValue = selector(item);
                if (comparer.Compare(maxItemValue, itemValue) < 0)
                {
                    maxItem = item;
                    maxItemValue = itemValue;
                }
            }

            return maxItem;
        }

        public static T MinBy<T, V>(this IEnumerable<T> items, Func<T, V> selector, IComparer<V> comparer = null)
        {
            comparer = comparer ?? Comparer<V>.Default;

            var minItemSet = false;
            var minItem = default(T);
            var minItemValue = default(V);
            foreach (var item in items)
            {
                if (!minItemSet)
                {
                    minItem = item;
                    minItemValue = selector(item);
                    minItemSet = true;
                    continue;
                }

                var itemValue = selector(item);
                if (comparer.Compare(itemValue, minItemValue) < 0)
                {
                    minItem = item;
                    minItemValue = itemValue;
                }
            }

            return minItem;
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

        public static IEnumerable<IReadOnlyList<T>> Chunk<T>(this IEnumerable<T> items, int chunkSize)
        {
            var chunk = new List<T>(capacity: chunkSize);

            foreach (var item in items)
            {
                chunk.Add(item);
                if (chunk.Count == chunkSize)
                {
                    yield return chunk;
                    chunk = new List<T>(capacity: chunkSize);
                }
            }

            if (chunk.Count > 0)
            {
                yield return chunk;
            }
        }

        public static IEnumerable<IReadOnlyList<string>> SplitByEmptyLine(this IEnumerable<string> lines)
        {
            var partition = new List<string>();

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
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
    }
}
