using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace NetHook.Cores.Handlers.Trace
{
    public class TraceFrame
    {
        private readonly Stopwatch _stopwatch;

        internal TraceFrame(MethodInfo method, TraceFrame parent = null)
        {
            Method = method;
            _stopwatch = new Stopwatch();

            Parent = parent;
            DateCreate = DateTime.Now;
        }

        public DateTime DateCreate { get; }

        public MethodInfo Method { get; }

        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public bool IsRunning => _stopwatch.IsRunning;
        public int ThreadID { get; set; }

        public TraceFrame Parent { get; }

        public ConcurrentBag<TraceFrame> ChildFrames { get; } = new ConcurrentBag<TraceFrame>();

        internal TraceFrame CreateChild(MethodInfo method)
        {
            TraceFrame traceFrame = new TraceFrame(method, this);
            ChildFrames.Add(traceFrame);
            return traceFrame;
        }

        internal void Start()
        {
            _stopwatch.Start();
        }

        internal void Stop()
        {
            _stopwatch.Stop();
        }
    }
}