using System;

namespace NetHook.Cores.Inject
{
    [Serializable]
    public class TypeModelInfo
    {
        public string Namespace { get; internal set; }

        public TypeMethodlInfo[] Methods { get; internal set; }
        public string Name { get; internal set; }
    }
}