using NetHook.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetHook.Cores.Handlers
{
    public class ActionHandler : IHandlerHook
    {
        public Action<MethodInfo, object, object, Exception> AfterEvent { get; set; }
        public Action<MethodInfo, object, object[]> BeforeEvent { get; set; }

        public void AfterInvoke(MethodInfo method, object instance, object @return, Exception ex)
        {
            AfterEvent?.Invoke(method, instance, @return, ex);
        }

        public void BeforeInvoke(MethodInfo method, object instance, object[] arguments)
        {
            BeforeEvent?.Invoke(method, instance, arguments);
        }
    }
}
