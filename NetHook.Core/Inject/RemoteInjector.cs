using EasyHook;
using NetHook.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading;

namespace NetHook.Cores.Inject
{
    public class RemoteInjector : IDisposable
    {

        public void InjectSocketerver(Process process, string address)
        {
            string dllPath = typeof(RemoteInjector).Assembly.Location;

            RemoteHooking.Inject(process.Id, InjectionOptions.NoService | InjectionOptions.DoNotRequireStrongName,
                dllPath, dllPath, address);
        }

        public void Dispose() { }
    }
}
