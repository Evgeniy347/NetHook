using NetHook.Cores.Handlers.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace NetHook.Cores.Inject.AssemblyModel
{

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

        [DataMember]
        public string URL { get; internal set; }
    }
}
