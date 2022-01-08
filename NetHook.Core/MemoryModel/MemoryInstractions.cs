using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetHook.Core
{
    public class MemoryInstractions : IDisposable
    {
        private readonly Memory _memory;
        private readonly int _maxSize;
        private readonly List<FrameInstractions> _frameInstractions = new List<FrameInstractions>();
        private byte[] _original;

        public MemoryInstractions(IntPtr address, Memory memory, int maxSize = -1)
        {
            _memory = memory;
            Address = address;
            _maxSize = maxSize;
        }

        public IntPtr Address { get; }

        public int Size => _frameInstractions.Sum(x => x.Value.Length);

        public IntPtr EndAddress => Address.Add(Size);

        public FrameInstractions Add(byte[] array)
        {
            var res = new FrameInstractions(array, this);
            _frameInstractions.Add(res);
            return res;
        }

        public byte[] ToArray()
        {
            return _frameInstractions.SelectMany(x => x.Value).ToArray();
        }

        public int GetOffset(FrameInstractions frameInstractions)
        {
            return _frameInstractions.TakeWhile(x => x != frameInstractions).Sum(x => x.Value.Length);
        }

        public byte[] GetOriginalBody(int length)
        {
            return _memory.GetOrigInstraction(Address, length);
        }

        public byte[] AddJMP(IntPtr address)
        {
            var jmp = EndAddress.Jmp(address);
            Add(jmp);
            return jmp;
        }

        public void FindAndReplaceCall(IntPtr address)
        {
            var result = FindAndReplaceCall(_memory, Address, address);
            Add(result);
        }

        public static byte[] FindAndReplaceCall(Memory memory, IntPtr methodAddress, IntPtr newMethod)
        {
            List<byte[]> resultInstruction = new List<byte[]>();
            int i = 0;
            Instruction[] instructions = memory.GetInstruction(methodAddress).ToArray();

            Instruction[] callInstructions = instructions.Reverse()
                .Where(x => x.Mnemonic == SharpDisasm.Udis86.ud_mnemonic_code.UD_Icall)
                .GroupBy(x => x.ToString())
                .Where(x => x.Count() == 2)
                .FirstOrDefault()
                .ToArray();

            foreach (var instruct in instructions)
            {
                if (callInstructions.Contains(instruct))
                {
                    var addressInstruction = methodAddress.Add(i);
                    var newCall = addressInstruction.Call(newMethod);
                    i += newCall.Length;
                    resultInstruction.Add(newCall);
                }
                else
                {
                    i += instruct.Length;
                    resultInstruction.Add(instruct.Bytes);
                }
            }

            byte[] result = resultInstruction
                  .SelectMany(x => x)
                  .ToArray();

            return result;
        }

        public void Install()
        {
            if (_maxSize != -1 && _maxSize < Size)
                throw new Exception($"Превышен размер инструкций записываемых в выделенную память Size:{Size} MaxSize:{_maxSize}");

            UnInstall();

            _original = _memory.Read(Address, Size);
            _memory.Write(Address, ToArray());
        }

        public void UnInstall()
        {
            if (_original == null)
                return;

            _memory.Write(Address, _original);
            _original = null;
        }

        public void WriteLog(bool whithOriginal = true)
        {
            var original = whithOriginal ? _memory.Read(Address, Size) : new byte[0];

            //WriteLines(original, ToArray(), Address);
        }

        public static void WriteLines(byte[] orig, byte[] newValue, IntPtr address)
        {
            string line = GetDisassemble(orig, newValue, address);
            Console.WriteLine(line);
        }

        public static string GetDisassemble(byte[] orig, byte[] newValue, IntPtr address)
        {
            string[] valueOrig = GetDisassemble(orig, address, false);

            string[] valueNew = GetDisassemble(newValue, address, true);

            StringBuilder result = new StringBuilder();
            for (int i = 0; i < valueOrig.Length || i < valueNew.Length; i++)
            {
                string origLine = i < valueOrig.Length ? valueOrig[i] : string.Empty;
                string newLine = i < valueNew.Length ? valueNew[i] : string.Empty;

                bool eq = origLine?.Split('|')?.LastOrDefault()?.Trim() == newLine?.Split('|')?.LastOrDefault()?.Trim();

                result.AppendLine($"{i,2}|{(eq ? " " : "X")}| {origLine}| {newLine.TrimEnd()}");
            }

            return result.ToString();
        }

        private static string[] GetDisassemble(byte[] orig, IntPtr address, bool addAddress)
        {
            if (orig.Length == 0)
                return new string[0];

            var instructions = new Disassembler(code: orig,
                       architecture: ArchitectureMode.x86_64,
                       address: (ulong)address,
                       copyBinaryToInstruction: true)
                       .Disassemble()
                       .ToArray();

            int pad = instructions.Length == 0 ? 0 : instructions.Max(x => x.Length) * 3;

            int i = 0;

            string[] valueOrig = instructions
                .Select(x => $"{(GetAddress(address, addAddress, x, ref i))}{x.Bytes.ToHexArray().PadRight(pad)} {x}")
                .ToArray();

            pad = valueOrig.Length == 0 ? 0 : valueOrig.Max(x => x.Length);

            valueOrig = valueOrig.Select(x => x.PadRight(pad)).ToArray();

            return valueOrig;
        }

        private static string GetAddress(IntPtr address, bool addAddress, Instruction x, ref int i)
        {
            string result = addAddress ? address.Add(i).ToHex() + " | " : "";
            i += x.Length;
            return result;
        }

        internal void CheckNop(int length)
        {
            int count = length - Size;
            if (count > 0)
            {
                byte[] newArray = new byte[count];
                for (int i = 0; i < count; i++)
                    newArray[i] = 0x90;

                Add(newArray);
            }
        }

        public void Dispose()
        {
            UnInstall();
            _memory.Dealloc(Address);
        }
    }
}