using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day07
{
    public static class Inputs
    {
        public static readonly IInput Sample =
            Input.Literal(""""""
32T3K 765
T55J5 684
KK677 28
KTJJT 220
QQQJA 483
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/7/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var hands = input.Lines().Select(Hand.ParseHandAndBid).ToList();

            var items = hands
                .Select(pair => (
                    hand: pair.hand,
                    bid: pair.bid,
                    type: pair.hand.Classify()
                ))
                .OrderBy(i => i.type)
                .ThenBy(i => i.hand, Hand.ByCardComparer(CardUtil.C2IsLowestComparer))
                .ToList();

            var sum = items
                .Select((i, index) => i.bid * (index + 1))
                .Sum();

            Console.WriteLine(sum);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var hands = input.Lines().Select(Hand.ParseHandAndBid).ToList();

            var items = hands
                .Select(pair => (
                    hand: pair.hand,
                    bid: pair.bid,
                    type: Classify(pair.hand)
                ))
                .OrderBy(i => i.type)
                .ThenBy(i => i.hand, Hand.ByCardComparer(CardUtil.JIsLowestComparer))
                .ToList();

            var sum = items
                .Select((i, index) => i.bid * (index + 1))
                .Sum();

            Console.WriteLine(sum);
        }

        private static HandType Classify(Hand hand)
        {
            static Hand ReplaceJs(Hand hand, Card card)
            {
                var cards = hand.Cards
                    .Select(c => c == Card.J ? card : c)
                    .ToList();
                return new Hand(cards);
            }

            var jc = hand.Cards.Count(c => c == Card.J);
            if (jc == 0)
            {
                return hand.Classify();
            }
            if (jc == hand.Cards.Count)
            {
                return HandType.FiveOfKind;
            }

            return hand.Cards.Distinct()
                .Where(card => card != Card.J)
                .Select(card => ReplaceJs(hand, card))
                .Select(hand => hand.Classify())
                .Max();
        }
    }

    private enum Card
    {
        A = 13,
        K = 12,
        Q = 11,
        J = 10,
        T =  9, 
        C9 = 8,
        C8 = 7,
        C7 = 6,
        C6 = 5,
        C5 = 4,
        C4 = 3,
        C3 = 2,
        C2 = 1,
    }

    private static class CardUtil
    {
        public static readonly IComparer<Card> C2IsLowestComparer =
            new CardC2IsLowestComparer();

        public static readonly IComparer<Card> JIsLowestComparer =
            new CardJIsLowestComparer();

        public static Card Parse(char ch) =>
            ch switch
            {
                'A' => Card.A,
                'K' => Card.K,
                'Q' => Card.Q,
                'J' => Card.J,
                'T' => Card.T,
                '9' => Card.C9,
                '8' => Card.C8,
                '7' => Card.C7,
                '6' => Card.C6,
                '5' => Card.C5,
                '4' => Card.C4,
                '3' => Card.C3,
                '2' => Card.C2,

                _ => throw new Exception("impossible")
            };

        private class CardC2IsLowestComparer : IComparer<Card>
        {
            public int Compare(Card x, Card y)
            {
                var xi = (int)x;
                var yi = (int)y;

                if (xi == yi)
                {
                    return 0;
                }
                return xi < yi ? -1 : 1;
            }
        }

        private class CardJIsLowestComparer : IComparer<Card>
        {
            public int Compare(Card x, Card y)
            {
                var xi = x == Card.J ? 0 : (int)x;
                var yi = y == Card.J ? 0 : (int)y;

                if (xi == yi)
                {
                    return 0;
                }
                return xi < yi ? -1 : 1;
            }
        }
    }

    private enum HandType
    {
        FiveOfKind = 7,
        FourOfKind = 6,
        FullHouse = 5,
        ThreeOfKind = 4,
        TwoPair = 3,
        OnePair = 2,
        HighCard = 1,
    }

    private record Hand(IReadOnlyList<Card> Cards)
    {
        public static IComparer<Hand> ByCardComparer(IComparer<Card> cardComparer) =>
            new HandByCardComparer(cardComparer);

        public static (Hand hand, int bid) ParseHandAndBid(string text)
        {
            var parts = text.Split(' ');

            var hand = Hand.Parse(parts[0]);
            var bid = int.Parse(parts[1]);

            return (hand, bid);
        }

        public static Hand Parse(string text)
        {
            var cards = text.Select(CardUtil.Parse).ToList();
            return new Hand(cards);
        }

        public HandType Classify()
        {
            var counts = Cards
                .GroupBy(c => c)
                .Select(g => (card: g.Key, count: g.Count()))
                .ToList();

            if (counts.Count == 1)
            {
                return HandType.FiveOfKind;
            }

            if (counts.Count == 2)
            {
                if (counts.Any(c => c.count == 4))
                {
                    return HandType.FourOfKind;
                }
                return HandType.FullHouse;
            }

            if (counts.Count == 3)
            {
                if (counts.Any(c => c.count == 3))
                {
                    return HandType.ThreeOfKind;
                }
                return HandType.TwoPair;
            }

            if (counts.Count == 4)
            {
                return HandType.OnePair;
            }

            return HandType.HighCard;
        }

        private sealed class HandByCardComparer : IComparer<Hand>
        {
            private readonly IComparer<Card> cardComparer;

            public HandByCardComparer(IComparer<Card> cardComparer)
            {
                this.cardComparer = cardComparer;
            }

            public int Compare(Hand? x, Hand? y)
            {
                if (x is null || y is null) throw new Exception("impossible");

                for (var i = 0; i < x.Cards.Count; i++)
                {
                    var c = this.cardComparer.Compare(x.Cards[i], y.Cards[i]);
                    if (c == 0)
                    {
                        continue;
                    }

                    return c;
                }

                return 0;
            }
        }
    }
}
