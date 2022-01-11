using NetHook.Core;
using NetHook.Core.Helpers;
using NetHook.Cores.Extensions;
using NetHook.Cores.Handlers;
using NetHook.Cores.Handlers.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
                    AppDomain currentDomain = AppDomain.CurrentDomain;
                    currentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

                    Console.WriteLine(inChannelName);
                    DomainModelInfo domainInfo = GetDomainInfo();

                    using (SetContextClient(inChannelName))
                    {
                        try
                        {
                            LoggerInterface client = new LoggerInterface();

                            client.SendModuleInfo(domainInfo);
                            object objLock = new object();

                            using (LocalHookAdapter adapter = LocalHookAdapter.CreateInstance())
                            {
                                TraceHandler handler = new TraceHandler(new TraceHandlerSetting());
                                adapter.RegisterHandler(handler);

                                DateTime lastUpdate = DateTime.MinValue;

                                while (true)
                                {
                                    if (lastUpdate != client.ChangeDateHook)
                                    {
                                        AddHookMethods(adapter, client.GetHook());
                                        lastUpdate = client.ChangeDateHook;
                                    }

                                    Thread.Sleep(500);

                                    ThreadInfo[] package = null;

                                    lock (objLock)
                                    {
                                        package = handler
                                            .GetStackTrace()
                                            .Select(x => ConvertFrames(x.Key, x.Value))
                                            .ToArray();
                                    }

                                    client.UploadThreadInfo(package);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
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

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < asms.Length; ++i)
            {
                if (asms[i].FullName == args.Name)
                    return asms[i];
            }
            return null;
        }

        private void AddHookMethods(LocalHookAdapter adapter, MethodModelInfo[] methodsModel)
        {
            Console.WriteLine("Добавление хуков ");
            adapter.UnInstall();

            foreach (var groupMethod in methodsModel.GroupBy(x => x.TypeName))
            {
                Type type = Type.GetType(groupMethod.Key);

                if (type == null)
                {
                    Console.WriteLine($"Не найден тип '{groupMethod.Key}'  {new StackTrace()}");
                    continue;
                }

                MethodInfo[] methods = LocalHookAdapter.GetMethods(type);

                foreach (MethodModelInfo methodModelInfo in groupMethod)
                {
                    MethodInfo method = methods.FirstOrDefault(x => x.Name == methodModelInfo.Name && GetSignature(x) == methodModelInfo.Signature);
                    if (method == null)
                    {
                        Console.WriteLine($"Не найден метод '{methodModelInfo.Signature}' в типе '{type.AssemblyQualifiedName}' {new StackTrace()}");
                        continue;
                    }

                    Console.WriteLine($"Добавление хука для метода '{methodModelInfo.Signature}' в типе '{type.AssemblyQualifiedName}'");
                    adapter.AddHook(method);
                }
            }

            adapter.Install();
        }

        private IDisposable SetContextClient(string inChannelName)
        {
            IpcClientChannel channel = new IpcClientChannel(inChannelName, null);
            ChannelServices.RegisterChannel(channel, true);

            if (RemotingConfiguration.IsWellKnownClientType(typeof(LoggerInterface)) == null)
                RemotingConfiguration.RegisterWellKnownClientType(typeof(LoggerInterface), inChannelName);

            return new DisposeAction<IpcClientChannel>(channel, (x) =>
            {
                Console.WriteLine("DisposeAction Channel");
                try
                {
                    ChannelServices.UnregisterChannel(x);
                }
                catch { }
                try
                {
                    RemotingServices.Disconnect(new LoggerInterface());
                }
                catch { }
            });
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
            };

            result.Types = assembly.GetTypes()
                .Select(x => GetTypeInfo(x))
                .Where(x => x.Methods.Length > 0)
                .ToArray();

            return result;
        }

        private TypeModelInfo GetTypeInfo(Type type)
        {
            TypeModelInfo result = new TypeModelInfo()
            {
                Name = type.Name,
                FullName = type.FullName,
                Namespace = type.Namespace
            };

            result.Methods = LocalHookAdapter.GetMethods(type)
               .Select(x => GetMethodInfo(x))
               .ToArray();

            return result;
        }

        private MethodModelInfo GetMethodInfo(MethodInfo methodInfo)
        {
            MethodModelInfo result = new MethodModelInfo()
            {
                Name = methodInfo.Name,
                TypeName = methodInfo.DeclaringType.AssemblyQualifiedName,
                Signature = GetSignature(methodInfo)
            };

            return result;
        }

        private string GetSignature(MethodInfo method)
        {
            string result = $"{method.ReturnType.Name} {method.Name}({method.GetParameters().Select(x => $"{x.ParameterType.Name} {x.Name}").JoinString()})";
            return result;
        }


        private ThreadInfo ConvertFrames(Thread thread, List<TraceFrame> frames)
        {
            ThreadInfo resilt = new ThreadInfo()
            {
                ThreadID = thread.ManagedThreadId,
                ThreadState = thread.ThreadState,
                Frames = frames.Select(x => ConvertFrame(x)).ToArray(),
            };

            return resilt;
        }

        private TraceFrameInfo ConvertFrame(TraceFrame frame, TraceFrameInfo parent = null)
        {
            TraceFrameInfo result = new TraceFrameInfo()
            {
                MethodName = frame.Method.Name,
                Signature = GetSignature(frame.Method),
                Assembler = frame.Method.DeclaringType.Assembly.FullName,
                TypeName = frame.Method.DeclaringType.FullName,
                Elapsed = frame.Elapsed,
                IsRunning = frame.IsRunning,
                Parent = parent,
                DateCreate = frame.DateCreate,
            };

            result.ChildFrames = frame.ChildFrames.Select(x => ConvertFrame(x, result)).ToArray();

            return result;
        }
    }
}
