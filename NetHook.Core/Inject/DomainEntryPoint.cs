using NetHook.Core;
using NetHook.Core.Helpers;
using NetHook.Cores.Extensions;
using NetHook.Cores.Handlers;
using NetHook.Cores.Handlers.Trace;
using NetHook.Cores.Inject.AssemblyModel;
using NetHook.Cores.Socket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;

namespace NetHook.Cores.Inject
{
    public class DomainEntryPoint : MarshalByRefObject, IDomainEntryPoint
    {

        public void InjectDomain(string address)
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    AppDomain currentDomain = AppDomain.CurrentDomain;
                    currentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

                    Console.WriteLine(address);

                    using (LoggerClient client = new LoggerClient())
                    {
                        client.OpenChanel(address);
                        try
                        {
                            object objLock = new object();

                            using (LocalHookAdapter adapter = LocalHookAdapter.CreateInstance())
                            {
                                TraceHandler handler = new TraceHandler();
                                adapter.RegisterHandler(handler);

                                while (client.Socket.IsSocketConnected())
                                {
                                    Console.WriteLine("WaitServer");

                                    MessageSocket message = client.AcceptMessage();

                                    Console.WriteLine("operation:" + message.MethodName);

                                    if (message.MethodName == "SendModuleInfo")
                                    {
                                        DomainModelInfo domainInfo = GetDomainInfo();
                                        client.SendModuleInfo(domainInfo);
                                    }
                                    else if (message.MethodName == "SetHook")
                                    {
                                        MethodModelInfo[] methods = message.Body.DeserializeJSON<MethodModelInfo[]>();
                                        AddHookMethods(adapter, methods);
                                        client.Send($"SetHook Domain '{AppDomain.CurrentDomain.Id}' OK");
                                    }
                                    else if (message.MethodName == "UploadThreadInfo")
                                    {
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
                                    else
                                    {
                                        Console.WriteLine(message.RawData);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            client.WriteInjectError(ex.ToString());
                        }
                        finally
                        {
                            Console.WriteLine("StopInjectThread");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });

            thread.Name = "InjectThread";
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

        public static IDisposable SetContextClient(string inChannelName)
        {
            IpcClientChannel channel = new IpcClientChannel(inChannelName, null);
            ChannelServices.RegisterChannel(channel, true);

            if (RemotingConfiguration.IsWellKnownClientType(typeof(LoggerProxy)) == null)
                RemotingConfiguration.RegisterWellKnownClientType(typeof(LoggerProxy), inChannelName);

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
                    RemotingServices.Disconnect(new LoggerProxy());
                }
                catch { }
            });
        }

        public static DomainModelInfo GetDomainInfo()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            DomainModelInfo result = new DomainModelInfo()
            {
                Assemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                //.Where(x => !x.FullName.StartsWith("NetHook.Core")
                //&& !x.FullName.StartsWith("EasyHook")
                //&& !x.FullName.StartsWith("EasyLoad")
                //&& !x.FullName.StartsWith("System")
                //&& !x.FullName.StartsWith("Microsoft")
                //&& !x.FullName.StartsWith("Aspose")
                //&& !x.FullName.StartsWith("mscorlib"))
                .Select(x => GetAssembleInfo(x))
                .Where(x => string.IsNullOrEmpty(x.ErrorText))
                .ToArray(),
            };

            stopwatch.Stop();
            result.Elapsed = stopwatch.Elapsed;

            return result;
        }

        private static AssembleModelInfo GetAssembleInfo(Assembly assembly)
        {
            AssembleModelInfo result = null;
            try
            {
                result = new AssembleModelInfo()
                {
                    FullName = assembly.FullName,
                    Name = assembly.GetName().Name,
                };

                result.Types = assembly.GetTypes()
                    .Where(Condition)
                    .Select(x => GetTypeInfo(x))
                    .Where(x => !string.IsNullOrEmpty(x.ErrorText) || x.Methods.Length > 0)
                    .ToArray();

                return result;
            }
            catch (Exception ex)
            {
                if (result != null)
                    result.ErrorText = ex.ToString();
            }

            return result;
        }



        private static bool Condition(Type type)
        {
            if (!type.IsClass)
                return false;

            Attribute attr = Attribute.GetCustomAttribute(type, typeof(CompilerGeneratedAttribute));
            if (attr != null)
                return false;

            return true;
        }

        private static TypeModelInfo GetTypeInfo(Type type)
        {
            TypeModelInfo result = null;
            try
            {
                result = new TypeModelInfo()
                {
                    Name = type.Name,
                    FullName = type.AssemblyQualifiedName,
                    Namespace = type.Namespace
                };

                result.Methods = LocalHookAdapter.GetMethods(type)
                   .Select(x => GetMethodInfo(x))
                   .ToArray();

                return result;
            }
            catch (Exception ex)
            {
                if (result != null)
                    result.ErrorText = ex.ToString();
            }

            return result;
        }

        private static MethodModelInfo GetMethodInfo(MethodInfo methodInfo)
        {
            MethodModelInfo result = new MethodModelInfo()
            {
                Name = methodInfo.Name,
                Signature = GetSignature(methodInfo)
            };

            return result;
        }

        private static string GetSignature(MethodInfo method)
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

        private TraceFrameInfo ConvertFrame(TraceFrame frame)
        {
            TraceFrameInfo result = new TraceFrameInfo()
            {
                MethodName = frame.Method.Name,
                Signature = GetSignature(frame.Method),
                TypeName = frame.Method.DeclaringType.AssemblyQualifiedName,
                Elapsed = frame.Elapsed,
                IsRunning = frame.IsRunning,
                DateCreate = frame.DateCreate,
            };

            result.ChildFrames = frame.ChildFrames.Select(x => ConvertFrame(x)).ToArray();

            return result;
        }
    }
}
