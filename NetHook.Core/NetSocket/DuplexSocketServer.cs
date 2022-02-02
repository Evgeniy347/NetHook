using System.Net.Sockets;

namespace NetHook.Cores.NetSocket
{
    public class DuplexSocketServer : DuplexSocket
    {
        public DuplexSocketServer(Socket sender, Socket listener)
        {
            _connectedSocketSender = sender;
            _connectedSocketListener = listener;
            RunListenThread();
            RunProcessQueue();
        }
    }
}
