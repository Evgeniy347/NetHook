using System;
using System.Runtime.Serialization;

namespace NetHook.Cores.Inject
{
    [Serializable]
    [DataContract]
    public class TypeModelInfo
    {
        [DataMember]
        public string Namespace { get; internal set; }

        [DataMember]
        public MethodModelInfo[] Methods { get; internal set; }

        [DataMember]
        public string Name { get; internal set; }

        [DataMember]
        public string FullName { get; internal set; }
    }
}