using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetHook.Cores.Inject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetHook.MSTest
{
    [TestClass]
    public class UnitTestInjectDll
    {
        [TestMethod]
        public void TestInjectDLL()
        {
            //string path = typeof(NetHook.TestInject.MainProgram).Assembly.Location;

            //Process process = Process.Start(path);

            //try
            //{
            //    using (RemoteInjector remoteInjector = new RemoteInjector())
            //    {
            //        remoteInjector.OpenProcess(process);

            //        //remoteInjector.Inject();
            //    }
            //}
            //finally
            //{
            //    process.Kill();
            //}
        }
    }
}
