using NetHook.Core;
using NetHook.Cores.Extensions;
using NetHook.Cores.Handlers;
using NetHook.Cores.Handlers.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;

namespace NetHook.Cores.Inject
{
    public class DomainEntryPoint : MarshalByRefObject, IDomainEntryPoint
    {

        public void InjectDomain(string inChannelName)
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    Console.WriteLine(inChannelName);
                    IpcClientChannel channel = new IpcClientChannel();
                    ChannelServices.RegisterChannel(channel, true);
                    RemotingConfiguration.RegisterWellKnownClientType(
                                typeof(LoggerInterface), inChannelName);

                    LoggerInterface client = new LoggerInterface();

                    DomainModelInfo domainInfo = GetDomainInfo();
                    client.SendModuleInfo(domainInfo);

                    using (LocalHookAdapter adapter = LocalHookAdapter.CreateInstance())
                    {
                        Console.WriteLine("AppDomain " + AppDomain.CurrentDomain.Id);

                        Assembly assembly = Assembly.LoadFrom(Process.GetCurrentProcess().MainModule.FileName);
                        Console.WriteLine(assembly);

                        var methods = assembly.GetTypes().SelectMany(x => x.GetMethods().Where(y => y.DeclaringType == x))
                            .Where(x => LocalHookAdapter.CheckMethod(x, false))
                            .ToArray();

                        adapter.AddHook(methods);

                        TraceHandler handler = new TraceHandler(new TraceHandlerSetting());
                        adapter.RegisterHandler(handler);

                        adapter.Install();
                        object objLock = new object();

                        while (true)
                        {
                            Thread.Sleep(500);

                            TraceFrameInfo[] package = null;

                            lock (objLock)
                            {
                                package = handler
                                    .GetStackTrace()
                                    .Select(x => ConvertFrame(x.Key, x.Value))
                                    .ToArray();
                            }

                            client.OnCreateFile(package);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private DomainModelInfo GetDomainInfo()
        {
            DomainModelInfo result = new DomainModelInfo()
            {
                Assemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(x => !x.FullName.StartsWith("NetHook.Core")
                && !x.FullName.StartsWith("EasyHook")
                && !x.FullName.StartsWith("EasyLoad"))
                .Select(x => GetAssembleInfo(x))
                .ToArray(),
            };

            return result;
        }


        private AssembleModelInfo GetAssembleInfo(Assembly assembly)
        {
            AssembleModelInfo result = new AssembleModelInfo()
            {
                FullName = assembly.FullName,
                Name = assembly.GetName().Name,
                Types = assembly.GetTypes().Select(x => GetTypeInfo(x)).Where(x => x.Methods.Length > 0).ToArray(),
            };

            return result;
        }

        private TypeModelInfo GetTypeInfo(Type type)
        {
            TypeModelInfo result = new TypeModelInfo()
            {
                Name = type.Name,
                Namespace = type.Namespace,
                Methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.DeclaringType == type)
                .Select(x => GetMethodInfo(x))
                .ToArray()
            };

            return result;
        }

        private TypeMethodlInfo GetMethodInfo(MethodInfo methodInfo)
        {
            TypeMethodlInfo result = new TypeMethodlInfo()
            {
                Name = methodInfo.Name,
                Signature = $"{methodInfo.ReturnType.Name} {methodInfo.Name}({methodInfo.GetParameters().Select(x => $"{x.ParameterType.Name} {x.Name}").JoinString()})"
            };

            return result;
        }


        private TraceFrameInfo ConvertFrame(Thread thread, TraceFrame frame, TraceFrameInfo parent = null)
        {
            TraceFrameInfo result = new TraceFrameInfo()
            {
                MethodName = frame.Method.Name,
                Assembler = frame.Method.DeclaringType.Assembly.FullName,
                TypeName = frame.Method.DeclaringType.FullName,
                Elapsed = frame.Elapsed,
                IsRunning = frame.IsRunning,
                Parent = parent,
                DateCreate = frame.DateCreate,
            };

            result.ChildFrames = frame.ChildFrames.Select(x => ConvertFrame(thread, x, result)).ToArray();

            return result;
        }
    }
}
