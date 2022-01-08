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
            Console.WriteLine("AppDomain " + AppDomain.CurrentDomain.Id);

            int i = 0;
            while (true)
            {
                i++;
                var value = TestMethod(i.ToString());

                Console.WriteLine(value);
                Thread.Sleep(1000);
            }
        }

        public static string TestMethod(string value)
        {
            return value + " " + string.Join(" => ", new StackTrace().GetFrames().Select(x => x.GetMethod().Name).Reverse().ToArray());
        }
    }
}
