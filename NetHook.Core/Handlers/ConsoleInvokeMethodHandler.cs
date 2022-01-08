using System;
using System.Reflection;

namespace NetHook.Core
{
    public class ConsoleInvokeMethodHandler : IHandlerHook
    {
        public void AfterInvoke(MethodInfo method, object instance, object @return, Exception ex)
        {
            Console.WriteLine("AfterInvoke " + method.Name);
        }

        public void BeforeInvoke(MethodInfo method, object instance, object[] arguments)
        {
            Console.WriteLine("BeginInvoke " + method.Name + " arg " + arguments[0].ToString());
        }
    }
}