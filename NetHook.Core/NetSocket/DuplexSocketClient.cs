using NetHook.Cores.Extensions;
using System;

namespace NetHook.Cores.NetSocket
{
    public class DuplexSocketClient : DuplexSocket
    {
        public DuplexSocketClient()
        { }

        public void OpenChanel(string host, int port)
        {
            _connectedSocketSender = ConnectSoccet(host, port);
            _connectedSocketSender.SetDefaultProperty();

            _connectedSocketListener = ConnectSoccet(host, port);
            _connectedSocketListener.SetDefaultProperty();

            string guid = Guid.NewGuid().ToString();
            _connectedSocketSender.SendMessage("SetSender", guid, int.MaxValue, TypeMessage.Request);
            _connectedSocketListener.SendMessage("SetListener", guid, int.MaxValue, TypeMessage.Request);

            RunListenThread();
            RunProcessQueue();
        }
    }
}
