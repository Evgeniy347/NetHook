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
        [ThreadStatic]
        private TraceFrame _currentFrames;

        private readonly ConcurrentDictionary<Thread, TraceFrame> _threadframes = new ConcurrentDictionary<Thread, TraceFrame>();

        private readonly TraceHandlerSetting _setting;
        public TraceHandler(TraceHandlerSetting setting)
        {
            _setting = setting ??
                throw new ArgumentNullException(nameof(setting));
        }

        public void BeforeInvoke(MethodInfo method, object instance, object[] arguments)
        {
            if (_currentFrames == null)
                _threadframes[Thread.CurrentThread] = _currentFrames = new TraceFrame(method);

            _currentFrames = _currentFrames.CreateChild(method);
            _currentFrames.Start();
        }

        public void AfterInvoke(MethodInfo method, object instance, object @return, Exception ex)
        {
            _currentFrames.Stop();
            _currentFrames = _currentFrames.Parent;
        }

        public Dictionary<Thread, TraceFrame> GetStackTrace()
        {
            return _threadframes.ToDictionary(x => x.Key, x => x.Value);
        }

    }
}
