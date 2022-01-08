using EasyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace InjectMainDLL
{
    public class Main : IEntryPoint
    {
        public Main(RemoteHooking.IContext InContext, String InChannelName)
        {

        }

        public void Run(RemoteHooking.IContext InContext, String InChannelName)
        {
            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}
