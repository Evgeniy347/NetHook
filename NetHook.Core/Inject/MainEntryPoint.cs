using EasyHook;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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

        public void Run(RemoteHooking.IContext context, string inChannelName)
        {
            try
            {
                using (DomainEntryPoint.SetContextClient(inChannelName))
                {
                    //LoggerServer client = new LoggerServer(); 
                    try
                    {
                        AppDomain[] domains = EnumAppDomains().ToArray();

                        //client.AllDomain(domains.Select(x => x.Id).ToArray());

                        foreach (var domain in domains)
                        {
                            try
                            {
                                if (domain.Id == AppDomain.CurrentDomain.Id)
                                    continue;

                                var obj = domain.CreateInstanceFromAndUnwrap(typeof(DomainEntryPoint).Assembly.Location, typeof(DomainEntryPoint).FullName);
                                var domainEntryPoint = (IDomainEntryPoint)obj;

                                domainEntryPoint.InjectDomain(inChannelName);
                            }
                            catch (Exception ex)
                            {

                                //client.WriteDomainError(domain.Id, ex.Message, ex.ToString());
                                Console.WriteLine(ex);
                            }
                        }

                        //client.InjectAllDomain(domains.Select(x => x.Id).ToArray());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        //client.WriteInjectError(AppDomain.CurrentDomain.Id, ex.Message, ex.ToString());
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
