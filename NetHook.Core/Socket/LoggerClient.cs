using NetHook.Cores.Extensions;
using NetHook.Cores.Inject;
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

        public void OpenChanel(string address, int port)
        {
            _connectedSocket = new ConnectedSocket(address, port);
        }

        public void OpenChanel()
        {
            _connectedSocket = new ConnectedSocket("127.0.0.1", 1339);
            _connectedSocket.UnderlyingSocket.ReceiveTimeout = 60 * 1000;
            _connectedSocket.UnderlyingSocket.ReceiveBufferSize = 60 * 1000;
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

        public string WaitServer(out string operation)
        {
            string raw = _connectedSocket.FullReceive().Trim('\"').Replace("\\\"", "\"");
            int indexType = raw.IndexOf("|");

            if (indexType == -1)
            {
                operation = raw;
                return string.Empty;
            }

            operation = raw.Substring(0, indexType);
            string objectJson = raw.Substring(indexType + 1);
            return objectJson;
        }

        public void Dispose()
        {
            _connectedSocket?.Dispose();
        }

    }
}
