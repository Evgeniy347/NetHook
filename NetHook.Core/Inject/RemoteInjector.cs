using EasyHook;
using NetHook.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public IpcServerChannel ServerChannel { get; private set; }

        public LoggerInterface LoggerServer { get; private set; } = new LoggerInterface();

        public void Inject(Process process)
        {
            string outChannelName = $"ipc://ServerChannel_{Process.GetCurrentProcess().Id}/ServerMethods";
            ServerChannel = new IpcServerChannel($"ServerChannel_{Process.GetCurrentProcess().Id}");
            ChannelServices.RegisterChannel(ServerChannel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(LoggerInterface), "ServerMethods", WellKnownObjectMode.Singleton);

            RemotingServices.Marshal(LoggerServer, $"ServerChannel_{Process.GetCurrentProcess().Id}");
            string dllPath = typeof(RemoteInjector).Assembly.Location;

            RemoteHooking.Inject(process.Id, InjectionOptions.NoService | InjectionOptions.DoNotRequireStrongName,
                dllPath, dllPath, outChannelName);
        }

        public void Dispose()
        {
            ChannelServices.UnregisterChannel(ServerChannel);
            LoggerServer.Reset();
        }
    }
}
