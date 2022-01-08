using System;
using System.Reflection;

namespace NetHook.Core
{
    public interface IHandlerHook
    {
        void BeforeInvoke(MethodInfo method, object instance, object[] arguments);

        void AfterInvoke(MethodInfo method, object instance, object @return, Exception ex);
    }
}