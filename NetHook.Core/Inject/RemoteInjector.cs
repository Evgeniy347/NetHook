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
        private IpcServerChannel ServerChannel { get; set; }

        private LoggerProxy LoggerServer => LoggerProxy.Current;

        private void InjectIpcServer(Process process)
        {
            Close();

            string serverUrl = $"{typeof(RemoteInjector).FullName}";
            string outChannelName = $"ipc://{serverUrl}/LoggerServer";
            ServerChannel = new IpcServerChannel(serverUrl);
            ChannelServices.RegisterChannel(ServerChannel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(LoggerProxy), "LoggerServer", WellKnownObjectMode.Singleton);

            string dllPath = typeof(RemoteInjector).Assembly.Location;
            ServerChannel.StartListening(null);

            RemoteHooking.Inject(process.Id, InjectionOptions.NoService | InjectionOptions.DoNotRequireStrongName,
                dllPath, dllPath, outChannelName);
        }

        public void InjectSocketerver(Process process, int port)
        {
            Close();
            string dllPath = typeof(RemoteInjector).Assembly.Location;
            RemoteHooking.Inject(process.Id, InjectionOptions.NoService | InjectionOptions.DoNotRequireStrongName,
                dllPath, dllPath, port.ToString());
        }

        private void Close()
        {
            if (ServerChannel != null)
            {
                ServerChannel.StopListening(null);
                ChannelServices.UnregisterChannel(ServerChannel);
            }

            LoggerServer?.Reset();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
