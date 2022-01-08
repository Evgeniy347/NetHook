using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

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
        }

        public MethodInfo Method { get; }

        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public TraceFrame Parent { get; }

        internal TraceFrame CreateChild(MethodInfo method)
        {
            return new TraceFrame(method, this);
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
