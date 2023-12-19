using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day19
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
px{a<2006:qkq,m>2090:A,rfg}
pv{a>1716:R,A}
lnx{m>1548:A,A}
rfg{s<537:gd,x>2440:R,A}
qs{s>3448:A,lnx}
qkq{x<1416:A,crn}
crn{x>2662:A,R}
in{s<1351:px,qqz}
qqz{s>2770:qs,m<1801:hdj,R}
gd{a>3333:R,R}
hdj{m>838:A,pv}

{x=787,m=2655,a=1222,s=2876}
{x=1679,m=44,a=2067,s=496}
{x=2036,m=264,a=79,s=2244}
{x=2461,m=1339,a=466,s=291}
{x=2127,m=1623,a=2188,s=1013}
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/19/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var (workflows, parts) = ParseInput(input.Lines());

            var workflowByName = workflows.ToDictionary(w => w.Name);

            var sum = parts
                .Where(part => EvalIsAccepted(part, workflowByName))
                .Select(part => (long)part.SumRatings())
                .Sum();

            Console.WriteLine(sum);
        }

        private static bool EvalIsAccepted(
            Part part,
            IReadOnlyDictionary<string, Workflow> workflowByName)
        {
            var workflow = workflowByName["in"];
            while (true)
            {
                var action = Eval(part, workflow);

                if (action is RuleAction.Accept)
                {
                    return true;
                }
                else if (action is RuleAction.Reject)
                {
                    return false;
                }
                else if (action is RuleAction.GoToWorkflow goToWorkflow)
                {
                    workflow = workflowByName[goToWorkflow.WorkflowName];
                }
                else
                {
                    throw new Exception("impossible");
                }
            }
        }

        private static RuleAction Eval(Part part, Workflow workflow)
        {
            foreach (var rule in workflow.Rules)
            {
                if (Matches(part, rule.Condition))
                {
                    return rule.Action;
                }
            }

            throw new Exception("impossible");
        }

        private static bool Matches(Part part, RuleCondition condition)
        {
            if (condition is RuleCondition.None)
            {
                return true;
            }
            if (condition is RuleCondition.Comparison comparison)
            {
                return MatchesComparison(part, comparison);
            }
            throw new Exception("impossible");
        }

        private static bool MatchesComparison(Part part, RuleCondition.Comparison comparison)
        {
            var left = part.GetRating(comparison.Category);
            var right = comparison.Value;

            return comparison.Operator switch
            {
                Operator.Lt => left < right,
                Operator.Gt => left > right,

                _ => throw new Exception("impossible"),
            };
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var (workflows, _) = ParseInput(input.Lines());

            var root = BuildGraph(workflows);
            var conditions = FindAcceptConditions(root);

            var domain = new Range(Start: 1, End: 4000 + 1);
            var sum = conditions
                .Select(bounds => PossibleCombinatinons(bounds, domain))
                .Sum();

            Console.WriteLine(sum);
        }

        private static Node BuildGraph(IReadOnlyList<Workflow> workflows)
        {
            var workflowByName = workflows.ToDictionary(w => w.Name);
            return BuildNode(workflowByName, workflowName: "in");
        }

        private static Node BuildNode(
            IReadOnlyDictionary<string, Workflow> workflowByName, 
            string workflowName)
        {
            static BoundType ConvertToType(Operator op) =>
                op switch
                {
                    Operator.Lt => BoundType.Lt,
                    Operator.Gt => BoundType.Gt,

                    _ => throw new Exception("impossible"),
                };

            static BoundType ConvertToTypeNegated(Operator op) =>
                op switch
                {
                    Operator.Lt => BoundType.GtEq,
                    Operator.Gt => BoundType.LtEq,

                    _ => throw new Exception("impossible"),
                };

            static Bound CreateBound(RuleCondition.Comparison condition) =>
                new(
                    condition.Category, 
                    ConvertToType(condition.Operator), 
                    condition.Value
                );

            static Bound CreateBoundNegated(RuleCondition.Comparison condition) =>
                new(
                    condition.Category,
                    ConvertToTypeNegated(condition.Operator),
                    condition.Value
                );

            var workflow = workflowByName[workflowName];

            var negatedBounds = new List<Bound>();
            var edges = new List<Edge>();

            foreach (var rule in workflow.Rules)
            {
                List<Bound> bounds = negatedBounds.ToList();

                if (rule.Condition is RuleCondition.Comparison comparison)
                {
                    bounds.Add(CreateBound(comparison));
                    negatedBounds.Add(CreateBoundNegated(comparison));
                }

                bool? accepted = null;
                Node? node = null;
                if (rule.Action is RuleAction.GoToWorkflow goToWorkflow)
                {
                    node = BuildNode(workflowByName, goToWorkflow.WorkflowName);
                }
                else
                {
                    accepted = rule.Action is RuleAction.Accept;
                }
                
                edges.Add(new Edge(bounds, node, accepted));
            }

            return new Node(workflow.Name, edges);
        }

        private static IReadOnlyList<IReadOnlyList<Bound>> FindAcceptConditions(Node root)
        {
            static void Traverse(Node node, List<Edge> path, List<IReadOnlyList<Bound>> results)
            {
                foreach (var edge in node.Edges)
                {
                    path.Add(edge);

                    if (edge.Accepted == true)
                    {
                        var bounds = path.SelectMany(e => e.Bounds).ToList();
                        results.Add(bounds);
                    }
                    else if (edge.Node is not null)
                    {
                        Traverse(edge.Node, path, results);
                    }

                    path.RemoveAt(path.Count - 1);
                }
            }

            var path = new List<Edge>();
            var results = new List<IReadOnlyList<Bound>>();
            Traverse(root, path, results);

            return results;
        }

        private static long PossibleCombinatinons(IReadOnlyList<Bound> bounds, Range domain)
        {
            var combinations = bounds
                .GroupBy(b => b.Category)
                .Select(g => (
                    key: g.Key, 
                    count: PossibleCombinatinonsOneCategory(g.ToList(), domain)
                ))
                .ToDictionary(g => g.key, g => g.count);

            return CategoryUtil.AllCategories
                .Select(category => combinations.TryGetValue(category, out var value)
                    ? value
                    : domain.Length()
                )
                .Aggregate(1L, (acc, num) => acc * num);
        }

        private static long PossibleCombinatinonsOneCategory(IReadOnlyList<Bound> bounds, Range domain)
        {
            static Range ConvertToRange(Bound bound, Range domain) =>
                bound.Type switch
                {
                    BoundType.Lt => new Range(domain.Start, bound.Value),
                    BoundType.LtEq => new Range(domain.Start, bound.Value + 1),
                    BoundType.Gt => new Range(bound.Value + 1, domain.End),
                    BoundType.GtEq => new Range(bound.Value, domain.End),

                    _ => throw new Exception("impossible"),
                };

            var ranges = bounds
                .Select(bound => ConvertToRange(bound, domain))
                .Where(range => range.Start < range.End)
                .ToList();

            var maxStart = ranges.Select(r => r.Start).Max();
            var minEnd = ranges.Select(r => r.End).Min();
            return maxStart < minEnd ? new Range(maxStart, minEnd).Length() : 0;
        }

        private enum BoundType { Lt, LtEq, Gt, GtEq }

        private record Bound(Category Category, BoundType Type, int Value)
        {
            public override string ToString()
            {
                static string CategoryToString(Category category) =>
                    category switch
                    {
                        Category.X => "X",
                        Category.M => "M",
                        Category.A => "A",
                        Category.S => "S",
                        _ => "?"
                    };

                static string TypeToString(BoundType type) =>
                    type switch
                    {
                        BoundType.Lt => "<",
                        BoundType.LtEq => "<=",
                        BoundType.Gt => ">",
                        BoundType.GtEq => ">=",
                        _ => "?"
                    };

                return $"{CategoryToString(Category)} {TypeToString(Type)} {Value}";
            }
        }

        private record Edge(IReadOnlyList<Bound> Bounds, Node? Node, bool? Accepted);

        private record Node(string WorkflowName, IReadOnlyList<Edge> Edges);

        private record Range(int Start, int End)
        {
            public long Length() => End - Start;

            public override string ToString() => $"[{Start},{End})";
        }
    }

    private enum Category { X, M, A, S }

    private enum Operator { Lt, Gt }

    private static class CategoryUtil
    {
        public static readonly IReadOnlyList<Category> AllCategories = 
            new[]
            {
                Category.X,
                Category.M,
                Category.A,
                Category.S,
            };

        public static Category Parse(string text) =>
            text switch
            {
                "x" => Category.X,
                "m" => Category.M,
                "a" => Category.A,
                "s" => Category.S,

                _ => throw new Exception("impossible"),
            };
    }

    private abstract class RuleCondition
    {
        public static RuleCondition Parse(string text)
        {
            static (Operator @operator, string operatorText) DetectOperator(string text)
            {
                if (text.Contains('>'))
                {
                    return (Operator.Gt, ">");
                }
                if (text.Contains('<'))
                {
                    return (Operator.Lt, "<");
                }
                throw new Exception("impossible");
            }

            if (text == string.Empty)
            {
                return None.Instance;
            }

            var (@operator, operatorText) = DetectOperator(text);
            var (categoryText, valueText) = SplitBy(text, operatorText);

            var category = CategoryUtil.Parse(categoryText);
            var value = int.Parse(valueText);

            return new Comparison(category, @operator, value);
        }

        public sealed class None : RuleCondition
        {
            public static readonly None Instance = new();
            public None() { }
        }

        public sealed class Comparison : RuleCondition
        {
            public Comparison(Category category, Operator @operator, int value)
            {
                Category = category;
                Operator = @operator;
                Value = value;
            }

            public Category Category { get; }
            public Operator Operator { get; }
            public int Value { get; }
        }
    }

    private abstract class RuleAction
    {
        public static RuleAction Parse(string text) =>
            text switch
            {
                "A" => Accept.Instance,
                "R" => Reject.Instance,
                var workflow => new GoToWorkflow(workflow)
            };
        
        public sealed class Accept : RuleAction
        {
            public static readonly Accept Instance = new();
            private Accept() { }
        }

        public sealed class Reject : RuleAction
        {
            public static readonly Reject Instance = new();
            private Reject() { }
        }

        public sealed class GoToWorkflow : RuleAction
        {
            public GoToWorkflow(string workflowName)
            {
                WorkflowName = workflowName;
            }

            public string WorkflowName { get; }
        }
    }

    private record Rule(RuleCondition Condition, RuleAction Action)
    {
        public static Rule Parse(string text)
        {
            var (conditionText, actionText) = text.Contains(':')
                ? SplitBy(text, ":")
                : (string.Empty, text);

            var condition = RuleCondition.Parse(conditionText);
            var action = RuleAction.Parse(actionText);

            return new Rule(condition, action);
        }
    }

    private record Workflow(string Name, IReadOnlyList<Rule> Rules)
    {
        public static Workflow Parse(string text)
        {
            var (name, rulesText) = SplitBy(text, "{");

            var rules = TrimSuffix(rulesText, "}")
                .Split(',')
                .Select(Rule.Parse)
                .ToList();

            return new Workflow(name, rules);
        }
    }

    private record Part(IReadOnlyDictionary<Category, int> Ratings)
    {
        public static Part Parse(string text)
        {
            static (Category category, int rating) ParseRating(string text)
            {
                var (categoryText, ratingText) = SplitBy(text, "=");

                var category = CategoryUtil.Parse(categoryText);
                var rating = int.Parse(ratingText);

                return (category, rating);
            }

            var ratings = TrimSuffix(TrimPrefix(text, "{"), "}")
                .Split(',')
                .Select(ParseRating)
                .ToDictionary(p => p.category, p => p.rating);

            return new Part(ratings);
        }

        public int GetRating(Category category) =>
            Ratings.TryGetValue(category, out var rating) ? rating : 0;

        public int SumRatings() => Ratings.Values.Sum();
    }

    private static (IReadOnlyList<Workflow> workflows, IReadOnlyList<Part> parts) ParseInput(IEnumerable<string> lines)
    {
        var groups = lines.SplitByEmptyLine().ToList();

        var workflows = groups[0].Select(Workflow.Parse).ToList();
        var parts = groups[1].Select(Part.Parse).ToList();

        return (workflows, parts);
    }

    private static (string left, string right) SplitBy(string text, string sep)
    {
        var index = text.IndexOf(sep);
        return (text.Substring(0, index), text.Substring(index + sep.Length));
    }

    private static string TrimPrefix(string text, string prefix) =>
        text.Substring(prefix.Length);

    private static string TrimSuffix(string text, string suffix) =>
        text.Substring(0, text.Length - suffix.Length);
}
