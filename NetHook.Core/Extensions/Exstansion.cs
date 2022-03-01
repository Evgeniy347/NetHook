using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetHook.Core
{
    public static class Exstension
    {
        public static bool HasFlag(this BindingFlags source, BindingFlags flag)
        {
            return (source & flag) == flag;

        }

        public static TResult GetProperty<TResult>(this object obj, string name)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            Type type = obj.GetType();
            PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            return (TResult)property.GetValue(obj, null);
        }

        public static IEnumerable<T> SelectRecursive<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> childSelector)
        {
            var stack = new Stack<T>(items);
            while (stack.Any())
            {
                var next = stack.Pop();
                yield return next;
                foreach (var child in childSelector(next))
                    stack.Push(child);
            }
        }

        public static bool HasFlag(this MethodAttributes source, MethodAttributes flag)
        {
            return (source & flag) == flag;
        }


        public static byte[] Call(this IntPtr from, IntPtr to)
        {
            long offset = (long)to - (long)from - 5;

            //if (offset > int.MaxValue || offset < int.MinValue)
            //{
            byte[] result = new byte[12];
            result[0] = 0x48;
            result[1] = 0xB8;

            byte[] toRaw = BitConverter.GetBytes(to.ToInt64());
            Array.Copy(toRaw, 0, result, 2, toRaw.Length);

            result[10] = 0xFF;
            result[11] = 0xD0;

            return result;
            //}
            //else
            //{
            //    byte[] offsetRaw = BitConverter.GetBytes((int)offset);
            //    byte[] result = new byte[5];
            //    result[0] = 0xE8;

            //    Array.Copy(offsetRaw, 0, result, 1, offsetRaw.Length);

            //    return result;
            //}
        }

        public static IntPtr JmpToOffcet(this IntPtr address, byte[] value)
        {
            return address.Add(BitConverter.ToInt32(value.Skip(1).Take(4).ToArray(), 0) + 5);
        }


        public static byte[] Jmp(this IntPtr from, IntPtr to)
        {
            return Jmp(from, (long)to);
        }

        public static byte[] Jmp(this IntPtr from, int to)
        {
            return Jmp(from, (long)to);
        }

        public static byte[] Jmp(this IntPtr from, long to)
        {
            long offset = to - (long)from - 5;

            //if (offset > int.MaxValue || offset < int.MinValue)
            //{
            byte[] result = new byte[12];
            result[0] = 0x48;
            result[1] = 0xB8;

            byte[] toRaw = BitConverter.GetBytes(to);
            Array.Copy(toRaw, 0, result, 2, toRaw.Length);

            result[10] = 0xFF;
            result[11] = 0xE0;

            return result;
            //}
            //else
            //{
            //    byte[] offsetRaw = BitConverter.GetBytes((int)offset);
            //    byte[] result = new byte[5];
            //    result[0] = 0xE9;

            //    Array.Copy(offsetRaw, 0, result, 1, offsetRaw.Length);

            //    return result;
            //}
        }


        public static IntPtr GetAddressBody(this Memory memory, IntPtr address)
        {
            var origInstractions = memory.GetOrigBytes(address, 5);

            if (origInstractions != null && origInstractions[0] == 0xE9)
                address = address.JmpToOffcet(origInstractions);

            return address;
        }

        public static byte[] FindAndReplaceCall(this Memory memory, IntPtr methodAddress, IntPtr currentMethod, IntPtr newMethod)
        {
            List<byte[]> resultInstruction = new List<byte[]>();
            int i = 0;
            foreach (var instruct in memory.GetInstruction(methodAddress))
            {
                if (instruct.Bytes[0] == 0xE8)
                {
                    try
                    {
                        var addressCall = instruct.ToString().Split(' ')[1].Replace("0x", "").ToIntPtr();
                        var addressCallBody = memory.GetAddressBody(addressCall);
                        if (currentMethod == addressCallBody)
                        {
                            var addressInstruction = methodAddress.Add(i);
                            var newCall = addressInstruction.Call(newMethod);
                            resultInstruction.Add(newCall);
                            break;
                        }
                    }
                    catch
                    { }
                }

                i += instruct.Length;
                resultInstruction.Add(instruct.Bytes);
            }

            byte[] result = resultInstruction
                  .SelectMany(x => x)
                  .ToArray();

            return result;
        }


        public static byte[] GetOrigBytes(this Memory memory, IntPtr methodAddress, int length)
        {
            int i = 0;

            List<Instruction> resultInstruction = new List<Instruction>();
            foreach (var instruct in memory.GetInstruction(methodAddress))
            {
                i += instruct.Length;

                resultInstruction.Add(instruct);

                if (i >= length)
                    break;
            }

            byte[] result = resultInstruction
                  .SelectMany(x => x.Bytes)
                  .ToArray();

            return result;
        }

        public static IEnumerable<Instruction> GetOrigInstraction(this Memory memory, IntPtr methodAddress, int length)
        {
            int i = 0;

            foreach (var instruct in memory.GetInstruction(methodAddress))
            {
                i += instruct.Length;

                yield return instruct;

                if (i >= length)
                    break;
            }
        }

        public static IEnumerable<Instruction> GetInstruction(this Memory memory, IntPtr methodAddress)
        {
            bool isRet = false;
            int countByte = 20;
            int countInstruct = 0;
            while (!isRet)
            {
                byte[] raw = memory.Read(methodAddress, countByte);

                var disassembler = new Disassembler(code: raw,
                    architecture: ArchitectureMode.x86_64,
                    address: (ulong)methodAddress,
                    copyBinaryToInstruction: true)
                    .Disassemble()
                    .Skip(countInstruct)
                    .ToArray();

                if (disassembler.Length == 0)
                {
                    countByte += 20;
                    continue;
                }

                foreach (var instruction in disassembler)
                {
                    if (instruction.Error)
                    {
                        countByte += 20;
                        break;
                    }

                    countInstruct++;
                    yield return instruction;

                    if (instruction.Mnemonic == SharpDisasm.Udis86.ud_mnemonic_code.UD_Iret ||
                        instruction.Mnemonic == SharpDisasm.Udis86.ud_mnemonic_code.UD_Iretf)
                    {
                        isRet = true;
                        break;
                    }
                }
            }

        }

        public static void Write(this Memory memory, IntPtr MemoryAddress, int addNOPCount, params byte[][] bytesToWrite)
        {
            memory.Write(MemoryAddress, bytesToWrite.SelectMany(x => x).ToArray(), addNOPCount);
        }

        public static void Write(this Memory memory, IntPtr MemoryAddress, params byte[][] bytesToWrite)
        {
            memory.Write(MemoryAddress, bytesToWrite.SelectMany(x => x).ToArray());
        }

        public static void Write(this Memory memory, IntPtr MemoryAddress, byte[] bytesToWrite, int addNOPCount = 0)
        {
            if (addNOPCount - bytesToWrite.Length > 0)
            {
                byte[] newArray = new byte[addNOPCount];
                Array.Copy(bytesToWrite, newArray, bytesToWrite.Length);
                for (int i = bytesToWrite.Length; i < addNOPCount; i++)
                    newArray[i] = 0x90;

                bytesToWrite = newArray;
            }

            memory.Write(MemoryAddress, bytesToWrite, out int bytes);
        }


        public static void WriteLines(this byte[] raw, IntPtr address)
        {
            Console.WriteLine(raw.ToHexArray());

            string[] instructions = GetDisassemble(raw, address);

            foreach (var instruction in instructions)
                Console.WriteLine(instruction);
        }

        public static string[] GetDisassemble(this byte[] raw, IntPtr address)
        {
            Disassembler disassembler = new Disassembler(code: raw,
                architecture: ArchitectureMode.x86_64,
                address: (ulong)address,
                copyBinaryToInstruction: true);

            return disassembler.Disassemble().Select(x => x.ToString()).ToArray();
        }


        public static IntPtr Add(this IntPtr value, params long[] adds) =>
           value.Add(adds.Select(x => new IntPtr(x)).ToArray());

        public static IntPtr Add(this IntPtr value, params int[] adds) =>
           value.Add(adds.Select(x => new IntPtr(x)).ToArray());

        public static IntPtr Add(this IntPtr value, params IntPtr[] adds)
        {
            var list = adds.ToList();
            list.Add(value);

            return list.Aggregate((x, y) => new IntPtr((long)x + (long)y));
        }

    }
}