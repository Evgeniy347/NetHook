using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetHook.Core
{
    public abstract class LocalHookRuntimeInstance
    {
        public LocalHookRuntimeInstance(MethodInfo method, MethodInfo methodHook)
        {
            Method = method;
            MethodHook = methodHook;
        }

        public MethodInfo Method { get; }

        public MethodInfo MethodHook { get; }
    }
}
