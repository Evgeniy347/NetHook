using System;
using System.Runtime.Serialization;

namespace NetHook.Cores.Inject
{
    [Serializable]
    [DataContract]
    public class DomainModelInfo
    {
        [DataMember]
        public int CurrentID { get; set; }

        [DataMember]
        public AssembleModelInfo[] Assemblies { get; set; } = new AssembleModelInfo[0];

        [DataMember]
        public TimeSpan Elapsed { get; set; }
    }
}