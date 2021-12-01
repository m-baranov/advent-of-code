using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019
{
    static class Day14
    {
        public static readonly IInput Sample1Input =
            Input.Literal(
                "10 ORE => 10 A",
                "1 ORE => 1 B",
                "7 A, 1 B => 1 C",
                "7 A, 1 C => 1 D",
                "7 A, 1 D => 1 E",
                "7 A, 1 E => 1 FUEL"
            );

        public static readonly IInput Sample2Input =
            Input.Literal(
                "9 ORE => 2 A",
                "8 ORE => 3 B",
                "7 ORE => 5 C",
                "3 A, 4 B => 1 AB",
                "5 B, 7 C => 1 BC",
                "4 C, 1 A => 1 CA",
                "2 AB, 3 BC, 4 CA => 1 FUEL"
            );

        public static readonly IInput Sample3Input =
            Input.Literal(
                "157 ORE => 5 NZVS",
                "165 ORE => 6 DCFZ",
                "44 XJWVT, 5 KHKGT, 1 QDVJ, 29 NZVS, 9 GPVTF, 48 HKGWZ => 1 FUEL",
                "12 HKGWZ, 1 GPVTF, 8 PSHF => 9 QDVJ",
                "179 ORE => 7 PSHF",
                "177 ORE => 5 HKGWZ",
                "7 DCFZ, 7 PSHF => 2 XJWVT",
                "165 ORE => 2 GPVTF",
                "3 DCFZ, 7 NZVS, 5 HKGWZ, 10 PSHF => 8 KHKGT"
            );

        public static readonly IInput Sample4Input =
            Input.Literal(
                "2 VPVL, 7 FWMGM, 2 CXFTF, 11 MNCFX => 1 STKFG",
                "17 NVRVD, 3 JNWZP => 8 VPVL",
                "53 STKFG, 6 MNCFX, 46 VJHF, 81 HVMC, 68 CXFTF, 25 GNMV => 1 FUEL",
                "22 VJHF, 37 MNCFX => 5 FWMGM",
                "139 ORE => 4 NVRVD",
                "144 ORE => 7 JNWZP",
                "5 MNCFX, 7 RFSQX, 2 FWMGM, 2 VPVL, 19 CXFTF => 3 HVMC",
                "5 VJHF, 7 MNCFX, 9 VPVL, 37 CXFTF => 6 GNMV",
                "145 ORE => 6 MNCFX",
                "1 NVRVD => 8 CXFTF",
                "1 VJHF, 6 MNCFX => 4 RFSQX",
                "176 ORE => 6 VJHF"
            );

        public static readonly IInput Sample5Input =
            Input.Literal(
                "171 ORE => 8 CNZTR",
                "7 ZLQW, 3 BMBT, 9 XCVML, 26 XMNCP, 1 WPTQ, 2 MZWV, 1 RJRHP => 4 PLWSL",
                "114 ORE => 4 BHXH",
                "14 VRPVC => 6 BMBT",
                "6 BHXH, 18 KTJDG, 12 WPTQ, 7 PLWSL, 31 FHTLT, 37 ZDVW => 1 FUEL",
                "6 WPTQ, 2 BMBT, 8 ZLQW, 18 KTJDG, 1 XMNCP, 6 MZWV, 1 RJRHP => 6 FHTLT",
                "15 XDBXC, 2 LTCX, 1 VRPVC => 6 ZLQW",
                "13 WPTQ, 10 LTCX, 3 RJRHP, 14 XMNCP, 2 MZWV, 1 ZLQW => 1 ZDVW",
                "5 BMBT => 4 WPTQ",
                "189 ORE => 9 KTJDG",
                "1 MZWV, 17 XDBXC, 3 XCVML => 2 XMNCP",
                "12 VRPVC, 27 CNZTR => 2 XDBXC",
                "15 KTJDG, 12 BHXH => 5 XCVML",
                "3 BHXH, 2 VRPVC => 7 MZWV",
                "121 ORE => 7 VRPVC",
                "7 XCVML => 6 RJRHP",
                "5 BHXH, 4 VRPVC => 5 LTCX"
            );

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/14/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var reactions = input.Lines().Select(Reaction.Parse).ToList();

                var oreAmount = Nanofactory.RequiredOreAmount(reactions, 1 /* fuelAmount */, out var excesses);

                Console.WriteLine($"Excess: {excesses}");
                Console.WriteLine(oreAmount);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var reactions = input.Lines().Select(Reaction.Parse).ToList();

                var orePerFuelUnit = Nanofactory.RequiredOreAmount(reactions, 1 /* fuelAmount */, out var _);

                var oreAmount = 1000000000000L;
                var minFuel = oreAmount / orePerFuelUnit;
                var maxFuel = minFuel * 2;

                while (maxFuel - minFuel > 1)
                {
                    var fuel = (maxFuel + minFuel) / 2;
                    var ore = Nanofactory.RequiredOreAmount(reactions, fuel, out var _);
                    if (ore <= oreAmount)
                    {
                        minFuel = fuel;
                    }
                    else
                    {
                        maxFuel = fuel;
                    }
                }

                Console.WriteLine(minFuel);
            }
        }

        private class Quantity
        {
            public static Quantity Parse(string text)
            {
                var parts = text.Split(' ');

                var amount = long.Parse(parts[0]);
                var chemical = parts[1];

                return new Quantity(chemical, amount);
            }

            public Quantity(string chemical, long amount)
            {
                Chemical = chemical;
                Amount = amount;
            }

            public string Chemical { get; }
            public long Amount { get; }

            public override string ToString() => $"{Amount} {Chemical}";
        }

        private class Reaction
        {
            public static Reaction Parse(string text)
            {
                var parts = text.Split(" => ");
                var requiredText = parts[0];
                var producedText = parts[1];

                var required = requiredText.Split(", ").Select(Quantity.Parse).ToList();
                var produced = Quantity.Parse(producedText);

                return new Reaction(required, produced);
            }

            public Reaction(IReadOnlyList<Quantity> required, Quantity produced)
            {
                Required = required;
                Produced = produced;
            }

            public IReadOnlyList<Quantity> Required { get; }
            public Quantity Produced { get; }
        }

        private class ChecmicalList
        {
            private readonly List<Quantity> quantities;

            public ChecmicalList()
            {
                quantities = new List<Quantity>();
            }

            public bool Empty() => quantities.Count == 0;

            public Quantity Find(string chemical)
            {
                return quantities.FirstOrDefault(q => q.Chemical == chemical);
            }

            public Quantity Pop()
            {
                var quantity = quantities.First();
                quantities.RemoveAt(0);
                return quantity;
            }

            public void Add(Quantity quantity)
            {
                var index = quantities.FindIndex(q => q.Chemical == quantity.Chemical);
                if (index < 0)
                {
                    quantities.Add(quantity);
                }
                else
                {
                    quantities[index] = new Quantity(quantity.Chemical, quantities[index].Amount + quantity.Amount);
                }
            }

            public void Sub(Quantity quantity)
            {
                var index = quantities.FindIndex(q => q.Chemical == quantity.Chemical);
                if (quantities[index].Amount > quantity.Amount)
                {
                    quantities[index] = new Quantity(quantity.Chemical, quantities[index].Amount - quantity.Amount);
                }
                else
                {
                    quantities.RemoveAt(index);
                }
            }

            public override string ToString()
            {
                return string.Join(", ", quantities);
            }
        }

        private static class Nanofactory
        {
            public static long RequiredOreAmount(
                IReadOnlyList<Reaction> reactions, 
                long fuelAmount,
                out ChecmicalList remainingExcesses)
            {
                var needed = new ChecmicalList();
                needed.Add(new Quantity("FUEL", fuelAmount));

                var excesses = new ChecmicalList();

                var oreAmount = 0L;

                while (!needed.Empty())
                {
                    var need = needed.Pop();

                    if (need.Chemical == "ORE")
                    {
                        oreAmount += need.Amount;
                        continue;
                    }

                    var needAmount = need.Amount;

                    var excess = excesses.Find(need.Chemical);
                    if (excess != null)
                    {
                        var excessAmount = Math.Min(excess.Amount, needAmount);

                        needAmount -= excessAmount;
                        excesses.Sub(new Quantity(excess.Chemical, excessAmount));
                    }

                    if (needAmount > 0)
                    {
                        var reaction = reactions.FirstOrDefault(r => r.Produced.Chemical == need.Chemical);
                        var reactionFactor = (long)Math.Ceiling((double)needAmount / reaction.Produced.Amount);

                        foreach (var req in reaction.Required)
                        {
                            needed.Add(new Quantity(req.Chemical, req.Amount * reactionFactor));
                        }

                        var extraAmount = reaction.Produced.Amount * reactionFactor - needAmount;
                        if (extraAmount > 0)
                        {
                            excesses.Add(new Quantity(need.Chemical, extraAmount));
                        }
                    }
                }

                remainingExcesses = excesses;
                return oreAmount;
            }
        }
    }
}
