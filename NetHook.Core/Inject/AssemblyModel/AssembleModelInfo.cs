using System;
using System.Runtime.Serialization;

namespace NetHook.Cores.Inject
{
    [Serializable]
    [DataContract]
    public class AssembleModelInfo
    {
        [DataMember]
        public TypeModelInfo[] Types { get; internal set; } = new TypeModelInfo[0];

        [DataMember]
        public string FullName { get; internal set; }

        [DataMember]
        public string Name { get; internal set; }

        [DataMember]
        public string ErrorText { get; internal set; }
    }
}