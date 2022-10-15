using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    static class Day24
    {
        public static class Inputs
        {
            public static readonly IInput Sample =
                Input.Literal(
                    "Immune System:",
                    "17 units each with 5390 hit points (weak to radiation, bludgeoning) with " +
                    "an attack that does 4507 fire damage at initiative 2",
                    "989 units each with 1274 hit points (immune to fire; weak to bludgeoning, " +
                    "slashing) with an attack that does 25 slashing damage at initiative 3",
                    "",
                    "Infection:",
                    "801 units each with 4706 hit points (weak to radiation) with an attack " +
                    "that does 116 bludgeoning damage at initiative 1",
                    "4485 units each with 2961 hit points (immune to radiation; weak to fire, " +
                    "cold) with an attack that does 12 slashing damage at initiative 4"
                );

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2018/day/24/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var groups = Group.ParseMany(input.Lines());

                var (winnerKind, winnerUnitCount) = Simulation.Run(groups);

                Console.WriteLine($"winner={winnerKind}, unitCount={winnerUnitCount}");
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var groups = Group.ParseMany(input.Lines());

                var boost = 1;

                while (true)
                {
                    var (result, winnerUnitCount) = Simulation.Run(BoostImmuneSystemAttackDamage(groups, boost));

                    if (result == Result.ImmuneSystemWon)
                    {
                        Console.WriteLine($"winner={result}, unitCount={winnerUnitCount}, boost={boost}");
                        break;
                    }

                    boost++;
                }
            }

            private IReadOnlyList<Group> BoostImmuneSystemAttackDamage(IReadOnlyList<Group> groups, int boost) =>
                groups
                    .Select(g => g.Kind == GroupKind.ImmuneSystem ? g with { AttackDamagePerUnit = g.AttackDamagePerUnit + boost } : g)
                    .ToList();
        }

        private enum GroupKind { ImmuneSystem, Infection }
        private enum Result { ImmuneSystemWon, InfectionWon, Stalemate }

        private record Group(
            GroupKind Kind, 
            int UnitCount, 
            int HitPointsPerUnit, 
            int Initiative,
            string AttackType,
            int AttackDamagePerUnit,
            IReadOnlyList<string> WeakTo, 
            IReadOnlyList<string> ImmuneTo)
        {
            public static IReadOnlyList<Group> ParseMany(IEnumerable<string> lines)
            {
                var parts = lines.SplitByEmptyLine().Select(p => p.Skip(1)).ToList();

                var immuneSystemGroups = parts[0].Select(l => Parse(l, GroupKind.ImmuneSystem));
                var infectionGroups = parts[1].Select(l => Parse(l, GroupKind.Infection));

                return immuneSystemGroups.Concat(infectionGroups).ToList();
            }

            public static Group Parse(string line, GroupKind kind)
            {
                static (string left, string right) Split(string text, string by)
                {
                    var index = text.IndexOf(by);
                    return (text.Substring(0, index), text.Substring(index + by.Length));
                }

                static IReadOnlyList<string> ParseWeakOrImmuneList(IReadOnlyList<string> parts, string prefix)
                {
                    var part = parts.FirstOrDefault(p => p.StartsWith(prefix));
                    if (part == null)
                    {
                        return Array.Empty<string>();
                    }

                    return part.Substring(prefix.Length).Split(", ");
                }

                static (IReadOnlyList<string> weakTo, IReadOnlyList<string> immuneTo) ParseWeakAndImmuneLists(string text)
                {
                    var parts = text.TrimStart('(').TrimEnd(')', ' ').Split("; ");

                    var weakTo = ParseWeakOrImmuneList(parts, "weak to ");
                    var immuneTo = ParseWeakOrImmuneList(parts, "immune to ");

                    return (weakTo, immuneTo);
                }

                var (unitsText, rest1) = Split(line, " units each with ");
                var (hpText, rest2) = Split(rest1, " hit points ");
                var (weakImmuneTest, rest3) = Split(rest2, "with an attack that does "); // no space at the beginning
                var (attackText, initiativeText) = Split(rest3, " damage at initiative ");

                var (attackDamageText, attackType) = Split(attackText, " ");

                var unitCount = int.Parse(unitsText);
                var hitPoints = int.Parse(hpText);
                var initiative = int.Parse(initiativeText);
                var attackDamage = int.Parse(attackDamageText);
                var (weakTo, immuneTo) = ParseWeakAndImmuneLists(weakImmuneTest);

                return new Group(kind, unitCount, hitPoints, initiative, attackType, attackDamage, weakTo, immuneTo);
            }

            public static int Damage(Group attacker, Group defender)
            {
                var isImmune = defender.ImmuneTo.Contains(attacker.AttackType);
                if (isImmune)
                {
                    return 0;
                }

                var isWeak = defender.WeakTo.Contains(attacker.AttackType);
                if (isWeak)
                {
                    return attacker.EffectivePower * 2;
                }

                return attacker.EffectivePower;
            }

            public static Group Attack(Group attacker, Group defender)
            {
                static int DivCeiling(int a, int b) => (int)Math.Ceiling((double)a / b);

                var damage = Damage(attacker, defender);

                var remainingHp = defender.HitPointsPerUnit * defender.UnitCount - damage;
                if (remainingHp <= 0)
                {
                    return defender with { UnitCount = 0 };
                }

                var remainingUnits = DivCeiling(remainingHp, defender.HitPointsPerUnit);
                return defender with { UnitCount = remainingUnits };
            }

            public bool IsAlive => this.UnitCount > 0;

            public int EffectivePower => this.UnitCount * this.AttackDamagePerUnit;
        }

        private static class Simulation
        {
            public static (Result result, int unitCount) Run(IReadOnlyList<Group> groups)
            {
                Result result;
                while (!IsFinished(groups, out result))
                {
                    var orderedGroups = groups
                        .OrderByDescending(g => g.EffectivePower)
                        .ThenByDescending(g => g.Initiative)
                        .ToList();

                    var defenderIndexes = DetermineDefenderIndexes(orderedGroups);
                    var attackerIndexes = DetermineAttackerIndexes(orderedGroups);

                    var advanced = false;
                    foreach (var attackerIndex in attackerIndexes)
                    {
                        var attacker = orderedGroups[attackerIndex];
                        if (!attacker.IsAlive)
                        {
                            continue;
                        }

                        var defenderIndex = defenderIndexes[attackerIndex];
                        if (defenderIndex < 0)
                        {
                            continue;
                        }

                        var defender = orderedGroups[defenderIndex];
                        if (!defender.IsAlive)
                        {
                            continue;
                        }

                        var defenderUpdated = Group.Attack(attacker, defender);
                        orderedGroups[defenderIndex] = defenderUpdated;

                        advanced = advanced || (defenderUpdated.UnitCount != defender.UnitCount);
                    }

                    if (!advanced)
                    {
                        return (Result.Stalemate, -1);
                    }

                    groups = orderedGroups.Where(g => g.IsAlive).ToList();
                }

                return (result, CountRemainingUnits(groups));
            }

            private static IReadOnlyList<int> DetermineAttackerIndexes(IReadOnlyList<Group> groups) =>
                groups
                    .Select((group, index) => (group, index))
                    .OrderByDescending(g => g.group.Initiative)
                    .Select(p => p.index)
                    .ToList();
            
            private static IReadOnlyList<int> DetermineDefenderIndexes(IReadOnlyList<Group> groups)
            {
                var defenderIndexes = new List<int>();
                
                foreach (var attacker in groups)
                {
                    defenderIndexes.Add(ChooseDefenderIndex(attacker, groups, defenderIndexes));
                }

                return defenderIndexes;
            }

            private static int ChooseDefenderIndex(Group attacker, IReadOnlyList<Group> groups, IReadOnlyList<int> defenderIndexes)
            {
                var allDefenders = groups
                    .Select((group, index) => (group, index))
                    .Where(p => p.group.Kind != attacker.Kind)
                    .Where(p => !defenderIndexes.Contains(p.index))
                    .Select(p => (p.group, p.index, damage: Group.Damage(attacker, p.group)))
                    .ToList();

                if (allDefenders.Count == 0)
                {
                    return -1;
                }

                var maxDamage = allDefenders.Max(d => d.damage);
                if (maxDamage == 0)
                {
                    return -1;
                }

                var defender = allDefenders
                    .Where(d => d.damage == maxDamage)
                    .OrderByDescending(d => d.group.EffectivePower)
                    .ThenByDescending(d => d.group.Initiative)
                    .First();

                return defender.index;
            }

            private static bool IsFinished(IReadOnlyList<Group> groups, out Result result)
            {
                var counts = CountGroups(groups);
                if (counts.immuneSystem == 0)
                {
                    result = Result.InfectionWon;
                    return true;
                }
                else if (counts.infection == 0)
                {
                    result = Result.ImmuneSystemWon;
                    return true;
                }
                else
                {
                    result = default;
                    return false;
                }
            }

            private static (int immuneSystem, int infection) CountGroups(IReadOnlyList<Group> groups)
            {
                var immuneSystem = 0;
                var infection = 0;
                foreach (var group in groups)
                {
                    if (group.Kind == GroupKind.ImmuneSystem)
                    {
                        immuneSystem++;
                    }
                    else
                    {
                        infection++;
                    }
                }

                return (immuneSystem, infection);
            }

            private static int CountRemainingUnits(IReadOnlyList<Group> groups) =>
                groups.Where(g => g.IsAlive).Select(g => g.UnitCount).Sum();
        }
    }
}
