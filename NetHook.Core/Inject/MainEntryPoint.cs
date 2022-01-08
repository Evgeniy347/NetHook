using EasyHook;
using NetHook.Core;
using NetHook.Cores.Handlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading;

namespace NetHook.Cores.Inject
{
    [CompilerGenerated]
    [Guid("CB2F6722-AB3A-11D2-9C40-00C04FA30A3E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [TypeIdentifier]
    [ComImport]
    [CLSCompliant(false)]
    public interface ICorRuntimeHost
    {
        void _VtblGap1_11();

        void EnumDomains(out IntPtr enumHandle);

        void NextDomain([In] IntPtr enumHandle, [MarshalAs(UnmanagedType.IUnknown)] out object appDomain);

        void CloseEnum([In] IntPtr enumHandle);
    }

    public interface IFoo
    {
        void SomeMethod(string inChannelName);
    }

    public class Foo : MarshalByRefObject, IFoo
    {
        public Foo()
        { }
        LoggerInterface Interface;
        object InterfaceRaw;
        Stack<String> Queue = new Stack<String>();


        public void SomeMethod(string inChannelName)
        {
            Interface = RemoteHooking.IpcConnectClient<LoggerInterface>(inChannelName);
            InterfaceRaw = (object)Interface;
            try
            {

                using (LocalHookAdapter adapter = LocalHookAdapter.CreateInstance())
                {
                    Console.WriteLine("AppDomain " + AppDomain.CurrentDomain.Id);

                    Assembly assembly = Assembly.LoadFrom(Process.GetCurrentProcess().MainModule.FileName);

                    Console.WriteLine(assembly);

                    var methods = assembly.GetTypes().SelectMany(x => x.GetMethods().Where(y => y.DeclaringType == x))
                        .Where(x => LocalHookAdapter.CheckMethod(x, false))
                        .ToArray();

                    methods.ToList().ForEach(x => Console.WriteLine(x));
                    adapter.AddHook(methods);

                    ActionHandler handler = new ActionHandler();

                    handler.BeforeEvent += (x, y, z) =>
                    {
                        Console.WriteLine("BeforeEvent");
                        Queue.Push(x.Name);
                    };

                    handler.AfterEvent += (x, y, z, e) =>
                    {
                        Console.WriteLine("AfterEvent");
                        Queue.Push(x.Name);
                    };

                    adapter.RegisterHandler(handler);

                    adapter.Install();

                    while (true)
                    {
                        Thread.Sleep(500);

                        if (Queue.Count > 0)
                        {
                            String[] Package = null;

                            lock (Queue)
                            {
                                Package = Queue.ToArray();

                                Queue.Clear();
                            }


                            Console.WriteLine(InterfaceRaw.GetHashCode());
                            var t = Interface.CreateObjRef();

                            Console.WriteLine(t);

                            Console.WriteLine("ok");
                            //Interface.OnCreateFile(1, Package);
                            Console.WriteLine("ok");


                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    public class MainEntryPoint : IEntryPoint
    {
        LoggerInterface Interface;
        Stack<String> Queue = new Stack<String>();

        public MainEntryPoint(RemoteHooking.IContext InContext, String InChannelName)
        {
            Interface = RemoteHooking.IpcConnectClient<LoggerInterface>(InChannelName);
        }
        private static ICorRuntimeHost GetCorRuntimeHost()
        {
            return (ICorRuntimeHost)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("CB2F6723-AB3A-11D2-9C40-00C04FA30A3E")));
        }

        public static IEnumerable<AppDomain> EnumAppDomains()
        {
            IntPtr enumHandle = IntPtr.Zero;
            ICorRuntimeHost host = null;

            try
            {
                host = GetCorRuntimeHost();
                host.EnumDomains(out enumHandle);
                object domain = null;

                host.NextDomain(enumHandle, out domain);
                while (domain != null)
                {
                    yield return (AppDomain)domain;
                    host.NextDomain(enumHandle, out domain);
                }
            }
            finally
            {
                if (host != null)
                {
                    if (enumHandle != IntPtr.Zero)
                    {
                        host.CloseEnum(enumHandle);
                    }

                    Marshal.ReleaseComObject(host);
                }
            }
        }

        public void Run(RemoteHooking.IContext InContext, String InChannelName)
        {
            try
            {
                foreach (var domain in EnumAppDomains())
                {
                    Console.WriteLine(domain.Id);
                    if (domain.Id == AppDomain.CurrentDomain.Id)
                        continue;

                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        try
                        {
                            domain.Load(File.ReadAllBytes(assembly.Location));
                            Console.WriteLine(assembly);
                        }
                        catch { }
                    }

                    //var obj = domain.CreateInstanceAndUnwrap(typeof(Foo).Assembly.FullName, typeof(Foo).FullName);
                    var obj = domain.CreateInstanceFromAndUnwrap(Path.GetFileName(typeof(Foo).Assembly.Location), typeof(Foo).FullName);
                    var foo = (IFoo)obj;

                    foo.SomeMethod(InChannelName);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                while (true)
                    Thread.Sleep(1000);
            }
        }

    }
}
