using EasyHook;
using NetHook.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

            string path = Path.GetTempPath();

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string dllPath = typeof(RemoteInjector).Assembly.Location;
            foreach (var assemble in asms)
            {
                try
                {
                    if (Directory.GetCurrentDirectory() != Path.GetDirectoryName(assemble.Location))
                        continue;

                    string newPath = Path.Combine(path, Path.GetFileName(assemble.Location));
                    if (dllPath == assemble.Location)
                        dllPath = newPath;

                    File.Copy(assemble.Location, newPath, true);
                }
                catch (Exception e)
                {
                }
            }

            RemoteHooking.Inject(process.Id, InjectionOptions.NoService | InjectionOptions.DoNotRequireStrongName,
                dllPath, dllPath, address);
        }

        public void Dispose() { }
    }
}
