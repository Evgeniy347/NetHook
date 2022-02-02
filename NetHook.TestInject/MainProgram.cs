using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace NetHook.TestInject
{
    public class MainProgram
    {
        public static void Main(string[] args)
        {
            for (int i = 0; i < 4; i++)
            {
                Thread thread = new Thread((o) =>
                {
                    int j = 0;
                    while (true)
                    {
                        j++;
                        string value = o?.ToString() == "0" ? TestMethodReqursive(j.ToString()) : TestMethod(j.ToString());

                        Console.WriteLine(value);

                        Thread.Sleep(i * 100 + 1000);
                    }
                });

                thread.IsBackground = true;
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start(i);
            }

            Console.ReadLine();
        }

        public static string TestMethod(string value)
        {
            return value + " " + Thread.CurrentThread.ManagedThreadId + " " + string.Join(" => ", new StackTrace().GetFrames().Select(x => x.GetMethod().Name).Reverse().ToArray());
        }

        static int index = 0;

        public static string TestMethodReqursive(string value)
        {

            if (index == 0)
                index = 5;
            else
            {
                index--;
                return TestMethodReqursive(value);
            }

            return value + " " + AppDomain.CurrentDomain.Id + " " + Thread.CurrentThread.ManagedThreadId + " " + string.Join(" => ", new StackTrace().GetFrames().Select(x => x.GetMethod().Name).Reverse().ToArray());
        }
    }
}
