using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day04
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "[1518-11-01 00:00] Guard #10 begins shift",
                    "[1518-11-01 00:05] falls asleep",
                    "[1518-11-01 00:25] wakes up",
                    "[1518-11-01 00:30] falls asleep",
                    "[1518-11-01 00:55] wakes up",
                    "[1518-11-01 23:58] Guard #99 begins shift",
                    "[1518-11-02 00:40] falls asleep",
                    "[1518-11-02 00:50] wakes up",
                    "[1518-11-03 00:05] Guard #10 begins shift",
                    "[1518-11-03 00:24] falls asleep",
                    "[1518-11-03 00:29] wakes up",
                    "[1518-11-04 00:02] Guard #99 begins shift",
                    "[1518-11-04 00:36] falls asleep",
                    "[1518-11-04 00:46] wakes up",
                    "[1518-11-05 00:03] Guard #99 begins shift",
                    "[1518-11-05 00:45] falls asleep",
                    "[1518-11-05 00:55] wakes up"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/4/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var entries = input.Lines().Select(LogEntry.Parse);

                var sleepSchedules = SleepSchedule.OfLogEntries(entries);

                var guardId = sleepSchedules
                    .SelectMany(s => s.SleepTimes.Select(t => new { s.GuardId, Range = t }))
                    .GroupBy(s => s.GuardId)
                    .Select(g => new { GuardId = g.Key, SleepTime = g.Sum(i => i.Range.DurationInMinutes()) })
                    .MaxBy(g => g.SleepTime)
                    .GuardId;

                var minute = sleepSchedules
                    .Where(s => s.GuardId == guardId)
                    .SelectMany(s => s.SleepTimes.SelectMany(t => t.Minites()))
                    .GroupBy(m => m)
                    .Select(g => new { Minute = g.Key, Count = g.Count() })
                    .MaxBy(g => g.Count)
                    .Minute;

                Console.WriteLine(guardId * minute);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var entries = input.Lines().Select(LogEntry.Parse);

                var sleepSchedules = SleepSchedule.OfLogEntries(entries);

                var answer = sleepSchedules
                    .SelectMany(s => s.SleepTimes.SelectMany(t => t.Minites().Select(m => new { s.GuardId, Minute = m })))
                    .GroupBy(p => p.GuardId)
                    .Select(g =>
                    {
                        var mostSleptMinute = g
                            .Select(i => i.Minute)
                            .GroupBy(m => m)
                            .Select(g => new { Minute = g.Key, Count = g.Count() })
                            .MaxBy(g => g.Count);

                        return new { GuardId = g.Key, mostSleptMinute.Minute, mostSleptMinute.Count };
                    })
                    .MaxBy(g => g.Count);

                Console.WriteLine(answer.GuardId * answer.Minute);
            }
        }

        private class LogEntry
        {
            public static LogEntry Parse(string text)
            {
                // [1518-11-01 00:00] Guard #10 begins shift

                var closeBracketIndex = text.IndexOf(']');

                var dateText = text.Substring(1, closeBracketIndex - 1); // +1/-1 skips '['
                var eventText = text.Substring(closeBracketIndex + 2); // +2 skips '] '

                var dateTime = DateTime.Parse(dateText);
                var (eventType, guardId) = ParseEvent(eventText);

                return new LogEntry(dateTime, eventType, guardId);
            }

            private static (EventType eventType, int? guardId) ParseEvent(string eventText)
            {
                if (eventText.Contains("falls asleep"))
                {
                    return (EventType.FallsAsleep, null);
                }

                if (eventText.Contains("wakes up"))
                {
                    return (EventType.WakesUp, null);
                }

                // Guard #10 begins shift
                var hashIndex = eventText.IndexOf('#');
                var spaceIndex = eventText.IndexOf(' ', startIndex: hashIndex);
                var idText = eventText.Substring(hashIndex + 1, spaceIndex - hashIndex - 1);

                return (EventType.StartsShift, int.Parse(idText));
            }

            public LogEntry(DateTime dateTime, EventType eventType, int? guardId)
            {
                DateTime = dateTime;
                EventType = eventType;
                GuardId = guardId;
            }

            public DateTime DateTime { get; }
            public EventType EventType { get; }
            public int? GuardId { get; }
        }

        private enum EventType { StartsShift, FallsAsleep, WakesUp }

        private class SleepSchedule
        {
            public static IReadOnlyList<SleepSchedule> OfLogEntries(IEnumerable<LogEntry> entries)
            {
                var groups = GroupByGuard(entries.OrderBy(e => e.DateTime));

                return groups
                    .Select(g => SingleDaySceduleOfLogEntries(g.guardId, g.entries))
                    .ToList();
            }

            private static IReadOnlyList<(int guardId, IReadOnlyList<LogEntry> entries)> GroupByGuard(IOrderedEnumerable<LogEntry> entries)
            {
                var groups = new List<(int guardId, IReadOnlyList<LogEntry>)>();

                var guardId = 0;
                var guardEntries = new List<LogEntry>();

                foreach (var entry in entries)
                {
                    if (entry.EventType == EventType.StartsShift)
                    {
                        if (guardEntries.Count > 0)
                        {
                            groups.Add((guardId, guardEntries));
                        }

                        guardId = entry.GuardId.Value;
                        guardEntries = new List<LogEntry>();
                    }
                    else
                    {
                        guardEntries.Add(entry);
                    }
                }

                if (guardEntries.Count > 0)
                {
                    groups.Add((guardId, guardEntries));
                }

                return groups;
            }

            private static SleepSchedule SingleDaySceduleOfLogEntries(int guardId, IEnumerable<LogEntry> dayEntries)
            {
                var date = dayEntries.First().DateTime.Date;

                var timeRanges = new List<TimeRange>();
                TimeSpan? start = null;

                foreach (var entry in dayEntries)
                {
                    if (entry.EventType == EventType.FallsAsleep)
                    {
                        Debug.Assert(start == null);

                        start = entry.DateTime.TimeOfDay;
                    }
                    else if (entry.EventType == EventType.WakesUp)
                    {
                        Debug.Assert(start != null);

                        var end = entry.DateTime.TimeOfDay;
                        timeRanges.Add(new TimeRange(start.Value, end));

                        start = null;
                    }
                }

                Debug.Assert(start == null);

                return new SleepSchedule(date, guardId, timeRanges);
            }

            public SleepSchedule(DateTime date, int guardId, IReadOnlyList<TimeRange> sleepTimes)
            {
                Date = date;
                GuardId = guardId;
                SleepTimes = sleepTimes;
            }

            public DateTime Date { get; }
            public int GuardId { get; }
            public IReadOnlyList<TimeRange> SleepTimes { get; }
        }

        private class TimeRange
        {
            public TimeRange(TimeSpan start, TimeSpan end)
            {
                Start = start;
                End = end;
            }

            public TimeSpan Start { get; }
            public TimeSpan End { get; }

            public IEnumerable<int> Minites() => Enumerable.Range(Start.Minutes, DurationInMinutes());

            public int DurationInMinutes() => End.Minutes - Start.Minutes;
        }
    }
}
