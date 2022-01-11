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
        public LoggerInterface()
        {
            Current = this;
        }

        public DateTime ChangeDateHook { get; set; }

        private static readonly AutoResetEvent _evt = new AutoResetEvent(false);

        private MethodModelInfo[] _methods { get; set; }

        public List<DomainModelInfo> DomainInfos { get; } = new List<DomainModelInfo>();

        public void UploadThreadInfo(ThreadInfo[] threadInfos)
        {
            if (threadInfos == null)
                return;

            UploadFrameIternal(threadInfos.SelectMany(x => x.Frames).ToArray());
            OnTraceLoad?.Invoke(threadInfos);

        }

        public void UploadFrameIternal(TraceFrameInfo[] frameInfos)
        {
            foreach (var frame in frameInfos)
            {
                Console.WriteLine(frame.MethodName + " " + frame.Elapsed);
                UploadFrameIternal(frame.ChildFrames);
            }
        }

        public void SendModuleInfo(DomainModelInfo domainInfo)
        {
            DomainInfos.Add(domainInfo);
            _evt.Reset();
        }

        public void SetHook(MethodModelInfo[] methods)
        {
            ChangeDateHook = DateTime.Now;
            _methods = methods;
        }

        public MethodModelInfo[] GetHook()
        {
            return _methods;
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
            DomainInfos.Clear();
        }

        public static void WaitOnline()
        {
            while (Current == null)
                _evt.WaitOne(1000);
        }

        public Action<ThreadInfo[]> OnTraceLoad;

        public static LoggerInterface Current;
    }

    [Serializable]
    public class ThreadInfo
    {
        public TraceFrameInfo[] Frames { get; set; }

        public int ThreadID { get; set; }

        public ThreadState ThreadState { get; set; }
    }
}
