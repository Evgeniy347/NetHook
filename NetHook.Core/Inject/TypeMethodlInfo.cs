using System;

namespace NetHook.Cores.Inject
{
    [Serializable]
    public class TypeMethodlInfo
    {
        public string Name { get; internal set; }

        public string Signature { get; internal set; }
    }
}