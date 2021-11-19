using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2020
{
    static class Day21
    {
        public static readonly IInput SampleInput =
            Input.Literal(
                "mxmxvkd kfcds sqjhc nhms (contains dairy, fish)",
                "trh fvjkl sbzzf mxmxvkd (contains dairy)",
                "sqjhc fvjkl (contains soy)",
                "sqjhc mxmxvkd sbzzf (contains fish)"
            );

        public static readonly IInput TestInput =
           Input.Http("https://adventofcode.com/2020/day/21/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var foods = Food.ParseAll(input.Lines());

                var ingredients = Solve(foods);

                foreach (var ingredient in ingredients)
                {
                    Console.WriteLine(ingredient);
                }

                var result = ingredients
                    .Select(i => foods.Where(f => f.Ingredients.Contains(i)).Count())
                    .Sum();

                Console.WriteLine(result);
            }

            private IEnumerable<string> Solve(IReadOnlyList<Food> foods)
            {
                var ingredients = foods.SelectMany(f => f.Ingredients).ToHashSet();
                var allergens = foods.SelectMany(f => f.Allergens).ToHashSet();

                var ingredientsWithPossibleAllergen = allergens
                    .SelectMany(allergen =>
                    {
                        var foodsWithAllergen = foods.Where(f => f.Allergens.Contains(allergen)).ToList();

                        var ingredientsPossiblyContainingAllergen = foodsWithAllergen.Aggregate(
                            foodsWithAllergen.First().Ingredients.ToHashSet(),
                            (acc, next) => { acc.IntersectWith(next.Ingredients); return acc; }
                        );

                        return ingredientsPossiblyContainingAllergen;
                    })
                    .Distinct();

                return ingredients.Except(ingredientsWithPossibleAllergen).ToList();
            }

            /*
            private IEnumerable<string> Solve(IReadOnlyList<Food> foods)
            {
                var ingredients = foods.SelectMany(f => f.Ingredients).ToHashSet();
                Solve(foods, new IngredientAllergenMap(), ingredients, 0);
                return ingredients;
            }

            private void Solve(IReadOnlyList<Food> foods, IngredientAllergenMap ingredientAllergen, HashSet<string> ingredients, int index)
            {
                if (index >= foods.Count)
                {
                    foreach (var ingredient in ingredientAllergen.Ingredients())
                    {
                        ingredients.Remove(ingredient);
                    } 

                    return;
                }

                var food = foods[index];
                foreach (var combination in Util.Combinations(food.Ingredients, food.Allergens))
                {
                    if (!ingredientAllergen.CanAdd(combination))
                    {
                        continue;
                    }

                    Solve(foods, ingredientAllergen.Add(combination), ingredients, index + 1);
                }
            }
            */
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var foods = Food.ParseAll(input.Lines());

                var ingredients = foods.SelectMany(f => f.Ingredients).ToHashSet();
                var allergens = foods.SelectMany(f => f.Allergens).ToHashSet();

                var possibilities = allergens
                    .Select(allergen =>
                    {
                        var foodsWithAllergen = foods.Where(f => f.Allergens.Contains(allergen)).ToList();

                        var possibleIngredients = foodsWithAllergen.Aggregate(
                            foodsWithAllergen.First().Ingredients.ToHashSet(),
                            (acc, next) => { acc.IntersectWith(next.Ingredients); return acc; }
                        );

                        return (allergen, possibleIngredients: possibleIngredients.ToList());
                    })
                    .ToList();

                var result = new List<(string allergen, string ingredient)>();

                while (possibilities.Count > 0)
                {
                    var possibilityIndex = possibilities.FindIndex(p => p.possibleIngredients.Count == 1);
                    if (possibilityIndex < 0)
                    {
                        Console.WriteLine("UNABLE TO SOLVE");
                        break;
                    }

                    var possibility = possibilities[possibilityIndex];
                    var allergen = possibility.allergen;
                    var ingredient = possibility.possibleIngredients[0];

                    result.Add((allergen, ingredient));
                    possibilities.RemoveAt(possibilityIndex);

                    foreach (var remainingPossibility in possibilities)
                    {
                        remainingPossibility.possibleIngredients.Remove(ingredient); 
                    }
                }

                foreach (var pair in result)
                {
                    Console.WriteLine($"a: {pair.allergen}, i: {pair.ingredient}");
                }

                var answer = string.Join(",", result.OrderBy(r => r.allergen).Select(r => r.ingredient));
                Console.Write(answer);
            }
        }

        public class Food
        {
            public static IReadOnlyList<Food> ParseAll(IEnumerable<string> lines)
            {
                return lines.Select(Parse).ToList();
            }

            public static Food Parse(string line)
            {
                var (ingredientsText, allergensText) = SplitIngredientsAndAllergens(line);

                var ingredients = ingredientsText.Split(new[] { " " }, StringSplitOptions.None);
                var allergens = allergensText.Split(new[] { ", " }, StringSplitOptions.None);

                return new Food(ingredients, allergens);
            }

            private static (string, string) SplitIngredientsAndAllergens(string line)
            {
                const string prefix = " (contains ";
                const string suffix = ")";

                var index = line.IndexOf(prefix);
                if (index < 0)
                {
                    return (line, string.Empty);
                }

                var ingredients = line.Substring(0, index);

                var allergens = line.Substring(index + prefix.Length);
                allergens = allergens.Substring(0, allergens.Length - suffix.Length);

                return (ingredients, allergens);
            }

            public Food(
                IReadOnlyList<string> ingredients, 
                IReadOnlyList<string> allergens)
            {
                Ingredients = ingredients;
                Allergens = allergens;
            }

            public IReadOnlyList<string> Ingredients { get; }
            public IReadOnlyList<string> Allergens { get; }
        }

        /*
        private class IngredientAllergenPair
        {
            public IngredientAllergenPair(string ingredient, string allergen)
            {
                Ingredient = ingredient;
                Allergen = allergen;
            }

            public string Ingredient { get; }
            public string Allergen { get; }

            public override string ToString()
            {
                return $"i: {Ingredient}, a: {Allergen}";
            }
        }

        private class IngredientAllergenMap
        {
            private readonly List<IngredientAllergenPair> pairs;

            public IngredientAllergenMap()
                : this(new List<IngredientAllergenPair>())
            {
            }

            private IngredientAllergenMap(List<IngredientAllergenPair> pairs)
            {
                this.pairs = pairs;
            }

            public IEnumerable<string> Ingredients() => pairs.Select(p => p.Ingredient);

            public bool CanAdd(IEnumerable<IngredientAllergenPair> pairs)
            {
                return pairs.All(CanAdd);
            }

            public bool CanAdd(IngredientAllergenPair pair)
            {
                if (Contains(pair))
                {
                    return true;
                }

                var allergenExists = pairs.Any(p => p.Allergen == pair.Allergen);
                if (allergenExists)
                {
                    return false;
                }

                return true;
            }

            public IngredientAllergenMap Add(IEnumerable<IngredientAllergenPair> newPairs)
            {
                var combined = pairs.ToList();

                foreach (var newPair in newPairs)
                {
                    if (!Contains(newPair))
                    {
                        combined.Add(newPair);
                    }
                }

                return new IngredientAllergenMap(combined);
            }

            private bool Contains(IngredientAllergenPair pair)
            {
                return pairs.Any(p => p.Ingredient == pair.Ingredient && p.Allergen == pair.Allergen);
            }
        }

        private static class Util
        {
            public static IEnumerable<IEnumerable<IngredientAllergenPair>> Combinations(
                IReadOnlyList<string> ingredients,
                IReadOnlyList<string> allergens) 
            {
                return Combinations<string, string>(ingredients, allergens)
                    .Select(pairs => pairs.Select(p => new IngredientAllergenPair(p.left, p.right)));
            }

            //private static void EnumerateCombinations(int leftCount, int rightCount)
            //{
            //    EnumerateCombinations(new int[rightCount], leftCount, 0);
            //}

            //private static void EnumerateCombinations(int[] indices, int leftCount, int index)
            //{
            //    if (index == indices.Length)
            //    {
            //        Console.WriteLine(string.Join(",", indices));
            //        return;
            //    }

            //    var candidates = Enumerable.Range(0, leftCount).Except(indices.Take(index));
            //    foreach (var candidate in candidates)
            //    {
            //        indices[index] = candidate;
            //        EnumerateCombinations(indices, leftCount, index + 1);
            //    }
            //}

            public static IEnumerable<IEnumerable<(TLeft left, TRight right)>> Combinations<TLeft, TRight>(
                IReadOnlyList<TLeft> lefts,
                IReadOnlyList<TRight> rights)
            {
                return Combinations(lefts.Count, rights.Count)
                    .Select(indices => rights.Select((r, i) => (lefts[indices[i]], r)));
            }

            public static IEnumerable<IReadOnlyList<int>> Combinations(int leftCount, int rightCount)
            {
                var indices = new int[rightCount];
                Array.Fill(indices, -1);

                var index = 0;

                while (index >= 0)
                {
                    if (index == indices.Length)
                    {
                        yield return indices.ToArray();
                        index--;
                    }
                    else
                    {
                        var current = indices[index];

                        var next = Enumerable.Range(0, leftCount).Except(indices.Take(index)).Where(i => i > current).DefaultIfEmpty(-1).First();
                        if (next < 0)
                        {
                            indices[index] = -1;
                            index--;
                        }
                        else
                        {
                            indices[index] = next;
                            index++;
                        }
                    }
                }
            }
        }
        */
    }
}
