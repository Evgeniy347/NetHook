//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LocalHookProviders
{
    using System;
    using NetHook.Core;
    using System.Reflection;
    using System.Runtime.ConstrainedExecution;
    
    
    public sealed class LocalHookProvider_Method2_0 : NetHook.Core.LocalHookRuntimeInstance
    {
        
        public LocalHookProvider_Method2_0(NetHook.Core.LocalHookAdapter adapter) : 
                base(LocalHookAdapter.Current.Get(55530882),  typeof(LocalHookProvider_Method2_0).GetMethod("Method2_Hook", BindingFlags.Static | BindingFlags.NonPublic))
        {
        }
        
        [System.Runtime.ConstrainedExecution.PrePrepareMethodAttribute()]
        private static object Method2_Hook(object arg_0)
        {
            object[] objectArray = new object[] { arg_0 };
            object value = IntPtr.Zero;
            Exception e = null;
            object thisObj = null;
            LocalHookAdapter.Current.BeginInvoke(55530882, thisObj, objectArray);
            try
            {
                if (objectArray.Length > 0) value = Method2_Hook(arg_0);
                else value = Method2_Hook(arg_0);
            }
            catch (System.Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                LocalHookAdapter.Current.AfterInvoke(55530882, thisObj, value, e);
            }
            return value;
        }
    }
}