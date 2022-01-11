using System;

namespace NetHook.Cores.Inject
{
    [Serializable]
    public class MethodModelInfo
    {
        public string Name { get; internal set; }

        public string Signature { get; internal set; }
        public string TypeName { get; internal set; }
    }
}