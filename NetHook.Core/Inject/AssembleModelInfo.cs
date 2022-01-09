using System;

namespace NetHook.Cores.Inject
{
    [Serializable]
    public class AssembleModelInfo
    {
        public TypeModelInfo[] Types { get; internal set; }
        public string FullName { get; internal set; }
        public string Name { get; internal set; }
    }
}