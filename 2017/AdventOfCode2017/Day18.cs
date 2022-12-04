using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdventOfCode2017
{
    static class Day18
    {
        public static class Inputs
        {
            public static readonly IInput Sample1 =
                Input.Literal(
                    "set a 1",
                    "add a 2",
                    "mul a a",
                    "mod a 5",
                    "snd a",
                    "set a 0",
                    "rcv a",
                    "jgz a -1",
                    "set a 1",
                    "jgz a -2"
                );

            public static readonly IInput Sample2 =
               Input.Literal(
                    "snd 1",
                    "snd 2",
                    "snd p",
                    "rcv a",
                    "rcv b",
                    "rcv c",
                    "rcv d"
               );


            public static readonly IInput Test =
                Input.Http("https://adventofcode.com/2017/day/18/input");
        }

        public class Part1 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().Select(Instruction.Parse).ToList();

                var inbox = new Mailbox();
                var outbox = new Mailbox();
                var computer = new Computer(inbox, outbox, ReceiveVersion.V1);

                var (stopReason, _) = computer.Execute(program);
                Debug.Assert(stopReason == StopReason.ReceiveBlocked);

                Console.WriteLine(outbox.AsEnumerable().Last());
            }
        }

        public class Part2 : IProblem
        {
            public void Run(TextReader input)
            {
                var program = input.Lines().Select(Instruction.Parse).ToList();

                var mailboxA = new Mailbox();
                var mailboxB = new Mailbox();
                
                var computerA = new Computer(mailboxA, mailboxB, ReceiveVersion.V2);
                computerA.Registers.Set('p', 0);

                var computerB = new Computer(mailboxB, mailboxA, ReceiveVersion.V2);
                computerB.Registers.Set('p', 1);

                while (true)
                {
                    var (reasonA, hasAdvancedA) = computerA.Execute(program);
                    var (reasonB, hasAdvancedB) = computerB.Execute(program);

                    if (!hasAdvancedA && !hasAdvancedB)
                    {
                        break;
                    }
                }

                Console.WriteLine(mailboxA.Sends);
            }
        }

        private abstract class Operand
        {
            public static Operand Parse(string text) => 
                int.TryParse(text, out var value) 
                    ? new Literal(value) 
                    : new Register(text[0]);

            private Operand() { }

            public sealed class Register : Operand
            {
                public Register(char name)
                {
                    Name = name;
                }

                public char Name { get; }
            }

            public sealed class Literal : Operand
            {
                public Literal(int value)
                {
                    Value = value;
                }

                public int Value { get; }
            }
        }

        private abstract class Instruction
        {
            public static Instruction Parse(string text)
            {
                static (string left, string right) SplitBy(string text, char by)
                {
                    var index = text.IndexOf(by);
                    return (text.Substring(0, index), text.Substring(index + 1));
                }

                static (Operand op1, Operand op2) ParseOperands(string text)
                {
                    var (left, right) = SplitBy(text, ' ');
                    return (Operand.Parse(left), Operand.Parse(right));
                }

                static Instruction ParseOneOperandInstruction(string name, string ops)
                {
                    var op = Operand.Parse(ops);

                    return name switch
                    {
                        "snd" => new Send(op),
                        "rcv" => new Receive(op),
                        _ => null,
                    };
                }

                static Instruction ParseTwoOperandInstruction(string name, string ops)
                {
                    var (op1, op2) = ParseOperands(ops);

                    return name switch
                    {
                        "set" => new Set(op1, op2),
                        "add" => new Add(op1, op2),
                        "mul" => new Multiply(op1, op2),
                        "mod" => new Modulo(op1, op2),
                        "jgz" => new Jump(op1, op2),

                        _ => null,
                    };
                }

                var (name, ops) = SplitBy(text, ' ');

                return ParseOneOperandInstruction(name, ops)
                    ?? ParseTwoOperandInstruction(name, ops)
                    ?? throw new Exception($"Unknown instruction '{text}'.");
            }

            private Instruction() { }

            public sealed class Send : Instruction
            {
                public Send(Operand operand)
                {
                    Operand = operand;
                }

                public Operand Operand { get; }
            }

            public sealed class Receive : Instruction
            {
                public Receive(Operand operand)
                {
                    Operand = operand;
                }

                public Operand Operand { get; }
            }

            public sealed class Set : Instruction
            {
                public Set(Operand operand1, Operand operand2)
                {
                    Operand1 = operand1;
                    Operand2 = operand2;
                }

                public Operand Operand1 { get; }
                public Operand Operand2 { get; }
            }

            public sealed class Add : Instruction
            {
                public Add(Operand operand1, Operand operand2)
                {
                    Operand1 = operand1;
                    Operand2 = operand2;
                }

                public Operand Operand1 { get; }
                public Operand Operand2 { get; }
            }

            public sealed class Multiply : Instruction
            {
                public Multiply(Operand operand1, Operand operand2)
                {
                    Operand1 = operand1;
                    Operand2 = operand2;
                }

                public Operand Operand1 { get; }
                public Operand Operand2 { get; }
            }

            public sealed class Modulo : Instruction
            {
                public Modulo(Operand operand1, Operand operand2)
                {
                    Operand1 = operand1;
                    Operand2 = operand2;
                }

                public Operand Operand1 { get; }
                public Operand Operand2 { get; }
            }

            public sealed class Jump : Instruction
            {
                public Jump(Operand operand1, Operand operand2)
                {
                    Operand1 = operand1;
                    Operand2 = operand2;
                }

                public Operand Operand1 { get; }
                public Operand Operand2 { get; }
            }
        }

        private sealed class Registers
        {
            private readonly Dictionary<char, long> values;

            public Registers()
            {
                this.values = new Dictionary<char, long>();
            }

            public long Get(char register) =>
                this.values.TryGetValue(register, out var value) ? value : 0;

            public void Set(char register, long value)
            {
                this.values[register] = value;
            }
        }

        private sealed class Mailbox
        {
            private readonly Queue<long> values;
            private int sends;

            public Mailbox()
            {
                this.values = new Queue<long>();
                this.sends = 0;
            }

            public int Count => this.values.Count;

            public int Sends => this.sends;

            public void Send(long value)
            {
                this.values.Enqueue(value);
                this.sends++;
            }

            public long Receive() => this.values.Dequeue();

            public IEnumerable<long> AsEnumerable() => this.values;
        }
        
        private enum ReceiveVersion { V1, V2 }
        private enum StopReason { EndOfProgram, ReceiveBlocked }

        private sealed class Computer
        {
            private int ip;
            private readonly Registers registers;
            private readonly Mailbox inbox;
            private readonly Mailbox outbox;
            private readonly ReceiveVersion receiveVersion;

            public Computer(Mailbox inbox, Mailbox outbox, ReceiveVersion receiveVersion)
            {
                this.ip = 0;
                this.registers = new Registers();
                this.inbox = inbox;
                this.outbox = outbox;
                this.receiveVersion = receiveVersion;
            }

            public Registers Registers => this.registers;

            public (StopReason, bool) Execute(IReadOnlyList<Instruction> program)
            {
                var executedAnything = false;

                while (0 <= this.ip && this.ip < program.Count)
                {
                    var instruction = program[this.ip];
                    var ipIncrement = Execute(instruction);
                    if (ipIncrement == 0)
                    {
                        return (StopReason.ReceiveBlocked, executedAnything);
                    }

                    this.ip += ipIncrement;
                    executedAnything = true;
                }

                return (StopReason.EndOfProgram, executedAnything);
            }

            private int Execute(Instruction instruction) => 
                instruction switch
                {
                    Instruction.Send send => Execute(send),
                    Instruction.Receive receive => Execute(receive),
                    Instruction.Set set => Execute(set),
                    Instruction.Add add => Execute(add),
                    Instruction.Multiply multiply => Execute(multiply),
                    Instruction.Modulo modulo => Execute(modulo),
                    Instruction.Jump jump => Execute(jump),
                    _ => throw new Exception("Unknown instruction kind."),
                };

            private int Execute(Instruction.Send send)
            {
                var (_, val) = Eval(send.Operand);
                this.outbox.Send(val);

                return 1;
            }

            private int Execute(Instruction.Receive receive)
            {
                var (reg, currentValue) = Eval(receive.Operand);
                
                if (this.receiveVersion == ReceiveVersion.V1 && currentValue == 0)
                {
                    // do nothing
                    return 1;
                }
                
                if (this.inbox.Count > 0)
                {
                    var newValue = this.inbox.Receive();

                    if (this.receiveVersion == ReceiveVersion.V2)
                    {
                        this.registers.Set(reg, newValue);
                    }

                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            private int Execute(Instruction.Set set)
            {
                var (reg, _) = Eval(set.Operand1);
                var (_, op2) = Eval(set.Operand2);

                this.registers.Set(reg, op2);

                return 1;
            }

            private int Execute(Instruction.Add add)
            {
                var (reg, op1) = Eval(add.Operand1);
                var (_, op2) = Eval(add.Operand2);

                this.registers.Set(reg, op1 + op2);

                return 1;
            }

            private int Execute(Instruction.Multiply multiply)
            {
                var (reg, op1) = Eval(multiply.Operand1);
                var (_, op2) = Eval(multiply.Operand2);

                this.registers.Set(reg, op1 * op2);

                return 1;
            }

            private int Execute(Instruction.Modulo modulo)
            {
                var (reg, op1) = Eval(modulo.Operand1);
                var (_, op2) = Eval(modulo.Operand2);

                this.registers.Set(reg, op1 % op2);

                return 1;
            }

            private int Execute(Instruction.Jump jump)
            {
                var (_, op1) = Eval(jump.Operand1);
                var (_, op2) = Eval(jump.Operand2);

                return op1 > 0 ? (int)op2 : 1;
            }

            private (char register, long value) Eval(Operand operand) => 
                operand switch
                {
                    Operand.Register regOp => (regOp.Name, this.registers.Get(regOp.Name)),
                    Operand.Literal litOp => (' ', litOp.Value),
                    _ => throw new Exception("Unknown operand kind."),
                };
        }
    }
}
