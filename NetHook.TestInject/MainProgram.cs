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
                    string indexStr = o.ToString();
                    int j = 0;
                    string value = null;

                    while (true)
                    {
                        j++;
                        string jStr = j.ToString();
                        switch (indexStr)
                        {
                            case "0":
                                value = TestMethodReqursive(jStr);
                                break;
                            case "1":
                                value = TestMethod_1(jStr, jStr, jStr, jStr, jStr, jStr, jStr);
                                break;
                            default:
                                value = TestMethod(jStr);
                                break;
                        }
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

        public string TestMethod(string value)
        {
            var t = this.GetHashCode();
            return value + " " + t + " " + Thread.CurrentThread.ManagedThreadId + " " + string.Join(" => ", new StackTrace().GetFrames().Select(x => x.GetMethod().Name).Reverse().ToArray());
        }

        public static string TestMethod_1(string value, string value2, string value3, string value4, string value5, string value6, string value7)
        {
            return value + " " + Thread.CurrentThread.ManagedThreadId + " " + string.Join(" => ", new StackTrace().GetFrames().Select(x => x.GetMethod().Name).Reverse().ToArray());
        }

        static int index = 0;

        public string TestMethodReqursive(string value)
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
