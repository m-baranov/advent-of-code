using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2023;

static class Day20
{
    public static class Inputs
    {
        public static readonly IInput Sample1 =
            Input.Literal(""""""
broadcaster -> a, b, c
%a -> b
%b -> c
%c -> inv
&inv -> a
"""""");

        public static readonly IInput Sample2 =
            Input.Literal(""""""
broadcaster -> a
%a -> inv, con
&inv -> b
%b -> con
&con -> output
"""""");

        public static readonly IInput Test =
            Input.Http("https://adventofcode.com/2023/day/20/input");
    }

    public class Part1 : IProblem
    {
        public void Run(TextReader input)
        {
            var configs = input.Lines().Select(ModuleConfiguration.Parse).ToList();

            var network = new Network(configs);

            var lows = 0;
            var highs = 0;

            for (var i = 0; i < 1000; i++)
            {
                network.Send(Pulse.Low, packetSent: packet =>
                {
                    if (packet.Pulse == Pulse.Low)
                    {
                        lows++;
                    }
                    else
                    {
                        highs++;
                    }
                });
            }

            Console.WriteLine(lows * highs);
        }
    }

    public class Part2 : IProblem
    {
        public void Run(TextReader input)
        {
            var configs = input.Lines().Select(ModuleConfiguration.Parse).ToList();

            var network = new Network(configs);

            // network.PrintAsPlantUmlStateDiagram() - will print out PlantUML state
            // diagram of the network.
            // 
            // In my case (and I suspect in all others) the diagram shows that the 
            // broadcaster sends signals to distinct subsets of nodes, which form 
            // isolated subnetworks, connecting to the final output node -- rx -- at
            // the very end. So each network can be simulated independently.
            //
            // Assuming that each of these networks produces a low signal once every
            // set constant number of cycles (which seems to be the case in my case),
            // the total number of cycles needed for the entire network to produce a 
            // low signal is LCM of the cycles of these subnetworks.

            var subnetworks = network.BroadcasterSubneworks();

            var counts = subnetworks
                .Select(subnetwork => (long)CountCyclesToTargetSignal(subnetwork))
                .ToList();

            var lcm = MathExtensions.Lcm(counts);
            Console.WriteLine(lcm);
        }

        private static int CountCyclesToTargetSignal(Network network)
        {
            const string TargetName = "rx";

            var count = 0;
            var stop = false;
            while (!stop)
            {
                count++;
                network.Send(Pulse.Low, packetSent: packet =>
                {
                    if (packet.Destination == TargetName && packet.Pulse == Pulse.Low)
                    {
                        stop = true;
                    }
                });
            }

            return count;
        }
    }

    private enum ModuleType { Sink, FlipFlop, Conjunction, Broadcast }

    private record ModuleConfiguration(
        string Name, 
        ModuleType Type, 
        IReadOnlyList<string> DestinationNames)
    {
        public static ModuleConfiguration Parse(string text)
        {
            static (ModuleType type, string name) ParseModule(string text) =>
                text[0] switch
                {
                    '%' => (ModuleType.FlipFlop, text[1..]),
                    '&' => (ModuleType.Conjunction, text[1..]),
                    _ => (text == "broadcaster" ? ModuleType.Broadcast : ModuleType.Sink, text),
                };
                
            var (moduleText, destinationsText) = SplitBy(text, " -> ");

            var (type, name) = ParseModule(moduleText);
            var destinationNames = destinationsText.Split(", ");

            return new ModuleConfiguration(name, type, destinationNames);
        }
    }

    private enum Pulse { Low, High }

    private abstract class Module
    {
        public abstract IReadOnlyList<Pulse> Receive(Pulse pulse, int input);

        public sealed class Sink : Module
        {
            public static readonly Sink Instance = new();

            private Sink() { }

            public override IReadOnlyList<Pulse> Receive(Pulse pulse, int input) =>
                Array.Empty<Pulse>();
        }

        public sealed class Broadcast : Module
        {
            public static readonly Broadcast Instance = new();

            private Broadcast() { }

            public override IReadOnlyList<Pulse> Receive(Pulse pulse, int input) =>
                new[] { pulse };
        }
        
        public sealed class FlipFlop : Module
        {
            private bool isOn;

            public FlipFlop()
            {
                this.isOn = false;
            }

            public override IReadOnlyList<Pulse> Receive(Pulse pulse, int input)
            {
                if (pulse == Pulse.High)
                {
                    return Array.Empty<Pulse>();
                }
                
                if (this.isOn)
                {
                    this.isOn = false;
                    return new[] { Pulse.Low };
                }
                else
                {
                    this.isOn = true;
                    return new[] { Pulse.High };
                }
            }
        }

        public sealed class Conjunction : Module
        {
            private readonly Pulse[] lastPulseByInput;

            public Conjunction(int inputCount)
            {
                this.lastPulseByInput = new Pulse[inputCount];
            }

            public override IReadOnlyList<Pulse> Receive(Pulse pulse, int input)
            {
                this.lastPulseByInput[input] = pulse;

                if (this.lastPulseByInput.All(p => p == Pulse.High))
                {
                    return new[] { Pulse.Low };
                }
                else
                {
                    return new[] { Pulse.High };
                }
            }
        }
    }

    private record Packet(Pulse Pulse, string Source, string Destination);

    private sealed class Network
    {
        private const string Broadcaster = "broadcaster";

        private readonly IReadOnlyDictionary<string, Node> nodes;

        public Network(IReadOnlyList<ModuleConfiguration> configs)
        {
            this.nodes = CreateNodes(configs);
        }

        private static IReadOnlyDictionary<string, Node> CreateNodes(IReadOnlyList<ModuleConfiguration> configs)
        {
            static IReadOnlyList<string> InputsOf(string name, IReadOnlyList<ModuleConfiguration> configs) =>
                configs
                    .Where(c => c.DestinationNames.Contains(name))
                    .Select(c => c.Name)
                    .ToList();

            static Module CreateModule(ModuleType type, int inputCount) =>
                type switch
                {
                    ModuleType.Sink => Module.Sink.Instance,
                    ModuleType.Broadcast => Module.Broadcast.Instance,
                    ModuleType.FlipFlop => new Module.FlipFlop(),
                    ModuleType.Conjunction => new Module.Conjunction(inputCount),

                    _ => throw new Exception("impossible"),
                };

            var nodeByName = new Dictionary<string, Node>();

            foreach (var config in configs)
            {
                var inputs = InputsOf(config.Name, configs);
                var outputs = config.DestinationNames;
                var module = CreateModule(config.Type, inputs.Count);

                nodeByName.Add(config.Name, new Node(config.Name, module, inputs, outputs));
            }

            var sinkNames = configs
                .SelectMany(c => c.DestinationNames)
                .Distinct()
                .Except(nodeByName.Keys)
                .ToList();

            foreach (var name in sinkNames)
            {
                var inputs = InputsOf(name, configs);
                var outputs = Array.Empty<string>();
                var module = CreateModule(ModuleType.Sink, inputs.Count);

                nodeByName.Add(name, new Node(name, module, inputs, outputs));
            }

            return nodeByName;
        }

        public void PrintAsPlantUmlStateDiagram()
        {
            Console.WriteLine("@startuml");
            Console.WriteLine($"[*] --> {Broadcaster}");

            foreach (var node in nodes.Values)
            {
                Console.WriteLine($"{node.Name} : {node.Module.GetType().Name}");

                foreach (var output in node.Outputs)
                {
                    Console.WriteLine($"{node.Name} --> {output}");
                }
            }

            Console.WriteLine("@enduml");
            Console.WriteLine();
        }

        public IReadOnlyList<Network> BroadcasterSubneworks()
        {
            var broadcaster = this.nodes[Broadcaster];
            return broadcaster.Outputs.Select(Subset).ToList();
        }

        private Network Subset(string start)
        {
            static ModuleType GetModuleType(Module module) =>
                module switch
                {
                    Module.Broadcast => ModuleType.Broadcast,
                    Module.Sink => ModuleType.Sink,
                    Module.FlipFlop => ModuleType.FlipFlop,
                    Module.Conjunction => ModuleType.Conjunction,

                    _ => throw new Exception("impossible")
                };

            var visit = new Queue<string>();
            visit.Enqueue(start);

            var subset = new List<Node>();
            while (visit.Count > 0)
            {
                var name = visit.Dequeue();
                var node = this.nodes[name];

                if (subset.Contains(node))
                {
                    continue;
                }

                subset.Add(node);
                visit.EnqueueRange(node.Outputs);
            }

            subset.Add(new Node(
                Broadcaster,
                Module.Broadcast.Instance,
                Inputs: Array.Empty<string>(),
                Outputs: new[] { start }
            ));

            var subsetNames = subset.Select(n => n.Name).ToHashSet();

            var configs = subset
                .Select(node => new ModuleConfiguration(
                    node.Name,
                    GetModuleType(node.Module),
                    DestinationNames: node.Outputs.Where(subsetNames.Contains).ToList()
                ))
                .ToList();

            return new Network(configs);
        }

        public void Send(Pulse pulse, Action<Packet> packetSent)
        {
            var bus = new Bus(packetSent);
            bus.Send(new Packet(pulse, Source: "", Destination: Broadcaster));

            while (!bus.IsEmpty())
            {
                var incomingPacket = bus.Receive();

                var node = this.nodes[incomingPacket.Destination];
                var input = node.Inputs.IndexOf(i => i == incomingPacket.Source);

                var pulses = node.Module.Receive(incomingPacket.Pulse, input);

                var outgoingPackets = node.Outputs
                    .SelectMany(output => pulses
                        .Select(pulse => new Packet(pulse, node.Name, output)));

                foreach (var outgoingPacket in outgoingPackets)
                {
                    bus.Send(outgoingPacket);
                }
            }
        }

        private record Node(
            string Name,
            Module Module, 
            IReadOnlyList<string> Inputs, 
            IReadOnlyList<string> Outputs
        );

        private sealed class Bus
        {
            private readonly Queue<Packet> queue;
            private readonly Action<Packet> packetSent;

            public Bus(Action<Packet> packetSent)
            {
                this.queue = new Queue<Packet>();
                this.packetSent = packetSent;
            }

            public bool IsEmpty() => this.queue.Count == 0;

            public void Send(Packet packet)
            {
                this.queue.Enqueue(packet);
                this.packetSent(packet);
            }

            public Packet Receive() => this.queue.Dequeue();
        }
    }

    private static (string left, string right) SplitBy(string text, string sep)
    {
        var index = text.IndexOf(sep);
        return (text.Substring(0, index), text.Substring(index + sep.Length));
    }

    private static string TrimPrefix(string text, string prefix) =>
        text.Substring(prefix.Length);
}
