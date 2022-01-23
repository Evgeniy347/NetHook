using NetHook.Cores.Extensions;
using NetHook.Cores.Handlers.Trace;
using NetHook.Cores.Inject;
using NetHook.Cores.Inject.AssemblyModel;
using SocketLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetHook.Cores.Socket
{
    public class LoggerServer : IDisposable
    {
        private List<ConnectedSocket> _connectionSockets = new List<ConnectedSocket>();
        private List<ConnectedSocket> _injectSocket = new List<ConnectedSocket>();
        private Thread _serverThread;
        private Thread _runTraceLog;
        private MethodModelInfo[] _methodsHook;

        public int CountConnection
        {
            get
            {
                lock (_connectionSockets)
                    return _connectionSockets.Count;
            }
        }

        public string Address { get; private set; }

        public void StartServer()
        {
            if (_serverThread != null)
                return;

            _serverThread = new Thread(() =>
            {
                try
                {
                    int port = FreePort();
                    Console.WriteLine($"StartServer Start {Address}");

                    using (var listener = new SocketListener(port))
                    {
                        listener.UnderlyingSocket.ReceiveBufferSize = int.MaxValue;
                        listener.UnderlyingSocket.SendBufferSize = int.MaxValue;
                        Address = listener.UnderlyingSocket.LocalEndPoint.ToString();
                        listener.UnderlyingSocket.ReceiveTimeout = 10000;
                        while (true)
                        {
                            try
                            {
                                ConnectedSocket remote = listener.Accept();
                                remote.UnderlyingSocket.ReceiveTimeout = 5000;
                                remote.UnderlyingSocket.ReceiveBufferSize = int.MaxValue;
                                remote.UnderlyingSocket.SendBufferSize = int.MaxValue;
                                RunThreadGetAssembly(remote);
                                Console.WriteLine($"Открыто соединение по сокету '{remote.UnderlyingSocket.RemoteEndPoint}'");
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });

            _serverThread.Name = "StartServer";
            _serverThread.SetApartmentState(ApartmentState.STA);
            _serverThread.IsBackground = true;
            _serverThread.Start();

        }

        private void RunInjectThread(ConnectedSocket connectedSocket)
        {
            Thread thread = new Thread(() =>
            {
                while (connectedSocket.IsSocketConnected())
                {
                    try
                    {
                        MessageSocket message = connectedSocket.AcceptMessage();
                        Console.WriteLine(message.RawData);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            });

            thread.Name = "InjectThread";
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
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

        public void RunThreadsGetAssembly()
        {
            foreach (var socket in _connectionSockets)
                RunThreadGetAssembly(socket);
        }

        public void RunThreadGetAssembly(ConnectedSocket connectedSocket)
        {
            Thread thread = new Thread(() =>
            {
                GetAssemblyInfo(connectedSocket);
            });
            thread.Name = "GetAssemblyInfo";
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        private void GetAssemblyInfo(ConnectedSocket connectedSocket)
        {
            Console.WriteLine("RunReceive Start");
            while (true)
            {
                try
                {
                    MessageSocket message = null;
                    lock (connectedSocket)
                    {
                        connectedSocket.SendMessage("SendModuleInfo", "");
                        message = connectedSocket.AcceptMessage();
                    }

                    if (message.MethodName == "InjectDomain")
                    {
                        Console.WriteLine(message.RawData);
                        _injectSocket.Add(connectedSocket);
                        RunInjectThread(connectedSocket);
                        return;
                    }
                    else if (message.MethodName == "SendModuleInfo")
                    {
                        DomainModelInfo domainModel = message.Body.DeserializeJSON<DomainModelInfo>();
                        SendModuleInfo(domainModel);

                        lock (_connectionSockets)
                        {
                            if (!_connectionSockets.Contains(connectedSocket))
                            {
                                _connectionSockets.Add(connectedSocket);
                                if (_methodsHook != null && _methodsHook.Length > 0)
                                    SetHook(_methodsHook, connectedSocket);
                            }
                        }
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
                catch (SocketException ex)
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
            lock (_connectionSockets)
            {
                foreach (ConnectedSocket connectedSocket in _connectionSockets.ToArray())
                    SetHook(methods, connectedSocket);

                _methodsHook = methods;
            }
        }

        private void SetHook(MethodModelInfo[] methods, ConnectedSocket connectedSocket)
        {
            try
            {
                string json = methods.SerializerJSON();

                lock (connectedSocket)
                {
                    connectedSocket.SendMessage("SetHook", json);
                    MessageSocket messageReceive = connectedSocket.AcceptMessage();
                    Console.WriteLine(messageReceive.RawData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (!connectedSocket.IsSocketConnected())
                {
                    lock (_connectionSockets)
                        _connectionSockets.Remove(connectedSocket);
                }
            }
        }

        public void StopTraceLog()
        {
            Console.WriteLine("StopTraceLog");

            if (_runTraceLog != null)
            {
                Thread thread = _runTraceLog;
                _runTraceLog = null;
                thread.Join();
            }
        }

        public void RunTraceLog()
        {
            if (_runTraceLog != null)
                return;

            Console.WriteLine("RunTraceLog");
            _runTraceLog = new Thread(() =>
            {
                while (_runTraceLog != null)
                {
                    if (_connectionSockets.Count > 0)
                    {
                        List<ThreadInfo> result = new List<ThreadInfo>();
                        lock (_connectionSockets)
                        {
                            foreach (ConnectedSocket connectedSocket in _connectionSockets.ToArray())
                            {
                                try
                                {
                                    MessageSocket messageReceive = null;

                                    lock (connectedSocket)
                                    {
                                        connectedSocket.SendMessage("UploadThreadInfo", string.Empty);
                                        messageReceive = connectedSocket.AcceptMessage("UploadThreadInfo");
                                    }

                                    ThreadInfo[] frameInfos = messageReceive.Body.DeserializeJSON<ThreadInfo[]>();
                                    if (frameInfos != null)
                                        result.AddRange(frameInfos);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                    if (!connectedSocket.IsSocketConnected())
                                    {
                                        lock (_connectionSockets)
                                            _connectionSockets.Remove(connectedSocket);
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
            foreach (var assemble in domainModel.Assemblies.Where(x => !string.IsNullOrEmpty(x.ErrorText)))
                Console.WriteLine(assemble.ErrorText);

            lock (DomainInfos)
                DomainInfos.Add(domainModel);

            ChangeAssemble?.Invoke(GetAssembles());
        }

        public AssembleModelInfo[] GetAssembles()
        {
            Console.WriteLine("GetAssembles");
            lock (DomainInfos)
            {
                return DomainInfos.SelectMany(x => x.Assemblies)
                .GroupBy(x => x.FullName)
                .Select(x => x.FirstOrDefault(y => string.IsNullOrEmpty(y.ErrorText)))
                .Where(x => x != null)
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
