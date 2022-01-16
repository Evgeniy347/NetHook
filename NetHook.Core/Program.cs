using EasyHook;
using NetHook.Cores.Extensions;
using NetHook.Cores.Inject;
using RGiesecke.DllExport;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace NetHook.Core
{
    public class Program
    {
        public int Instance;

        public static string Method(int i)
        {
            return Method2("static value " + i.ToString());
        }

        public string MethodF(int i)
        {
            return Method2(" instance " + this.Instance + " value " + i.ToString());
        }

        public static object Method_Hook(object i)
        {
            object result = null;
            try
            {
                result = Method_Hook(i);
                Console.WriteLine();
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {

            }

            return result;
        }

        public static string Method2(string i)
        {
            Console.WriteLine(i + " " + new StackTrace().GetFrames().Select(x => x.GetMethod().Name).Reverse().JoinString(" => "));
            Thread.Sleep(200);
            return i;
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("start");
            try
            {
                //Test1();
                //Test1_2();
                //Test2();
                Test3();
                //Test4();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("end");
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
        }

        private static void Test4()
        {
            Memory memory = new Memory();
            memory.Open(Process.GetCurrentProcess());

            MethodInfo method = typeof(Program).GetMethod("Main2");
            RuntimeHelpers.PrepareMethod(method.MethodHandle);
            Console.WriteLine(method.MethodHandle.GetFunctionPointer().ToHex());
            Console.WriteLine(memory.ReadProcess.MainModule.EntryPointAddress.ToHex());
            Console.WriteLine(memory.ReadProcess.MainModule.BaseAddress.ToHex());

            Console.ReadLine();
            memory.CreateRemoteThread(method.MethodHandle.Value);
        }


        private static void Test2()
        {
            string fileOutPut = @"C:\Users\User\source\repos\ConsoleAppCross\ConsoleAppEasyHook\OutCodeGenerator.cs";


            MethodInfo method = typeof(Program).GetMethod("Method2");
            MethodInfo methodF = typeof(Program).GetMethod("MethodF", BindingFlags.Instance | BindingFlags.Public);

            Method2("test");
            var p = new Program() { Instance = 1 };
            p.MethodF(1);
            var p2 = new Program() { Instance = 2 };
            p2.MethodF(1);
            Console.WriteLine(new string('*', 50));
            using (LocalHookAdapter adapter = LocalHookAdapter.CreateInstance())
            {
                adapter.AddHook(method);
                adapter.AddHook(methodF);

                string cs = adapter.CodeDoomProvider.Save();
                File.WriteAllText(fileOutPut, cs);

                adapter.Install();
                adapter.RegisterHandler(new ConsoleInvokeMethodHandler());
                Console.ReadLine();
                Method2("test");
                p.MethodF(2);
                p2.MethodF(2);
                adapter.UnInstall();
            }
            Console.WriteLine(new string('*', 50));
            Method2("test");
            p.MethodF(3);
            p2.MethodF(3);
        }
        static private void Server()
        {
            Thread thread = new Thread(() =>
            {
                var pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.InOut, 4);

                StreamReader sr = new StreamReader(pipeServer);
                StreamWriter sw = new StreamWriter(pipeServer);

                try
                {
                    pipeServer.WaitForConnection();
                    do
                    {
                        try
                        {
                            string test;
                            //pipeServer.WaitForPipeDrain();
                            test = sr.ReadLine();
                            Console.WriteLine(test);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    } while (true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            });

            thread.IsBackground = true;
            thread.Start();
        }

        private static IpcServerChannel _ipcLogChannel;

        private static void Test3()
        {
            string path = @"..\..\NetHook.TestInject\bin\Debug\NetHook.TestInject.exe";

            Process process = Process.Start(path);

            try
            {
                string dllPath = typeof(RemoteInjector).Assembly.Location;

                //Server();

                string outChannelName = $"ipc://ServerChannel_{Process.GetCurrentProcess().Id}/ServerMethods";
                _ipcLogChannel = new IpcServerChannel($"ServerChannel_{Process.GetCurrentProcess().Id}");
                ChannelServices.RegisterChannel(_ipcLogChannel, true);
                RemotingConfiguration.RegisterWellKnownServiceType(typeof(LoggerProxy), "ServerMethods", WellKnownObjectMode.Singleton);

                //IpcServerChannel ipcLogChannel = RemoteHooking.IpcCreateServer<LoggerInterface>(ref outChannelName, WellKnownObjectMode.Singleton, WellKnownSidType.WorldSid);

                //_ipcLogChannel = ipcLogChannel;

                Thread.Sleep(1000);

                EasyHook.RemoteHooking.Inject(process.Id, InjectionOptions.NoService | InjectionOptions.DoNotRequireStrongName, dllPath, dllPath, outChannelName);

                Thread.Sleep(1000);

                //ipcLogChannel.StartListening(null);


                using (RemoteInjector remoteInjector = new RemoteInjector())
                {
                    //remoteInjector.OpenProcess(process);
                    //remoteInjector.Inject();
                }

                Thread.Sleep(5000);
            }
            finally
            {
                //process.Kill();
            }
        }
    }
}