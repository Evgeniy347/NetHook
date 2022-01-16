using System;
using System.Runtime.Serialization;

namespace NetHook.Cores.Inject
{
    [Serializable]
    [DataContract]
    public class MethodModelInfo
    {
        [DataMember]
        public string Name { get; internal set; }
        [DataMember]
        public string Signature { get; internal set; }
        [DataMember]
        public string TypeName { get; internal set; }
    }
}