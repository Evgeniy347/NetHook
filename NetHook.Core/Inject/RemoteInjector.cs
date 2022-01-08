using NetHook.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetHook.Cores.Inject
{
    public class RemoteInjector : IDisposable
    {
        public Memory Memory { get; } = new Memory();


        public void OpenProcess(Process process)
        {
            Memory.Open(process);
        }

        public void Inject()
        {
            string dllPath = //@"C:\Users\GTR\source\repos\NetHook\NetHook\Debug\InjectDll.dll";
            typeof(RemoteInjector).Assembly.Location;

            Memory.InjectDLL(dllPath);
        }

        public void Dispose()
        {
            Memory.CloseHandle();
        }
    }
}
