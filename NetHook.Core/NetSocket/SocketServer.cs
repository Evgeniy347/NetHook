using NetHook.Cores.Extensions;
using NetHook.Cores.Handlers;
using NetHook.Cores.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetHook.Cores.NetSocket
{
    public class SocketServer : IDisposable
    {
        private Thread _serverThread;
        private Socket _listener;

        public Action<DuplexSocketServer> OnInitSocket;

        private ConcurentList<DuplexSocketServer> _duplexSockets = new ConcurentList<DuplexSocketServer>();

        private Dictionary<string, Socket> _serverListenThreads = new Dictionary<string, Socket>();
        public Dictionary<string, Func<MessageSocket, object>> HandlerRequest { get; } = new Dictionary<string, Func<MessageSocket, object>>();

        public string Address => _listener?.LocalEndPoint?.ToString();

        public int CountConnection => _duplexSockets.Count;

        private bool _disposed;

        public void StartServer()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DuplexSocket));

            if (_serverThread != null)
                throw new Exception("Сервер уже запущен");

            _serverThread = ThreadHelper.RunLogic(() =>
             {
                 int port = FreePort();

                 using (_listener = SocketListener(port))
                 {
                     Console.WriteLine($"StartServer Start {Address}");
                     _listener.SetDefaultProperty();
                     while (true)
                     {
                         try
                         {
                             Socket remote = _listener.Accept();
                             remote.SetDefaultProperty();
                             RunListenThread(remote);
                             Console.WriteLine($"Открыто соединение по сокету '{remote.RemoteEndPoint}'");
                         }
                         catch (SocketException ex)
                         {
                             if (ex.SocketErrorCode == SocketError.TimedOut)
                                 Console.WriteLine("ConnectedSocket TimedOut");
                             else
                                 Console.WriteLine(ex);
                         }
                         catch (Exception ex)
                         {
                             Console.WriteLine(ex);
                         }
                         finally
                         {
                             Thread.Sleep(100);
                         }
                     }
                 }
             }, "StartServer");
        }

        private Socket SocketListener(int port, string ip = "127.0.0.1", int backlog = 10)
        {
            IPAddress address = IPAddress.Parse(ip);
            IPEndPoint localEP = new IPEndPoint(address, port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEP);
            socket.Listen(backlog);

            return socket;
        }

        public void AddProcessHandler(string methodName, Func<MessageSocket, object> fun)
        {
            HandlerRequest.Add(methodName, fun);
        }

        private void RunListenThread(Socket connectedSocket)
        {
            ThreadHelper.RunLogic(() =>
            {
                if (_disposed || !connectedSocket.IsSocketConnected())
                    return;

                MessageSocket message = connectedSocket.AcceptMessage();
                Console.WriteLine(message.RawData);

                if (message.MethodName == "SetListener" || message.MethodName == "SetSender")
                {
                    lock (_serverListenThreads)
                    {
                        if (_serverListenThreads.TryGetValue(message.Body, out Socket socket))
                        {
                            Socket listener = message.MethodName == "SetSender" ? connectedSocket : socket;
                            Socket sender = message.MethodName == "SetSender" ? socket : connectedSocket;
                            Console.WriteLine($"sender:{listener.GetKey()} listener:{sender.GetKey()}");

                            DuplexSocketServer duplexSocket = new DuplexSocketServer(sender, listener);
                            _duplexSockets.Add(duplexSocket);
                            duplexSocket.HandlerRequest = HandlerRequest;
                            OnInitSocket?.Invoke(duplexSocket);

                            _serverListenThreads.Remove(message.Body);
                        }
                        else
                            _serverListenThreads[message.Body] = connectedSocket;
                    }
                }
                else
                {
                    throw new Exception();
                }

            }, $"ListenThread {connectedSocket.RemoteEndPoint}");
        }

        public List<TResponce> SendWithReceve<TRequest, TResponce>(string method, TRequest body)
        {
            Dictionary<int, DuplexSocketServer> value = new Dictionary<int, DuplexSocketServer>(_duplexSockets.Count);

            foreach (DuplexSocketServer socketServer in _duplexSockets)
            {
                try
                {
                    int id = socketServer.SendMessage(method, body);
                    value[id] = socketServer;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    if (!socketServer.IsSocketConnected())
                        _duplexSockets.Remove(socketServer);
                }
            }

            List<TResponce> responces = new List<TResponce>(value.Count);

            foreach (var keyValue in value)
            {
                try
                {
                    TResponce responce = keyValue.Value.Receve<TResponce>(keyValue.Key);
                    if (responce != null)
                        responces.Add(responce);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    if (!keyValue.Value.IsSocketConnected())
                        _duplexSockets.Remove(keyValue.Value);
                }
            }

            return responces;
        }

        public void Dispose()
        {
            _disposed = true;
            _listener.Dispose();
            _serverThread.Join();

            foreach (var keyValue in _serverListenThreads)
            {
                try
                {
                    keyValue.Value.DisposeSocket();
                }
                catch { }
            }
            _serverListenThreads.Clear();

            foreach (var sockets in _duplexSockets)
            {
                try
                {
                    sockets.Dispose();
                }
                catch { }
            }
        }

        private static int FreePort()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            int port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port;
        }
    }
}
