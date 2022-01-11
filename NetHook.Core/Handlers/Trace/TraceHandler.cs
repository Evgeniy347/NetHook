using NetHook.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace NetHook.Cores.Handlers.Trace
{
    public class TraceHandler : IHandlerHook
    {
        private class ThreadComparer : IEqualityComparer<Thread>
        {

            public bool Equals(Thread x, Thread y)
            {
                return x.ManagedThreadId == y.ManagedThreadId;
            }

            public int GetHashCode(Thread obj) => obj.ManagedThreadId.GetHashCode();
        }

        private readonly ConcurrentDictionary<Thread, TraceRoot> _threadframes = new ConcurrentDictionary<Thread, TraceRoot>(new ThreadComparer());


        private readonly TraceHandlerSetting _setting;
        public TraceHandler(TraceHandlerSetting setting)
        {
            _setting = setting ??
                throw new ArgumentNullException(nameof(setting));
        }

        public void BeforeInvoke(MethodInfo method, object instance, object[] arguments)
        {
            if (!_threadframes.TryGetValue(Thread.CurrentThread, out TraceRoot traceRoot))
                _threadframes[Thread.CurrentThread] = traceRoot = new TraceRoot();

            TraceFrame traceFrame = traceRoot.Current;

            if (traceFrame == null)
            {
                traceFrame = new TraceFrame(method);
                traceRoot.Frames.Add(traceFrame);
            }
            else
                traceFrame = traceFrame.CreateChild(method);

            traceFrame.Start();
            traceFrame.ThreadID = Thread.CurrentThread.ManagedThreadId;

            traceRoot.Current = traceFrame;
        }

        public void AfterInvoke(MethodInfo method, object instance, object @return, Exception ex)
        {
            if (!_threadframes.TryGetValue(Thread.CurrentThread, out TraceRoot traceRoot))
                throw new Exception();

            TraceFrame traceFrame = traceRoot.Current ??
                throw new NullReferenceException("Current Is Null");

            if (traceFrame.ThreadID != Thread.CurrentThread.ManagedThreadId)
                throw new Exception($"{traceFrame.ThreadID} {Thread.CurrentThread.ManagedThreadId}");

            traceFrame.Stop();
            traceRoot.Current = traceFrame.Parent;

        }

        private class TraceRoot
        {
            public List<TraceFrame> Frames { get; } = new List<TraceFrame>();
            public TraceFrame Current { get; internal set; }
        }

        public Dictionary<Thread, List<TraceFrame>> GetStackTrace()
        {
            return _threadframes.ToDictionary(x => x.Key, x => x.Value.Frames);
        }

        //private readonly TraceHandlerSetting _setting;
        //public TraceHandler(TraceHandlerSetting setting)
        //{
        //    _setting = setting ??
        //        throw new ArgumentNullException(nameof(setting));
        //}

        //public void BeforeInvoke(MethodInfo method, object instance, object[] arguments)
        //{
        //    if (!_threadframes.TryGetValue(Thread.CurrentThread, out TraceFrame value))
        //    {
        //        _threadframes[Thread.CurrentThread] = value = new TraceFrame(method);
        //        value.Start();
        //    }
        //    else
        //        value.CreateChild(method).Start();

        //}

        //public void AfterInvoke(MethodInfo method, object instance, object @return, Exception ex)
        //{
        //    TraceFrame value = _threadframes[Thread.CurrentThread];

        //    value.Stop();
        //    _threadframes[Thread.CurrentThread] = value.Parent;
        //}

        //public Dictionary<Thread, TraceFrame> GetStackTrace()
        //{
        //    return _threadframes.Where(x => x.Key != null && x.Value != null).ToDictionary(x => x.Key, x => x.Value);
        //}

    }
}
