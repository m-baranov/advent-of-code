using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AdventOfCode2024;

static class Day05
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
47|53
97|13
97|61
97|47
75|29
61|13
75|53
29|13
97|29
53|29
61|53
97|53
61|29
47|13
75|47
97|75
47|61
75|61
47|29
75|13
53|13

75,47,61,53,29
97,61,53,29,13
75,29,13
75,97,47,61,53
61,13,29
97,13,75,29,47
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2024/day/5/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var inputData = InputData.Parse(input.Lines());

            var sum = 0;
            foreach (var update in inputData.Updates)
            {
                var matchingRules = inputData.Rules
                    .Where(r => update.Numbers.Contains(r.Before) &&
                                update.Numbers.Contains(r.After))
                    .ToList();

                var order = new OrderLookup(matchingRules);

                if (IsCorrectlyOrdered(update, order))
                {
                    sum += update.Numbers[update.Numbers.Count / 2];
                }
            }

            Console.WriteLine(sum);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
             var inputData = InputData.Parse(input.Lines());

            var sum = 0;
            foreach (var update in inputData.Updates)
            {
                var matchingRules = inputData.Rules
                    .Where(r => update.Numbers.Contains(r.Before) &&
                                update.Numbers.Contains(r.After))
                    .ToList();

                var order = new OrderLookup(matchingRules);

                if (!IsCorrectlyOrdered(update, order))
                {
                    var ordered = Order(update, order);

                    sum += ordered.Numbers[ordered.Numbers.Count / 2];
                }
            }

            Console.WriteLine(sum);
        }

        private Update Order(Update update, OrderLookup order)
        {
            var numbers = new List<int>();

            var remaining = update.Numbers.ToList();

            while (remaining.Count > 0)
            {
                var min = remaining.First(num => remaining
                    .Where(rem => rem != num)
                    .All(rem => order.NumbersAfter(num).Contains(rem)));

                numbers.Add(min);
                remaining.Remove(min);
            }

            return new Update(numbers);
        }
    }

    private static bool IsCorrectlyOrdered(Update update, OrderLookup order)
    {
        static IEnumerable<int> ItemsUpToIndex(IReadOnlyList<int> numbers, int index)
        {
            for (var i = 0; i < index; i++)
            {
                yield return numbers[i];
            }
        }

        return update.Numbers
            .Select((number, index) =>
            {
                var afters = order.NumbersAfter(number);
                var befores = ItemsUpToIndex(update.Numbers, index);
                return befores.All(b => !afters.Contains(b));
            })
            .All(correct => correct);
    }

    private class OrderLookup
    {
        private readonly IReadOnlyList<Rule> rules;
        private readonly Dictionary<int, ISet<int>> memo;

        public OrderLookup(IReadOnlyList<Rule> rules)
        {
            this.rules = rules;
            this.memo = new Dictionary<int, ISet<int>>();
        }

        public ISet<int> NumbersAfter(int number)
        {
            if (this.memo.TryGetValue(number, out var cached))
            {
                return cached;
            }

            var afters = new HashSet<int>();

            foreach (var rule in rules.Where(r => r.Before == number))
            {
                afters.Add(rule.After);
                afters.AddRange(NumbersAfter(rule.After));
            }

            this.memo.Add(number, afters);

            return afters;
        }
    }

    private record Rule(int Before, int After)
    {
        public static Rule Parse(string text)
        {
            var parts = text.Split('|');
            return new Rule(int.Parse(parts[0]), int.Parse(parts[1]));
        }
    }

    private record Update(IReadOnlyList<int> Numbers)
    {
        public static Update Parse(string text)
        {
            var parts = text.Split(',');
            var numbers = parts.Select(int.Parse).ToList();
            return new Update(numbers);
        }
    }

    private record InputData(
        IReadOnlyList<Rule> Rules,
        IReadOnlyList<Update> Updates)
    {
        public static InputData Parse(IEnumerable<string> lines)
        {
            var groups = lines.SplitByEmptyLine().ToList();

            var rules = groups[0].Select(Rule.Parse).ToList();
            var updates = groups[1].Select(Update.Parse).ToList();

            return new InputData(rules, updates);
        }
    }
}
