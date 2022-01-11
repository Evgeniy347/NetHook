using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetHook.Cores.Handlers.Trace
{
    [Serializable]
    public class TraceFrameInfo
    {
        public DateTime DateCreate { get; set; }

        public string MethodName { get; set; }

        public string Signature { get; set; }

        public string TypeName { get; set; }

        public string Assembler { get; set; }

        public TimeSpan Elapsed { get; set; }

        public bool IsRunning { get; set; }

        public TraceFrameInfo Parent { get; set; }

        public TraceFrameInfo[] ChildFrames { get; set; }
    }
}
