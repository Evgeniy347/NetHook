using System;
using System.Runtime.Serialization;

namespace NetHook.Cores.Inject
{
    [Serializable]
    [DataContract]
    public class MethodModelInfo
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Signature { get; set; }

        [DataMember]
        public string TypeName { get; set; }
    }
}