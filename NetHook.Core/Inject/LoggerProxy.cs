using NetHook.Cores.Extensions;
using NetHook.Cores.Handlers.Trace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace NetHook.Cores.Inject
{
    public class LoggerProxy : MarshalByRefObject, IDisposable
    {
        public LoggerProxy()
        {
            if (Current != null)
                Current.Dispose();

            Current = this;
            _evt.Reset();
        }

        public Action<ThreadInfo[]> OnTraceLoad;

        public static Action<int, string, string> OnInjectDomainError;
        public static Action<string, string> OnInjectProcessError;

        public DateTime ChangeDateHook { get; set; }

        public bool AllDomainInject { get; set; }

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

        public void SendModuleInfo(int id, DomainModelInfo domainInfo)
        {
            AllDomainInject = true;
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

            while (!Current.AllDomainInject)
                _evt.WaitOne(1000);
        }


        public static LoggerProxy Current;

        public void WriteInjectError(int domainID, string message, string fullError)
        {
            OnInjectProcessError.Invoke(message, fullError);
            _evt.Reset();
        }

        public void WriteDomainError(int domainID, string message, string fullError)
        {
            OnInjectDomainError.Invoke(domainID, message, fullError);
        }

        internal void InjectAllDomain(int[] domainIDs)
        {
            AllDomainInject = true;
            _evt.Reset();
        }

        internal void AllDomain(int[] domainIDs)
        {

        }

        public void Dispose()
        {

        }
    }

    [Serializable]
    [DataContract]
    public class ThreadInfo
    {
        [DataMember]
        public TraceFrameInfo[] Frames { get; set; }

        [DataMember]
        public int ThreadID { get; set; }


        [DataMember]
        public ThreadState ThreadState { get; set; }
    }
}
