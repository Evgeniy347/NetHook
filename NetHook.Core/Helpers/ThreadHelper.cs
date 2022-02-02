using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NetHook.Cores.Helpers
{
    public static class ThreadHelper
    {
        public static Thread RunWhileLogic(Func<bool> func, string threadName)
        {
           var stack = Environment.StackTrace;

            Thread thread = new Thread(() =>
            {
                bool result = true;
                while (result)
                {
                    try
                    {
                        result = func.Invoke();
                    }
                    catch (ThreadAbortException ex)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Console.WriteLine(stack);
                    }
                }
            });

            thread.Name = threadName;
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            return thread;
        }

        public static Thread RunLogic(Action action, string threadName)
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });

            thread.Name = threadName;
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            return thread;
        }
    }
}
