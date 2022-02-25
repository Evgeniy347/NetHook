using NetHook.Cores.Extensions;
using NetHook.Cores.Handlers;
using NetHook.Cores.Handlers.Trace;
using NetHook.Cores.Inject;
using NetHook.Cores.Inject.AssemblyModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetHook.Cores.NetSocket
{
    public class LoggerServer : IDisposable
    {
        private MethodModelInfo[] _methodsHook;
        private SocketServer _socketServer;
        private Thread _runTraceLog;

        public int CountConnection => _socketServer.CountConnection;

        public string Address => _socketServer.Address;

        public void StartServer()
        {
            _socketServer = new SocketServer();
            _socketServer.StartServer();

            _socketServer.OnInitSocket += (s) =>
            {
                if (_methodsHook != null)
                {
                    string log = s.SendWithReceve<MethodModelInfo[], string>("SetHook", _methodsHook);
                    Console.WriteLine(log);
                }
            };

            _socketServer.HandlerRequest.Add("WriteInjectError", (x) => { Console.WriteLine(x.RawData); return null; });
            _socketServer.HandlerRequest.Add("NewInjectDomain", (x) => { Console.WriteLine(x.RawData); return null; });
        }

        public void SetHook(MethodModelInfo[] methods)
        {
            Console.WriteLine("SetHook");
            _methodsHook = methods;
            List<string> responce = _socketServer.SendWithReceve<MethodModelInfo[], string>("SetHook", methods);

            Console.WriteLine(responce.JoinString());
        }

        public void StopTraceLog()
        {
            Console.WriteLine("StopTraceLog");

            if (_runTraceLog != null)
            {
                Thread thread = _runTraceLog;
                _runTraceLog = null;
                thread.Abort();
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
                    if (_socketServer.CountConnection > 0)
                    {
                        ThreadInfo[] result = _socketServer.SendWithReceve<string, ThreadInfo[]>("UploadThreadInfo", string.Empty)
                            .SelectMany(x => x)
                            .ToArray();
                        ReceveTraceLog?.Invoke(result);
                    }

                    Thread.Sleep(500);
                }
            });

            _runTraceLog.Name = "RunTraceLog";
            _runTraceLog.SetApartmentState(ApartmentState.STA);
            _runTraceLog.IsBackground = true;
            _runTraceLog.Start();
        }

        public void GetInjectInfo()
        {
            Console.WriteLine("GetInjectInfo");
            List<string> result = _socketServer.SendWithReceve<string, string>("GetInjectInfo", string.Empty);

            foreach (string info in result)
                Console.WriteLine(info);
        }

        public AssembleModelInfo[] GetAssembles()
        {
            Console.WriteLine("GetAssembles");

            List<DomainModelInfo> domains = _socketServer.SendWithReceve<string, DomainModelInfo>("GetAssemblyInfo", string.Empty);

            return domains.SelectMany(x => x.Assemblies)
                .GroupBy(x => x.FullName)
                .Select(x => x.FirstOrDefault(y => string.IsNullOrEmpty(y.ErrorText)))
                .Where(x => x != null)
                .ToArray();
        }

        public void Dispose()
        {
            _socketServer.Dispose();
        }

        public Action<ThreadInfo[]> ReceveTraceLog;

    }
}
