using NetHook.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Web;

namespace NetHook.Cores.Handlers.Trace
{
    public class TraceRoot
    {
        public TraceRoot(Thread thread)
        {
            Thread = thread;
        }

        public List<TraceFrame> Frames { get; } = new List<TraceFrame>();

        public Thread Thread { get; }

        public TraceFrame Current { get; internal set; }
    }

    public class TraceHandler : IHandlerHook
    {
        private readonly ConcurrentDictionary<int, TraceRoot> _threadframes = new ConcurrentDictionary<int, TraceRoot>();

        public void BeforeInvoke(MethodInfo method, object instance, object[] arguments)
        {
            Thread thread = Thread.CurrentThread;

            if (!_threadframes.TryGetValue(thread.ManagedThreadId, out TraceRoot traceRoot))
                _threadframes[thread.ManagedThreadId] = traceRoot = new TraceRoot(thread);

            TraceFrame traceFrame = traceRoot.Current;

            if (traceFrame == null)
            {
                traceFrame = new TraceFrame(method);
                if (HttpContext.Current != null)
                    traceFrame.URL = HttpContext.Current.Request.RawUrl;

                traceRoot.Frames.Add(traceFrame);
            }
            else
                traceFrame = traceFrame.CreateChild(method);

            traceFrame.Start();
            traceFrame.ThreadID = thread.ManagedThreadId;

            traceRoot.Current = traceFrame;
        }

        public void AfterInvoke(MethodInfo method, object instance, object @return, Exception ex)
        {
            Thread thread = Thread.CurrentThread;

            if (!_threadframes.TryGetValue(thread.ManagedThreadId, out TraceRoot traceRoot))
                throw new Exception();

            TraceFrame traceFrame = traceRoot.Current ??
                throw new NullReferenceException("Current Is Null");

            if (traceFrame.ThreadID != thread.ManagedThreadId)
                throw new Exception($"{traceFrame.ThreadID} {thread.ManagedThreadId}");

            traceFrame.Stop();
            traceRoot.Current = traceFrame.Parent;

        }

        public Dictionary<Thread, List<TraceFrame>> GetStackTrace()
        {
            return _threadframes.ToDictionary(x => x.Value.Thread, x => x.Value.Frames);
        }
    }
}
