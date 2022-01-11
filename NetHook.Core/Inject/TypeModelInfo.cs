using System;

namespace NetHook.Cores.Inject
{
    [Serializable]
    public class TypeModelInfo
    {
        public string Namespace { get; internal set; }

        public MethodModelInfo[] Methods { get; internal set; }

        public string Name { get; internal set; }
        public string FullName { get; internal set; }
    }
}