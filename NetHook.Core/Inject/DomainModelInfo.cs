using System;

namespace NetHook.Cores.Inject
{
    [Serializable]
    public class DomainModelInfo
    {
        public AssembleModelInfo[] Assemblies { get; internal set; }
    }
}