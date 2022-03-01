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
            byte[] res = _frameInstractions.SelectMany(x => x.Value).ToArray();
            return res;
        }

        public byte[] ToArrayWhithNops()
        {
            byte[] res = _frameInstractions.SelectMany(x => x.Value).ToArray();

            if (_maxSize > 0)
            {
                byte[] result = new byte[_maxSize];

                for (int i = 0; i < res.Length; i++)
                    result[i] = res[i];

                for (int i = res.Length; i < _maxSize; i++)
                    result[i] = 0x90;

                res = result;
            }

            return res;
        }

        public int GetOffset(FrameInstractions frameInstractions)
        {
            return _frameInstractions.TakeWhile(x => x != frameInstractions).Sum(x => x.Value.Length);
        }

        public byte[] GetOriginalBody(int length)
        {
            return _memory.GetOrigBytes(Address, length);
        }

        public IEnumerable<Instruction> GetOriginalInstruction(int length)
        {
            return _memory.GetOrigInstraction(Address, length);
        }

        public byte[] AddJMP(IntPtr address)
        {
            var jmp = EndAddress.Jmp(address);
            Add(jmp);
            return jmp;
        }

        public byte[] AddJMP(long address)
        {
            var jmp = EndAddress.Jmp(address);
            Add(jmp);
            return jmp;
        }

        public byte[] AddJMP(int address)
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

        public void InjectOrig(byte[] origByte, IntPtr to)
        {
            var result = InjectOrig(_memory, Address, origByte, to);
            Add(result);
        }

        public byte[] ReadBytes(int size)
        {
            return _memory.GetOrigBytes(Address, size);
        }

        public static byte[] InjectOrig(Memory memory, IntPtr methodAddress, byte[] origByte, IntPtr to)
        {
            List<byte[]> resultInstruction = new List<byte[]>();
            Instruction[] instructions = memory.GetInstruction(methodAddress).ToArray();

            Instruction[][] callInstructionsGroups = instructions.Reverse()
                .Where(x => x.Mnemonic == SharpDisasm.Udis86.ud_mnemonic_code.UD_Icall)
                .GroupBy(x => x.ToString())
                .Where(x => x.Count() == 16)
                .Select(x => x.ToArray())
                .ToArray();

            Instruction[] callInstructions = callInstructionsGroups.Single().Reverse().ToArray();

            Instruction firstInstruction = callInstructions.First();
            Instruction lastInstruction = callInstructions.Last();
            IntPtr addressNewMem = methodAddress.Add(instructions.TakeWhile(x => x != firstInstruction).Select(x => x.Length).Sum());
            int size = (int)(lastInstruction.Offset - firstInstruction.Offset);

            resultInstruction.AddRange(instructions.TakeWhile(x => x != firstInstruction).Select(x => x.Bytes));

            MemoryInstractions memoryInstractions = new MemoryInstractions(addressNewMem, memory, size);

            memoryInstractions.Add(addressNewMem.Call(addressNewMem.Add(10)));
            memoryInstractions.AddJMP(new IntPtr((long)lastInstruction.Offset + 5));
            memoryInstractions.Add(origByte);
            memoryInstractions.AddJMP(to);

            resultInstruction.Add(memoryInstractions.ToArrayWhithNops());

            resultInstruction.AddRange(instructions.SkipWhile(x => x != lastInstruction).Select(x => x.Bytes));

            byte[] result = resultInstruction
                  .SelectMany(x => x)
                  .ToArray();

            return result;
        }

        public static byte[] FindAndReplaceCall(Memory memory, IntPtr methodAddress, IntPtr newMethod)
        {
            List<byte[]> resultInstruction = new List<byte[]>();
            int i = 0;
            Instruction[] instructions = memory.GetInstruction(methodAddress).ToArray();

            Instruction[][] callInstructionsGroups = instructions.Reverse()
                .Where(x => x.Mnemonic == SharpDisasm.Udis86.ud_mnemonic_code.UD_Icall)
                .GroupBy(x => x.ToString())
                .Where(x => x.Count() == 16)
                .Select(x => x.ToArray())
                .ToArray();

            Instruction[] callInstructions = callInstructionsGroups.Single();

            foreach (var instruct in instructions)
            {
                if (callInstructions.Contains(instruct))
                {
                    var addressInstruction = methodAddress.Add(i);
                    var newCall = addressInstruction.Call(newMethod);


                    if (newCall.Length != instruct.Length)
                    {
                        if (newCall.Length < instruct.Length)
                        {
                            byte[] newCallAndNoop = new byte[instruct.Length];

                            for (int noopindex = 0; noopindex < newCallAndNoop.Length; noopindex++)
                                newCallAndNoop[noopindex] = 0x90;

                            for (int noopindex = 0; noopindex < newCall.Length; noopindex++)
                                newCallAndNoop[noopindex] = newCall[noopindex];

                            newCall = newCallAndNoop;
                        }
                        else
                            throw new Exception("Не совпадает размер инструкций");
                    }

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

        public string WriteLog(bool whithOriginal = true)
        {
            var original = whithOriginal ? _memory.Read(Address, _maxSize > 0 ? _maxSize : Size) : new byte[0];
            return WriteLines(original, ToArray(), Address);
        }

        private static string WriteLines(byte[] orig, byte[] newValue, IntPtr address)
        {
            string line = GetDisassemble(orig, newValue, address);
            return line;
        }

        public static string GetDisassemble(byte[] orig, byte[] newValue, IntPtr address)
        {
            Instruction[] valueOrig = GetDisassemble(orig, address, false);

            Instruction[] valueNew = GetDisassemble(newValue, address, true);

            int maxInstruction = orig.Length > newValue.Length ? orig.Length : newValue.Length;

            InstructionAddress[] groupInstruction = new InstructionAddress[maxInstruction];

            List<InstructionAddress> instructionAddresses = new List<InstructionAddress>();

            StringBuilder result = new StringBuilder();

            AddInstructions(address, valueOrig, instructionAddresses, true);
            AddInstructions(address, valueNew, instructionAddresses, false);

            int padByteOrig = valueOrig.Length == 0 ? 0 : valueOrig.Max(x => x.Length) * 3;
            int padByteNew = valueNew.Length == 0 ? 0 : valueNew.Max(x => x.Length) * 3;
            int padOrigInstruction = valueOrig.Length == 0 ? 0 : valueOrig.Max(x => x.ToString().Length) + 1;
            int padNewInstruction = valueNew.Length == 0 ? 0 : valueNew.Max(x => x.ToString().Length) + 1;

            int i = 0;
            foreach (InstructionAddress instructionAddress in instructionAddresses.OrderBy(x => x.Address.ToInt64()))
            {
                string origLine = GetStringInstruction(instructionAddress.OrigInstruction, padByteOrig, padOrigInstruction);
                string newLine = GetStringInstruction(instructionAddress.NewInstruction, padByteNew, padNewInstruction);

                string eq = origLine?.Split('|')?.LastOrDefault()?.Trim() == newLine?.Split('|')?.LastOrDefault()?.Trim() ? " " : "X";

                result.AppendLine($"{i,2}|{eq}| {origLine}| {instructionAddress.Address.ToHex()} | {newLine.TrimEnd()}");
            }

            return result.ToString();
        }

        private static string GetStringInstruction(Instruction instruction, int padByte, int padInstruction)
        {
            if (instruction == null)
                return new string(' ', padByte + padInstruction + 1);
            return $"{instruction.Bytes.ToHexArray().PadRight(padByte)} {instruction.ToString().PadRight(padInstruction)}";
        }

        private static void AddInstructions(IntPtr address, Instruction[] instructions, List<InstructionAddress> instructionAddresses, bool isOrig)
        {
            IntPtr len = address;
            foreach (Instruction instruction in instructions)
            {
                InstructionAddress instructionAddress = instructionAddresses.FirstOrDefault(x => x.Address == len);
                if (instructionAddress == null)
                {
                    instructionAddress = new InstructionAddress() { Address = len };
                    instructionAddresses.Add(instructionAddress);
                }

                if (isOrig)
                    instructionAddress.OrigInstruction = instruction;
                else
                    instructionAddress.NewInstruction = instruction;

                len = len.Add(instruction.Length);
            }
        }

        public class InstructionAddress
        {
            public IntPtr Address { get; set; }

            public Instruction OrigInstruction { get; set; }

            public Instruction NewInstruction { get; set; }
        }

        private static Instruction[] GetDisassemble(byte[] orig, IntPtr address, bool addAddress)
        {
            if (orig.Length == 0)
                return new Instruction[0];

            Instruction[] instructions = new Disassembler(code: orig,
                       architecture: ArchitectureMode.x86_64,
                       address: (ulong)address,
                       copyBinaryToInstruction: true)
                       .Disassemble()
                       .ToArray();

            return instructions;
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
        }
    }
}