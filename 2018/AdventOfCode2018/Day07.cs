using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day07
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "Step C must be finished before step A can begin.",
                    "Step C must be finished before step F can begin.",
                    "Step A must be finished before step B can begin.",
                    "Step A must be finished before step D can begin.",
                    "Step B must be finished before step E can begin.",
                    "Step D must be finished before step E can begin.",
                    "Step F must be finished before step E can begin."
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/7/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var plan = Plan.Parse(input.Lines());

                var completedStepNames = new List<char>();

                while (completedStepNames.Count < plan.StepCount)
                {
                    var canDoNow = plan
                        .StepsThatCanStart(completedStepNames)
                        .Select(d => d.Name)
                        .Min();

                    completedStepNames.Add(canDoNow);
                }

                Console.WriteLine(string.Join("", completedStepNames));
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                const int workerCount = 5;

                static int StepDuration(char step) => step - 'A' + 1 + 60;

                var plan = Plan.Parse(input.Lines());

                var seconds = 0;
                var completedStepNames = new List<char>();
                var queue = new Queue(workerCount);

                while (completedStepNames.Count < plan.StepCount)
                {
                    completedStepNames.AddRange(queue.Complete(seconds));

                    var freeWorkers = queue.FreeWorkers();
                    if (freeWorkers > 0)
                    {
                        var canDoNow = plan
                            .StepsThatCanStart(completedStepNames)
                            .Where(s => !queue.InProgress().Contains(s.Name))
                            .Take(freeWorkers)
                            .ToList();

                        foreach (var step in canDoNow)
                        {
                            queue.Add(step.Name, seconds + StepDuration(step.Name));
                        }
                    }

                    seconds++;
                }

                Console.WriteLine(seconds - 1);
            }
        }

        private class Plan
        {
            public static Plan Parse(IEnumerable<string> lines)
            {
                var stepsWithDependencies = lines
                    .Select(ParseDependency)
                    .GroupBy(p => p.step)
                    .Select(g => new Step(g.Key, g.Select(p => p.dependsOn).ToList()))
                    .ToList();

                var allStepNames = stepsWithDependencies
                    .Select(d => d.Name)
                    .Concat(stepsWithDependencies.SelectMany(d => d.DependsOn))
                    .Distinct();

                var stepsWithoutDependencies = allStepNames
                    .Where(s => !stepsWithDependencies.Any(d => d.Name == s))
                    .Select(s => new Step(s, Array.Empty<char>()));

                var allSteps = stepsWithDependencies
                    .Concat(stepsWithoutDependencies)
                    .ToList();

                return new Plan(allSteps);
            }

            private static (char step, char dependsOn) ParseDependency(string text)
            {
                //           111111111122222222223333333
                // 0123456789012345678901234567890123456
                // Step C must be finished before step A can begin.

                var dependsOn = text[5];
                var step = text[36];

                return (step, dependsOn);
            }

            private readonly IReadOnlyList<Step> steps;

            public Plan(IReadOnlyList<Step> steps)
            {
                this.steps = steps;
            }

            public int StepCount => this.steps.Count;

            public IEnumerable<Step> StepsThatCanStart(IReadOnlyList<char> completedStepNames) =>
                steps
                    .Where(s => !completedStepNames.Contains(s.Name))
                    .Where(s => s.DependsOn.Count == 0 || s.DependsOn.All(d => completedStepNames.Contains(d)));
        }

        private class Step
        {
            public Step(char name, IReadOnlyList<char> dependsOn)
            {
                Name = name;
                DependsOn = dependsOn;
            }

            public char Name { get; }
            public IReadOnlyList<char> DependsOn { get; }
        }

        private class Queue
        {
            private readonly Job[] jobs;

            public Queue(int workerCount)
            {
                jobs = new Job[workerCount];
            }

            public int FreeWorkers() => jobs.Count(j => j == null);

            public IEnumerable<char> InProgress() => 
                jobs.Where(j => j != null).Select(j => j.StepName);

            public IReadOnlyList<char> Complete(int seconds)
            {
                var completed = new List<char>();

                for (var i = 0; i < jobs.Length; i++)
                {
                    var job = jobs[i];

                    if (job != null && job.CompetesOnSecond == seconds)
                    {
                        completed.Add(job.StepName);
                        jobs[i] = null;
                    }
                }

                return completed;
            }

            public bool Add(char stepName, int completesOnSecond)
            {
                for (var i = 0; i < jobs.Length; i++)
                {
                    if (jobs[i] == null)
                    {
                        jobs[i] = new Job(stepName, completesOnSecond);
                        return true;
                    }
                }

                return false;
            }

            private class Job
            {
                public Job(char stepName, int competesOnSecond)
                {
                    StepName = stepName;
                    CompetesOnSecond = competesOnSecond;
                }

                public char StepName { get; }
                public int CompetesOnSecond { get; }
            }
        }
    }
}
