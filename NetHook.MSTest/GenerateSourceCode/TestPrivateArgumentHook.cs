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
    
    
    public sealed class LocalHookProvider_TestInstanceMethod_0 : NetHook.Core.LocalHookRuntimeInstance
    {
        
        public LocalHookProvider_TestInstanceMethod_0(NetHook.Core.LocalHookAdapter adapter) : 
                base(LocalHookAdapter.Current.Get("NetHook.MSTest.UnitTestLocalHook+TestPrivateArgumentInstance, NetHook.MSTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;TestInstanceMethod"),  typeof(LocalHookProvider_TestInstanceMethod_0).GetMethod("TestInstanceMethod_Hook", BindingFlags.Static | BindingFlags.NonPublic))
        {
        }
        
        private static object TestInstanceMethod_Hook(object thisObj, object arg_0)
        {
            object[] objectArray = new object[] { arg_0 };
            object value = IntPtr.Zero;
            Exception e = null;
            LocalHookAdapter.Current.BeginInvoke("NetHook.MSTest.UnitTestLocalHook+TestPrivateArgumentInstance, NetHook.MSTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;TestInstanceMethod", thisObj, objectArray);
            try
            {
                if (objectArray.Length == 0) value = TestInstanceMethod_Hook(thisObj, arg_0);
                else if (objectArray.Length == 1) value = TestInstanceMethod_Hook(thisObj, arg_0);
                else if (objectArray.Length == 2) value = TestInstanceMethod_Hook(thisObj, arg_0);
                else if (objectArray.Length == 3) value = TestInstanceMethod_Hook(thisObj, arg_0);
                else value = TestInstanceMethod_Hook(thisObj, arg_0);
            }
            catch (System.Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                LocalHookAdapter.Current.AfterInvoke("NetHook.MSTest.UnitTestLocalHook+TestPrivateArgumentInstance, NetHook.MSTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;TestInstanceMethod", thisObj, value, e);
            }
            return value;
        }
    }
    
    public sealed class LocalHookProvider_TestStaticMethod_1 : NetHook.Core.LocalHookRuntimeInstance
    {
        
        public LocalHookProvider_TestStaticMethod_1(NetHook.Core.LocalHookAdapter adapter) : 
                base(LocalHookAdapter.Current.Get("NetHook.MSTest.UnitTestLocalHook+TestPrivateArgumentInstance, NetHook.MSTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;TestStaticMethod"),  typeof(LocalHookProvider_TestStaticMethod_1).GetMethod("TestStaticMethod_Hook", BindingFlags.Static | BindingFlags.NonPublic))
        {
        }
        
        private static object TestStaticMethod_Hook(object arg_0)
        {
            object[] objectArray = new object[] { arg_0 };
            object value = IntPtr.Zero;
            Exception e = null;
            object thisObj = null;
            LocalHookAdapter.Current.BeginInvoke("NetHook.MSTest.UnitTestLocalHook+TestPrivateArgumentInstance, NetHook.MSTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;TestStaticMethod", thisObj, objectArray);
            try
            {
                if (objectArray.Length == 0) value = TestStaticMethod_Hook(arg_0);
                else if (objectArray.Length == 1) value = TestStaticMethod_Hook(arg_0);
                else if (objectArray.Length == 2) value = TestStaticMethod_Hook(arg_0);
                else if (objectArray.Length == 3) value = TestStaticMethod_Hook(arg_0);
                else value = TestStaticMethod_Hook(arg_0);
            }
            catch (System.Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                LocalHookAdapter.Current.AfterInvoke("NetHook.MSTest.UnitTestLocalHook+TestPrivateArgumentInstance, NetHook.MSTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;TestStaticMethod", thisObj, value, e);
            }
            return value;
        }
    }
}
