using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace NetHook.Cores.Handlers.Trace
{
    [Serializable]
    [DataContract]
    public class TraceFrameInfo
    {
        [DataMember]
        public DateTime DateCreate { get; set; }

        [DataMember]
        public string MethodName { get; set; }

        [DataMember]
        public string Signature { get; set; }

        [DataMember]
        public string TypeName { get; set; }

        [DataMember]
        public string Assembler { get; set; }

        [DataMember]
        public TimeSpan Elapsed { get; set; }

        [DataMember]
        public bool IsRunning { get; set; }

        [DataMember]
        public TraceFrameInfo[] ChildFrames { get; set; }

        [DataMember]
        public string URL { get; internal set; }
    }
}
