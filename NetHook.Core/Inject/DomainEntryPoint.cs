using NetHook.Core;
using NetHook.Core.Helpers;
using NetHook.Cores.Extensions;
using NetHook.Cores.Handlers;
using NetHook.Cores.Handlers.Trace;
using NetHook.Cores.Inject.AssemblyModel;
using NetHook.Cores.NetSocket;
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
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NetHook.Cores.Inject
{
    public class DomainEntryPoint : MarshalByRefObject, IDomainEntryPoint
    {

        public void InjectDomain(string address)
        {
            SocketExtensions.DisableLog();

            Thread thread = new Thread(() =>
            {
                try
                {
                    AppDomain currentDomain = AppDomain.CurrentDomain;
                    currentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

                    Console.WriteLine(address);

                    using (DuplexSocketClient duplexSocket = new DuplexSocketClient())
                    {
                        string[] addressParts = address.Split(':');
                        duplexSocket.OpenChanel(addressParts[0], int.Parse(addressParts[1]));

                        try
                        {
                            object objLock = new object();

                            using (LocalHookAdapter adapter = LocalHookAdapter.CreateInstance())
                            {
                                TraceHandler handler = new TraceHandler();
                                adapter.RegisterHandler(handler);

                                duplexSocket.HandlerRequest.Add("GetAssemblyInfo", (x) => GetDomainInfo());
                                duplexSocket.HandlerRequest.Add("SetHook", (x) => SetHook(adapter, x));
                                duplexSocket.HandlerRequest.Add("UploadThreadInfo", (x) =>
                                {
                                    ThreadInfo[] package = null;

                                    lock (objLock)
                                    {
                                        package = handler
                                            .GetStackTrace()
                                            .Select(y => ConvertFrames(y.Key, y.Value))
                                            .ToArray();
                                    }

                                    return package;
                                });

                                while (duplexSocket.IsSocketConnected())
                                    Thread.Sleep(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            duplexSocket.SendMessage("WriteInjectError", ex.ToString());
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

            thread.Name = "DomainEntryPointInject";
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        StringBuilder Log;

        private string SetHook(LocalHookAdapter adapter, MessageSocket message)
        {
            Log = new StringBuilder();
            try
            {
                Log.AppendLine($"SetHook Domain '{AppDomain.CurrentDomain.Id}'");
                MethodModelInfo[] methods = message.GetObject<MethodModelInfo[]>();
                AddHookMethods(adapter, methods);

                Log.AppendLine("OK");
            }
            catch (Exception ex)
            {
                Log.AppendLine(ex.ToString());
            }

            return Log.ToString();
        }

        private void WriteLine(string message)
        {
            Console.WriteLine(message);
            Log?.AppendLine(message);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
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
            WriteLine("Добавление хуков ");
            adapter.UnInstall();

            foreach (var groupMethod in methodsModel.GroupBy(x => x.TypeName))
            {
                Type type = Type.GetType(groupMethod.Key);

                if (type == null)
                {
                    WriteLine($"Не найден тип '{groupMethod.Key}'  {new StackTrace()}");
                    continue;
                }

                MethodInfo[] methods = LocalHookAdapter.GetMethods(type);

                foreach (MethodModelInfo methodModelInfo in groupMethod)
                {
                    MethodInfo method = methods.FirstOrDefault(x => x.Name == methodModelInfo.Name && GetSignature(x) == methodModelInfo.Signature);
                    if (method == null)
                    {
                        WriteLine($"Не найден метод '{methodModelInfo.Signature}' в типе '{type.AssemblyQualifiedName}' {new StackTrace()}");
                        continue;
                    }

                    WriteLine($"Добавление хука для метода '{methodModelInfo.Signature}' в типе '{type.AssemblyQualifiedName}'");
                    adapter.AddHook(method);
                }
            }

            try
            {
                adapter.Install(Log);
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
            }
        }

        public static DomainModelInfo GetDomainInfo()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            DomainModelInfo result = new DomainModelInfo()
            {
                Assemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(x => !x.FullName.StartsWith("NetHook.Core")
                && !x.FullName.StartsWith("EasyHook")
                && !x.FullName.StartsWith("EasyLoad")
                && !x.FullName.StartsWith("System")
                && !x.FullName.StartsWith("Microsoft")
                && !x.FullName.StartsWith("Aspose")
                && !x.FullName.StartsWith("mscorlib"))
                .Select(x => GetAssembleInfo(x))
                .Where(x => x != null && string.IsNullOrEmpty(x.ErrorText))
                .ToArray(),
                CurrentID = AppDomain.CurrentDomain.Id,
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

                if (assembly.Location == Process.GetCurrentProcess().MainModule.FileName)
                {
                    Console.WriteLine(assembly);
                }

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
                URL = frames.FirstOrDefault(x => x?.URL != null)?.URL,
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
