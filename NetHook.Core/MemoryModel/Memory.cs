using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace NetHook.Core
{
    public class Memory
    {
        public Memory()
        {
        }

        public Process ReadProcess { get; private set; }

        private IntPtr m_hProcess = IntPtr.Zero;

        public void Open(Process process)
        {
            if (m_hProcess != IntPtr.Zero)
                throw new Exception("Уже открыт другой процесс");

            ReadProcess = process;

            ProcessAccessType access = ProcessAccessType.PROCESS_VM_READ
         | ProcessAccessType.PROCESS_VM_WRITE
         | ProcessAccessType.PROCESS_VM_OPERATION;
            m_hProcess = MemoryAPI.OpenProcess((uint)access, 1, (uint)process.Id);
        }

        public void CloseHandle()
        {
            if (m_hProcess == IntPtr.Zero)
                return;

            if (!MemoryAPI.CloseHandle(m_hProcess))
                throw new Win32Exception("CloseHandle Failed");
        }

        public byte[] Read(IntPtr MemoryAddress, int bytesToRead)
        {
            return ReadEnumerable(MemoryAddress, bytesToRead).ToArray();
        }


        public IEnumerable<byte> ReadEnumerable(IntPtr MemoryAddress, int bytesToRead)
        {
            int count = bytesToRead;
            while (true)
            {
                int bufSize = bytesToRead < count ? bytesToRead : count;
                byte[] buffer = new byte[bufSize];
                IntPtr ptrBytesRead;
                if (!MemoryAPI.ReadProcessMemory(m_hProcess, MemoryAddress, buffer, bufSize, out ptrBytesRead))
                {
                    WriteError();
                    break;
                }

                int bytesRead = ptrBytesRead.ToInt32();

                for (int i = 0; i < bytesRead; i++)
                    yield return buffer[i];

                if (bytesRead < count)
                    break;

                break;
            }
        }

        public void InjectDLL(string strDLLName)
        {
            var t1 = ReadProcess.Modules.Cast<ProcessModule>().Select(x => x.ModuleName).ToArray();

            int lenWrite = strDLLName.Length + 1;
            IntPtr allocMem = Alloc(lenWrite);

            MemoryAPI.WriteProcessMemory(m_hProcess, allocMem, strDLLName, (uint)lenWrite, out IntPtr bytesout);

            IntPtr injector = MemoryAPI.GetProcAddress(MemoryAPI.GetModuleHandle("kernel32.dll"), "LoadLibraryA");

            if (injector == null)
                throw new Win32Exception("Injecto Error!");

            IntPtr hThread = MemoryAPI.CreateRemoteThread(m_hProcess, (IntPtr)null, 0, injector, allocMem, 0, out uint bytesout2);
            if (hThread == IntPtr.Zero)
                throw new Win32Exception("Thread injection Failed");

            IntPtr result = MemoryAPI.WaitForSingleObject(hThread, 10 * 1000);
            if (result == new IntPtr(0x00000080L) || result == new IntPtr(0x00000102L) || result == new IntPtr(0xFFFFFFFF))
                throw new Win32Exception("Thread 2 inject failed");

            Thread.Sleep(1000);
            MemoryAPI.VirtualFreeEx(m_hProcess, allocMem, 0, FreeType.Release);

            var h = MemoryAPI.GetModuleHandle("NetHook.Core.exe");
            if (h == IntPtr.Zero)
                throw new Win32Exception();

            IntPtr injectorMain = MemoryAPI.GetProcAddress(h, "Main2");
            if (injectorMain == IntPtr.Zero)
                throw new Win32Exception();

            IntPtr hThread2 = MemoryAPI.CreateRemoteThread(m_hProcess, (IntPtr)null, 0, injectorMain, IntPtr.Zero, 0, out uint bytesout23);
            if (hThread2 == IntPtr.Zero)
                throw new Win32Exception("Thread injection Failed");

            var t2 = Process.GetProcessById(ReadProcess.Id).Modules.Cast<ProcessModule>().Select(x => x.ModuleName).ToArray();

            var res = t2.Except(t1).ToArray();
        }

        public void CreateRemoteThread(IntPtr address)
        {
            IntPtr hThread = MemoryAPI.CreateRemoteThread(m_hProcess, (IntPtr)null, 0, address, IntPtr.Zero, 0, out uint bytesout);
            if (hThread == IntPtr.Zero)
                throw new Win32Exception("Thread injection Failed");
        }

        private static void WriteError()
        {
            int error = Marshal.GetLastWin32Error();
            Console.WriteLine($"error: {error} message:{new Win32Exception(error).Message}");
        }

        public byte[] PointerRead(IntPtr MemoryAddress, int bytesToRead, int[] Offset, out int bytesRead)
        {
            int iPointerCount = Offset.Length - 1;
            IntPtr ptrBytesRead;
            bytesRead = 0;
            byte[] buffer = new byte[4];
            int tempAddress = 0;

            if (iPointerCount == 0)
            {
                MemoryAPI.ReadProcessMemory(m_hProcess, MemoryAddress, buffer, 4, out ptrBytesRead);
                tempAddress = Addr.ToDec(Addr.Make(buffer)) + Offset[0];

                buffer = new byte[bytesToRead];
                MemoryAPI.ReadProcessMemory(m_hProcess, (IntPtr)tempAddress, buffer, bytesToRead, out ptrBytesRead);

                bytesRead = ptrBytesRead.ToInt32();
                return buffer;
            }

            for (int i = 0; i <= iPointerCount; i++)
            {
                if (i == iPointerCount)
                {
                    MemoryAPI.ReadProcessMemory(m_hProcess, (IntPtr)tempAddress, buffer, 4, out ptrBytesRead);
                    tempAddress = Addr.ToDec(Addr.Make(buffer)) + Offset[i]; //Final Address

                    buffer = new byte[bytesToRead];
                    MemoryAPI.ReadProcessMemory(m_hProcess, (IntPtr)tempAddress, buffer, bytesToRead, out ptrBytesRead);

                    bytesRead = ptrBytesRead.ToInt32();
                    return buffer;
                }
                else if (i == 0)
                {
                    MemoryAPI.ReadProcessMemory(m_hProcess, MemoryAddress, buffer, 4, out ptrBytesRead);
                    tempAddress = Addr.ToDec(Addr.Make(buffer)) + Offset[1];
                }
                else
                {
                    MemoryAPI.ReadProcessMemory(m_hProcess, (IntPtr)tempAddress, buffer, 4, out ptrBytesRead);
                    tempAddress = Addr.ToDec(Addr.Make(buffer)) + Offset[i];
                }
            }

            return buffer;
        }

        public void Write(IntPtr MemoryAddress, byte[] bytesToWrite, out int bytesWritten)
        {
            IntPtr ptrBytesWritten;
            if (!MemoryAPI.WriteProcessMemory(m_hProcess, MemoryAddress, bytesToWrite, (uint)bytesToWrite.Length, out ptrBytesWritten))
                throw new Win32Exception();
            bytesWritten = ptrBytesWritten.ToInt32();
        }

        public string PointerWrite(IntPtr MemoryAddress, byte[] bytesToWrite, int[] Offset, out int bytesWritten)
        {
            int iPointerCount = Offset.Length - 1;
            IntPtr ptrBytesWritten;
            bytesWritten = 0;
            byte[] buffer = new byte[4]; //DWORD to hold an Address
            int tempAddress = 0;

            if (iPointerCount == 0)
            {
                MemoryAPI.ReadProcessMemory(m_hProcess, MemoryAddress, buffer, 4, out ptrBytesWritten);
                tempAddress = Addr.ToDec(Addr.Make(buffer)) + Offset[0]; //Final Address
                MemoryAPI.WriteProcessMemory(m_hProcess, (IntPtr)tempAddress, bytesToWrite, (uint)bytesToWrite.Length, out ptrBytesWritten);

                bytesWritten = ptrBytesWritten.ToInt32();
                return Addr.ToHex(tempAddress);
            }

            for (int i = 0; i <= iPointerCount; i++)
            {
                if (i == iPointerCount)
                {
                    MemoryAPI.ReadProcessMemory(m_hProcess, (IntPtr)tempAddress, buffer, 4, out ptrBytesWritten);
                    tempAddress = Addr.ToDec(Addr.Make(buffer)) + Offset[i]; //Final Address
                    MemoryAPI.WriteProcessMemory(m_hProcess, (IntPtr)tempAddress, bytesToWrite, (uint)bytesToWrite.Length, out ptrBytesWritten);

                    bytesWritten = ptrBytesWritten.ToInt32();
                    return Addr.ToHex(tempAddress);
                }
                else if (i == 0)
                {
                    MemoryAPI.ReadProcessMemory(m_hProcess, MemoryAddress, buffer, 4, out ptrBytesWritten);
                    tempAddress = Addr.ToDec(Addr.Make(buffer)) + Offset[i];
                }
                else
                {
                    MemoryAPI.ReadProcessMemory(m_hProcess, (IntPtr)tempAddress, buffer, 4, out ptrBytesWritten);
                    tempAddress = Addr.ToDec(Addr.Make(buffer)) + Offset[i];
                }
            }

            return Addr.ToHex(tempAddress);
        }

        public IntPtr Alloc(int size, IntPtr adress = default)
        {
            IntPtr result = MemoryAPI.VirtualAllocEx(m_hProcess, adress, size, AllocType.Commit, Protect.ExecuteReadWrite);

            if (result == IntPtr.Zero)
                throw new Win32Exception();

            return result;
        }

        public bool Dealloc(IntPtr addr)
        {
            return MemoryAPI.VirtualFreeEx(m_hProcess, addr, 0, FreeType.Release);
        }

        public int PID()
        {
            return ReadProcess.Id;
        }

        public string BaseAddressH()
        {
            return Addr.ToHex(ReadProcess.MainModule.BaseAddress.ToInt32());
        }

        public int BaseAddressD()
        {
            return ReadProcess.MainModule.BaseAddress.ToInt32();
        }
    }
}
