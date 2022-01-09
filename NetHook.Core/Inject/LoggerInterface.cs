using NetHook.Cores.Handlers.Trace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace NetHook.Cores.Inject
{
    public class LoggerInterface : MarshalByRefObject
    {

        static public bool Online { get; private set; }

        static private readonly AutoResetEvent _evt = new AutoResetEvent(false);

        static public List<DomainModelInfo> DomainInfos { get; } = new List<DomainModelInfo>();

        public void OnCreateFile(TraceFrameInfo[] traceFrames)
        {
            if (traceFrames == null)
                return;

            OnCreateFileInternal(traceFrames);
        }

        public void OnCreateFileInternal(TraceFrameInfo[] traceFrames)
        {

            for (int i = 0; i < traceFrames.Length; i++)
            {
                Console.WriteLine(traceFrames[i].MethodName + " " + traceFrames[i].Elapsed);
                OnCreateFileInternal(traceFrames[i].ChildFrames);
            }
        }

        public void SendModuleInfo(DomainModelInfo domainInfo)
        {
            Online = true;
            DomainInfos.Add(domainInfo);
            _evt.Reset();
        }

        public AssembleModelInfo[] GetAssembles()
        {
            return DomainInfos.SelectMany(x => x.Assemblies)
                .GroupBy(x => x.FullName)
                .Select(x => x.First())
                .ToArray();
        }

        internal void Reset()
        {
            Online = false;
            DomainInfos.Clear();
        }

        public void WaitOnline()
        {
            while (!Online)
                _evt.WaitOne(1000);
        }
    }
}
