using System;
using System.Runtime.Serialization;

namespace NetHook.Cores.Inject
{
    [Serializable]
    [DataContract]
    public class TypeModelInfo
    {
        [DataMember]
        public string Namespace { get; set; }

        [DataMember]
        public MethodModelInfo[] Methods { get; set; } = new MethodModelInfo[0];

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public string ErrorText { get; set; }
    }
}