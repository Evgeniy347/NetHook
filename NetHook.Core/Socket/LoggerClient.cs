using NetHook.Cores.Extensions;
using NetHook.Cores.Inject;
using NetHook.Cores.Inject.AssemblyModel;
using SocketLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetHook.Cores.Socket
{
    public class LoggerClient : IDisposable
    {
        private ConnectedSocket _connectedSocket;
        public ConnectedSocket Socket => _connectedSocket;
        public LoggerClient()
        { }

        public void OpenChanel(string address)
        {
            string[] addressParts = address.Split(':');
            _connectedSocket = new ConnectedSocket(addressParts[0], int.Parse(addressParts[1]));
            _connectedSocket.UnderlyingSocket.ReceiveTimeout = 60 * 1000;
            _connectedSocket.UnderlyingSocket.ReceiveBufferSize = int.MaxValue;
            _connectedSocket.UnderlyingSocket.SendBufferSize = int.MaxValue;
        }

        public MessageSocket AcceptMessage()
        {
            return _connectedSocket.AcceptMessage();
        }

        internal void UploadThreadInfo(ThreadInfo[] package)
        {
            _connectedSocket.SendMessage("UploadThreadInfo", package);
        }

        public void SendModuleInfo(DomainModelInfo domainModelInfo)
        {
            _connectedSocket.SendMessage($"SendModuleInfo", domainModelInfo);
        }

        public void Send(string message)
        {
            _connectedSocket.SendMessage("Info", message);
        }

        public void WriteInjectError(string error)
        {
            _connectedSocket.SendMessage($"Error", error);
        }

        public void Dispose()
        {
            _connectedSocket?.Dispose();
        }

        internal void SetInjectConnection(string message)
        {
            _connectedSocket.SendMessage($"InjectDomain", message);
        }
    }
}
