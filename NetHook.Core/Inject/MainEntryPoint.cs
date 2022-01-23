using EasyHook;
using NetHook.Cores.Extensions;
using NetHook.Cores.Socket;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace NetHook.Cores.Inject
{
    public class MainEntryPoint : IEntryPoint
    {
        public MainEntryPoint(RemoteHooking.IContext InContext, string inChannelName)
        { }

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

        public void Run(RemoteHooking.IContext context, string address)
        {
            try
            {
                using (LoggerClient client = new LoggerClient())
                {
                    client.OpenChanel(address);
                    HashSet<int> injectDomainsIDs = new HashSet<int>();
                    HashSet<int> errorDomainsIDs = new HashSet<int>();

                    bool isFirst = true;
                    while (client.Socket.IsSocketConnected())
                    {
                        try
                        {
                            AppDomain[] domains = EnumAppDomains()
                                .Where(x => x.Id != AppDomain.CurrentDomain.Id &&
                                !injectDomainsIDs.Union(errorDomainsIDs).Contains(x.Id))
                                .ToArray();

                            string message = $"CountDomain:{domains.Length} newDomains:{domains.Select(x => x.Id).JoinString()} injectDomainsIDs:({injectDomainsIDs.JoinString()}) errorDomainsIDs:({errorDomainsIDs.JoinString()})";

                            if (isFirst)
                            {
                                var messageSocket = client.AcceptMessage();
                                isFirst = false;
                            }

                            if (domains.Length > 0)
                            {
                                client.SetInjectConnection(message);

                                foreach (var domain in domains)
                                {
                                    try
                                    {
                                        var obj = domain.CreateInstanceFromAndUnwrap(typeof(DomainEntryPoint).Assembly.Location, typeof(DomainEntryPoint).FullName);
                                        var domainEntryPoint = (IDomainEntryPoint)obj;

                                        domainEntryPoint.InjectDomain(address);
                                        injectDomainsIDs.Add(domain.Id);
                                    }
                                    catch (Exception ex)
                                    {
                                        errorDomainsIDs.Add(domain.Id);
                                        client.SetInjectConnection(ex.ToString());
                                        Console.WriteLine(ex);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Send(ex.ToString());
                            Console.WriteLine(ex);
                        }

                        Thread.Sleep(500);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

    }
}
