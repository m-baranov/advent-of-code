using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2021
{
    static class Day16
    {
        public static class Inputs
        {
            public static readonly IInput Sample1 =
                Input.Literal("8A004A801A8002F478");

            public static readonly IInput Sample2 =
                Input.Literal("620080001611562C8802118E34");

            public static readonly IInput Sample3 =
                Input.Literal("C0015000016115A2E0802F182340");

            public static readonly IInput Sample4 =
                Input.Literal("A0016C880162017C3686B18A3D4780");

            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2021/day/16/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var hexString = input.Lines().First();

                var packet = Packet.Parse(hexString);

                var answer = SumVersions(packet);

                Console.WriteLine(answer);
            }

            private long SumVersions(IPacket packet)
            {
                if (packet is LiteralPacket literal)
                {
                    return literal.Header.Version;
                }

                if (packet is OperatorPacket @operator) 
                {
                    return @operator.Header.Version + @operator.SubPackets.Select(SumVersions).Sum();
                }

                return 0;
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var hexString = input.Lines().First();

                var packet = Packet.Parse(hexString);

                var result = Evaluate(packet);
                Console.WriteLine(result);
            }

            private long Evaluate(IPacket packet)
            {
                if (packet is LiteralPacket literal)
                {
                    return literal.Value;
                }

                if (packet is OperatorPacket @operator)
                {
                    var operands = @operator.SubPackets.Select(Evaluate).ToList();

                    return @operator.Header.TypeId switch
                    {
                        PacketTypeId.Sum => operands.Sum(),
                        PacketTypeId.Product => operands.Aggregate((acc, op) => acc * op),
                        PacketTypeId.Minimum => operands.Min(),
                        PacketTypeId.Maximum => operands.Max(),
                        PacketTypeId.GreaterThan => operands[0] > operands[1] ? 1 : 0,
                        PacketTypeId.LessThan => operands[0] < operands[1] ? 1 : 0,
                        PacketTypeId.EqualTo => operands[0] == operands[1] ? 1 : 0,
                        _ => 0
                    };
                }

                return 0;
            }
        }

        private class BitFrame
        {
            private static readonly IReadOnlyDictionary<char, IReadOnlyList<bool>> _hexDigitToBits =
                new Dictionary<char, IReadOnlyList<bool>>()
                {
                    { '0', new[] { false, false, false, false } },
                    { '1', new[] { false, false, false,  true } },
                    { '2', new[] { false, false,  true, false } },
                    { '3', new[] { false, false,  true,  true } },
                    { '4', new[] { false,  true, false, false } },
                    { '5', new[] { false,  true, false,  true } },
                    { '6', new[] { false,  true,  true, false } },
                    { '7', new[] { false,  true,  true,  true } },
                    { '8', new[] {  true, false, false, false } },
                    { '9', new[] {  true, false, false,  true } },
                    { 'A', new[] {  true, false,  true, false } },
                    { 'B', new[] {  true, false,  true,  true } },
                    { 'C', new[] {  true,  true, false, false } },
                    { 'D', new[] {  true,  true, false,  true } },
                    { 'E', new[] {  true,  true,  true, false } },
                    { 'F', new[] {  true,  true,  true,  true } },
                };

            public static BitFrame OfHex(string hexString)
            {
                var bits = hexString.SelectMany(hd => _hexDigitToBits[hd]).ToList();
                return new BitFrame(bits, start: 0, count: bits.Count);
            }

            private readonly IReadOnlyList<bool> bits;
            private readonly int start;

            public BitFrame(IReadOnlyList<bool> bits, int start, int count)
            {
                this.bits = bits;
                this.start = start;
                this.Count = count;
            }

            public int Count { get; }

            public IEnumerable<bool> AsEnumerable()
            {
                for (var i = 0; i < Count; i++)
                {
                    yield return bits[start + i];
                }
            } 

            public (BitFrame first, BitFrame rest) Take(int takeCount)
            {
                if (takeCount <= this.Count)
                {
                    return (
                        new BitFrame(bits, start, takeCount),
                        new BitFrame(bits, start + takeCount, this.Count - takeCount)
                    );
                }
                else
                {
                    return (this, ZeroLength());
                }
            }

            private BitFrame ZeroLength() => new BitFrame(bits, start, count: 0);
        }

        private static class Packet
        {
            public static IPacket Parse(string hexString)
            {
                var bits = BitFrame.OfHex(hexString);
                var (packet, _) = Parse(bits);
                return packet;
            }

            private static (IPacket, BitFrame) Parse(BitFrame bits)
            {
                var (header, rest) = PacketHeader.Parse(bits);

                if (header.TypeId == PacketTypeId.LiteralValue)
                {
                    return ParseLiteralValue(header, rest);
                }
                else
                {
                    return ParseOperator(header, rest);
                }
            }

            private static (IPacket, BitFrame) ParseLiteralValue(PacketHeader header, BitFrame bits)
            {
                var valueBits = new List<bool>();

                var lastGroup = false;
                var remaining = bits;
                do
                {
                    var (group, rest) = remaining.Take(5);
                    remaining = rest;

                    lastGroup = group.AsEnumerable().First() == false;
                    valueBits.AddRange(group.AsEnumerable().Skip(1));
                } while (!lastGroup);

                var value = BitUtil.ToLong(valueBits);
                return (new LiteralPacket(header, value), remaining);
            }

            private static (IPacket, BitFrame) ParseOperator(PacketHeader header, BitFrame bits)
            {
                var (lengthType, length, rest1) = ParseSubPacketLength(bits);

                var (subPackets, rest2) = lengthType == LengthType.Bits
                    ? ParseSubPacketsByBits(rest1, length)
                    : ParseSubPacketsByCount(rest1, length);

                return (new OperatorPacket(header, subPackets), rest2);
            }

            private static (LengthType type, int length, BitFrame) ParseSubPacketLength(BitFrame bits)
            {
                var (lengthTypeBits, rest1) = bits.Take(1);
                
                var lengthType = lengthTypeBits.AsEnumerable().First() 
                    ? LengthType.Packets 
                    : LengthType.Bits;

                var (lengthBits, rest2) = rest1.Take(lengthType == LengthType.Bits ? 15 : 11);
                var length = BitUtil.ToInt(lengthBits.AsEnumerable());

                return (lengthType, length, rest2);
            }

            private enum LengthType { Bits, Packets }

            private static (IReadOnlyList<IPacket>, BitFrame) ParseSubPacketsByBits(BitFrame bits, int bitCount)
            {
                var consumedBits = 0;
                var remaining = bits;
                var packets = new List<IPacket>();

                while (consumedBits < bitCount)
                {
                    var (packet, rest) = Parse(remaining);

                    packets.Add(packet);
                    consumedBits += remaining.Count - rest.Count;
                    remaining = rest;
                }

                return (packets, remaining);
            }

            private static (IReadOnlyList<IPacket>, BitFrame) ParseSubPacketsByCount(BitFrame bits, int packetCount)
            {
                var remaining = bits;
                var packets = new List<IPacket>();

                while (packets.Count < packetCount)
                {
                    var (packet, rest) = Parse(remaining);

                    packets.Add(packet);
                    remaining = rest;
                }

                return (packets, remaining);
            }
        }

        private static class PacketTypeId
        {
            public const int Sum = 0;
            public const int Product = 1;
            public const int Minimum = 2;
            public const int Maximum = 3;
            public const int LiteralValue = 4;
            public const int GreaterThan = 5;
            public const int LessThan = 6;
            public const int EqualTo = 7;
        }

        private class PacketHeader
        {
            public static (PacketHeader, BitFrame) Parse(BitFrame bits)
            {
                var (versionBits, rest1) = bits.Take(3);
                var (typeIdBits, rest2) = rest1.Take(3);

                var header = new PacketHeader(
                    BitUtil.ToInt(versionBits.AsEnumerable()),
                    BitUtil.ToInt(typeIdBits.AsEnumerable())
                );

                return (header, rest2);
            }

            public PacketHeader(int version, int typeId)
            {
                Version = version;
                TypeId = typeId;
            }

            public int Version { get; }
            public int TypeId { get; }
        }

        private interface IPacket
        {
            PacketHeader Header { get; }
        }

        private class LiteralPacket : IPacket
        {
            public LiteralPacket(PacketHeader header, long value)
            {
                Header = header;
                Value = value;
            }

            public PacketHeader Header { get; }
            public long Value { get; }
        }

        private class OperatorPacket : IPacket
        {
            public OperatorPacket(PacketHeader header, IReadOnlyList<IPacket> subPackets)
            {
                Header = header;
                SubPackets = subPackets;
            }

            public PacketHeader Header { get; }
            public IReadOnlyList<IPacket> SubPackets { get; }
        }

        private static class BitUtil
        {
            public static int ToInt(IEnumerable<bool> bits) =>
                (int)ToLong(bits);

            public static long ToLong(IEnumerable<bool> bits)
            {
                var result = 0L;
                foreach (var bit in bits)
                {
                    result = (result << 1) + (bit ? 1 : 0);
                }
                return result;
            }
        }
    }
}
