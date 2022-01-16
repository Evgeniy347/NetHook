using NetHook.Cores.Extensions;
using NetHook.Cores.Handlers.Trace;
using NetHook.Cores.Inject;
using SocketLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetHook.Cores.Socket
{
    public class LoggerServer : IDisposable
    {
        private List<ConnectedSocket> _connectedSockets = new List<ConnectedSocket>();
        private Thread _serverThread;
        private Thread _runTraceLog;

        public int Port { get; private set; }

        public void StartServer()
        {
            if (_serverThread != null)
                return;

            Thread thread = new Thread(() =>
            {
                try
                {
                    Port = FreePort();
                    Console.WriteLine($"StartServer Start {Port}");

                    using (var listener = new SocketListener(Port))
                    {
                        listener.UnderlyingSocket.ReceiveTimeout = 10000;
                        while (true)
                        {
                            try
                            {
                                ConnectedSocket remote = listener.Accept();
                                remote.UnderlyingSocket.ReceiveTimeout = 5000;
                                RunThreadReceive(remote);
                                Console.WriteLine($"Открыто соединение по сокету '{remote.UnderlyingSocket.RemoteEndPoint}'");
                            }
                            catch (System.Net.Sockets.SocketException ex)
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
            thread.Name = "StartServer";
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            _serverThread = thread;
            thread.Start();
        }

        private static int FreePort()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            int port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port;
        }

        public void RunThreadReceive(ConnectedSocket connectedSocket)
        {
            Thread thread = new Thread(() =>
            {
                RunReceive(connectedSocket);
            });
            thread.Name = "RunThreadReceive";
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        private void RunReceive(ConnectedSocket connectedSocket)
        {
            Console.WriteLine("RunReceive Start");
            while (true)
            {
                try
                {
                    MessageSocket message = connectedSocket.AcceptMessage();

                    if (message.MethodName == "SendModuleInfo")
                    {
                        DomainModelInfo domainModel = message.Body.DeserializeJSON<DomainModelInfo>();
                        SendModuleInfo(domainModel);

                        lock (_connectedSockets)
                            _connectedSockets.Add(connectedSocket);
                        return;
                    }
                    else if (message.MethodName == "Error")
                    {
                        WriteInjectError(message.Body);
                        return;
                    }
                    else
                    {
                        Console.WriteLine(message.RawData);
                    }
                }
                catch (System.Net.Sockets.SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        Thread.Sleep(100);
                        Console.WriteLine("RunReceive TimedOut");
                    }
                    else
                    {
                        Console.WriteLine(ex);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return;
                }
            }
        }

        public void SetHook(MethodModelInfo[] methods)
        {
            lock (_connectedSockets)
            {
                foreach (ConnectedSocket connectedSocket in _connectedSockets.ToArray())
                {
                    try
                    {
                        Console.WriteLine("SetHook");
                        string json = methods.SerializerJSON();
                        connectedSocket.SendMessage("SetHook", json);
                        MessageSocket messageReceive = connectedSocket.AcceptMessage();
                        Console.WriteLine(messageReceive.RawData);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        if (!connectedSocket.IsSocketConnected())
                        {
                            lock (_connectedSockets)
                                _connectedSockets.Remove(connectedSocket);
                        }
                    }
                }
            }
        }

        public void StopTraceLog()
        {
            Console.WriteLine("StopTraceLog");
            _runTraceLog?.Abort();
            _runTraceLog = null;
        }

        public void RunTraceLog()
        {
            if (_runTraceLog != null)
                return;

            Console.WriteLine("RunTraceLog");
            _runTraceLog = new Thread(() =>
            {
                while (true)
                {
                    if (_connectedSockets.Count > 0)
                    {
                        List<ThreadInfo> result = new List<ThreadInfo>();
                        lock (_connectedSockets)
                        {
                            foreach (ConnectedSocket connectedSocket in _connectedSockets.ToArray())
                            {
                                try
                                {
                                    connectedSocket.SendMessage("UploadThreadInfo", string.Empty);
                                    MessageSocket messageReceive = connectedSocket.AcceptMessage("UploadThreadInfo");

                                    ThreadInfo[] frameInfos = messageReceive.Body.DeserializeJSON<ThreadInfo[]>();
                                    if (frameInfos != null)
                                        result.AddRange(frameInfos);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                    if (!connectedSocket.IsSocketConnected())
                                    {
                                        lock (_connectedSockets)
                                            _connectedSockets.Remove(connectedSocket);
                                    }
                                }
                            }
                        }

                        ReceveTraceLog?.Invoke(result.ToArray());
                    }
                    Thread.Sleep(500);
                }
            });

            _runTraceLog.Name = "RunTraceLog";
            _runTraceLog.SetApartmentState(ApartmentState.STA);
            _runTraceLog.IsBackground = true;
            _runTraceLog.Start();
        }

        public void WriteInjectError(string message)
        {
            Console.WriteLine("WriteInjectError");
            OnInjectProcessError?.Invoke(message);
            Console.WriteLine(message);
        }

        public List<DomainModelInfo> DomainInfos { get; } = new List<DomainModelInfo>();

        private void SendModuleInfo(DomainModelInfo domainModel)
        {
            lock (DomainInfos)
            {
                DomainInfos.Add(domainModel);
                ChangeAssemble?.Invoke(GetAssembles());
            }
        }

        public AssembleModelInfo[] GetAssembles()
        {
            Console.WriteLine("GetAssembles");
            lock (DomainInfos)
            {
                return DomainInfos.SelectMany(x => x.Assemblies)
                .GroupBy(x => x.FullName)
                .Select(x => x.First())
                .ToArray();
            }
        }

        public void Dispose()
        {
            _serverThread.Abort();
        }

        public Action<AssembleModelInfo[]> ChangeAssemble;

        public Action<ThreadInfo[]> ReceveTraceLog;

        public Action<string> OnInjectProcessError;
    }
}
