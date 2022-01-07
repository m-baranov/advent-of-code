using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Computer = AdventOfCode2019.Day09.Computer;

namespace AdventOfCode2019
{
    static class Day23
    {
        public static readonly IInput SampleInput =
            Input.Literal();

        public static readonly IInput TestInput =
            Input.Http("https://adventofcode.com/2019/day/23/input");

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();

                var network = Network.Create(program, count: 50);

                Packet packet255 = null;
                while (packet255 == null)
                {
                    network.RunOnce();

                    var packets255 = network.PacketQueue.DequeueAllByAddress(255);
                    if (packets255.Any())
                    {
                        packet255 = packets255.First();
                    }
                }

                Console.WriteLine(packet255.Y);
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().First();

                var network = Network.Create(program, count: 50);

                Packet natPacket = null;

                var seenYs = new HashSet<long>();
                long? duplicateY = null; 

                while (duplicateY == null)
                {
                    var idling = network.RunOnce();

                    var natPackets = network.PacketQueue.DequeueAllByAddress(255);
                    if (natPackets.Any())
                    {
                        natPacket = natPackets.Last();
                    }

                    if (idling && natPacket != null)
                    {
                        network.PacketQueue.Enqueue(natPacket.WithAddress(0));

                        if (seenYs.Contains(natPacket.Y))
                        {
                            duplicateY = natPacket.Y;
                        }
                        else
                        {
                            seenYs.Add(natPacket.Y);
                        }
                    }
                }

                Console.WriteLine(duplicateY.Value);
            }
        }

        private class Network
        {
            public static Network Create(string program, int count)
            {
                var computers = Enumerable.Range(0, count)
                   .Select(addr => Computer.Of(program, new[] { (long)addr }))
                   .ToArray();

                return new Network(computers);
            }

            private readonly IReadOnlyList<Computer> computers;

            public Network(IReadOnlyList<Computer> computers)
            {
                this.computers = computers;
                this.PacketQueue = new PacketQueue();
            }

            public PacketQueue PacketQueue { get; }

            public bool RunOnce()
            {
                var idling = true; 

                for (var addr = 0; addr < computers.Count; addr++)
                {
                    var computer = computers[addr];

                    var (result, output) = computer.ExecuteAndGetNewOutput();

                    var sentPackets = Packet.ParseMany(output);
                    PacketQueue.EnqueueRange(sentPackets);

                    IReadOnlyList<Packet> receivedPackets = Array.Empty<Packet>();
                    if (result is Computer.Result.WaitingForInput)
                    {
                        receivedPackets = PacketQueue.DequeueAllByAddress(addr);
                        SendPackets(computer, receivedPackets);
                    }

                    idling = idling && (sentPackets.Count == 0 && receivedPackets.Count == 0);
                }

                return idling;
            }

            private static void SendPackets(Computer computer, IReadOnlyList<Packet> packets)
            {
                if (packets.Count == 0)
                {
                    computer.Input.Enter(-1);
                    return;
                }

                foreach (var packet in packets)
                {
                    computer.Input.Enter(packet.X);
                    computer.Input.Enter(packet.Y);
                }
            }
        }

        private class PacketQueue
        {
            private List<Packet> packets;
            
            public PacketQueue()
            {
                packets = new List<Packet>();
            }

            public void Enqueue(Packet newPacket)
            {
                packets.Add(newPacket);
            }

            public void EnqueueRange(IEnumerable<Packet> newPackets)
            {
                packets.AddRange(newPackets);
            }

            public IReadOnlyList<Packet> DequeueAllByAddress(long address)
            {
                var dequeued = packets.Where(p => p.Address == address).ToList();

                packets = packets.Where(p => p.Address != address).ToList();

                return dequeued;
            }
        }

        private class Packet
        {
            public static IReadOnlyList<Packet> ParseMany(IReadOnlyList<long> output) =>
                output.Chunk(3).Select(chunk => new Packet(chunk[0], chunk[1], chunk[2])).ToList();

            public Packet(long address, long x, long y)
            {
                Address = address;
                X = x;
                Y = y;
            }

            public long Address { get; }
            public long X { get; }
            public long Y { get; }

            public Packet WithAddress(long newAddress) => new Packet(newAddress, X, Y);
        }
    }
}
