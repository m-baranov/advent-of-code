using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode2019
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T, T)> Pairwise<T>(this IEnumerable<T> items)
        {
            return items.Zip(items.Skip(1), (first, second) => (first, second));
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

        public static IEnumerable<IReadOnlyList<int>> AllPossibleOrders(int count) =>
            AllPossibleOrders(Enumerable.Range(0, count).ToList());

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

        public static IEnumerable<IReadOnlyList<T>> AllPossibleCombinations<T>(IReadOnlyList<IReadOnlyList<T>> choices)
        {
            static IEnumerable<IReadOnlyList<T>> Combine<T>(IEnumerable<IReadOnlyList<T>> xs, IReadOnlyList<T> ys)
            {
                return from x in xs
                       from y in ys
                       select x.Append(y).ToList();
            }

            if (choices.Count == 0)
            {
                return Array.Empty<IReadOnlyList<T>>();
            }

            var first = choices.First().Select(c => (IReadOnlyList<T>)(new[] { c }));
            if (choices.Count == 1)
            {
                return first;
            }

            return choices.Skip(1).Aggregate(first, (acc, next) => Combine(acc, next));
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
    }

    public static class DictionaryExtensions
    {
        public static TValue GetOrDefault<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> dict, 
            TKey key, 
            TValue defaultValue)
        {
            return dict.TryGetValue(key, out var value) ? value : defaultValue;
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
